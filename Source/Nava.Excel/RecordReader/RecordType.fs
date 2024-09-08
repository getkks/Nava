namespace Nava.Excel.BinaryFormat

type CellType =
    | Boolean
    | Date
    | Error
    | Numeric
    | SharedString
    | String

namespace Nava.Excel.BinaryFormat.Xls

type BOFType =
    | WorkbookGlobals = 0x0005
    | VisualBasicModule = 0x0006
    | Worksheet = 0x0010
    | Chart = 0x0020
    | Biff4MacroSheet = 0x0040
    | Biff4WorkbookGlobals = 0x0100

type RecordType =
    | Dimension = 0x0200
    | YearEpoch = 0x022
    | Blank = 0x0201
    | Number = 0x0203
    | Label = 0x0204
    | BoolErr = 0x0205
    | Formula = 0x0006
    | String = 0x0207
    | BOF = 0x0809
    | Continue = 0x003c
    | CRN = 0x005a
    | LabelSST = 0x00fd
    | RK = 0x027e
    | MulRK = 0x00BD
    | EOF = 0x000A
    | XF = 0x00e0
    | Font = 0x0031
    | ExtSst = 0x00ff
    | Format = 0x041e
    | Style = 0x0293
    | Row = 0x0208
    | ExternSheet = 0x0017
    | DefinedName = 0x0018
    | Country = 0x008c
    | Index = 0x020B
    | CalcCount = 0x000c
    | CalcMode = 0x000d
    | Precision = 0x000e
    | RefMode = 0x000f
    | Delta = 0x0010
    | Iteration = 0x0011
    | Protect = 0x0012
    | Password = 0x0013
    | Header = 0x0014
    | Footer = 0x0015
    | ExternCount = 0x0016
    | Guts = 0x0080
    | SheetPr = 0x0081
    | GridSet = 0x0082
    | HCenter = 0x0083
    | VCenter = 0x0084
    | Sheet = 0x0085
    | WriteProt = 0x0086
    | Sort = 0x0090
    | ColInfo = 0x007d
    | Sst = 0x00fc
    | MulBlank = 0x00be
    | RString = 0x00d6
    | Array = 0x0221
    | SharedFmla = 0x04bc
    | DataTable = 0x0236
    | DBCell = 0x00d7

namespace Nava.Excel.BinaryFormat.Xlsb

type RecordType =
    | None = -1
    | Row = 0
    | CellBlank = 1
    | CellRK = 2
    | CellError = 3
    | CellBool = 4
    | CellReal = 5
    | CellSt = 6
    | CellIsst = 7
    | CellFmlaString = 8
    | CellFmlaNum = 9
    | CellFmlaBool = 10
    | CellFmlaError = 11
    | SSTItem = 19
    | Fmt = 44
    | XF = 47
    | BundleBegin = 143
    | BundleEnd = 144
    | BundleSheet = 156
    | BookBegin = 131
    | BookEnd = 132
    | Dimension = 148
    | SSTBegin = 159
    | SSTEnd = 160
    | StyleBegin = 278
    | StyleEnd = 279
    | CellXFStart = 617
    | CellXFEnd = 618
    | FontsStart = 611
    | FontsEnd = 612
    | Font = 43
    | FillsStart = 603
    | FillsEnd = 604
    | Fill = 45
    | BordersStart = 613
    | BordersEnd = 614
    | Border = 46
    | StyleXFsStart = 626
    | StyleXFsEnd = 627
    | FmtStart = 615
    | FmtEnd = 616
    | SheetStart = 129
    | SheetEnd = 130
    | DataStart = 145
    | DataEnd = 146
    | WsViewsStart = 133
    | WsViewsEnd = 134
    | WsViewStart = 137
    | WsViewEnd = 138
    | Pane = 151
    | Selection = 152
    | WbProp = 153
    | ColInfoStart = 390
    | ColInfoEnd = 391
    | ColInfo = 60
    | FilterStart = 161
    | FilterEnd = 162
    | BeginIgnoreError = 648
    | IgnoreError = 649
    | EndIgnoreError = 650