// Tabular Editor Script: Archive\1.1 Measure: Selected SUM Simple in Current Table
// Valid Contexts: Column
// Tooltip: 
// Generated: 2025-08-21T16:54:13.142Z

// Creates a SUM measure for every currently selected column and hide the column.
foreach(var c in Selected.Columns)
{
    var newMeasure = c.Table.AddMeasure(
        "NameToReplace" + c.Name,                    // Name
        "SUM(" + c.DaxObjectFullName + ")",    // DAX expression
        "ENTERHERE_DisplayFolderName"                        // Display Folder
    );
    
    // Set the format string on the new measure:
    newMeasure.FormatString = "0.0";

    // Provide some documentation:
    newMeasure.Description = "This measure is the sum of column " + c.DaxObjectFullName;

    // Hide the base column:
    c.IsHidden = true;
}