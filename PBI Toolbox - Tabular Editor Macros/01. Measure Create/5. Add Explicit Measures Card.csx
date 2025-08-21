// Tabular Editor Script: 1. Measure Create\5. Add Explicit Measures: Card
// Valid Contexts: Measure
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

    foreach (var selectedMeasure in Selected.Measures)
    {
    // Get the selected measure
    var measuresTable = Model.Tables["Measure"];

    // Define the new measure name and expression
    string newMeasureName1 = selectedMeasure.Name + " M-1";
    string newMeasureName2 = selectedMeasure.Name + " M-2";
    string newMeasureName3 = selectedMeasure.Name + " M-13";
    string newMeasureName4 = selectedMeasure.Name + " Δ Delta M-2";
    string newMeasureName5 = selectedMeasure.Name + " Δ Delta M-2 %";
    string newMeasureName6 = selectedMeasure.Name + " Δ Delta M-13";
    string newMeasureName7 = selectedMeasure.Name + " Δ Delta M-13 %";
    string newMeasureName8 = selectedMeasure.Name + " Reference 1 Δ Vormonat";
    string newMeasureName9 = selectedMeasure.Name + " Reference 2 Δ Vorjahr";
    string newMeasureName10 = selectedMeasure.Name + " Sparkline SVG";
    string newMeasureName11 = selectedMeasure.Name + " Color Δ PM";
    string newMeasureName12 = selectedMeasure.Name + " Color Δ LY";
    
    string newExpression1 = 
        "IF(ISFILTERED(Kalender[Monat (MMM)]),["+selectedMeasure.Name+"]"
        +",CALCULATE(" +
        "["+selectedMeasure.Name+"]" + ", " +
        "Kalender[Relativer Monat #]=-1))";
    string newExpression2 = 
        "CALCULATE(" +
        "["+selectedMeasure.Name+"]" + ", " +
        "Kalender[Relativer Monat #]=-2)";
    string newExpression3 = 
        "CALCULATE(" +
        "["+selectedMeasure.Name+"]" + ", " +
        "Kalender[Relativer Monat #]=-13)";
    
    string newExpression4 = 
        "IF( ISFILTERED(Kalender[Monat (MMM)])"+
        ",["+selectedMeasure.Name+"]"
        +"-CALCULATE("+"["+selectedMeasure.Name+"]"+",PREVIOUSMONTH(Kalender[Datum]))"+
        ",["+newMeasureName1+"]-["+newMeasureName2+"])";
    string newExpression5 = 
        "IF( ISFILTERED(Kalender[Monat (MMM)])"+
        ",["+newMeasureName4+"]/CALCULATE("+"["+selectedMeasure.Name+"]"+",PREVIOUSMONTH(Kalender[Datum]))"+
        ",["+newMeasureName4+"]/["+newMeasureName1+"])";

    //string newExpression6 =  "["+newMeasureName1+"]-["+newMeasureName3+"]";
    //string newExpression7 =  "["+newMeasureName6+"]/["+newMeasureName1+"]";
    
    string newExpression6 = 
        "IF( ISFILTERED(Kalender[Monat (MMM)])"+
        ","+"["+selectedMeasure.Name+"]"
        +"-CALCULATE("+"["+selectedMeasure.Name+"]"+",SAMEPERIODLASTYEAR(Kalender[Datum]))"+
        ",["+newMeasureName1+"]-["+newMeasureName3+"])";
    string newExpression7 = 
        "IF( ISFILTERED(Kalender[Monat (MMM)])"+
        ",["+newMeasureName6+"]/CALCULATE("+"["+selectedMeasure.Name+"]"+",SAMEPERIODLASTYEAR(Kalender[Datum]))"+
        ",["+newMeasureName6+"]/["+newMeasureName1+"])";
    
    string newExpression8 = "var _sign_icon = IF([" + newMeasureName4 + "] > 0, "▲ +", "▼ -" & UNICHAR(127))
" +
                            "var _sign_plusminus = IF([" + newMeasureName4 + "] > 0, " | +", " | - ")
" +
                            "var _valprct = ABS([" + newMeasureName5 + "])
" +
                            "var _valdiff = ABS([" + newMeasureName4 + "])
" +
                            "RETURN _sign_icon & FORMAT(_valprct, "#.0%") & _sign_plusminus & FORMAT(_valdiff, "#,0")";
    string newExpression9 = "var _sign_icon = IF([" + newMeasureName6 + "] > 0, "▲ +", "▼ -" & UNICHAR(127))
" +
                            "var _sign_plusminus = IF([" + newMeasureName6 + "] > 0, " | +", " | - ")
" +
                            "var _valprct = ABS([" + newMeasureName7 + "])
" +
                            "var _valdiff = ABS([" + newMeasureName6 + "])
" +
                            "RETURN _sign_icon & FORMAT(_valprct, "#.0%") & _sign_plusminus & FORMAT(_valdiff, "#,0")";
    
    string newExpression10 =
    "VAR LineColour = "%23808080"
" +
    "VAR PointColour = "white"
" +
    "VAR Defs = " +
        ""<defs><linearGradient id='grad' x1='0' y1='25' x2='0' y2='50' gradientUnits='userSpaceOnUse'>" +
        "<stop stop-color='%23808080' offset='0' />" +
        "<stop stop-color='%23808080' offset='0.3' />" +
        "<stop stop-color='white' offset='1' />" +
        "</linearGradient></defs>"
" +
    "VAR XMinDate = MIN('Kalender'[MonatKey #])
" +
    "VAR XMaxDate = MAX('Kalender'[MonatKey #])
" +
    "VAR YMinValue = MINX(Values(Kalender[MonatKey #]),["+selectedMeasure.Name+"])
" +
    "VAR YMaxValue = MAXX(Values(Kalender[MonatKey #]),["+selectedMeasure.Name+"])
" +
    "VAR SparklineTable = ADDCOLUMNS(
" +
    "    SUMMARIZE('Kalender',Kalender[MonatKey #]),
" +
    "        "X",INT(150 * DIVIDE(Kalender[MonatKey #] - XMinDate, XMaxDate - XMinDate)),
" +
    "        "Y",INT(50 * DIVIDE(["+selectedMeasure.Name+"] - YMinValue,YMaxValue - YMinValue)))
" +
    "VAR Lines = CONCATENATEX(SparklineTable,[X] & "," & 50-[Y]," ", Kalender[MonatKey #])
" +
    "VAR LastSparkYValue = MAXX( FILTER(SparklineTable, Kalender[MonatKey #] = XMaxDate), [Y])
" +
    "VAR LastSparkXValue = MAXX( FILTER(SparklineTable, Kalender[MonatKey #] = XMaxDate), [X])
" +
    "VAR SVGImageURL = 
" +
    "    "data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' x='0px' y='0px' viewBox='-7 -7 164 64'>" & Defs & 
" +
    "     "<polyline fill='url(#grad)' fill-opacity='0.3' stroke='transparent' stroke-width='0' points=' 0 50 " & Lines & 
" +
    "      " 150 150 Z '/>" &
" +
    "    "<polyline fill='transparent' stroke='" & LineColour & "' stroke-linecap='round' stroke-linejoin='round' stroke-width='3' points=' " & Lines & 
" +
    "      " '/>" &
" +
    "    "<circle cx='"& LastSparkXValue & "' cy='" & 50 - LastSparkYValue & "' r='4' stroke='" & LineColour & "' stroke-width='3' fill='" & PointColour & "' />" &
" +
    "    "</svg>"
" +
    "RETURN SVGImageURL";

    string newExpression11 = 
    "VAR MeasureToUse = [" + newMeasureName4 + "]
" +
    "RETURN
" +
    "IF (
" +
    "    MeasureToUse < 0,
" +
    "    "#FF0000",
" +
    "    IF (
" +
    "        MeasureToUse > 0,
" +
    "        "#92D050"
" +
    "    )
" +
    ")";

    string newExpression12 = 
    "VAR MeasureToUse = [" + newMeasureName6 + "]
" +
    "RETURN
" +
    "IF (
" +
    "    MeasureToUse < 0,
" +
    "    "#FF0000",
" +
    "    IF (
" +
    "        MeasureToUse > 0,
" +
    "        "#92D050"
" +
    "    )
" +
    ")";
    
    // Create the new measure in the same table as the selected measure
    var newMeasure1 = measuresTable.AddMeasure(newMeasureName1, newExpression1);
    var newMeasure2 = measuresTable.AddMeasure(newMeasureName2, newExpression2);
    var newMeasure3 = measuresTable.AddMeasure(newMeasureName3, newExpression3);
    var newMeasure4 = measuresTable.AddMeasure(newMeasureName4, newExpression4);
    var newMeasure5 = measuresTable.AddMeasure(newMeasureName5, newExpression5);
    var newMeasure6 = measuresTable.AddMeasure(newMeasureName6, newExpression6);
    var newMeasure7 = measuresTable.AddMeasure(newMeasureName7, newExpression7);
    var newMeasure8 = measuresTable.AddMeasure(newMeasureName8, newExpression8);
    var newMeasure9 = measuresTable.AddMeasure(newMeasureName9, newExpression9);
    var newMeasure10 = measuresTable.AddMeasure(newMeasureName10, newExpression10);
    var newMeasure11 = measuresTable.AddMeasure(newMeasureName11, newExpression11);
    var newMeasure12 = measuresTable.AddMeasure(newMeasureName12, newExpression12);
    
    // Set the display folder for the new measure
    newMeasure1.DisplayFolder = "Card " + selectedMeasure.Name;
    newMeasure2.DisplayFolder = "Card " + selectedMeasure.Name;
    newMeasure3.DisplayFolder = "Card " + selectedMeasure.Name;
    newMeasure4.DisplayFolder = "Card " + selectedMeasure.Name;
    newMeasure5.DisplayFolder = "Card " + selectedMeasure.Name;
    newMeasure6.DisplayFolder = "Card " + selectedMeasure.Name;
    newMeasure7.DisplayFolder = "Card " + selectedMeasure.Name;
    newMeasure8.DisplayFolder = "Card " + selectedMeasure.Name;
    newMeasure9.DisplayFolder = "Card " + selectedMeasure.Name;
    newMeasure10.DisplayFolder = "Card " + selectedMeasure.Name;
    newMeasure11.DisplayFolder = "Card " + selectedMeasure.Name;
    newMeasure12.DisplayFolder = "Card " + selectedMeasure.Name;
    
    newMeasure1.FormatString = selectedMeasure.FormatString;
    newMeasure2.FormatString = selectedMeasure.FormatString;
    newMeasure3.FormatString = selectedMeasure.FormatString;
    newMeasure4.FormatString = selectedMeasure.FormatString;
    newMeasure5.FormatString = selectedMeasure.FormatString;
    newMeasure6.FormatString = selectedMeasure.FormatString;
    newMeasure7.FormatString = selectedMeasure.FormatString;
    newMeasure8.FormatString = selectedMeasure.FormatString;
    newMeasure9.FormatString = selectedMeasure.FormatString;
    newMeasure10.FormatString = selectedMeasure.FormatString;
    newMeasure11.FormatString = selectedMeasure.FormatString;
    newMeasure12.FormatString = selectedMeasure.FormatString;
    
}