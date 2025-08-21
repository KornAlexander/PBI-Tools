// Tabular Editor Script: 6. Other\5. Auto Clean Up Table Into Displayfolders
// Valid Contexts: Measure, Column
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

//Author: David Kofod Hanna
//Modified: Alexander Korn 
//Modification: Calculated Columns extra folder and applies only to selected tables + result box

// Get selected tables from the Tabular Editor interface
var selectedTables = Selected.Tables;

// If no tables are selected, show a message and exit
if (!selectedTables.Any())
{
    Info("No tables selected. Please select one or more tables to modify.");
    return;
}

// Go through each selected table
foreach(var table in selectedTables) 
{
    if(table.Name != "Date")
    {
        // First look at columns
        foreach(var column in table.Columns)
        {
            var keySuffix = "Key";
            var columnDateType = column.DataType.ToString();
            
            // Calculated columns go into their own folder
            if(column.Type == ColumnType.Calculated)
            {
                column.DisplayFolder = "Calculated Columns";
                continue; // Skip other processing for calculated columns
            }
            
            // DWCreatedDate column should be hidden in a separate folder
            if(column.Name == "DWCreatedDate")
            {
                column.DisplayFolder = "Attributes\\Metadata";   
                column.IsHidden = true;   
            }
            
            // Numeric columns should not be aggregated and float (double) data type should not be used
            if(column.DataType == DataType.Double || column.DataType == DataType.Decimal || column.DataType == DataType.Int64)
            {
                column.DisplayFolder = "Numeric";  
                column.SummarizeBy = AggregateFunction.None;
                if(column.DataType == DataType.Double)
                {
                    column.DataType = DataType.Decimal;
                }
            }
            
            // Boolean data types into their own folder
            if(column.DataType == DataType.Boolean)
            {
                column.DisplayFolder = "Flags";  
            }
            
            if(column.DataType == DataType.String)
            {
                column.DisplayFolder = "Attributes";  
            }
            
            // Keys go into their own display folder, should not be aggregated and hidden
            if(column.UsedInRelationships.Any()) 
            {
                column.DisplayFolder = "Key";
                column.SummarizeBy = AggregateFunction.None;
                column.IsHidden = true;
            }
            
            // Date keys get their own folder and other dates go in attributes
            if(columnDateType == "DateTime" && column.Name != "DWCreatedDate")
            {
                if(column.UsedInRelationships.Any()) 
                {
                    column.DisplayFolder = "Key";
                    column.IsHidden = true;
                }
                else
                {
                    column.DisplayFolder = "Dates";
                }
            }
        }
    }
}

// Provide feedback on completion
Info("Process Complete: Column organization completed for " + selectedTables.Count().ToString() + " selected table(s).");