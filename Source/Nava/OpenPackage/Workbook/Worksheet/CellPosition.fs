namespace Nava.Excel.Common

#nowarn "3391"

open System
open System.Collections.Frozen
open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.IO.Compression
open System.Xml

open Nava.Runtime.Collections
open Nava.Runtime.Xml
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result
open InlineIL

module CellPosition =
    [<TailCall>]
    let rec internal parseReference index column (str: char ReadOnlySpan) =
        if index < str.Length then
            let digit =
                uint(str[index])
                - uint('A')
                + 1u

            if digit <= 26u then
                parseReference (index + 1) (column * 26u + digit) str
            else
                struct (index, column)
        else
            struct (index, column)

    let parseReferenceIL index column (str: char ReadOnlySpan) =
        let mutable column = 0u
        let mutable index = 0

        while index < str.Length
              && (let digit =
                      uint(str[index])
                      - uint('A')
                      + 1u in

                  if Char.IsLetter str[index] then
                      column <- column * 26u + digit
                      index <- index + 1
                      true
                  else
                      false) do
            ()

        struct (index, column)

    let parseColumnReferenceSpan str = parseReferenceIL 0 0u str

    let parseColumnReference(str: string) =
        let struct (_, column) = parseColumnReferenceSpan(str.AsSpan())
        column

    let columnToString column =
        let digitCount =
            if column < 27u then
                1
            elif column < 703u then
                2
            else
                3

        String.Create(
            digitCount,
            column,
            (fun char column ->
                let mutable column = column
                let mutable index = char.Length - 1

                while index >= 0
                      && column > 0u do
                    let modulo = (column - 1u) % 26u
                    char[index] <- Convert.ToChar(uint('A') + modulo)

                    column <-
                        (column - modulo)
                        / 26u

                    index <- index - 1)
        )

    [<DebuggerDisplay("{ToString(),nq}"); Struct>]
    type CellPosition = {
        Column: uint
        Row: uint
    } with

        override x.ToString() =
            (columnToString x.Column)
            + x.Row.ToString()

    let ParseReadOnly(str: _ ReadOnlySpan) =
        let struct (index, column) = parseColumnReferenceSpan str

        {
            Column = column
            Row = UInt32.Parse(str.Slice(index))
        }

    let Parse(str: char Span) = ParseReadOnly(str)