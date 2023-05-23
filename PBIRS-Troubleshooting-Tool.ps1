<# 
SYNOPSIS:
Data Collection Script for Power BI Report Server Issues

Objective:
This script has the objective to cover and simplify the data collection for the majority of troubleshooting scenarios related to Power BI Report Server.

Important Disclaimer:
The script is not supported under any Microsoft standard support program or service. 
The script is provided AS IS without warranty of any kind. 
Microsoft further disclaims all implied warranties including, without limitation, any implied warranties of merchantability or of 
fitness for a particular purpose. The entire risk arising out of the use or performance of the sample scripts and documentation 
remains with you. In no event shall Microsoft, its authors, or anyone else involved in the creation, production, or delivery of 
the scripts be liable for any damages whatsoever (including, without limitation, damages for loss of business profits, 
business interruption, loss of business information, or other pecuniary loss) arising out of the use of or inability to use the 
sample scripts or documentation, even if Microsoft has been advised of the possibility of such damages. 

The collected data may contain Personally Identifiable Information (PII) and/or sensitive data, such as usernames.
Therefore we advice that you review the created files before sharing it with anyone. 
#>

#-------- Change here the preselected checkmarks for topics ------------
$checkedItems = @(1, 2, 3, 4, 5, 6, 7, 8)


