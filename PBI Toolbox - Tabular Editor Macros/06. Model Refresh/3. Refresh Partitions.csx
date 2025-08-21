// Tabular Editor Script: 6. Model Refresh\3. Refresh Partitions
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.142Z

#r "Microsoft.AnalysisServices.Core.dll"
using ToM = Microsoft.AnalysisServices.Tabular;

var refreshType = ToM.RefreshType.DataOnly;
ToM.SaveOptions so = new ToM.SaveOptions();
//so.MaxParallelism = 10;

foreach (var p in Selected.Partitions)
{
    string tableName = p.Table.Name;
    string partitionName = p.Name;
    Model.Database.TOMDatabase.Model.Tables[tableName].Partitions[partitionName].RequestRefresh(refreshType); 
}

Model.Database.TOMDatabase.Model.SaveChanges(so);