SELECT 
  C.[ItemID],
  C.[Path],
  C.[Name],
  C.[ParentID],
  C.[Type],
  -- Use ISNULL and NULLIF to handle cases where the ParentItem is blank
  ISNULL(NULLIF(cp.[Name], ''), 'Root') AS ParentItem,
  -- Use a CASE statement to translate the Type column to a more readable ItemType value
  CASE c.[Type]
    WHEN 1 THEN 'Folder'
    WHEN 2 THEN 'Paginated Report'
    WHEN 3 THEN 'Resources'
    WHEN 4 THEN 'Linked Report'
    WHEN 5 THEN 'Data Source'
    WHEN 6 THEN 'Report Model'
    WHEN 7 THEN 'Report Part'
    WHEN 8 THEN 'Shared dataset'
    WHEN 11 THEN 'KPI Card'
    WHEN 13 THEN 'Power BI'
    WHEN 14 THEN 'Excel'
    ELSE CAST(c.[Type] AS VARCHAR(10))
  END AS ItemType,
  C.[Content],
  C.[Intermediate],
  C.[SnapshotDataID],
  C.[LinkSourceID],
  C.[Property],
  C.[Description],
  C.[Hidden],
  C.[CreatedByID],
  C.[CreationDate],
  C.[ModifiedByID],
  C.[ModifiedDate],
  C.[MimeType],
  C.[SnapshotLimit],
  C.[Parameter],
  C.[PolicyID],
  C.[PolicyRoot],
  C.[ExecutionFlag],
  C.[ExecutionTime],
  C.[SubType],
  C.[ComponentID],
  C.[ContentSize],
  -- Join the Users table to get the CreatedUserName and ModifiedUserName
  cu.UserName AS CreatedUserName,
  mu.UserName AS ModifiedUserName,
  -- Use a CASE statement to translate the PolicyRoot column to a more readable IsCustomizedItemSecurity value
  CASE c.PolicyRoot
    WHEN 0 THEN 'No'
    WHEN 1 THEN 'Yes'
    ELSE CAST(c.PolicyRoot AS VARCHAR(10))
  END AS IsCustomizedItemSecurity,
  -- Calculate the ContentSize in MB
  ISNULL(CAST(c.ContentSize AS FLOAT) / CAST((1024 * 1024) AS FLOAT), 0) AS ContentSizeMb,
  -- Use a CASE statement to determine if the ContentLocation is a User Folder or a Shared Folder
  CASE WHEN LEFT(c.[Path], 14) = '/Users Folders' THEN 'User Folder' ELSE 'Shared Folder' END as ContentLocation,
  -- Join the ExecutionLogStorage table to get the LastRequestType, lastRunDate, Days report unused, and LastUsedBy columns
  
  intr.lastRunDateInteractive,
  intr.[Days report unused],
    sub.lastRunDateSubscription,
	refr.lastRunDateRefresh,
	sub.[Days report unused] AS 'Days report unused Subscription',
	refr.[Days report unused] AS 'Days report unused Refreshes',
  --l.UserName AS LastUsedBy,
    MEM.DataModelMBs,
    MEM.DSType,
    MEM.DSKind,
    MEM.AuthType,
    MEM.HasEmbeddedModels,
    MEM.PbixShredderVersion,
    MEM.ModelRefreshAllowed,
    MEM.HasDirectQuery,
    MEM.ModelVersion,
	    CASE
        WHEN intr.[Days report unused] >= 0 THEN 'No'
		When C.[Type] = 1 THEN 'No'
        ELSE 'Yes'
    END AS 'IsReportUnused'
FROM [dbo].[Catalog] C
-- Join the Catalog table to itself to get the ParentItem value
LEFT JOIN dbo.[Catalog] cp ON c.ParentID = cp.ItemID
-- Join the Users table to get the CreatedUserName and ModifiedUserName
LEFT JOIN dbo.Users cu ON c.CreatedByID = cu.UserID
LEFT JOIN dbo.Users mu ON c.ModifiedByID = mu.UserID
-- Join the subquery that calculates the LastRequestType, lastRunDate, Days report unused, and LastUsedBy columns
LEFT JOIN (
	SELECT 
		c.[itemid] 
		-- Use the SUBSTRING function to extract the folder name from the Path column
		--,SUBSTRING(SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)),0,CHARINDEX('/',SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)))) Folder 
        ,MAX(l.TimeStart) AS lastRunDateInteractive
		,DATEDIFF(day, MAX(l.TimeStart), getdate())  AS 'Days report unused'
    FROM Catalog c 
		INNER JOIN ExecutionLogStorage l ON l.ReportID = c.ItemID
	Where RequestType = 0
    GROUP BY c.[itemid]
           --,SUBSTRING(SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)),0,CHARINDEX('/',SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)))) 
           --,CASE(RequestType) WHEN 0 THEN 'Interactive' WHEN 1 THEN 'Subscription' WHEN 2 THEN 'Refresh Cache' ELSE 'Unknown' END 
             --,l.UserName
) intr ON C.ItemID = intr.itemid

