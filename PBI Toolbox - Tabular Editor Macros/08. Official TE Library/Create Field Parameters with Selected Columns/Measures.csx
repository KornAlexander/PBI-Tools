// Tabular Editor Script: 7. Official TE Library\Create Field Parameters with Selected Columns\Measures
// Valid Contexts: Column
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

// Before running the script, select the measures or columns that you
// would like to use as field parameters (hold down CTRL to select multiple
// objects). Also, you may change the name of the field parameter table
// below. NOTE: If used against Power BI Desktop, you must enable unsupported
// features under File > Preferences (TE2) or Tools > Preferences (TE3).
var name = "Parameter";

if(Selected.Columns.Count == 0 && Selected.Measures.Count == 0) throw new Exception("No columns or measures selected!");

// Construct the DAX for the calculated table based on the current selection:
var objects = Selected.Columns.Any() ? Selected.Columns.Cast<ITabularTableObject>() : Selected.Measures;
var dax = "{
    " + string.Join(",
    ", objects.Select((c,i) => string.Format("("{0}", NAMEOF('{1}'[{0}]), {2})", c.Name, c.Table.Name, i))) + "
}";

// Add the calculated table to the model:
var table = Model.AddCalculatedTable(name, dax);

// In TE2 columns are not created automatically from a DAX expression, so 
// we will have to add them manually:
var te2 = table.Columns.Count == 0;
var nameColumn = te2 ? table.AddCalculatedTableColumn(name, "[Value1]") : table.Columns["Value1"] as CalculatedTableColumn;
var fieldColumn = te2 ? table.AddCalculatedTableColumn(name + " Fields", "[Value2]") : table.Columns["Value2"] as CalculatedTableColumn;
var orderColumn = te2 ? table.AddCalculatedTableColumn(name + " Order", "[Value3]") : table.Columns["Value3"] as CalculatedTableColumn;

if(!te2) {
    // Rename the columns that were added automatically in TE3:
    nameColumn.IsNameInferred = false;
    nameColumn.Name = name;
    fieldColumn.IsNameInferred = false;
    fieldColumn.Name = name + " Fields";
    orderColumn.IsNameInferred = false;
    orderColumn.Name = name + " Order";
}
// Set remaining properties for field parameters to work
// See: https://twitter.com/markbdi/status/1526558841172893696
nameColumn.SortByColumn = orderColumn;
nameColumn.GroupByColumns.Add(fieldColumn);
fieldColumn.SortByColumn = orderColumn;
fieldColumn.SetExtendedProperty("ParameterMetadata", "{"version":3,"kind":2}", ExtendedPropertyType.Json);
fieldColumn.IsHidden = true;
orderColumn.IsHidden = true;
