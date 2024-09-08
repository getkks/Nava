namespace Nava.Office

open System.Collections.Generic
open System.Linq
open Microsoft.Office.Interop.Excel

module rec Workbook =
    type Workbook with
        /// <summary> Get worksheets collection from the workbook after casting it to <see cref="Worksheet" /> type. </summary>
        /// <returns> The worksheets collection. </returns>
        member this.SheetCollection = this.Worksheets |> Enumerable.Cast<Worksheet>

    /// <summary> Get active worksheet from the workbook. </summary>
    /// <param name="workbook"> The workbook. </param>
    let activeSheet(workbook: Workbook) = workbook.ActiveSheet :?> Worksheet
    /// <summary> Get worksheets collection from the workbook. </summary>
    /// <param name="workbook"> The workbook. </param>
    let worksheets(workbook: Workbook) = workbook.SheetCollection
    /// <summary> Map worksheets collection from the workbook. </summary>
    /// <param name="map"> Map function. </param>
    /// <param name="workbook"> The workbook. </param>
    let mapSheets map workbook = workbook |> worksheets |> Seq.map map