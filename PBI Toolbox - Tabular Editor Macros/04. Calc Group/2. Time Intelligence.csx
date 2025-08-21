// Tabular Editor Script: 4. Calc Group\2. Time Intelligence
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;
using System.Windows.Forms;

if (!Model.DiscourageImplicitMeasures)
{
    // Show message box
    DialogResult dialogResult14 = MessageBox.Show(
        text: "We saw that discourageImplicitMeasures is not yet set to true.

If you proceed this will automaticaly disable implicit measures. Disabling implicit measures is generally recommended. 

Would you like to proceed?",
        caption: "Discourage Implicit Measures",
        buttons: MessageBoxButtons.YesNo);

    // If user clicks Yes, set DiscourageImplicitMeasures to true
    if (dialogResult14 == DialogResult.No)
    {
        return;
    }
}

// Find the table with the data category "Time"
var CalendarTable = Model.Tables.FirstOrDefault(table => table.DataCategory == "Time");

if (CalendarTable == null)
{
    string tableName = Interaction.InputBox("Provide the name of the date dimension table", "Table Name", "Calendar");
    CalendarTable = Model.Tables.FirstOrDefault(table => table.Name == tableName);

    if (CalendarTable.Name == null)
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
    var TimeIntelligenceCalculationGroupName = Interaction.InputBox("Provide the name of the calculation group name","Time Intelligence Calc Group Name","Time Intelligence");

    DialogResult dialogResult4 = MessageBox.Show(text:"Generate YTD Calc Items?", caption:"Calc Group: YTD", buttons:MessageBoxButtons.YesNo);
    bool GenerateYTD = (dialogResult4 == DialogResult.Yes);            

    // Add a new Time Intellignce Calculation Group **************************************************
    try
    {
        var calcGroup = Model.AddCalculationGroup();
        calcGroup.Name = TimeIntelligenceCalculationGroupName;
        calcGroup.Columns["Name"].Name = TimeIntelligenceCalculationGroupName;
        // Define calculation item data
        var calculationItemData = new[]
        {
        new { Name = "AC", Expression = "SELECTEDMEASURE()" },
        new { Name = "Y-1", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], "12/31"), -1, YEAR), ALL({0}))", CalendarTable.Name, DateColumn) },
        new { Name = "Y-2", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], "12/31"), -2, YEAR), ALL({0}))", CalendarTable.Name, DateColumn) },
        new { Name = "Y-3", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], "12/31"), -3, YEAR), ALL({0}))", CalendarTable.Name, DateColumn) },
        GenerateYTD ? new { Name = "YTD", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], "12/31"), ALL({0}))", CalendarTable.Name, DateColumn) }: null,
        GenerateYTD ? new { Name = "YTD-1", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], "12/31"), -1, YEAR), ALL({0}))", CalendarTable.Name, DateColumn) }: null,
        GenerateYTD ? new { Name = "YTD-2", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], "12/31"), -2, YEAR), ALL({0}))", CalendarTable.Name, DateColumn) }: null,
        new { Name = "abs. AC vs Y-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], "12/31"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], "12/31"), -1, YEAR), ALL({0})) RETURN AC - Y1", CalendarTable.Name, DateColumn) },
        new { Name = "abs. AC vs Y-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], "12/31"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], "12/31"), -2, YEAR), ALL({0})) RETURN AC - Y2", CalendarTable.Name, DateColumn) },
        GenerateYTD ? new { Name = "abs. AC vs YTD-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], "12/31"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], "12/31"), -1, YEAR), ALL({0})) RETURN AC - Y1", CalendarTable.Name, DateColumn) }: null,
        GenerateYTD ? new { Name = "abs. AC vs YTD-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], "12/31"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], "12/31"), -2, YEAR), ALL({0})) RETURN AC - Y2", CalendarTable.Name, DateColumn) }: null,
        new { Name = "AC vs Y-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], "12/31"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), SAMEPERIODLASTYEAR({0}[{1}]), ALL({0})) RETURN DIVIDE(AC - Y1, Y1)", CalendarTable.Name, DateColumn) },
        new { Name = "AC vs Y-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], "12/31"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD({0}[{1}], -2, YEAR), ALL({0})) RETURN DIVIDE(AC - Y2, Y2)", CalendarTable.Name, DateColumn) },
        GenerateYTD ? new { Name = "AC vs YTD-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], "12/31"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], "12/31"), -1, YEAR), ALL({0})) RETURN DIVIDE(AC - Y1, Y1)", CalendarTable.Name, DateColumn) }: null,
        GenerateYTD ? new { Name = "AC vs YTD-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], "12/31"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], "12/31"), -2, YEAR), ALL({0})) RETURN DIVIDE(AC - Y2, Y2)", CalendarTable.Name, DateColumn) }: null,
		new { Name = "achiev. AC vs Y-1", Expression = string.Format("VAR AC = SELECTEDMEASURE() VAR Y1 = CALCULATE(SELECTEDMEASURE(), SAMEPERIODLASTYEAR({0}[{1}]), ALL({0})) RETURN 1 - DIVIDE((Y1 - AC), Y1, 0)", CalendarTable.Name, DateColumn) },
		new { Name = "achiev. AC vs Y-2", Expression = string.Format("VAR AC = SELECTEDMEASURE() VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD({0}[{1}], -2, YEAR), ALL({0})) RETURN 1 - DIVIDE((Y2 - AC), Y2, 0)", CalendarTable.Name, DateColumn) },
		GenerateYTD ? new { Name = "achiev. AC vs YTD-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], "12/31"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], "12/31"), -1, YEAR), ALL({0})) RETURN 1 - DIVIDE((Y1 - AC), Y1, 0)", CalendarTable.Name, DateColumn) } : null,
		GenerateYTD ? new { Name = "achiev. AC vs YTD-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], "12/31"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], "12/31"), -2, YEAR), ALL({0})) RETURN 1 - DIVIDE((Y2 - AC), Y2, 0)", CalendarTable.Name, DateColumn) } : null

    }.Where(item => item != null).ToArray();
        // Add calculation items to the Calculation Group
        foreach (var itemData in calculationItemData)
        {
            var item = calcGroup.AddCalculationItem();
            item.Name = itemData.Name;
            item.Expression = itemData.Expression;
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show("Adding the calc group time intelligence was not fully successful, but the rest of the script was completed

Reason: " + ex.Message);
    }


// Formats the DAX of all calculation items in calc groups. Those are not measures ************************************************************
FormatDax(Model.AllCalculationItems);

