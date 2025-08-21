// Tabular Editor Script: 6. Model Refresh\1. Refresh Whole Model with Calculate
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.142Z

#r "Microsoft.AnalysisServices.Core.dll"
using ToM = Microsoft.AnalysisServices.Tabular;

var refreshType = ToM.RefreshType.Calculate;
Model.Database.TOMDatabase.Model.RequestRefresh(refreshType); 
Model.Database.TOMDatabase.Model.SaveChanges();