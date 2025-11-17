/****** Object:  View [dbo].[vertipaq_summary_view]    Script Date: 11/17/2025 2:59:56 PM ******/
-- ============================================================================
-- VIEW 4: VertiPaq Summary View
-- Provides overall model size summary with counts
-- ============================================================================
CREATE OR ALTER   VIEW [dbo].[vertipaq_summary_view] AS
WITH 
-- Get table cardinality and RI violations
table_cardinality AS (
    SELECT 
        DIMENSION_NAME,
        SUM(ROWS_COUNT) AS Cardinality,
        SUM(RIVIOLATION_COUNT) AS RIViolation
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
        COALESCE(ds.DataSize, 0) + COALESCE(dic.DicSize, 0) + 
        COALESCE(hs.HierSize, 0) + COALESCE(uhs.UserHierSize, 0) + 
        COALESCE(rs.RelSize, 0) AS Total_Size,
        COALESCE(ds.DataSize, 0) AS Data,
        COALESCE(dic.DicSize, 0) AS Dictionary,
        COALESCE(hs.HierSize, 0) AS Hier_Size,
        tc.RIViolation
    FROM table_cardinality tc
    LEFT JOIN data_size ds ON tc.DIMENSION_NAME = ds.DIMENSION_NAME
    LEFT JOIN dic_size dic ON tc.DIMENSION_NAME = dic.DIMENSION_NAME
    LEFT JOIN hier_size hs ON tc.DIMENSION_NAME = hs.DIMENSION_NAME
    LEFT JOIN user_hier_size uhs ON tc.DIMENSION_NAME = uhs.DIMENSION_NAME
    LEFT JOIN rel_size rs ON tc.DIMENSION_NAME = rs.DIMENSION_NAME
)
SELECT 
    (SELECT COUNT(DISTINCT DIMENSION_NAME) 
     FROM info_storage_tables 
     WHERE RIGHT(LEFT(TABLE_ID, 2), 1) <> '$') AS Table_Count,
    (SELECT COUNT(*) 
     FROM info_storage_columns 
     WHERE COLUMN_TYPE = 'BASIC_DATA') AS Column_Count,
    ROUND(SUM(Total_Size) / 1024.0 / 1024.0, 2) AS Size_in_MB,
    SUM(Cardinality) AS Total_Rows,
    SUM(RIViolation) AS Total_RI_Violations
FROM combined;
GO


