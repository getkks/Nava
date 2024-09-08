namespace Nava.Excel

#nowarn "3391"

open System
open System.Collections.Generic
open System.IO
open System.IO.Compression
open System.Xml

open Nava.Runtime.Collections
open Nava.Runtime.Xml
open Nava.Runtime.Zip
open FsToolkit.ErrorHandling
open InlineIL
open StringBuffer
open Zio

type OpenOptions =
    val mutable DisposeStream: bool
    new() = { DisposeStream = true }

type OpenPackage(zipPackage: ZipPackage) =
    inherit OpenPart(String.Empty, UPath.Empty, zipPackage)
    new(stream: Stream, options: OpenOptions) = OpenPackage(ZipPackage(options.DisposeStream, stream))

    member val Workbook =
        let relation =
            (base.Relations.Values
             |> Seq.tryFind(fun relation -> RelationType.Workbook = relation.Type))
                .Value

        Workbook(relation.Id, relation.Target, base.OpenPart)

    override this.ToString() =
        base.ToString(stringBuffer { "Workbook: " + this.Workbook.ToString() })