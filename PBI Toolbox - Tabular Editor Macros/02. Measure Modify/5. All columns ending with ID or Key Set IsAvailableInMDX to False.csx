// Tabular Editor Script: 2. Measure Modify\5. All columns ending with ID or Key: Set IsAvailableInMDX to False
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z



// Set IsAvailableInMDX to false; ***********************************************************
    foreach (var table in Model.Tables)
    {
        foreach (var column in table.Columns)
        {
            if (column.Name.EndsWith("Key") || column.Name.EndsWith("ID"))
            {
                column.IsAvailableInMDX = false;
            }
        }
    }

