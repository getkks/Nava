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

type ResourceBuilder<'T when 'T :> IStyleHost> =
    val mutable context: 'T Context
    val mutable resource: ResourceDictionary

    new(context: 'T Context, resource: ResourceDictionary) =
        {
            context = context
            resource = resource
        }

    member this.GetRootObject(serviceProvider: IServiceProvider) =
        match serviceProvider with
        | null -> ValueNone
        | _ ->
            match serviceProvider.GetService<IRootObjectProvider>().RootObject with
            | :? 'T as rootObject -> ValueSome rootObject
            | _ -> ValueNone

    member this.GetOrCreateContext(serviceProvider: IServiceProvider) =
        match serviceProvider with
        | :? Context<'T> as context -> context
        | _ -> Context(serviceProvider |> this.GetRootObject, serviceProvider, this.context.BaseUri.ToString())

    [<CustomOperation("resource")>]
    member this.Resource<'TValue>(resourceDictionary: ResourceDictionary, name: string, value: 'TValue) =
        resourceDictionary.Add(name, value)
        resourceDictionary

    [<CustomOperation("resourceDeferred")>]
    member this.ResourceDeferred<'TValue>(resourceDictionary: ResourceDictionary, name: string, value: 'TValue) : ResourceDictionary =
        resourceDictionary.AddDeferred(
            name,
            XamlIlRuntimeHelpers.DeferredTransformationFactoryV2((fun _ -> value :> _), this.context :> IServiceProvider)
        )

        resourceDictionary

    member this.ControlThemeDeferred
        (resourceDictionary: ResourceDictionary, themer: ControlThemeBuilder<'T, 'TControl> -> ControlTheme)
        : ResourceDictionary =
        resourceDictionary.AddDeferred(
            typeof<'TControl>,
            XamlIlRuntimeHelpers.DeferredTransformationFactoryV2(
                (fun serviceProvider -> this.context |> ControlThemeBuilder |> themer :> _),
                this.context :> IServiceProvider
            )
        )

        resourceDictionary

    [<CustomOperation("staticResource")>]
    member this.StaticResource<'TObject, 'TValue when 'TObject: (new: unit -> 'TObject) and 'TObject :> AvaloniaObject>
        (
            resourceDictionary: ResourceDictionary,
            property: 'TValue StyledProperty,
            name: string,
            staticResourceName: string,
            initializer: 'TObject -> 'TObject,
            setter: 'TObject -> 'TValue -> unit
        ) : ResourceDictionary =
        resourceDictionary.AddDeferred(
            name,
            XamlIlRuntimeHelpers.DeferredTransformationFactoryV2(
                (fun serviceProvider ->
                    let context = this.GetOrCreateContext serviceProvider
                    let object = new 'TObject() |> initializer
                    context.PushParent object
                    context.ProvideTargetProperty <- property
                    let resource = StaticResourceExtension staticResourceName

                    match resource.ProvideValue context with
                    | :? 'TValue as value -> setter object value
                    | :? IBinding as binding -> object.Bind(property, binding) |> ignore
                    | :? UnsetValueType as _ -> ()
                    | null -> NullReferenceException() |> raise
                    | _ -> InvalidCastException() |> raise

                    context.PopParent()
                    object :> obj),
                this.context :> IServiceProvider
            )
        )

        resourceDictionary

    [<CustomOperation("fontFamily")>]
    member this.FontFamily(resourceDictionary: ResourceDictionary, name: string, font: FontFamily) =
        this.Resource(resourceDictionary, name, font)

    [<CustomOperation("fontFamily")>]
    member this.FontFamily(resourceDictionary: ResourceDictionary, name: string, font: string) =
        this.FontFamily(resourceDictionary, name, FontFamily((this.context :> IUriContext).BaseUri, font))

    [<CustomOperation("solidColor")>]
    member this.SolidColor(resourceDictionary: ResourceDictionary, name: string, color: Color) =
        this.Resource(resourceDictionary, name, color)

    [<CustomOperation("solidColor")>]
    member this.SolidColor(resourceDictionary: ResourceDictionary, name: string, color: uint) =
        this.SolidColor(resourceDictionary, name, Color.FromUInt32 color)

    [<CustomOperation("staticSolidColorBrush")>]
    member this.StaticSolidColorBrush(resourceDictionary: ResourceDictionary, name: string, colorName: string) =
        this.StaticResource<SolidColorBrush, _>(
            resourceDictionary,
            SolidColorBrush.ColorProperty,
            name,
            colorName,
            id,
            (fun brush color -> brush.Color <- color)
        )

    [<CustomOperation("staticSolidColorBrush")>]
    member this.StaticSolidColorBrush(resourceDictionary: ResourceDictionary, name: string, colorName: string, opacity) =
        this.StaticResource<SolidColorBrush, _>(
            resourceDictionary,
            SolidColorBrush.ColorProperty,
            name,
            colorName,
            (fun brush ->
                brush.Opacity <- opacity
                brush),
            (fun brush color -> brush.Color <- color)
        )

    [<CustomOperation("staticSolidColorBrush")>]
    member this.StaticSolidColorBrush(resourceDictionary: ResourceDictionary, name: string, color: Color) =
        this.ResourceDeferred(resourceDictionary, name, SolidColorBrush color)

    [<CustomOperation("staticSolidColorBrush")>]
    member this.StaticSolidColorBrush(resourceDictionary: ResourceDictionary, name: string, color: uint) =
        this.StaticSolidColorBrush(resourceDictionary, name, Color.FromUInt32 color)

    [<CustomOperation("staticSolidColorBrush")>]
    member this.StaticSolidColorBrush(resourceDictionary: ResourceDictionary, name: string, color: Color, opacity) =
        this.ResourceDeferred(resourceDictionary, name, SolidColorBrush(color, opacity))

    [<CustomOperation("staticSolidColorBrush")>]
    member this.StaticSolidColorBrush(resourceDictionary: ResourceDictionary, name: string, color: uint, opacity) =
        this.StaticSolidColorBrush(resourceDictionary, name, Color.FromUInt32 color, opacity)

    member this.Zero() = ()
    member inline this.Delay([<InlineIfLambda>] f: unit -> ResourceDictionary) = f()

    member this.Yield(_: unit) =
        this.context.PushParent this.resource
        this.resource

    member this.Run(resourceDictionary: ResourceDictionary) =
        this.context.PopParent()

        this.context
        |> this.GetRootObject
        |> ValueOption.iter(fun host -> host.Styles.Resources <- resourceDictionary)

module ResourceBuilder =

    let resource context = ResourceBuilder context