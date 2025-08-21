// Tabular Editor Script: 1. Measure Create\4. Replace Text in Measure
// Valid Contexts: Model, Table, Measure
// Tooltip: This adds for selected Tables all explicit sum measures
// Generated: 2025-08-21T16:54:13.141Z

// Iterate over each measure in the selected measures to replace occurrences of a specific text
foreach (var measure in Selected.Measures)
{
    const string textToReplace = "Text_To_Replace";
    const string replacementText = "Text_Instead";

    // Replace all occurrences of the text in the measure's name
    string updatedName = measure.Name.Replace(textToReplace, replacementText);

    // Update the measure's name
    measure.Name = updatedName;
}

// Optionally, apply other updates or process further
