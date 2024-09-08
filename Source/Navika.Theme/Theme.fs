namespace Navika

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

open Navika.Theme.Builders
open SelectorBuilder
open StyleBuilder
open Navika.Theme

module Themes =
    let getRootObject(serviceProvider: IServiceProvider) : IStyleHost =
        match serviceProvider with
        | null -> null
        | _ -> serviceProvider.GetService<IRootObjectProvider>().RootObject :?> _

    let createContext(serviceProvider: IServiceProvider) =
        Context(serviceProvider |> getRootObject |> ValueOption.ofObj, serviceProvider, [||], null)

    let createSolidColorBrush (color: Color) (parentContext: 'T Context) =
        XamlIlRuntimeHelpers.DeferredTransformationFactoryV2((fun _ -> color :> obj), parentContext :> IServiceProvider)

    let getSolidColorBrush (colorName: string) (parentContext: 'T Context) =
        XamlIlRuntimeHelpers.DeferredTransformationFactoryV2(
            (fun serviceProvider ->
                let context = createContext serviceProvider
                let brush = SolidColorBrush()
                context.PushParent brush
                context.ProvideTargetProperty <- SolidColorBrush.ColorProperty
                let resource = StaticResourceExtension colorName

                match resource.ProvideValue context with
                | :? Color as color -> brush.Color <- color
                | :? IBinding as binding -> brush.Bind(SolidColorBrush.ColorProperty, binding) |> ignore
                | :? UnsetValueType as _ -> ()
                | null -> NullReferenceException() |> raise
                | _ -> InvalidCastException() |> raise

                context.PopParent()
                brush :> obj),
            parentContext :> IServiceProvider
        )

    let defaultThemeColors(resource: 'T ResourceBuilder) =
        resource {
            solidColor "ThemeBackgroundColor" UInt32.MaxValue
            solidColor "ThemeBorderLowColor" 4289374890u
            solidColor "ThemeBorderMidColor" 4287137928u
            solidColor "ThemeBorderHighColor" 4281545523u
            solidColor "ThemeControlLowColor" 4287007129u
            solidColor "ThemeControlMidColor" 4294309365u
            solidColor "ThemeControlMidHighColor" 4290954185u
            solidColor "ThemeControlHighColor" 4285032552u
            solidColor "ThemeControlVeryHighColor" 4284177243u
            solidColor "ThemeControlHighlightLowColor" 4293980400u
            solidColor "ThemeControlHighlightMidColor" 4291875024u
            solidColor "ThemeControlHighlightHighColor" 4286611584u
            solidColor "ThemeForegroundColor" 4278190080u
            solidColor "HighlightColor" 4278742942u
            solidColor "HighlightColor2" 4278804613u
            solidColor "HyperlinkVisitedColor" 4285013416u
            staticSolidColorBrush "ThemeBackgroundBrush" "ThemeBackgroundColor"
            staticSolidColorBrush "ThemeBorderLowBrush" "ThemeBorderLowColor"
            staticSolidColorBrush "ThemeBorderMidBrush" "ThemeBorderMidColor"
            staticSolidColorBrush "ThemeBorderHighBrush" "ThemeBorderHighColor"
            staticSolidColorBrush "ThemeControlLowBrush" "ThemeControlLowColor"
            staticSolidColorBrush "ThemeControlMidBrush" "ThemeControlMidColor"
            staticSolidColorBrush "ThemeControlMidHighBrush" "ThemeControlMidHighColor"
            staticSolidColorBrush "ThemeControlHighBrush" "ThemeControlHighColor"
            staticSolidColorBrush "ThemeControlVeryHighBrush" "ThemeControlVeryHighColor"
            staticSolidColorBrush "ThemeControlHighlightLowBrush" "ThemeControlHighlightLowColor"
            staticSolidColorBrush "ThemeControlHighlightMidBrush" "ThemeControlHighlightMidColor"
            staticSolidColorBrush "ThemeControlHighlightHighBrush" "ThemeControlHighlightHighColor"
            staticSolidColorBrush "ThemeForegroundBrush" "ThemeForegroundColor"
            staticSolidColorBrush "HighlightBrush" "HighlightColor"
            staticSolidColorBrush "HighlightBrush2" "HighlightColor2"
            staticSolidColorBrush "HyperlinkVisitedBrush" "HyperlinkVisitedColor"
            staticSolidColorBrush "RefreshVisualizerForeground" Colors.Black
            staticSolidColorBrush "RefreshVisualizerBackground" Colors.Transparent
            staticSolidColorBrush "CaptionButtonForeground" Colors.Black
            staticSolidColorBrush "CaptionButtonBackground" 4293256677u
            staticSolidColorBrush "CaptionButtonBorderBrush" 4291480266u
        }

    let resources(resource: 'T ResourceBuilder) =
        resource {
            fontFamily "ContentControlThemeFontFamily" "fonts:Inter#Inter, $Default"
            solidColor "ThemeAccentColor" 3423706842u
            solidColor "ThemeAccentColor2" 2568068826u
            solidColor "ThemeAccentColor3" 1712430810u
            solidColor "ThemeAccentColor4" 856792794u
            solidColor "ThemeForegroundLowColor" 4286611584u
            solidColor "HighlightForegroundColor" UInt32.MaxValue
            solidColor "ErrorColor" 4294901760u
            solidColor "ErrorLowColor" 285147136u
            staticSolidColorBrush "HighlightForegroundBrush" "HighlightForegroundColor"
            staticSolidColorBrush "ThemeForegroundLowBrush" "ThemeForegroundLowColor"
            staticSolidColorBrush "ThemeAccentBrush2" "ThemeAccentColor2"
            staticSolidColorBrush "ThemeAccentBrush3" "ThemeAccentColor3"
            staticSolidColorBrush "ThemeAccentBrush4" "ThemeAccentColor4"
            staticSolidColorBrush "ThemeAccentBrush" "ThemeAccentColor"
            staticSolidColorBrush "ErrorBrush" "ErrorColor"
            staticSolidColorBrush "ErrorLowBrush" "ErrorLowColor"
            staticSolidColorBrush "NotificationCardBackgroundBrush" 0x444444u 0.75
            staticSolidColorBrush "NotificationCardInformationBackgroundBrush" 0x007ACCu 0.75
            staticSolidColorBrush "NotificationCardSuccessBackgroundBrush" 0x1F9E45u 0.75
            staticSolidColorBrush "NotificationCardWarningBackgroundBrush" 0xFDB328u 0.75
            staticSolidColorBrush "NotificationCardErrorBackgroundBrush" 0xBD202Cu 0.75
            staticSolidColorBrush "ThemeControlTransparentBrush" Colors.Transparent
            staticSolidColorBrush "DatePickerFlyoutPresenterHighlightFill" "ThemeAccentColor" 0.4
            staticSolidColorBrush "TimePickerFlyoutPresenterHighlightFill" "ThemeAccentColor" 0.4
            resource "ThemeBorderThickness" (Thickness 1.0)
            resource "ThemeDisabledOpacity" 0.5
            resource "FontSizeSmall" 10.
            resource "FontSizeNormal" 12.
            resource "FontSizeLarge" 16.
            resource "ScrollBarThickness" 18.
            resource "ScrollBarThumbThickness" 8.
            resource "IconElementThemeHeight" 20.
            resource "IconElementThemeWidth" 20.
        }

    let simple(styleBuilder: 'T StyleBuilder) =
        styleBuilder {
            styleSelector(selector { object typeof<Border> })
            setter Border.CornerRadiusProperty (CornerRadius 5.)            
        }

type Theme<'T when 'T :> IStyleHost>(this: 'T) =
    do
        ThemeBuilder(this, (nameof Navika), (nameof Theme)) {
            defaultTheme Themes.defaultThemeColors
            resources Themes.resources
            style Themes.simple
        }

module Load =
    open System.Xml.Linq

    type Property = {
        Name: string
        Value: string
    }

    type Resource = {
        Key: string
        Attributes: Property ResizeArray
    } with

        override this.ToString() =
            sprintf
                $"""Key: {this.Key + "\n"}{this.Attributes
                                           |> Seq.fold (fun state value -> state + $"{value.Name}: {value.Value}\n") ""}"""

    let getResources(path: string) =
        let document = XDocument.Load path
        let elements = document.Root.Elements() |> Seq.skip 1

        elements
        |> Seq.map(fun element ->
            if element.IsEmpty then
                element.Attributes()
                |> Seq.fold
                    (fun state attribute ->
                        match attribute.Name.LocalName with
                        | "Key" -> { state with Key = attribute.Value }
                        | _ ->
                            state.Attributes.Add(
                                {
                                    Name = attribute.Name.LocalName
                                    Value = attribute.Value
                                }
                            )

                            state)
                    {
                        Key = ""
                        Attributes = ResizeArray()
                    }
            else
                let list = ResizeArray()

                list.Add(
                    {
                        Name = element.Name.LocalName
                        Value = element.Value
                    }
                )

                {
                    Key = (element.Attributes() |> Seq.head).Value
                    Attributes = list
                })
        |> Seq.iter(fun resource -> printfn $"%s{resource.ToString()}")

// elements
// |> Seq.iter(fun element ->
//     if element.IsEmpty then
//         for attribute in element.Attributes() do
//             match attribute.Name.LocalName with
//             | "Key" -> printf $"{attribute.Value}"
//             | _ -> printf $" {attribute.Name.LocalName} {attribute.Value}"

//         printfn ""
//     else
//         printfn $"{(element.Attributes() |> Seq.head).Value} {element.Name.LocalName} {element.Value}")

// getResources @"/var/home/karthikkselvan/drives/development/FSharp/Nava/Source/TestBed/Theme/Accents/Base.axaml"