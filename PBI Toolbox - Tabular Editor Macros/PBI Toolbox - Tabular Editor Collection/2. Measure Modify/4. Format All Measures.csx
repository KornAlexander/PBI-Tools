// Tabular Editor Script: 2. Measure Modify\4. Format All Measures
// Valid Contexts: Model, Measure
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

// Formats DAX of all Calculation Items of Calculation Groups
FormatDax(Model.AllCalculationItems);

// Formats DAX of all Measures
FormatDax(Model.AllMeasures, shortFormat: true, skipSpaceAfterFunctionName: true);