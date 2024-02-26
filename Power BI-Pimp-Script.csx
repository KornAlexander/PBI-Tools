// Author: Alexander Korn
// Created on: 5. December 2023
// Modified on 29. January 2024


// *********Description***********
// Script asks for and adds the following items:
// 1. Add a new Calculation Group for "Time Intelligence" Measures
// 2. Adding a Date Dimension Table, the table is automatically marked as date table
// 2.1 Adding a Function for Date Dimension, credit Lars Schreiber
// 2.2 Adding a calc table for dimension
// 3. Adding an Empty Measure Table
// 4. Adding a Last Refresh Table
// 5. Formats the DAX of ALL calculation items in the model
// 6. All Key and ID columns: Set Summarize By to "None"
// 7. Adds Explicit Measures based on defined aggregation, credit Thomas Martens
// 8. Adding a Calc Group for "Units"
// 9. Checking for DiscourageImplicitMeasures and ask to set to true
// 10. Load BPA into TE - needs TE reopen
// Variables to fill in:
//     - Calc Group Name,
//     - Date Table Name
//     - Date Column Name
//     - If YTD / FY measures shall be created and what the cutoff date for the FY is


#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;
using System.Windows.Forms; 

// *********Variables to Modify from User if needed***********
// Here you can modify the names
string tableNameEmptyMeasure = "Measure";
string tableNameLastRefresh = "Last Refresh";
string calctableCalendar = "Calendar CalcTable";
string columnNameLastRefresh = "Last Refreshes";
string LastRefreshMeasureName = "Last Refresh Measure";
var FromYear = "2018";
var ToYear = "2024"; 
var TimeIntelligenceCalculationGroupName = "Time Intelligence";
var CalcGroupUnitsName = "Units";

// Here you can modify if you need Fiscal Year Time Intelligence Calculation Items
bool GenerateYTD = true;     
bool GenerateFiscalYear = false; 
string fiscalYearEndDate = "07/31";

// do not modify below this line
// Script starts ***********************************************************

//Variables
bool addtables;
bool GenerateEmptyMeasureTable = false;
bool GenerateLastRefreshTable = false;
bool GenerateDateDimensionTable = false;
bool GenerateDateDimensionTable2 = false;
bool GenerateDateDimensionTable3 = false;


// Checking if adding tables is possible otherwise prompt user ****************
// normally this part is just skipped and not visible to the the user
try
{
    // Define the table name
    string tableName = "TestingTable";
    // Create a new table in the model
    Table table = Model.AddTable(tableName);
    // delete the table if adding was successful
    var tableToDelete = Model.Tables[tableName];
    tableToDelete.Delete();
    addtables = true;
}
catch (Exception ex)
{
    addtables = false;
    DialogResult dialogResult = MessageBox.Show(text:"Important Disclaimer: You are executing this script at your own risk!\n\nWarning: Adding tables will not be successful. \n\nInstead of directly opening your model from an open instance of Power BI Desktop please save the model.bim file locally and reopen it afterwards.\n\nYou can still continue to run the script to apply other fixes. \n\nError message: " + ex.Message,caption:"Adding Tables Unsuccessful", buttons:MessageBoxButtons.OKCancel);
        if (dialogResult == DialogResult.Cancel)
    {
        return; // Cancel the script
    }
}

// This is the first prompt to the user *****************************************************************************
if (addtables)
{
    DialogResult dialogResult10 = MessageBox.Show(text:"You are executing this script at your own risk!\n\nPlease make sure to save and reopen the model.bim file from the .pbix. The model.bim from the Power BI Project folder is currently not supported.\n\nThe following parameters can be defined directly within the script: \n-Names for Tables \n-Names for Calc Groups \n-Time Intelligence Details \n\nDo you would like to proceed?", caption:"Important Disclaimer", buttons:MessageBoxButtons.OKCancel);
    if (dialogResult10 == DialogResult.Cancel)
    {
        return; // Cancel the script
    }
    
    DialogResult dialogResult = MessageBox.Show(text:"Add Empty Measure table?\n\nThis will add just an empty table, used as container for all measures with subfolders.", caption:"Empty Measure Table", buttons:MessageBoxButtons.YesNo);
    GenerateEmptyMeasureTable = (dialogResult == DialogResult.Yes);
}


DialogResult dialogResult17 = MessageBox.Show(text:"Set IsAvailableInMDX to False for all all columns ending with 'ID' or 'Key'? \n\nThis is a best practice to reduce memory. Please be aware that those column will not be available in Excel and will not work for DAX calculation like DISCTINCTCOUNT.", caption:"MDXAvailable False", buttons:MessageBoxButtons.YesNo);
bool MagicSauce = (dialogResult17 == DialogResult.Yes); 

DialogResult dialogResult15 = MessageBox.Show(text:"Change the current aggregation to 'none' for all columns ending with 'Key' or 'ID'?\n\nThis is a best practice, but also helpful if you want to add explicit measures for all non key columns in a following step within this script.", caption:"Aggregation Key Columns", buttons:MessageBoxButtons.YesNo);
bool KeyColumnsAggregation = (dialogResult15 == DialogResult.Yes); 

DialogResult dialogResult13 = MessageBox.Show(text:"Add Explicit Measures based on current aggregation?\n\nMake sure to use the correct aggregation for all of your columns. \nProperty name to modify: Summarize By", caption:"Explicit Measure creation", buttons:MessageBoxButtons.YesNo);
bool ExplicitMeasure = (dialogResult13 == DialogResult.Yes); 

// This portion is only run if adding tables was possible **********************************
if (addtables)
{
    DialogResult dialogResult1 = MessageBox.Show(text:"Add Last Refresh table?\n\nThis also adds a measure which you can use in your report to display when the last refresh happend. \nThis simple approach does not work if you have incremental refresh / multiple partitions set up.", caption:"Last Refresh Table", buttons:MessageBoxButtons.YesNo);
    GenerateLastRefreshTable = (dialogResult1 == DialogResult.Yes); 

    DialogResult dialogResult2 = MessageBox.Show(text:"Add Date Dimension table? \n\nThe table is automatically marked as date table. \nBasis is a Power Query Script", caption:"PQ: Calendar Dimension ", buttons:MessageBoxButtons.YesNo);
    GenerateDateDimensionTable = (dialogResult2 == DialogResult.Yes); 
    
    DialogResult dialogResult20 = MessageBox.Show(text:"Add Lars Schreiber's function for a calendar dimension table? \n\nThis adds a function for an ultimate calendar dimension. Just go afterwards to your power query and fill in the parameters for your calendar table. \nDon't forget to mark the table as calendar table afterwards", caption:"Lars' PQ Function: Calendar Dimension", buttons:MessageBoxButtons.YesNo);
    GenerateDateDimensionTable2 = (dialogResult20 == DialogResult.Yes); 
    
}

    DialogResult dialogResult21 = MessageBox.Show(text:"Add Date Dimension with Calculated Table? \n\nThis adds a calculuated table for a date dimension. \n\nWARNING: If you have birthdates or similar very old dates in your data model than this calculated table will cause problems. Make sure to mark the table as calendar table afterwards", caption:"Calculated Table: Calendar Dimension", buttons:MessageBoxButtons.YesNo);
    GenerateDateDimensionTable3 = (dialogResult21 == DialogResult.Yes);

// Getting the names of the calendar table and date column, this is also run if adding tables is not possible, because it is needed for the time intelligence calc group ************
var Table = Interaction.InputBox("Provide the name of the date dimension table","Table Name","Calendar");
var Column = Interaction.InputBox("Provide the name of the date column name","Column Name","Date");

// Asking the user if calc groups and formatting shall be applied ****************************************************************************************
DialogResult dialogResult0 = MessageBox.Show(text:"Add Time Intelligence Calc Group?\n\nThis calculation group will immediately provide the possibility for all measures to have various time intelligence calculation items for example measures like Delta to previous year (Y-1) as absolut, percent and of 100%", caption:"Time Intelligence Calculation Group", buttons:MessageBoxButtons.YesNo);
bool GenerateCalcGroupTimeInt = (dialogResult0 == DialogResult.Yes); 

DialogResult dialogResult12 = MessageBox.Show(text:"Add Units Calculation Group for thousands and millions?\n\nThis simply divides the 'Selectedmeasure()' by 1k and 1mio with 'DIVIDE'.", caption:"Units Calculation Group", buttons:MessageBoxButtons.YesNo);
bool GenerateCalcGroupUnits = (dialogResult12 == DialogResult.Yes); 

DialogResult dialogResult5 = MessageBox.Show(text:"Format ALL calculation items for the calculation groups? This is not formatting measures.", caption:"Format DAX calc items", buttons:MessageBoxButtons.YesNo);
bool FormatDAXCalcItems = (dialogResult5 == DialogResult.Yes); 

DialogResult dialogResult6 = MessageBox.Show(text:"Format ALL DAX measures? \n\nThis is formatting ALL measures, therefore if you apply this script to an existing model and you have a lot of measures this can be a rather big impact.", caption:"Format ALL measures", buttons:MessageBoxButtons.YesNo);
bool FormatDAX = (dialogResult6 == DialogResult.Yes);

DialogResult dialogResult16 = MessageBox.Show(text:"Load BPA into TE? \n\nThis loads initially or updates all rules into Tabular Editor for the Best Practice Analyzer (BPA). The BPA is superpowerful tool to optimize your data model even further.\n\nYou will need to REOPEN Tabular Editor for this part to be applied.", caption:"Load BPA", buttons:MessageBoxButtons.YesNo);
bool LoadBPA = (dialogResult16 == DialogResult.Yes);


