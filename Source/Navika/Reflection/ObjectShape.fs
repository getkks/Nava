namespace Navika.Reflection

open System
open System.Collections.Generic
open System.Reflection
open System.Runtime.CompilerServices

open TypeShape
open TypeShape.Core

[<RequireQualifiedAccess>]
module MemberVisitor =
    type MemberVisitor<'TObject, 'TReturn>(func) =
        interface IMemberVisitor<'TObject, 'TReturn> with
            member _.Visit(shape: ShapeMember<'TObject, 'TField>) : 'TReturn = shape |> box |> (func |> unbox)

    let visit<'TObject, 'TReturn> (func: obj -> 'TReturn) (shape: IShapeMember<'TObject>) : 'TReturn =
        func |> box |> MemberVisitor |> shape.Accept

type ObjectShape<'TObject>() =
    static let shapeMembers =
        match shapeof<'TObject> with
        | Shape.FSharpRecord(:? ShapeFSharpRecord<'TObject> as shape) -> shape.Fields
        | Shape.CliMutable(:? ShapeCliMutable<'TObject> as shape) -> shape.Properties |> Array.filter _.IsPublic
        | Shape.Poco(:? ShapePoco<'TObject> as shape) ->
            [|
                shape.Fields |> Array.filter _.IsPublic
                [|
                    for shape in shape.Properties do
                        match shape with
                        | :? IShapeMember<'TObject> as shape when shape.IsPublic -> shape
                        | _ -> ()
                |]
            |]
            |> Array.concat
        | _ -> failwith "not a record"

    static member Members = shapeMembers