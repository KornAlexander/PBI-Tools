// Tabular Editor Script: 2. Measure Modify\2. Add Last Refresh Measure
// Valid Contexts: Model, Table
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

string tableNameEmptyMeasure = "Measure";
string tableNameLastRefresh = "Last Refresh";
string LastRefreshMeasureName = "Last Refresh Measure";
string columnNameLastRefresh = "Last Refresh Timestamp";
string measureDax = ""Last Refresh: " & MAX('" + tableNameLastRefresh + "'[" + columnNameLastRefresh + "])";

var table2 = Model.Tables[tableNameEmptyMeasure];
var measurelastrefresh = table2.AddMeasure(LastRefreshMeasureName, measureDax, "Meta");
