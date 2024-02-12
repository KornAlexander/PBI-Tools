$removepath = 'C:\Users\Username\Targetfolder' 

#clean up the folder
Get-ChildItem -Path $removepath -File -Recurse | Remove-Item -Verbose

#General Variables
$PortalURL = 'http://reportserverurl/ReportServer'

#Variables for source and target folders
$Target_01= 'C:\Users\Targetfolder' 
$Source_01= '/Foldername -->UseHTMLEncodedFolderName' 

#download items
Out-RsRestFolderContent -ReportPortalUri $PortalURL -RsFolder $Source_01 -Recurse -Destination $Target_01
