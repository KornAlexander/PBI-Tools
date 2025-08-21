// Tabular Editor Script: 7. Official TE Library\Create M Parameter
// Valid Contexts: Model
// Tooltip: If you want to create a new dynamic M Parameter to use in Power Query queries (M Partitions or Shared Expressions).
// Generated: 2025-08-21T16:54:13.141Z

// This script creates a new M parameter in the 'Shared Expressions' of a model.
//
// Create a new shared expression called "New Parameter"
Model.AddExpression( 
    "New Parameter", 
    @"
""Parameter Text"" meta
[
	IsParameterQuery = true,
	IsParameterQueryRequired = true,
	Type = type text
]"
);

// Provides an output informing how to configure and use the parameter
Info ( 
    "Created a new Shared Expression called 'New Parameter', which is an M Parameter template." + 
    "
------------------------------------------------------
" + 
    "To configure:" +
    "
------------------------------------------------------
    " + 
    "1. Replace the text 'New Parameter' with the desired parameter value
    " +
    "2. Set the data type appropriately
    " +
    "3. Replace any values found in the M partitions with the parameter reference." );
