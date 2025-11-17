#!/usr/bin/env python
# coding: utf-8

# ## O2C Analyzer - BPA & Memory Analyzer
# 
# null

# In[2]:


import sempy.fabric as fabric
from datetime import datetime
import re
import pandas as pd

# ============================================================================
# CONFIGURATION
# ============================================================================
dataset = "Semantic Model Name"
workspace = "Workspace Name"
lakehouse = "Lakehouse_Name"

# Function to clean column names
def clean_column_name(col_name):
    col_name = str(col_name).replace('[', '').replace(']', '')
    col_name = col_name.replace(' ', '_')
    col_name = re.sub(r'[,;{}()\n\t=]', '', col_name)
    return col_name

# Function to save DataFrame with smart append/overwrite
def save_to_lakehouse(df, table_name, description="results"):
    """
    Saves DataFrame to lakehouse. Tries append first, creates table if needed.
    """
    full_table_name = f"{lakehouse}.{table_name}"
    
    # Convert pandas to Spark if needed
    if isinstance(df, pd.DataFrame):
        spark_df = spark.createDataFrame(df)
    else:
        spark_df = df
    
    print(f"Saving {description}...")
    
    try:
        # Try append first
        spark_df.write \
            .format("delta") \
            .mode("append") \
            .option("mergeSchema", "true") \
            .saveAsTable(full_table_name)
        print(f"✓ Appended {len(df)} records")
    except:
        # Table doesn't exist, create it
        spark_df.write \
            .format("delta") \
            .mode("overwrite") \
            .option("overwriteSchema", "true") \
            .saveAsTable(full_table_name)
        print(f"✓ Created table with {len(df)} records")

# Function to process DAX results with metadata and timestamp handling
def process_dax_results(results, dataset, workspace, table_name, description):
    """
    Processes DAX query results: adds metadata, cleans columns, handles timestamps
    """
    if results is not None and len(results) > 0:
        results['analysis_timestamp'] = datetime.now()
        results['model_name'] = dataset
        results['workspace_name'] = workspace
        results.columns = [clean_column_name(col) for col in results.columns]
        
        # Handle old timestamps in common date columns
        for col in ['ModifiedTime', 'RefreshedTime', 'StructureModifiedTime']:
            if col in results.columns:
                results[col] = pd.to_datetime(results[col], errors='coerce')
                results[col] = results[col].where(results[col] > pd.Timestamp('1900-01-01'), None)
        
        save_to_lakehouse(results, table_name, description)
        display(results.head(10))
        return True
    return False

print("="*80)
print("STEP 1: Running Best Practice Analyzer...")
print("="*80)

# Run BPA analysis
try:
    bpa_results = fabric.run_model_bpa(
        dataset=dataset, 
        workspace=workspace,
        return_dataframe=True
    )
except TypeError:
    bpa_results = fabric.run_model_bpa(dataset=dataset, workspace=workspace)

if bpa_results is not None and len(bpa_results) > 0:
    # Add metadata
    bpa_results['analysis_timestamp'] = datetime.now()
    bpa_results['model_name'] = dataset
    bpa_results['workspace_name'] = workspace
    bpa_results.columns = [clean_column_name(col) for col in bpa_results.columns]

    # Save to lakehouse
    save_to_lakehouse(bpa_results, "bpa_analysis_results", "BPA results")

    print("\nSummary by Severity:")
    display(bpa_results.groupby('Severity').size())

print("\n" + "="*80)
print("STEP 2: Running Model Memory Analyzer...")
print("="*80)

memory_results = fabric.model_memory_analyzer(dataset=dataset, workspace=workspace)

print("\n" + "="*80)
print("STEP 3: Capturing Memory & Storage Statistics via DAX...")
print("="*80)

spark.conf.set("spark.sql.parquet.datetimeRebaseModeInWrite", "CORRECTED")

# Define all INFO queries
dax_queries = [
    ("INFO.TABLES()", "info_tables", "table statistics"),
    ("INFO.COLUMNS()", "info_columns", "column statistics"),
    ("INFO.MEASURES()", "info_measures", "measure statistics"),
    ("INFO.RELATIONSHIPS()", "info_relationships", "relationship statistics"),
    ("INFO.STORAGETABLES()", "info_storage_tables", "storage table statistics"),
    ("INFO.STORAGETABLECOLUMNS()", "info_storage_columns", "storage column statistics"),
    ("INFO.STORAGETABLECOLUMNSEGMENTS()", "info_storage_segments", "storage segment statistics")
    ("INFO.PARTITIONS()", "info_partitions", "partitions statistics")
]

try:
    for dax_query, table_name, description in dax_queries:
        print(f"\nGetting {description}...")
        
        results = fabric.evaluate_dax(
            dataset=dataset, 
            workspace=workspace, 
            dax_string=f"EVALUATE {dax_query}"
        )
        
        process_dax_results(results, dataset, workspace, table_name, description)
        
except Exception as e:
    print(f"Error: {str(e)}")

print("\n" + "="*80)
print("✓ ANALYSIS COMPLETE!")
print("="*80)
print("✓ All results saved to lakehouse and queryable via SQL Analytics Endpoint")
print("\nTables created:")
print("  - bpa_analysis_results")
print("  - info_tables")
print("  - info_columns")
print("  - info_measures")
print("  - info_relationships")
print("  - info_storage_tables")
print("  - info_storage_columns")
print("  - info_storage_segments")
print("  - info_partitions")

