// Tabular Editor Script: 6. Other\Document Model\Step 1: Create Tables
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;
using System.Windows.Forms;

string calctableTables = "_Tables";
string calctableColumns = "_Columns";
string calctableMeasure = "_Measure";
string calctableRelationships = "_Relationships";

// Define the DAX expression for the calculated table
string tableExpression1 = "INFO.VIEW.TABLES()";
string tableExpression2 = "INFO.VIEW.COLUMNS()";
string tableExpression3 = "INFO.VIEW.MEASURES()";
string tableExpression4 = "INFO.VIEW.RELATIONSHIPS()";

// Add the calculated table to the model
var table1 = Model.AddCalculatedTable(calctableTables, tableExpression1);
var table2 = Model.AddCalculatedTable(calctableColumns, tableExpression2);
var table3 = Model.AddCalculatedTable(calctableMeasure, tableExpression3);
var table4 = Model.AddCalculatedTable(calctableRelationships, tableExpression4);