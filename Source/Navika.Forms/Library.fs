namespace Navika.Forms

open FsToolkit.ErrorHandling

module List =
    // Copied from https://github.com/fsprojects/FSharpPlus/blob/35fa2a97e0c3f93c7cf36c172314e7f2d921d438/src/FSharpPlus/Extensions/List.fs
    let drop count source =
        let rec loop i lst =
            match lst, i with
            | [] as x, _
            | x, 0 -> x
            | x, n -> loop (n - 1) (List.tail x)

        if count > 0 then loop count source else source

    let setAt i x lst =
        if List.length lst > i && i >= 0 then
            lst[0 .. i - 1] @ x :: lst[i + 1 ..]
        else
            lst

module Error =
    type Error =
        | RequiredFieldIsEmpty
        | ValidationFailed of string
        | External of string

module Field =

    [<NoComparison; NoEquality>]
    type Field<'Attributes, 'Value, 'Values> = {
        Value: 'Value
        Update: 'Value -> 'Values
        Attributes: 'Attributes
    }

    let mapValues (fn: 'T -> 'R) (field: Field<'Attributes, 'Value, 'T>) : Field<'Attributes, 'Value, 'R> = {
        Value = field.Value
        Update = field.Update >> fn
        Attributes = field.Attributes
    }

module Base =
    type FilledField<'Field> = {
        State: 'Field
        Error: Error.Error option
        IsDisabled: bool
    }

    type FilledForm<'Output, 'Field> = {
        Fields: FilledField<'Field> list
        Result: Result<'Output, (Error.Error * Error.Error list)>
        IsEmpty: bool
    }

    [<NoComparison; NoEquality>]
    type Form<'Values, 'Output, 'Field> = | Form of ('Values -> FilledForm<'Output, 'Field>)

    [<NoComparison; NoEquality>]
    type FieldConfig<'Attributes, 'Input, 'Values, 'Output> = {
        Parser: 'Input -> Result<'Output, string>
        Value: 'Values -> 'Input
        Update: 'Input -> 'Values -> 'Values
        Error: 'Values -> string option
        Attributes: 'Attributes
    }

    type CustomField<'Output, 'Field> = {
        State: 'Field
        Result: Result<'Output, (Error.Error * Error.Error list)>
        IsEmpty: bool
    }

    let succeed(output: 'Output) : Form<'Values, 'Output, 'Field> =
        Form(fun _ -> {
            Fields = []
            Result = Ok output
            IsEmpty = true
        })

    let fill<'Values, 'Output, 'Field>(Form form: Form<'Values, 'Output, 'Field>) : 'Values -> FilledForm<'Output, 'Field> = form

    let custom(fillField: 'Values -> CustomField<'Output, 'Field>) : Form<'Values, 'Output, 'Field> =
        Form(fun values ->
            let filled = fillField values

            {
                Fields = [
                    {
                        State = filled.State
                        Error =
                            if filled.IsEmpty then
                                Some Error.RequiredFieldIsEmpty
                            else
                                match filled.Result with
                                | Ok _ -> None
                                | Error(firstError, _) -> Some firstError
                        IsDisabled = false
                    }
                ]
                Result = filled.Result
                IsEmpty = filled.IsEmpty
            })

    let meta(fn: 'Values -> Form<'Values, 'Output, 'Field>) : Form<'Values, 'Output, 'Field> =
        Form(fun values -> fill (fn values) values)

    let mapValues (fn: 'A -> 'B) (form: Form<'B, 'Output, 'Field>) : Form<'A, 'Output, 'Field> = Form(fn >> fill form)

    let mapField (fn: 'A -> 'B) (form: Form<'Values, 'Output, 'A>) : Form<'Values, 'Output, 'B> =
        Form(fun values ->
            let filled = fill form values

            {
                Fields =
                    filled.Fields
                    |> List.map(fun filledField -> {
                        State = fn filledField.State
                        Error = filledField.Error
                        IsDisabled = filledField.IsDisabled
                    })
                Result = filled.Result
                IsEmpty = filled.IsEmpty
            })

    let append (newForm: Form<'Values, 'A, 'Field>) (currentForm: Form<'Values, 'A -> 'B, 'Field>) : Form<'Values, 'B, 'Field> =
        Form(fun values ->
            let filledNew = fill newForm values
            let filledCurrent = fill currentForm values
            let fields = filledCurrent.Fields @ filledNew.Fields
            let isEmpty = filledCurrent.IsEmpty && filledNew.IsEmpty

            match filledCurrent.Result with
            | Ok fn -> {
                Fields = fields
                Result = Result.map fn filledNew.Result
                IsEmpty = isEmpty
              }

            | Error(firstError, otherErrors) ->
                match filledNew.Result with
                | Ok _ -> {
                    Fields = fields
                    Result = Error(firstError, otherErrors)
                    IsEmpty = isEmpty
                  }

                | Error(newFirstError, newOtherErrors) -> {
                    Fields = fields
                    Result = Error(firstError, otherErrors @ (newFirstError :: newOtherErrors))
                    IsEmpty = isEmpty
                  })

    let andThen (child: 'A -> Form<'Values, 'B, 'Field>) (parent: Form<'Values, 'A, 'Field>) : Form<'Values, 'B, 'Field> =
        Form(fun values ->
            let filled = fill parent values

            match filled.Result with
            | Ok output ->
                let childFilled = fill (child output) values

                {
                    Fields = filled.Fields @ childFilled.Fields
                    Result = childFilled.Result
                    IsEmpty = filled.IsEmpty && childFilled.IsEmpty
                }

            | Error errors -> {
                Fields = filled.Fields
                Result = Error errors
                IsEmpty = filled.IsEmpty
              })

    let disable(form: Form<'Values, 'Output, 'Field>) : Form<'Values, 'Output, 'Field> =
        Form(fun values ->
            let filled = fill form values

            {
                Fields =
                    filled.Fields
                    |> List.map(fun filledField -> { filledField with IsDisabled = true })
                Result = filled.Result
                IsEmpty = filled.IsEmpty
            })

    let map (fn: 'A -> 'B) (form: Form<'Values, 'A, 'Field>) : Form<'Values, 'B, 'Field> =
        Form(fun values ->
            let filled = fill form values

            {
                Fields = filled.Fields
                Result = Result.map fn filled.Result
                IsEmpty = filled.IsEmpty
            })

    let field
        (isEmpty: 'Input -> bool)
        (build: Field.Field<'Attributes, 'Input, 'Values> -> 'Field)
        (config: FieldConfig<'Attributes, 'Input, 'Values, 'Output>)
        : Form<'Values, 'Output, 'Field> =

        let requiredParser value =
            if isEmpty value then
                Error(Error.RequiredFieldIsEmpty, [])
            else
                config.Parser value
                |> Result.mapError(fun error -> (Error.ValidationFailed error, []))

        let parse values =
            result {
                let! output = requiredParser(config.Value values)

                return!
                    values
                    |> config.Error
                    |> Option.map(fun error -> Error(Error.External error, []))
                    |> Option.defaultValue(Ok output)
            }

        let field_ values =
            let value = config.Value values
            let update newValue = config.Update newValue values

            build {
                Value = value
                Update = update
                Attributes = config.Attributes
            }

        Form(fun values ->
            let result = parse values

            let (error, isEmpty_) =
                match result with
                | Ok _ -> (None, false)
                | Error(firstError, _) -> Some firstError, firstError = Error.RequiredFieldIsEmpty

            {
                Fields = [
                    {
                        State = field_ values
                        Error = error
                        IsDisabled = false
                    }
                ]
                Result = result
                IsEmpty = isEmpty_
            })

    let optional(form: Form<'Values, 'Output, 'Field>) : Form<'Values, 'Output option, 'Field> =
        Form(fun values ->
            let filled = fill form values

            match filled.Result with
            | Ok value -> {
                Fields = filled.Fields
                Result = Ok(Some value)
                IsEmpty = filled.IsEmpty
              }

            | Error(firstError, otherErrors) ->
                if filled.IsEmpty then
                    {
                        Fields = filled.Fields |> List.map(fun field -> { field with Error = None })
                        Result = Ok None
                        IsEmpty = filled.IsEmpty
                    }
                else
                    {
                        Fields = filled.Fields
                        Result = Error(firstError, otherErrors)
                        IsEmpty = false
                    })

module TextField =

    type Attributes<'Attributes> = {
        Label: string
        Placeholder: string
        HtmlAttributes: 'Attributes list
    }

    type TextField<'Values, 'Attributes> = Field.Field<Attributes<'Attributes>, string, 'Values>

    let form<'Values, 'Attributes, 'Field, 'Output>
        : ((TextField<'Values, 'Attributes> -> 'Field)
              -> Base.FieldConfig<Attributes<'Attributes>, string, 'Values, 'Output>
              -> Base.Form<'Values, 'Output, 'Field>) =
        Base.field System.String.IsNullOrEmpty

module RadioField =

    type Attributes = {
        Label: string
        Options: (string * string) list
    }

    type RadioField<'Values> = Field.Field<Attributes, string, 'Values>

    let form<'Values, 'Field, 'Output>
        : ((RadioField<'Values> -> 'Field) -> Base.FieldConfig<Attributes, string, 'Values, 'Output> -> Base.Form<'Values, 'Output, 'Field>) =
        Base.field System.String.IsNullOrEmpty

module CheckboxField =

    type Attributes = { Text: string }

    type CheckboxField<'Values> = Field.Field<Attributes, bool, 'Values>

    let form<'Values, 'Field, 'Output>
        : ((CheckboxField<'Values> -> 'Field) -> Base.FieldConfig<Attributes, bool, 'Values, 'Output> -> Base.Form<'Values, 'Output, 'Field>) =
        Base.field(fun _ -> false)

module SelectField =

    type Attributes = {
        Label: string
        Placeholder: string
        Options: (string * string) list
    }

    type SelectField<'Values> = Field.Field<Attributes, string, 'Values>

    let form<'Values, 'Field, 'Output>
        : ((SelectField<'Values> -> 'Field) -> Base.FieldConfig<Attributes, string, 'Values, 'Output> -> Base.Form<'Values, 'Output, 'Field>) =
        Base.field System.String.IsNullOrEmpty

module FileField =
    type FileType =
        | Any
        | Specific of string list

    type FileIconClassName =
        | Default
        | Custom of string

    type Attributes = {
        Label: string
        InputLabel: string
        Accept: FileType
        FileIconClassName: FileIconClassName
        Multiple: bool
    }

    type FileField<'Values> = Field.Field<Attributes, System.IO.FileInfo array, 'Values>

    let form<'Values, 'Field, 'Output>
        : ((FileField<'Values> -> 'Field)
              -> Base.FieldConfig<Attributes, System.IO.FileInfo array, 'Values, 'Output>
              -> Base.Form<'Values, 'Output, 'Field>) =
        Base.field Array.isEmpty

module FormList =
    [<NoComparison; NoEquality>]
    type Form<'Values, 'Field> = {
        Fields: Base.FilledField<'Field> list
        Delete: unit -> 'Values
    }

    type Attributes = {
        Label: string
        Add: string option
        Delete: string option
    }

    [<NoComparison; NoEquality>]
    type FormList<'Values, 'Field> = {
        Forms: Form<'Values, 'Field> list
        Add: unit -> 'Values
        Attributes: Attributes
    }

    [<NoComparison; NoEquality>]
    type Config<'Values, 'ElementValues> = {
        Value: 'Values -> 'ElementValues list
        Update: 'ElementValues list -> 'Values -> 'Values
        Default: 'ElementValues
        Attributes: Attributes
    }

    [<NoComparison; NoEquality>]
    type ElementState<'Values, 'ElementValues> = {
        Index: int
        Update: 'ElementValues -> 'Values -> 'Values
        Values: 'Values
        ElementValues: 'ElementValues
    }

    let form<'Values, 'Field, 'ElementValues, 'Output>
        (tagger: FormList<'Values, 'Field> -> 'Field)
        (formConfig: Config<'Values, 'ElementValues>)
        (buildElement: ElementState<'Values, 'ElementValues> -> Base.FilledForm<'Output, 'Field>)
        : Base.Form<'Values, 'Output list, 'Field> =

        Base.custom(fun values ->
            let listOfElementValues = formConfig.Value values

            let elementForIndex index elementValues =
                buildElement {
                    Update =
                        fun newElementValues values ->
                            let newList = List.setAt index newElementValues listOfElementValues

                            formConfig.Update newList values
                    Index = index
                    Values = values
                    ElementValues = elementValues
                }

            let filledElements = List.mapi elementForIndex listOfElementValues

            let toForm (index: int) (form: Base.FilledForm<'Output, 'Field>) = {
                Fields = form.Fields
                Delete =
                    fun () ->
                        let previousForms = List.take index listOfElementValues

                        let nextForms = List.drop (index + 1) listOfElementValues

                        formConfig.Update (previousForms @ nextForms) values
            }

            let gatherResults
                (next: Base.FilledForm<'Output, 'Field>)
                (current: Result<'Output list, Error.Error * Error.Error list>)
                : Result<'Output list, Error.Error * Error.Error list> =

                match next.Result with
                | Ok output -> Result.map (fun x -> output :: x) current

                | Error(head, errors) ->
                    match current with
                    | Ok _ -> Error(head, errors)

                    | Error(currentHead, currentErrors) -> Error(head, errors @ (currentHead :: currentErrors))

            let result = List.foldBack gatherResults filledElements (Ok [])

            let isEmpty =
                List.fold (fun state (element: Base.FilledForm<'Output, 'Field>) -> element.IsEmpty && state) false filledElements

            {
                State =
                    tagger {
                        Forms = List.mapi toForm filledElements
                        Add = fun _ -> formConfig.Update (listOfElementValues @ [ formConfig.Default ]) values
                        Attributes = formConfig.Attributes
                    }
                Result = result
                IsEmpty = isEmpty
            })

[<RequireQualifiedAccess>]
module Form =
    type TextField<'Values, 'Attributes> = TextField.TextField<'Values, 'Attributes>
    type RadioField<'Values> = RadioField.RadioField<'Values>
    type CheckboxField<'Values> = CheckboxField.CheckboxField<'Values>
    type SelectField<'Values> = SelectField.SelectField<'Values>
    type FileField<'Values> = FileField.FileField<'Values>

    type TextType =
        // | TextColor
        // | TextDate
        // | TextDateTimeLocal
        // | TextEmail
        // Not supported yet because there are not cross browser support Firefox doesn't support it for example
        // and there is no polyfill for it
        // | TextMonth
        // | TextNumber
        // | TextPassword
        // TODO:
        // | TextRange
        // | TextSearch
        // | TextTel
        // Match for input="text"
        | TextRaw
    // | TextTime
    // Not supported yet because there are not cross browser support Firefox doesn't support it for example
    // and there is no polyfill for it
    // | TextWeek
    // | TextArea

    [<RequireQualifiedAccess; NoComparison; NoEquality>]
    type Field<'Values, 'Attributes> =
        | Text of TextType * TextField<'Values, 'Attributes>
        | Radio of RadioField<'Values>
        | Checkbox of CheckboxField<'Values>
        | Select of SelectField<'Values>
        | File of FileField<'Values>
        | Group of FilledField<'Values, 'Attributes> list
        | Section of title: string * FilledField<'Values, 'Attributes> list
        | List of FormList.FormList<'Values, Field<'Values, 'Attributes>>

    and FilledField<'Values, 'Attributes> = Base.FilledField<Field<'Values, 'Attributes>>
    type Form<'Values, 'Output, 'Attributes> = Base.Form<'Values, 'Output, Field<'Values, 'Attributes>>

    // Redefined some function from the Base module so the user can access them transparently and they are also specifically type for the Fable.Form.Simple abstraction
    let succeed(output: 'Output) : Form<'Values, 'Output, 'Attributes> = Base.succeed output

    let append
        (newForm: Form<'Values, 'A, 'Attributes>)
        (currentForm: Form<'Values, 'A -> 'B, 'Attributes>)
        : Form<'Values, 'B, 'Attributes> =
        Base.append newForm currentForm

    let disable(form: Form<'Values, 'A, 'Attributes>) : Form<'Values, 'A, 'Attributes> = Base.disable form

    let andThen (child: 'A -> Form<'Values, 'B, 'Attributes>) (parent: Form<'Values, 'A, 'Attributes>) : Form<'Values, 'B, 'Attributes> =
        Base.andThen child parent

    let optional(form: Form<'Values, 'A, 'Attributes>) : Form<'Values, 'A option, 'Attributes> = Base.optional form

    let textField
        (config: Base.FieldConfig<TextField.Attributes<'Attributes>, string, 'Values, 'Output>)
        : Form<'Values, 'Output, 'Attributes> =
        TextField.form (fun x -> Field.Text(TextRaw, x)) config

    // let passwordField
    //     (config: Base.FieldConfig<TextField.Attributes<'Attributes>, string, 'Values, 'Output>)
    //     : Form<'Values, 'Output, 'Attributes> =
    //     TextField.form (fun x -> Field.Text(TextPassword, x)) config

    // let colorField
    //     (config: Base.FieldConfig<TextField.Attributes<'Attributes>, string, 'Values, 'Output>)
    //     : Form<'Values, 'Output, 'Attributes> =
    //     TextField.form (fun x -> Field.Text(TextColor, x)) config

    // let dateField
    //     (config: Base.FieldConfig<TextField.Attributes<'Attributes>, string, 'Values, 'Output>)
    //     : Form<'Values, 'Output, 'Attributes> =
    //     TextField.form (fun x -> Field.Text(TextDate, x)) config

    // let dateTimeLocalField
    //     (config: Base.FieldConfig<TextField.Attributes<'Attributes>, string, 'Values, 'Output>)
    //     : Form<'Values, 'Output, 'Attributes> =
    //     TextField.form (fun x -> Field.Text(TextDateTimeLocal, x)) config

    // let numberField
    //     (config: Base.FieldConfig<TextField.Attributes<'Attributes>, string, 'Values, 'Output>)
    //     : Form<'Values, 'Output, 'Attributes> =
    //     TextField.form (fun x -> Field.Text(TextNumber, x)) config

    // let searchField
    //     (config: Base.FieldConfig<TextField.Attributes<'Attributes>, string, 'Values, 'Output>)
    //     : Form<'Values, 'Output, 'Attributes> =
    //     TextField.form (fun x -> Field.Text(TextSearch, x)) config

    // let telField
    //     (config: Base.FieldConfig<TextField.Attributes<'Attributes>, string, 'Values, 'Output>)
    //     : Form<'Values, 'Output, 'Attributes> =
    //     TextField.form (fun x -> Field.Text(TextTel, x)) config

    // let timeField
    //     (config: Base.FieldConfig<TextField.Attributes<'Attributes>, string, 'Values, 'Output>)
    //     : Form<'Values, 'Output, 'Attributes> =
    //     TextField.form (fun x -> Field.Text(TextTime, x)) config

    // let emailField
    //     (config: Base.FieldConfig<TextField.Attributes<'Attributes>, string, 'Values, 'Output>)
    //     : Form<'Values, 'Output, 'Attributes> =
    //     TextField.form (fun x -> Field.Text(TextEmail, x)) config

    // let textareaField
    //     (config: Base.FieldConfig<TextField.Attributes<'Attributes>, string, 'Values, 'Output>)
    //     : Form<'Values, 'Output, 'Attributes> =
    //     TextField.form (fun x -> Field.Text(TextArea, x)) config

    let checkboxField(config: Base.FieldConfig<CheckboxField.Attributes, bool, 'Values, 'Output>) : Form<'Values, 'Output, 'Attributes> =
        CheckboxField.form Field.Checkbox config

    let radioField(config: Base.FieldConfig<RadioField.Attributes, string, 'Values, 'Output>) : Form<'Values, 'Output, 'Attributes> =
        RadioField.form Field.Radio config

    let selectField(config: Base.FieldConfig<SelectField.Attributes, string, 'Values, 'Output>) : Form<'Values, 'Output, 'Attributes> =
        SelectField.form Field.Select config

    let fileField
        (config: Base.FieldConfig<FileField.Attributes, System.IO.FileInfo array, 'Values, 'Output>)
        : Form<'Values, 'Output, 'Attributes> =
        FileField.form Field.File config

    let group(form: Form<'Values, 'Output, 'Attributes>) : Form<'Values, 'Output, 'Attributes> =
        Base.custom(fun values ->
            let res = Base.fill form values

            {
                State = Field.Group res.Fields
                Result = res.Result
                IsEmpty = res.IsEmpty
            })

    let section (title: string) (form: Form<'Values, 'Output, 'Attributes>) : Form<'Values, 'Output, 'Attributes> =
        Base.custom(fun values ->
            let res = Base.fill form values

            {
                State = Field.Section(title, res.Fields)
                Result = res.Result
                IsEmpty = res.IsEmpty
            })

    let fill (form: Form<'Values, 'Output, 'Attributes>) (values: 'Values) =
        // Work around type system complaining about the 'Field behind forced to a type
        // Revisit? Good enough?
        let filledForm = Base.fill form values

        {|
            Fields = filledForm.Fields
            Result = filledForm.Result
            IsEmpty = filledForm.IsEmpty
        |}

    let rec private mapFieldValues (update: 'A -> 'B -> 'B) (values: 'B) (field: Field<'A, 'Attributes>) : Field<'B, 'Attributes> =
        let newUpdate oldValues = update oldValues values

        match field with
        | Field.Text(textType, textField) -> Field.Text(textType, Field.mapValues newUpdate textField)
        | Field.Radio radioField -> Field.Radio(Field.mapValues newUpdate radioField)
        | Field.Checkbox checkboxField -> Field.Checkbox(Field.mapValues newUpdate checkboxField)
        | Field.Select selectField -> Field.Select(Field.mapValues newUpdate selectField)
        | Field.File fileField -> Field.File(Field.mapValues newUpdate fileField)
        | Field.Group fields ->
            fields
            |> List.map(fun filledField ->
                {
                    State = mapFieldValues update values filledField.State
                    Error = filledField.Error
                    IsDisabled = filledField.IsDisabled
                }
                : FilledField<'B, 'Attributes>)
            |> Field.Group
        | Field.Section(title, fields) ->
            let newFields =
                fields
                |> List.map(fun filledField ->
                    {
                        State = mapFieldValues update values filledField.State
                        Error = filledField.Error
                        IsDisabled = filledField.IsDisabled
                    }
                    : FilledField<'B, 'Attributes>)

            Field.Section(title, newFields)
        | Field.List formList ->
            Field.List {
                Forms =
                    List.map
                        (fun (form: FormList.Form<'A, Field<'A, 'Attributes>>) -> {
                            Fields =
                                List.map
                                    (fun (filledField: Base.FilledField<Field<'A, 'Attributes>>) -> {
                                        State = mapFieldValues update values filledField.State
                                        Error = filledField.Error
                                        IsDisabled = filledField.IsDisabled
                                    })
                                    form.Fields
                            Delete = fun _ -> update (form.Delete()) values
                        })
                        formList.Forms
                Add = fun _ -> update (formList.Add()) values
                Attributes = formList.Attributes
            }

    let list
        (config: FormList.Config<'Values, 'ElementValues>)
        (elementForIndex: int -> Form<'ElementValues, 'Output, 'Attributes>)
        : Form<'Values, 'Output list, 'Attributes> =

        let fillElement
            (elementState: FormList.ElementState<'Values, 'ElementValues>)
            : Base.FilledForm<'Output, Field<'Values, 'Attributes>> =
            let filledElement = fill (elementForIndex elementState.Index) elementState.ElementValues

            {
                Fields =
                    filledElement.Fields
                    |> List.map(fun filledField -> {
                        State = mapFieldValues elementState.Update elementState.Values filledField.State
                        Error = filledField.Error
                        IsDisabled = filledField.IsDisabled
                    })
                Result = filledElement.Result
                IsEmpty = filledElement.IsEmpty
            }

        let tagger formList = Field.List formList
        FormList.form tagger config fillElement

    let meta(fn: 'Values -> Form<'Values, 'Output, 'Attributes>) : Form<'Values, 'Output, 'Attributes> = Base.meta fn

    [<NoComparison; NoEquality>]
    type MapValuesConfig<'A, 'B> = {
        Value: 'A -> 'B
        Update: 'B -> 'A -> 'A
    }

    let mapValues
        ({
             Value = value
             Update = update
         }: MapValuesConfig<'A, 'B>)
        (form: Form<'B, 'Output, 'Attributes>)
        : Form<'A, 'Output, 'Attributes> =
        Base.meta(fun values -> form |> Base.mapValues value |> Base.mapField(mapFieldValues update values))

    module View =
        open Elmish
        open Avalonia.FuncUI.Types

        type State =
            | Idle
            | Loading
            | Error of string
            | Success of string

        type ErrorTracking =
            | ErrorTracking of
                {|
                    ShowAllErrors: bool
                    ShowFieldError: Set<string>
                |}

        type Model<'Values> = {
            Values: 'Values
            State: State
            ErrorTracking: ErrorTracking
        }

        type Validation =
            | ValidateOnBlur
            | ValidateOnSubmit

        [<RequireQualifiedAccess; NoComparison; NoEquality>]
        type Action<'Msg> =
            | SubmitOnly of string
            | Custom of (State -> Elmish.Dispatch<'Msg> -> IView)

        [<NoComparison; NoEquality>]
        type ViewConfig<'Values, 'Msg> = {
            Dispatch: Dispatch<'Msg>
            OnChange: Model<'Values> -> 'Msg
            Action: Action<'Msg>
            Validation: Validation
        }

        [<NoComparison; NoEquality>]
        type FormConfig<'Msg> = {
            Dispatch: Dispatch<'Msg>
            OnSubmit: 'Msg option
            State: State
            Action: Action<'Msg>
            Fields: IView list
        }

        [<NoComparison; NoEquality>]
        type TextFieldConfig<'Msg, 'Attributes> = {
            Dispatch: Dispatch<'Msg>
            OnChange: string -> 'Msg
            OnBlur: 'Msg option
            Disabled: bool
            Value: string
            Error: Error.Error option
            ShowError: bool
            Attributes: TextField.Attributes<'Attributes>
        }

        [<NoComparison; NoEquality>]
        type CheckboxFieldConfig<'Msg> = {
            Dispatch: Dispatch<'Msg>
            OnChange: bool -> 'Msg
            OnBlur: 'Msg option
            Disabled: bool
            Value: bool
            Error: Error.Error option
            ShowError: bool
            Attributes: CheckboxField.Attributes
        }

        [<NoComparison; NoEquality>]
        type RadioFieldConfig<'Msg> = {
            Dispatch: Dispatch<'Msg>
            OnChange: string -> 'Msg
            OnBlur: 'Msg option
            Disabled: bool
            Value: string
            Error: Error.Error option
            ShowError: bool
            Attributes: RadioField.Attributes
        }

        [<NoComparison; NoEquality>]
        type SelectFieldConfig<'Msg> = {
            Dispatch: Dispatch<'Msg>
            OnChange: string -> 'Msg
            OnBlur: 'Msg option
            Disabled: bool
            Value: string
            Error: Error.Error option
            ShowError: bool
            Attributes: SelectField.Attributes
        }

        [<NoComparison; NoEquality>]
        type FileFieldConfig<'Msg> = {
            Dispatch: Dispatch<'Msg>
            OnChange: System.IO.FileInfo array -> 'Msg
            Disabled: bool
            Value: System.IO.FileInfo array
            Error: Error.Error option
            ShowError: bool
            Attributes: FileField.Attributes
        }

        [<NoComparison; NoEquality>]
        type FormListConfig<'Msg> = {
            Dispatch: Dispatch<'Msg>
            Forms: IView list
            Label: string
            Add:
                {|
                    Action: unit -> 'Msg
                    Label: string
                |} option
            Disabled: bool
        }

        [<NoComparison; NoEquality>]
        type FormListItemConfig<'Msg> = {
            Dispatch: Dispatch<'Msg>
            Fields: IView list
            Delete:
                {|
                    Action: unit -> 'Msg
                    Label: string
                |} option
            Disabled: bool
        }

        let idle(values: 'Values) = {
            Values = values
            State = Idle
            ErrorTracking =
                ErrorTracking {|
                    ShowAllErrors = false
                    ShowFieldError = Set.empty
                |}
        }

        let setLoading(formModel: Model<'Values>) = { formModel with State = Loading }

        [<NoComparison; NoEquality>]
        type CustomConfig<'Msg, 'Attributes> = {
            Form: FormConfig<'Msg> -> IView
            TextField: TextFieldConfig<'Msg, 'Attributes> -> IView
            // PasswordField: TextFieldConfig<'Msg, 'Attributes> -> IView
            // EmailField: TextFieldConfig<'Msg, 'Attributes> -> IView
            // ColorField: TextFieldConfig<'Msg, 'Attributes> -> IView
            // DateField: TextFieldConfig<'Msg, 'Attributes> -> IView
            // DateTimeLocalField: TextFieldConfig<'Msg, 'Attributes> -> IView
            // NumberField: TextFieldConfig<'Msg, 'Attributes> -> IView
            // SearchField: TextFieldConfig<'Msg, 'Attributes> -> IView
            // TelField: TextFieldConfig<'Msg, 'Attributes> -> IView
            // TimeField: TextFieldConfig<'Msg, 'Attributes> -> IView
            TextAreaField: TextFieldConfig<'Msg, 'Attributes> -> IView
            CheckboxField: CheckboxFieldConfig<'Msg> -> IView
        // RadioField: RadioFieldConfig<'Msg> -> IView
        // SelectField: SelectFieldConfig<'Msg> -> IView
        // FileField: FileFieldConfig<'Msg> -> IView
        // Group: IView list -> IView
        // Section: string -> IView list -> IView
        // FormList: FormListConfig<'Msg> -> IView
        // FormListItem: FormListItemConfig<'Msg> -> IView
        }

        [<NoComparison; NoEquality>]
        type FieldConfig<'Values, 'Msg> = {
            OnChange: 'Values -> 'Msg
            OnBlur: (string -> 'Msg) option
            Disabled: bool
            ShowError: string -> bool
        }

        type InputType = | Text
        // | Password
        // | Email
        // | Color
        // | Date
        // | DateTimeLocal
        // | Number
        // | Search
        // | Tel
        // | Time

        let errorToString(error: Error.Error) =
            match error with
            | Error.RequiredFieldIsEmpty -> "This field is required"
            | Error.ValidationFailed validationError -> validationError
            | Error.External externalError -> externalError

        let ignoreChildError
            (parentError: Error.Error option)
            (field: FilledField<'Values, 'Attributes>)
            : FilledField<'Values, 'Attributes> =

            match parentError with
            | Some _ -> field
            | None -> { field with Error = None }

        let rec renderField
            (dispatch: Dispatch<'Msg>)
            (customConfig: CustomConfig<'Msg, 'Attributes>)
            (fieldConfig: FieldConfig<'Values, 'Msg>)
            (field: FilledField<'Values, 'Attributes>)
            : IView =
            let blur label =
                Option.map (fun onBlurEvent -> onBlurEvent label) fieldConfig.OnBlur

            match field.State with
            | Field.Text(typ, info) ->
                let config: TextFieldConfig<'Msg, 'Attributes> = {
                    Dispatch = dispatch
                    OnChange = info.Update >> fieldConfig.OnChange
                    OnBlur = blur info.Attributes.Label
                    Disabled = field.IsDisabled || fieldConfig.Disabled
                    Value = info.Value
                    Error = field.Error
                    ShowError = fieldConfig.ShowError info.Attributes.Label
                    Attributes = info.Attributes
                }

                match typ with
                | TextRaw -> customConfig.TextField config
            // | TextPassword -> customConfig.PasswordField config
            // | TextArea -> customConfig.TextAreaField config
            // | TextEmail -> customConfig.EmailField config
            // | TextColor -> customConfig.ColorField config
            // | TextDate -> customConfig.DateField config
            // | TextDateTimeLocal -> customConfig.DateTimeLocalField config
            // | TextNumber -> customConfig.NumberField config
            // | TextSearch -> customConfig.SearchField config
            // | TextTel -> customConfig.TelField config
            // | TextTime -> customConfig.TimeField config
            | Field.Checkbox info ->
                let config: CheckboxFieldConfig<'Msg> = {
                    Dispatch = dispatch
                    OnChange = info.Update >> fieldConfig.OnChange
                    OnBlur = blur info.Attributes.Text
                    Disabled = field.IsDisabled || fieldConfig.Disabled
                    Value = info.Value
                    Error = field.Error
                    ShowError = fieldConfig.ShowError info.Attributes.Text
                    Attributes = info.Attributes
                }

                customConfig.CheckboxField config

        // | Field.Radio info ->
        //     let config: RadioFieldConfig<'Msg> = {
        //         Dispatch = dispatch
        //         OnChange = info.Update >> fieldConfig.OnChange
        //         OnBlur = blur info.Attributes.Label
        //         Disabled = field.IsDisabled || fieldConfig.Disabled
        //         Value = info.Value
        //         Error = field.Error
        //         ShowError = fieldConfig.ShowError info.Attributes.Label
        //         Attributes = info.Attributes
        //     }

        //     customConfig.RadioField config

        // // | Field.Select info ->
        // //     let config: SelectFieldConfig<'Msg> = {
        // //         Dispatch = dispatch
        // //         OnChange = info.Update >> fieldConfig.OnChange
        // //         OnBlur = blur info.Attributes.Label
        // //         Disabled = field.IsDisabled || fieldConfig.Disabled
        // //         Value = info.Value
        // //         Error = field.Error
        // //         ShowError = fieldConfig.ShowError info.Attributes.Label
        // //         Attributes = info.Attributes
        // //     }

        //     customConfig.SelectField config

        // | Field.File info ->
        //     let config: FileFieldConfig<'Msg> = {
        //         Dispatch = dispatch
        //         OnChange = info.Update >> fieldConfig.OnChange
        //         Disabled = field.IsDisabled || fieldConfig.Disabled
        //         Value = info.Value
        //         Error = field.Error
        //         ShowError = fieldConfig.ShowError info.Attributes.Label
        //         Attributes = info.Attributes
        //     }

        //     customConfig.FileField config

        // | Field.Group fields ->
        //     fields
        //     |> List.map(fun field ->
        //         (ignoreChildError field.Error
        //          >> renderField dispatch customConfig { fieldConfig with Disabled = field.IsDisabled || fieldConfig.Disabled })
        //             field)
        //     |> customConfig.Group

        // | Field.Section(title, fields) ->
        //     fields
        //     |> List.map(fun field ->
        //         (ignoreChildError field.Error
        //          >> renderField dispatch customConfig { fieldConfig with Disabled = field.IsDisabled || fieldConfig.Disabled })
        //             field)
        //     |> customConfig.Section title

        // | Field.List {
        //                  Forms = forms
        //                  Add = add
        //                  Attributes = attributes
        //              } ->
        //     customConfig.FormList {
        //         Dispatch = dispatch
        //         Forms =
        //             forms
        //             |> List.map
        //                 (fun
        //                     {
        //                         Fields = fields
        //                         Delete = delete
        //                     } ->
        //                     customConfig.FormListItem {
        //                         Dispatch = dispatch
        //                         Fields = List.map (renderField dispatch customConfig fieldConfig) fields
        //                         Delete =
        //                             attributes.Delete
        //                             |> Option.map(fun deleteLabel -> {|
        //                                 Action = delete >> fieldConfig.OnChange
        //                                 Label = deleteLabel
        //                             |})
        //                         Disabled = field.IsDisabled || fieldConfig.Disabled
        //                     })
        //         Label = attributes.Label
        //         Add =
        //             attributes.Add
        //             |> Option.map(fun addLabel -> {|
        //                 Action = add >> fieldConfig.OnChange
        //                 Label = addLabel
        //             |})
        //         Disabled = field.IsDisabled || fieldConfig.Disabled
        //     }

        let custom
            (config: CustomConfig<'Msg, 'Attributes>)
            (viewConfig: ViewConfig<'Values, 'Msg>)
            (form: Form<'Values, 'Msg, 'Attributes>)
            (model: Model<'Values>)
            =
            let (fields, result) =
                let res = fill form model.Values
                res.Fields, res.Result

            let (ErrorTracking errorTracking) = model.ErrorTracking

            let onSubmit =
                match result, model.State, errorTracking.ShowAllErrors with
                | Ok msg, Loading, _ -> Some msg
                | Result.Error _, _, false ->
                    viewConfig.OnChange { model with ErrorTracking = ErrorTracking {| errorTracking with ShowAllErrors = true |} }
                    |> Some
                | _ -> None

            let onBlur =
                match viewConfig.Validation with
                | ValidateOnSubmit -> None
                | ValidateOnBlur ->
                    Some(fun label ->
                        viewConfig.OnChange {
                            model with
                                ErrorTracking =
                                    ErrorTracking {| errorTracking with ShowFieldError = Set.add label errorTracking.ShowFieldError |}
                        })

            let showError(label: string) =
                errorTracking.ShowAllErrors || Set.contains label errorTracking.ShowFieldError

            let fieldToElement =
                renderField viewConfig.Dispatch config {
                    OnChange = fun values -> viewConfig.OnChange { model with Values = values }
                    OnBlur = onBlur
                    Disabled = model.State = Loading
                    ShowError = showError
                }

            config.Form {
                Dispatch = viewConfig.Dispatch
                OnSubmit = onSubmit
                Action = viewConfig.Action
                State = model.State
                Fields = List.map fieldToElement fields
            }

[<RequireQualifiedAccess>]
module Avalonia =
    module View =
        open System
        open Avalonia
        open Avalonia.Controls.ApplicationLifetimes
        open Avalonia.Input
        open Avalonia.Controls
        open Avalonia.Layout
        open Avalonia.Media
        open Avalonia.Media.Imaging
        open Avalonia.Platform
        open Avalonia.Styling
        open Avalonia.Markup.Xaml.MarkupExtensions
        open Avalonia.Markup.Xaml.XamlIl.Runtime
        open Avalonia.Markup.Xaml
        open Avalonia.FuncUI
        open Avalonia.FuncUI.DSL
        open Avalonia.FuncUI.Hosts
        open Avalonia.FuncUI.Elmish
        open Avalonia.FuncUI.VirtualDom
        open Avalonia.FuncUI.Types

        open Form.View

        let fieldLabel(label: string) : IView =
            TextBlock.create [ TextBlock.text label ]

        let errorMessage(message: string) =
            TextBlock.create [
                TextBlock.text message
                // TODO: Fix this
                TextBlock.foreground Colors.Red
            ]

        let errorMessageAsHtml (showError: bool) (error: Error.Error option) =
            match error with
            | Some(Error.External externalError) -> errorMessage externalError
            | _ ->
                if showError then
                    error
                    |> Option.map errorToString
                    |> Option.map errorMessage
                    |> Option.defaultValue(TextBlock.create [])
                else
                    TextBlock.create []

        let wrapInFieldContainer(children: IView list) : IView = Grid.create [ Grid.children children ]

        let withLabelAndError (label: string) (showError: bool) (error: Error.Error option) (fieldAsHtml: IView) : IView =
            [
                fieldLabel label
                fieldAsHtml
                errorMessageAsHtml showError error
            ]
            |> wrapInFieldContainer

        let inputField
            (typ: InputType)
            ({
                 Dispatch = dispatch
                 OnChange = onChange
                 OnBlur = onBlur
                 Disabled = disabled
                 Value = value
                 Error = error
                 ShowError = showError
                 Attributes = attributes
             }: TextFieldConfig<'Msg, IAttr>)
            =

            let inputFunc =
                match typ with
                | Text -> TextBox.create
            // | Password -> Bulma.input.password
            // | Email -> Bulma.input.email
            // | Color -> Bulma.input.color
            // | Date -> Bulma.input.date
            // | DateTimeLocal -> Bulma.input.datetimeLocal
            // | Number -> Bulma.input.number
            // | Search -> Bulma.input.search
            // | Tel -> Bulma.input.tel
            // | Time -> Bulma.input.time
            Grid.create [
                Grid.columnDefinitions "*,*"
                Grid.children [
                    TextBlock.create [
                        Grid.column 0
                        TextBlock.text attributes.Label
                    ]
                    inputFunc [
                        Grid.column 1
                        match onBlur with
                        | Some onBlur -> TextBox.onLostFocus(fun _ -> dispatch onBlur)
                        | None -> ()
                        TextBox.text value
                        TextBox.onTextChanged(onChange >> dispatch)
                        TextBox.isEnabled(not disabled)
                        TextBox.watermark attributes.Placeholder
                        if showError && error.IsSome then
                            DataValidationErrors.errors [ error |> Option.map errorToString |> Option.defaultValue "" ]
                        yield! attributes.HtmlAttributes |> List.map(fun attr -> attr :?> _)
                    ]
                ]
            ]
            : IView
        // |> withLabelAndError attributes.Label showError error

        let textareaField
            ({
                 Dispatch = dispatch
                 OnChange = onChange
                 OnBlur = onBlur
                 Disabled = disabled
                 Value = value
                 Error = error
                 ShowError = showError
                 Attributes = attributes
             }: TextFieldConfig<'Msg, IAttr>)
            =

            TextBox.create [
                match onBlur with
                | Some onBlur -> TextBox.onLostFocus(fun _ -> dispatch onBlur)
                | None -> ()
                TextBox.onTextChanged(onChange >> dispatch)
                TextBox.isEnabled(not disabled)
                TextBox.text value
                TextBox.watermark attributes.Placeholder
                if showError && error.IsSome then
                    // TODO: Fix this
                    TextBox.foreground Colors.Red

                yield! attributes.HtmlAttributes |> List.map(fun attr -> attr :?> _)
            ]
            |> withLabelAndError attributes.Label showError error

        type CheckBox with
            static member onChecked<'t when 't :> CheckBox>(func: bool -> unit, ?subPatchOptions) =
                Builder.AttrBuilder<'t>
                    .CreateSubscription<bool Nullable>(
                        CheckBox.IsCheckedProperty,
                        (fun (value: bool Nullable) ->
                            if value.HasValue then
                                func value.Value),
                        ?subPatchOptions = subPatchOptions
                    )

        let checkboxField
            ({
                 Dispatch = dispatch
                 OnChange = onChange
                 OnBlur = onBlur
                 Disabled = disabled
                 Value = value
                 Attributes = attributes
             }: CheckboxFieldConfig<'Msg>)
            =
            wrapInFieldContainer [
                TextBlock.create [ TextBlock.text attributes.Text ]
                CheckBox.create [
                    CheckBox.onChecked(onChange >> dispatch)
                    match onBlur with
                    | Some onBlur -> CheckBox.onLostFocus(fun _ -> dispatch onBlur)
                    | None -> ()
                    CheckBox.isEnabled(not disabled)
                    CheckBox.isChecked value
                ]
            ]

        //         let radioField
        //             ({
        //                  Dispatch = dispatch
        //                  OnChange = onChange
        //                  OnBlur = onBlur
        //                  Disabled = disabled
        //                  Value = value
        //                  Error = error
        //                  ShowError = showError
        //                  Attributes = attributes
        //              }: RadioFieldConfig<'Msg>)
        //             =
        //             let radio(key: string, label: string) =
        //                 Bulma.input.labels.radio [
        //                     Bulma.input.radio [
        //                         prop.name attributes.Label
        //                         prop.isChecked(key = value: bool)
        //                         prop.disabled disabled
        //                         prop.onChange(fun (_: bool) -> onChange key |> dispatch)
        //                         match onBlur with
        //                         | Some onBlur -> prop.onBlur(fun _ -> dispatch onBlur)
        //                         | None -> ()
        //                     ]

        //                     Html.text label
        //                 ]

        //             Bulma.control.div [ attributes.Options |> List.map radio |> prop.children ]
        //             |> withLabelAndError attributes.Label showError error

        //         let selectField
        //             ({
        //                  Dispatch = dispatch
        //                  OnChange = onChange
        //                  OnBlur = onBlur
        //                  Disabled = disabled
        //                  Value = value
        //                  Error = error
        //                  ShowError = showError
        //                  Attributes = attributes
        //              }: SelectFieldConfig<'Msg>)
        //             =

        //             let toOption(key: string, label: string) =
        //                 Html.option [
        //                     prop.value key
        //                     prop.text label
        //                 ]

        //             let placeholderOption =
        //                 Html.option [
        //                     prop.disabled true
        //                     prop.value ""
        //                     prop.text("-- " + attributes.Placeholder + " --")
        //                 ]

        //             Bulma.select [
        //                 prop.disabled disabled
        //                 prop.onChange(onChange >> dispatch)

        //                 match onBlur with
        //                 | Some onBlur -> prop.onBlur(fun _ -> dispatch onBlur)
        //                 | None -> ()
        //                 prop.value value
        //                 prop.children [
        //                     placeholderOption

        //                     yield! attributes.Options |> List.map toOption
        //                 ]
        //             ]
        //             |> withLabelAndError attributes.Label showError error

        //         let fileField
        //             ({
        //                  Dispatch = dispatch
        //                  OnChange = onChange
        //                  Disabled = disabled
        //                  Value = value
        //                  Error = error
        //                  ShowError = showError
        //                  Attributes = attributes
        //              }: FileFieldConfig<'Msg>)
        //             =

        //             let fileInput =
        //                 Bulma.file [
        //                     if not(value |> Array.isEmpty) then
        //                         Bulma.file.hasName

        //                     prop.children [
        //                         Bulma.fileLabel.label [
        //                             Bulma.fileInput [
        //                                 prop.onInput(fun x ->
        //                                     let files = (x.currentTarget :?> Browser.Types.HTMLInputElement).files
        //                                     let files = Array.init files.length (fun i -> files[i])
        //                                     files |> onChange |> dispatch)
        //                                 prop.multiple attributes.Multiple
        //                                 match attributes.Accept with
        //                                 | FileField.FileType.Any -> ()
        //                                 | FileField.FileType.Specific fileTypes -> prop.accept(fileTypes |> String.concat ",")
        //                                 prop.disabled disabled
        //                             ]
        //                             Bulma.fileCta [
        //                                 match attributes.FileIconClassName with
        //                                 | FileField.FileIconClassName.Default ->
        //                                     Bulma.fileIcon [
        //                                         prop.innerHtml
        //                                             """<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-upload">
        //     <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/>
        //     <polyline points="17 8 12 3 7 8"/>
        //     <line x1="12" x2="12" y1="3" y2="15"/>
        // </svg>
        // <!--
        //     This icon has been taken from Lucide icons project

        //     See: https://lucide.dev/license
        // -->"""
        //                                     ]

        //                                 | FileField.FileIconClassName.Custom className -> Bulma.fileIcon [ Html.i [ prop.className className ] ]

        //                                 Bulma.fileLabel.span [ prop.text attributes.InputLabel ]
        //                             ]

        //                             if not(value |> Array.isEmpty) then
        //                                 Bulma.fileName [ prop.text (value |> Array.head).name ]
        //                         ]
        //                     ]
        //                 ]

        //             fileInput |> withLabelAndError attributes.Label showError error

        //         let group(fields: IView list) =
        //             Bulma.field.div [ Bulma.columns [ fields |> List.map Bulma.column |> prop.children ] ]

        //         let section (title: string) (fields: IView list) =
        //             Html.fieldSet [
        //                 prop.className "fieldset"

        //                 prop.children [
        //                     Html.legend [ prop.text title ]

        //                     yield! fields
        //                 ]
        //             ]

        let ignoreChildError
            (parentError: Error.Error option)
            (field: Form.FilledField<'Values, IAttr>)
            : Form.FilledField<'Values, IAttr> =
            match parentError with
            | Some _ -> field
            | None -> { field with Error = None }

        // let formList
        //     ({
        //          Dispatch = dispatch
        //          Forms = forms
        //          Label = label
        //          Add = add
        //          Disabled = disabled
        //      }: FormListConfig<'Msg>)
        //     =
        //     StackPanel.create [
        //         StackPanel.children [
        //             fieldLabel label
        //             yield! forms
        //             match disabled, add with
        //             | (false, Some add) ->
        //                 Button.create [
        //                     Button.onClick(fun _ -> add.Action() |> dispatch)
        //                     // prop.children [
        //                     //     Bulma.icon [
        //                     //         icon.isSmall
        //                     //         prop.children [ Html.i [ prop.className "fas fa-plus" ] ]
        //                     //     ]
        //                     Button.content add.Label
        //                 ]
        //             | _ -> ()
        //         ]
        //     ]

        // let formListItem
        //     ({
        //          Dispatch = dispatch
        //          Fields = fields
        //          Delete = delete
        //          Disabled = disabled
        //      }: FormListItemConfig<'Msg>)
        //     =

        //     let removeButton =
        //         match disabled, delete with
        //         | (false, Some delete) ->
        //             Bulma.button.a [
        //                 prop.onClick(fun _ -> delete.Action() |> dispatch)

        //                 prop.children [
        //                     Bulma.icon [
        //                         icon.isSmall

        //                         prop.children [ Html.i [ prop.className "fas fa-times" ] ]
        //                     ]

        //                     if delete.Label <> "" then
        //                         Html.span delete.Label
        //                 ]
        //             ]

        //         | _ -> Html.none

        //     Html.div [
        //         prop.className "form-list"

        //         prop.children [
        //             yield! fields

        //             Bulma.field.div [
        //                 field.isGrouped
        //                 field.isGroupedRight

        //                 prop.children [ Bulma.control.div [ removeButton ] ]
        //             ]
        //         ]
        //     ]

        let form
            ({
                 Dispatch = dispatch
                 OnSubmit = onSubmit
                 State = state
                 Action = action
                 Fields = fields
             }: FormConfig<'Msg>)
            : IView =
            StackPanel.create [
                StackPanel.children [
                    yield! fields
                    match state with
                    | Error error -> errorMessage error
                    | Success success -> TextBlock.create [ TextBlock.text success ]
                    | Loading
                    | Idle ->
                        // TODO: Fix this
                        () //Html.none

                    match action with
                    | Action.SubmitOnly submitLabel ->
                        Button.create [
                            // Button.onClick(fun _ -> onSubmit |> Option.map dispatch |> Option.defaultWith ignore)
                            Button.onClick(fun _ ->
                                match onSubmit with
                                | Some onSubmit -> dispatch onSubmit
                                | _ -> ())
                            Button.content submitLabel
                        ]
                    | Action.Custom func -> func state dispatch
                ]
            ]

        let htmlViewConfig<'Msg> : CustomConfig<'Msg, IAttr> = {
            Form = form
            TextField = inputField Text
            // PasswordField = inputField Password
            // EmailField = inputField Email
            TextAreaField = textareaField
            // ColorField = inputField Color
            // DateField = inputField Date
            // DateTimeLocalField = inputField DateTimeLocal
            // NumberField = inputField Number
            // SearchField = inputField Search
            // TelField = inputField Tel
            // TimeField = inputField Time
            CheckboxField = checkboxField
        // RadioField = radioField
        // SelectField = selectField
        // FileField = fileField
        // Group = group
        // Section = section
        // FormList = formList
        // FormListItem = formListItem
        }

        let asHtml(config: ViewConfig<'Values, 'Msg>) = custom htmlViewConfig config