/****** Object:  View [dbo].[vertipaq_column_view]    Script Date: 11/17/2025 2:58:11 PM ******/
-- ============================================================================
-- VIEW 2: VertiPaq Column View (FIXED - No Duplicates)
-- Provides column-level memory analysis with cardinality and encoding
-- ============================================================================
CREATE OR ALTER   VIEW [dbo].[vertipaq_column_view] AS
WITH 
-- Get column metadata
column_info AS (
    SELECT 
        DIMENSION_NAME AS Table_Name,
        ATTRIBUTE_NAME AS Column_Name,
        CASE DATATYPE
            WHEN 'DBTYPE_I8' THEN 'Int64'
            WHEN 'DBTYPE_WSTR' THEN 'String'
            WHEN 'DBTYPE_CY' THEN 'Decimal'
            WHEN 'DBTYPE_BOOL' THEN 'Boolean'
            WHEN 'DBTYPE_DATE' THEN 'DateTime'
            WHEN 'DBTYPE_R8' THEN 'Double'
            ELSE DATATYPE
        END AS Data_Type,
        DICTIONARY_SIZE AS Dictionary_Size,
        CASE COLUMN_ENCODING
            WHEN 1 THEN 'HASH'
            WHEN 2 THEN 'VALUE'
            ELSE CAST(COLUMN_ENCODING AS VARCHAR(10))
        END AS Column_Encoding
    FROM info_storage_columns
    WHERE COLUMN_TYPE = 'BASIC_DATA'
),
-- Get data size per column (aggregated to handle multiple segments)
data_size AS (
    SELECT 
        DIMENSION_NAME AS Table_Name,
        REPLACE(
            LEFT(COLUMN_ID, 
                CASE 
                    WHEN CHARINDEX(' (', COLUMN_ID) > 0 
                    THEN CHARINDEX(' (', COLUMN_ID) - 1 
                    ELSE LEN(COLUMN_ID) 
                END),
            'RowNumber ', 
            'RowNumber-'
        ) AS Column_Name,
        SUM(USED_SIZE) AS Data_Size
    FROM info_storage_segments
    WHERE RIGHT(LEFT(TABLE_ID, 2), 1) <> '$'
    GROUP BY DIMENSION_NAME, 
             REPLACE(
                LEFT(COLUMN_ID, 
                    CASE 
                        WHEN CHARINDEX(' (', COLUMN_ID) > 0 
                        THEN CHARINDEX(' (', COLUMN_ID) - 1 
                        ELSE LEN(COLUMN_ID) 
                    END),
                'RowNumber ', 
                'RowNumber-'
            )
),
-- Get hierarchy size per column (aggregated)
hier_size AS (
    SELECT 
        DIMENSION_NAME AS Table_Name,
        CASE 
            WHEN CHARINDEX('$', TABLE_ID, CHARINDEX('$', TABLE_ID) + 1) > 0 
            THEN LEFT(
                SUBSTRING(TABLE_ID, CHARINDEX('$', TABLE_ID, CHARINDEX('$', TABLE_ID) + 1) + 1, LEN(TABLE_ID)),
                CASE 
                    WHEN CHARINDEX(' (', SUBSTRING(TABLE_ID, CHARINDEX('$', TABLE_ID, CHARINDEX('$', TABLE_ID) + 1) + 1, LEN(TABLE_ID))) > 0
                    THEN CHARINDEX(' (', SUBSTRING(TABLE_ID, CHARINDEX('$', TABLE_ID, CHARINDEX('$', TABLE_ID) + 1) + 1, LEN(TABLE_ID))) - 1
                    ELSE LEN(SUBSTRING(TABLE_ID, CHARINDEX('$', TABLE_ID, CHARINDEX('$', TABLE_ID) + 1) + 1, LEN(TABLE_ID)))
                END
            )
            ELSE NULL
        END AS Column_Name,
        SUM(USED_SIZE) AS HierSize
    FROM info_storage_segments
    WHERE LEFT(TABLE_ID, 2) = 'H$' 
      AND SEGMENT_NUMBER = 0
    GROUP BY DIMENSION_NAME, TABLE_ID
),
-- Get table cardinality
table_cardinality AS (
    SELECT 
        DIMENSION_NAME AS Table_Name,
        SUM(ROWS_COUNT) AS Rows
    FROM info_storage_tables
    WHERE RIGHT(LEFT(TABLE_ID, 2), 1) <> '$'
    GROUP BY DIMENSION_NAME
),
-- Get column cardinality
column_cardinality AS (
    SELECT 
        DIMENSION_NAME AS Table_Name,
        CASE 
            WHEN CHARINDEX('$', TABLE_ID, CHARINDEX('$', TABLE_ID) + 1) > 0 
            THEN LEFT(
                SUBSTRING(TABLE_ID, CHARINDEX('$', TABLE_ID, CHARINDEX('$', TABLE_ID) + 1) + 1, LEN(TABLE_ID)),
                CASE 
                    WHEN CHARINDEX(' (', SUBSTRING(TABLE_ID, CHARINDEX('$', TABLE_ID, CHARINDEX('$', TABLE_ID) + 1) + 1, LEN(TABLE_ID))) > 0
                    THEN CHARINDEX(' (', SUBSTRING(TABLE_ID, CHARINDEX('$', TABLE_ID, CHARINDEX('$', TABLE_ID) + 1) + 1, LEN(TABLE_ID))) - 1
                    ELSE LEN(SUBSTRING(TABLE_ID, CHARINDEX('$', TABLE_ID, CHARINDEX('$', TABLE_ID) + 1) + 1, LEN(TABLE_ID)))
                END
            )
            ELSE NULL
        END AS Column_Name,
        ROWS_COUNT - 3 AS Column_Cardinality
    FROM info_storage_tables
    WHERE LEFT(TABLE_ID, 2) = 'H$'
),
-- Combine all column metrics
combined AS (
    SELECT 
        ci.Table_Name,
        ci.Column_Name,
        tc.Rows,
        COALESCE(cc.Column_Cardinality, 0) AS Cardinality,
        CASE 
            WHEN ds.Data_Size IS NULL AND ci.Column_Name LIKE '%RowNumber%' 
            THEN ci.Dictionary_Size
            ELSE COALESCE(ds.Data_Size, 0)
        END AS Data,
        ci.Dictionary_Size AS Dictionary,
        COALESCE(hs.HierSize, 0) AS Hier_Size,
        ci.Column_Encoding AS Encoding,
        ci.Data_Type
    FROM column_info ci
    LEFT JOIN data_size ds 
        ON ci.Table_Name = ds.Table_Name 
        AND ci.Column_Name = ds.Column_Name
    LEFT JOIN hier_size hs 
        ON ci.Table_Name = hs.Table_Name 
        AND ci.Column_Name = hs.Column_Name
    LEFT JOIN table_cardinality tc 
        ON ci.Table_Name = tc.Table_Name
    LEFT JOIN column_cardinality cc 
        ON ci.Table_Name = cc.Table_Name 
        AND ci.Column_Name = cc.Column_Name
)
SELECT 
    Table_Name,
    Column_Name,
    Rows,
    Cardinality,
    Data + Dictionary + Hier_Size AS Col_Size,
    Data,
    Dictionary,
    Hier_Size,
    Encoding,
    Data_Type,
    ROUND((Data + Dictionary + Hier_Size) * 100.0 / 
        NULLIF(SUM(Data + Dictionary + Hier_Size) OVER(), 0), 4) AS Percent_DB
FROM combined;
GO


