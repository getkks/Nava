namespace Navika.Controls

open System
open System.ComponentModel
open System.Runtime.CompilerServices
open Avalonia
open Avalonia.Data
open Avalonia.Controls
open Avalonia.Controls.Templates
open Avalonia.Layout

open Avalonia.FuncUI.Types
open Avalonia.FuncUI.Builder
open Avalonia.FuncUI.DSL

[<AutoOpen>]
module rec AutoGrid =

    [<Extension>]
    type DependencyExtensions =
        /// <summary>Sets the value of the <paramref name="property"/> only if it hasn't been explicitly set.</summary>
        [<Extension>]
        static member SetIfDefault(o: AvaloniaObject, property: AvaloniaProperty, value: obj) =
            let diag = Diagnostics.AvaloniaObjectExtensions.GetDiagnostic(o, property)

            match diag.Priority with
            | BindingPriority.Unset ->
                o.SetValue(property, value) |> ignore
                true
            | _ -> false

    let ChildHorizontalAlignmentProperty = AvaloniaProperty.Register<AutoGrid, HorizontalAlignment Nullable>("ChildHorizontalAlignment")
    let ChildMarginProperty = AvaloniaProperty.Register<AutoGrid, Thickness Nullable>("ChildMargin")
    let ChildVerticalAlignmentProperty = AvaloniaProperty.Register<AutoGrid, VerticalAlignment Nullable>("ChildVerticalAlignment")
    let ColumnCountProperty = AvaloniaProperty.RegisterAttached<Control, int>("ColumnCount", typeof<AutoGrid>, 1)
    let ColumnWidthProperty = AvaloniaProperty.RegisterAttached<Control, GridLength>("ColumnWidth", typeof<AutoGrid>, GridLength.Auto)
    let IsAutoIndexingProperty = AvaloniaProperty.Register<AutoGrid, bool>("IsAutoIndexing", true)
    let OrientationProperty = AvaloniaProperty.Register<AutoGrid, Orientation>("Orientation")
    let RowCountProperty = AvaloniaProperty.RegisterAttached<Control, int>("RowCount", typeof<AutoGrid>, 1)
    let RowHeightProperty = AvaloniaProperty.RegisterAttached<Control, GridLength>("RowHeight", typeof<AutoGrid>, GridLength.Auto)

    let nullablePropertyValue(propertyValue: 'T Nullable) =
        if propertyValue.HasValue then
            propertyValue.Value |> box
        else
            AvaloniaProperty.UnsetValue

    let inline onChildLinkedPropertyChanged
        (eventArgument: AvaloniaPropertyChangedEventArgs)
        (property: 'T StyledProperty)
        ([<InlineIfLambda>] getValue: AutoGrid -> 'T Nullable)
        =
        match eventArgument.Sender with
        | :? AutoGrid as grid ->
            let value = grid |> getValue |> nullablePropertyValue
            let mutable i = 0

            while i < grid.Children.Count do
                grid.Children[i].SetValue(property, value) |> ignore
                i <- i + 1
        | _ -> ()

    let inline clamp(value, max) = if value > max then max else value

    /// <summary>Defines a flexible grid area that consists of columns and rows. Depending on the orientation, either the rows or the columns are auto-generated, and the children's position is set according to their index.</summary>
    /// <remarks>Based on <see href="https://github.com/AvaloniaUI/AvaloniaAutoGrid">AvaloniaAutoGrid</see>.</remarks>
    type AutoGrid() =
        inherit Grid()

        static do
            AutoGrid.AffectsMeasure<AutoGrid>(
                ChildHorizontalAlignmentProperty,
                ChildMarginProperty,
                ChildVerticalAlignmentProperty,
                ColumnCountProperty,
                ColumnWidthProperty,
                IsAutoIndexingProperty,
                OrientationProperty,
                RowHeightProperty
            )

            AutoGrid.OnChildHorizontalAlignmentChanged
            |> ChildHorizontalAlignmentProperty.Changed.Subscribe
            |> ignore

            AutoGrid.OnChildVerticalAlignmentChanged
            |> ChildVerticalAlignmentProperty.Changed.Subscribe
            |> ignore

            AutoGrid.OnChildMarginChanged |> ChildMarginProperty.Changed.Subscribe |> ignore
            AutoGrid.RowCountChanged |> RowCountProperty.Changed.Subscribe |> ignore
            AutoGrid.ColumnCountChanged |> ColumnCountProperty.Changed.Subscribe |> ignore
            AutoGrid.FixedRowHeightChanged |> RowHeightProperty.Changed.Subscribe |> ignore

            AutoGrid.FixedColumnWidthChanged
            |> ColumnWidthProperty.Changed.Subscribe
            |> ignore

        /// <summary>Gets or sets the child horizontal alignment.</summary>
        /// <value>The child horizontal alignment.</value>
        [<Category("Layout"); Description("Presets the horizontal alignment of all child controls")>]
        member _.ChildHorizontalAlignment
            with get () = base.GetValue(ChildHorizontalAlignmentProperty)
            and set value = base.SetValue(ChildHorizontalAlignmentProperty, value) |> ignore

        /// <summary>Gets or sets the child margin.</summary>
        /// <value>The child margin.</value>
        [<Category("Layout"); Description("Presets the margin of all child controls")>]
        member _.ChildMargin
            with get () = base.GetValue(ChildMarginProperty)
            and set value = base.SetValue(ChildMarginProperty, value) |> ignore

        /// <summary>Gets or sets the child vertical alignment.</summary>
        /// <value>The child vertical alignment.</value>
        [<Category("Layout"); Description("Presets the vertical alignment of all child controls")>]
        member _.ChildVerticalAlignment
            with get () = base.GetValue(ChildVerticalAlignmentProperty)
            and set value = base.SetValue(ChildVerticalAlignmentProperty, value) |> ignore

        /// <summary>
        /// Gets or sets the column count
        /// </summary>
        [<Category("Layout"); Description("Defines a set number of columns")>]
        member _.ColumnCount
            with get () = base.GetValue(ColumnCountProperty)
            and set value = base.SetValue(ColumnCountProperty, value) |> ignore

        /// <summary>
        /// Gets or sets the fixed column width
        /// </summary>
        [<Category("Layout"); Description("Presets the width of all columns set using the ColumnCount property")>]
        member _.ColumnWidth
            with get () = base.GetValue(ColumnWidthProperty)
            and set value = base.SetValue(ColumnWidthProperty, value) |> ignore

        /// <summary>Gets or sets a value indicating whether the children are automatically indexed.</summary>
        /// <remarks>
        /// The default is <c>true</c>.
        /// Note that if children are already indexed, setting this property to <c>false</c> will not remove their indices.
        /// </remarks>
        [<Category("Layout"); Description("Set to false to disable auto layout functionality")>]
        member _.IsAutoIndexing
            with get () = base.GetValue(IsAutoIndexingProperty)
            and set value = base.SetValue(IsAutoIndexingProperty, value) |> ignore

        /// <summary>Gets or sets the orientation.</summary>
        /// <value>The orientation.</value>
        /// <remarks>The default is Vertical.</remarks>
        [<Category("Layout");
          Description("Defines the directionality of auto layout. Use vertical for a column first layout, horizontal for a row first layout.")>]
        member _.Orientation
            with get () = base.GetValue(OrientationProperty)
            and set value = base.SetValue(OrientationProperty, value) |> ignore

        /// <summary>Gets or sets the number of rows</summary>
        [<Category("Layout"); Description("Defines a set number of rows")>]
        member val RowCount: int = 0 with get, set

        /// <summary>Gets or sets the fixed row height</summary>
        [<Category("Layout"); Description("Presets the height of all rows set using the RowCount property")>]
        member val RowHeight: GridLength = GridLength.Auto with get, set

        /// <summary>Handles the column count changed event</summary>
        static member ColumnCountChanged(eventArgument: AvaloniaPropertyChangedEventArgs) =
            let value = eventArgument.NewValue :?> int

            match eventArgument.Sender with
            | :? AutoGrid as grid when value >= 0 ->
                // look for an existing column definition for the height
                let width =
                    if grid.IsSet ColumnWidthProperty || grid.ColumnDefinitions.Count <= 0 then
                        grid.ColumnWidth
                    else
                        grid.ColumnDefinitions[0].Width

                // clear and rebuild
                grid.ColumnDefinitions.Clear()
                let mutable i = 0

                while i < value do
                    width |> ColumnDefinition |> grid.ColumnDefinitions.Add
                    i <- i + 1
            | _ -> ()

        /// <summary>
        /// Handles the row count changed event
        /// </summary>
        static member RowCountChanged(eventArgument: AvaloniaPropertyChangedEventArgs) =
            let value = eventArgument.NewValue :?> int

            match eventArgument.Sender with
            | :? AutoGrid as grid when value >= 0 ->
                // look for an existing column definition for the height
                let height =
                    if grid.IsSet RowHeightProperty || grid.RowDefinitions.Count <= 0 then
                        grid.RowHeight
                    else
                        grid.RowDefinitions[0].Height

                // clear and rebuild
                grid.RowDefinitions.Clear()
                let mutable i = 0

                while i < value do
                    height |> RowDefinition |> grid.RowDefinitions.Add
                    i <- i + 1
            | _ -> ()

        /// <summary>Handle the fixed column width changed event</summary>
        static member FixedColumnWidthChanged(eventArgument: AvaloniaPropertyChangedEventArgs) =
            let value = eventArgument.NewValue :?> GridLength

            match eventArgument.Sender with
            | :? AutoGrid as grid ->
                // add a default column if missing
                if grid.ColumnDefinitions.Count = 0 then
                    ColumnDefinition() |> grid.ColumnDefinitions.Add

                let definitions = grid.ColumnDefinitions
                let count = definitions.Count
                let mutable i = 0

                while i < count do
                    definitions[i].Width <- value
                    i <- i + 1
            | _ -> ()

        /// <summary>Handle the fixed row height changed event</summary>
        static member FixedRowHeightChanged(eventArgument: AvaloniaPropertyChangedEventArgs) =
            let value = eventArgument.NewValue :?> GridLength

            match eventArgument.Sender with
            | :? AutoGrid as grid ->
                // add a default row if missing
                if grid.RowDefinitions.Count = 0 then
                    RowDefinition() |> grid.RowDefinitions.Add

                let definitions = grid.RowDefinitions
                let count = definitions.Count
                let mutable i = 0

                while i < count do
                    definitions[i].Height <- value
                    i <- i + 1
            | _ -> ()

        /// <summary>Called when [child horizontal alignment changed].</summary>
        static member OnChildHorizontalAlignmentChanged(eventArgument: AvaloniaPropertyChangedEventArgs) =
            onChildLinkedPropertyChanged eventArgument Layoutable.HorizontalAlignmentProperty _.ChildHorizontalAlignment

        /// <summary>Called when [child layout changed].</summary>
        static member OnChildMarginChanged(eventArgument: AvaloniaPropertyChangedEventArgs) =
            onChildLinkedPropertyChanged eventArgument Layoutable.MarginProperty _.ChildMargin

        /// <summary>Called when [child vertical alignment changed].</summary>
        static member OnChildVerticalAlignmentChanged(eventArgument: AvaloniaPropertyChangedEventArgs) =
            onChildLinkedPropertyChanged eventArgument Layoutable.VerticalAlignmentProperty _.ChildVerticalAlignment

        /// <summary>Apply child margins and layout effects such as alignment</summary>
        member internal this.ApplyChildLayout(child: Control) =
            if this.ChildMargin.HasValue then
                child.SetIfDefault(Layoutable.MarginProperty, this.ChildMargin.Value) |> ignore

            if this.ChildHorizontalAlignment.HasValue then
                child.SetIfDefault(Layoutable.HorizontalAlignmentProperty, this.ChildHorizontalAlignment.Value)
                |> ignore

            if this.ChildVerticalAlignment.HasValue then
                child.SetIfDefault(Layoutable.VerticalAlignmentProperty, this.ChildVerticalAlignment.Value)
                |> ignore

        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member internal _.PrepareChild(child: Control, colCount, rowCount, fillRowFirst, skip: bool array2d, position: _ byref) =
            let mutable row, col =
                if fillRowFirst then
                    clamp(position / colCount, rowCount - 1), clamp(position % colCount, colCount - 1)
                else
                    clamp(position % colCount, rowCount - 1), clamp(position / colCount, colCount - 1)

            if skip[row, col] then
                position <- position + 1

                if fillRowFirst then
                    row <- position / colCount
                    col <- position % colCount
                else
                    row <- position % colCount
                    col <- position / colCount

            AutoGrid.SetRow(child, row)
            AutoGrid.SetColumn(child, col)

            position <-
                position
                + if fillRowFirst then
                      AutoGrid.GetColumnSpan child
                  else
                      AutoGrid.GetRowSpan child

            let mutable offset =
                if fillRowFirst then
                    AutoGrid.GetRowSpan child
                else
                    AutoGrid.GetColumnSpan child
                - 1

            while offset > 0 do
                skip[row + offset, col] <- true
                offset <- offset - 1

        /// <summary>Perform the grid layout of row and column indexes </summary>
        member internal this.PerformLayout() =
            let fillRowFirst = this.Orientation = Orientation.Horizontal
            let rowCount = this.RowDefinitions.Count
            let colCount = this.ColumnDefinitions.Count

            if rowCount <> 0 && colCount <> 0 && rowCount * colCount >= this.Children.Count then
                let mutable position = 0
                let skip = Array2D.zeroCreate rowCount colCount

                for child in this.Children do
                    this.PrepareChild(child, colCount, rowCount, fillRowFirst, skip, &position)
                    this.ApplyChildLayout(child)

        override this.MeasureOverride(constraintSize: Size) : Size =
            this.PerformLayout()
            base.MeasureOverride constraintSize

        static member childHorizontalAlignment<'T when 'T :> AutoGrid>(alignment: HorizontalAlignment Nullable) : 'T IAttr =
            AttrBuilder.CreateProperty(ChildHorizontalAlignmentProperty, alignment, ValueNone)

        static member childHorizontalAlignment<'T when 'T :> AutoGrid>(alignment: HorizontalAlignment) : 'T IAttr =
            alignment |> Nullable |> AutoGrid.childHorizontalAlignment

        static member childHorizontalAlignment<'T when 'T :> AutoGrid>(alignment: HorizontalAlignment option) : 'T IAttr =
            alignment |> Option.toNullable |> AutoGrid.childHorizontalAlignment

        static member childHorizontalAlignment<'T when 'T :> AutoGrid>(alignment: HorizontalAlignment voption) : 'T IAttr =
            alignment |> ValueOption.toNullable |> AutoGrid.childHorizontalAlignment

        static member childVerticalAlignment<'T when 'T :> AutoGrid>(alignment: VerticalAlignment Nullable) : 'T IAttr =
            AttrBuilder.CreateProperty(ChildVerticalAlignmentProperty, alignment, ValueNone)

        static member childVerticalAlignment<'T when 'T :> AutoGrid>(alignment: VerticalAlignment) : 'T IAttr =
            alignment |> Nullable |> AutoGrid.childVerticalAlignment

        static member childVerticalAlignment<'T when 'T :> AutoGrid>(alignment: VerticalAlignment option) : 'T IAttr =
            alignment |> Option.toNullable |> AutoGrid.childVerticalAlignment

        static member childVerticalAlignment<'T when 'T :> AutoGrid>(alignment: VerticalAlignment voption) : 'T IAttr =
            alignment |> ValueOption.toNullable |> AutoGrid.childVerticalAlignment

        static member childMargin<'T when 'T :> AutoGrid>(margin: Thickness Nullable) : 'T IAttr =
            AttrBuilder.CreateProperty(ChildMarginProperty, margin, ValueNone)

        static member childMargin<'T when 'T :> AutoGrid>(margin: Thickness) : 'T IAttr =
            margin |> Nullable |> AutoGrid.childMargin

        static member childMargin<'T when 'T :> AutoGrid>(margin: Thickness option) : 'T IAttr =
            margin |> Option.toNullable |> AutoGrid.childMargin

        static member childMargin<'T when 'T :> AutoGrid>(margin: Thickness voption) : 'T IAttr =
            margin |> ValueOption.toNullable |> AutoGrid.childMargin

        static member columnCount<'T when 'T :> AutoGrid>(columnCount: int) : 'T IAttr =
            AttrBuilder.CreateProperty(ColumnCountProperty, columnCount, ValueNone)

        static member columnWidth<'T when 'T :> AutoGrid>(columnWidth: GridLength) : 'T IAttr =
            AttrBuilder.CreateProperty(ColumnWidthProperty, columnWidth, ValueNone)

        static member columnWidth<'T when 'T :> AutoGrid>(columnWidth: float) : 'T IAttr =
            columnWidth |> GridLength |> AutoGrid.columnWidth

        static member columnWidth<'T when 'T :> AutoGrid>(columnWidth: float, ``type``: GridUnitType) : 'T IAttr =
            GridLength(columnWidth, ``type``) |> AutoGrid.columnWidth

        static member isAutoIndexing<'T when 'T :> AutoGrid>(autoIndexing: bool) : 'T IAttr =
            AttrBuilder.CreateProperty(IsAutoIndexingProperty, autoIndexing, ValueNone)

        static member rowCount<'T when 'T :> AutoGrid>(orientation: Orientation) : 'T IAttr =
            AttrBuilder.CreateProperty(OrientationProperty, orientation, ValueNone)

        static member rowCount<'T when 'T :> AutoGrid>(rowCount: int) : 'T IAttr =
            AttrBuilder.CreateProperty(RowCountProperty, rowCount, ValueNone)

        static member rowHeight<'T when 'T :> AutoGrid>(rowHeight: GridLength) : 'T IAttr =
            AttrBuilder.CreateProperty(RowHeightProperty, rowHeight, ValueNone)

        static member rowHeight<'T when 'T :> AutoGrid>(rowHeight: float) : 'T IAttr =
            rowHeight |> GridLength |> AutoGrid.rowHeight

        static member rowHeight<'T when 'T :> AutoGrid>(rowHeight: float, ``type``: GridUnitType) : 'T IAttr =
            GridLength(rowHeight, ``type``) |> AutoGrid.rowHeight

    let create(attrs: AutoGrid IAttr list) = ViewBuilder.Create attrs