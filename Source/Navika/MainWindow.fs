namespace Navika

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

open Elmish

open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Hosts
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI.VirtualDom
open Avalonia.FuncUI.Types
open Avalonia.Svg.Skia
open TypeShape.Core

type Options = {
    Input: string
    Output: string
    Overwrite: bool
}

module Shell =
    let labeledView (label: string) (view: IView) : IView list = [
        TextBlock.create [ TextBlock.text label ]
        view
    ]

    type CheckBox with
        static member onChecked<'t when 't :> CheckBox>(func: bool -> unit, ?subPatchOptions) =
            Builder.AttrBuilder<'t>
                .CreateSubscription<bool Nullable>(
                    property = CheckBox.IsCheckedProperty,
                    func =
                        (fun (value: bool Nullable) ->
                            if value.HasValue then
                                func value.Value),
                    ?subPatchOptions = subPatchOptions
                )

    let inline wrapGetter<'TRecord, 'T, 'R>([<InlineIfLambda>] func: 'TRecord -> 'T) = unbox<'TRecord -> 'R> func
    let inline wrapSetter<'TRecord, 'T, 'R>([<InlineIfLambda>] func: 'TRecord -> 'T -> 'TRecord) = unbox<'TRecord -> 'R -> 'TRecord> func

    let mapPrimitivesToView<'TRecord, 'TField> dispatch (shape: ShapeMember<'TRecord, 'TField>) object : IView =
        match shapeof<'TField> with
        | Shape.Bool ->
            let value = object |> (wrapGetter<'TRecord, 'TField, bool> shape.Get)
            let setValue = object |> (wrapSetter shape.Set)

            CheckBox.create [
                CheckBox.isChecked value
                CheckBox.onChecked(setValue >> dispatch)
            ]
        | Shape.String ->
            let value = object |> (wrapGetter shape.Get)
            let setValue = object |> (wrapSetter shape.Set)

            TextBox.create [
                TextBox.text value
                TextBox.onTextChanged(setValue >> dispatch)
            ]

    let mapMemberToView dispatch (shape: IShapeMember<'TRecord>) =
        shape.Accept
            { new IMemberVisitor<'TRecord, 'TRecord -> IView list> with
                member _.Visit(shape: ShapeMember<'TRecord, 'TField>) =
                    (fun (object: 'TRecord) ->
                        mapPrimitivesToView<'TRecord, 'TField> dispatch shape object
                        |> labeledView shape.Label)
            }

    let mapFSharpRecordToView dispatch (record: 'TRecord) =
        match shapeof<'TRecord> with
        | Shape.FSharpRecord(:? ShapeFSharpRecord<'TRecord> as shape) ->
            shape.Fields
            |> Array.map(fun memberShape -> mapMemberToView dispatch memberShape record)
            |> Array.toList
            |> List.concat

    open Navika.Controls

    let optionsView (options: Options) dispatch =
        let children = options |> mapFSharpRecordToView dispatch

        create [
            AutoGrid.columnWidth GridLength.Star
            AutoGrid.columnCount 2
            AutoGrid.rowHeight GridLength.Star
            AutoGrid.rowCount(List.length(children) >>> 1)
            children |> Grid.children
        ]

    let loadImage uri =
        uri |> Uri |> AssetLoader.Open |> Bitmap

    let loadSvg<'TAssembly> path =
        SvgImage(Source = SvgSource.Load(path, Uri $"avares://{(typeof<'TAssembly>).Assembly.GetName().Name}/"))
        |> Image.source

    type Values = {
        Count: int
        Options: Options
    }
    //<a href="https://www.flaticon.com/free-icons/crew" title="crew icons">Crew icons created by Freepik - Flaticon</a>
    [<NoComparison; NoEquality>]
    type Message =
        | FormSubmit of string * string
        | FormChanged of Options

    let init() =
        {
            Input = "Default Input"
            Output = "Default Output"
            Overwrite = true
        // Compression = IO.Compression.CompressionLevel.Optimal
        },
        Cmd.batch []

    let update message (model) =
        match message with
        | FormChanged newModel -> newModel, Cmd.none
        | FormSubmit(input, output) ->
            {
                model with
                    Input = input
                    Output = output
            },
            Cmd.none

    let view (model: Options) dispatch =
        Grid.create [
            Grid.columnDefinitions "48, 1*"
            Grid.rowDefinitions "48, *"
            Grid.children [
                Image.create [
                    Image.isHitTestVisible false
                    loadSvg<Message> "/Assets/processing.svg"
                    Image.width 32.0
                    Image.height 32.0
                    Grid.row 0
                    Grid.column 0
                ]
                Menu.create [
                    Menu.isHitTestVisible true
                    Grid.row 0
                    Grid.column 1
                    Menu.viewItems [
                        MenuItem.create [
                            MenuItem.header "File"
                            Menu.viewItems [ MenuItem.create [ MenuItem.header "Exit" ] ]
                        ]
                    ]
                ]
                StackPanel.create [
                    Grid.row 1
                    Grid.column 0
                    Grid.columnSpan 2
                    StackPanel.children [ optionsView model (FormChanged >> dispatch) ]
                ]
            ]
        ]

    type MainWindow() as this =
        inherit FluentAvalonia.UI.Windowing.AppWindow()
        let mutable lastViewElement: IView option = None
        // inherit HostWindow()

        do
            base.Title <- "Navika"
            base.Width <- 1600
            base.Height <- 1200
            // base.ExtendClientAreaToDecorationsHint <- true
            // base.ExtendClientAreaChromeHints <- ExtendClientAreaChromeHints.NoChrome
            // base.ExtendClientAreaTitleBarHeightHint <- -1
            // base.SystemDecorations <- SystemDecorations.BorderOnly
            base.TitleBar.ExtendsContentIntoTitleBar <- true
            base.TitleBar.TitleBarHitTestType <- FluentAvalonia.UI.Windowing.TitleBarHitTestType.Complex
#if DEBUG
            base.AttachDevTools()
#endif
            Program.mkProgram init update view
            |> Program.withHost this
#if DEBUG
            |> Program.withConsoleTrace
#endif
            |> Program.run

        interface IViewHost with
            member this.Update next =
                VirtualDom.updateRoot(this, lastViewElement, next)
                lastViewElement <- next