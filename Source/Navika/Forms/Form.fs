namespace Navika.Forms

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
open Navika.Reflection
open FormModel

module Form =

    let init model = FormModel.create model, Cmd.none

    let update message model =
        match message with
        | Changed newModel -> newModel, Cmd.none
        | Submit newModel -> newModel, Cmd.none

    let getValue object (shape: ShapeMember<'TObject, 'TField>) = object |> shape.Get
    let setValue object value (shape: ShapeMember<'TObject, 'TField>) = value |> shape.Set object

    let panelProps<'TView when 'TView :> Control> row column : 'TView IAttr list = [
        Grid.column column
        Grid.row row
    ]

    let createLabel index label : IView =
        TextBlock.create [
            TextBlock.text label
            yield! panelProps index 0
        ]

    let view (model: FormModel<'TObject>) dispatch =
        Grid.create [
            Grid.columnDefinitions "*, *"
            Grid.rowDefinitions((String.Join(",", Linq.Enumerable.Repeat("*", model.ShapeMembers.Length >>> 1))))
            Grid.children [
                let mutable index = 0

                for memberShape in model.ShapeMembers do
                    yield!
                        memberShape
                        |> MemberVisitor.visit<_, IView list>(fun (shapeMember: obj) ->
                            match shapeMember with
                            | :? ShapeMember<'TObject, bool> as shape -> [
                                createLabel index shape.Label
                                CheckBox.create [
                                    yield! panelProps index 1
                                    shape |> getValue model.Model |> CheckBox.isChecked
                                ]
                              ]
                            | :? ShapeMember<'TObject, string> as shape -> [
                                createLabel index shape.Label
                                TextBox.create [
                                    yield! panelProps index 1
                                    shape |> getValue model.Model |> TextBox.text
                                ]
                              ]
                            | _ -> [
                                TextBlock.create [
                                    TextBlock.text "TODO"
                                    yield! panelProps index 1
                                ]
                              ])
            ]
        ]