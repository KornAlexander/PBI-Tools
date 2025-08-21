// Tabular Editor Script: 3. Calc Table\2. Create Empty Measure Table (TE3)
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

// ATTENTION FOR TE2 Users: Script needs modification AND needs to be run in 2 steps

// First Step: Add Table
var table = Model.AddCalculatedTable("Measure", "{0}"); 

// Second Step: JUST FOR TE2 Save Data Model Changes

// Third Step: Hides the column, uncomment the next line and execute it separately
//var table = Model.Tables["Measure"]; //uncomment this line for TE2
table.Columns[0].IsHidden = true;