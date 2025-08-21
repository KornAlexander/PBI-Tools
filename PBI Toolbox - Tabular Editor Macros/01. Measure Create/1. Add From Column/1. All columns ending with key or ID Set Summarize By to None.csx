// Tabular Editor Script: 1. Measure Create\1. Add From Column\1. All columns ending with key or ID: Set Summarize By to None
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.140Z

// Change SummarizeBy to None for All ID and Key columns ***********************************************************

    foreach (var table in Model.Tables)
    {
        foreach (var column in table.Columns)
        {
            if (column.Name.EndsWith("Key") || column.Name.EndsWith("ID"))
            {
                column.SummarizeBy = AggregateFunction.None;
            }
        }
    }
