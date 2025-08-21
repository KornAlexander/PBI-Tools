// Tabular Editor Script: 4. Calc Group\3. Time Intelligence with Value Parameter (TE3)
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;
using System.Windows.Forms;

// Define the table name
string tableName = "Parameter 1 to 10";

// Create the calculated table using the DAX expression GENERATESERIES(0, 10, 1)
var calculatedTable = Model.AddCalculatedTable(tableName, "GENERATESERIES(0, 10, 1)");

// Access the 'Value' column created by the GENERATESERIES function
var fieldColumn = calculatedTable.Columns["Value"];

// Set the extended property with the given JSON value on the renamed column
fieldColumn.SetExtendedProperty("ParameterMetadata", "{"version":0}", ExtendedPropertyType.Json);
fieldColumn.SummarizeBy = AggregateFunction.None;

// Add the measure 'Parameter 1 to 10 Value' to the table
string measureName = "Parameter 1 to 10 Value";
string daxExpression = "SELECTEDVALUE('Parameter 1 to 10'[Value], 1)";

// Add the new measure
var measure = calculatedTable.AddMeasure(measureName, daxExpression);

if (!Model.DiscourageImplicitMeasures)
{
    // Show message box
    DialogResult dialogResult14 = MessageBox.Show(
        text: "We saw that discourageImplicitMeasures is not yet set to true.

If you proceed this will automatically disable implicit measures. Disabling implicit measures is generally recommended. 

Would you like to proceed?",
        caption: "Discourage Implicit Measures",
        buttons: MessageBoxButtons.YesNo);

    // If user clicks Yes, set DiscourageImplicitMeasures to true
    if (dialogResult14 == DialogResult.Yes)
    {
        Model.DiscourageImplicitMeasures = true;
    }
    else
    {
        return;
    }
}

// Find the table with the data category "Time"
var CalendarTable = Model.Tables.FirstOrDefault(t => t.DataCategory == "Time");

if (CalendarTable == null)
{
    string dateTableName = Interaction.InputBox("Provide the name of the date dimension table", "Table Name", "Calendar");
    CalendarTable = Model.Tables.FirstOrDefault(t => t.Name == dateTableName);

    if (CalendarTable == null)
    {
        MessageBox.Show("The table you provided does not exist in the model.");
        return;
    }
}

// Checking for Date Column otherwise prompt for user input
string DateColumn = null;

// Check if there is a column in the CalendarTable with IsKey = true
var keyColumn = CalendarTable.Columns.FirstOrDefault(col => col.IsKey == true);
if (keyColumn != null)
{
    DateColumn = keyColumn.Name;
}
else
{
    // If no key column found, prompt the user for input
    DateColumn = Interaction.InputBox("Provide the name of the date column name", "Column Name", "Date");
}

// Creates Calculation Group for Time Intelligence *******************************************************************************
var TimeIntelligenceCalculationGroupName = Interaction.InputBox("Provide the name of the calculation group name", "Time Intelligence Calc Group Name", "Time Intelligence");

DialogResult dialogResult4 = MessageBox.Show(text: "Generate YTD Calc Items?", caption: "Calc Group: YTD", buttons: MessageBoxButtons.YesNo);
bool GenerateYTD = (dialogResult4 == DialogResult.Yes);

// Add a new Time Intelligence Calculation Group **************************************************
try
{
    var calcGroup = Model.AddCalculationGroup();
    calcGroup.Name = TimeIntelligenceCalculationGroupName;
    calcGroup.Columns["Name"].Name = TimeIntelligenceCalculationGroupName;

    // Define calculation item data
    var calculationItemData = new[]
    {
        new { Name = "AC", Expression = "SELECTEDMEASURE()" },
        new { Name = "Y-X", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD('{0}'[{1}], "12/31"), -[Parameter 1 to 10 Value], YEAR), ALL('{0}'))", CalendarTable.Name, DateColumn) },
        GenerateYTD ? new { Name = "YTD", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATESYTD('{0}'[{1}], "12/31"), ALL('{0}'))", CalendarTable.Name, DateColumn) }: null,
        new { Name = "abs. AC vs Y-X", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD('{0}'[{1}], "12/31"), ALL('{0}')) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD('{0}'[{1}], "12/31"), -[Parameter 1 to 10 Value], YEAR), ALL('{0}')) RETURN AC - Y1", CalendarTable.Name, DateColumn) },
        GenerateYTD ? new { Name = "abs. AC vs YTD-X", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD('{0}'[{1}], "12/31"), ALL('{0}')) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD('{0}'[{1}], "12/31"), -[Parameter 1 to 10 Value], YEAR), ALL('{0}')) RETURN AC - Y1", CalendarTable.Name, DateColumn) }: null,
        new { Name = "AC vs Y-X", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD('{0}'[{1}], "12/31"), ALL('{0}')) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD('{0}'[{1}], "12/31"), -[Parameter 1 to 10 Value], YEAR), ALL('{0}')) RETURN DIVIDE(AC - Y1, Y1)", CalendarTable.Name, DateColumn) },
        GenerateYTD ? new { Name = "AC vs YTD-X", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD('{0}'[{1}], "12/31"), ALL('{0}')) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD('{0}'[{1}], "12/31"), -[Parameter 1 to 10 Value], YEAR), ALL('{0}')) RETURN DIVIDE(AC - Y1, Y1)", CalendarTable.Name, DateColumn) }: null,
        new { Name = "achiev. AC vs Y-X", Expression = string.Format("VAR AC = SELECTEDMEASURE() VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD('{0}'[{1}], "12/31"), -[Parameter 1 to 10 Value], YEAR), ALL('{0}')) RETURN 1 - DIVIDE((Y1 - AC), Y1, 0)", CalendarTable.Name, DateColumn) },
        GenerateYTD ? new { Name = "achiev. AC vs YTD-X", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD('{0}'[{1}], "12/31"), ALL('{0}')) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD('{0}'[{1}], "12/31"), -[Parameter 1 to 10 Value], YEAR), ALL('{0}')) RETURN 1 - DIVIDE((Y1 - AC), Y1, 0)", CalendarTable.Name, DateColumn) } : null
    }.Where(item => item != null).ToArray();

    // Add calculation items to the Calculation Group
    foreach (var itemData in calculationItemData)
    {
        var item = calcGroup.AddCalculationItem();
        item.Name = itemData.Name;
        item.Expression = itemData.Expression;

        // Add the format string to "AC vs Y-X" and "AC vs YTD-X"
        if (itemData.Name == "AC vs Y-X" || itemData.Name == "AC vs YTD-X" || itemData.Name == "achiev. AC vs Y-X" || itemData.Name == "achiev. AC vs YTD-X")
        {
            item.FormatStringExpression = ""0.00%""; // Sets the format to percentage with two decimal places
        }
    }
}
catch (Exception ex)
{
    MessageBox.Show("Adding the calc group time intelligence was not fully successful, but the rest of the script was completed

Reason: " + ex.Message);
}

// Formats the DAX of all calculation items in calc groups. Those are not measures ************************************************************
FormatDax(Model.AllCalculationItems);
