// Tabular Editor Script: 8. Bernats Repo\3. Calc Group\Create comparison Calc Group
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.142Z

#r "Microsoft.VisualBasic"
using System.Windows.Forms;

using Microsoft.VisualBasic;
string calcGroupName = Fx.GetNameFromUser("Input name",DefaultResponse: "Model");
string selectedCalcItemName = Fx.GetNameFromUser("Input name", DefaultResponse: "Selected");
string referenceCalcItemName =  Fx.GetNameFromUser("Input name", DefaultResponse: "Reference");
string comparisonCalcItemName =  Fx.GetNameFromUser("Input name", DefaultResponse: "Comparison");
string comparisonPctCalcItemName =  Fx.GetNameFromUser("Input name", DefaultResponse: "Comparison %");
string daysMeasureName =  Fx.GetNameFromUser("Input name", DefaultResponse: "Days Selected");
string referenceDaysRawMeasureName =  Fx.GetNameFromUser("Input name", DefaultResponse: "Days Reference Raw");
string referenceDaysMeasureName =  Fx.GetNameFromUser("Input name", DefaultResponse: "Days Reference");
string daysDifferenceMeasureName =  Fx.GetNameFromUser("Input name", DefaultResponse: "Days Difference");
IEnumerable<Table> dateTables = Fx.GetDateTables(Model);
if (dateTables == null) return;
if (dateTables.Count() < 2)
{
    Error("Less than 2 date tables detected in your model. A minimum of 2 date tables (marked as date tables) are required to run this script");
    return;
}
Table dateTable = SelectTable(tables: dateTables, preselect: dateTables.First(), label: "Select main date table");
if (dateTable == null)
{
    Error("No table selected.");
    return;
}
Func<Column, bool> weekColFunc = c => c.Name.Contains("Week") || c.Name.Contains("Semana");
IEnumerable<Column> dayOfWeekColumns = Fx.GetFilteredColumns(dateTable.Columns, weekColFunc);
//IEnumerable <Column> dayOfWeekColumns = null as IEnumerable<Column>;
//if (dateTable.Columns.Any(c => c.Name.Contains("Week") || c.Name.Contains("Semana")))
//{
//    dayOfWeekColumns = dateTable.Columns.Where(c => c.Name.Contains("Week") || c.Name.Contains("Semana"));
//}
//else
//{
//    dayOfWeekColumns = dateTable.Columns;
//}
Column dayOfWeekColumn = SelectColumn(dayOfWeekColumns, dayOfWeekColumns.First(), label: "Select Day of Week column");
if (dayOfWeekColumn == null) { Error("No column selected"); return; }
Table referenceDateTable = SelectTable(tables: dateTables, preselect: dateTables.Last(), label: "Select reference date table");
if (referenceDateTable == null)
{
    Error("No table selected.");
    return;
}
IEnumerable<Column> referenceDayOfWeekColumns = Fx.GetFilteredColumns(referenceDateTable.Columns, weekColFunc);
Column referenceDayOfWeekColumn = SelectColumn(referenceDayOfWeekColumns, referenceDayOfWeekColumns.First(), label: "Select Day of Week column of reference Date Table");
if (referenceDayOfWeekColumn == null) { Error("No column selected"); return; }
CalculationGroupTable calcGroup = Model.AddCalculationGroup(calcGroupName);
Column calcGroupColumn = calcGroup.Columns[0];
calcGroupColumn.Name = calcGroup.Name;
string selectedCalcItemExpression =
    String.Format(
        @"CALCULATE(
            SELECTEDMEASURE( ),
            REMOVEFILTERS( {0} )
        )",
        referenceDateTable.DaxObjectFullName);
CalculationItem selectedCalcItem = calcGroup.AddCalculationItem(selectedCalcItemName, selectedCalcItemExpression);
selectedCalcItem.FormatDax();
selectedCalcItem.Ordinal = 0;
string referenceCalcItemExpression =
    String.Format(
        @"CALCULATE(
            CALCULATE( SELECTEDMEASURE( ), REMOVEFILTERS( {0} ) ),
            TREATAS(
                VALUES( {1} ),
                {2}
            )
        )",
        dateTable.DaxObjectFullName,
        dayOfWeekColumn.DaxObjectFullName,
        referenceDayOfWeekColumn.DaxObjectFullName
     );
