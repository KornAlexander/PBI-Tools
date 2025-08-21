// Tabular Editor Script: 8. Bernats Repo\2. Calc Table\Duplicate Date Table
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

#r "Microsoft.VisualBasic"
using System.Windows.Forms;

using Microsoft.VisualBasic;
string annotationLabel = "createSecondaryDateTable";
string annotationValue1 = "Main";
string annotationValue2 = "Secondary"; 
Table dateTable = Fx.GetTablesWithAnnotation(Model.Tables, annotationLabel, annotationValue1); 
Table dateTable2 = Fx.GetTablesWithAnnotation(Model.Tables, annotationLabel, annotationValue2);
if (dateTable == null || dateTable2 == null)
{
    IEnumerable<Table> dateTables = Fx.GetDateTables(Model);
    if (dateTables == null) return;
    if (dateTables.Count() != 1)
    {
        dateTable = SelectTable(dateTables, dateTables.First(), "Select Date table to duplicate");
        if (dateTable == null) return;
    }
    else
    {
        dateTable = dateTables.First();
    }
    string dateTable2Name = Fx.GetNameFromUser("Secondary Date Table Name", "Name", dateTable.Name + " comparison");
    dateTable2 = Model.AddCalculatedTable(name: dateTable2Name, expression: dateTable.DaxObjectFullName);
    dateTable2.DataCategory = dateTable.DataCategory;
    var te2 = dateTable2.Columns.Count == 0;
    for (int i = 0; i < dateTable2.Columns.Count(); i++)
    {
        Column c = dateTable.Columns[i];
        Column c2 = te2 ? dateTable2.AddCalculatedColumn(c.Name, String.Format("[Value{0}]", i)) : dateTable2.Columns[i];
    }
    dateTable.SetAnnotation(annotationLabel, annotationValue1);
    dateTable2.SetAnnotation(annotationLabel, annotationValue2);
    Info("Save changes back to the model, recalculate and run again");
}
else
{
    for (int i = 0; i < dateTable2.Columns.Count(); i++)
    {
        Column c = dateTable.Columns[i];
        Column c2 = dateTable2.Columns[i];
        c2.IsKey = c.IsKey;
        c2.SortByColumn = c.SortByColumn;
    }
    IEnumerable<SingleColumnRelationship> dateTableRelatioships =
        Model.Relationships.Where(r => r.FromTable.Name == dateTable.Name
        || r.ToTable.Name == dateTable.Name);
    foreach (SingleColumnRelationship r in dateTableRelatioships)
    {
        SingleColumnRelationship newR = Model.AddRelationship();
        if (r.FromTable.Name == dateTable.Name)
        {
            newR.FromColumn = dateTable2.Columns[r.FromColumn.Name];
            newR.ToColumn = r.ToColumn;
        }else
        {
            newR.ToColumn = dateTable2.Columns[r.ToColumn.Name];
            newR.FromColumn = r.FromColumn;
        }
        newR.FromCardinality = r.FromCardinality;
        newR.ToCardinality = r.ToCardinality;
        newR.CrossFilteringBehavior = r.CrossFilteringBehavior;
        newR.IsActive = r.IsActive; 
    }
    Info("Metadata updated");
}

public static class Fx
{
    public static IEnumerable<Table> GetDateTables(Model model)
    {
        IEnumerable<Table> dateTables = null as IEnumerable<Table>;
        if (model.Tables.Any(t => t.DataCategory == "Time" && t.Columns.Any(c => c.IsKey == true)))
        {
            dateTables = model.Tables.Where(t => t.DataCategory == "Time" && t.Columns.Any(c => c.IsKey == true && c.DataType == DataType.DateTime));
        }
        else
        {
            Error("No date table detected in the model. Please mark your date table(s) as date table");
        }
        return dateTables;
    }
    public static Table GetTablesWithAnnotation(IEnumerable<Table> tables, string annotationLabel, string annotationValue)
    {
        Func<Table, bool> lambda = t => t.GetAnnotation(annotationLabel) == annotationValue;
        IEnumerable<Table> matchTables = GetFilteredTables(tables, lambda);
        if(matchTables == null)
        {
            return null;
        }
        else
        {
            return matchTables.First();
        }
    }
    public static IEnumerable<Table> GetFilteredTables(IEnumerable<Table> tables, Func<Table,bool> lambda)
    {
        if (tables.Any(t => lambda(t)))
        {
            return tables.Where(t => lambda(t));
        }
        else
        {
            return null as IEnumerable<Table>; 
        }
    }
    public static IEnumerable<Column> GetFilteredColumns(IEnumerable<Column> columns, Func<Column,bool> lambda, bool returnAllIfNoneFound = true) 
    {
        if (columns.Any(c => lambda(c)))
        {
            return columns.Where(c => lambda(c));
        }
        else
        {
            if(returnAllIfNoneFound)
            {
                return columns;
            }
            else
            {
                return null as IEnumerable<Column>;
            }
        }
    }
    public static Table CreateCalcTable(Model model, string tableName, string tableExpression)
    {
        if(!model.Tables.Any(t => t.Name == tableName))
        {
            return model.AddCalculatedTable(tableName, tableExpression);
        }
        else
        {
            return model.Tables.Where(t => t.Name == tableName).First();
        }
    }
    public static string GetNameFromUser(string Prompt, string Title ="", string DefaultResponse = "")
    {    
        string response = Interaction.InputBox(Prompt, Title, DefaultResponse, 740, 400);
        return response;
    }
    public static string ChooseString(IList<string> OptionList)
    {
        Func<IList<string>, string, string> SelectString = (IList<string> options, string title) =>
        {
            var form = new Form();
            form.Text = title;
            var buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 30;
            var okButton = new Button() { DialogResult = DialogResult.OK, Text = "OK" };
            var cancelButton = new Button() { DialogResult = DialogResult.Cancel, Text = "Cancel", Left = 80 };
            var listbox = new ListBox();
            listbox.Dock = DockStyle.Fill;
            listbox.Items.AddRange(options.ToArray());
            listbox.SelectedItem = options[0];
            form.Controls.Add(listbox);
            form.Controls.Add(buttonPanel);
            buttonPanel.Controls.Add(okButton);
            buttonPanel.Controls.Add(cancelButton);
            var result = form.ShowDialog();
            if (result == DialogResult.Cancel) return null;
            return listbox.SelectedItem.ToString();
        };
        //let the user select the name of the macro to copy
        String select = SelectString(OptionList, "Choose a macro");
        //check that indeed one macro was selected
        if (select == null)
        {
            Info("You cancelled!");
        }
        return select;
    }
}
