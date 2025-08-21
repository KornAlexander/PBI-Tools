// Tabular Editor Script: 1. Measure Create\2. Time: PY\1. Y-1
// Valid Contexts: Measure
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;
using System.Windows.Forms;

var newMeasures = new List<Measure>();

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


// Checking for Date Column otherwise prompt for user input
string DateColumn = null;
// Check if there is a column in the CalendarTable with IsKey = true
var keyColumn = CalendarTable.Columns.FirstOrDefault(col => col.IsKey == true);
if (keyColumn != null)
{
    DateColumn = keyColumn.Name;
}
else
{
    // If no key column found, prompt the user for input
    DateColumn = Interaction.InputBox("Provide the name of the date column name", "Column Name", "Date");
}




// Ask the user if they want to add the new measure to the current table
DialogResult dialogResult = MessageBox.Show("Do you want to add the new measure to the current table? (Click 'No' to add it to a custom table)", "Select Target Table", MessageBoxButtons.YesNo);

string measuresTableName = null;
if (dialogResult == DialogResult.No)
{
    // Ask for the name of the measure table if the user selects "No"
    measuresTableName = Interaction.InputBox("Provide the name of the measure table", "Name of Measure Table", "Measure");

    // Check if the provided table exists
    if (Model.Tables.FirstOrDefault(table => table.Name == measuresTableName) == null)
    {
        MessageBox.Show("The table you provided does not exist in the model.");
        return;
    }
}

foreach (var selectedMeasure in Selected.Measures)
{
    // Define the new measure name and expression
    string newMeasureName1 = selectedMeasure.Name + " PY";
    
    string newExpression1 = 
        "CALCULATE(" +
        "["+selectedMeasure.Name+"]" + ", " 
        +"SAMEPERIODLASTYEAR("+CalendarTable.Name +"["+DateColumn+"]))";

    // Add the new measure based on the user's choice
    Measure newMeasure1;
    if (dialogResult == DialogResult.Yes)
    {
        // Add measure to the current table (selectedMeasure.Table)
        newMeasure1 = selectedMeasure.Table.AddMeasure(newMeasureName1, newExpression1);
    }
    else
    {
        // Add measure to the user-specified table
        var measuresTable = Model.Tables.FirstOrDefault(table => table.Name == measuresTableName);
        newMeasure1 = measuresTable.AddMeasure(newMeasureName1, newExpression1);
    }

    // Set the format and add to the list
    newMeasure1.FormatString = selectedMeasure.FormatString;
    // Set the DisplayFolder to the current DisplayFolder + "PY"
    newMeasure1.DisplayFolder = string.IsNullOrEmpty(selectedMeasure.DisplayFolder) ? "PY" : selectedMeasure.DisplayFolder + @"\PY";

    newMeasures.Add(newMeasure1);
    // Format the DAX of the new measure
    FormatDax(newMeasures, shortFormat: true, skipSpaceAfterFunctionName: true);

}