CalculationItem referenceCalcItem = calcGroup.AddCalculationItem(referenceCalcItemName, referenceCalcItemExpression);
referenceCalcItem.FormatDax();
referenceCalcItem.Ordinal = 1;
string comparisonCalcItemExpression =
    String.Format(
        @"VAR _selection =
            CALCULATE(
                SELECTEDMEASURE( ),
                REMOVEFILTERS( {0} )
            )
        VAR _refrence =
            CALCULATE(
                CALCULATE( SELECTEDMEASURE( ),REMOVEFILTERS( {1} )),
                TREATAS(
                    VALUES( {2} ),
                    {3}
                )
            )
        VAR _result =
            IF(
                ISBLANK( _selection ) || ISBLANK( _refrence ),
                BLANK( ),
                _selection - _refrence
            )
        RETURN
            _result",
        referenceDateTable.DaxObjectFullName,
        dateTable.DaxObjectFullName,
        dayOfWeekColumn.DaxObjectFullName,
        referenceDayOfWeekColumn.DaxObjectFullName);
string comparisonCalcItemFormatStringExpression =
    @"VAR _fs = SELECTEDMEASUREFORMATSTRING()
    RETURN ""+"" & _fs & "";-"" & _fs & "";-"" ";
CalculationItem comparisonCalcItem = calcGroup.AddCalculationItem(comparisonCalcItemName, comparisonCalcItemExpression);
comparisonCalcItem.FormatStringExpression = comparisonCalcItemFormatStringExpression;
comparisonCalcItem.FormatDax();
comparisonCalcItem.Ordinal = 2;
string comparisonPctCalcItemExpression =
    String.Format(
        @"VAR _selection =
            CALCULATE(
                SELECTEDMEASURE( ),
                REMOVEFILTERS( {0} )
            )
        VAR _refrence =
            CALCULATE(
                CALCULATE( SELECTEDMEASURE( ),REMOVEFILTERS( {1} ) ),
                TREATAS(
                    VALUES( {2} ),
                    {3}
                )
            )
        VAR _result =
            IF(
                ISBLANK( _selection ) || ISBLANK( _refrence ),
                BLANK( ),
                DIVIDE( _selection - _refrence, _refrence )
            )
        RETURN
            _result",
        referenceDateTable.DaxObjectFullName,
        dateTable.DaxObjectFullName,
        dayOfWeekColumn.DaxObjectFullName,
        referenceDayOfWeekColumn.DaxObjectFullName);
string comparisonPctCalcItemFormatStringExpression = @"""+0 %;-0 %;-""";
CalculationItem comparisonPctCalcItem = calcGroup.AddCalculationItem(comparisonPctCalcItemName, comparisonPctCalcItemExpression);
comparisonPctCalcItem.FormatStringExpression = comparisonPctCalcItemFormatStringExpression;
comparisonPctCalcItem.FormatDax();
comparisonPctCalcItem.Ordinal = 3;
string daysMeasureExpression =
    String.Format(
        @"COUNTROWS({0})",
        dateTable.DaxObjectFullName);
Measure daysMeasure = dateTable.AddMeasure(name: daysMeasureName, expression: daysMeasureExpression);
daysMeasure.FormatString = @"""0"""; 
daysMeasure.FormatDax();
string referenceDaysRawMeasureExpression =
    String.Format(
        @"COUNTROWS({0})",
        referenceDateTable.DaxObjectFullName);
Measure referenceDaysRawMeasure = referenceDateTable.AddMeasure(name: referenceDaysRawMeasureName, expression: referenceDaysRawMeasureExpression);
referenceDaysRawMeasure.FormatString = @"""0""";
referenceDaysRawMeasure.FormatDax();
string referenceDaysMeasureExpression =
    String.Format(
        @"CALCULATE({0},{1}=""{2}"")",
        referenceDaysRawMeasure.DaxObjectFullName,
        calcGroupColumn.DaxObjectFullName,
        referenceCalcItem.Name);
Measure referenceDaysMeasure = referenceDateTable.AddMeasure(name: referenceDaysMeasureName, expression: referenceDaysMeasureExpression);
referenceDaysMeasure.FormatDax();
string daysDifferenceMeasureExpression =
    String.Format(
        @"{0} - {1}",
        daysMeasure.DaxObjectFullName,
        referenceDaysMeasure.DaxObjectFullName);
Measure differenceDaysMeasure = referenceDateTable.AddMeasure(name: daysDifferenceMeasureName, expression: daysDifferenceMeasureExpression);
differenceDaysMeasure.FormatDax();
differenceDaysMeasure.FormatString = @"""0""";

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
