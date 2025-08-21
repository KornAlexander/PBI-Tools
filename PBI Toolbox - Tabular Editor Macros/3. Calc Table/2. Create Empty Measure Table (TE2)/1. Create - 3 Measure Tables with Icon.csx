// Tabular Editor Script: 3. Calc Table\2. Create Empty Measure Table (TE2)\1. Create - 3 Measure Tables with Icon
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

// ATTENTION FOR TE2 Users: Hiding of value column is not included, therefore 3 steps as 2 scripts

// First Step: Add Tables
var table = Model.AddCalculatedTable("\U0001F3AFMeasures | 1.\U0001F4C8KPIs", "{0}"); 
var table1 = Model.AddCalculatedTable("\U0001F3AFMeasures | 2. \U00000023\U000020E3 Variables", "{0}");
var table2 = Model.AddCalculatedTable("\U0001F3AFMeasures | 3.\U0001F4CB Titles and Labels", "{0}");

// Second Step: JUST FOR TE2 Save Data Model Changes

// Third Step: Hide the columns "value"