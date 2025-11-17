/****** Object:  View [dbo].[vertipaq_relationship_view]    Script Date: 11/17/2025 2:59:30 PM ******/
-- ============================================================================
-- VIEW 3: VertiPaq Relationship View
-- Provides relationship-level memory analysis
-- ============================================================================
CREATE OR ALTER   VIEW [dbo].[vertipaq_relationship_view] AS
WITH 
-- Get relationship sizes
relationship_size AS (
    SELECT 
        DIMENSION_NAME AS Table_Name,
        USED_SIZE AS Used_Size,
        REPLACE(
            SUBSTRING(
                TABLE_ID, 
                CHARINDEX('(', TABLE_ID, CHARINDEX('(', TABLE_ID) + 1) + 1, 
                LEN(TABLE_ID)
            ),
            ')', 
            ''
        ) AS Relationship_ID
    FROM info_storage_segments
    WHERE LEFT(TABLE_ID, 2) = 'R$'
),
-- Get from column cardinality
from_column_cardinality AS (
    SELECT 
        REPLACE(
            SUBSTRING(
                TABLE_ID, 
                CHARINDEX('(', TABLE_ID, CHARINDEX('(', TABLE_ID) + 1) + 1, 
                LEN(TABLE_ID)
            ),
            ')', 
            ''
        ) AS From_Column_ID,
        ROWS_COUNT - 3 AS From_Column_Cardinality
    FROM info_storage_tables
    WHERE LEFT(TABLE_ID, 2) = 'H$'
),
-- Get to column cardinality
to_column_cardinality AS (
    SELECT 
        REPLACE(
            SUBSTRING(
                TABLE_ID, 
                CHARINDEX('(', TABLE_ID, CHARINDEX('(', TABLE_ID) + 1) + 1, 
                LEN(TABLE_ID)
            ),
            ')', 
            ''
        ) AS To_Column_ID,
        ROWS_COUNT - 3 AS To_Column_Cardinality
    FROM info_storage_tables
    WHERE LEFT(TABLE_ID, 2) = 'H$'
),
-- Get relationship metadata
relationship_info AS (
    SELECT 
        CAST(ID AS VARCHAR(50)) AS Relationship_ID,
        FromTableID AS From_Table_ID,
        CAST(FromColumnID AS VARCHAR(50)) AS From_Column_ID,
        ToTableID AS To_Table_ID,
        CAST(ToColumnID AS VARCHAR(50)) AS To_Column_ID,
        CASE CrossFilteringBehavior
            WHEN 1 THEN 'One'
            WHEN 2 THEN 'Both'
            ELSE 'Something else'
        END AS Cross_Filter_Behavior_From,
        CASE FromCardinality
            WHEN 1 THEN 'One'
            WHEN 2 THEN 'Many'
            ELSE 'Something else'
        END AS From_Cardinality,
        CASE ToCardinality
            WHEN 1 THEN 'One'
            WHEN 2 THEN 'Many'
            ELSE 'Something else'
        END AS To_Cardinality
    FROM info_relationships
),
-- Get from table names
from_table AS (
    SELECT 
        ID AS From_Table_ID,
        Name AS From_TableName
    FROM info_tables
),
-- Get to table names
to_table AS (
    SELECT 
        ID AS To_Table_ID,
        Name AS To_TableName
    FROM info_tables
),
-- Get from column names
from_col AS (
    SELECT 
        CAST(ID AS VARCHAR(50)) AS From_Column_ID,
        COALESCE(ExplicitName, InferredName) AS From_ColumnName
    FROM info_columns
),
-- Get to column names
to_col AS (
    SELECT 
        CAST(ID AS VARCHAR(50)) AS To_Column_ID,
        COALESCE(ExplicitName, InferredName) AS To_ColumnName
    FROM info_columns
)
SELECT 
    ft.From_TableName AS From_Table_Name,
    fc.From_ColumnName AS From_Column_Name,
    tt.To_TableName AS To_Table_Name,
    tc.To_ColumnName AS To_Column_Name,
    ri.From_Cardinality AS From_Card,
    ri.To_Cardinality AS To_Card,
    ri.Cross_Filter_Behavior_From AS Cross_Filter_Behaviour,
    rs.Used_Size AS Size,
    fcc.From_Column_Cardinality AS Max_From_Cardinality,
    tcc.To_Column_Cardinality AS Max_To_Cardinality
FROM relationship_info ri
LEFT JOIN relationship_size rs ON ri.Relationship_ID = rs.Relationship_ID
LEFT JOIN from_column_cardinality fcc ON ri.From_Column_ID = fcc.From_Column_ID
LEFT JOIN to_column_cardinality tcc ON ri.To_Column_ID = tcc.To_Column_ID
LEFT JOIN from_table ft ON ri.From_Table_ID = ft.From_Table_ID
LEFT JOIN to_table tt ON ri.To_Table_ID = tt.To_Table_ID
LEFT JOIN from_col fc ON ri.From_Column_ID = fc.From_Column_ID
LEFT JOIN to_col tc ON ri.To_Column_ID = tc.To_Column_ID;
GO


