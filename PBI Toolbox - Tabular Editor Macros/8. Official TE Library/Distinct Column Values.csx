// Tabular Editor Script: 7. Official TE Library\Distinct Column Values
// Valid Contexts: Column
// Tooltip: Display the distinct values in a column for quick data profiling and access. Save as a Macro on the column level to have it quickly available.
// Generated: 2025-08-21T16:54:13.141Z

// Construct the DAX expression to get all distinct column values, from the selected column:
var dax = string.Format("ALL({0})", Selected.Column.DaxObjectFullName);

// Evaluate the DAX expression against the connected model:
var result = EvaluateDax(dax);

// Output the DataTable containing the result of the DAX expression:
Output(result);