// This portion is skipped if DiscourageImplicitMeasure is already set to true, otherwise the user is asked to change it*************************************************************
// This being set to true is needed for calculation groups to work.

if (!Model.DiscourageImplicitMeasures)
{
    // Show message box
    DialogResult dialogResult14 = MessageBox.Show(
        text: "Set DiscourageImplicitMeasures to true?\n\nThis is in general recommended and needed for calculation groups.", 
        caption: "Discourage Implicit Measures", 
        buttons: MessageBoxButtons.YesNo);

    // If user clicks Yes, set DiscourageImplicitMeasures to true
    if (dialogResult14 == DialogResult.Yes)
    {
        Model.DiscourageImplicitMeasures = true;
    }
}

// Show the result of all of the previous selection *************************************************************************************************************************
{
    DialogResult dialogResult11 = MessageBox.Show(text:
    "Tables:"+
    "\n1. Create Last Refresh Table: "+GenerateLastRefreshTable+
    "\n   with name: "+tableNameLastRefresh+
    "\n2. Create Empty Measure Table: "+GenerateEmptyMeasureTable+
    "\n   with name: "+tableNameEmptyMeasure+
    "\n3. Create Date Dimension Table: "+GenerateDateDimensionTable+
    "\n  3.1 Date Dimension Table Name: '"+Table+"'"+
    "\n  3.2 Date Dimension Column Name: '"+Column+"'"+
    "\n  3.3 CalendarFunction added: "+GenerateDateDimensionTable2+
    "\n  3.4 Calendar Calc Table added: "+GenerateDateDimensionTable3+
    
    "\n\nCalculation Groups:"+
    "\n4. Time Intelligence Calc Group: "+GenerateCalcGroupTimeInt+
    "\n   with name: "+TimeIntelligenceCalculationGroupName+
    "\n  4.1 YTD items: "+GenerateYTD+
    "\n  4.2 FiscalYear items: "+GenerateFiscalYear+
    "\n  4.3 FiscalYear cutoff date: "+fiscalYearEndDate+
    "\n5. Units Calc Group: "+GenerateCalcGroupUnits+
    "\n   with name: "+CalcGroupUnitsName+
    
    "\n\nMeasures:"+
    "\n6. Create all Explicit Measures: "+ExplicitMeasure+
    "\n7. Format all Measures: "+FormatDAX+
    "\n8. Format all Calculation Items: "+FormatDAXCalcItems+
    
    "\n\nOther:"+
    "\n9. Remove Aggregation for Key Columns: "+KeyColumnsAggregation+
    "\n10. Set AVAILABLEMDX to False: "+MagicSauce+
    "\n11. Load BPA: "+LoadBPA
    
    
    ,caption:"Summary of Selected Parameters", buttons:MessageBoxButtons.OKCancel);
if (dialogResult11 == DialogResult.Cancel)
    {
        return; // Cancel the script
    }}

    
