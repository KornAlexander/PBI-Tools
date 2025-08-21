// Tabular Editor Script: 6. Other\2. Check Discourage Implicit Measures
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
        text: "Set DiscourageImplicitMeasures to true?

This is in general recommended and needed for calculation groups.",
        caption: "Discourage Implicit Measures",
        buttons: MessageBoxButtons.YesNo);

    // If user clicks Yes, set DiscourageImplicitMeasures to true
    if (dialogResult14 == DialogResult.Yes)
    {
        Model.DiscourageImplicitMeasures = true;
    }
}
else
{
    // Show message box indicating it is already set to true
    MessageBox.Show(
        text: "DiscourageImplicitMeasures is already set to true.",
        caption: "Discourage Implicit Measures",
        buttons: MessageBoxButtons.OK,
        icon: MessageBoxIcon.Information);
}