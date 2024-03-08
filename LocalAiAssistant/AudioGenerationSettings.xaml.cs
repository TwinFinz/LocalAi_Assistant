using CommunityToolkit.Maui.Views;
using LocalAiAssistant.Utilities;

namespace LocalAiAssistant
{

    public partial class AudioGenerationSettings : ContentPage
    {
        public AudioGenerationSettingsData UiData { get; set; } = new();
        internal static string TtsPreference = "LocalAiAssistant-TTS";
        private GeneralSettingsData defaultData = new();
        public AudioGenerationSettings()
        {
            InitializeComponent();
            BindingContext = UiData;
            OnLoadBtnClicked(this, new EventArgs());
            ApiKeyInput.Unfocused += ApiKeyInput_Unfocused;
            CustomServerSwitch.Toggled += CustomServerToggled;
        }
        private List<string> BarkModels = new List<string>()
        {
            "v2/en_speaker_0",
            "v2/en_speaker_1",
            "v2/en_speaker_2",
            "v2/en_speaker_3",
            "v2/en_speaker_4",
            "v2/en_speaker_5",
            "v2/en_speaker_6",
            "v2/en_speaker_7",
            "v2/en_speaker_8",
            "v2/en_speaker_9",
            "v2/zh_speaker_0",
            "v2/zh_speaker_1",
            "v2/zh_speaker_2",
            "v2/zh_speaker_3",
            "v2/zh_speaker_4",
            "v2/zh_speaker_5",
            "v2/zh_speaker_6",
            "v2/zh_speaker_7",
            "v2/zh_speaker_8",
            "v2/zh_speaker_9",
            "v2/fr_speaker_0",
            "v2/fr_speaker_1",
            "v2/fr_speaker_2",
            "v2/fr_speaker_3",
            "v2/fr_speaker_4",
            "v2/fr_speaker_5",
            "v2/fr_speaker_6",
            "v2/fr_speaker_7",
            "v2/fr_speaker_8",
            "v2/fr_speaker_9",
            "v2/de_speaker_0",
            "v2/de_speaker_1",
            "v2/de_speaker_2",
            "v2/de_speaker_3",
            "v2/de_speaker_4",
            "v2/de_speaker_5",
            "v2/de_speaker_6",
            "v2/de_speaker_7",
            "v2/de_speaker_8",
            "v2/de_speaker_9",
            "v2/hi_speaker_0",
            "v2/hi_speaker_1",
            "v2/hi_speaker_2",
            "v2/hi_speaker_3",
            "v2/hi_speaker_4",
            "v2/hi_speaker_5",
            "v2/hi_speaker_6",
            "v2/hi_speaker_7",
            "v2/hi_speaker_8",
            "v2/hi_speaker_9",
            "v2/it_speaker_0",
            "v2/it_speaker_1",
            "v2/it_speaker_2",
            "v2/it_speaker_3",
            "v2/it_speaker_4",
            "v2/it_speaker_5",
            "v2/it_speaker_6",
            "v2/it_speaker_7",
            "v2/it_speaker_8",
            "v2/it_speaker_9",
            "v2/ja_speaker_0",
            "v2/ja_speaker_1",
            "v2/ja_speaker_2",
            "v2/ja_speaker_3",
            "v2/ja_speaker_4",
            "v2/ja_speaker_5",
            "v2/ja_speaker_6",
            "v2/ja_speaker_7",
            "v2/ja_speaker_8",
            "v2/ja_speaker_9",
            "v2/ko_speaker_0",
            "v2/ko_speaker_1",
            "v2/ko_speaker_2",
            "v2/ko_speaker_3",
            "v2/ko_speaker_4",
            "v2/ko_speaker_5",
            "v2/ko_speaker_6",
            "v2/ko_speaker_7",
            "v2/ko_speaker_8",
            "v2/ko_speaker_9",
            "v2/pl_speaker_0",
            "v2/pl_speaker_1",
            "v2/pl_speaker_2",
            "v2/pl_speaker_3",
            "v2/pl_speaker_4",
            "v2/pl_speaker_5",
            "v2/pl_speaker_6",
            "v2/pl_speaker_7",
            "v2/pl_speaker_8",
            "v2/pl_speaker_9",
            "v2/pt_speaker_0",
            "v2/pt_speaker_1",
            "v2/pt_speaker_2",
            "v2/pt_speaker_3",
            "v2/pt_speaker_4",
            "v2/pt_speaker_5",
            "v2/pt_speaker_6",
            "v2/pt_speaker_7",
            "v2/pt_speaker_8",
            "v2/pt_speaker_9",
            "v2/ru_speaker_0",
            "v2/ru_speaker_1",
            "v2/ru_speaker_2",
            "v2/ru_speaker_3",
            "v2/ru_speaker_4",
            "v2/ru_speaker_5",
            "v2/ru_speaker_6",
            "v2/ru_speaker_7",
            "v2/ru_speaker_8",
            "v2/ru_speaker_9",
            "v2/es_speaker_0",
            "v2/es_speaker_1",
            "v2/es_speaker_2",
            "v2/es_speaker_3",
            "v2/es_speaker_4",
            "v2/es_speaker_5",
            "v2/es_speaker_6",
            "v2/es_speaker_7",
            "v2/es_speaker_8",
            "v2/es_speaker_9",
            "v2/tr_speaker_0",
            "v2/tr_speaker_1",
            "v2/tr_speaker_2",
            "v2/tr_speaker_3",
            "v2/tr_speaker_4",
            "v2/tr_speaker_5",
            "v2/tr_speaker_6",
            "v2/tr_speaker_7",
            "v2/tr_speaker_8",
            "v2/tr_speaker_9"
        };
        private List<string> CoquiModels = new List<string>()
        {
            "tts_models/en/vctk/vits",
            "tts_models/en/ljspeech/glow-tts"
        };
        private List<string> PiperModels = new List<string>()
        {
            "",
            ""
        };
        private void OnModelSelected(object? sender, EventArgs e)
        {
            if (UiData.ModelList.Count > 0 && UiData.SelectedModelIndex >= 0 && UiData.SelectedModelIndex < UiData.ModelList.Count)
            {
                if (UiData.ModelList[UiData.SelectedModelIndex] == "Bark")
                {
                    UiData.ModelList2 = BarkModels;
                    SecondaryModelPicker.IsVisible = true;
                }
                else if (UiData.ModelList[UiData.SelectedModelIndex] == "CoquiTTS")
                {
                    UiData.ModelList2 = CoquiModels;
                    SecondaryModelPicker.IsVisible = true;
                }
                else if (UiData.ModelList[UiData.SelectedModelIndex] == "Piper")
                {
                    UiData.ModelList2 = PiperModels;
                    SecondaryModelPicker.IsVisible = true;
                }
                else
                {
                    SecondaryModelPicker.IsVisible = false;
                }
            }
        }
        private void TimeOutDelaySliderChanged(object sender, ValueChangedEventArgs e)
        {
            double roundedValue = Math.Round(e.NewValue);
            UiData.TimeOutDelay = roundedValue;
        }
        private void SpeedSliderChanged(object sender, ValueChangedEventArgs e)
        {
            double roundedValue = (double)Math.Round(e.NewValue * 20) / 20;
            UiData.Speed = roundedValue;
        }
        private void ApiKeyInput_Unfocused(object? sender, FocusEventArgs e)
        {
            if (ApiKeyInput.Text.Length > 5)
            {
                UiData.AuthEnabled = true;
            }
            else
            {
                UiData.AuthEnabled = false;
            }
        }
        private async void OnReloadListBtnClicked(object? sender, EventArgs e)
        {
            await UpdateModelsPicker();
        }
        private async Task UpdateModelsPicker()
        {
            if (UiData.ModelList.Count < 1 || DateTime.Now - UiData.TimeModelListUpdated > TimeSpan.FromDays(3) || UiData.ModelListUpdatedUsingUrl != UiData.ServerUrlInput)
            {
                try
                {
                    List<string> curList = await MyAIAPI.GetLocalAiModelNameList(apiKey: UiData.ApiKey, timeoutInSeconds: 3, serverUrl: UiData.ServerUrlInput, authEnabled: UiData.AuthEnabled);
                    if (UiData.ServerMode == AudioGenerationSettingsData.ServerModes.LocalAi)
                    {
                        curList.Add("Bark");
                        curList.Add("CoquiTTS");
                    }
                    curList.Sort();
                    UiData.ModelList = curList;
                    UiData.ModelListUpdatedUsingUrl = UiData.ServerUrlInput;
                    UiData.TimeModelListUpdated = DateTime.Now;
                }
                catch (Exception ex)
                {
                    await MyMultiPlatformUtils.WriteToLog($"Retrieving model list failed,\nPlease make sure the Server Url is set in settings.\n\nLocalAi: {ex}");
                }
            }
        }
        private void CustomServerToggled(object? sender, ToggledEventArgs e)
        {
            if (e.Value)
            {
                ServerInfoInput.IsVisible = true;
            }
            else
            {
                ServerInfoInput.IsVisible = false;
            }
        }
        private async void OnSaveBtnClicked(object? sender, EventArgs e)
    {
        SemanticScreenReader.Announce(SaveBtn.Text);
        if (UiData != null)
        {
            await MyMultiPlatformUtils.WriteToPreferences(AudioGenerationSettings.TtsPreference, UiData);
            await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Save Settings", "Success");
        }
    }
        private async void OnLoadBtnClicked(object? sender, EventArgs e)
        {
            SemanticScreenReader.Announce(LoadBtn.Text);
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
            else
            {
                UiData.ServerUrlInput = defaultData.DefaultServerUrl;
                UiData.AuthEnabled = defaultData.AuthEnabled;
                UiData.ApiKey = defaultData.DefaultApiKey;
            }
        }

    }
    public class AudioGenerationSettingsData : BindableObject
    {
        public enum ServerModes
        {
            System,
            OpenAi,
            LocalAi
        }
        public enum OpenAiVoices
        {
            Alloy,
            Echo,
            Fable,
            Onyx,
            Nova,
            Shimmer
        }

