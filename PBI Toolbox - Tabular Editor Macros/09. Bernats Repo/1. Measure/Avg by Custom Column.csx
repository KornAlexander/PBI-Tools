// Tabular Editor Script: 8. Bernats Repo\1. Measure\Avg by Custom Column
// Valid Contexts: Measure
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

if(Selected.Measures.Count() == 0)
{
    Error("You need to select at least one measure.");
    return;
};
Table table = SelectTable(label: "Select a table");
if(table == null)
{
    Error("You cancelled");
    return;
};
Column column = SelectColumn(table);
if (column == null)
{
    Error("You cancelled");
    return;
};
foreach(Measure measure in Selected.Measures)
{
    string measureName = "Avg " + measure.Name + " by " + column.Name;
    string measureExpression = String.Format(@"AVERAGEX(  VALUES({0}),  {1})", column.DaxObjectFullName, measure.DaxObjectFullName);
    string measureDisplayFolder = "Avgs of " + measure.Name;
    string measureDescription = measureName;
    Measure newMeasure = measure.Table.AddMeasure(measureName, measureExpression,displayFolder:measureDisplayFolder);
    newMeasure.Description = measureDescription;
    newMeasure.FormatDax();
}
