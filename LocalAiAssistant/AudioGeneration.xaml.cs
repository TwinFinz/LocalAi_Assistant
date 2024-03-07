using CommunityToolkit.Maui.Views;
using LocalAiAssistant.Utilities;

namespace LocalAiAssistant;

public partial class AudioGeneration : ContentPage
{
	public AudioGeneration()
	{
		InitializeComponent();
        BindingContext = UiData;
        //this.Loaded += Txt2SpeechLoaded;
        SizeChanged += OnPageSizeChanged;
        Task.Run(async () => await LoadData());
    }
    #region Init/Events
    public AudioGenerationSettingsData UiData = new();
    private byte[]? curAudioBytes;
    private async void Txt2SpeechLoaded(object? sender, EventArgs e)
    {
        await LoadData();
    }
    private async Task LoadData()
    {
        if (MyMultiPlatformUtils.CheckPreferenceContains(AudioGenerationSettings.TtsPreference))
        {
            AudioGenerationSettingsData? saveData = await MyMultiPlatformUtils.ReadFromPreferences<AudioGenerationSettingsData>(AudioGenerationSettings.TtsPreference);
            if (saveData != null)
            {
                UiData.ApiKey = saveData.ApiKey;
                UiData.ServerUrlInput = saveData.ServerUrlInput;
                UiData.TimeOutDelay = saveData.TimeOutDelay;
                UiData.AuthEnabled = saveData.AuthEnabled;
                UiData.TTSEnabled = saveData.TTSEnabled;
                UiData.Speed = saveData.Speed;
                UiData.SelectedModel = saveData.SelectedModel;
                UiData.ModelList = saveData.ModelList;
                UiData.SelectedModelIndex = saveData.SelectedModelIndex;
                if (UiData.ModelList.Count > 1 && UiData.SelectedModelIndex > 0 && UiData.SelectedModelIndex < UiData.ModelList.Count)
                {
                    UiData.SelectedModel = saveData.ModelList[saveData.SelectedModelIndex];
                }
                UiData.ModelList2 = saveData.ModelList2;
                UiData.SelectedModelIndex2 = saveData.SelectedModelIndex2;
                if (UiData.ModelList2.Count > 0 && UiData.SelectedModelIndex2 >= 0 && UiData.SelectedModelIndex2 < UiData.ModelList2.Count)
                {
                    UiData.SelectedModel2 = saveData.ModelList2[saveData.SelectedModelIndex2];
                }
                if (UiData.ServerModeNames.Count > 1 && saveData.SelectedServerModeIndex < UiData.ServerModeNames.Count)
                {
                    if (UiData.ServerModeNames.Count > 1 && saveData.SelectedServerModeIndex < UiData.ServerModeNames.Count)
                    {
                        UiData.ServerMode = (AudioGenerationSettingsData.ServerModes)saveData.SelectedServerModeIndex;
                        UiData.SelectedServerModeIndex = saveData.SelectedServerModeIndex;
                    }
                    if (UiData.ServerMode == AudioGenerationSettingsData.ServerModes.OpenAi)
                    {
                        OpenAiOptionsView.IsVisible = true;
                    }
                }
                else
                {
                    UiData.ServerMode = AudioGenerationSettingsData.ServerModes.OpenAi;
                }
            }
        }
    }
    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (Width > Height)
        {
            Grid.SetRow(SettingsPanel, 0);
            Grid.SetColumn(SettingsPanel, 1);
        }
        else
        {
            Grid.SetRow(SettingsPanel, 1);
            Grid.SetColumn(SettingsPanel, 0);
        }
    }

    private void SpeedSliderChanged(object sender, ValueChangedEventArgs e)
    {
        double roundedValue = (double)Math.Round(e.NewValue * 20) / 20;
        UiData.Speed = roundedValue;
    }
    #endregion
    #region Button Events/Animations
    private async void OnSaveAudioBtnClicked(object? sender, EventArgs e)
    {
        SemanticScreenReader.Announce(SaveImageBtn.Text);
        if (UiData.PlayerMediaSource != null)
        {
            byte[] audioBytes = await MyMultiPlatformUtils.ConvertMediaSourceToByteArray(UiData.PlayerMediaSource);
            string folderPath = await MyMultiPlatformUtils.PickFolderAsync();
            if (MyMultiPlatformUtils.IsFolderWritable(folderPath))
            {
                string baseFilePath = $"{folderPath}/output.mp3";
                string filePath = baseFilePath;
                if (File.Exists(filePath))
                {
                    int index = 1;
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(baseFilePath);
                    string fileExtension = Path.GetExtension(baseFilePath);
                    do
                    {
                        filePath = Path.Combine(folderPath, $"{fileNameWithoutExtension}-{index}{fileExtension}");
                        index++;
                    } while (File.Exists(filePath));
                }
                await MyMultiPlatformUtils.WriteToFileAsync(filePath, audioBytes);
                await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Save File", "Done!");
            }
        }
    }
    private async void OnGenerateBtnClicked(object? sender, EventArgs e)
    {
        SemanticScreenReader.Announce(GenerateBtn.Text);
        await Generate();
    }
    private async void OnSelectBtnClicked(object? sender, EventArgs e)
    {
        SemanticScreenReader.Announce(GenerateBtn.Text);
        try
        {
            //UiData.InputImage = await MyMultiPlatformUtils.PickImageAsync() ?? throw new Exception("Image failed to load, returned null.");
            //await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Select Mask", $"If you want to use a mask select it now.");
            //UiData.MaskImage = await MyMultiPlatformUtils.PickImageAsync() ?? MyMultiPlatformUtils.CreateBlankPngAsync(UiData.ImageWidth, UiData.ImageHeight);
        }
        catch (Exception ex)
        {
            await MyMultiPlatformUtils.MessageBoxWithOK(Application.Current!, "Error", $"Failed to generate image: {ex.Message}");
        }
    }
    public async Task Generate(string textToGenerate = "", CancellationToken cancellationToken = default)
    {
        curAudioBytes = await GenerateTtsAudio(textToGenerate, cancellationToken);
        if (curAudioBytes != null)
        {
            string filePath = Path.Combine(FileSystem.CacheDirectory, "temp_audio.mp3");
            await File.WriteAllBytesAsync(filePath, curAudioBytes, cancellationToken); // Asynchronous file write
            MediaSource source = MediaSource.FromFile(filePath);
            MediaPlayer.Source = source;
            MediaOutput.IsVisible = false;
            MediaPlayer.ShouldAutoPlay = true;
        }
    }
    public async Task<byte[]?> GenerateTtsAudio(string textToGenerate = "", CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(textToGenerate))
            {
                textToGenerate = UiData.Prompt;
            }
            if (string.IsNullOrWhiteSpace(textToGenerate))
            {
                await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Warning!", "To generate TTS you must provide a prompt.");
                return null;
            }
            byte[] audioBytes = Array.Empty<byte>();
            switch (UiData.ServerMode)
            {
                case AudioGenerationSettingsData.ServerModes.System:
                    {
                        await MyAudioUtils.SpeakText(textToGenerate, cancellationToken);
                        curAudioBytes = Array.Empty<byte>();
                        break;
                    }
                case AudioGenerationSettingsData.ServerModes.LocalAi:
                    {
                        // LocalAI Text2Audio
                        if (UiData.SelectedModel == "Bark")
                        {
                            audioBytes = await MyAIAPI.GenerateLocalAiTTS(prompt: textToGenerate, apiKey: UiData.ApiKey, backend: "bark", model: UiData.SelectedModel2, timeoutInSeconds: (int)UiData.TimeOutDelay, serverUrl: UiData.ServerUrlInput, authEnabled: UiData.AuthEnabled, cancellationToken: cancellationToken);
                        }
                        else if (UiData.SelectedModel == "CoquiTTS")
                        {
                            audioBytes = await MyAIAPI.GenerateLocalAiTTS(prompt: textToGenerate, apiKey: UiData.ApiKey, backend: "coqui", model: UiData.SelectedModel2, timeoutInSeconds: (int)UiData.TimeOutDelay, serverUrl: UiData.ServerUrlInput, authEnabled: UiData.AuthEnabled, cancellationToken: cancellationToken);
                        }
                        else
                        {
                            audioBytes = await MyAIAPI.GenerateLocalAiTTS(prompt: textToGenerate, apiKey: UiData.ApiKey, model: UiData.SelectedModel, timeoutInSeconds: (int)UiData.TimeOutDelay, serverUrl: UiData.ServerUrlInput, authEnabled: UiData.AuthEnabled, cancellationToken: cancellationToken);
                        }
                        if (audioBytes != Array.Empty<byte>() && audioBytes != null)
                        {
                            curAudioBytes = audioBytes;
                        }
                        break;
                    }
                case AudioGenerationSettingsData.ServerModes.OpenAi:
                    {
                        audioBytes = await MyAIAPI.GenerateSpeech(prompt: textToGenerate, apiKey: UiData.ApiKey, model: UiData.SelectedModel, voice: UiData.OpenAiVoiceNames[UiData.SelectedVoiceIndex].ToLower(), speed: UiData.Speed, timeoutInSeconds: (int)UiData.TimeOutDelay, serverUrl: UiData.ServerUrlInput, authEnabled: UiData.AuthEnabled);
                        if (audioBytes != Array.Empty<byte>() && audioBytes != null)
                        {
                            curAudioBytes = audioBytes;
                        }
                        break;
                    }
            } // Process Appropriately

            return curAudioBytes;
        }
        catch (Exception ex)
        {
            await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Error", $"Failed to generate Audio: {ex.Message}");
            return null;
        }
    }
    #endregion
}