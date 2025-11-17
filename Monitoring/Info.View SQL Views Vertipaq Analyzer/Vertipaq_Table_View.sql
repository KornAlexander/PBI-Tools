/****** Object:  View [dbo].[vertipaq_table_view]    Script Date: 11/17/2025 3:00:20 PM ******/
-- ============================================================================
-- VIEW 1: VertiPaq Table View
-- Provides table-level memory analysis with size breakdown
-- ============================================================================
CREATE OR ALTER   VIEW [dbo].[vertipaq_table_view] AS
WITH 
-- Get table cardinality and RI violations
table_cardinality AS (
    SELECT 
        DIMENSION_NAME,
        SUM(ROWS_COUNT) AS Cardinality,
        SUM(RIVIOLATION_COUNT) AS RI_Violations
    FROM info_storage_tables
    WHERE RIGHT(LEFT(TABLE_ID, 2), 1) <> '$'
    GROUP BY DIMENSION_NAME
),
-- Get data size per table
data_size AS (
    SELECT 
        DIMENSION_NAME,
        SUM(USED_SIZE) AS DataSize
    FROM info_storage_segments
    WHERE RIGHT(LEFT(TABLE_ID, 2), 1) <> '$'
    GROUP BY DIMENSION_NAME
),
-- Get hierarchy size per table
hier_size AS (
    SELECT 
        DIMENSION_NAME,
        SUM(USED_SIZE) AS HierSize
    FROM info_storage_segments
    WHERE LEFT(TABLE_ID, 2) = 'H$' 
      AND SEGMENT_NUMBER = 0
    GROUP BY DIMENSION_NAME
),
-- Get user hierarchy size per table
user_hier_size AS (
    SELECT 
        DIMENSION_NAME,
        SUM(USED_SIZE) AS UserHierSize
    FROM info_storage_segments
    WHERE LEFT(TABLE_ID, 2) = 'U$' 
      AND SEGMENT_NUMBER = 0
    GROUP BY DIMENSION_NAME
),
-- Get relationship size per table
rel_size AS (
    SELECT 
        DIMENSION_NAME,
        SUM(USED_SIZE) AS RelSize
    FROM info_storage_segments
    WHERE LEFT(TABLE_ID, 2) = 'R$' 
      AND SEGMENT_NUMBER = 0
    GROUP BY DIMENSION_NAME
),
-- Get dictionary size per table
dic_size AS (
    SELECT 
        DIMENSION_NAME,
        SUM(DICTIONARY_SIZE) AS DicSize
    FROM info_storage_columns
    WHERE COLUMN_TYPE = 'BASIC_DATA'
    GROUP BY DIMENSION_NAME
),
-- Combine all metrics
combined AS (
    SELECT 
        tc.DIMENSION_NAME AS Table_Name,
        tc.Cardinality,
        COALESCE(ds.DataSize, 0) AS Data,
        COALESCE(dic.DicSize, 0) AS Dictionary,
        COALESCE(hs.HierSize, 0) AS Hier_Size,
        COALESCE(uhs.UserHierSize, 0) AS User_Hier_Size,
        COALESCE(rs.RelSize, 0) AS Rel_Size,
        COALESCE(ds.DataSize, 0) + COALESCE(dic.DicSize, 0) + 
        COALESCE(hs.HierSize, 0) + COALESCE(uhs.UserHierSize, 0) + 
        COALESCE(rs.RelSize, 0) AS Total_Size,
        tc.RI_Violations
    FROM table_cardinality tc
    LEFT JOIN data_size ds ON tc.DIMENSION_NAME = ds.DIMENSION_NAME
    LEFT JOIN dic_size dic ON tc.DIMENSION_NAME = dic.DIMENSION_NAME
    LEFT JOIN hier_size hs ON tc.DIMENSION_NAME = hs.DIMENSION_NAME
    LEFT JOIN user_hier_size uhs ON tc.DIMENSION_NAME = uhs.DIMENSION_NAME
    LEFT JOIN rel_size rs ON tc.DIMENSION_NAME = rs.DIMENSION_NAME
)
SELECT 
    Table_Name,
    Cardinality,
    Total_Size,
    Data,
    Dictionary,
    Hier_Size,
    User_Hier_Size,
    Rel_Size,
    RI_Violations,
    CONCAT(CAST(ROUND(Total_Size * 100.0 / SUM(Total_Size) OVER(), 2) AS VARCHAR(20)), '%') AS DB_Percent
FROM combined;
GO


