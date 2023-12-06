// Author: Alexander Korn
// Created on: 5. December 2023

// Script asks for and adds the following items:
// 1. Add a new Calculation Group for "Time Intelligence" Measures
// 2. Adding a Date Dimension Table
// 3. Adding an Empty Measure Table
// 4. Adding a Last Refresh Table
// 5. Formats the DAX of ALL calculation items in the model
// Variables to fill in:
//     - Calc Group Name,
//     - Date Table Name
//     - Date Column Name
//     - If YTD / FY measures shall be created and what the cutoff date for the FY is


#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;
using System.Windows.Forms; 
DialogResult dialogResult = MessageBox.Show(text:"Important Disclaimer: You are executing this script at your own risk!\n\n Generate Empty Measure table?\n\nCAUTION: This only works if you are not directly connected to a local PBI Desktop model, such as modifying a locally saved .bim file", caption:"Empty Measure Table", buttons:MessageBoxButtons.YesNo);
bool GenerateEmptyMeasureTable = (dialogResult == DialogResult.Yes);

DialogResult dialogResult1 = MessageBox.Show(text:"Generate Last Refresh table?\n\nCAUTION: This only works if you are not directly connected to a local PBI Desktop model, such as modifying a locally saved .bim file", caption:"Last Refresh Table", buttons:MessageBoxButtons.YesNo);
bool GenerateLastRefreshTable = (dialogResult1 == DialogResult.Yes); 

DialogResult dialogResult2 = MessageBox.Show(text:"Generate Date Dimension table? Basis is a Power Query Script\n\nCAUTION: This only works if you are not directly connected to a local PBI Desktop model, such as modifying a locally saved .bim file", caption:"Date Dimension Table", buttons:MessageBoxButtons.YesNo);
bool GenerateDateDimensionTable = (dialogResult2 == DialogResult.Yes); 

DialogResult dialogResult0 = MessageBox.Show(text:"Generate Time Intelligence Calc Group?\n\n This works also for PBI Desktop models", caption:"Time Intelligence Table", buttons:MessageBoxButtons.YesNo);
bool GenerateCalcGroupTimeInt = (dialogResult0 == DialogResult.Yes); 

bool waitCursor = Application.UseWaitCursor;
Application.UseWaitCursor = false;
var Table = Interaction.InputBox("Provide the name of the date dimension table","table name","Date");
var Column = Interaction.InputBox("Provide the name of the date column name","column","CalendarDate");

