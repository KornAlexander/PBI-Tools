Add-Type -AssemblyName System.Windows.Forms
 
$openFileDialog = New-Object System.Windows.Forms.OpenFileDialog
$openFileDialog.Filter = "Image files (*.jpg;*.jpeg;*.gif;*.bmp;*.png;*.svg)|*.jpg;*.jpeg;*.gif;*.bmp;*.png;*.svg"
$openFileDialog.Title = "Select an Image File"
 
if ($openFileDialog.ShowDialog() -eq [System.Windows.Forms.DialogResult]::OK) {
    $imagePath = $openFileDialog.FileName
    $imageBytes = [System.IO.File]::ReadAllBytes($imagePath)
    $base64 = [Convert]::ToBase64String($imageBytes)
 
    $txtFilePath = [System.IO.Path]::ChangeExtension($imagePath, ".txt")
    [System.IO.File]::WriteAllText($txtFilePath, $base64)
 
    Write-Output "Base64 string saved to file: $txtFilePath"
    # Open the text file
    Start-Process "notepad.exe" $txtFilePath
} else {
    Write-Output "No file selected."
}