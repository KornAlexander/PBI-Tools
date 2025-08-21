// Tabular Editor Script: 7. Official TE Library\Count Rows in Table
// Valid Contexts: Table
// Tooltip: If you want to see how many rows are loaded to a table, or quickly check if the table has been loaded, at all. This script requires connection to a remote model or connection via Workspace Mode.
// Generated: 2025-08-21T16:54:13.141Z

// This script counts rows in a selected table and displays the result in a pop-up info box.
// It does not write any changes to this model.
//
// Use this script when you want to check whether a table was loaded or how many rows it has.
//
// Get table name
string _TableName = 
    Selected.Table.DaxObjectFullName;

// Count table rows
string _dax = 
    "{ FORMAT( COUNTROWS (" + _TableName + "), "#,##0" ) }";

// Evaluate DAX
string _TableRows = 
    Convert.ToString(EvaluateDax( _dax ));

// Return output in pop-up
Info ( "Number of rows in " + _TableName + ": " + _TableRows);
