using CommunityToolkit.Maui.Views;
using LocalAiAssistant.Utilities;

namespace LocalAiAssistant;

public partial class TranscriptionGeneration : ContentPage
{
	public TranscriptionGeneration()
	{
		InitializeComponent();
        BindingContext = UiData;
        this.Loaded += Txt2SpeechLoaded;
        SizeChanged += OnPageSizeChanged;
    }
    #region Init/Events
    public AudioGenerationSettingsData UiData = new();
    private GeneralSettingsData defaultData = new();
    private byte[]? curAudioBytes;
    private async void Txt2SpeechLoaded(object? sender, EventArgs e)
    {
        await LoadData();
    }
    private async Task LoadData()
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
            }
        }
        if (MyMultiPlatformUtils.CheckPreferenceContains(AudioGenerationSettings.ModelListPreference))
        {
            List<string> saveData = await MyMultiPlatformUtils.ReadFromPreferences<List<string>>(AudioGenerationSettings.ModelListPreference) ?? new();
            if (saveData.Count > 0)
            {
                UiData.ModelList = saveData;
            }
        }
        if (MyMultiPlatformUtils.CheckPreferenceContains(AudioGenerationSettings.SecondaryModelListPreference))
        {
            List<string> saveData = await MyMultiPlatformUtils.ReadFromPreferences<List<string>>(AudioGenerationSettings.SecondaryModelListPreference) ?? new();
            if (saveData.Count > 0)
            {
                UiData.SecondaryModelList = saveData;
            }
        }
        if (MyMultiPlatformUtils.CheckPreferenceContains(AudioGenerationSettings.TtsPreference))
        {
            AudioGenerationSettingsData? saveData = await MyMultiPlatformUtils.ReadFromPreferences<AudioGenerationSettingsData>(AudioGenerationSettings.TtsPreference);
            if (saveData != null)
            {
                UiData.CustomServerEnabled = saveData.CustomServerEnabled;
                if (UiData.CustomServerEnabled)
                {
                    UiData.ServerUrlInput = saveData.ServerUrlInput;
                    UiData.AuthEnabled = saveData.AuthEnabled;
                    UiData.ApiKey = saveData.ApiKey;
                }
                else
                {
                    UiData.ServerUrlInput = defaultData.DefaultServerUrl;
                    UiData.AuthEnabled = defaultData.AuthEnabled;
                    UiData.ApiKey = defaultData.DefaultApiKey;
                }
                UiData.TimeOutDelay = saveData.TimeOutDelay;
                UiData.TTSEnabled = saveData.TTSEnabled;
                UiData.Speed = saveData.Speed;
                UiData.SelectedModel = saveData.SelectedModel;
                UiData.SelectedModelIndex = UiData.ModelList.IndexOf(saveData.SelectedModel);
                if (UiData.ModelList.Count > 1 && UiData.SelectedModelIndex > 0 && UiData.SelectedModelIndex < UiData.ModelList.Count)
                {
                    UiData.SelectedModel = UiData.ModelList[UiData.SelectedModelIndex];
                }
                UiData.SelectedModel2 = saveData.SelectedModel2;
                UiData.SelectedModelIndex2 = UiData.SecondaryModelList.IndexOf(saveData.SelectedModel2);
                if (UiData.SecondaryModelList.Count > 0 && UiData.SelectedModelIndex2 >= 0 && UiData.SelectedModelIndex2 < UiData.SecondaryModelList.Count)
                {
                    UiData.SelectedModel2 = UiData.SecondaryModelList[UiData.SelectedModelIndex2];
                }
                UiData.SelectedTranscriptionModel = saveData.SelectedTranscriptionModel;
                UiData.SelectedTranscriptionModelIndex = UiData.ModelList.IndexOf(UiData.SelectedTranscriptionModel);
                if (UiData.ModelList.Count > 1 && UiData.SelectedTranscriptionModelIndex > 0 && UiData.SelectedTranscriptionModelIndex < UiData.ModelList.Count)
                {
                    UiData.SelectedModel = UiData.ModelList[UiData.SelectedTranscriptionModelIndex];
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
        else
        {
            UiData.ServerUrlInput = defaultData.DefaultServerUrl;
            UiData.AuthEnabled = defaultData.AuthEnabled;
            UiData.ApiKey = defaultData.DefaultApiKey;
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
        try
        {
            if (UiData.ModelList.Count <= 0)
            {
                await LoadData();
            }
            if (string.IsNullOrWhiteSpace(textToGenerate))
            {
                textToGenerate = UiData.Prompt;
            }
            if (string.IsNullOrWhiteSpace(textToGenerate))
            {
                await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Warning!", "To generate TTS you must provide a prompt.");
                return;
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
                        // LocalAI Audio2Text
                        audioBytes = await MyAIAPI.GenerateLocalAiTTS(prompt: textToGenerate, apiKey: UiData.ApiKey, model: UiData.SelectedModel, timeoutInSeconds: (int)UiData.TimeOutDelay, serverUrl: UiData.ServerUrlInput, authEnabled: UiData.AuthEnabled, cancellationToken: cancellationToken);
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

            return;
        }
        catch (Exception ex)
        {
            await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Error", $"Failed to generate Audio: {ex.Message}");
            return;
        }
    }
    #endregion
}