if (GenerateCalcGroupTimeInt)
    {
var CalculationGroupName = Interaction.InputBox("Provide the name of the calculation group name","Calc group","Time Intelligence");

DialogResult dialogResult3 = MessageBox.Show(text:"Generate Fiscal Year Calc Items?", caption:"Calc Group: Fiscal Year", buttons:MessageBoxButtons.YesNo);
bool GenerateFiscalYear = (dialogResult3 == DialogResult.Yes);

string fiscalYearEndDate = "07/31"; // Default value in case the input box is not shown
if (GenerateFiscalYear)
{fiscalYearEndDate = Interaction.InputBox("Enter the fiscal year end date (MM/DD):", "Fiscal Year End Date", fiscalYearEndDate);}

DialogResult dialogResult4 = MessageBox.Show(text:"Generate YTD Calc Items?", caption:"Calc Group: YTD", buttons:MessageBoxButtons.YesNo);
bool GenerateYTD = (dialogResult4 == DialogResult.Yes);

DialogResult dialogResult5 = MessageBox.Show(text:"Format ALL calculation items for the calculation groups", caption:"Format DAX calc items", buttons:MessageBoxButtons.YesNo);
bool FormatDAXCalcItems = (dialogResult5 == DialogResult.Yes); 

DialogResult dialogResult6 = MessageBox.Show(text:"Format ALL DAX measures", caption:"Format ALL measures", buttons:MessageBoxButtons.YesNo);
bool FormatDAX = (dialogResult6 == DialogResult.Yes); 


            

// Add a new Time Intellignce Calculation Group 
try{
var calcGroup = Model.AddCalculationGroup();
calcGroup.Name = CalculationGroupName; //Name provided through Input Box
// Define calculation item data
var calculationItemData = new[]
{
    new { Name = "AC", Expression = "SELECTEDMEASURE()" },
    new { Name = "Y-1", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -1, YEAR), ALL({0}))", Table, Column) },
    new { Name = "Y-2", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -2, YEAR), ALL({0}))", Table, Column) },
    new { Name = "Y-3", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -3, YEAR), ALL({0}))", Table, Column) },
    GenerateYTD ? new { Name = "YTD", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0}))", Table, Column) }: null,
    GenerateYTD ? new { Name = "YTD-1", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -1, YEAR), ALL({0}))", Table, Column) }: null,
    GenerateYTD ? new { Name = "YTD-2", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -2, YEAR), ALL({0}))", Table, Column) }: null,
    new { Name = "abs. AC vs Y-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -1, YEAR), ALL({0})) RETURN AC - Y1", Table, Column) },
    new { Name = "abs. AC vs Y-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -2, YEAR), ALL({0})) RETURN AC - Y2", Table, Column) },
    GenerateYTD ? new { Name = "abs. AC vs YTD-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -1, YEAR), ALL({0})) RETURN AC - Y1", Table, Column) }: null,
    GenerateYTD ? new { Name = "abs. AC vs YTD-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -2, YEAR), ALL({0})) RETURN AC - Y2", Table, Column) }: null,
    new { Name = "AC vs Y-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), SAMEPERIODLASTYEAR({0}[{1}]), ALL({0})) RETURN DIVIDE(AC - Y1, Y1)", Table, Column) },
    new { Name = "AC vs Y-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD({0}[{1}], -2, YEAR), ALL({0})) RETURN DIVIDE(AC - Y2, Y2)", Table, Column) },
    GenerateYTD ? new { Name = "AC vs YTD-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -1, YEAR), ALL({0})) RETURN DIVIDE(AC - Y1, Y1)", Table, Column) }: null,
    GenerateYTD ? new { Name = "AC vs YTD-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -2, YEAR), ALL({0})) RETURN DIVIDE(AC - Y2, Y2)", Table, Column) }: null,
    new { Name = "achiev. AC vs Y-1", Expression = string.Format("VAR AC = SELECTEDMEASURE() VAR Y1 = CALCULATE(SELECTEDMEASURE(), SAMEPERIODLASTYEAR({0}[{1}]), ALL({0})) RETURN 1 - ( ( IFERROR( ( Y1 - AC ), 0 ) / Y1 ) )", Table, Column) },
    new { Name = "achiev. AC vs Y-2", Expression = string.Format("VAR AC = SELECTEDMEASURE() VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD({0}[{1}], -2, YEAR), ALL({0})) RETURN 1 - ( ( IFERROR( ( Y2 - AC ), 0 ) / Y2 ) )", Table, Column) },
    GenerateYTD ? new { Name = "achiev. AC vs YTD-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -1, YEAR), ALL({0})) RETURN 1 - ( ( IFERROR( ( Y1 - AC ), 0 ) / Y1 ) )", Table, Column) }: null,
    GenerateYTD ? new { Name = "achiev. AC vs YTD-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -2, YEAR), ALL({0})) RETURN 1 - ( ( IFERROR( ( Y2 - AC ), 0 ) / Y2 ) )", Table, Column) }: null,
    GenerateFiscalYear ? new { Name = "FYTD", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0}))", Table, Column, fiscalYearEndDate) } : null,
    GenerateFiscalYear ? new { Name = "FYTD-1", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -1, YEAR), ALL({0}))", Table, Column, fiscalYearEndDate) } : null,
    GenerateFiscalYear ? new { Name = "FYTD-2", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -2, YEAR), ALL({0}))", Table, Column, fiscalYearEndDate) } : null,
    GenerateFiscalYear ? new { Name = "abs. AC vs FYTD-1", Expression = string.Format("VAR AC = CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -1, YEAR), ALL({0})) RETURN AC - Y1", Table, Column, fiscalYearEndDate) } : null,
    GenerateFiscalYear ? new { Name = "abs. AC vs FYTD-2", Expression = string.Format("VAR AC = CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -2, YEAR), ALL({0})) RETURN AC - Y2", Table, Column, fiscalYearEndDate) } : null,
    GenerateFiscalYear ? new { Name = "AC vs FYTD-1", Expression = string.Format("VAR AC = CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -1, YEAR), ALL({0})) RETURN DIVIDE(AC - Y1, Y1)", Table, Column, fiscalYearEndDate) } : null,
    GenerateFiscalYear ? new { Name = "AC vs FYTD-2", Expression = string.Format("VAR AC = CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -2, YEAR), ALL({0})) RETURN DIVIDE(AC - Y2, Y2)", Table, Column, fiscalYearEndDate) } : null,
    GenerateFiscalYear ? new { Name = "achiev. AC vs FYTD-1", Expression = string.Format("VAR AC = CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -1, YEAR), ALL({0})) RETURN 1 - ( ( IFERROR( ( Y1 - AC ), 0 ) / Y1 ) )", Table, Column, fiscalYearEndDate) } : null,
    GenerateFiscalYear ? new { Name = "achiev. AC vs FYTD-2", Expression = string.Format("VAR AC = CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -2, YEAR), ALL({0})) RETURN 1 - ( ( IFERROR( ( Y2 - AC ), 0 ) / Y2 ) )", Table, Column, fiscalYearEndDate) } : null
}.Where(item => item != null).ToArray();
// Add calculation items to the Calculation Group
foreach (var itemData in calculationItemData)
{
    var item = calcGroup.AddCalculationItem();
    item.Name = itemData.Name;
    item.Expression = itemData.Expression;
}


