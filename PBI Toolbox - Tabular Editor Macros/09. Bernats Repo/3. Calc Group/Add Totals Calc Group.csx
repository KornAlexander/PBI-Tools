// Tabular Editor Script: 8. Bernats Repo\3. Calc Group\Add Totals Calc Group
// Valid Contexts: Column
// Tooltip: 
// Generated: 2025-08-21T16:54:13.142Z

string annotationLabel = "Namasdata";
string annotationValue = "Totales";
string sliceColumnLabel = "sliceColumn";
if (Selected.Columns.Count() != 1)
{
    Error("Selecciona una única columna y vuelve a intentarlo");
    return;
}
Column sliceColumn = Selected.Column;
string sliceColumnValue = sliceColumn.DaxObjectFullName;
CalculationGroupTable totalsCG = null as CalculationGroupTable;
if (Model.Tables.Any(t => t.GetAnnotation(annotationLabel) == annotationValue))
{
    totalsCG = Model.Tables.Where(t => t.GetAnnotation(annotationLabel) == annotationValue).First() as CalculationGroupTable;
}
else
{
    totalsCG = Model.AddCalculationGroup("Totals");
    totalsCG.SetAnnotation(annotationLabel, annotationValue);
    totalsCG.SetAnnotation(sliceColumnLabel, sliceColumnValue);
}
string valuesCalcItemName = "Values";
string valuesCalcItemExpression =
    String.Format(
    @"IF(
        ISINSCOPE({0}),
        SELECTEDMEASURE(),
        BLANK()
    )", sliceColumn.DaxObjectFullName);
CalculationItem valuesCalcItem = totalsCG.AddCalculationItem(valuesCalcItemName, valuesCalcItemExpression);
string totalCalcItemName = "Total";
string totalCalcItemExpression =
    String.Format(
    @"IF(
        ISINSCOPE({0}),
        BLANK(),
        SELECTEDMEASURE()
    )", sliceColumn.DaxObjectFullName);
CalculationItem totalCalcItem = totalsCG.AddCalculationItem(totalCalcItemName, totalCalcItemExpression);
