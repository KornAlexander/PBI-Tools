// Tabular Editor Script: 3. Calc Table\2. Create Empty Measure Table (TE2)\2. Hide Column - 3 Measure Table with Icon
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

// Third Step: After saving the created table this hides the "value" column
var table = Model.Tables["\U0001F3AFMeasures | 1.\U0001F4C8KPIs"];
table.Columns[0].IsHidden = true;

var table1 = Model.Tables["\U0001F3AFMeasures | 2. \U00000023\U000020E3 Variables"];
table1.Columns[0].IsHidden = true;

var table2 = Model.Tables["\U0001F3AFMeasures | 3.\U0001F4CB Titles and Labels"];
table2.Columns[0].IsHidden = true;