<# 
SYNOPSIS:
Power BI Semantic Model Pimp Script

Objective:
This script has the objective to improve Power BI semantic models in Power BI project files with tmdl.

Important Disclaimer:
The script is not supported under any Microsoft standard support program or service. 
The script is provided AS IS without warranty of any kind. 

The entire risk arising out of the use or performance of the sample scripts and documentation 
remains with you. In no event shall the authors, or anyone else involved in the creation, production, or delivery of 
the scripts be liable for any damages whatsoever (including, without limitation, damages for loss of business profits, 
business interruption, loss of business information, or other pecuniary loss) arising out of the use of or inability to use the 
sample scripts or documentation.

#>

#-------- Change here the preselected checkmarks for topics ------------
$checkedItems = @(1, 2, 3, 4, 5, 6)


#-------- Disclaimer to Start Process ------------
Add-Type -AssemblyName Microsoft.VisualBasic
[void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$title = 'Info'
$message = 'Description: 
Please be informed that this script updates one of your Power BI Reports and adds content to it

Please select scope in subsequent step from topics such as:
- various tables to add

Disclaimer: Please have a backup of your pbix file. Make sure to have the TMDL file format for your pbip files enabled. 

Would you like to proceed?'
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
$SelectionOption1  = "Date Dimension Calculated Table"
$SelectionOption2  = "Date Dimension PQ Script"
$SelectionOption3  = "Last Refresh Table"
$SelectionOption4  = "Calculation Group - Time Intelligence"
$SelectionOption5  = "Calculation Group - Units"
$SelectionOption6  = "Empty Measure Table"
$SelectionOption7  = "-"
$SelectionOption8  = "-"
$SelectionOption9  = "-"
$SelectionOption10 = "-"
$SelectionOption11 = "-"
$SelectionOption12 = "-"
$SelectionOption13 = "-" 



#-------- PopUp to determine which data will be collected ------------ 
Add-Type -AssemblyName System.Windows.Forms
$title = 'Topic Selector'
$msg   = 
$options = @($SelectionOption1,$SelectionOption2, $SelectionOption3, $SelectionOption4, $SelectionOption5, $SelectionOption6<#, $SelectionOption7, $SelectionOption8, $SelectionOption9, $SelectionOption10, $SelectionOption11, $SelectionOption13 #> )
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

# Event handler for form resize
$form.add_Resize({
    $checkedListBox.Width = $form.ClientSize.Width - 40
    $checkedListBox.Height = $form.ClientSize.Height - 120
    $label.Width = $checkedListBox.Width
    $okButton.Left = ($form.ClientSize.Width / 2) - ($okButton.Width / 2)
    $okButton.Top = $form.ClientSize.Height - 70
})

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

Write-Host $SelectionOption1": $($SelectedOption[$SelectionOption1])"
Write-Host $SelectionOption2": $($SelectedOption[$SelectionOption2])"
Write-Host $SelectionOption3": $($SelectedOption[$SelectionOption3])"
Write-Host $SelectionOption4": $($SelectedOption[$SelectionOption4])"
Write-Host $SelectionOption5": $($SelectedOption[$SelectionOption5])"
Write-Host $SelectionOption6": $($SelectedOption[$SelectionOption6])"
Write-Host $SelectionOption7": $($SelectedOption[$SelectionOption7])"
Write-Host $SelectionOption8": $($SelectedOption[$SelectionOption8])"
Write-Host $SelectionOption9": $($SelectedOption[$SelectionOption9])"
Write-Host $SelectionOption10": $($SelectedOption[$SelectionOption10])"
Write-Host $SelectionOption11": $($SelectedOption[$SelectionOption11])"
Write-Host $SelectionOption12": $($SelectedOption[$SelectionOption12])"
Write-Host $SelectionOption13": $($SelectedOption[$SelectionOption13])"


#-------------Warn for discourageimplicitmeausre
if ($SelectedOption[$SelectionOption4] -or $SelectedOption[$SelectionOption5]) {

Add-Type -AssemblyName Microsoft.VisualBasic
[void][Reflection.Assembly]::LoadWithPartialName('Microsoft.VisualBasic')
$title = 'discourageImplicitMeasures is needed'
$message = 'Description: 
You selected to add at least one calculation group. For calculation groups it is necessary to set the property discourageimplicitmeasures to true. That means you will need to create explicit measures for your visualizations.

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
}

#------------------------------------
# Open a folder dialog to select a directory
<#
Add-Type -AssemblyName System.Windows.Forms
$folderBrowserDialog = New-Object System.Windows.Forms.FolderBrowserDialog
$folderBrowserDialog.Description = "Please select the folder in which the Power BI Project files and folders are saved"
$folderBrowserDialog.RootFolder = [Environment+SpecialFolder]::Desktop
$folderBrowserDialog.ShowNewFolderButton = $true
$dialogResult = $folderBrowserDialog.ShowDialog()

# Check if the user selected a folder and clicked OK
if ($dialogResult -eq [System.Windows.Forms.DialogResult]::OK)
{
    $selectedFolder = $folderBrowserDialog.SelectedPath
    # Store the full path including the additional segment in a variable
    $Folder = Join-Path -Path $selectedFolder -ChildPath "DemoBasis.Dataset\definition\tables\"
    
    # Output the path of the selected folder
    Write-Output "Selected folder: $selectedFolder"
    # Output the full path including the additional segment
    Write-Output "Full path with additional segment: $Folder"
}
else
{
    Write-Output "No folder was selected."
}

Add-Type -AssemblyName System.Windows.Forms
$openFileDialog = New-Object System.Windows.Forms.OpenFileDialog
$openFileDialog.InitialDirectory = [Environment]::GetFolderPath("Desktop")
$openFileDialog.Filter = "All files (*.*)|*.*"
$dialogResult = $openFileDialog.ShowDialog()
#>

Add-Type -AssemblyName System.Windows.Forms
$openFileDialog = New-Object System.Windows.Forms.OpenFileDialog
$openFileDialog.InitialDirectory = [Environment]::GetFolderPath("Desktop")
$openFileDialog.Filter = "All files (*.*)|*.*"
$dialogResult = $openFileDialog.ShowDialog()

# Check if the user selected a file and clicked OK
if ($dialogResult -eq [System.Windows.Forms.DialogResult]::OK)
{
    $selectedFile = $openFileDialog.FileName
    # Get the folder path from the selected file
    $Selectedfolder = [System.IO.Path]::GetDirectoryName($selectedFile)

    $selectedFileWithoutExtension = [System.IO.Path]::GetFileNameWithoutExtension($selectedFile)

    # Output the folder path and the full path of the file
    Write-Output "Folder: $Selectedfolder"
    Write-Output "Selected file: $selectedFile"
    Write-Output "Selected file without extension: $selectedFileWithoutExtension"
    # If you just want the file name, not the full path, use:
    # Write-Output "Selected file: $(Split-Path -Leaf $selectedFile)"
}
else
{
    Write-Output "No file was selected."
}


#-------- Variables for tables:
$NameSelectionOption1  = "Calendar CalcTable.tmdl"
$NameSelectionOption2  = "Calendar.tmdl"
$NameSelectionOption3  = "Last Refresh.tmdl"
$NameSelectionOption4  = "Time Intelligence.tmdl"
$NameSelectionOption5  = "Units.tmdl"
$NameSelectionOption6  = "Measure.tmdl"

$BaseURL = "https://raw.githubusercontent.com/KornAlexander/PBI-Tools/main/Table%20Template/"

$URLSelectionOption1  = $BaseURL + $NameSelectionOption1 -replace ' ', '%20'
$URLSelectionOption2  = $BaseURL + $NameSelectionOption2 -replace ' ', '%20'
$URLSelectionOption3  = $BaseURL + $NameSelectionOption3 -replace ' ', '%20'
$URLSelectionOption4  = $BaseURL + $NameSelectionOption4 -replace ' ', '%20'
$URLSelectionOption5  = $BaseURL + $NameSelectionOption5 -replace ' ', '%20'
$URLSelectionOption6  = $BaseURL + $NameSelectionOption6 -replace ' ', '%20'




#------------- create tables directory if it does not exist
# Define the path to the directory
$path = $selectedFolder +"\"+$selectedFileWithoutExtension + ".Dataset\definition\tables"

# Check if the directory exists
if (-Not (Test-Path -Path $path)) {
    # Directory does not exist, so create it
    New-Item -ItemType Directory -Path $path -Force
    Write-Output "Directory created: $path"
} else {
    # Directory already exists
    Write-Output "Directory already exists: $path"
}
#/

if ($SelectedOption[$SelectionOption4] -or $SelectedOption[$SelectionOption5]) {
#------------ Add discourageImplicitMeasures if it does not exist
# Define the path to your file
$ModelFilePath = $selectedFolder +"\"+$selectedFileWithoutExtension + ".Dataset\definition\model.tmdl"

# Read the content of the file
$content = Get-Content $ModelFilePath
$content1 = Get-Content $ModelFilePath -Raw

# Replace the specific block of text
$modifiedContent = $content -replace "model Model","model Model`n	discourageImplicitMeasures`n"

if (-not $content1.Contains("discourageImplicitMeasures")) {
# Write the modified content back to the file
$modifiedContent | Set-Content $ModelFilePath
}
}


#------------ Add defaultPowerBIDataSourceVersion if it does not exist
# Define the path to your file
$ModelFilePath = $selectedFolder +"\"+$selectedFileWithoutExtension + ".Dataset\definition\model.tmdl"

# Read the content of the file
$content = Get-Content $ModelFilePath
$content1 = Get-Content $ModelFilePath -Raw

# Replace the specific block of text
$modifiedContent = $content -replace "model Model","model Model`n	defaultPowerBIDataSourceVersion: powerBI_V3`n"

if (-not $content1.Contains("defaultPowerBIDataSourceVersion")) {
# Write the modified content back to the file
$modifiedContent | Set-Content $ModelFilePath
}


#-------- Starting the script
if ($SelectedOption[$SelectionOption1]) {
# Use Invoke-WebRequest to download the file
Invoke-WebRequest -Uri $URLSelectionOption1 -OutFile ($path + "\"+ $NameSelectionOption1)
}

#-------- Starting the script
if ($SelectedOption[$SelectionOption2]) {
# Use Invoke-WebRequest to download the file
Invoke-WebRequest -Uri $URLSelectionOption2 -OutFile ($path + "\"+ $NameSelectionOption2)
}

#-------- Starting the script
if ($SelectedOption[$SelectionOption3]) {
# Use Invoke-WebRequest to download the file
Invoke-WebRequest -Uri $URLSelectionOption3 -OutFile ($path + "\"+ $NameSelectionOption3)
}

#-------- Starting the script
if ($SelectedOption[$SelectionOption4]) {
# Use Invoke-WebRequest to download the file
Invoke-WebRequest -Uri $URLSelectionOption4 -OutFile ($path + "\"+ $NameSelectionOption4)
}

#-------- Starting the script
if ($SelectedOption[$SelectionOption5]) {
# Use Invoke-WebRequest to download the file
Invoke-WebRequest -Uri $URLSelectionOption5 -OutFile ($path + "\"+ $NameSelectionOption5)
}

#-------- Starting the script
if ($SelectedOption[$SelectionOption6]) {
# Use Invoke-WebRequest to download the file
Invoke-WebRequest -Uri $URLSelectionOption6 -OutFile ($path + "\"+ $NameSelectionOption6)
}



Write-Output "Script completed"
