// Tabular Editor Script: 1. Measure Create\3. Time: Other\Measure: YTD
// Valid Contexts: Measure
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

var newMeasures = new List<Measure>();
   
 foreach (var selectedMeasure in Selected.Measures)
    {
    // Variables to use and getting the measure table
    var measuresTable = Model.Tables["Measure"];
    var Kalendartable = "Calendar";
    var MonthColumn = "Month (MMM)";
    var RelativMonth = "Relative Month";

    // Define the new measure name and expression
    string newMeasureName1 = selectedMeasure.Name + " YTD";
    
    string newExpression1 = 
            "CALCULATE(["+selectedMeasure.Name+"],ALL('"+Kalendartable+"'),'"+Kalendartable+"'["+RelativMonth+"]<0,'"+Kalendartable+"'["+RelativMonth+"]>-13)";
        
    // Create the new measure in the same table as the selected measure
    var newMeasure1 = measuresTable.AddMeasure(newMeasureName1, newExpression1);

    // Set the display folder for the new measure
    //newMeasure1.DisplayFolder =  Get.selectedMeasure.DisplayFolder;
    
    newMeasure1.FormatString = selectedMeasure.FormatString;
    newMeasures.Add(newMeasure1);

}

FormatDax(newMeasures, shortFormat: true, skipSpaceAfterFunctionName: true);
//FormatDax(Model.AllMeasures, shortFormat: true, skipSpaceAfterFunctionName: true);
