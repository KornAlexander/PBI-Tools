// Tabular Editor Script: Min Date Column
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.142Z

// Loop through all tables to find the one with DataCategory = Time
var calendarTable = Model.Tables
    .FirstOrDefault(t => t.DataCategory == "Time");

if (calendarTable == null)
{
    Error("No calendar table found (with DataCategory = Time).");
    return;
}

// Try to find the key column (usually marked as IsKey)
var keyColumn = calendarTable.Columns
    .FirstOrDefault(c => c.IsKey && c.DataType == DataType.DateTime);

// If not found, fallback to first DateTime column
if (keyColumn == null)
{
    keyColumn = calendarTable.Columns
        .FirstOrDefault(c => c.DataType == DataType.DateTime);
}

if (keyColumn == null)
{
    Error("No DateTime column found in the calendar table.");
    return;
}

// Generate the DAX for the measure
string measureName = "Min Selected Date";
string daxFormula = $@"
VAR __minDate = CALCULATE(MIN('{calendarTable.Name}'[{keyColumn.Name}]), REMOVEFILTERS('{calendarTable.Name}'))
RETURN
IF(
    ISFILTERED('{calendarTable.Name}'[{keyColumn.Name}]),
    SELECTEDVALUE('{calendarTable.Name}'[{keyColumn.Name}]),
    __minDate
)";

// Add or update the measure
var existingMeasure = calendarTable.Measures.FirstOrDefault(m => m.Name == measureName);
if (existingMeasure != null)
{
    existingMeasure.Expression = daxFormula;
}
else
{
    calendarTable.AddMeasure(measureName, daxFormula);
}

// Optional: output message
Info($"Measure '{measureName}' created/updated on table '{calendarTable.Name}' using column '{keyColumn.Name}'.");