// Adding an empty measure table *********************************************************** 
if (GenerateEmptyMeasureTable)
        {
            try {

    // Create a new table in the model
    Table table = Model.AddTable(tableNameEmptyMeasure);
    // Add the "Name of Measure" column to the table
    DataColumn column1 = table.AddDataColumn();
    column1.Name = "Name of Measure";
    column1.DataType = DataType.String;
    column1.SourceColumn = "Name of Measure";
    column1.IsHidden = true; // Hide the column
    column1.SummarizeBy = AggregateFunction.None;
    // Add the "Description" column to the table
    DataColumn column2 = table.AddDataColumn();
    column2.Name = "Description";
    column2.DataType = DataType.String;
    column2.SourceColumn = "Description";
    column2.IsHidden = true; // Hide the column
    column2.SummarizeBy = AggregateFunction.None;

    if (!Model.Tables.Any(t => t.Name == tableNameEmptyMeasure))
    {
        throw new InvalidOperationException("Empty measure table does not exist in the model.");
    }

                string mExpression = @"
    let
        Source = Table.FromRows(Json.Document(Binary.Decompress(Binary.FromText(""i44FAA=="", BinaryEncoding.Base64), Compression.Deflate)), let _t = ((type nullable text) meta [Serialized.Text = true]) in type table [#""Name of Measure"" = _t, Description = _t])
    in
        Source";


        // Update existing partition
    var partition = table.Partitions.First();
    partition.Expression = mExpression;
            partition.Mode = ModeType.Import; // Set the refresh policy to Import
    }
    catch (Exception ex)
    {MessageBox.Show("Adding Empty Measure table was not successful but the rest of the script was completed\n\nReason: "+ex.Message);
        }
    }


// Change SummarizeBy to None for All ID and Key columns ***********************************************************
if (KeyColumnsAggregation)
    {
    foreach (var table in Model.Tables)
    {
        foreach (var column in table.Columns)
        {
            if (column.Name.EndsWith("Key") || column.Name.EndsWith("ID"))
            {
                column.SummarizeBy = AggregateFunction.None;
            }
        }
    }
    }

// Set IsAvailableInMDX to false; ***********************************************************
if (MagicSauce)
    {
    foreach (var table in Model.Tables)
    {
        foreach (var column in table.Columns)
        {
            if (column.Name.EndsWith("Key") || column.Name.EndsWith("ID"))
            {
                column.IsAvailableInMDX = false;
            }
        }
    }
    }

// Create Explicit Measures for all tables for all columns with summarize by in the empty measure folder. ******************
if (ExplicitMeasure)
   {
// Title: Auto-create explicit measures from all columns in all tables that have qualifying aggregation functions assigned 
// Author of this part: Tom Martens, twitter.com/tommartens68
// Edited on 24/01/04 by: Alexander Korn (e.g. moving all measures to the empty measure table, creating one if has not been previously created, hiding all columns) 
//  
// This script, when executed, will loop through all the tables and creates explicit measure for all the columns with qualifying
// aggregation functions.
// The qualifying aggregation functions are SUM, COUNT, MIN, MAX, AVERAGE.
// This script can create a lot of measures, as by default the aggregation function for columns with a numeric data type is SUM.
// So, it is a good idea to check all columns for the proper aggregation type, e.g. the aggregation type of id columns 
// should be set to None, as it does not make any sense to aggregate id columns.
// An annotation:CreatedThrough is created with a value:CreateExplicitMeasures this will help to identify the measures created
// using this script.
// What is missing, the list below shows what might be coming in subsequent iterations of the script:
// - the base column property hidden is not set to true
// - no black list is used to prevent the creation of unwanted measures

// ***************************************************************************************************************
//the following variables are allowing controling the script
var overwriteExistingMeasures = 0; // 1 overwrites existing measures, 0 preserves existing measures

var measureNameTemplate = "{0} ({1}) ";
//"{0} ({1}) - {2}"; // String.Format is used to create the measure name. 
//{0} will be replaced with the columnname (c.Name), {1} will be replaced with the aggregation function, and last but not least
//{2} will be replaced with the tablename (t.Name). Using t.Name is necessary to create a distinction between measure names if
//columns with the same name exist in different tables.
//Assuming the column name inside the table "Fact Sale" is "Sales revenue" and the aggregation function is SUM 
//the measure name will be: "Sales revenue (Sum) - Fact Sale"

//store aggregation function that qualify for measure creation to the hashset aggFunctions
var aggFunctions = new HashSet<AggregateFunction>{
    AggregateFunction.Default, //remove this line, if you do not want to mess up your measures list by automatically created measures for all the columns that have the Default AggregateFunction assigned
    AggregateFunction.Sum,
    AggregateFunction.Count,
    AggregateFunction.Min,
    AggregateFunction.Max,
    AggregateFunction.Average
};

//You have to be aware that by default this script will just create measures using the aggregate functions "Sum" or "Count" if
//the column has the aggregate function AggregateFunction.Default assigned, this is checked further down below.
//Also, if a column has the Default AggregateFunction assigned and is of the DataType
//DataType.Automatic, DataType.Unknown, or DataType.Variant, no measure is created automatically, this is checked further down below.
//dictDataTypeAggregateFunction = new Dictionary<DataType, string>();
//see this article for all the available data types: https://docs.microsoft.com/en-us/dotnet/api/microsoft.analysisservices.tabular.datatype?view=analysisservices-dotnet
//Of course you can change the aggregation function that will be used for different data types,
//as long as you are using "Sum" and "Count"
//Please be careful, if you change the aggregation function you might end up with multiplemeasures
var dictDataTypeAggregateFunction = new Dictionary<DataType, AggregateFunction>();
dictDataTypeAggregateFunction.Add( DataType.Binary , AggregateFunction.Count ); //adding a key/value pair(s) to the dictionary using the Add() method
dictDataTypeAggregateFunction.Add( DataType.Boolean , AggregateFunction.Count );
dictDataTypeAggregateFunction.Add( DataType.DateTime , AggregateFunction.Count );
dictDataTypeAggregateFunction.Add( DataType.Decimal , AggregateFunction.Sum );
dictDataTypeAggregateFunction.Add( DataType.Double , AggregateFunction.Sum );
dictDataTypeAggregateFunction.Add( DataType.Int64 , AggregateFunction.Sum );
dictDataTypeAggregateFunction.Add( DataType.String , AggregateFunction.Count );

// ***************************************************************************************************************
//all the stuff below this line should not be altered 
//of course this is not valid if you have to fix my errors, make the code more efficient, 
//or you have a thorough understanding of what you are doing

//store all the existing measures to the list listOfMeasures
var listOfMeasures = new List<string>();
foreach( var m in Model.AllMeasures ) {
    listOfMeasures.Add( m.Name );
}

// Check if the "Measure" table exists, if not, create it
Table measureTable;
if (!Model.Tables.Any(t => t.Name == tableNameEmptyMeasure)) {
    measureTable = Model.AddTable(tableNameEmptyMeasure);
} else {
    measureTable = Model.Tables.First(t => t.Name == tableNameEmptyMeasure);
}

//loop across all tables
foreach( var t in Model.Tables ) {
    
    //loop across all columns of the current table t
    foreach( var c in t.Columns ) {
        
        var currAggFunction = c.SummarizeBy; //cache the aggregation function of the current column c
        var useAggFunction = AggregateFunction.Sum;
        var theMeasureName = ""; // Name of the new Measure
        var posInListOfMeasures = 0; //check if the new measure already exists <> -1
        
        if( aggFunctions.Contains(currAggFunction) ) //check if the current aggregation function qualifies for measure aggregation
        {
            //check if the current aggregation function is Default
            if( currAggFunction == AggregateFunction.Default )
            {
                // check if the datatype of the column is considered for measure creation
                if( dictDataTypeAggregateFunction.ContainsKey( c.DataType ) )
                {
                    
                    //some kind of sanity check
                    if( c.DataType == DataType.Automatic || c.DataType == DataType.Unknown || c.DataType == DataType.Variant )
                    {
                        Output("No measure will be created for columns with the data type: " + c.DataType.ToString() + " (" + c.DaxObjectFullName + ")");
                        continue; //moves to the next item in the foreach loop, the next colum in the current table
                    }
                  
                    //cache the aggregation function from the dictDataTypeAggregateFunction
                    useAggFunction = dictDataTypeAggregateFunction[ c.DataType ];
                    
                    //some kind of sanity check
                    if( useAggFunction != AggregateFunction.Count && useAggFunction != AggregateFunction.Sum ) 
                    {    
                        Output("No measure will be created for the column: " + c.DaxObjectFullName);
                        continue; //moves to the next item in the foreach loop, the next colum in the current table
                    }
                    theMeasureName = String.Format( measureNameTemplate , c.Name , useAggFunction.ToString() , t.Name ); // Name of the new Measure
                    posInListOfMeasures = listOfMeasures.IndexOf( theMeasureName ); //check if the new measure already exists <> -1
                    
                } else {
                   
                    continue; //moves to the next item in the foreach loop, the next colum in the current table
                }
                        
            } else {
                
                useAggFunction = currAggFunction;    
                theMeasureName = String.Format( measureNameTemplate , c.Name , useAggFunction.ToString() , t.Name ); // Name of the new Measure
                posInListOfMeasures = listOfMeasures.IndexOf( theMeasureName ); //check if the new measure already exists <> -1
            }
            
            //sanity check
            if(theMeasureName == "")
            {
                continue; //moves to the next item in the foreach loop, the next colum in the current table
            }
            
            // create the measure
            if( ( posInListOfMeasures == -1 || overwriteExistingMeasures == 1 )) 
            {    
                if( overwriteExistingMeasures == 1 ) 
                {
                    foreach( var m in Model.AllMeasures.Where( m => m.Name == theMeasureName ).ToList() ) 
                    {
                        m.Delete();
                    }
                }
                
                var newMeasure = measureTable.AddMeasure
                (
                    theMeasureName                                                                      // Name of the new Measure
                    , "" + useAggFunction.ToString().ToUpper() + "(" + c.DaxObjectFullName + ")"        // DAX expression
                    , t.DaxObjectFullName.Replace("'", "")+c.DisplayFolder
                );
                
                c.IsHidden = true;
                newMeasure.SetAnnotation( "CreatedThrough" , "CreateExplicitMeasures" ); // flag the measures created through this script
                
            }
        }    
    } }       
}

// Creates Calculation Group for Units *******************************************************************************
// only sticked with k and mio because billion in English is trillion in other languages such as German
    
if (GenerateCalcGroupUnits)
    {
// Add a new Units Calculation Group 
try{
var calcGroup = Model.AddCalculationGroup();
calcGroup.Name = CalcGroupUnitsName;
calcGroup.Columns["Name"].Name = CalcGroupUnitsName;
// Define calculation item data
var calculationItemData = new[]
{
    new { Name = "number", Expression = "SELECTEDMEASURE()" },
    new { Name = "k", Expression = "DIVIDE(SELECTEDMEASURE(), 1000)" },
    new { Name = "mio", Expression = "DIVIDE(SELECTEDMEASURE(), 1000000)" }
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
{MessageBox.Show("Adding the calc group units was not fully successful, but the rest of the script was completed\n\nReason: "+ex.Message);
    }

    }
    

// Creates Calculation Group for Time Intelligence *******************************************************************************
if (GenerateCalcGroupTimeInt)
        {
    /* Uncomment here if you want input boxes for the following four variables, already defined all the way at the beginning as text within this script
    var TimeIntelligenceCalculationGroupName = Interaction.InputBox("Provide the name of the calculation group name","Calc group","Time Intelligence");

    DialogResult dialogResult3 = MessageBox.Show(text:"Generate Fiscal Year Calc Items?", caption:"Calc Group: Fiscal Year", buttons:MessageBoxButtons.YesNo);
    bool GenerateFiscalYear = (dialogResult3 == DialogResult.Yes);

    if (GenerateFiscalYear)
    {fiscalYearEndDate = Interaction.InputBox("Enter the fiscal year end date (MM/DD):", "Fiscal Year End Date", fiscalYearEndDate);}

    DialogResult dialogResult4 = MessageBox.Show(text:"Generate YTD Calc Items?", caption:"Calc Group: YTD", buttons:MessageBoxButtons.YesNo);
    bool GenerateYTD = (dialogResult4 == DialogResult.Yes);            
    */

    // Add a new Time Intellignce Calculation Group **************************************************
    try{
    var calcGroup = Model.AddCalculationGroup();
    calcGroup.Name = TimeIntelligenceCalculationGroupName;
    calcGroup.Columns["Name"].Name = TimeIntelligenceCalculationGroupName; 
    // Define calculation item data
    var calculationItemData = new[]
    {
        new { Name = "AC", Expression = "SELECTEDMEASURE()" },
        new { Name = "Y-1", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -1, YEAR), ALL({0}))", Table, Column) },
        new { Name = "Y-2", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -2, YEAR), ALL({0}))", Table, Column) },
        new { Name = "Y-3", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -3, YEAR), ALL({0}))", Table, Column) },
        GenerateYTD ? new { Name = "YTD", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0}))", Table, Column) }: null,
        GenerateYTD ? new { Name = "YTD-1", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -1, YEAR), ALL({0}))", Table, Column) }: null,
        GenerateYTD ? new { Name = "YTD-2", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -2, YEAR), ALL({0}))", Table, Column) }: null,
        new { Name = "abs. AC vs Y-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -1, YEAR), ALL({0})) RETURN AC - Y1", Table, Column) },
        new { Name = "abs. AC vs Y-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -2, YEAR), ALL({0})) RETURN AC - Y2", Table, Column) },
        GenerateYTD ? new { Name = "abs. AC vs YTD-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -1, YEAR), ALL({0})) RETURN AC - Y1", Table, Column) }: null,
        GenerateYTD ? new { Name = "abs. AC vs YTD-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -2, YEAR), ALL({0})) RETURN AC - Y2", Table, Column) }: null,
        new { Name = "AC vs Y-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), SAMEPERIODLASTYEAR({0}[{1}]), ALL({0})) RETURN DIVIDE(AC - Y1, Y1)", Table, Column) },
        new { Name = "AC vs Y-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD({0}[{1}], -2, YEAR), ALL({0})) RETURN DIVIDE(AC - Y2, Y2)", Table, Column) },
        GenerateYTD ? new { Name = "AC vs YTD-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -1, YEAR), ALL({0})) RETURN DIVIDE(AC - Y1, Y1)", Table, Column) }: null,
        GenerateYTD ? new { Name = "AC vs YTD-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -2, YEAR), ALL({0})) RETURN DIVIDE(AC - Y2, Y2)", Table, Column) }: null,
        new { Name = "achiev. AC vs Y-1", Expression = string.Format("VAR AC = SELECTEDMEASURE() VAR Y1 = CALCULATE(SELECTEDMEASURE(), SAMEPERIODLASTYEAR({0}[{1}]), ALL({0})) RETURN 1 - ( ( IFERROR( ( Y1 - AC ), 0 ) / Y1 ) )", Table, Column) },
        new { Name = "achiev. AC vs Y-2", Expression = string.Format("VAR AC = SELECTEDMEASURE() VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD({0}[{1}], -2, YEAR), ALL({0})) RETURN 1 - ( ( IFERROR( ( Y2 - AC ), 0 ) / Y2 ) )", Table, Column) },
        GenerateYTD ? new { Name = "achiev. AC vs YTD-1", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -1, YEAR), ALL({0})) RETURN 1 - ( ( IFERROR( ( Y1 - AC ), 0 ) / Y1 ) )", Table, Column) }: null,
        GenerateYTD ? new { Name = "achiev. AC vs YTD-2", Expression = string.Format("VAR AC = TOTALYTD(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"12/31\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"12/31\"), -2, YEAR), ALL({0})) RETURN 1 - ( ( IFERROR( ( Y2 - AC ), 0 ) / Y2 ) )", Table, Column) }: null,
        GenerateFiscalYear ? new { Name = "FYTD", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0}))", Table, Column, fiscalYearEndDate) } : null,
        GenerateFiscalYear ? new { Name = "FYTD-1", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -1, YEAR), ALL({0}))", Table, Column, fiscalYearEndDate) } : null,
        GenerateFiscalYear ? new { Name = "FYTD-2", Expression = string.Format("CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -2, YEAR), ALL({0}))", Table, Column, fiscalYearEndDate) } : null,
        GenerateFiscalYear ? new { Name = "abs. AC vs FYTD-1", Expression = string.Format("VAR AC = CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -1, YEAR), ALL({0})) RETURN AC - Y1", Table, Column, fiscalYearEndDate) } : null,
        GenerateFiscalYear ? new { Name = "abs. AC vs FYTD-2", Expression = string.Format("VAR AC = CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -2, YEAR), ALL({0})) RETURN AC - Y2", Table, Column, fiscalYearEndDate) } : null,
        GenerateFiscalYear ? new { Name = "AC vs FYTD-1", Expression = string.Format("VAR AC = CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -1, YEAR), ALL({0})) RETURN DIVIDE(AC - Y1, Y1)", Table, Column, fiscalYearEndDate) } : null,
        GenerateFiscalYear ? new { Name = "AC vs FYTD-2", Expression = string.Format("VAR AC = CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -2, YEAR), ALL({0})) RETURN DIVIDE(AC - Y2, Y2)", Table, Column, fiscalYearEndDate) } : null,
        GenerateFiscalYear ? new { Name = "achiev. AC vs FYTD-1", Expression = string.Format("VAR AC = CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0})) VAR Y1 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -1, YEAR), ALL({0})) RETURN 1 - ( ( IFERROR( ( Y1 - AC ), 0 ) / Y1 ) )", Table, Column, fiscalYearEndDate) } : null,
        GenerateFiscalYear ? new { Name = "achiev. AC vs FYTD-2", Expression = string.Format("VAR AC = CALCULATE(SELECTEDMEASURE(), DATESYTD({0}[{1}], \"{2}\"), ALL({0})) VAR Y2 = CALCULATE(SELECTEDMEASURE(), DATEADD(DATESYTD({0}[{1}], \"{2}\"), -2, YEAR), ALL({0})) RETURN 1 - ( ( IFERROR( ( Y2 - AC ), 0 ) / Y2 ) )", Table, Column, fiscalYearEndDate) } : null
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
    {MessageBox.Show("Adding the calc group time intelligence was not fully successful, but the rest of the script was completed\n\nReason: "+ex.Message);
        }  
    }

