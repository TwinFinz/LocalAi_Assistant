using LocalAiAssistant.Utilities;
using System.Windows.Input;

namespace LocalAiAssistant
{
    public partial class FileManager : ContentPage
    {
        private FileManagerData UiData = new();
        private GeneralSettingsData defaultData = new();
        public FileManager()
        {
            InitializeComponent();
            BindingContext = UiData;
            this.Loaded += FileManagerLoaded;
        }
        private string fileSelected = "";
        private async void FileManagerLoaded(object? sender, EventArgs e)
        {
            if (MyMultiPlatformUtils.CheckPreferenceContains(GeneralSettings.MainPreference))
            {
                GeneralSettingsData? saveData = await MyMultiPlatformUtils.ReadFromPreferences<GeneralSettingsData>(GeneralSettings.MainPreference);
                if (saveData != null)
                {
                    defaultData.DefaultServerUrl = saveData.DefaultServerUrl;
                    defaultData.AuthEnabled = saveData.AuthEnabled;
                    defaultData.DefaultApiKey = saveData.DefaultApiKey;
                    defaultData.EncryptEnabled = saveData.EncryptEnabled;
                    defaultData.EncryptKey = saveData.EncryptKey;
                    defaultData.DefaultTimeOutDelay = saveData.DefaultTimeOutDelay;
                }
            }
            await UpdateFiles();
        }
        private async Task UpdateFiles()
        {
            List<MyAIAPI.FileInformation> filesDetected = await MyAIAPI.GetFilesAsync(serverUrl: defaultData.DefaultServerUrl, apiKey: defaultData.DefaultApiKey, timeoutInSeconds: (int)defaultData.DefaultTimeOutDelay, authEnabled: defaultData.AuthEnabled) ?? new();
            if (filesDetected.Count > 0)
            {
                List<string> files = new List<string>();
                foreach (MyAIAPI.FileInformation curFile in filesDetected)
                {
                    files.Add($"Name: {curFile.Filename} | Id: {curFile.Id} | Size: {curFile.Bytes} bytes | Purpose: {curFile.Purpose} | CreatedAt: {curFile.CreatedAt}");
                }
                FilesListView.ItemsSource = null;
                FilesListView.ItemsSource = files;
                FileViewer.IsVisible = true;
            }
            else
            {
                FileViewer.IsVisible = false;
            }
        }
        private async void OnRemoveClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is string curFileString)
                {
                    string? curFileId = curFileString.Split('|').FirstOrDefault(part => part.Contains("Id"))?.Split(':').LastOrDefault()?.Trim();
                    if (curFileId != null)
                    {
                        await MyAIAPI.DeleteFileAsync(fileUrl: $"{defaultData.DefaultServerUrl}/files/{curFileId}", apiKey: defaultData.DefaultApiKey, timeoutInSeconds: (int)defaultData.DefaultTimeOutDelay, authEnabled: defaultData.AuthEnabled);
                        await UpdateFiles();
                    }
                    else
                    {
                        throw new Exception("Failed to extract file ID from the provided string.");
                    }
                }
            }
            catch (Exception ex)
            {
                await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Error", $"Failed to remove file: {ex.Message}");
            }
        }
        private async void OnSelectBtnClicked(object sender, EventArgs e)
        {
            try
            {
                fileSelected = await MyMultiPlatformUtils.PickFileAsync() ?? "";
            }
            catch (Exception ex)
            {
                await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Error", $"Failed to select file: {ex.Message}");
            }
        }
        private async void OnUploadBtnClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileSelected))
                {
                    throw new Exception("No file selected.");
                }
                byte[] selectedFileBytes = File.ReadAllBytes(fileSelected);
                string selectedFileName = Path.GetFileName(fileSelected);
                await MyAIAPI.UploadFileAsync(purpose: "fine-tune", fileBytes: selectedFileBytes, fileName: selectedFileName, serverUrl: defaultData.DefaultServerUrl, apiKey: defaultData.DefaultApiKey, timeoutInSeconds: (int)defaultData.DefaultTimeOutDelay, authEnabled: true); // defaultData.AuthEnabled);

                await UpdateFiles();
            }
            catch (Exception ex)
            {
                await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Error", $"Failed to upload file: {ex.Message}");
            }
        }
    }
    public class FileManagerData : BindableObject
    {
        public static readonly BindableProperty FilesProperty = BindableProperty.Create(nameof(Files), typeof(List<string>), typeof(TextGenerationSettingsData), new List<string>());
        public List<string> Files { get => (List<string>)GetValue(FilesProperty); set => SetValue(FilesProperty, value); }
    }
}