// Tabular Editor Script: 9. Model Refresh\2. Refresh Selected Tables
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.142Z

#r "Microsoft.AnalysisServices.Core.dll"
using ToM = Microsoft.AnalysisServices.Tabular;

var refreshType = ToM.RefreshType.DataOnly;
ToM.SaveOptions so = new ToM.SaveOptions();
//so.MaxParallelism = 10;

foreach (var t in Selected.Tables)
{
    string tableName = t.Name;
    Model.Database.TOMDatabase.Model.Tables[tableName].RequestRefresh(refreshType); 
}

Model.Database.TOMDatabase.Model.SaveChanges(so);