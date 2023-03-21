<# 
.SYNOPSIS
Troubleshoot Power BI Report Server Issues

.Objective
This script has the objective to cover the data collection for the majority of troubleshooting scenarios related to Power BI Report Server

.DESCRIPTION
The sample scripts are not supported under any Microsoft standard support program or service. 
The sample scripts are provided AS IS without warranty of any kind. 
Microsoft further disclaims all implied warranties including, without limitation, any implied warranties of merchantability or of 
fitness for a particular purpose. The entire risk arising out of the use or performance of the sample scripts and documentation 
remains with you. In no event shall Microsoft, its authors, or anyone else involved in the creation, production, or delivery of 
the scripts be liable for any damages whatsoever (including, without limitation, damages for loss of business profits, 
business interruption, loss of business information, or other pecuniary loss) arising out of the use of or inability to use the 
sample scripts or documentation, even if Microsoft has been advised of the possibility of such damages. 
#>

#-------- Disclaimer to Start Process ------------
$result = [System.Windows.Forms.MessageBox]::Show(
"Please be informed that this script collects and zips data from various report server database tables, the rsreportserver.config and all '.log' files. 

The script just zips all files to a destination you determine in a following step. The files will not automatically be shared with anyone but yourself. 

We advice that you review the created files before sharing with anyone. 

Do you would like to proceed?", "Consent", "YesNo", "Information")

if ($result -eq "No") {
    Write-Host "Execution aborted by user." -ForegroundColor Red
    exit
}


#-------- Determining Input Varibles Through PopUp Message Boxes ------------ 
Add-Type -AssemblyName Microsoft.VisualBasic
[void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$title = 'Instancename'
$msg   = 'Enter your Instancename here:'
$serverInstancename = [Microsoft.VisualBasic.Interaction]::InputBox($msg, $title, 'localhost')

[void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$title = 'Report Server Database Name'
$msg   = 'Enter your Report Server Database Name here:'
$ReportserverDB = [Microsoft.VisualBasic.Interaction]::InputBox($msg, $title, 'ReportServer')

[void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$title = 'PBIRS Installation Path'
$msg   = 'Enter your PBIRS Installation Path here:'
$PBIRSInstallationPath = [Microsoft.VisualBasic.Interaction]::InputBox($msg, $title, 'C:\Program Files\Microsoft Power BI Report Server\')

$Folder = Join-Path $env:USERPROFILE "\Documents\ReportServerInvestigation"

[void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$title = 'Path Result Folder'
$msg   = 'This will be the path the collected documents will be saved in'
$Folder = [Microsoft.VisualBasic.Interaction]::InputBox($msg, $title, $Folder)

$ResultFileName = (Get-Date).ToString("yyMMdd") + $env:USERNAME + "Result.zip" 
[void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$title = 'Name Zip File'
$msg   = 'This will be the name of the zip file'
$ResultFileName = [Microsoft.VisualBasic.Interaction]::InputBox($msg, $title, $ResultFileName)

[void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$title = 'Time of Error'
$msg   = 'Please provide one or multiple precise timestamp(s) when you experienced the error. 

Just enter the date and time as text.'
$ErrorTime = [Microsoft.VisualBasic.Interaction]::InputBox($msg, $title, "Unknown")

#-------- Checking if a variable is empty and aborting the process if so ------------
if ([string]::IsNullOrEmpty($ResultFileName) -or [string]::IsNullOrEmpty($Folder) -or [string]::IsNullOrEmpty($PBIRSInstallationPath) -or [string]::IsNullOrEmpty($ReportserverDB) -or [string]::IsNullOrEmpty($serverInstancename) -or [string]::IsNullOrEmpty($ErrorTime)) {
    Write-Host "One or more required variables are empty. Aborting process." -ForegroundColor Red
    exit 1
}


#-------- Other variables ------------
$FolderLogs = Join-Path -Path $Folder "\Logs"
$PowerBILogs = $PBIRSInstallationPath + "PBIRS\LogFiles"
$RSreportserverConfigFile = $PBIRSInstallationPath + "PBIRS\ReportServer\RSreportserver.config"



#-------- SQL Command Subscription and Schedule Refresh ----------
$RenderFormatValue1 = '(//ParameterValue/Value[../Name="RenderFormat"])[1]'
$RenderFormatValue2 = '(//ParameterValue/Value[../Name="RENDER_FORMAT"])[1]'
$RenderFormatExpression = 'ISNULL(Convert(XML,sub.[ExtensionSettings]).value(' + "'$RenderFormatValue1', 'nvarchar(50)'), Convert(XML,sub.[ExtensionSettings]).value('$RenderFormatValue2', 'nvarchar(50)')) AS RenderFormat"
$SubjectValue = '(//ParameterValue/Value[../Name="Subject"])[1]'
$SubjectExpression = "Convert(XML,sub.[ExtensionSettings]).value('$SubjectValue', 'nvarchar(150)') AS [Subject]"

$sqlcmdSubscriptionScheduleRefresh = 
@"
SELECT rs.ReportID
,rs.SubscriptionID
,ROUND(TRY_CAST(cat.ContentSize AS FLOAT)/1048576,3) ContentSizeMB 
,SUBSTRING(cat.[Path],1,LEN(cat.[Path])-LEN(cat.[Name])) AS ReportFolder
,cat.[Name] AS ReportName
,sub.[Description] AS SubscriptionDescription
,sub.LastStatus
,sub.LastRunTime
,sub.EventType
,s.ScheduleID AS JobName
,'EXEC msdb.dbo.sp_start_job @job_name = ''' + CAST(s.ScheduleID AS VARCHAR(50)) + '''' AS RunSubscriptionManually
,s.StartDate
,s.EndDate
,CASE   WHEN s.RecurrenceType=1 THEN 'Once'  
    WHEN s.RecurrenceType=2 THEN 'Hourly'  
    WHEN s.RecurrenceType=3 THEN 'Daily'  
    WHEN s.RecurrenceType=4 AND s.WeeksInterval <= 1 THEN 'Daily'  
    WHEN s.RecurrenceType=4 AND s.WeeksInterval > 1 THEN 'Weekly'
        WHEN s.RecurrenceType=5 THEN 'Monthly (days)'  
        WHEN s.RecurrenceType=6 THEN 'Monthly (weeks)'  
END AS Recurrence
,s.MinutesInterval
,s.DaysInterval
,s.WeeksInterval
,s.DaysOfWeek
,s.DaysOfMonth
,s.[Month]
,s.MonthlyWeek
,$RenderFormatExpression
,$SubjectExpression
,sub.[Parameters]
,cat.[Path]
,cat.CreationDate  AS ReportCreateDate
,cat.ModifiedDate AS ReportSettingsModified
,u.UserName
,sub.DataSettings AS DataDrivenSettings
,sub.ExtensionSettings
,sub.OwnerID AS SubscriptionOwnerID
FROM dbo.ReportSchedule rs WITH (NOLOCK)
INNER JOIN dbo.Schedule s WITH (NOLOCK) ON rs.ScheduleID = s.ScheduleID
INNER JOIN dbo.[Catalog] cat WITH (NOLOCK) ON rs.ReportID = cat.ItemID
INNER JOIN dbo.Subscriptions sub WITH (NOLOCK) ON rs.SubscriptionID = sub.SubscriptionID
INNER JOIN dbo.Users u WITH (NOLOCK) ON sub.OwnerID = u.UserID
"@


#-------- SQL Command for Subscription History ----------
$sqlcmdSubscriptionScheduleRefreshHistory =
"
select *, 
DATEDIFF(MINUTE, StartTime , EndTime) AS MinuteDiff 
from dbo.SubscriptionHistory 
Order By MinuteDiff desc 
"


#--------- SQL Command for ExecutionLog3 ---------
$sqlcmdExecutionLog3 = "select * from executionlog3"


#--------- SQL Command for Event table -----------
$sqlcmdEvent = "select * from Event"

#--------- SQL Command for Configuration Info -----------
$sqlcmdConfigurationInfo = "SELECT * FROM [dbo].[ConfigurationInfo]"


#--------- Install Prereq SQL Module -------------
#install module
Install-Module -Name SqlServer -AllowClobber -Scope CurrentUser


#--------- Create Destination Folder and Subfolder -------
New-Item -ItemType Directory -Path  $Folder -Force
Write-Host "
New Folder created $Folder"

New-Item -ItemType Directory -Path  $FolderLogs -Force
Write-Host "
New Folder created $FolderLogs
"

#--------- Save Timestamp ----------------------------------------
$ErrorTime | Out-File -FilePath "$Folder\Timestamp.txt" -NoNewline -Encoding ASCII
Write-Host "Successfully Timestamp in txt file saved"

#--------- Logfiles ----------------------------------------
# Get all .log files in the source folder
$files = Get-ChildItem $PowerBILogs -Filter *.log -File

# Loop through each file and move it to the destination folder
foreach ($file in $files) {
    $destinationFile = Join-Path $FolderLogs $file.Name

    Copy-Item $file.FullName $destinationFile
}


#--------- RSreportserver.config ---------------------------
  Copy-Item $RSreportserverConfigFile $FolderLogs -Force
  Write-Host "Successfully collected the RSreportserver.config file
  "


#---------- Getting Database tables -------------------------------
#execute SQL commands to collect data
Invoke-Sqlcmd -ServerInstance $serverInstancename -Database $ReportserverDB -Query $sqlcmdSubscriptionScheduleRefresh | Export-Csv -NoTypeInformation "$Folder\SubscriptionScheduleRefresh.csv" -Force
Invoke-Sqlcmd -ServerInstance $serverInstancename -Database $ReportserverDB -Query $sqlcmdSubscriptionScheduleRefreshHistory | Export-Csv -NoTypeInformation "$Folder\SubscriptionScheduleRefreshHistory.csv" -Force
Invoke-Sqlcmd -ServerInstance $serverInstancename -Database $ReportserverDB -Query $sqlcmdExecutionLog3 | Export-Csv -NoTypeInformation "$Folder\ExecutionLog3.csv" -Force
Invoke-Sqlcmd -ServerInstance $serverInstancename -Database $ReportserverDB -Query $sqlcmdEvent  | Export-Csv -NoTypeInformation "$folder\Eventtable.csv" -Force
Invoke-Sqlcmd -ServerInstance $serverInstancename -Database $ReportserverDB -Query $sqlcmdConfigurationInfo  | Export-Csv -NoTypeInformation "$folder\ConfigurationInfo.csv" -Force


Write-Host "Collection of Data from Report Server Database"
Write-Host "Successfully collected ExecutionLog3 table"
Write-Host "Successfully collected Event table"
Write-Host "Successfully collected ConfigurationInfo table"
Write-Host "Successfully collected Subscription and Schedule Refresh Last Status"
Write-Host "Successfully collected Subscription and Schedule Refresh History
"


#---------- Zipping all Files -------------------------------
$zipFile = Join-Path -Path $Folder $ResultFileName
Compress-Archive -Path $Folder -DestinationPath $zipFile -force

Write-Host "Successfully zipped the collected files"


#---------- Removing non zipped Files -------------------------------
Get-ChildItem $Folder | Where-Object { $_.Extension -ne '.zip' } | Remove-Item -Recurse -Force

Write-Host "Successfully deleted non-zipped files"


#---------- Finished Message Box -------------------------------
[System.Windows.Forms.MessageBox]::Show("Please check the successful completion in $Folder", "Script Completed", "OKCancel", "Information")