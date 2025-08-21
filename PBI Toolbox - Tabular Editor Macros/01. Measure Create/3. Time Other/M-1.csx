// Tabular Editor Script: 1. Measure Create\3. Time: Other\M-1
// Valid Contexts: Measure
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

var newMeasures = new List<Measure>();
    
foreach (var selectedMeasure in Selected.Measures)
    {
    // Get the selected measure
    var measuresTable = Model.Tables["Measure"];
    var Kalendartable = "Calendar";
    var MonthColumn = "Month (MMM)";
    var RelativMonth = "Relative Month";

    // Define the new measure name and expression
    string newMeasureName1 = selectedMeasure.Name + " M-1";
    
    string newExpression1 = 
        "IF(DISTINCTCOUNT("+Kalendartable+"["+MonthColumn+"])=1,["+selectedMeasure.Name+"]"
        +",CALCULATE(" +
        "["+selectedMeasure.Name+"]" + ", " 
        +Kalendartable+"["+RelativMonth+"]=-1))";

    // Create the new measure in the same table as the selected measure
    var newMeasure1 = measuresTable.AddMeasure(newMeasureName1, newExpression1);

    // Set the display folder for the new measure
    //newMeasure1.DisplayFolder = "Just Created " + selectedMeasure.Name;
    
    newMeasure1.FormatString = selectedMeasure.FormatString;
    newMeasures.Add(newMeasure1);
    
}

FormatDax(newMeasures, shortFormat: true, skipSpaceAfterFunctionName: true);