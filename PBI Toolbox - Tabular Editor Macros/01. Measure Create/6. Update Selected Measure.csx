// Tabular Editor Script: 1. Measure Create\6. Update Selected Measure
// Valid Contexts: Measure
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

var updatedMeasures = new List<Measure>();

foreach (var selectedMeasure in Selected.Measures)
{
    // Get the selected measure
    var measuresTable = Model.Tables["Measure"];

    // Retrieve the previous DAX expression of the selected measure
    var previousExpression = selectedMeasure.Expression;

    // Define the new DAX expression by appending the new condition
    string newExpression = "("+previousExpression+ ")*( 0.3 + RAND())";

    // Update the existing measure with the new expression
    selectedMeasure.Expression = newExpression;
    updatedMeasures.Add(selectedMeasure);
}

FormatDax(updatedMeasures, shortFormat: true, skipSpaceAfterFunctionName: true);