        public enum AudioFormats
        {
            Mp3,
            Opus,
            Aac,
            Flac
        }
        public static readonly BindableProperty ServerModeNamesProperty = BindableProperty.Create(nameof(ServerModeNames), typeof(List<string>), typeof(AudioGenerationSettingsData), Enum.GetNames(typeof(ServerModes)).ToList());
        public List<string> ServerModeNames { get => (List<string>)GetValue(ServerModeNamesProperty); set => SetValue(ServerModeNamesProperty, value); }

        public static readonly BindableProperty AudioFormatNamesProperty = BindableProperty.Create(nameof(AudioFormatNames), typeof(List<string>), typeof(AudioGenerationSettingsData), Enum.GetNames(typeof(AudioFormats)).ToList());
        public List<string> AudioFormatNames { get => (List<string>)GetValue(AudioFormatNamesProperty); set => SetValue(AudioFormatNamesProperty, value); }

        public static readonly BindableProperty OpenAiVoiceNamesProperty = BindableProperty.Create(nameof(OpenAiVoiceNames), typeof(List<string>), typeof(AudioGenerationSettingsData), Enum.GetNames(typeof(OpenAiVoices)).ToList());
        public List<string> OpenAiVoiceNames { get => (List<string>)GetValue(OpenAiVoiceNamesProperty); set => SetValue(OpenAiVoiceNamesProperty, value); }

