using LocalAiAssistant.Utilities;

namespace LocalAiAssistant;

public partial class FileManager : ContentPage
{
	public FileManager()
	{
		InitializeComponent();
        this.Loaded += FileManagerLoaded;
	}
    private string fileSelected = "";
    private async void FileManagerLoaded(object? sender, EventArgs e)
    {
        List<string> filesDetected = await MyAIAPI.GetFilesAsync(serverUrl: "http://192.168.0.100:4800/v1", apiKey: "", timeoutInSeconds: 60, authEnabled: false) ?? new();
        if (filesDetected.Count > 0)
        {
            FilesListView.ItemsSource = null;
            FilesListView.ItemsSource = filesDetected;
            FileViewer.IsVisible = true;
        }
        else
        {
            FileViewer.IsVisible = false;
        }
    } // Fix to correctly call the right server.
    private async void OnRemoveClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.CommandParameter is string CurFileUrl)
            {
                await MyAIAPI.DeleteFileAsync(fileUrl: CurFileUrl, apiKey: "", timeoutInSeconds: 60, authEnabled: false);
            }
        }
        catch (Exception ex)
        {
            await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Error", $"Failed to remove file: {ex.Message}");
        }
    } // Fix to correctly call the right server.
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
    } // Fix to correctly call the right server.
    private async void OnUploadBtnClicked(object sender, EventArgs e)
    {
        try
        {
            byte[] selectedFileBytes = File.ReadAllBytes(fileSelected);
            await MyAIAPI.UploadFileAsync(purpose: "fine-tune", fileBytes: selectedFileBytes, serverUrl: "http://192.168.0.100:4800/v1", apiKey: "", timeoutInSeconds: 60, authEnabled: false);
        }
        catch (Exception ex)
        {
            await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Error", $"Failed to upload file: {ex.Message}");
        }
    } // Fix to correctly call the right server.

}