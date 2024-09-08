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
type ThemeBuilder<'T when 'T :> IStyleHost> =
    val mutable context: 'T Context
    val mutable rootObject: 'T
    val mutable nameSpace: string
    val mutable typeName: string

    new(rootObject: 'T, nameSpace: string, typeName: string) =
        {
            rootObject = rootObject
            nameSpace = nameSpace
            typeName = typeName
            context =
                Context<'T>(
                    ValueSome rootObject,
                    XamlIlRuntimeHelpers.CreateRootServiceProviderV3 null,
                    "avares://" + nameSpace + "/" + typeName
                )
        }

    member this.CreateOrGetResource() =
        match this.rootObject.Styles.Resources with
        | null ->
            let resource = ResourceDictionary()
            this.rootObject.Styles.Resources <- resource
            resource
        | resources -> resources |> unbox

    member this.Zero() = this.context
    member inline this.Delay([<InlineIfLambda>] f: unit -> 'T Context) = f()

    member inline this.Combine(style: Style, context: 'T Context) =
        style |> this.rootObject.Styles.Add
        context

    member this.Yield(styler: 'T Context -> Style) =
        this.context |> styler |> this.rootObject.Styles.Add
        this.context

    member this.Yield(context: _ Context) = this.context
    member this.Yield(_: unit) = this.context
    member _.Run(closer: unit -> 'T Context) = closer() |> ignore
    member _.Run(_) = ()

    [<CustomOperation("style")>]
    member inline this.Style(_: 'T Context, [<InlineIfLambdaAttribute>] (styler: 'T StyleBuilder -> Style)) =
        StyleBuilder.style this.context |> styler |> this.rootObject.Styles.Add
        this.context

    [<CustomOperation("styles")>]
    member this.Styles(_: 'T Context, styles) =
        for style in styles do
            style |> this.rootObject.Styles.Add

    [<CustomOperation("defaultTheme")>]
    member inline this.DefaultTheme(_: 'T Context, [<InlineIfLambdaAttribute>] themer) =
        let resource = this.CreateOrGetResource()
        this.context.PushParent resource
        let themeResource = ResourceDictionary()
        ResourceBuilder(this.context, themeResource) |> themer
        resource[ThemeVariant.Default] <- themeResource
        this.context.PopParent()
        this.context

    [<CustomOperation("resources")>]
    member inline this.Resources(_: 'T Context, [<InlineIfLambdaAttribute>] themer) =
        let resource = this.CreateOrGetResource()
        ResourceBuilder(this.context, resource) |> themer
        this.context

module ThemeBuilder =
    let context rootObject nameSpace typeName =
        // ThemeBuilder(rootObject, nameSpace, typeName)
        Context<'T>(rootObject, XamlIlRuntimeHelpers.CreateRootServiceProviderV3 null, [||], "avares://" + nameSpace + "/" + typeName)