if (FormatDAXCalcItems)
    {
// DAX Formatting all Measures
FormatDax(Model.AllCalculationItems);
}

if (FormatDAX)
    {
// DAX Formatting all Measures
FormatDax(Model.AllMeasures);
}


}
catch (Exception ex)
{MessageBox.Show("Adding the calc group was not fully successful, but the rest of the script was completed\n\nReason: "+ex.Message);
    }
}

// Script adds an empty measure table
if (GenerateEmptyMeasureTable)
    {
        try {
// Define the table name
string tableName = "Measure";
// Create a new table in the model
Table table = Model.AddTable(tableName);
// Add the "Name of Measure" column to the table
DataColumn column1 = table.AddDataColumn();
column1.Name = "Name of Measure";
column1.DataType = DataType.String;
column1.SourceColumn = "Name of Measure";
column1.IsHidden = true; // Hide the column
// Add the "Description" column to the table
DataColumn column2 = table.AddDataColumn();
column2.Name = "Description";
column2.DataType = DataType.String;
column2.SourceColumn = "Description";
column2.IsHidden = true; // Hide the column

if (!Model.Tables.Any(t => t.Name == tableName))
{
    throw new InvalidOperationException("Empty measure table does not exist in the model.");
}

            string mExpression = @"
let
    Source = Table.FromRows(Json.Document(Binary.Decompress(Binary.FromText(""i44FAA=="", BinaryEncoding.Base64), Compression.Deflate)), let _t = ((type nullable text) meta [Serialized.Text = true]) in type table [#""Name of Measure"" = _t, Description = _t])
in
    Source";


    // Update existing partition
var partition = table.Partitions.First();
partition.Expression = mExpression;
        partition.Mode = ModeType.Import; // Set the refresh policy to Import
}
catch (Exception ex)
{MessageBox.Show("Adding Empty Measure table was not successful but the rest of the script was completed\n\nReason: "+ex.Message);
    }
}


if (GenerateLastRefreshTable)
    {
        try {
// Script adds a Last Refresh Table:
// Define the table name
string tableName = "Last Refresh";
// Create a new table in the model
Table table = Model.AddTable(tableName);
// Add the "Column1" column to the table
DataColumn column1 = table.AddDataColumn();
column1.Name = "Column1";
column1.DataType = DataType.String;
column1.SourceColumn = "Column1";
column1.IsHidden = true; // Hide the column
// Check if the table exists in the model
if (!Model.Tables.Any(t => t.Name == tableName))
{
    throw new InvalidOperationException("Table Last Refresh does not exist in the model.");
}
// Define the M expression
string mExpression = @"
let
    #""Today"" = #table({""Last Refresh""}, {{DateTime.From(DateTime.LocalNow())}})
in
    #""Today"" ";
        // Update existing partition
        var partition = table.Partitions.First();
        partition.Expression = mExpression;
        partition.Mode = ModeType.Import; // Set the refresh policy to Import

}
catch (Exception ex)
{MessageBox.Show("Adding the Last Refresh table was not successful but the rest of the script was completed\n\nReason: "+ex.Message);
    }
}


if (GenerateDateDimensionTable)
    {
        try {
// Script adds a Date Dimension Table:
// Create a new table in the model
Table table = Model.AddTable(Table);
// Add columns with specified names and data types, including SourceColumn
DataColumn dateColumn = table.AddDataColumn();
dateColumn.Name = Column;
dateColumn.DataType = DataType.DateTime;
dateColumn.SourceColumn = "Date";
DataColumn yearColumn = table.AddDataColumn();
yearColumn.Name = "Year";
yearColumn.DataType = DataType.Int64;
yearColumn.SourceColumn = "Year";
DataColumn monthColumn = table.AddDataColumn();
monthColumn.Name = "Month";
monthColumn.DataType = DataType.Int64;
monthColumn.SourceColumn = "Month";
DataColumn dayColumn = table.AddDataColumn();
dayColumn.Name = "Day";
dayColumn.DataType = DataType.Int64;
dayColumn.SourceColumn = "Day";
DataColumn dayNameColumn = table.AddDataColumn();
dayNameColumn.Name = "DayName";
dayNameColumn.DataType = DataType.String;
dayNameColumn.SourceColumn = "DayName";
DataColumn monthNameColumn = table.AddDataColumn();
monthNameColumn.Name = "MonthName";
monthNameColumn.DataType = DataType.String;
monthNameColumn.SourceColumn = "MonthName";
DataColumn quarterColumn = table.AddDataColumn();
quarterColumn.Name = "Quarter";
quarterColumn.DataType = DataType.Int64;
quarterColumn.SourceColumn = "Quarter";
DataColumn weekOfYearColumn = table.AddDataColumn();
weekOfYearColumn.Name = "WeekOfYear";
weekOfYearColumn.DataType = DataType.Int64;
weekOfYearColumn.SourceColumn = "WeekOfYear";
DataColumn yearMonthColumn = table.AddDataColumn();
yearMonthColumn.Name = "YearMonth";
yearMonthColumn.DataType = DataType.String;
yearMonthColumn.SourceColumn = "YearMonth";
DataColumn yearMonthCodeColumn = table.AddDataColumn();
yearMonthCodeColumn.Name = "YearMonth Code";
yearMonthCodeColumn.DataType = DataType.String;
yearMonthCodeColumn.SourceColumn = "YearMonth Code";
// Check if the table exists in the model
if (!Model.Tables.Any(t => t.Name == Table))
{
    throw new InvalidOperationException("Table Date Dimension does not exist in the model.");
}
// Define the M expression
string mExpression = @"
let
    // configurations start
    Today=Date.From(DateTime.LocalNow()), // today's date
    FromYear = 2018, // set the start year of the date dimension. dates start from 1st of January of this year
    ToYear=2025, // set the end year of the date dimension. dates end at 31st of December of this year
    StartofFiscalYear=7, // set the month number that is start of the financial year. example; if fiscal year start is July, value is 7
    firstDayofWeek=Day.Monday, // set the week's start day, values: Day.Monday, Day.Sunday....
    // configuration end
    FromDate=#date(FromYear,1,1),
    ToDate=#date(ToYear,12,31),
    Source=List.Dates(
        FromDate,
        Duration.Days(ToDate-FromDate)+1,
        #duration(1,0,0,0)
    ),
    #""Converted to Table"" = Table.FromList(Source, Splitter.SplitByNothing(), null, null, ExtraValues.Error),
    #""Renamed Columns"" = Table.RenameColumns(#""Converted to Table"",{{""Column1"", ""Date""}}),
    #""Changed Type"" = Table.TransformColumnTypes(#""Renamed Columns"",{{""Date"", type date}}),
    #""Added Custom"" = Table.AddColumn(#""Changed Type"", ""Custom"", each [
        Year = Date.Year([Date]),
        StartOfYear = Date.StartOfYear([Date]),
        EndOfYear = Date.EndOfYear([Date]),
        Month = Date.Month([Date]),
        StartOfMonth = Date.StartOfMonth([Date]),
        EndOfMonth = Date.EndOfMonth([Date]),
        DaysInMonth = Date.DaysInMonth([Date]),
        Day = Date.Day([Date]),
        DayName = Date.DayOfWeekName([Date]),
        DayOfWeek = Date.DayOfWeek([Date], firstDayofWeek),
        DayOfYear = Date.DayOfYear([Date]),
        MonthName = Date.MonthName([Date]),
        Quarter = Date.QuarterOfYear([Date]),
        StartOfQuarter = Date.StartOfQuarter([Date]),
        EndOfQuarter = Date.EndOfQuarter([Date]),
        WeekOfYear = Date.WeekOfYear([Date], firstDayofWeek),
        WeekOfMonth = Date.WeekOfMonth([Date], firstDayofWeek),
        StartOfWeek = Date.StartOfWeek([Date], firstDayofWeek),
        EndOfWeek = Date.EndOfWeek([Date], firstDayofWeek)
    ]),
    #""Expanded Custom"" = Table.ExpandRecordColumn(#""Added Custom"", ""Custom"", {""Year"", ""StartOfYear"", ""EndOfYear"", ""Month"", ""StartOfMonth"", ""EndOfMonth"", ""DaysInMonth"", ""Day"", ""DayName"", ""DayOfWeek"", ""DayOfYear"", ""MonthName"", ""Quarter"", ""StartOfQuarter"", ""EndOfQuarter"", ""WeekOfYear"", ""WeekOfMonth"", ""StartOfWeek"", ""EndOfWeek""}, {""Year"", ""StartOfYear"", ""EndOfYear"", ""Month"", ""StartOfMonth"", ""EndOfMonth"", ""DaysInMonth"", ""Day"", ""DayName"", ""DayOfWeek"", ""DayOfYear"", ""MonthName"", ""Quarter"", ""StartOfQuarter"", ""EndOfQuarter"", ""WeekOfYear"", ""WeekOfMonth"", ""StartOfWeek"", ""EndOfWeek""}),
    FiscalMonthBaseIndex=13-StartofFiscalYear,
    adjustedFiscalMonthBaseIndex=if(FiscalMonthBaseIndex>=12 or FiscalMonthBaseIndex<0) then 0 else FiscalMonthBaseIndex,
    #""Added CustomA"" = Table.AddColumn(#""Expanded Custom"", ""FiscalBaseDate"", each Date.AddMonths([Date],adjustedFiscalMonthBaseIndex)),
    #""Changed Type2"" = Table.TransformColumnTypes(#""Added CustomA"",{{""FiscalBaseDate"", type date}}),
    #""Added CustomB"" = Table.AddColumn(#""Changed Type2"", ""Custom2"", each [
        Fiscal Year = Date.Year([FiscalBaseDate]),
        Fiscal Quarter = Date.QuarterOfYear([FiscalBaseDate]),
        Fiscal Month = Date.Month([FiscalBaseDate]),
        YearMonth = Date.ToText([Date], ""yyyy MMM""),
        YearMonth Code = Date.ToText([Date], ""yyyyMM"")
    ]),
    #""Expanded Custom2"" = Table.ExpandRecordColumn(#""Added CustomB"", ""Custom2"", {""Fiscal Year"", ""Fiscal Quarter"", ""Fiscal Month"", ""Age"", ""Month Offset"", ""Year Offset"", ""Quarter Offset"", ""YearMonth"", ""YearMonth Code""}, {""Fiscal Year"", ""Fiscal Quarter"", ""Fiscal Month"", ""Age"", ""Month Offset"", ""Year Offset"", ""Quarter Offset"", ""YearMonth"", ""YearMonth Code""}),
    #""Extracted Days"" = Table.TransformColumns(#""Expanded Custom2"",{{""Age"", Duration.Days, Int64.Type}}),
    #""Renamed Columns1"" = Table.RenameColumns(#""Extracted Days"",{{""Age"", ""Day Offset""}}),
    #""Changed Type1"" = Table.TransformColumnTypes(#""Renamed Columns1"",{{""StartOfYear"", type date}, {""EndOfYear"", type date}, {""StartOfMonth"", type date}, {""EndOfMonth"", type date}, {""StartOfQuarter"", type date}, {""EndOfQuarter"", type date}, {""StartOfWeek"", type date}, {""EndOfWeek"", type date}}),
    #""Removed Other Columns"" = Table.SelectColumns(#""Changed Type1"",{""Date"", ""Year"", ""Month"", ""Day"", ""DayName"", ""MonthName"", ""Quarter"", ""WeekOfYear"", ""YearMonth"", ""YearMonth Code""}),
#""Renamed Columns2"" = Table.RenameColumns(#""Extracted Days"",{{""Date"", """ + Column + @"""}})
in
    #""Renamed Columns2"" ";
// Update existing partition
var partition = table.Partitions.First();
partition.Expression = mExpression;
partition.Mode = ModeType.Import; // Set the refresh policy to Import
}
catch (Exception ex)
{MessageBox.Show("Adding the Date Dimension Table was not successful but the rest of the script was completed\n\nReason: "+ex.Message);
    }
}


