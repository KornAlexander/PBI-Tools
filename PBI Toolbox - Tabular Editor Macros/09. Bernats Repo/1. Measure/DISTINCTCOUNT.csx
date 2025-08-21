// Tabular Editor Script: 8. Bernats Repo\1. Measure\DISTINCTCOUNT
// Valid Contexts: Column
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

if(Selected.Columns.Count() == 0)
{
    Error("You need to select at least one measure.");
    return;
};
foreach(Column column in Selected.Columns)
{
    string measureName = "Distinct " + column.Name;
    string measureExpression = String.Format("DISTINCTCOUNT({0})", column.DaxObjectFullName);
    string measureDescription = "Distinct count of " + column.Name;
    Measure measure = column.Table.AddMeasure(measureName, measureExpression);
    measure.Description = measureDescription;
    measure.FormatDax(); 
};
