// Tabular Editor Script: 8. Bernats Repo\1. Measure\Add Total by Calc Item (NAMASDATA)
// Valid Contexts: CalculationItem
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

string annotationLabel = "Namasdata";
string annotationValue = "Totales";
string sliceColumnLabel = "sliceColumn";
if (Selected.CalculationItems.Count() == 0)
{
    Error("Selecciona al menos un elemento de cálculo");
    return;
}
if (!Model.Tables.Any(t => t.GetAnnotation(annotationLabel) == annotationValue))
{
    Error("Ejecuta primero la macro de crear Grupo de cálculo de Totales");
    return;
}
CalculationGroupTable totalsCG = Model.Tables.Where(t => t.GetAnnotation(annotationLabel) == annotationValue).First() as CalculationGroupTable;
string sliceColumn = totalsCG.GetAnnotation(sliceColumnLabel);
foreach(CalculationItem cItem in Selected.CalculationItems)
{
    Column cItemColumn = cItem.CalculationGroupTable.Columns[0];
    string calcItemName = cItem.Name;
    string calcItemExpression =
        String.Format(
            @"IF(
                ISINSCOPE({0}),
                BLANK(),
                CALCULATE(
                    SELECTEDMEASURE(),
                    {1} = ""{2}""
                )
            )", sliceColumn, cItemColumn.DaxObjectFullName, cItem.Name);
    CalculationItem calcItem = totalsCG.AddCalculationItem(calcItemName, calcItemExpression);
    calcItem.FormatDax();
}
