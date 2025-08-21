// Tabular Editor Script: 6. Other\6. Favorites Displayfolder
// Valid Contexts: Measure, Column
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

//Author: Mads Steenstrup Hannibal

string favoriteFolderName = "00. Favorites";

foreach (var measure in Selected.Measures)
{
 string displayFolder = measure.DisplayFolder;

 if(!displayFolder.Contains(favoriteFolderName))
 measure.DisplayFolder = displayFolder + ";" + favoriteFolderName; 
}

foreach (var column in Selected.Columns)
{
 string displayFolder = column.DisplayFolder;

 if(!displayFolder.Contains(favoriteFolderName))
 column.DisplayFolder = displayFolder + ";" + favoriteFolderName; 
}