// Tabular Editor Script: 7. Official TE Library\Format Numeric Measures
// Valid Contexts: Measure
// Tooltip: Allows you to quickly set default format strings on the measures selected.
// Generated: 2025-08-21T16:54:13.141Z

// This script is meant to format all measures with a default formatstring
foreach (var ms in Selected.Measures) {
//Don't set format string on hidden measures
	if (ms.IsHidden) continue;
// If the format string is empty continue. 
	if (!string.IsNullOrWhiteSpace(ms.FormatString)) continue;
//If the data type is int set a whole number format string
	if (ms.DataType == DataType.Int64) ms.FormatString = "#,##0";
//If the datatype is double or decimal 
	if (ms.DataType == DataType.Double || ms.DataType == DataType.Decimal) {
    //and the name contains # or QTY then set the format string to a whole number
		if (ms.Name.Contains("#")
			|| ms.Name.IndexOf("QTY", StringComparison.OrdinalIgnoreCase) >= 0) ms.FormatString = "#,##0";
		//otherwise set it a decimal format string. 
    else ms.FormatString = "#,##0.00";
	}
}
