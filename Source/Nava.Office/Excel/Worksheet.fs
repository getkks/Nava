namespace Nava.Office

open System.Linq
open Microsoft.Office.Interop.Excel

module rec Worksheet =
    type Worksheet with

        member this.Range = this.UsedRange

    let range index (worksheet: Worksheet) = worksheet.Range index