#-------- Disclaimer to Start Process ------------
Add-Type -AssemblyName Microsoft.VisualBasic
[void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$title = 'Info'
$message = 'Description: 
Please be informed that this "Report Server Data Collection" script is designed to collect information that will help Microsoft Customer Support Services (CSS) troubleshoot an issue you may be experiencing with your report server.
The script zips data from various sources to a destination you determine in a following step. The files will not automatically be shared with anyone but yourself. 

Please select scope in subsequent step from topics such as:
- various report server database tables
- RS configuration info such as rsreportserver.config
- all RS ".log" files.
- system and application log: Error/warnings

Disclaimer: Review the created files
The collected data may contain Personally Identifiable Information (PII) and/or sensitive data, such as usernames.
Therefore we advice that you review the created files before sharing it with anyone. 

Do you would like to proceed?'
$confirm = [Microsoft.VisualBasic.Interaction]::MsgBox(
    $message,
    [Microsoft.VisualBasic.MsgBoxStyle]::Information + [Microsoft.VisualBasic.MsgBoxStyle]::YesNo,
    $title
)
if ($confirm -eq 'No') {
    Write-Host 'Execution aborted by user.' -ForegroundColor Red
    exit
}

#-------- Names for selection Options ------------ 
$SelectionOption1 = "RS configuration info"
$SelectionOption2 = "RS logs"
$SelectionOption3 = "ExecutionLog3"
$SelectionOption4 = "subscription / schedule refresh / event table"
$SelectionOption5 = "URL Reservations / Netsh"
$SelectionOption6 = "System and application log: Error/Warnings"
$SelectionOption7 = "authentication scripts from aka.ms/authscripts"
$SelectionOption8 = "Report Server Language Settings"
$SelectionOption9 = "Basic Info: Timestamp, Name of Report, RS Version"
$SelectionOption10 = "RS web.config / rssrvpolicy.config files"
$SelectionOption11 = "Create Backup of rsreportserver.config file"
$SelectionOption12 = "netsh information urlacl and sslcert"


#-------- PopUp to determine which data will be collected ------------ 
Add-Type -AssemblyName System.Windows.Forms
$title = 'Topic Selector'
$msg   = 
$options = @($SelectionOption9, $SelectionOption1,$SelectionOption2, $SelectionOption3, $SelectionOption4, $SelectionOption5, $SelectionOption8, $SelectionOption10, $SelectionOption11, $SelectionOption6, $SelectionOption7)
$checkedListBox = New-Object System.Windows.Forms.CheckedListBox
$checkedListBox.Items.AddRange($options)
$checkedListBox.CheckOnClick = $true # set CheckOnClick property to true
$checkedListBox.Top = 20 # adjust position from the top
$checkedListBox.Left = 20 # adjust position to the right
$checkedListBox.Width = 250
$checkedListBox.Height = 140
$form = New-Object System.Windows.Forms.Form
$form.StartPosition = 'CenterScreen' # set StartPosition property to CenterScreen
$form.Text = $title
$form.Width = 300
$form.Height = 240 # increase height to avoid overlapping
$form.Controls.Add($checkedListBox)
$label = New-Object System.Windows.Forms.Label
$label.Text = 'Select the topics you want to collect:'
$label.Left = 20 # adjust position to the right
$label.Width = 250
$form.Controls.Add($label)
$okButton = New-Object System.Windows.Forms.Button
$okButton.Text = 'OK'
$okButton.DialogResult = [System.Windows.Forms.DialogResult]::OK
$okButton.Left = 150 - $okButton.Width / 2
$okButton.Top = 170 # increase top position to avoid overlapping
$form.AcceptButton = $okButton
$form.Controls.Add($okButton)

# set the first four checkboxes to be checked by default
#for ($i = 0; $i -lt 5; $i++) {
#    $checkedListBox.SetItemChecked($i, $true)
#}

# set the checkboxes to be checked by default as previously defined
foreach ($item in $checkedItems) {
    $checkedListBox.SetItemChecked($item -1, $true)
}

$result = $form.ShowDialog()
$SelectedOption = @{}

if ($result -eq [System.Windows.Forms.DialogResult]::OK) {
    # Check if any items are selected in the checkedListBox
    if ($checkedListBox.CheckedItems.Count -eq 0) {
        Write-Host "Please select at least one topic to collect data for. Aborting process" -ForegroundColor Red
        exit 1  # abort the process with an error code
    }
    
    # Proceed with the rest of the script
    foreach ($option in $options) {
        $SelectedOption[$option] = $checkedListBox.CheckedItems.Contains($option)
    }
}

Write-Host $SelectionOption9": $($SelectedOption[$SelectionOption9])"
Write-Host $SelectionOption1": $($SelectedOption[$SelectionOption1])"
Write-Host $SelectionOption2": $($SelectedOption[$SelectionOption2])"
Write-Host $SelectionOption3": $($SelectedOption[$SelectionOption3])"
Write-Host $SelectionOption4": $($SelectedOption[$SelectionOption4])"
Write-Host $SelectionOption5": $($SelectedOption[$SelectionOption5])"
Write-Host $SelectionOption8": $($SelectedOption[$SelectionOption8])"
Write-Host $SelectionOption6": $($SelectedOption[$SelectionOption6])"
Write-Host $SelectionOption7": $($SelectedOption[$SelectionOption7])"
Write-Host $SelectionOption10": $($SelectedOption[$SelectionOption10])"
Write-Host $SelectionOption11": $($SelectedOption[$SelectionOption11])"

#-------- Checking which version is installed in all drives ------------
$drive = $null
$drives = $null
$drives = Get-PSDrive -PSProvider FileSystem | Select-Object -ExpandProperty Name

Write-Host "drives found: $drives"

$paths = @(
    "Microsoft Power BI Report Server\PBIRS",
    "Microsoft SQL Server Reporting Services\SSRS",
    "Microsoft SQL Server\MSRS13.MSSQLSERVER\Reporting Services"
)

$PBIRSPath = $null
$SSRS2017OrLater = $null
$SSRS2016OrEarlier = $null

foreach ($path in $paths) {
    $found = $false  # Flag to track if path is found on any drive
    foreach ($drive in $drives) {
        $fullPath = "${drive}:\\Program Files\$path"
        if (Test-Path $fullPath) {
            switch ($path) {
                "Microsoft Power BI Report Server\PBIRS" {
                    $PBIRSPath = "$fullPath"
                }
                "Microsoft SQL Server Reporting Services\SSRS" {
                    $SSRS2017OrLaterPath = "$fullPath"
                }
                "Microsoft SQL Server\MSRS13.MSSQLSERVER\Reporting Services" {
                    $SSRS2016OrEarlierPath = "$fullPath"
                }
            }
            $found = $true
            Write-Host "Found $fullPath"
            break  # Exit the inner loop since path is found on a drive
        }
    }

    if (-not $found) {
        switch ($path) {
            "Microsoft Power BI Report Server\PBIRS" {
                $PBIRSPath = "No"
            }
            "Microsoft SQL Server Reporting Services\SSRS" {
                $SSRS2017OrLaterPath = "No"
            }
            "Microsoft SQL Server\MSRS13.MSSQLSERVER\Reporting Services" {
                $SSRS2016OrEarlierPath = "No"
            }
        }
        Write-Host "Not Found $path"
    }
}

# Display the output messages
Write-Output "PBIRS Path if exists = $PBIRSPath"
Write-Output "SSRS2017OrLater Path if Exists = $SSRS2017OrLaterPath"
Write-Output "SSRS2016OrEarlier Path if Exists = $SSRS2016OrEarlierPath"



#-------- Stating where the file will be saved in and checking if Folder is Empty ------------ 
$Folder = Join-Path $env:USERPROFILE "\Documents\ReportServerInvestigation"

[void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$title = 'Path Result Folder'
$msg   = 'This will be the path the collected documents will be saved in. The script automatically opens this path upon completion.

Please make sure to select an empty or non-existent folder.'
$Folder = [Microsoft.VisualBasic.Interaction]::InputBox($msg, $title, $Folder)

if (Test-Path $Folder) {
$items = Get-ChildItem $Folder | Measure-Object
if ($items.Count -gt 0) {

Add-Type -AssemblyName Microsoft.VisualBasic
[void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$title = 'Path already exists'
$message = 'The selected path already contains data. 
Please make sure the selected folder is empty. 

Click Okay to open the currently selefolder'
$confirm = [Microsoft.VisualBasic.Interaction]::MsgBox(
    $message,
    [Microsoft.VisualBasic.MsgBoxStyle]::Information + [Microsoft.VisualBasic.MsgBoxStyle]::Okay,
    $title
)
Write-Host "There are already items in the folder. Please select an empty folder or define a path which does not exist yet. Aborting the process." -ForegroundColor Red
#---------- Opening Folder Path -------------------------------
Invoke-Item $Folder

return
} else {
Write-Host "The folder exists, but is empty."
}
} else {
Write-Host "The folder doesn't exist."
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

#-------- Based on the logic 
if ($PBIRSPath -eq "No") {
    Write-Host "PBIRS not installed going to the next check"

    if ($SSRS2017OrLaterPath -eq "No") {
        Write-Host "SSRS 2017 not installed going to the next check"

        if ($SSRS2016OrEarlierPath -eq "No") {
            Write-Host "SSRS 2016 not installed going to the next check"
        } else {
                [void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$title = 'SSRS Installation Path'
$msg   = 'We recognized you are using SSRS 2016 or an earlier version and have it installed in the following path. Please confirm if this is the instance you want to work with.
If you want to analyze a different instance please manually change the installation path'
$PBIRSInstallationPath = [Microsoft.VisualBasic.Interaction]::InputBox($msg, $title, "$SSRS2016OrEarlierPath\")
        }
    } else {
    [void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$title = 'SSRS 2017 Installation Path'
$msg   = 'We recognized you are using SSRS 2017 or a later version and have it installed in the following path. Please confirm if this is the instance you want to work with.
If you want to analyze a different instance please manually change the installation path'
$PBIRSInstallationPath = [Microsoft.VisualBasic.Interaction]::InputBox($msg, $title, "$SSRS2017OrLaterPath\")
    }
} else {
    [void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$title = 'PBIRS Installation Path'
$msg   = 'We recognized you are using Power BI Report Server and have it installed in the following path. Please confirm if this is the instance you want to work with.
If you want to analyze a different instance or SSRS please change the installation path manually'
$PBIRSInstallationPath = [Microsoft.VisualBasic.Interaction]::InputBox($msg, $title, "$PBIRSPath\")
}


#------------ Other prompts for input variables  ------------ 

$ResultFileName = (Get-Date).ToString("yyMMdd") + $env:USERNAME + "Result.zip" 
[void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$title = 'Name Zip File'
$msg   = 'This will be the name of the zip file'
$ResultFileName = [Microsoft.VisualBasic.Interaction]::InputBox($msg, $title, $ResultFileName)

if ($SelectedOption[$SelectionOption6]) {
$startDate = (Get-Date).AddDays(-1)
[void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$title = 'Start of Event Log'
$msg   = 'This will be the starting day the application and system log will be collected from. 

If you had the issue earlier please modify'
$startDate = [Microsoft.VisualBasic.Interaction]::InputBox($msg, $title, $startDate)
}

if ($SelectedOption[$SelectionOption6]) {
$endDate = Get-Date
[void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$title = 'End of Event Log'
$msg   = 'The application and system log will be collected till today. 

To limit the data selection feel free to modify the collection to end at an earlier day.'
$endDate = [Microsoft.VisualBasic.Interaction]::InputBox($msg, $title, $endDate)
}

if ($SelectedOption[$SelectionOption9]) {
[void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$title = 'Time of Error'
$msg   = 'Please provide one or multiple precise timestamp(s) when you experienced the error. 

Just enter the date and time as text.'
$ErrorTime = [Microsoft.VisualBasic.Interaction]::InputBox($msg, $title, "Unknown")

[void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$title = 'Report with Error'
$msg   = 'Please provide one or multiple report names you are having issues with. 

If you have issues with all reports please leave ALL'
$ImpactedReport = [Microsoft.VisualBasic.Interaction]::InputBox($msg, $title, "All")
}

#-------- Checking if a variable is empty and aborting the process if so ------------
if ([string]::IsNullOrEmpty($ResultFileName) -or [string]::IsNullOrEmpty($Folder) -or [string]::IsNullOrEmpty($PBIRSInstallationPath) -or [string]::IsNullOrEmpty($ReportserverDB) -or [string]::IsNullOrEmpty($serverInstancename) -or [string]::IsNullOrEmpty($ErrorTime)) {
    Write-Host "One or more required variables are empty. Aborting process." -ForegroundColor Red
    exit 1
}


#-------- Other variables ------------
$FolderLogs = Join-Path -Path $Folder "\Logs"
$PowerBILogs = $PBIRSInstallationPath + "\LogFiles"
$RSreportserverConfigFile = $PBIRSInstallationPath + "ReportServer\RSreportserver.config"
$ApplicationLogFile = $Folder+"\ApplicationLog.csv"
$SystemLogFile = $Folder+"\SystemLog.csv"

$rssrvpolicyConfigFile = $PBIRSInstallationPath + "ReportServer\rssrvpolicy.config"
$webConfigFile = $PBIRSInstallationPath + "ReportServer\web.config"


#-------- SQL Command Subscription and Schedule Refresh ----------
$RenderFormatValue1 = '(//ParameterValue/Value[../Name="RenderFormat"])[1]'
$RenderFormatValue2 = '(//ParameterValue/Value[../Name="RENDER_FORMAT"])[1]'
$RenderFormatExpression = 'ISNULL(Convert(XML,sub.[ExtensionSettings]).value(' + "'$RenderFormatValue1', 'nvarchar(50)'), Convert(XML,sub.[ExtensionSettings]).value('$RenderFormatValue2', 'nvarchar(50)')) AS RenderFormat"
$SubjectValue = '(//ParameterValue/Value[../Name="Subject"])[1]'
$SubjectExpression = "Convert(XML,sub.[ExtensionSettings]).value('$SubjectValue', 'nvarchar(150)') AS [Subject]"

$sqlcmdSubscriptionScheduleRefreshSSRS = 
@"
SELECT rs.ReportID
,rs.SubscriptionID
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


$sqlcmdSubscriptionScheduleRefreshPBIRS = 
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
Set-PSRepository -Name 'PSGallery' -InstallationPolicy Trusted
Install-Module -Name SqlServer -AllowClobber -Scope CurrentUser


#--------- Create Destination Folder and Subfolder -------
New-Item -ItemType Directory -Path  $Folder -Force
Write-Host "
New Folder created $Folder"

if ($SelectedOption[$SelectionOption2]) {
New-Item -ItemType Directory -Path  $FolderLogs -Force
Write-Host "
New Folder created $FolderLogs
"
}

#--------- Starting process to collect information -----------------------------------------



#--------- Check version in ReportingServicesService log file -----------------------------------------

# Specify the directory path where the files are located
$directoryPath = "$PBIRSInstallationPath\LogFiles"

# Search for files with names starting with "ReportingServicesService" in the specified directory
$files = Get-ChildItem -Path $directoryPath -Filter "ReportingServicesService_*" | Sort-Object LastWriteTime -Descending

if ($files.Count -gt 0) {
    # Get the most recently modified file
    $mostRecentFile = $files[0].FullName

    try {
        # Read the contents of the most recently modified file
        $fileContent = Get-Content -Path $mostRecentFile -Raw

        # Extract the version using regex
        $pattern = "(?<=<Product>)(.*?)(?=</Product>)"
        $matches = [regex]::Matches($fileContent, $pattern)
        if ($matches.Count -gt 0) {
            $installedVersion = $matches[0].Value

            Write-Host "Version found in $mostRecentFile"
            Write-Host "Installed version: $installedVersion"
        } else {
            Write-Host "Version not found in $mostRecentFile."
        }
    }
    catch {
        Write-Host "Failed to read the file $mostRecentFile."
    }
}
else {
    Write-Host "No files starting with 'ReportingServicesService' found in the specified directory."
}

if ($SelectedOption[$SelectionOption9]) {
#--------- Save Timestamp and Report Name---------------------------
$ErrorTime_ImpactedReport = [PSCustomObject]@{
    "ErrorTime" = $ErrorTime
    "ImpactedReport" = $ImpactedReport
    "Version" = $installedVersion
    }

$ErrorTime_ImpactedReport | Export-Csv -Path "$Folder\BasicInfo_Timestamp_ReportName_Version.csv" -NoTypeInformation
Write-Host "Successfully Timestamp in txt file saved"
Write-Host "Successfully ImpactedReport in txt file saved"
}

<#
#--------- Save Timestamp ----------------------------------------
$ErrorTime | Out-File -FilePath "$Folder\Timestamp.txt" -NoNewline -Encoding ASCII
Write-Host "Successfully Timestamp in txt file saved"

#--------- Save ReportName ----------------------------------------
$ImpactedReport | Out-File -FilePath "$Folder\ImpactedReportName.txt" -NoNewline -Encoding ASCII
Write-Host "Successfully ImpactedReport in txt file saved"
#>

#--------- Retrieve the Windows application logs for the specified date range----
if ($SelectedOption[$SelectionOption6]) {
$Applicationlogs = Get-WinEvent -FilterHashtable @{
    LogName = "Application"
    StartTime = $startDate
    EndTime = $endDate
    Level = 1,2,3  # Warning, Error, Critical
}


# Output the logs to a CSV file
$Applicationlogs | Export-Csv -Path $ApplicationLogFile -NoTypeInformation

Write-Host "Successfully Application Logs as csv saved - Event Level Information and Verbose EXCLUDED"
}

#--------- Retrieve just URL Reservations----
if ($SelectedOption[$SelectionOption5]) {

$tableURLRes = netsh http show urlacl | Select-String -Pattern '(?<=Reserved URL\s+: )(.+)' | ForEach-Object {
    $_.Matches.Value.Split(',').Trim() | ForEach-Object {
        New-Object -TypeName PSObject -Property @{
            "Reserved URL" = $_
        }
    }
}

$tableURLRes | Export-Csv -Path "$Folder\URL_Reservations_OnlyURLs.csv" -NoTypeInformation


Write-Host "Successfully Just Reservations collected"
}

#--------- Retrieve full show URLACL----
if ($SelectedOption[$SelectionOption5]) {

netsh http show urlacl | ForEach-Object { $_.Trim() } | ConvertFrom-String -Delimiter ": " | Select-Object @{Name="Property";Expression={$_.P1}}, @{Name="Value";Expression={$_.P2}} | Export-Csv -Path "$Folder\URL_Reservations_Full_ShowURLacl.csv" -NoTypeInformation

Write-Host "Successfully Full Show URLACL collected"
}

#--------- Retrieve SSLCert ----
if ($SelectedOption[$SelectionOption5]) {

netsh http show sslcert > "$Folder\URL_Reservations_SSLCERT.txt"

Write-Host "Successfully netsh SSLCert collected"
}

#--------- Retrieve the Windows system logs for the specified date range----
if ($SelectedOption[$SelectionOption6]) {
$Systemlogs = Get-WinEvent -FilterHashtable @{
    LogName = "System"
    StartTime = $startDate
    EndTime = $endDate
    Level = 1,2,3  # Warning, Error, Critical
}

# Output the logs to a CSV file
$Systemlogs | Export-Csv -Path $SystemLogFile -NoTypeInformation

Write-Host "Successfully System Logs as csv saved - Event Level Information and Verbose EXCLUDED"
}


#--------- Logfiles ----------------------------------------
if ($SelectedOption[$SelectionOption2]) {
# Get all .log files in the source folder
$files = Get-ChildItem $PowerBILogs -Filter *.log -File

# Loop through each file and move it to the destination folder
foreach ($file in $files) {
    $destinationFile = Join-Path $FolderLogs $file.Name

    Copy-Item $file.FullName $destinationFile
}
Write-Host "Successfully Report Server Logs collected"
}

#--------- RSreportserver.config ---------------------------
if ($SelectedOption[$SelectionOption1]) {
  Copy-Item $RSreportserverConfigFile $Folder -Force
  Write-Host "Successfully collected the RSreportserver.config file
  "
  }

#--------- RS srvpolicy and web.config ---------------------------
if ($SelectedOption[$SelectionOption10]) {
  Copy-Item $rssrvpolicyConfigFile $Folder -Force
  Copy-Item $webConfigFile $Folder -Force
  Write-Host "Successfully collected the Web.config Rssrvpolicy.config files
  "
  }

#---------- Getting Database tables -------------------------------
#execute SQL commands to collect data
#Write-Host "Collection of Data from Report Server Database"
if ($SelectedOption[$SelectionOption4]) {

if ($PBIRSInstallationPath -notlike "*Power BI Report Server*") {
    Write-Host "PBIRS not installed going to the next check"
    
    if ($PBIRSInstallationPath -notlike "*SQL Server*") {
        Write-Host "SSRS not installed going to the next check"
    } else {Invoke-Sqlcmd -ServerInstance $serverInstancename -Database $ReportserverDB -Query $sqlcmdSubscriptionScheduleRefreshSSRS | Export-Csv -NoTypeInformation "$Folder\SubscriptionScheduleRefresh.csv" -Force
Write-Host "Successfully collected Subscription and Schedule Refresh Last Status"
}
    } else {Invoke-Sqlcmd -ServerInstance $serverInstancename -Database $ReportserverDB -Query $sqlcmdSubscriptionScheduleRefreshPBIRS | Export-Csv -NoTypeInformation "$Folder\SubscriptionScheduleRefresh.csv" -Force
    Invoke-Sqlcmd -ServerInstance $serverInstancename -Database $ReportserverDB -Query $sqlcmdSubscriptionScheduleRefreshHistory | Export-Csv -NoTypeInformation "$Folder\SubscriptionScheduleRefreshHistory.csv" -Force
    Write-Host "Successfully collected Subscription and Schedule Refresh Last Status"
    Write-Host "Successfully collected Subscription and Schedule Refresh History"}
}
if ($SelectedOption[$SelectionOption3]) {
Invoke-Sqlcmd -ServerInstance $serverInstancename -Database $ReportserverDB -Query $sqlcmdExecutionLog3 | Export-Csv -NoTypeInformation "$Folder\ExecutionLog3.csv" -Force
Write-Host "Successfully collected ExecutionLog3 table"
}

if ($SelectedOption[$SelectionOption4]) {
Invoke-Sqlcmd -ServerInstance $serverInstancename -Database $ReportserverDB -Query $sqlcmdEvent  | Export-Csv -NoTypeInformation "$folder\Eventtable.csv" -Force
Write-Host "Successfully collected Event table"
}

if ($SelectedOption[$SelectionOption1]) {
Invoke-Sqlcmd -ServerInstance $serverInstancename -Database $ReportserverDB -Query $sqlcmdConfigurationInfo  | Export-Csv -NoTypeInformation "$folder\ConfigurationInfo.csv" -Force
Write-Host "Successfully collected ConfigurationInfo table
"
}

#---------- Language Information -------------------------------
if ($SelectedOption[$SelectionOption8]) {

$CurrentCulture = Get-Culture # Get current culture
$CurrentUICulture = Get-UICulture # Get current UI culture
$SystemLocale = Get-WinSystemLocale # Get system default locale for non-Unicode programs
$UserLanguageList = Get-WinUserLanguageList # Get list of input languages and their settings for the current user

# Create an object containing the language information
$LanguageInfo = [PSCustomObject]@{
    "Current Culture" = $CurrentCulture.Name
    "Current UI Culture" = $CurrentUICulture.Name
    "System Default Locale" = $SystemLocale.Name
    "User Language List" = ($UserLanguageList | Format-Table | Out-String)
}

# Save the language information to a CSV file
$LanguageInfo | Export-Csv -Path "$Folder\LanguageInfo.csv" -NoTypeInformation

Write-Output "Successfully PC Language collected"
}


#---------- Action create a copy of rsreportserver.config ------------------------------
if ($SelectedOption[$SelectionOption11]) {
$destinationFolder = $PBIRSInstallationPath + "ReportServer\"
$timestamprsconfig = Get-Date -Format "yyyyMMddHHmmss"
$destinationPathrsconfig = Join-Path -Path $destinationFolder -ChildPath "rsreportserver_$timestamprsconfig.config"

# Copy the file to the destination
Copy-Item $RSreportserverConfigFile $destinationPathrsconfig

# Output the copied file's path
Write-Host "Successfully rsreportserver.config backed-up to: $destinationPathrsconfig
  "
  }

#---------- Authentication Scripts -------------------------------

if ($SelectedOption[$SelectionOption7]) {
    
# Check if PowerShell is running with administrative privileges
if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "Please run this script as an administrator."
    [System.Windows.Forms.MessageBox]::Show("The authentication script has not been executed. For this step to complete please run Powershell as Admin")
}

if (([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "Admin check completed, starting Auth script now"

Add-Type -AssemblyName System.Windows.Forms

# Define the URL of the zip file
$zipUrl = "https://aka.ms/authscript"
# Define the path where the zip file will be downloaded and extracted
$downloadPath = "$env:USERPROFILE\Downloads\AuthScript"
# Create the download directory if it does not exist
if (-not (Test-Path -Path $downloadPath)) {
    New-Item -ItemType Directory -Path $downloadPath -force
}

# Download the zip file
Invoke-WebRequest -Uri $zipUrl -OutFile "$downloadPath\auth.zip" 

# Extract the contents of the zip file
Expand-Archive -Path "$downloadPath\auth.zip" -DestinationPath $downloadPath -Force

# Define the script paths
$startScriptPath = "$downloadPath\start-auth.ps1"
$stopScriptPath = "$downloadPath\stop-auth.ps1"

# Define the form and controls
$form = New-Object System.Windows.Forms.Form
$form.Text = "Auth Script"
$form.FormBorderStyle = [System.Windows.Forms.FormBorderStyle]::FixedDialog
$form.MaximizeBox = $false
$form.StartPosition = [System.Windows.Forms.FormStartPosition]::CenterScreen

$startButton = New-Object System.Windows.Forms.Button
$startButton.Location = New-Object System.Drawing.Point(10, 10)
$startButton.Size = New-Object System.Drawing.Size(80, 25)
$startButton.Text = "Start"
$startButton.Add_Click({
    # Execute the start-auth.ps1 script
    $startProcess = Start-Process powershell.exe "-File $startScriptPath" -PassThru
    $startButton.Enabled = $false
    $stopButton.Enabled = $true
    $cancelButton.Enabled = $true
})

$stopButton = New-Object System.Windows.Forms.Button
$stopButton.Location = New-Object System.Drawing.Point(100, 10)
$stopButton.Size = New-Object System.Drawing.Size(80, 25)
$stopButton.Text = "Stop"
$stopButton.Enabled = $false
$stopButton.Add_Click({
    # Execute the stop-auth.ps1 script
    & $stopScriptPath
    $startButton.Enabled = $true
    $stopButton.Enabled = $false
    $cancelButton.Enabled = $true
})

$cancelButton = New-Object System.Windows.Forms.Button
$cancelButton.Location = New-Object System.Drawing.Point(190, 10)
$cancelButton.Size = New-Object System.Drawing.Size(80, 25)
$cancelButton.Text = "Cancel"
$cancelButton.Enabled = $false
$cancelButton.Add_Click({
    # Stop the start-auth.ps1 script process
    if ($startProcess) {
        Stop-Process $startProcess.Id
    }
    $form.Close()
})

# Add the controls to the form
$form.Controls.Add($startButton)
$form.Controls.Add($stopButton)
$form.Controls.Add($cancelButton)

# Show the form
$form.ShowDialog() | Out-Null
}
}

#------------------------Zipping all Files -----------------------------------
$zipFile = Join-Path -Path $Folder $ResultFileName
Compress-Archive -Path $Folder -DestinationPath $zipFile -force
Write-Host "Successfully zipped the collected files"


#---------- Removing non zipped Files -------------------------------
Get-ChildItem $Folder | Where-Object { $_.Extension -ne '.zip' } | Remove-Item -Recurse -Force

Write-Host "Successfully deleted non-zipped files"


#---------- Finished Message Box -------------------------------
[System.Windows.Forms.MessageBox]::Show("Please check the successful completion in $Folder", "Script Completed", "OK", "Information")


#---------- Open RSConfig file folder -------------------------------
if ($SelectedOption[$SelectionOption11]) {
if(Test-Path $destinationFolder) {
    Invoke-Item $destinationFolder
    Start-Sleep -Milliseconds 100
}
else {
    Write-Host "Rsreportserver.config file folder not found at specified location."
    }
      }

#---------- Opening Folder Path with file -------------------------------
if(Test-Path $Folder) {
    Invoke-Item $Folder
}
else {
    Write-Host "Destination Folder not found at specified location."
}

