// Tabular Editor Script: 3. Calc Table\3. Create Value Parameter 1 to 10 (TE3)
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

// Define the table name
string tableName = "Parameter 1 to 10";

// Create the calculated table using the DAX expression GENERATESERIES(0, 10, 1)
var table = Model.AddCalculatedTable(tableName, "GENERATESERIES(0, 10, 1)");

// Access the 'Value' column created by the GENERATESERIES function
var fieldColumn = table.Columns["Value"];

    // Set the extended property with the given JSON value on the renamed column
    fieldColumn.SetExtendedProperty("ParameterMetadata", "{"version":0}", ExtendedPropertyType.Json);
    fieldColumn.SummarizeBy =AggregateFunction.None;


// Add the measure 'Parameter 1 to 10 Value' to the table
string measureName = "Parameter 1 to 10 Value";
string daxExpression = "SELECTEDVALUE('Parameter 1 to 10'[Value], 1)";

// Add the new measure
var measure = table.AddMeasure(measureName, daxExpression);
