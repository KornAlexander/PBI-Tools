// Tabular Editor Script: 8. Bernats Repo\3. Calc Group\Mask Calc Group Test
// Valid Contexts: Measure
// Tooltip: 
// Generated: 2025-08-21T16:54:13.142Z

if(Selected.Measures.Count() != 1)
{
    Error("No measures seleted");
    return;
}
Measure targetMeasure = Selected.Measure;
Table table = SelectTable(label: "Pick the table of the slicing column");
if(table == null) { return; };
Column column = SelectColumn(table, label: "Pick the slicing column");
if(column == null) { return; };
Measure sliderMeasure = SelectMeasure(label:"Pick Slider Measure");
CalculationGroupTable cg = Model.AddCalculationGroup("Mask");
string calcItemName = "Mask";
string calcItemExpression = "SELECTEDMEASURE()";
string calcItemFormatString =
    String.Format(@"VAR threshold = {2}
    VAR tbl =
        CALCULATETABLE(
            ADDCOLUMNS(
                VALUES( {1} ),
                ""@{3}"", {0}
            ),
            ALLSELECTED( {1} )
        )
    VAR tbl2 =
        ADDCOLUMNS(
            tbl,
            ""@RANK"", RANKX( tbl, [@{3}], , ASC, SKIP )
        )
    VAR valuesUnderThreshold =
        COUNTROWS( FILTER( tbl2, [@{3}] < threshold ) )
    VAR valuesToMask =
        switch( 
            TRUE(),
            valuesUnderThreshold = 0, 0,
            valuesUnderThreshold < 2, 2, 
            valuesUnderThreshold
        )
    VAR currentRank = 
        VAR tbl = 
        CALCULATETABLE(
            VALUES({1}),
            ALLSELECTED({1})
        )
        RETURN 
            RANKX(tbl,{0}, ,ASC)
    VAR result =
        IF( currentRank <= valuesToMask, ""'*'"", SELECTEDMEASUREFORMATSTRING( ) )
    RETURN
        result", 
    targetMeasure.DaxObjectFullName,
    column.DaxObjectFullName,
    sliderMeasure.DaxObjectFullName,
    targetMeasure.Name);
CalculationItem calcItem = cg.AddCalculationItem(calcItemName,calcItemExpression);
calcItem.FormatStringExpression = calcItemFormatString;
calcItem.FormatDax();
