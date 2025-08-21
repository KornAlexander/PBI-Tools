// Tabular Editor Script: 2. Measure Modify\3. Unit Dynamic FormatStringExpression
// Valid Contexts: Measure
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

var updatedMeasures = new List<Measure>();
    
foreach (var selectedMeasure in Selected.Measures)
    {
    // Get the selected measure
    var measuresTable = Model.Tables["Measure"];
    var UnitTable = "Einheit";
    var UnitColumn = "Einheit";
    var UnitThousand = "Tausend";
    var UnitMillion = "Millionen";

    // Define the dynamic format string expression
    string dynamicFormatString = 
        "SWITCH(" +
        "SELECTEDVALUE( "+UnitTable+"["+UnitColumn+"] ), " +
        """+UnitThousand+"", "0,#", " +
        """+UnitMillion+"", "#,0.#", " +
        ""#,#")";

    // Update the format string for the selected measure
    selectedMeasure.FormatStringExpression = dynamicFormatString;
    updatedMeasures.Add(selectedMeasure);
    
}

