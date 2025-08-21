// Tabular Editor Script: 6. Other\4. Backup All MacroAction.json Files to Desktop
// Valid Contexts: Model
// Tooltip: 
// Generated: 2025-08-21T16:54:13.141Z

#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;
using System.IO;
using System.Windows.Forms;

// Get the path to Local AppData and Desktop directories
string localAppDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);

// Get the current timestamp for the backup file name
string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

// Show a message box to ask the user for confirmation
DialogResult result = MessageBox.Show("Do you want to backup the MacroActions.json files?", "Backup Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

// If the user confirms, proceed with the backup
if (result == DialogResult.Yes)
{
    // Look for folders containing "TabularEditor" in LocalAppData
    string[] folders = Directory.GetDirectories(localAppDataPath, "*TabularEditor*", SearchOption.TopDirectoryOnly);

    int filesCopied = 0; // Counter to track how many files are copied

    foreach (string folder in folders)
    {
        // Check if MacroActions.json exists in the folder
        string macroActionsPath = Path.Combine(folder, "MacroActions.json");
        if (File.Exists(macroActionsPath))
        {
            // Create a new file name with folder name and timestamp
            string folderName = new DirectoryInfo(folder).Name;
            string newFileName = folderName + "_MacroActionsBackup_" + timestamp + ".json";
            string newFilePath = Path.Combine(desktopPath, newFileName);

            // Copy the file to the desktop with the new name
            File.Copy(macroActionsPath, newFilePath, true);

            // Count the number of successful copies
            filesCopied++;
        }
    }

    // Show a message box with the result of the operation
    if (filesCopied > 0)
    {
        Interaction.MsgBox(filesCopied+"MacroActions.json file(s) backed up successfully!", MsgBoxStyle.Information, "Backup Completed");
    }
    else
    {
        Interaction.MsgBox("No MacroActions.json files found to back up.", MsgBoxStyle.Exclamation, "Backup Completed");
    }
}
else
{
    Interaction.MsgBox("Backup canceled.", MsgBoxStyle.Information, "Operation Canceled");
}