// Formats the DAX of all calculation items in calc groups. Those are not measures ************************************************************
if (FormatDAXCalcItems)
    {
    // DAX Formatting all Measures
    FormatDax(Model.AllCalculationItems);
    }

// Formats the DAX of all measures.  *************************************************************************************************************************
if (FormatDAX)
    {
    // DAX Formatting all Measures
    FormatDax(Model.AllMeasures);
    }


// Creates a last Refresh Table *******************************************************************************************************
    if (GenerateLastRefreshTable)
        {
            try {
    // Script adds a Last Refresh Table:
    // Create a new table in the model
    Table table = Model.AddTable(tableNameLastRefresh);

    string measureDax = "\"Last Refresh: \" & MAX('" + tableNameLastRefresh + "'[" + columnNameLastRefresh + "])";


    // Add the "Column1" column to the table
    DataColumn column1 = table.AddDataColumn();
    column1.Name = "Column1";
    column1.DataType = DataType.String;
    column1.SourceColumn = "Column1";
    column1.IsHidden = true; // Hide the column
    // Check if the table exists in the model
    if (!Model.Tables.Any(t => t.Name == tableNameLastRefresh))
    {
        throw new InvalidOperationException("Table Last Refresh does not exist in the model.");
    }
    // Define the M expression
    string mExpression = @"
    let
    #""Today"" = #table({""" + columnNameLastRefresh + @"""}, {{DateTime.From(DateTime.LocalNow())}})
    in
        #""Today"" ";
            // Update existing partition
            var partition = table.Partitions.First();
            partition.Expression = mExpression;
            partition.Mode = ModeType.Import; // Set the refresh policy to Import
            
      // Creates a last Refresh Measure ****************************************************************************
      var table2 = Model.Tables[tableNameEmptyMeasure];
      var measurelastrefresh = table2.AddMeasure(LastRefreshMeasureName, measureDax, "Meta");
    }
    catch (Exception ex)
    {MessageBox.Show("Adding the Last Refresh table was not successful but the rest of the script was completed\n\nReason: "+ex.Message);
        }
    }


// Creates a Date Dimension table *******************************************************************************
if (GenerateDateDimensionTable)
        {
            try {
    // Script adds a Date Dimension Table:
    // Create a new table in the model
    Table table = Model.AddTable(Table);
    table.DataCategory = "Time";
    // Add columns with specified names and data types, including SourceColumn
    DataColumn dateColumn = table.AddDataColumn();
    dateColumn.Name = Column;
    dateColumn.DataType = DataType.DateTime;
    dateColumn.IsKey = true;
    dateColumn.SourceColumn = "Date";
    DataColumn yearColumn = table.AddDataColumn();
    yearColumn.Name = "Year";
    yearColumn.DataType = DataType.Int64;
    yearColumn.SourceColumn = "Year";
    DataColumn monthColumn = table.AddDataColumn();
    monthColumn.Name = "Month";
    monthColumn.DataType = DataType.Int64;
    monthColumn.SourceColumn = "Month";
    DataColumn dayColumn = table.AddDataColumn();
    dayColumn.Name = "Day";
    dayColumn.DataType = DataType.Int64;
    dayColumn.SourceColumn = "Day";
    DataColumn dayNameColumn = table.AddDataColumn();
    dayNameColumn.Name = "DayName";
    dayNameColumn.DataType = DataType.String;
    dayNameColumn.SourceColumn = "DayName";
    DataColumn monthNameColumn = table.AddDataColumn();
    monthNameColumn.Name = "MonthName";
    monthNameColumn.DataType = DataType.String;
    monthNameColumn.SourceColumn = "MonthName";
    DataColumn quarterColumn = table.AddDataColumn();
    quarterColumn.Name = "Quarter";
    quarterColumn.DataType = DataType.Int64;
    quarterColumn.SourceColumn = "Quarter";
    DataColumn weekOfYearColumn = table.AddDataColumn();
    weekOfYearColumn.Name = "WeekOfYear";
    weekOfYearColumn.DataType = DataType.Int64;
    weekOfYearColumn.SourceColumn = "WeekOfYear";
    DataColumn yearMonthColumn = table.AddDataColumn();
    yearMonthColumn.Name = "YearMonth";
    yearMonthColumn.DataType = DataType.String;
    yearMonthColumn.SourceColumn = "YearMonth";
    DataColumn yearMonthCodeColumn = table.AddDataColumn();
    yearMonthCodeColumn.Name = "YearMonth Code";
    yearMonthCodeColumn.DataType = DataType.String;
    yearMonthCodeColumn.SourceColumn = "YearMonth Code";
    
    // Check if the table exists in the model
    if (!Model.Tables.Any(t => t.Name == Table))
    {
        throw new InvalidOperationException("Table Date Dimension does not exist in the model.");
    }
    // Define the M expression
    string mExpression = @"
    let
        // configurations start
        Today=Date.From(DateTime.LocalNow()), // today's date
        FromYear =" + FromYear + @", // set the start year of the date dimension. dates start from 1st of January of this year
        ToYear=" + ToYear + @", // set the end year of the date dimension. dates end at 31st of December of this year
        StartofFiscalYear=7, // set the month number that is start of the financial year. example; if fiscal year start is July, value is 7
        firstDayofWeek=Day.Monday, // set the week's start day, values: Day.Monday, Day.Sunday....
        // configuration end
        FromDate=#date(FromYear,1,1),
        ToDate=#date(ToYear,12,31),
        Source=List.Dates(
            FromDate,
            Duration.Days(ToDate-FromDate)+1,
            #duration(1,0,0,0)
        ),
        #""Converted to Table"" = Table.FromList(Source, Splitter.SplitByNothing(), null, null, ExtraValues.Error),
        #""Renamed Columns"" = Table.RenameColumns(#""Converted to Table"",{{""Column1"", ""Date""}}),
        #""Changed Type"" = Table.TransformColumnTypes(#""Renamed Columns"",{{""Date"", type date}}),
        #""Added Custom"" = Table.AddColumn(#""Changed Type"", ""Custom"", each [
            Year = Date.Year([Date]),
            StartOfYear = Date.StartOfYear([Date]),
            EndOfYear = Date.EndOfYear([Date]),
            Month = Date.Month([Date]),
            StartOfMonth = Date.StartOfMonth([Date]),
            EndOfMonth = Date.EndOfMonth([Date]),
            DaysInMonth = Date.DaysInMonth([Date]),
            Day = Date.Day([Date]),
            DayName = Date.DayOfWeekName([Date]),
            DayOfWeek = Date.DayOfWeek([Date], firstDayofWeek),
            DayOfYear = Date.DayOfYear([Date]),
            MonthName = Date.MonthName([Date]),
            Quarter = Date.QuarterOfYear([Date]),
            StartOfQuarter = Date.StartOfQuarter([Date]),
            EndOfQuarter = Date.EndOfQuarter([Date]),
            WeekOfYear = Date.WeekOfYear([Date], firstDayofWeek),
            WeekOfMonth = Date.WeekOfMonth([Date], firstDayofWeek),
            StartOfWeek = Date.StartOfWeek([Date], firstDayofWeek),
            EndOfWeek = Date.EndOfWeek([Date], firstDayofWeek)
        ]),
        #""Expanded Custom"" = Table.ExpandRecordColumn(#""Added Custom"", ""Custom"", {""Year"", ""StartOfYear"", ""EndOfYear"", ""Month"", ""StartOfMonth"", ""EndOfMonth"", ""DaysInMonth"", ""Day"", ""DayName"", ""DayOfWeek"", ""DayOfYear"", ""MonthName"", ""Quarter"", ""StartOfQuarter"", ""EndOfQuarter"", ""WeekOfYear"", ""WeekOfMonth"", ""StartOfWeek"", ""EndOfWeek""}, {""Year"", ""StartOfYear"", ""EndOfYear"", ""Month"", ""StartOfMonth"", ""EndOfMonth"", ""DaysInMonth"", ""Day"", ""DayName"", ""DayOfWeek"", ""DayOfYear"", ""MonthName"", ""Quarter"", ""StartOfQuarter"", ""EndOfQuarter"", ""WeekOfYear"", ""WeekOfMonth"", ""StartOfWeek"", ""EndOfWeek""}),
        FiscalMonthBaseIndex=13-StartofFiscalYear,
        adjustedFiscalMonthBaseIndex=if(FiscalMonthBaseIndex>=12 or FiscalMonthBaseIndex<0) then 0 else FiscalMonthBaseIndex,
        #""Added CustomA"" = Table.AddColumn(#""Expanded Custom"", ""FiscalBaseDate"", each Date.AddMonths([Date],adjustedFiscalMonthBaseIndex)),
        #""Changed Type2"" = Table.TransformColumnTypes(#""Added CustomA"",{{""FiscalBaseDate"", type date}}),
        #""Added CustomB"" = Table.AddColumn(#""Changed Type2"", ""Custom2"", each [
            Fiscal Year = Date.Year([FiscalBaseDate]),
            Fiscal Quarter = Date.QuarterOfYear([FiscalBaseDate]),
            Fiscal Month = Date.Month([FiscalBaseDate]),
            YearMonth = Date.ToText([Date], ""yyyy MMM""),
            YearMonth Code = Date.ToText([Date], ""yyyyMM"")
        ]),
        #""Expanded Custom2"" = Table.ExpandRecordColumn(#""Added CustomB"", ""Custom2"", {""Fiscal Year"", ""Fiscal Quarter"", ""Fiscal Month"", ""Age"", ""Month Offset"", ""Year Offset"", ""Quarter Offset"", ""YearMonth"", ""YearMonth Code""}, {""Fiscal Year"", ""Fiscal Quarter"", ""Fiscal Month"", ""Age"", ""Month Offset"", ""Year Offset"", ""Quarter Offset"", ""YearMonth"", ""YearMonth Code""}),
        #""Extracted Days"" = Table.TransformColumns(#""Expanded Custom2"",{{""Age"", Duration.Days, Int64.Type}}),
        #""Renamed Columns1"" = Table.RenameColumns(#""Extracted Days"",{{""Age"", ""Day Offset""}}),
        #""Changed Type1"" = Table.TransformColumnTypes(#""Renamed Columns1"",{{""StartOfYear"", type date}, {""EndOfYear"", type date}, {""StartOfMonth"", type date}, {""EndOfMonth"", type date}, {""StartOfQuarter"", type date}, {""EndOfQuarter"", type date}, {""StartOfWeek"", type date}, {""EndOfWeek"", type date}}),
        #""Removed Other Columns"" = Table.SelectColumns(#""Changed Type1"",{""Date"", ""Year"", ""Month"", ""Day"", ""DayName"", ""MonthName"", ""Quarter"", ""WeekOfYear"", ""YearMonth"", ""YearMonth Code""}),
    #""Renamed Columns2"" = Table.RenameColumns(#""Extracted Days"",{{""Date"", """ + Column + @"""}})
    in
        #""Renamed Columns2"" ";


    // Update existing partition
    var partition = table.Partitions.First();
    partition.Expression = mExpression;
    partition.Mode = ModeType.Import; // Set the refresh policy to Import
    }
    catch (Exception ex)
    {MessageBox.Show("Adding the Date Dimension Table was not successful but the rest of the script was completed\n\nReason: "+ex.Message);
        }
    }

