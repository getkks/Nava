namespace Navika.Theme.Builders

open System
open System.Collections.Generic
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Data
open Avalonia.Input
open Avalonia.Markup.Xaml
open Avalonia.Markup.Xaml.MarkupExtensions
open Avalonia.Markup.Xaml.XamlIl.Runtime
open Avalonia.Media
open Avalonia.Styling
open Microsoft.FSharp.Core

[<NoComparison; NoEquality>]
type SelectorBuilder() =
    member _.Yield _ = Unchecked.defaultof<Selector>
    member _.Run(selector: Selector) = selector

    [<CustomOperation("className")>]
    member _.Class(selector: Selector, name: string) = selector.Class name

    [<CustomOperation("name")>]
    member _.Name(selector: Selector, name: string) = selector.Name name

    [<CustomOperation("object")>]
    member _.OfType(selector: Selector, elementType: Type) = selector.OfType elementType

    [<CustomOperation("is")>]
    member _.Is(selector: Selector, elementType: Type) = selector.Is elementType

module SelectorBuilder =
    let selector = SelectorBuilder()