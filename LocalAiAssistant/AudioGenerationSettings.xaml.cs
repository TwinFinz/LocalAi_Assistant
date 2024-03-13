using CommunityToolkit.Maui.Views;
using LocalAiAssistant.Utilities;

namespace LocalAiAssistant
{

    public partial class AudioGenerationSettings : ContentPage
    {
        public AudioGenerationSettingsData UiData { get; set; } = new();
        internal static string TtsPreference = "LocalAiAssistant-TTS";
        internal static string ModelListPreference = "LocalAiAssistant-Models";
        internal static string SecondaryModelListPreference = "LocalAiAssistant-SecondaryModels";
        private GeneralSettingsData defaultData = new();
        public AudioGenerationSettings()
        {
            InitializeComponent();
            BindingContext = UiData;
            OnLoadBtnClicked(this, new EventArgs());
            ApiKeyInput.Unfocused += ApiKeyInput_Unfocused;
            SizeChanged += OnPageSizeChanged;
            DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
            CustomServerSwitch.Toggled += CustomServerToggled;
        }
        public List<string> BarkModels = new List<string>()
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
        public List<string> CoquiModels = new List<string>()
        {
            "tts_models/multilingual/multi-dataset/xtts_v2",
            "tts_models/multilingual/multi-dataset/xtts_v1.1",
            "tts_models/multilingual/multi-dataset/your_tts",
            "tts_models/multilingual/multi-dataset/bark",
            "tts_models/bg/cv/vits",
            "tts_models/cs/cv/vits",
            "tts_models/da/cv/vits",
            "tts_models/et/cv/vits",
            "tts_models/ga/cv/vits",
            "tts_models/en/ek1/tacotron2",
            "tts_models/en/ljspeech/tacotron2-DDC",
            "tts_models/en/ljspeech/tacotron2-DDC_ph",
            "tts_models/en/ljspeech/glow-tts",
            "tts_models/en/ljspeech/speedy-speech",
            "tts_models/en/ljspeech/tacotron2-DCA",
            "tts_models/en/ljspeech/vits",
            "tts_models/en/ljspeech/vits--neon",
            "tts_models/en/ljspeech/fast_pitch",
            "tts_models/en/ljspeech/overflow",
            "tts_models/en/ljspeech/neural_hmm",
            "tts_models/en/vctk/vits",
            "tts_models/en/vctk/fast_pitch",
            "tts_models/en/sam/tacotron-DDC",
            "tts_models/en/blizzard2013/capacitron-t2-c50",
            "tts_models/en/blizzard2013/capacitron-t2-c150_v2",
            "tts_models/en/multi-dataset/tortoise-v2",
            "tts_models/en/jenny/jenny",
            "tts_models/es/mai/tacotron2-DDC",
            "tts_models/es/css10/vits",
            "tts_models/fr/mai/tacotron2-DDC",
            "tts_models/fr/css10/vits",
            "tts_models/uk/mai/glow-tts",
            "tts_models/uk/mai/vits",
            "tts_models/zh-CN/baker/tacotron2-DDC-GST",
            "tts_models/nl/mai/tacotron2-DDC",
            "tts_models/nl/css10/vits",
            "tts_models/de/thorsten/tacotron2-DCA",
            "tts_models/de/thorsten/vits",
            "tts_models/de/thorsten/tacotron2-DDC",
            "tts_models/de/css10/vits-neon",
            "tts_models/ja/kokoro/tacotron2-DDC",
            "tts_models/tr/common-voice/glow-tts",
            "tts_models/it/mai_female/glow-tts",
            "tts_models/it/mai_female/vits",
            "tts_models/it/mai_male/glow-tts",
            "tts_models/it/mai_male/vits",
            "tts_models/ewe/openbible/vits",
            "tts_models/hau/openbible/vits",
            "tts_models/lin/openbible/vits",
            "tts_models/tw_akuapem/openbible/vits",
            "tts_models/tw_asante/openbible/vits",
            "tts_models/yor/openbible/vits",
            "tts_models/hu/css10/vits",
            "tts_models/el/cv/vits",
            "tts_models/fi/css10/vits",
            "tts_models/hr/cv/vits",
            "tts_models/lt/cv/vits",
            "tts_models/lv/cv/vits",
            "tts_models/mt/cv/vits",
            "tts_models/pl/mai_female/vits",
            "tts_models/pt/cv/vits",
            "tts_models/ro/cv/vits",
            "tts_models/sk/cv/vits",
            "tts_models/sl/cv/vits",
            "tts_models/sv/cv/vits",
            "tts_models/ca/custom/vits",
            "tts_models/fa/custom/glow-tts",
            "tts_models/bn/custom/vits-male",
            "tts_models/bn/custom/vits-female",
            "tts_models/be/common-voice/glow-tts",
        };
        public List<string> CoquiVocoderModels = new List<string>()
        {        
            "vocoder_models/universal/libri-tts/wavegrad",
            "vocoder_models/universal/libri-tts/fullband-melgan",
            "vocoder_models/en/ek1/wavegrad",
            "vocoder_models/en/ljspeech/multiband-melgan",
            "vocoder_models/en/ljspeech/hifigan_v2",
            "vocoder_models/en/ljspeech/univnet",
            "vocoder_models/en/blizzard2013/hifigan_v2",
            "vocoder_models/en/vctk/hifigan_v2",
            "vocoder_models/en/sam/hifigan_v2",
            "vocoder_models/nl/mai/parallel-wavegan",
            "vocoder_models/de/thorsten/wavegrad",
            "vocoder_models/de/thorsten/fullband-melgan",
            "vocoder_models/de/thorsten/hifigan_v1",
            "vocoder_models/ja/kokoro/hifigan_v1",
            "vocoder_models/uk/mai/multiband-melgan",
            "vocoder_models/tr/common-voice/hifigan",
            "vocoder_models/be/common-voice/hifigan"
        };
        public List<string> PiperModels = new List<string>()
        {
            "",
            ""
        };
        private void OnModelSelected(object? sender, EventArgs e)
        {
            if (UiData.ModelList.Count > 0 && UiData.SelectedModelIndex >= 0 && UiData.SelectedModelIndex < UiData.ModelList.Count)
            {
                UiData.SelectedModel = UiData.ModelList[UiData.SelectedModelIndex];
                if (UiData.ModelList[UiData.SelectedModelIndex] == "Bark")
                {
                    UiData.SecondaryModelList = BarkModels;
                    SecondaryModelPicker.IsVisible = true;
                }
                else if (UiData.ModelList[UiData.SelectedModelIndex] == "CoquiTTS")
                {
                    UiData.SecondaryModelList = CoquiModels;
                    SecondaryModelPicker.IsVisible = true;
                }
                else if (UiData.ModelList[UiData.SelectedModelIndex] == "Piper")
                {
                    UiData.SecondaryModelList = PiperModels;
                    SecondaryModelPicker.IsVisible = true;
                }
                else
                {
                    SecondaryModelPicker.IsVisible = false;
                }
            }
        }
        private void OnSecondaryModelSelected(object? sender, EventArgs e)
        {
            if (UiData.SecondaryModelList.Count > 0 && UiData.SelectedModelIndex2 >= 0 && UiData.SelectedModelIndex2 < UiData.SecondaryModelList.Count)
            {
                UiData.SelectedModel2 = UiData.SecondaryModelList[UiData.SelectedModelIndex2];
            }
        }
        private void OnTranscriptionModelSelected(object? sender, EventArgs e)
        {
            if (UiData.ModelList.Count > 0 && UiData.SelectedTranscriptionModelIndex >= 0 && UiData.SelectedTranscriptionModelIndex < UiData.ModelList.Count)
            {
                UiData.SelectedTranscriptionModel = UiData.ModelList[UiData.SelectedTranscriptionModelIndex];
            }
        }

