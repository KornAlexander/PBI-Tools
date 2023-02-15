USE [ReportServer] 

/*Documentation
This SQL statement retrieves a list of unused reports by querying the "ReportServer" database. 
The SELECT statement retrieves the distinct columns from a subquery that returns: 
report name, folder name, request type, last run date, and days since the report was last used. 
The subquery joins the "Catalog" table and "ExecutionLogStorage" table, and filters the results to exclude certain report types and actions. 
It groups the results by report name, folder name, and request type, and calculates the number of days since the report was last run. 
The outer query joins the ExecutionLogStorage table again to retrieve the user name of the user who last ran the report.*/

SELECT distinct a.* 
      ,l.UserName 
FROM
( 
     SELECT c.[name] as reportName 
           ,SUBSTRING(SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)),0,CHARINDEX('/',SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)))) Folder 
           ,CASE(RequestType) WHEN 0 THEN 'Interactive' WHEN 1 THEN 'Subscription' WHEN 2 THEN 'Refresh Cache' ELSE 'Unknown' END AS RequestType 
           ,MAX(l.TimeStart) AS lastRunDate 
		   ,DATEDIFF(day, MAX(l.TimeStart), getdate())  AS 'Days report unused'
     FROM Catalog c 
           INNER JOIN ExecutionLogStorage l ON l.ReportID = c.ItemID 
     WHERE c.Type NOT IN (1,3,5,8) 
           AND l.ReportAction IN(1,13) 
	
     GROUP BY c.[name] 
          ,SUBSTRING(SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)),0,CHARINDEX('/',SUBSTRING(path, CHARINDEX('/', path) + 1, LEN(path)))) 
          ,CASE(RequestType) WHEN 0 THEN 'Interactive' WHEN 1 THEN 'Subscription' WHEN 2 THEN 'Refresh Cache' ELSE 'Unknown' END 
		 --HAVING MAX(l.TimeStart) < getdate() - 30
) a 
INNER JOIN ExecutionLogStorage l ON l.TimeStart = a.lastRunDate
Order by A.[Days report unused] DESC
