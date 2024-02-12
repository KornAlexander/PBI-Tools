#Set Global Variables
$PortalURL = "http://reporturl/ReportServer";
$FormattedInventory = @();

#Variables for source and target folders
$Target_01= "C:\Users\targetfilename.csv";
$Source_01= '/Foldername'
 
$PBIReports = Get-RsFolderContent -ReportServerUri $PortalURL -RsFolder $ReportFolder -Recurse | Where-Object {$_.TypeName -eq "Folder"}
 
 
foreach($report in $PBIReports)
{
    #Friendly formatting
    $fullURL = $PortalURL + "/powerbi" + $report.Path;
    $created = $report.CreationDate.ToShortDateString() + " " + $report.CreationDate.ToShortTimeString();
    $modified = $report.ModifiedDate.ToShortDateString() + " " + $report.ModifiedDate.ToShortTimeString();
    $hidden = "N";
    $description = $null;
 
    $newPBIReport = New-Object –TypeName PSObject;
    $newPBIReport | Add-Member –MemberType NoteProperty –Name ReportName –Value $report.Name;
    $newPBIReport | Add-Member –MemberType NoteProperty –Name ReportDescription –Value $report.Description;
    $newPBIReport | Add-Member –MemberType NoteProperty –Name ModifiedBy –Value $report.ModifiedBy;
    $newPBIReport | Add-Member –MemberType NoteProperty –Name Modified –Value $modified;
 
    $FormattedInventory += $newPBIReport;
}
 
$FormattedInventory | Export-CSV -LiteralPath $Target_01 -NoTypeInformation
