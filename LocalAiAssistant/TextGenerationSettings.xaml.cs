using LocalAiAssistant.Utilities;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace LocalAiAssistant
{
    public partial class TextGenerationSettings : ContentPage
    {
        public TextGenerationSettingsData UiData { get; set; } = new();
        private GeneralSettingsData defaultData = new();
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable CS0169 // The field 'Settings.selectedAudioFile' is never used
#pragma warning disable CS0414 // The field 'TextGenerationSettings.ttsEnabled' is assigned but its value is never used
        private readonly object _lockBotNameList = new();
        private readonly object _lockEditModelList = new();
        private readonly object _lockInsertModelList = new();
        private readonly object _lockEmbedModelList = new();
        private readonly object _lockBotNameListImageGen = new();
        private readonly object _lockBotNameListAudio = new();
        private readonly object _lockselectedModel = new();
        private static bool ttsEnabled = false;
        private static bool settingsShown = false;
        private static byte[]? selectedAudioFile;
        internal static string TextGenerationPreference = "LocalAiAssistant-TextGeneration";
        private static byte[]? selectedImageFile;
        private static byte[]? selectedMaskFile;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0052 // Remove unread private members
#pragma warning restore CS0169 // The field 'Settings.selectedAudioFile' is never used
#pragma warning restore CS0414 // The field 'TextGenerationSettings.ttsEnabled' is assigned but its value is never used

        DisplayInfo displayInfo = new();
        public TextGenerationSettings()
        {
            InitializeComponent();
            BindingContext = UiData;
            OnLoadBtnClicked(this, new EventArgs());
            ApiKeyInput.Unfocused += ApiKeyInput_Unfocused;
            this.SizeChanged += OnPageSizeChanged;
            DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
            CustomServerSwitch.Toggled += CustomServerToggled;
        }
        private async void DeviceDisplay_MainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
        {
            displayInfo = e.DisplayInfo;
            MainGrid.MaximumHeightRequest = displayInfo.Height;
            MainGrid.MaximumWidthRequest = displayInfo.Width;
            if (displayInfo.Orientation == DisplayOrientation.Portrait)
            {
#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"DisplayChanged Set Orientation: Vertical");
                switchStack.Orientation = StackOrientation.Vertical;
#endif
            }
            else if (displayInfo.Orientation == DisplayOrientation.Landscape)
            {
#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"DisplayChanged Set Orientation: Horizontal");
                switchStack.Orientation = StackOrientation.Horizontal;
#endif
            }
        }
        private async void OnPageSizeChanged(object? sender, EventArgs e)
        {
            double aspectRatio = (double)Width / Height;
            if (aspectRatio > (4.0 / 3.0))
            {
#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"PageSize Set Orientation: Horizontal");
                switchStack.Orientation = StackOrientation.Horizontal;
#endif
            }
            else
            {
#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"PageSize Set Orientation: Vertical");
                switchStack.Orientation = StackOrientation.Vertical;
#endif
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
            UiData.CustomServerEnabled = e.Value;
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
            await UpdateModelsPickerList();
        }
        private async Task UpdateModelsPickerList()
        {
            if (UiData.ModelList.Count < 1 || DateTime.Now - UiData.TimeModelListUpdated > TimeSpan.FromMinutes(5) || UiData.ModelListUpdatedUsingUrl != UiData.ServerUrlInput)
            {
                try
                {
                    UiData.ModelList = await MyAIAPI.GetLocalAiModelNameList(apiKey: UiData.ApiKey, timeoutInSeconds: (int)UiData.TimeOutDelay, serverUrl: UiData.ServerUrlInput, authEnabled: UiData.AuthEnabled);
                    UiData.ModelListUpdatedUsingUrl = UiData.ServerUrlInput;
                    UiData.TimeModelListUpdated = DateTime.Now;
                }
                catch (Exception ex)
                {
                    await MyMultiPlatformUtils.WriteToLog($"Retrieving model list failed,\nPlease make sure the Server Url is set in settings.\n\nLocalAi: {ex}");
                }
            }
        }
        private void OnAddStopWordClicked(object sender, EventArgs e)
        {
            string newStopWord = NewStopWordEntry.Text;
            if (!string.IsNullOrWhiteSpace(newStopWord))
            {
                UiData.Stop.Add(newStopWord);
                // Update the ListView to reflect the changes
                StopListView.ItemsSource = null;
                StopListView.ItemsSource = UiData.Stop;
                NewStopWordEntry.Text = ""; // Clear the entry field after adding
            }
        }
        private void OnRemoveStopWordClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string stopWord)
            {
                UiData.Stop.Remove(stopWord);
                // Update the ListView to reflect the changes
                StopListView.ItemsSource = null;
                StopListView.ItemsSource = UiData.Stop;
            }
        }
        private void TimeOutDelaySliderChanged(object sender, ValueChangedEventArgs e)
        {
            double roundedValue = Math.Round(e.NewValue);
            UiData.TimeOutDelay = roundedValue;
        }
        private async void OnSaveBtnClicked(object? sender, EventArgs e)
        {
            SemanticScreenReader.Announce(SaveBtn.Text);
            if (UiData != null)
            {
                UiData.SelectedModelIndex = LlmModelPicker.SelectedIndex;
                UiData.ApiKey = ApiKeyInput.Text;
                UiData.ServerUrlInput = ServerUrlInput.Text;
                UiData.TimeOutDelay = TimeOutDelaySlider.Value;
                UiData.AuthEnabled = AuthSwitch.IsToggled;
                UiData.TTSEnabled = TTSSwitch.IsToggled;
                UiData.SystemPromptInput = SystemPromptInput.Text;
                await MyMultiPlatformUtils.WriteToPreferences(TextGenerationPreference, UiData);
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
            if (MyMultiPlatformUtils.CheckPreferenceContains(TextGenerationPreference))
            {
                TextGenerationSettingsData? saveData = await MyMultiPlatformUtils.ReadFromPreferences<TextGenerationSettingsData>(TextGenerationPreference);
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
                    UiData.SystemPromptInput = saveData.SystemPromptInput;
                    UiData.ModelList = saveData.ModelList;
                    UiData.SelectedModelIndex = saveData.SelectedModelIndex;
                    UiData.TimeModelListUpdated = saveData.TimeModelListUpdated;
                    UiData.ModelListUpdatedUsingUrl = saveData.ModelListUpdatedUsingUrl;
                    UiData.UserInput = saveData.UserInput;
                    UiData.Stream = saveData.Stream;
                    UiData.Logprobs = saveData.Logprobs;
                    UiData.Messages = saveData.Messages;
                    UiData.Model = saveData.Model;
                    UiData.FrequencyPenalty = saveData.FrequencyPenalty;
                    UiData.LogitBias = saveData.LogitBias;
                    UiData.TopLogprobs = saveData.TopLogprobs;
                    UiData.MaxTokens = saveData.MaxTokens;
                    UiData.N = saveData.N;
                    UiData.PresencePenalty = saveData.PresencePenalty;
                    UiData.Seed = saveData.Seed;
                    UiData.Stop = saveData.Stop;
                    UiData.Temperature = saveData.Temperature;
                    UiData.TopP = saveData.TopP;
                    UiData.Tools = saveData.Tools;
                    UiData.User = saveData.User;
                    UiData.MultiModal = saveData.MultiModal;
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
    public class TextGenerationSettingsData : BindableObject
    {
        public TextGenerationSettingsData()
        {
        }
        public static readonly BindableProperty UserInputProperty = BindableProperty.Create(nameof(UserInput), typeof(string), typeof(TextGenerationSettingsData), string.Empty);
        public string UserInput { get => (string)GetValue(UserInputProperty); set => SetValue(UserInputProperty, value); }

        public static readonly BindableProperty CustomServerEnabledProperty = BindableProperty.Create(nameof(CustomServerEnabled), typeof(bool), typeof(AudioGenerationSettingsData), false);
        public bool CustomServerEnabled { get => (bool)GetValue(CustomServerEnabledProperty); set => SetValue(CustomServerEnabledProperty, value); }

        public static readonly BindableProperty ServerUrlProperty = BindableProperty.Create(nameof(ServerUrlInput), typeof(string), typeof(TextGenerationSettingsData), "https://api.openai.com/v1");
        public string ServerUrlInput { get => (string)GetValue(ServerUrlProperty); set => SetValue(ServerUrlProperty, value); }

        public static readonly BindableProperty AuthEnabledProperty = BindableProperty.Create(nameof(AuthEnabled), typeof(bool), typeof(TextGenerationSettingsData), false);
        public bool AuthEnabled { get => (bool)GetValue(AuthEnabledProperty); set => SetValue(AuthEnabledProperty, value); }

        public static readonly BindableProperty TTSEnabledProperty = BindableProperty.Create(nameof(TTSEnabled), typeof(bool), typeof(TextGenerationSettingsData), false);
        public bool TTSEnabled { get => (bool)GetValue(TTSEnabledProperty); set => SetValue(TTSEnabledProperty, value); }

        public static readonly BindableProperty StreamProperty = BindableProperty.Create(nameof(Stream), typeof(bool), typeof(TextGenerationSettingsData), false);
        public bool Stream { get => (bool)GetValue(StreamProperty); set => SetValue(StreamProperty, value); }

        public static readonly BindableProperty LogprobsProperty = BindableProperty.Create(nameof(Logprobs), typeof(bool), typeof(TextGenerationSettingsData), false);
        public bool Logprobs { get => (bool)GetValue(LogprobsProperty); set => SetValue(LogprobsProperty, value); }

        public static readonly BindableProperty ApiKeyProperty = BindableProperty.Create(nameof(ApiKey), typeof(string), typeof(TextGenerationSettingsData), string.Empty);
        public string ApiKey { get => (string)GetValue(ApiKeyProperty); set => SetValue(ApiKeyProperty, value); }

        public static readonly BindableProperty TimeOutDelayProperty = BindableProperty.Create(nameof(TimeOutDelay), typeof(double), typeof(TextGenerationSettingsData), 240.0);
        public double TimeOutDelay { get => (double)GetValue(TimeOutDelayProperty); set => SetValue(TimeOutDelayProperty, value); }

        public static readonly BindableProperty ModelListProperty = BindableProperty.Create(nameof(ModelList), typeof(List<string>), typeof(TextGenerationSettingsData), new List<string>());
        public List<string> ModelList { get => (List<string>)GetValue(ModelListProperty); set => SetValue(ModelListProperty, value); }

        public static readonly BindableProperty TimeModelListUpdatedProperty = BindableProperty.Create(nameof(TimeModelListUpdated), typeof(DateTime), typeof(TextGenerationSettingsData), DateTime.MinValue);
        public DateTime TimeModelListUpdated { get => (DateTime)GetValue(TimeModelListUpdatedProperty); set => SetValue(TimeModelListUpdatedProperty, value); }

        public static readonly BindableProperty ModelListUpdatedUsingUrlProperty = BindableProperty.Create(nameof(ModelListUpdatedUsingUrl), typeof(string), typeof(TextGenerationSettingsData), string.Empty);
        public string ModelListUpdatedUsingUrl { get => (string)GetValue(ModelListUpdatedUsingUrlProperty); set => SetValue(ModelListUpdatedUsingUrlProperty, value); }

        public static readonly BindableProperty SelectedModelIndexProperty = BindableProperty.Create(nameof(SelectedModelIndex), typeof(int), typeof(TextGenerationSettingsData), int.MinValue);
        public int SelectedModelIndex { get => (int)GetValue(SelectedModelIndexProperty); set => SetValue(SelectedModelIndexProperty, value); }

        public static readonly BindableProperty SystemPromptInputProperty = BindableProperty.Create(nameof(SystemPromptInput), typeof(string), typeof(TextGenerationSettingsData), "You are a helpful AI assistant.");
        public string SystemPromptInput { get => (string)GetValue(SystemPromptInputProperty); set => SetValue(SystemPromptInputProperty, value); }

        public static readonly BindableProperty MessagesProperty = BindableProperty.Create(nameof(Messages), typeof(List<MyAIAPI.Message>), typeof(TextGenerationSettingsData), new List<MyAIAPI.Message>());
        public List<MyAIAPI.Message> Messages { get => (List<MyAIAPI.Message>)GetValue(MessagesProperty); set => SetValue(MessagesProperty, value); }

        public static readonly BindableProperty ModelProperty = BindableProperty.Create(nameof(Model), typeof(string), typeof(TextGenerationSettingsData), "gpt-3.5-turbo");
        public string Model { get => (string)GetValue(ModelProperty); set => SetValue(ModelProperty, value); }

        public static readonly BindableProperty MultiModalProperty = BindableProperty.Create(nameof(MultiModal), typeof(bool), typeof(TextGenerationSettingsData), false);
        public bool MultiModal { get => (bool)GetValue(MultiModalProperty); set => SetValue(MultiModalProperty, value); }

        public static readonly BindableProperty FrequencyPenaltyProperty = BindableProperty.Create(nameof(FrequencyPenalty), typeof(double), typeof(TextGenerationSettingsData), 0.3);
        public double FrequencyPenalty { get => (double)GetValue(FrequencyPenaltyProperty); set => SetValue(FrequencyPenaltyProperty, value); }

        public static readonly BindableProperty LogitBiasProperty = BindableProperty.Create(nameof(LogitBias), typeof(Dictionary<string, int>), typeof(TextGenerationSettingsData), null);
        public Dictionary<string, int> LogitBias { get => (Dictionary<string, int>)GetValue(LogitBiasProperty); set => SetValue(LogitBiasProperty, value); }

        public static readonly BindableProperty TopLogprobsProperty = BindableProperty.Create(nameof(TopLogprobs), typeof(int), typeof(TextGenerationSettingsData), 80);
        public int TopLogprobs { get => (int)GetValue(TopLogprobsProperty); set => SetValue(TopLogprobsProperty, value); }

        public static readonly BindableProperty MaxTokensProperty = BindableProperty.Create(nameof(MaxTokens), typeof(int), typeof(TextGenerationSettingsData), 4096);
        public int MaxTokens { get => (int)GetValue(MaxTokensProperty); set => SetValue(MaxTokensProperty, value); }

        public static readonly BindableProperty NProperty = BindableProperty.Create(nameof(N), typeof(int), typeof(TextGenerationSettingsData), 1);
        public int N { get => (int)GetValue(NProperty); set => SetValue(NProperty, value); }

        public static readonly BindableProperty PresencePenaltyProperty = BindableProperty.Create(nameof(PresencePenalty), typeof(double), typeof(TextGenerationSettingsData), 0.2);
        public double PresencePenalty { get => (double)GetValue(PresencePenaltyProperty); set => SetValue(PresencePenaltyProperty, value); }

        public static readonly BindableProperty SeedProperty = BindableProperty.Create(nameof(Seed), typeof(int), typeof(TextGenerationSettingsData), -1);
        public int Seed { get => (int)GetValue(SeedProperty); set => SetValue(SeedProperty, value); }

        public static readonly BindableProperty StopProperty = BindableProperty.Create(nameof(Stop), typeof(List<string>), typeof(TextGenerationSettingsData), new List<string>());
        public List<string> Stop { get => (List<string>)GetValue(StopProperty); set => SetValue(StopProperty, value); }

        public static readonly BindableProperty TemperatureProperty = BindableProperty.Create(nameof(Temperature), typeof(double), typeof(TextGenerationSettingsData), 0.7);
        public double Temperature { get => (double)GetValue(TemperatureProperty); set => SetValue(TemperatureProperty, value); }

        public static readonly BindableProperty TopPProperty = BindableProperty.Create(nameof(TopP), typeof(double), typeof(TextGenerationSettingsData), 0.9);
        public double TopP { get => (double)GetValue(TopPProperty); set => SetValue(TopPProperty, value); }

        public static readonly BindableProperty ToolsProperty = BindableProperty.Create(nameof(Tools), typeof(List<string>), typeof(TextGenerationSettingsData), new List<string>());
        public List<string> Tools { get => (List<string>)GetValue(ToolsProperty); set => SetValue(ToolsProperty, value); }

        public static readonly BindableProperty UserProperty = BindableProperty.Create(nameof(User), typeof(string), typeof(TextGenerationSettingsData), string.Empty);
        public string User { get => (string)GetValue(UserProperty); set => SetValue(UserProperty, value); }

        public static readonly BindableProperty SelectedImageProperty = BindableProperty.Create(nameof(SelectedImage), typeof(byte[]), typeof(TextGenerationSettingsData), Array.Empty<byte>());
        public byte[] SelectedImage { get => (byte[])GetValue(SelectedImageProperty); set => SetValue(SelectedImageProperty, value); }


        /* Needs to be implemented
        public static readonly BindableProperty ResponseFormatProperty = BindableProperty.Create(nameof(ResponseFormat), typeof(ResponseFormat), typeof(TextGenerationSettingsData), new ResponseFormat());
        public ResponseFormat ResponseFormat { get => (ResponseFormat)GetValue(ResponseFormatProperty); set => SetValue(ResponseFormatProperty, value); }

        public static readonly BindableProperty ToolChoiceProperty = BindableProperty.Create(nameof(ToolChoice), typeof(ToolChoice), typeof(TextGenerationSettingsData), new ToolChoice());
        public ToolChoice ToolChoice { get => (ToolChoice)GetValue(ToolChoiceProperty); set => SetValue(ToolChoiceProperty, value); }

        public static readonly BindableProperty FunctionCallProperty = BindableProperty.Create(nameof(FunctionCall), typeof(FunctionCall), typeof(TextGenerationSettingsData), new FunctionCall());
        public FunctionCall FunctionCall { get => (FunctionCall)GetValue(FunctionCallProperty); set => SetValue(FunctionCallProperty, value); }
        */
        // Other existing properties...
    }
}
