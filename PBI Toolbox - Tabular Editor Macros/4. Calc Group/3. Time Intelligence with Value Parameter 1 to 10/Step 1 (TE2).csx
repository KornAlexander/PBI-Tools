// Tabular Editor Script: 4. Calc Group\3. Time Intelligence with Value Parameter 1 to 10\Step 1 (TE2)
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;
using System.Windows.Forms;

// Define the table name
string tableName = "Parameter 1 to 10";

// Create the calculated table using the DAX expression GENERATESERIES(0, 10, 1)
var calculatedTable = Model.AddCalculatedTable(tableName, "GENERATESERIES(0, 10, 1)");
