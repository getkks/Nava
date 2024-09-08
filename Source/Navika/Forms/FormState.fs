namespace Navika.Forms

open System
open Elmish
open TypeShape.Core
open Navika.Reflection

module FormModel =
    [<NoComparison; NoEquality>]
    type FormModel<'TModel> = {
        Model: 'TModel
        ShapeMembers: IShapeMember<'TModel>[]
        State: int
    }

    type Message<'TModel> =
        | Changed of FormModel<'TModel>
        | Submit of FormModel<'TModel>

    let create model : FormModel<'TObject> = {
        Model = model
        ShapeMembers = ObjectShape.Members
        State = 0
    }