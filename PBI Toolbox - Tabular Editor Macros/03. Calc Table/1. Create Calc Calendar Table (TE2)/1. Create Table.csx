// Tabular Editor Script: 3. Calc Table\1. Create Calc Calendar Table (TE2)\1. Create Table
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;
using System.Windows.Forms;

string calctableCalendar = Interaction.InputBox("Provide the name of the date dimension table", "Table Name", "CalendarCalcTable");



    // Define the DAX expression for the calculated table
    string tableExpression = @"
VAR all_dates = CALENDARAUTO()
RETURN
    ADDCOLUMNS(
        all_dates,
        ""DateKey"", VALUE(FORMAT([Date], ""YYYYMMDD"")),
        ""Year"", YEAR([Date]),
        ""Quarter"", QUARTER([Date]),
        ""Month"", MONTH([Date]),
        ""End of Month"", EOMONTH([Date], 0),
        ""Week of Year"", WEEKNUM([Date]),
        ""Weekday"", WEEKDAY([Date])
    )
";

    // Add the calculated table to the model
    var table = Model.AddCalculatedTable(calctableCalendar, tableExpression);
        table.DataCategory = "Time";