        public static readonly BindableProperty SelectedVoiceProperty = BindableProperty.Create(nameof(SelectedVoice), typeof(OpenAiVoices), typeof(AudioGenerationSettingsData), OpenAiVoices.Onyx);
        public OpenAiVoices SelectedVoice { get => (OpenAiVoices)GetValue(SelectedVoiceProperty); set => SetValue(SelectedVoiceProperty, value); }

        public static readonly BindableProperty SelectedFormatProperty = BindableProperty.Create(nameof(SelectedFormat), typeof(AudioFormats), typeof(AudioGenerationSettingsData), AudioFormats.Mp3);
        public AudioFormats SelectedFormat { get => (AudioFormats)GetValue(SelectedFormatProperty); set => SetValue(SelectedFormatProperty, value); }

        public static readonly BindableProperty SelectedServerModeIndexProperty = BindableProperty.Create(nameof(SelectedServerModeIndex), typeof(int), typeof(AudioGenerationSettingsData), 0);
        public int SelectedServerModeIndex { get => (int)GetValue(SelectedServerModeIndexProperty); set => SetValue(SelectedServerModeIndexProperty, value); }

        public static readonly BindableProperty SelectedVoiceIndexProperty = BindableProperty.Create(nameof(SelectedVoiceIndex), typeof(int), typeof(AudioGenerationSettingsData), 0);
        public int SelectedVoiceIndex { get => (int)GetValue(SelectedVoiceIndexProperty); set => SetValue(SelectedVoiceIndexProperty, value); }

