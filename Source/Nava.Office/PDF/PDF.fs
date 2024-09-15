namespace Nava.Office.PDF

open System.IO
open SkiaSharp
open PDFtoImage

module Conversion =
    type Sized<'T when 'T: (member Width: int) and 'T: (member Height: int)> = 'T
    type ImageExtractSubset<'T when 'T: (member ExtractSubset: 'T -> SKRectI -> bool) and 'T: (new: unit -> 'T)> = 'T
    type ImageEncode<'T when 'T: (member Encode: Stream * SKEncodedImageFormat * int -> bool)> = 'T

    let inline cropBitmap rect (bitmap: 'T ImageExtractSubset) =
        // use pixmap = SKPixmap(bitmap.Info, bitmap.GetPixels())
        // rect |> pixmap.ExtractSubset |> ValueOption.ofObj
        let result = new 'T()

        if bitmap.ExtractSubset(result, rect) then
            ValueSome result
        else
            ValueNone

    type Crop =
        | LeftTo of int
        | LeftBy of float32
        | RightTo of int
        | RightBy of float32
        | TopTo of int
        | TopBy of float32
        | BottomTo of int
        | BottomBy of float32
        | AllTo of int
        | AllBy of float32
        | Crop of SKRectI

        member this.Rect(original: SKRectI byref) =
            match this with
            | LeftTo value -> original.Left <- value
            | LeftBy factor -> original.Left <- float32 original.Width * factor |> int
            | RightTo value -> original.Right <- value
            | RightBy factor -> original.Right <- float32 original.Width * factor |> int
            | TopTo value -> original.Top <- value
            | TopBy factor -> original.Top <- float32 original.Height * factor |> int
            | BottomTo value -> original.Bottom <- value
            | BottomBy factor -> original.Bottom <- float32 original.Height * factor |> int
            | AllTo value ->
                original.Left <- value
                original.Right <- value
            | AllBy factor ->
                original.Left <- float32 original.Width * factor |> int
                original.Right <- float32 original.Width * factor |> int
            | Crop rect -> original <- rect

        member inline this.Image(bitmap: 'T Sized) : 'T voption =
            let mutable rect = SKRectI(0, 0, bitmap.Width, bitmap.Height)
            this.Rect &rect
            cropBitmap rect bitmap

    type Options = {
        Crop: Crop voption
        DPI: int
        Format: SKEncodedImageFormat
        Extension: string
        Input: FileInfo
        OutputFolder: DirectoryInfo
        Quality: int
    }

    module Options =

        let imageFormatToExtension(format: SKEncodedImageFormat) = "." + format.ToString().ToLower()

        let create fileName =
            if File.Exists fileName then
                {
                    Crop = ValueNone
                    DPI = 72
                    Format = SKEncodedImageFormat.Png
                    Extension = imageFormatToExtension SKEncodedImageFormat.Png
                    Input = FileInfo fileName
                    OutputFolder = fileName |> Path.GetDirectoryName |> DirectoryInfo
                    Quality = 100
                }
            else
                $"The system cannot find the file specified.\nFile: '{fileName}'"
                |> FileNotFoundException
                |> raise

        let withCrop crop options = { options with Crop = ValueSome crop }
        let withDPI dpi options = { options with DPI = dpi }

        let withFormat format options = {
            options with
                Format = format
                Extension = imageFormatToExtension format
        }

        let withOutput folder options = {
            options with
                OutputFolder =
                    if Directory.Exists folder then
                        DirectoryInfo folder
                    else
                        $"The system cannot find the folder specified.\nFolder: '{folder}'"
                        |> DirectoryNotFoundException
                        |> raise
        }

        let withQuality quality options = { options with Quality = quality }

    let inline encode output options (image: 'T ImageEncode) =
        image.Encode(output, options.Format, options.Quality) |> ignore

    let processPage (options: Options) (index: int64) (page: SKBitmap) =
        let outputFile = Path.Combine(options.OutputFolder.FullName, index.ToString() + options.Extension)
        use output = new FileStream(outputFile, FileMode.Create)

        options.Crop
        |> ValueOption.bind(fun crop -> crop.Image page)
        |> ValueOption.defaultValue page
        |> encode output options

        outputFile

    open FSharp.Control

    let mapiAsyncParallel (mapping: int64 -> 'T -> Async<'U>) (sequence: AsyncSeq<'T>) : AsyncSeq<'U> =
        sequence
        |> AsyncSeq.mapi(fun i x -> struct (i, x))
        |> AsyncSeq.mapAsyncParallel(fun struct (i, x) -> mapping i x)

    let toImages options =
        use input = options.Input.OpenRead()
        let renderOptions = RenderOptions(Dpi = options.DPI)

        Conversion.ToImagesAsync(input, options = renderOptions)
        |> AsyncSeq.ofAsyncEnum
        |> mapiAsyncParallel(fun i page -> async { return processPage options i page })
        |> AsyncSeq.toArrayAsync
        |> Async.RunSynchronously