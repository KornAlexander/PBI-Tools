// Tabular Editor Script: 6. Other\Document Model\Step 2: Create Relationship and Hide Tables
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

// Make Sure to SAVE THE MODEL after you have added the four info tables
var rel = Model.AddRelationship();
// Assign the "many" side (from _Columns[Table])
rel.FromColumn = Model.Tables["_Columns"].Columns["Table"];
// Assign the "one" side (to _Tables[Table Name])
rel.ToColumn = Model.Tables["_Tables"].Columns["Name"];
// Set the relationship as active
rel.IsActive = true;
// Name the relationship
rel.Name = "Relationship between _Columns and _Tables";

Model.Tables["_Tables"].IsHidden = true;
Model.Tables["_Columns"].IsHidden = true;
Model.Tables["_Relationships"].IsHidden = true;
Model.Tables["_Measure"].IsHidden = true;