        public static readonly BindableProperty SelectedAudioFormatIndexProperty = BindableProperty.Create(nameof(SelectedAudioFormatIndex), typeof(int), typeof(AudioGenerationSettingsData), int.MinValue);
        public int SelectedAudioFormatIndex { get => (int)GetValue(SelectedAudioFormatIndexProperty); set => SetValue(SelectedAudioFormatIndexProperty, value); }

        public static readonly BindableProperty ServerModeProperty = BindableProperty.Create(nameof(ServerMode), typeof(ServerModes), typeof(AudioGenerationSettingsData), ServerModes.OpenAi);
        public ServerModes ServerMode { get => (ServerModes)GetValue(ServerModeProperty); set => SetValue(ServerModeProperty, value); }

        public static readonly BindableProperty CustomServerEnabledProperty = BindableProperty.Create(nameof(CustomServerEnabled), typeof(bool), typeof(AudioGenerationSettingsData), false);
        public bool CustomServerEnabled { get => (bool)GetValue(CustomServerEnabledProperty); set => SetValue(CustomServerEnabledProperty, value); }

        public static readonly BindableProperty ServerUrlProperty = BindableProperty.Create(nameof(ServerUrlInput), typeof(string), typeof(AudioGenerationSettingsData), "https://api.openai.com/v1");
        public string ServerUrlInput { get => (string)GetValue(ServerUrlProperty); set => SetValue(ServerUrlProperty, value); }

        public static readonly BindableProperty AuthEnabledProperty = BindableProperty.Create(nameof(AuthEnabled), typeof(bool), typeof(AudioGenerationSettingsData), false);
        public bool AuthEnabled { get => (bool)GetValue(AuthEnabledProperty); set => SetValue(AuthEnabledProperty, value); }

        public static readonly BindableProperty TTSEnabledProperty = BindableProperty.Create(nameof(TTSEnabled), typeof(bool), typeof(AudioGenerationSettingsData), false);
        public bool TTSEnabled { get => (bool)GetValue(TTSEnabledProperty); set => SetValue(TTSEnabledProperty, value); }

        public static readonly BindableProperty ApiKeyProperty = BindableProperty.Create(nameof(ApiKey), typeof(string), typeof(AudioGenerationSettingsData), string.Empty);
        public string ApiKey { get => (string)GetValue(ApiKeyProperty); set => SetValue(ApiKeyProperty, value); }

        public static readonly BindableProperty TimeOutDelayProperty = BindableProperty.Create(nameof(TimeOutDelay), typeof(double), typeof(AudioGenerationSettingsData), 30.0);
        public double TimeOutDelay { get => (double)GetValue(TimeOutDelayProperty); set => SetValue(TimeOutDelayProperty, value); }

