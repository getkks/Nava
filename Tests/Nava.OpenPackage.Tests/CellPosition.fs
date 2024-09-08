namespace Nava.OpenPackage

#nowarn "3391"

open System
open Xunit
open FsUnit.Xunit
open FsCheck

open FsCheck.FSharp

open FsCheck.Xunit
open Nava.Excel.Common

module ``Cell Position Tests`` =
    [<Property>]
    let ``Column ToString`` column =
        if column > 0u then
            if column < 27u then
                Convert
                    .ToChar(
                        column + uint('A')
                        - 1u
                    )
                    .ToString() = CellPosition.columnToString column
            else
                true
        else
            true

    [<Property>]
    let ``Conversion to and from Cell Position should match`` column row =

        if
            column > 0u
            && row > 0u
        then
            let position: CellPosition.CellPosition = {
                Column = column
                Row = row
            }

            CellPosition.ParseReadOnly(position.ToString()) = position
        else
            true

    [<Property>]
    let ``Row ToString`` row =
        if row > 0u then
            let position: CellPosition.CellPosition = {
                Column = 1u
                Row = row
            }

            position.ToString() = "A" + row.ToString()
        else
            true