// Tabular Editor Script: 3. Calc Table\1. Create Calc Calendar Table (TE2)\2. Mark as Date Table
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;
using System.Windows.Forms;

// Find the table with the data category "Time"
var CalendarTable = Model.Tables.FirstOrDefault(table => table.DataCategory == "Time");

if (CalendarTable == null)
{
    string tableName = Interaction.InputBox("Provide the name of the date dimension table", "Table Name", "Calendar");
    CalendarTable = Model.Tables.FirstOrDefault(table => table.Name == tableName);

    if (CalendarTable == null)
    {
        MessageBox.Show("The table you provided does not exist in the model.");
        return;
    }
}

// Check if a column named "Date" exists in the CalendarTable
string dateColumnName = "Date";
var dateColumn = CalendarTable.Columns.FirstOrDefault(col => col.Name == dateColumnName);

if (dateColumn == null)
{
    // If no "Date" column is found, prompt the user for input
    dateColumnName = Interaction.InputBox("The 'Date' column was not found. Provide the name of the date column.", "Column Name", "Date");
    dateColumn = CalendarTable.Columns.FirstOrDefault(col => col.Name == dateColumnName);

    if (dateColumn == null)
    {
        MessageBox.Show("The column you provided does not exist in the model.");
        return;
    }
}

// Modify the found column (or user-provided column)
dateColumn.DataType = DataType.DateTime;
dateColumn.IsKey = true; // Ensure this is the key column