//Load or Update BPA Rules into Tabular Editor
if (LoadBPA)
        {
System.Net.WebClient w = new System.Net.WebClient(); 

string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
string url = "https://raw.githubusercontent.com/microsoft/Analysis-Services/master/BestPracticeRules/BPARules.json";
string downloadLoc = path+@"\TabularEditor\BPARules.json";
w.DownloadFile(url, downloadLoc);
}

if (GenerateDateDimensionTable3)
{// Define the name of the calculated table
    string tableName = calctableCalendar;

// Define the DAX expression for the calculated table
string tableExpression = @"
VAR all_dates = CALENDARAUTO()
RETURN
    ADDCOLUMNS(
        all_dates,
        ""Year"", YEAR([Date]),
        ""Quarter"", QUARTER([Date]),
        ""Month"", MONTH([Date]),
        ""End of Month"", EOMONTH([Date], 0),
        ""Week of Year"", WEEKNUM([Date]),
        ""Weekday"", WEEKDAY([Date])
    )
";

// Add the calculated table to the model
var table = Model.AddCalculatedTable(tableName, tableExpression);

}


if (GenerateDateDimensionTable2)
{
    try
    {
        // Define the name of the table
        string expressionName = "CalendarFunction";

        // Check if the table with the name 'CalendarFunction' already exists in the model
        if (Model.Expressions.Any(e => e.Name == expressionName))
        {
            throw new InvalidOperationException("Function already exists in the model.");
        }



        
        // Define the M expression as a string - Replace this with your actual Power Query M script
        string mExpression = @"
let
    
fn = (StartYear, YearsIntoFuture, optional Culture, optional StartFiscalYear, optional WeekStart) =>

let
/*
YearsIntoFuture = 2,
StartYear = 2017, 
Culture = ""de-de"", 
StartFiscalYear = ""Feb"",
WeekStart = ""Tue"", 
*/
    Today = Date.From(DateTime.LocalNow()),
    EndYear = Date.Year(Date.From(DateTime.LocalNow())) + YearsIntoFuture, 
    StartFiscYear = if StartFiscalYear = null then ""Jan"" else StartFiscalYear,
    GetStartDay = if StartFiscYear = ""Jan"" then #date(StartYear,1,1) else Date.FromText(""01.""&StartFiscYear &Number.ToText(StartYear)),
    GetEndDay = if StartFiscYear = ""Jan"" then #date(EndYear,12,31) else Date.EndOfMonth( Date.AddMonths( Date.AddYears( Date.FromText(""01.""&StartFiscYear &Number.ToText(EndYear)), 1), -1) ),
    GetCultureDefaultGermany = if Culture = null then ""de-de"" else Culture, 
    DayCount = Duration.Days(Duration.From(GetEndDay - GetStartDay)) + 1, 
    GetListOfDates = List.Dates(GetStartDay,DayCount,#duration(1,0,0,0)), 
    TableFromList = Table.FromList(GetListOfDates, Splitter.SplitByNothing()),    
    ChangedType = Table.TransformColumnTypes(TableFromList,{{""Column1"", type date}}), 
    Date = Table.Buffer( Table.RenameColumns(ChangedType,{{""Column1"", ""Date""}}) ),
    #""AddFull date Description"" = Table.AddColumn(Date, ""Full Date Description"", each Date.ToText([Date], ""dd. MMMM yyyy"", Culture), type text),
    
    //================================================
    DetermineWeekStart = 
        if WeekStart = null then 
          1 
        else  
            List.Select( {
              [Day=""Mo"", WeekStart= Day.Monday],
              [Day=""Tu"", WeekStart= Day.Tuesday],
              [Day=""We"", WeekStart= Day.Wednesday],
              [Day=""Th"", WeekStart= Day.Thursday],
              [Day=""Fr"", WeekStart= Day.Friday],
              [Day=""Sa"", WeekStart= Day.Saturday],
              [Day=""Su"", WeekStart= Day.Sunday]
              }, each _[Day] = WeekStart ){0}[WeekStart] ,
    //================================================
    
    AddWeekDaySort = Table.AddColumn(#""AddFull date Description"", ""Day of Week #"", each Date.DayOfWeek([Date], DetermineWeekStart) + 1, Int64.Type),
    AddMonthDaySort = Table.AddColumn(AddWeekDaySort, ""Day of Month #"", each Date.Day([Date]), Int64.Type),
    #""Day of Year added"" = Table.AddColumn(AddMonthDaySort, ""Day of Year #"", each Date.DayOfYear([Date]), Int64.Type),
    AddDayKey = Table.AddIndexColumn(#""Day of Year added"", ""DayKey #"", 1, 1, Int64.Type),
    AddDayName = Table.AddColumn(AddDayKey, ""Name of the Day (DDDD)"", each Date.DayOfWeekName([Date], Culture), type text),
    AddDaysName2digits = Table.AddColumn(AddDayName, ""Name of the Day (DD)"", each Date.ToText([Date],""ddd"", Culture), type text),
    AddDaysName1digit = Table.AddColumn(AddDaysName2digits, ""Name of the Day (D)"", each Text.Start(Date.DayOfWeekName([Date], Culture),1) & Text.Repeat(Character.FromNumber(8203), [#""Day of Week #""]), type text),
    WT_WE_flag = Table.AddColumn(AddDaysName1digit, ""Weekday_Flag"", each if [#""Day of Week #""] < 6 then ""Weekday"" else ""Weekend"", type text),
    AddWeekOfYear = Table.AddColumn(WT_WE_flag, ""Week #"", each Date.WeekOfYear([Date]), Int64.Type),
    #""Week Start added"" = Table.AddColumn(AddWeekOfYear, ""Week Start"", each Date.StartOfWeek([Date]), type date),
    #""Weekend added"" = Table.AddColumn(#""Week Start added"", ""Week End"", each Date.EndOfWeek([Date]), type date),
    AddYearWeek = Table.AddColumn(#""Weekend added"", ""YearWeek #"", each Date.Year([Date])*100+[#""Week #""], Int64.Type),
    TableWeekKey = /*List.Distinct(#""Changed Type4""[YearWeek])*/ Table.AddIndexColumn(Table.TransformColumnTypes(Table.RenameColumns(Table.FromList(List.Sort(List.Distinct(AddYearWeek[#""YearWeek #""]),Order.Ascending), Splitter.SplitByNothing(), null, null, ExtraValues.Error),{{""Column1"", ""YearWeek""}}),{{""YearWeek"", Int64.Type}}), ""WeekKey #"", 1, 1),
    AddWeekKey = Table.NestedJoin(AddYearWeek,{""YearWeek #""},TableWeekKey,{""YearWeek""},""WK"",JoinKind.LeftOuter),
    #""Expanded WK"" = Table.ExpandTableColumn(AddWeekKey, ""WK"", {""WeekKey #""}, {""WeekKey #""}),
    RemovedYearWeek = Table.RemoveColumns(#""Expanded WK"",{""YearWeek #""}),
    AddKW_Year = Table.AddColumn(RemovedYearWeek, ""Week Year"", each ""KW""&Text.PadStart(Number.ToText([#""Week #""]),2,""0"") &"" ""&Number.ToText(Date.Year([Date])), type text),
    AddYear_KW = Table.AddColumn(AddKW_Year, ""Year Week"", each Number.ToText(Date.Year([Date])) &"" ""&""KW""&Text.PadStart(Number.ToText([#""Week #""]),2,""0""), type text),
    fnGetIsoWeekNumber = (MyDate as date) =>
      //Source --> https://blogs.office.com/en-us/2009/06/30/week-numbers-in-excel/
    let
      //MyDate = #date(2016,1,3),
      Part1 = Number.From(MyDate) - Number.From(#date(Date.Year(Date.From(Number.From(MyDate) - (Date.DayOfWeek(Date.AddDays(MyDate,-1), Day.Sunday) + 1) + 4)),1,3)),
      Part2 = Date.DayOfWeek(#date(Date.Year(Date.From(Number.From(MyDate) - (Date.DayOfWeek(Date.AddDays(MyDate,-1), Day.Sunday) +1) + 4)),1,3), Day.Sunday)+1 + 5,
      ISOWeekNumber = Number.IntegerDivide(Part1 + Part2, 7)
    in
      ISOWeekNumber,
    AddIsoWeek = Table.AddColumn(AddYear_KW, ""IsoKW #"", each fnGetIsoWeekNumber([Date]), Int64.Type),
    AddIsoYear = Table.AddColumn(AddIsoWeek,""IsoYear #"",each Date.Year(Date.AddDays([Date],3 - Date.DayOfWeek([Date], 1))), Int64.Type),
    AddIsoYear_IsoKW = Table.AddColumn(AddIsoYear, ""IsoYear IsoKW"", each Text.From([#""IsoYear #""]) &  "" KW"" & Text.PadStart(Text.From([#""IsoKW #""]),2,""0"") , type text),
    AddIsoKW_IsoYear = Table.AddColumn(AddIsoYear_IsoKW, ""IsoKW IsoYear"", each ""KW"" & Text.PadStart(Text.From([#""IsoKW #""]),2,""0"") &"" ""&Text.From([#""IsoYear #""]), type text),
    GetIsoCalendarWeekKey = Table.AddIndexColumn( Table.Distinct( Table.SelectColumns(AddIsoKW_IsoYear, {""IsoYear IsoKW""}), {""IsoYear IsoKW""}), ""IsoKWKey #"", 1, 1),
    AddIsoCalendarWeekKey = Table.AddJoinColumn(AddIsoKW_IsoYear, {""IsoYear IsoKW""}, GetIsoCalendarWeekKey, {""IsoYear IsoKW""}, ""NEW""),
    ExpandIsoCalendarWeekKey = Table.ExpandTableColumn(AddIsoCalendarWeekKey, ""NEW"", {""IsoKWKey #""}, {""IsoKWKey #""}),
    AddMonthSort = Table.AddColumn(ExpandIsoCalendarWeekKey, ""Month #"", each Date.Month([Date]), Int64.Type),
    AddMonthName = Table.AddColumn(AddMonthSort, ""Month (MMMM)"", each Date.MonthName([Date], Culture), type text),
    AddMonthName3digits = Table.AddColumn(AddMonthName, ""Month (MMM)"", each Date.ToText([Date], ""MMM"", Culture), type text),
    AddMonthName1digit = Table.AddColumn(AddMonthName3digits, ""Month (M)"", each Text.Start(Date.MonthName([Date], Culture),1) & Text.Repeat(Character.FromNumber(8203), [#""Month #""]), type text),
    AddMonthNameShort_Year = Table.AddColumn(AddMonthName1digit, ""Month (MMM) Year"", each [#""Month (MMM)""] &"" ""& Number.ToText(Date.Year([Date])), type text),
#""Add Year Month (MMM)"" = Table.AddColumn(AddMonthNameShort_Year, ""Year Month (MMM)"", each Number.ToText(Date.Year([Date])) &"" ""&[#""Month (MMM)""], type text),
AddYearMonth = Table.TransformColumnTypes(Table.AddColumn(#""Add Year Month (MMM)"", ""YearMonth #"", each Date.Year([Date])*100 + [#""Month #""]),{{""YearMonth #"", Int64.Type}}),
TableYearMonth = Table.AddIndexColumn(Table.TransformColumnTypes(Table.RenameColumns(Table.FromList(List.Sort(List.Distinct(AddYearMonth[#""YearMonth #""]),Order.Ascending), Splitter.SplitByNothing(), null, null, ExtraValues.Error),{{""Column1"", ""YearMonth""}}),{{""YearMonth"", Int64.Type}}), ""YearMonthKey"", 1, 1),
#""Merged Queries"" = Table.NestedJoin(AddYearMonth,{""YearMonth #""},TableYearMonth,{""YearMonth""},""MK"",JoinKind.LeftOuter),
#""Expanded MK"" = Table.ExpandTableColumn(#""Merged Queries"", ""MK"", {""YearMonthKey""}, {""MonthKey #""}),
#""Removed Columns1"" = Table.RemoveColumns(#""Expanded MK"",{""YearMonth #""}),
AddSoM = Table.AddColumn(#""Removed Columns1"", ""StartOfMonth"", each Date.StartOfMonth([Date]), type date),
AddEoM = Table.AddColumn(AddSoM, ""EndOfMonth"", each Date.EndOfMonth([Date]), type date),
AddQuarter = Table.AddColumn(AddEoM, ""Quarter #"", each Date.QuarterOfYear([Date]), Int64.Type),
AddQuarterName = Table.AddColumn(AddQuarter, ""Quarter"", each ""Q"" & Number.ToText([#""Quarter #""]), type text),
AddQuarter_Year = Table.AddColumn(AddQuarterName, ""Quarter Year"", each ""Q""&Number.ToText([#""Quarter #""]) &""-""&Number.ToText(Date.Year([Date])), type text),
AddYear_Quarter = Table.AddColumn(AddQuarter_Year, ""Year Quarter"", each Number.ToText(Date.Year([Date])) & ""-Q"" & Number.ToText([#""Quarter #""]), type text),
AddYearQuarter = Table.AddColumn(AddYear_Quarter, ""Year Quarter #"", each Date.Year([Date]) * 100 + [#""Quarter #""], Int64.Type),
TableYearQuarter = Table.AddIndexColumn(Table.TransformColumnTypes(Table.RenameColumns(Table.FromList(List.Sort(List.Distinct(#""AddYearQuarter""[#""Year Quarter #""]),Order.Ascending), Splitter.SplitByNothing(), null, null, ExtraValues.Error),{{""Column1"", ""YearQuarter""}}),{{""YearQuarter"", Int64.Type}}), ""QuarterKey"", 1, 1),
GetQuarterKey = Table.NestedJoin(AddYearQuarter,{""Year Quarter #""},TableYearQuarter,{""YearQuarter""},""QK"",JoinKind.LeftOuter),
AddQuarterKey = Table.ExpandTableColumn(GetQuarterKey, ""QK"", {""QuarterKey""}, {""QuarterKey #""}),
#""Removed Columns"" = Table.RemoveColumns(AddQuarterKey,{""Year Quarter #""}),
AddHalfYearSort = Table.TransformColumnTypes(Table.AddColumn(#""Removed Columns"", ""HalfYear #"", each if Date.Month([Date]) < 7 then 1 else 2),{{""HalfYear #"", Int64.Type}}),
AddHalfYearName = Table.AddColumn(AddHalfYearSort, ""HalfYear"", each ""HJ "" & Number.ToText([#""HalfYear #""]), type text),
AddHalfYear_Year = Table.AddColumn(AddHalfYearName, ""HalfYear Year"", each ""HJ ""&Number.ToText([#""HalfYear #""])&""-""&Number.ToText(Date.Year([Date])), type text),
#""Added Custom Column3"" = Table.AddColumn(AddHalfYear_Year, ""Year HalfYear"", each Number.ToText(Date.Year([Date]))&""-""& ""HJ ""&Number.ToText([#""HalfYear #""]), type text),
AddYearHalfYear = Table.TransformColumnTypes(Table.AddColumn(#""Added Custom Column3"", ""YearHalfYear #"", each Date.Year([Date])*100+[#""HalfYear #""]),{{""YearHalfYear #"", Int64.Type}}),
TableYearHalfYear = Table.AddIndexColumn(Table.TransformColumnTypes(Table.RenameColumns(Table.FromList(List.Sort(List.Distinct(AddYearHalfYear[#""YearHalfYear #""]),Order.Ascending), Splitter.SplitByNothing(), null, null, ExtraValues.Error),{{""Column1"", ""YearHalfYear""}}),{{""YearHalfYear"", Int64.Type}}), ""HalfYearKey"", 1, 1),
GetHalfYearKey = Table.NestedJoin(AddYearHalfYear,{""YearHalfYear #""},TableYearHalfYear,{""YearHalfYear""},""HYK"",JoinKind.LeftOuter),
AddHalfYearKey = Table.ExpandTableColumn(GetHalfYearKey, ""HYK"", {""HalfYearKey""}, {""HalfYearKey #""}),
#""Removed Columns4"" = Table.RemoveColumns(AddHalfYearKey,{""YearHalfYear #""}),
AddYear = Table.AddColumn(#""Removed Columns4"", ""Year #"", each Date.Year([Date]), Int64.Type),
AddYearKey = Table.AddColumn(AddYear, ""YearKey #"", each [#""Year #""] - List.Min(AddYear[#""Year #""]) + 1, Int64.Type),
IsLeapYear = Table.Buffer( Table.TransformColumnTypes(Table.AddColumn(AddYearKey, ""LeapYear"", each Number.From( Date.IsLeapYear( [Date] ))),{{""LeapYear"", Int64.Type}}) ),

fnKeysTodayRecord =
  let
  FilterTableToToday = Table.SelectRows(IsLeapYear, each _[Date] = Today),
  Output = 
    [
      TodayDayKey = FilterTableToToday[#""DayKey #""]{0}, 
      TodayWeekKey = FilterTableToToday[#""WeekKey #""]{0},
      TodayIsoWeekKey = FilterTableToToday[#""IsoKW #""]{0},
      TodayMonthKey = FilterTableToToday[#""MonthKey #""]{0},
      TodayQuarterKey = FilterTableToToday[#""QuarterKey #""]{0},
      TodayHalfYearKey = FilterTableToToday[#""HalfYearKey #""]{0},
      TodayYearKey = FilterTableToToday[#""YearKey #""]{0}
    ]
  in
  Output, 
AddRelativeDay = Table.AddColumn(IsLeapYear, ""Relative Day #"", each [#""DayKey #""] - fnKeysTodayRecord[TodayDayKey], Int64.Type),
AddRelativeWeek = Table.AddColumn(AddRelativeDay, ""Relative Week #"", each [#""WeekKey #""] - fnKeysTodayRecord[TodayWeekKey], Int64.Type),
AddRelativeIsoWeek = Table.AddColumn(AddRelativeWeek, ""Relative IsoWeek #"", each [#""IsoKW #""] - fnKeysTodayRecord[TodayIsoWeekKey], Int64.Type),
AddRelativeMonth = Table.AddColumn(AddRelativeIsoWeek, ""Relative Month #"", each [#""MonthKey #""] - fnKeysTodayRecord[TodayMonthKey], Int64.Type),
AddRelativeQuarter = Table.AddColumn(AddRelativeMonth, ""Relative Quarter #"", each [#""QuarterKey #""] - fnKeysTodayRecord[TodayQuarterKey], Int64.Type),
AddRelativeHalfYear = Table.AddColumn(AddRelativeQuarter, ""Relative HalfYear #"", each [#""HalfYearKey #""] - fnKeysTodayRecord[TodayHalfYearKey], Int64.Type),
AddRelativeYear = Table.AddColumn(AddRelativeHalfYear, ""Relative Year #"", each [#""YearKey #""] - fnKeysTodayRecord[TodayYearKey], Int64.Type),

//2Go-Detection
ListGetWeek2Go = List.Buffer( Table.SelectRows(AddRelativeYear, each [#""Relative Week #""] = 0 and [Date] > Today)[Date] ),
ListGetIsoWeek2Go = List.Buffer( Table.SelectRows(AddRelativeYear, each [#""Relative IsoWeek #""] = 0 and [Date] > Today)[Date] ),
ListGetMonth2Go = List.Buffer( Table.SelectRows(AddRelativeYear, each [#""Relative Month #""] = 0 and [Date] > Today)[Date] ),
ListGetQuarter2Go = List.Buffer( Table.SelectRows(AddRelativeYear, each [#""Relative Quarter #""] = 0 and [Date] > Today)[Date] ),
ListGetHalfYear2Go = List.Buffer( Table.SelectRows(AddRelativeYear, each [#""Relative HalfYear #""] = 0 and [Date] > Today)[Date] ),
ListGetYear2Go = List.Buffer( Table.SelectRows(AddRelativeYear, each [#""Relative Year #""] = 0 and [Date] > Today)[Date] ),

AddCol_Week2Go = Table.AddColumn( AddRelativeYear, ""Week 2 Go"", each Number.From( List.Contains(ListGetWeek2Go, [Date]) ), Int64.Type),
AddCol_IsoWeek2Go = Table.AddColumn( AddCol_Week2Go, ""IsoWeek 2 Go"", each Number.From( List.Contains(ListGetIsoWeek2Go, [Date]) ), Int64.Type),
AddCol_Month2Go = Table.AddColumn( AddCol_IsoWeek2Go, ""Month 2 Go"", each Number.From( List.Contains(ListGetMonth2Go, [Date]) ), Int64.Type),
AddCol_Quarter2Go = Table.AddColumn(AddCol_Month2Go, ""Quarter 2 Go"", each Number.From(List.Contains(ListGetQuarter2Go, [Date])), Int64.Type),
AddCol_HalfYear2Go = Table.AddColumn(AddCol_Quarter2Go, ""Half Year 2 Go"", each Number.From(List.Contains(ListGetHalfYear2Go, [Date])), Int64.Type),
AddCol_Year2Go = Table.AddColumn(AddCol_HalfYear2Go, ""Year 2 Go"", each Number.From(List.Contains(ListGetYear2Go, [Date])), Int64.Type),
//==============================================================================Fiscal Year Calculations=================================================================
GetStartMonthNumberFiscalYear = List.PositionOf({""Jan"", ""Feb"", ""Mar"", ""Apr"", ""May"", ""Jun"", ""Jul"", ""Aug"", ""Sep"", ""Oct"", ""Nov"", ""Dec""}, StartFiscalYear) + 1,

AddFiscalYear = Table.AddColumn(AddCol_Year2Go, ""Fiscal Year #"", each if Date.Month([Date]) < GetStartMonthNumberFiscalYear then Date.Year([Date]) else Date.Year([Date]) + 1, Int64.Type),
#""Added Custom Column"" = Table.AddColumn(AddFiscalYear, ""Fiscal Year"", each if Date.Month([Date]) < GetStartMonthNumberFiscalYear then ""FY "" & Number.ToText(Date.Year([Date])) else ""FY "" & Number.ToText(Date.Year([Date]) + 1), type text),
#""AddFiscalMonth#"" = Table.AddColumn(#""Added Custom Column"", ""Fiscal Month #"", each if (Date.Month([Date]) >= GetStartMonthNumberFiscalYear) then Date.Month([Date]) - GetStartMonthNumberFiscalYear +1 
else 
Date.Month([Date])+13-GetStartMonthNumberFiscalYear, Int64.Type),
AddFiscalMonth_MMMM = Table.AddColumn(#""AddFiscalMonth#"", ""Fiscal Month (MMMM)"", each Date.MonthName([Date]), type text),
Add_FiscalMonth_MMM = Table.AddColumn(AddFiscalMonth_MMMM, ""Fiscal Month (MMM)"", each Date.ToText([Date], ""MMM"", Culture), type text),
AddFiscalMonth_MM = Table.AddColumn(Add_FiscalMonth_MMM, ""Fiscal Month (M)"", each Text.Start(Date.MonthName([Date], Culture),1) & Text.Repeat(Character.FromNumber(8203), [#""Month #""]), type text),
Add_FiscalQuarter = Table.TransformColumnTypes(Table.AddColumn(AddFiscalMonth_MM, ""Fiscal Quarter #"", each Number.RoundUp([#""Fiscal Month #""]/3, 0)), {{""Fiscal Quarter #"", Int64.Type}}),
#""Added Custom Column1"" = Table.AddColumn(Add_FiscalQuarter, ""Fiscal Quarter"", each ""FQ "" & Number.ToText([#""Fiscal Quarter #""]), type text),
#""Added Custom Column2"" = Table.AddColumn(#""Added Custom Column1"", ""Fiscal Quarter Fiscal Year"", each [Fiscal Quarter]&""-FY ""&Number.ToText([#""Fiscal Year #""]), type text),
AddFiscalYearQuarterName = Table.AddColumn(#""Added Custom Column2"", ""Fiscal Year Fiscal Quarter"", each ""FY ""&Text.From([#""Fiscal Year #""]) &""-FQ"" & Text.From([#""Fiscal Quarter #""]), type text),
AddFiscalYearQuarter = Table.AddColumn(AddFiscalYearQuarterName, ""Fiscal_YearQuarter"", each [#""Fiscal Year #""] * 100 + [#""Fiscal Quarter #""], Int64.Type),
TableFiscalYearQuarter = Table.AddIndexColumn(Table.TransformColumnTypes(Table.RenameColumns(Table.FromList(List.Sort(List.Distinct(#""AddFiscalYearQuarter""[#""Fiscal_YearQuarter""]), Order.Ascending), Splitter.SplitByNothing(), null, null, ExtraValues.Error), {{""Column1"", ""Fiscal_YearQuarter""}}), {{""Fiscal_YearQuarter"", Int64.Type}}), ""Fiscal_QuarterKey"", 1, 1),
GetFiscalYearQuarterKey = Table.NestedJoin(AddFiscalYearQuarter,{""Fiscal_YearQuarter""},TableFiscalYearQuarter,{""Fiscal_YearQuarter""},""FYQ"",JoinKind.LeftOuter),
#""Expanded FYQ"" = Table.ExpandTableColumn(GetFiscalYearQuarterKey, ""FYQ"", {""Fiscal_QuarterKey""}, {""FiscalQuarterKey #""}),
#""Removed Columns2"" = Table.RemoveColumns(#""Expanded FYQ"",{""Fiscal_YearQuarter""}),
AddFiscalHalfYear = Table.TransformColumnTypes(Table.AddColumn(#""Removed Columns2"", ""Fiscal Half Year #"", each if [#""Fiscal Month #""] < 7 then 1 else 2), {{""Fiscal Half Year #"", Int64.Type}}),
AddFiscalHalfYearName = Table.AddColumn(AddFiscalHalfYear, ""Fiscal Half Year"", each ""FHY "" & Number.ToText([#""Fiscal Half Year #""]), type text),
AddFiscalYearHalfYearName = Table.AddColumn(AddFiscalHalfYearName, ""Fiscal Year Fiscal Half Year"", each ""FY "" &Text.From([#""Fiscal Year #""]) & ""-FHY"" & Text.From([#""Fiscal Half Year #""]), type text),
#""Added Custom Column4"" = Table.AddColumn(AddFiscalYearHalfYearName, ""Fiscal Half Year Fiscal Year"", each ""FHY "" & Text.From([#""Fiscal Half Year #""])&""-FY "" &Text.From([#""Fiscal Year #""]), type text),
AddFiscalYearHalfYear = Table.AddColumn(#""Added Custom Column4"", ""Fiscal_YearHalfYear"", each [#""Fiscal Year #""] * 100 + [#""Fiscal Half Year #""], Int64.Type),
TableFiscalYearHalfYear = Table.AddIndexColumn(Table.TransformColumnTypes(Table.RenameColumns(Table.FromList(List.Sort(List.Distinct(#""AddFiscalYearHalfYear""[Fiscal_YearHalfYear]),Order.Ascending), Splitter.SplitByNothing(), null, null, ExtraValues.Error),{{""Column1"", ""Fiscal_YearHalfYear""}}),{{""Fiscal_YearHalfYear"", Int64.Type}}), ""Fiscal_HalfYearKey"", 1, 1),
GetFiscalYearHalfYearKey = Table.NestedJoin(AddFiscalYearHalfYear,{""Fiscal_YearHalfYear""},TableFiscalYearHalfYear,{""Fiscal_YearHalfYear""},""FYHY"",JoinKind.LeftOuter),
#""Expanded FYHY"" = Table.ExpandTableColumn(GetFiscalYearHalfYearKey, ""FYHY"", {""Fiscal_HalfYearKey""}, {""FiscalHalfYearKey #""}),
AddFiscalYearKey = Table.AddColumn(#""Expanded FYHY"", ""FiscalYearKey #"", each [#""Fiscal Year #""] - List.Min(#""Expanded FYHY""[#""Fiscal Year #""]) + 1, Int64.Type),
#""Removed Columns3"" = Table.RemoveColumns(AddFiscalYearKey,{""Fiscal_YearHalfYear""}),


fnKeysTodayRecordFiscal =
      let
      TableFilterToday = Table.SelectRows(#""Removed Columns3"", each _[Date] = Today),
      Output = 
        [
          FiscalQuarterKeyToday = TableFilterToday[#""FiscalQuarterKey #""]{0},
          FiscalHalfYearKeyToday = TableFilterToday[#""FiscalHalfYearKey #""]{0},
          FiscalYearKeyToday = TableFilterToday[#""FiscalYearKey #""]{0}
        ]
      in
      Output,

  //Relative Fiscal Units
   AddRelativeFiscalQuarter = Table.AddColumn(#""Removed Columns3"", ""Relative Fiscal Quarter #"", each [#""FiscalQuarterKey #""] - fnKeysTodayRecordFiscal[FiscalQuarterKeyToday], Int64.Type),
    AddRelativeFiscalHalfYear = Table.AddColumn(AddRelativeFiscalQuarter, ""Relative Fiscal Half Year #"", each [#""FiscalHalfYearKey #""] - fnKeysTodayRecordFiscal[FiscalHalfYearKeyToday], Int64.Type),
    AddRelativeFiscalYear = Table.AddColumn(AddRelativeFiscalHalfYear, ""Relative Fiscal Year #"", each [#""FiscalYearKey #""] - fnKeysTodayRecordFiscal[FiscalYearKeyToday], Int64.Type),

    //2Go-Determination
    ListGetFiscalQuarter2Go = List.Buffer(Table.SelectRows(AddRelativeFiscalYear, each [#""Relative Fiscal Quarter #""] = 0 and [Date] > Today)[Date]),
    ListGetFiscalHalfYear2Go = List.Buffer(Table.SelectRows(AddRelativeFiscalYear, each [#""Relative Fiscal Half Year #""] = 0 and [Date] > Today)[Date]),
    ListGetFiscalYear2Go = List.Buffer(Table.SelectRows(AddRelativeFiscalYear, each [#""Relative Fiscal Year #""] = 0 and [Date] > Today)[Date]),

    AddCol_FiscalQuarter2Go = Table.AddColumn(AddRelativeFiscalYear, ""Fiscal Quarter 2 Go"", each Number.From(List.Contains(ListGetFiscalQuarter2Go, [Date])), Int64.Type),
    AddCol_FiscalHalfYear2Go = Table.AddColumn(AddCol_FiscalQuarter2Go, ""Fiscal Half Year 2 Go"", each Number.From(List.Contains(ListGetFiscalHalfYear2Go, [Date])), Int64.Type),
    AddCol_FiscalYear2Go = Table.AddColumn(AddCol_FiscalHalfYear2Go, ""Fiscal Year 2 Go"", each Number.From(List.Contains(ListGetFiscalYear2Go, [Date])), Int64.Type),

    //==============================================================================Fiscal Year Calculations=================================================================
    Output = if StartFiscalYear = ""Jan"" then AddCol_Year2Go else AddCol_FiscalYear2Go
in
    Output

  ,
    fnType = type function(
              StartYear as number,
              YearsIntoTheFuture as number, 
              optional Culture as (type text meta [Documentation.AllowedValues={""de-de"", ""en-US"", ""fr-FR"", ""es-ES""}]), 
              optional StartFiscalYear as (type text meta[Documentation.AllowedValues={""Jan"", ""Feb"", ""Mar"", ""Apr"", ""May"", ""Jun"", ""Jul"", ""Aug"", ""Sep"", ""Oct"", ""Nov"", ""Dec""}]),
              optional WeekStart as (type text meta[Documentation.AllowedValues={""Mo"", ""Tu"", ""We"", ""Th"", ""Fr"", ""Sa"", ""Su""}])
               ) as table meta [
                                Documentation.Name=""fnCalendar (by Lars Schreiber --> ssbi-blog.de)"",
                                Documentation.LongDescription=""This function creates a calendar table, based on the suggestions of the Kimball Group."",
                                Documentation.Author=""Lars Schreiber, ssbi-blog.de"",
                                Documentation.Examples=
                                    {[
                                        Description = ""Returns a calendar table starting in 2019 and automatically expanding by one year into the future from the current year."",
                                        Code = ""fnCalendar(2019, 1, null, null, null)"",
                                        Result =""""
                                        
                                    ],[
                                        Description = ""Returns a calendar table starting in 2019 and automatically expanding by one year into the future from the current year. It includes additional columns that follow the logic of a fiscal year that starts in July and ends in June of the following year."",
                                        Code = ""fnCalendar(2019, 1, null, Jul, null)"",
                                        Result =""""
                                        
                                    ]} 
                                ]
in
Value.ReplaceType(fn, fnType)";


        // Create a new table in the model
        NamedExpression Namedexpression = Model.AddExpression(expressionName, mExpression);
        Namedexpression.Expression = mExpression;

        Namedexpression.Kind = ExpressionKind.M;


    }
    catch (Exception ex)
    {
        MessageBox.Show("Adding the DateFunction table was not successful but the rest of the script was completed.\n\nReason: {ex.Message}");
    }
}