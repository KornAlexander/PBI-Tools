// Tabular Editor Script: 4. Calc Group\1. Units
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;
using System.Windows.Forms;

// Creates Calculation Group for Units *******************************************************************************
// only sticked with k and mio because billion is not internationally usable. The German "Billion" is not the same as the English "billion"

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

    // Add a new Units Calculation Group 
    try
    {
        var calcGroup = Model.AddCalculationGroup();
        calcGroup.Name = Interaction.InputBox("Provide a name of the Units calculation group table and column Name", "Units Calc Group Names", "Unit"); 
        calcGroup.Columns["Name"].Name = calcGroup.Name;
        // Define calculation item data
        var calculationItemData = new[]
        {
    new { Name = "Thousand", Expression = string.Format("IF(ISNUMBER(SELECTEDMEASURE()),IF(NOT(CONTAINSSTRING(SELECTEDMEASURENAME(), "%") ||CONTAINSSTRING(SELECTEDMEASURENAME(), "ratio")),DIVIDE(SELECTEDMEASURE(), 1000),SELECTEDMEASURE()),SELECTEDMEASURE())") },
    new { Name = "Million", Expression = string.Format("IF(ISNUMBER(SELECTEDMEASURE()),IF(NOT(CONTAINSSTRING(SELECTEDMEASURENAME(), "%") ||CONTAINSSTRING(SELECTEDMEASURENAME(), "ratio")),DIVIDE(SELECTEDMEASURE(), 1000000),SELECTEDMEASURE()),SELECTEDMEASURE())") }
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
        MessageBox.Show("Adding the calc group units was not fully successful, but the rest of the script was completed

Reason: " + ex.Message);
    }


// Formats the DAX of all calculation items in calc groups. Those are not measures ************************************************************
    FormatDax(Model.AllCalculationItems);

