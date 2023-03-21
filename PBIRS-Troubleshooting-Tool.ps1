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


$result = [System.Windows.Forms.MessageBox]::Show("Please be informed that this script collects and zips data from various report server database tables, the rsreportserver.config and all '.log' files. The script just zips all files to a destination you determine in a following step. Do you consent?", "Consent", "YesNo", "Information")

if ($result -eq "No") {
    Write-Host "Execution aborted by user." -ForegroundColor Red
    exit
}


#-------- Input Varibles ------------
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
$msg   = 'This will be the name of the Zip File'
$ResultFileName = [Microsoft.VisualBasic.Interaction]::InputBox($msg, $title, $ResultFileName)

#--------Checking if a variable is empty and aborting the process if so------------
if ([string]::IsNullOrEmpty($ResultFileName) -or [string]::IsNullOrEmpty($Folder) -or [string]::IsNullOrEmpty($PBIRSInstallationPath) -or [string]::IsNullOrEmpty($ReportserverDB) -or [string]::IsNullOrEmpty($serverInstancename)) {
    Write-Host "One or more required variables are empty. Aborting process." -ForegroundColor Red
    exit 1
}

#--------Other variables------------
$FolderLogs = Join-Path -Path $Folder "\Logs"
$PowerBILogs = $PBIRSInstallationPath + "PBIRS\LogFiles"
$RSreportserverConfigFile = $PBIRSInstallationPath + "PBIRS\ReportServer\RSreportserver.config"

#----------------------------------




#-------- SQL Command Subscription and Schedule Refresh----------
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

#-------------------------------

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
#-------------------------------

#--------- SQL Command for Event table -----------
$sqlcmdEvent = "select * from Event"
#-------------------------------------------------


#install module
Install-Module -Name SqlServer -AllowClobber -Scope CurrentUser

New-Item -ItemType Directory -Path  $Folder -Force
Write-Host "New Folder created $Folder"

New-Item -ItemType Directory -Path  $FolderLogs -Force
Write-Host "New Folder created $FolderLogs"

#-------------------------------Logfiles--------------------
# Get all .log files in the source folder
$files = Get-ChildItem $PowerBILogs -Filter *.log -File

# Loop through each file and move it to the destination folder
foreach ($file in $files) {
    $destinationFile = Join-Path $FolderLogs $file.Name

    Copy-Item $file.FullName $destinationFile
}
#-----------------------------------------------------------

#-------------------------------RSreportserver.config--------------------

  Copy-Item $RSreportserverConfigFile $FolderLogs -Force
#-----------------------------------------------------------

Write-Host "Exporting ReportServer Database Information"
#execute SQL commands to collect data
Invoke-Sqlcmd -ServerInstance $serverInstancename -Database $ReportserverDB -Query $sqlcmdSubscriptionScheduleRefresh | Export-Csv -NoTypeInformation "$Folder\SubscriptionScheduleRefresh.csv" -Force
Invoke-Sqlcmd -ServerInstance $serverInstancename -Database $ReportserverDB -Query $sqlcmdSubscriptionScheduleRefreshHistory | Export-Csv -NoTypeInformation "$Folder\SubscriptionScheduleRefreshHistory.csv" -Force
Invoke-Sqlcmd -ServerInstance $serverInstancename -Database $ReportserverDB -Query $sqlcmdExecutionLog3 | Export-Csv -NoTypeInformation "$Folder\ExecutionLog3.csv" -Force
Invoke-Sqlcmd -ServerInstance $serverInstancename -Database $ReportserverDB -Query $sqlcmdEvent  | Export-Csv -NoTypeInformation "$folder\Eventtable.csv" -Force

Write-Host "Successfully collected the ReportServer SQL Database info"

#Compress-Archive -Path $Folder -Destination $Folder

$zipFile = Join-Path -Path $Folder $ResultFileName
Compress-Archive -Path $Folder -DestinationPath $zipFile -force

Write-Host "Successfully zipped the collected files"

Get-ChildItem $Folder | Where-Object { $_.Extension -ne '.zip' } | Remove-Item -Recurse -Force

Write-Host "Successfully deleted non-zipped files"

[System.Windows.Forms.MessageBox]::Show("Please check the successful completion in $Folder", "Script Completed", "OKCancel", "Information")

#Add-Type -AssemblyName System.Windows.Forms [System.Windows.Forms.MessageBox]::Show("The file has been saved $Folder")
 