        private async void OnPageSizeChanged(object? sender, EventArgs e)
        {
            double aspectRatio = (double)Width / Height;
            if (aspectRatio > (4.0 / 3.0))
            {
#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"DisplayChanged Set Orientation: Horizontal");
#endif
                ModelPickers.Orientation = StackOrientation.Horizontal;
            }
            else
            {
#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"DisplayChanged Set Orientation: Vertical");
#endif
                ModelPickers.Orientation = StackOrientation.Vertical;
            }
        }
        private async void DeviceDisplay_MainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
        {
            var displayInfo = e.DisplayInfo;
            MainView.MaximumHeightRequest = displayInfo.Height;
            MainView.MaximumWidthRequest = displayInfo.Width;
            if (displayInfo.Orientation == DisplayOrientation.Portrait)
            {
                ModelPickers.Orientation = StackOrientation.Vertical;
#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"DisplayChanged Set Orientation: Vertical");
#endif
            }
            else if (displayInfo.Orientation == DisplayOrientation.Landscape)
            {
                ModelPickers.Orientation = StackOrientation.Horizontal;
#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"DisplayChanged Set Orientation: Horizontal");
#endif
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
                await MyMultiPlatformUtils.WriteToPreferences(AudioGenerationSettings.ModelListPreference, UiData.ModelList);
                await MyMultiPlatformUtils.WriteToPreferences(AudioGenerationSettings.SecondaryModelListPreference, UiData.SecondaryModelList);
                UiData.ModelList = new();
                UiData.SecondaryModelList = new();
                await MyMultiPlatformUtils.WriteToPreferences(AudioGenerationSettings.TtsPreference, UiData);
                OnLoadBtnClicked(this, new EventArgs());
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
                    defaultData.DefaultTimeOutDelay = saveData.DefaultTimeOutDelay;
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
                        UiData.SelectedTranscriptionModel = UiData.ModelList[UiData.SelectedTranscriptionModelIndex];
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

        public static readonly BindableProperty SecondaryModelListProperty = BindableProperty.Create(nameof(SecondaryModelList), typeof(List<string>), typeof(AudioGenerationSettingsData), new List<string>());
        public List<string> SecondaryModelList { get => (List<string>)GetValue(SecondaryModelListProperty); set => SetValue(SecondaryModelListProperty, value); }

        public static readonly BindableProperty SelectedModelIndex2Property = BindableProperty.Create(nameof(SelectedModelIndex2), typeof(int), typeof(AudioGenerationSettingsData), int.MinValue);
        public int SelectedModelIndex2 { get => (int)GetValue(SelectedModelIndex2Property); set => SetValue(SelectedModelIndex2Property, value); }

        public static readonly BindableProperty SelectedModel2Property = BindableProperty.Create(nameof(SelectedModel2), typeof(string), typeof(AudioGenerationSettingsData), "deliberate_v3");
        public string SelectedModel2 { get => (string)GetValue(SelectedModel2Property); set => SetValue(SelectedModel2Property, value); }

        public static readonly BindableProperty SelectedTranscriptionModelIndexProperty = BindableProperty.Create(nameof(SelectedTranscriptionModelIndex), typeof(int), typeof(AudioGenerationSettingsData), int.MinValue);
        public int SelectedTranscriptionModelIndex { get => (int)GetValue(SelectedTranscriptionModelIndexProperty); set => SetValue(SelectedTranscriptionModelIndexProperty, value); }

        public static readonly BindableProperty SelectedTranscriptionModelProperty = BindableProperty.Create(nameof(SelectedTranscriptionModel), typeof(string), typeof(AudioGenerationSettingsData), "deliberate_v3");
        public string SelectedTranscriptionModel { get => (string)GetValue(SelectedTranscriptionModelProperty); set => SetValue(SelectedTranscriptionModelProperty, value); }

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