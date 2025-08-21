// Tabular Editor Script: Archive\1.1 Measure: Selected SUM Simple in Measure Table
// Valid Contexts: Measure
// Tooltip: This adds for selected Tables all explicit sum measures
// Generated: 2025-08-21T16:54:13.142Z

#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;
using System.Windows.Forms;

// Ask the user if they want to add the new measure to the current table
DialogResult dialogResult = MessageBox.Show("Do you want to add the new measure to the current table? (Click 'No' to add it to a custom table)", "Select Target Table", MessageBoxButtons.YesNo);

string measuresTableName = Selected.Columns.First().Table.Name;
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

// Create a SUM measure for every currently selected column and hide the column
foreach(var c in Selected.Columns)
{
    // Use the target table determined by user selection or default "Measure" table
    var measuresTable = Model.Tables[measuresTableName];
    var newMeasure = measuresTable.AddMeasure(
        /*"Sum_" +*/ c.Name,                            // Name
        "SUM(" + c.DaxObjectFullName + ")",         // DAX expression
        c.Table.Name                                // Display Folder
    );
    
    // Set the format string on the new measure:
    newMeasure.FormatString = "0.0";

    // Provide some documentation:
    newMeasure.Description = "This measure is the sum of column " + c.DaxObjectFullName;

    // Hide the base column:
    c.IsHidden = true;
}
