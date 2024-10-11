#r "Microsoft.VisualBasic"
using Microsoft.VisualBasic;
using System.IO;
using System.Windows.Forms;


string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
string exeName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName); // Get the executable name without the extension
string URLdesktopBackupMacroActionsFile = "https://raw.githubusercontent.com/KornAlexander/PBI-Tools/refs/heads/main/Data%20Model%20Toolbox/DataModelToolbox.txt";
string URLdesktopMacroActionsFile = "https://raw.githubusercontent.com/KornAlexander/PBI-Tools/refs/heads/main/Data%20Model%20Toolbox/MacroActions.json";

System.Net.WebClient w = new System.Net.WebClient();
string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
string downloadLocBackup = Path.Combine(desktopPath, "HICODataModelToolbox.txt");
string downloadLocMacroActions = Path.Combine(desktopPath, "MacroActions.json");

// Download files
w.DownloadFile(URLdesktopBackupMacroActionsFile, downloadLocBackup);
w.DownloadFile(URLdesktopMacroActionsFile, downloadLocMacroActions);
string desktopMacroActionsFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "HICODataModelToolbox.txt");
string desktopBackupMacroActionsFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "MacroActions.json");
string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

// Determine if the executable is TabularEditor or TabularEditor3 and set the appropriate AppData folder
string backupDirectory = exeName.Contains("TabularEditor3")
    ? Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "TabularEditor3")
    : Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "TabularEditor");

// Create the backup folder if it does not exist
if (!Directory.Exists(backupDirectory))
{
    Directory.CreateDirectory(backupDirectory);
}

// Prompt the user to confirm the operation
if (MessageBox.Show("This script will backup your Macros and append the HICO-Group Macro toolbox to your collection. Macros will NOT be replaced. Both needed files (MacroActions.json and HICODataModelToolbox.txt) will be saved to your desktop. Do you would like to proceed?",
                    "Backup and Append Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
{
    if (!File.Exists(desktopMacroActionsFile))
    {
        MessageBox.Show("No HICODataModelToolbox.txt file found on the desktop!", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
    }

    string hicoContent = File.ReadAllText(desktopMacroActionsFile);
    string appDataMacroActionsPath = Path.Combine(backupDirectory, "MacroActions.json");
    string backupFilePath = Path.Combine(backupDirectory, "MacroActionsBackup_" + timestamp + ".json");

    try
    {
        bool backupCompleted = false, operationCompleted = false;

        // Backup and append operations
        if (File.Exists(appDataMacroActionsPath))
        {
            // Backup existing MacroActions.json
            File.Copy(appDataMacroActionsPath, backupFilePath, true);
            backupCompleted = true;

            // Append HICODataModelToolbox.txt to MacroActions.json
            string macroActionsContent = File.ReadAllText(appDataMacroActionsPath);
            string updatedMacroActionsContent = macroActionsContent.Insert(macroActionsContent.Length - 4, hicoContent + "\n");
            File.WriteAllText(appDataMacroActionsPath, updatedMacroActionsContent);
            operationCompleted = true;
        }
        else if (File.Exists(desktopBackupMacroActionsFile))
        {
            // If MacroActions.json is missing, copy from desktop
            File.Copy(desktopBackupMacroActionsFile, appDataMacroActionsPath, true);
            operationCompleted = true;

            MessageBox.Show("Adding the HICO-Group Data Model Toolbox was successful. (The MacroActions.json was copied from the desktop to appdata.) RESTART Tabular Editor now.", "Operation Summary", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        else
        {
            MessageBox.Show("No MacroActions.json found in folders or on the desktop.", "Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return;
        }

        // Display operation result
        string message = backupCompleted
            ? "Backup successful: " + backupFilePath + "\n"
            : "No existing MacroActions.json file was found for backup.\n";
        message += operationCompleted
        ? "Operation Successful: The HICO-Group Data Model Toolbox was added successfully to your existing Macros. (Macros in HICODataModelToolbox.txt appended to MacroActions.json.) RESTART Tabular Editor now."
            : "Operation failed.";

        MessageBox.Show(message, "Operation Summary", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    catch (Exception ex)
    {
        MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
else
{
    MessageBox.Show("Operation canceled.", "Operation Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
