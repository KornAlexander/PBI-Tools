// Tabular Editor Script: 8. Bernats Repo\1. Measure\MAX
// Valid Contexts: Column
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

string annotationLabel = "Namasdata";
string annotationValue = "Statistics";
if(Selected.Columns.Count() != 1)
{
    Error("Selecciona una única columna y vuelve a intentarlo");
    return;
}
Column sliceColumn = Selected.Column;
CalculationGroupTable statisticsCG = null as CalculationGroupTable;
if(Model.Tables.Any(t => t.GetAnnotation(annotationLabel) == annotationValue))
{
    statisticsCG = Model.Tables.Where(t => t.GetAnnotation(annotationLabel) == annotationValue).First() as CalculationGroupTable;
} else
{
    statisticsCG = Model.AddCalculationGroup("Statistics");
    statisticsCG.SetAnnotation(annotationLabel, annotationValue);
}
string calcItemName = "MAX";
string calcItemExpression =
    String.Format(
        @"MAXX (
            VALUES ( {0} ),
            SELECTEDMEASURE ()
        )", sliceColumn.DaxObjectFullName);
CalculationItem calcItem = statisticsCG.AddCalculationItem(calcItemName, calcItemExpression);
calcItem.FormatDax();