LEFT JOIN (
	SELECT 
		c.[itemid] 
		-- Use the SUBSTRING function to extract the folder name from the Path column
		--,SUBSTRING(SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)),0,CHARINDEX('/',SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)))) Folder 
        ,MAX(l.TimeStart) AS lastRunDateSubscription
		,DATEDIFF(day, MAX(l.TimeStart), getdate())  AS 'Days report unused'
    FROM Catalog c 
		INNER JOIN ExecutionLogStorage l ON l.ReportID = c.ItemID
	Where RequestType = 1
    GROUP BY c.[itemid]
           --,SUBSTRING(SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)),0,CHARINDEX('/',SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)))) 
           --,CASE(RequestType) WHEN 0 THEN 'Interactive' WHEN 1 THEN 'Subscription' WHEN 2 THEN 'Refresh Cache' ELSE 'Unknown' END 
           --,l.UserName
) sub ON C.ItemID = sub.itemid


LEFT JOIN (
	SELECT 
		c.[itemid] 
		-- Use the SUBSTRING function to extract the folder name from the Path column
		--,SUBSTRING(SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)),0,CHARINDEX('/',SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)))) Folder 
        ,MAX(l.TimeStart) AS lastRunDateRefresh
		,DATEDIFF(day, MAX(l.TimeStart), getdate())  AS 'Days report unused'
    FROM Catalog c 
		INNER JOIN ExecutionLogStorage l ON l.ReportID = c.ItemID
	Where RequestType = 2
    GROUP BY c.[itemid]
           --,SUBSTRING(SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)),0,CHARINDEX('/',SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)))) 
           --,CASE(RequestType) WHEN 0 THEN 'Interactive' WHEN 1 THEN 'Subscription' WHEN 2 THEN 'Refresh Cache' ELSE 'Unknown' END 
           --,l.UserName
) Refr ON C.ItemID = Refr.itemid

LEFT JOIN (
	SELECT 
		c.[itemid] 
		-- Use the SUBSTRING function to extract the folder name from the Path column
		--,SUBSTRING(SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)),0,CHARINDEX('/',SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)))) Folder 
        ,MAX(l.TimeStart) AS lastRunDateSubscription
		,DATEDIFF(day, MAX(l.TimeStart), getdate())  AS 'Days report unused'
    FROM Catalog c 
		INNER JOIN ExecutionLogStorage l ON l.ReportID = c.ItemID
	Where RequestType = 1
    GROUP BY c.[itemid]
           --,SUBSTRING(SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)),0,CHARINDEX('/',SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)))) 
           --,CASE(RequestType) WHEN 0 THEN 'Interactive' WHEN 1 THEN 'Subscription' WHEN 2 THEN 'Refresh Cache' ELSE 'Unknown' END 
           --,l.UserName
) b ON C.ItemID = b.itemid
--Left JOIN ExecutionLogStorage l ON l.TimeStart = a.lastRunDate -- to get lastusedby
LEFT JOIN (
	SELECT
		CAT.ItemID AS ReportID,
		DMS.DataModelMBs,
		DS.DSType,
		DS.DSKind AS DSKind,
		DS.AuthType,
		CAST([Property] as xml).value('(/Properties/HasEmbeddedModels)[1]','nvarchar(max)') AS HasEmbeddedModels,
		CAST([Property] as xml).value('(/Properties/PbixShredderVersion)[1]','nvarchar(max)') AS PbixShredderVersion,
		CAST([Property] as xml).value('(/Properties/ModelRefreshAllowed)[1]','nvarchar(max)') AS ModelRefreshAllowed,
		CAST([Property] as xml).value('(/Properties/HasDirectQuery)[1]','nvarchar(max)') AS HasDirectQuery,
		CAST([Property] as xml).value('(/Properties/ModelVersion)[1]','nvarchar(max)') AS ModelVersion
	FROM [Catalog] AS CAT
	LEFT OUTER JOIN DataModelDataSource AS DS
	    ON DS.ItemID = CAT.ItemID
	LEFT OUTER JOIN (
	    SELECT
	        C.ItemId,
	        DATALENGTH(C.Content)/1048576 AS DataModelMBs
	    FROM CatalogItemExtendedContent AS C
	    WHERE C.ContentType = 'DataModel'
		) AS DMS
	    ON DMS.ItemId = CAT.ItemID
		WHERE CAT.Type = 13) 
MEM On MEM.ReportID = C.ItemID
