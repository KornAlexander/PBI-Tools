// Tabular Editor Script: Archive\1.2 Measure: Δ Y-1 Simple  With fixed Variable
// Valid Contexts: Measure
// Tooltip: 
// Generated: 2025-08-21T16:54:13.142Z

var newMeasures = new List<Measure>();
    
foreach (var selectedMeasure in Selected.Measures)
    {
    // Get the selected measure
    var measuresTable = Model.Tables["Measure"];
    var Kalendartable = "Datum";
    var DateColumn = "Datum";

    // Define the new measure name and expression
    string newMeasureName1 = selectedMeasure.Name + " Δ PY";
    
    string newExpression1 = 
        "["+selectedMeasure.Name+"] - " + "["+selectedMeasure.Name+" PY]";

    // Create the new measure in the same table as the selected measure
    var newMeasure1 = measuresTable.AddMeasure(newMeasureName1, newExpression1);

    // Set the display folder for the new measure
    //newMeasure1.DisplayFolder = "Just Created " + selectedMeasure.Name;
    
    newMeasure1.FormatString = selectedMeasure.FormatString;
    newMeasures.Add(newMeasure1);
    
}

FormatDax(newMeasures, shortFormat: true, skipSpaceAfterFunctionName: true);