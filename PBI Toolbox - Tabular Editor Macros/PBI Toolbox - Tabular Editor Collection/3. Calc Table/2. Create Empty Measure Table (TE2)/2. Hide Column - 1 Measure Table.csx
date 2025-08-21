// Tabular Editor Script: 3. Calc Table\2. Create Empty Measure Table (TE2)\2. Hide Column - 1 Measure Table
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

// Third Step: After saving the created table this hides the "value" column
var table = Model.Tables["Measure"];
table.Columns[0].IsHidden = true;
