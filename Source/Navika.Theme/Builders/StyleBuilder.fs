namespace Navika.Theme.Builders

open System
open System.Collections.Generic
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Data
open Avalonia.Data.Core
open Avalonia.Input
open Avalonia.Markup.Xaml
open Avalonia.Markup.Xaml.MarkupExtensions
open Avalonia.Markup.Xaml.XamlIl.Runtime
open Avalonia.Media
open Avalonia.Styling
open Navika.Theme

[<NoComparison; NoEquality>]
type 'T StyleBuilder =
    val mutable Context: 'T Context
    val mutable Style: Style

    new(context: 'T Context) =
        let style = Style()
        context.PushParent style

        {
            Context = context
            Style = style
        }

    member this.Zero() = ()
    // member inline this.Delay([<InlineIfLambda>] f: unit -> Style) = f

    // member this.Combine(selector: Selector, style: Style) =
    //     this.Style.Selector <- selector
    //     this.Style

    member this.Yield(_: unit) = this.Style

    // member this.Yield(selector: Selector) =
    //     this.Style.Selector <- selector
    //     this.Style

    // member this.Yield(setter: Setter) =
    //     this.Style.Setters.Add setter
    //     this.Style

    member inline this.Run([<InlineIfLambda>] delay: unit -> Style) =
        this.Context.PopParent()
        this.Context.AvaloniaNameScope.Complete()
        this.Style

    member inline this.Run(_: unit) =
        this.Context.PopParent()
        this.Context.AvaloniaNameScope.Complete()
        this.Style

    [<CustomOperation("styleSelector")>]
    member this.StyleSelector(_, selector: Selector) = this.Style.Selector <- selector

    [<CustomOperation("setter")>]
    member this.Setter(_, setter: Setter) = this.Style.Setters.Add setter

    [<CustomOperation("setter")>]
    member this.Setter<'TProp>(_, property: 'TProp StyledProperty, value: 'TProp) =
        this.Style.Setters.Add(Setter(Property = property, Value = value))

    [<CustomOperation("setter")>]
    member this.Setter<'TProp>(_, property: 'TProp AvaloniaProperty, value: 'TProp) =
        this.Style.Setters.Add(Setter(Property = property, Value = value))

    [<CustomOperation("dynamicSetter")>]
    member this.DynamicSetter<'TProp>(_, property: 'TProp StyledProperty, value: string) =
        let setter = Setter()
        setter.Property <- property
        this.Context.PushParent setter
        this.Context.ProvideTargetProperty <- SetterBuilder.setterValueProperty
        setter.Value <- DynamicResourceExtension(value).ProvideValue this.Context
        this.Context.ProvideTargetProperty <- null
        this.Context.PopParent()
        this.Style.Setters.Add setter

// member inline this.Zero() = fun (context: 'T Context) -> Style()
// member inline this.Delay([<InlineIfLambda>] f: unit -> 'T Context -> Style) = f

// member inline this.Combine(selector: Selector, style: 'T Context -> Style) =
//     fun context ->
//         let style = style context
//         style.Selector <- selector
//         style

// member inline this.Combine(style: 'T Context -> Style, selector: Selector) = this.Combine(selector, style)

// member inline this.Combine(styler: Context<'a> -> Style, delay: unit -> Context<'a> -> Style) =
//     fun context ->
//         delay () context |> ignore
//         context |> styler

// member inline this.Yield([<InlineIfLambda>] styler: 'T Context -> Style) = styler
// member inline this.Yield(_: unit) = this.Zero()

// member inline this.Yield(selector: Selector) =
//     fun (context: 'T Context) ->
//         let style = Style()
//         style.Selector <- selector
//         style

// member inline this.Run([<InlineIfLambda>] styler: 'T Context -> Style) =
//     fun context ->
//         let style = styler context
//         context.PopParent()
//         context.AvaloniaNameScope.Complete()
//         style

// member inline this.Run([<InlineIfLambda>] delay: unit -> 'T Context -> Style) = delay()

// [<CustomOperation("setter")>]
// member inline this.Setter(styler: 'T Context -> Style, setter: Setter) =
//     fun context ->
//         let style = styler context
//         style.Setters.Add setter
//         style

// [<CustomOperation("setter")>]
// member inline this.Setter<'TProp>(styler: 'T Context -> Style, setter: Setter, property: 'TProp StyledProperty, value: 'TProp) =
//     fun context ->
//         let style = styler context
//         style.Setters.Add(Setter(Property = property, Value = value))
//         style

// [<CustomOperation("setter")>]
// member inline this.Setter<'TProp>(styler: 'T Context -> Style, property: 'TProp AvaloniaProperty, value: 'TProp) =
//     fun context ->
//         let style = styler context
//         style.Setters.Add(Setter(Property = property, Value = value))
//         style

// [<CustomOperation("dynamicSetter")>]
// member inline this.DynamicSetter<'TProp>(styler: 'T Context -> Style, property: 'TProp StyledProperty, value: string) =
//     fun context ->
//         let style = styler context
//         let setter = Setter()
//         setter.Property <- property
//         context.PushParent setter
//         context.ProvideTargetProperty <- SetterBuilder.setterValueProperty
//         setter.Value <- DynamicResourceExtension(value).ProvideValue context
//         context.ProvideTargetProperty <- null
//         context.PopParent()
//         style.Setters.Add setter
//         style

module StyleBuilder =
    let style context = StyleBuilder context
// let style() = StyleBuilder<'T>()