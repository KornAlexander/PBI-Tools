// Tabular Editor Script: Archive\1.2 Measure: All Y-1 Simple with fixed Variable
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
    string newMeasureName1 = selectedMeasure.Name + " PY";
    string newMeasureName2 = selectedMeasure.Name + " Δ PY";
    string newMeasureName3 = selectedMeasure.Name + " Δ PY %";
    
    string newExpression1 = 
        "CALCULATE(" +
        "["+selectedMeasure.Name+"]" + ", " 
        +"SAMEPERIODLASTYEAR("+Kalendartable+"["+DateColumn+"]))";
    string newExpression2 = 
        "["+selectedMeasure.Name+"] - " + "["+selectedMeasure.Name+" PY]";
    string newExpression3 = 
        "DIVIDE(["+selectedMeasure.Name+"] - " + "["+selectedMeasure.Name+" PY], ["+selectedMeasure.Name+"])";


    // Create the new measure in the same table as the selected measure
    var newMeasure1 = measuresTable.AddMeasure(newMeasureName1, newExpression1);
    var newMeasure2 = measuresTable.AddMeasure(newMeasureName2, newExpression2);
    var newMeasure3 = measuresTable.AddMeasure(newMeasureName3, newExpression3);
    
    // Set the display folder for the new measure
    //newMeasure1.DisplayFolder = "Just Created " + selectedMeasure.Name;
    
    newMeasure1.FormatString = selectedMeasure.FormatString;
    newMeasures.Add(newMeasure1);
    newMeasures.Add(newMeasure2);
    newMeasures.Add(newMeasure3);
}

FormatDax(newMeasures, shortFormat: true, skipSpaceAfterFunctionName: true);