        public static readonly BindableProperty modelListProperty = BindableProperty.Create(nameof(ModelList), typeof(List<string>), typeof(AudioGenerationSettingsData), new List<string>());
        public List<string> ModelList { get => (List<string>)GetValue(modelListProperty); set => SetValue(modelListProperty, value); }

        public static readonly BindableProperty TimeModelListUpdatedProperty = BindableProperty.Create(nameof(TimeModelListUpdated), typeof(DateTime), typeof(AudioGenerationSettingsData), DateTime.MinValue);
        public DateTime TimeModelListUpdated { get => (DateTime)GetValue(TimeModelListUpdatedProperty); set => SetValue(TimeModelListUpdatedProperty, value); }

        public static readonly BindableProperty ModelListUpdatedUsingUrlProperty = BindableProperty.Create(nameof(ModelListUpdatedUsingUrl), typeof(string), typeof(AudioGenerationSettingsData), string.Empty);
        public string ModelListUpdatedUsingUrl { get => (string)GetValue(ModelListUpdatedUsingUrlProperty); set => SetValue(ModelListUpdatedUsingUrlProperty, value); }

        public static readonly BindableProperty SelectedModelIndexProperty = BindableProperty.Create(nameof(SelectedModelIndex), typeof(int), typeof(AudioGenerationSettingsData), int.MinValue);
        public int SelectedModelIndex { get => (int)GetValue(SelectedModelIndexProperty); set => SetValue(SelectedModelIndexProperty, value); }

        public static readonly BindableProperty SelectedModelProperty = BindableProperty.Create(nameof(SelectedModel), typeof(string), typeof(AudioGenerationSettingsData), "deliberate_v3");
        public string SelectedModel { get => (string)GetValue(SelectedModelProperty); set => SetValue(SelectedModelProperty, value); }

        public static readonly BindableProperty modelList2Property = BindableProperty.Create(nameof(ModelList2), typeof(List<string>), typeof(AudioGenerationSettingsData), new List<string>());
        public List<string> ModelList2 { get => (List<string>)GetValue(modelList2Property); set => SetValue(modelList2Property, value); }

        public static readonly BindableProperty SelectedModelIndex2Property = BindableProperty.Create(nameof(SelectedModelIndex2), typeof(int), typeof(AudioGenerationSettingsData), int.MinValue);
        public int SelectedModelIndex2 { get => (int)GetValue(SelectedModelIndex2Property); set => SetValue(SelectedModelIndex2Property, value); }

        public static readonly BindableProperty SelectedModel2Property = BindableProperty.Create(nameof(SelectedModel2), typeof(string), typeof(AudioGenerationSettingsData), "deliberate_v3");
        public string SelectedModel2 { get => (string)GetValue(SelectedModel2Property); set => SetValue(SelectedModel2Property, value); }

        public static readonly BindableProperty SpeedProperty = BindableProperty.Create(nameof(Speed), typeof(double), typeof(AudioGenerationSettingsData), 1.0);
        public double Speed { get => (double)GetValue(SpeedProperty); set => SetValue(SpeedProperty, value); }

        public static readonly BindableProperty VoiceProperty = BindableProperty.Create(nameof(Voice), typeof(string), typeof(AudioGenerationSettingsData), "onyx");
        public string Voice { get => (string)GetValue(VoiceProperty); set => SetValue(VoiceProperty, value); }

        public static readonly BindableProperty PromptProperty = BindableProperty.Create(nameof(Prompt), typeof(string), typeof(AudioGenerationSettingsData), string.Empty);
        public string Prompt { get => (string)GetValue(PromptProperty); set => SetValue(PromptProperty, value); }

        public static readonly BindableProperty PlayerMediaSourceProperty = BindableProperty.Create(nameof(PlayerMediaSource), typeof(MediaSource), typeof(AudioGenerationSettingsData), null);
        public MediaSource PlayerMediaSource { get => (MediaSource)GetValue(PlayerMediaSourceProperty); set => SetValue(PlayerMediaSourceProperty, value); }

    }
}