// Tabular Editor Script: 1. Measure Create\2. Time: PY\4. ALL Y-1
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

    string newMeasureName2 = selectedMeasure.Name + " Δ PY";
    string newExpression2 = 
        "["+selectedMeasure.Name+"] - " + "["+selectedMeasure.Name+" PY]";

    string newMeasureName3 = selectedMeasure.Name + " Δ PY %";
    string newExpression3 = 
        "DIVIDE(["+selectedMeasure.Name+"] - " + "["+selectedMeasure.Name+" PY], ["+selectedMeasure.Name+"])";

    string newMeasureName4 = selectedMeasure.Name + " Max Green PY";
    string newExpression4 = 
        "IF(["+selectedMeasure.Name+" Δ PY] > 0, MAX( ["+selectedMeasure.Name+"], [" +selectedMeasure.Name+" PY]))";

        string newMeasureName5 = selectedMeasure.Name + " Max Red AC";
    string newExpression5 = 
        "IF(["+selectedMeasure.Name+" Δ PY] < 0, MAX( ["+selectedMeasure.Name+"], [" +selectedMeasure.Name+" PY]))";

    // Add the new measure based on the user's choice
    Measure newMeasure1;
    Measure newMeasure2;
    Measure newMeasure3;
    Measure newMeasure4;
    Measure newMeasure5;
    if (dialogResult == DialogResult.Yes)
    {
        // Add measure to the current table (selectedMeasure.Table)
        newMeasure1 = selectedMeasure.Table.AddMeasure(newMeasureName1, newExpression1);
        newMeasure2 = selectedMeasure.Table.AddMeasure(newMeasureName2, newExpression2);
        newMeasure3 = selectedMeasure.Table.AddMeasure(newMeasureName3, newExpression3);
        newMeasure4 = selectedMeasure.Table.AddMeasure(newMeasureName4, newExpression4);
        newMeasure5 = selectedMeasure.Table.AddMeasure(newMeasureName5, newExpression5);
    }
    else
    {
        // Add measure to the user-specified table
        var measuresTable = Model.Tables.FirstOrDefault(table => table.Name == measuresTableName);
        newMeasure1 = measuresTable.AddMeasure(newMeasureName1, newExpression1);
        newMeasure2 = measuresTable.AddMeasure(newMeasureName2, newExpression2);
        newMeasure3 = measuresTable.AddMeasure(newMeasureName3, newExpression3);
        newMeasure4 = measuresTable.AddMeasure(newMeasureName4, newExpression4);
        newMeasure5 = measuresTable.AddMeasure(newMeasureName5, newExpression5);
    }

    // Set the format and add to the list
    newMeasure1.FormatString = selectedMeasure.FormatString;
    newMeasure1.DisplayFolder = string.IsNullOrEmpty(selectedMeasure.DisplayFolder) ? "PY" : selectedMeasure.DisplayFolder + @"\PY";
    newMeasure2.FormatString = selectedMeasure.FormatString;
    newMeasure2.DisplayFolder = string.IsNullOrEmpty(selectedMeasure.DisplayFolder) ? "PY" : selectedMeasure.DisplayFolder + @"\PY";
    newMeasure3.FormatString = selectedMeasure.FormatString;
    newMeasure3.DisplayFolder = string.IsNullOrEmpty(selectedMeasure.DisplayFolder) ? "PY" : selectedMeasure.DisplayFolder + @"\PY";
    newMeasure4.FormatString = selectedMeasure.FormatString;
    newMeasure4.DisplayFolder = string.IsNullOrEmpty(selectedMeasure.DisplayFolder) ? "PY" : selectedMeasure.DisplayFolder + @"\PY";
    newMeasure5.FormatString = selectedMeasure.FormatString;
    newMeasure5.DisplayFolder = string.IsNullOrEmpty(selectedMeasure.DisplayFolder) ? "PY" : selectedMeasure.DisplayFolder + @"\PY";


    newMeasures.Add(newMeasure1);
    newMeasures.Add(newMeasure2);
    newMeasures.Add(newMeasure3);
    newMeasures.Add(newMeasure4);
    newMeasures.Add(newMeasure5);

    // Format the DAX of the new measure
    FormatDax(newMeasures, shortFormat: true, skipSpaceAfterFunctionName: true);

}


