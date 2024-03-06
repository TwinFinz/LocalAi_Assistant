using LocalAiAssistant.Utilities;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Linq;
using System.Windows.Input;

namespace LocalAiAssistant
{
    public partial class GeneralSettings : ContentPage
    {

        public GeneralSettingsData UiData { get; set; } = new();
        /*
        private static string apiKey = "";
        private static string _serverUrl = "";
        private static List<string> botNameList = new();
        private static List<string> botNameListImageGen = new();
        private static List<string> botNameListAudio = new();
        private static List<string> editEndpointModelList = new();
        private static List<string> insertEndpointModelList = new();
        private static List<string> embedEndpointModelList = new();
        private static List<MyAIAPI.Message> TextGenerationMessages = new();
        private static List<MyAIAPI.Message> prevMessages = new();
        private static MyAIAPI.Message GptSystemMsg = new()
        {
            Role = "System",
            Content = "You are ChatGPT, a large language model trained by OpenAI. Answer as concisely as possible. Knowledge cutoff: {knowledge_cutoff} Current date: {current_date}"
        };
        private readonly object _lockBotNameList = new();
        private readonly object _lockEditModelList = new();
        private readonly object _lockInsertModelList = new();
        private readonly object _lockEmbedModelList = new();
        private readonly object _lockBotNameListImageGen = new();
        private readonly object _lockBotNameListAudio = new();
        private readonly object _lockselectedModel = new();
        private static string selectedModel = string.Empty;
        private static readonly string pathBase = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
        private static bool ttsEnabled = false;
        private static bool settingsShown = false;
        private static bool chatEmulation = false;
        private static EditText entry;
        private static ISharedPreferences sharedPreferences;
        private static int curLayout = int.MinValue;
        private static byte[] selectedAudioFile;
        private static string selectedAudioFileName = "";
        private static byte[] selectedImageFile;
        private static byte[] selectedMaskFile;
        private static readonly string preferenceName = Application.Context.PackageName;
        private const string apiKeyPreference = "ApiKey";
        private const string botListPreference = "BotList";
        private const string serverUrlPreference = "ServerUrl";
        private const string selectedModelPreference = "SelectedModel";
        private const string imageBotListPreference = "ImageBotList";
        private const string audioBotListPreference = "AudioBotList";
        private const string imagePreference = "imagePreference";
        private const string maskPreference = "maskPreference";
        private const string editModelPreference = "editModelList";
        private const string insertModelPreference = "insertModelList";
        private const string embedModelPreference = "embedModelList";
        private static string previousResponses = string.Empty;
        private static int backPressedTimes;
        */

#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable CS0169 // The field 'Settings.selectedAudioFile' is never used
#pragma warning disable CS0414 // The field 'Settings.ttsEnabled' is assigned but its value is never used
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
        internal static string MainPreference = "LocalAiAssistant";
        private static byte[]? selectedImageFile;
        private static byte[]? selectedMaskFile;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0052 // Remove unread private members
#pragma warning restore CS0169 // The field 'Settings.selectedAudioFile' is never used
#pragma warning restore CS0414 // The field 'Settings.ttsEnabled' is assigned but its value is never used

        public GeneralSettings()
        {
            InitializeComponent();
            BindingContext = UiData;
            OnLoadBtnClicked(this, new EventArgs());
        }
        private async void OnSaveBtnClicked(object? sender, EventArgs e)
        {
            SemanticScreenReader.Announce(SaveBtn.Text);
            if (UiData != null)
            {
                UiData.EncryptEnabled = EncryptSwitch.IsToggled;
                UiData.EncryptKey = EncryptKeyInput.Text;
                await MyMultiPlatformUtils.WriteToPreferences(MainPreference, UiData);
                await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Save Settings", "Success");
            }
        }
        private async void OnLoadBtnClicked(object? sender, EventArgs e)
        {
            SemanticScreenReader.Announce(LoadBtn.Text);
            if (MyMultiPlatformUtils.CheckPreferenceContains(MainPreference))
            {
                UiData = await MyMultiPlatformUtils.ReadFromPreferences<GeneralSettingsData>(MainPreference) ?? new();
                EncryptKeyInput.Text = UiData.EncryptKey;
                EncryptSwitch.IsToggled = UiData.EncryptEnabled;
            }
        }
    }
    public class GeneralSettingsData : BindableObject
    {
        public GeneralSettingsData()
        {
            // Initialize left swipe commands with corresponding methods
            LeftSwipeCommand1 = new Command(ExecuteLeftSwipeCommand1);
            LeftSwipeCommand2 = new Command(ExecuteLeftSwipeCommand2);
            RightSwipeCommand1 = new Command(ExecuteRightSwipeCommand1);
            RightSwipeCommand2 = new Command(ExecuteRightSwipeCommand2);
        }

        public static readonly BindableProperty EncryptEnabledProperty = BindableProperty.Create(nameof(EncryptEnabled), typeof(bool), typeof(GeneralSettingsData), false);
        public bool EncryptEnabled { get => (bool)GetValue(EncryptEnabledProperty); set => SetValue(EncryptEnabledProperty, value); }

        public static readonly BindableProperty EncryptKeyProperty = BindableProperty.Create(nameof(EncryptKey), typeof(string), typeof(GeneralSettingsData), string.Empty);
        public string EncryptKey { get => (string)GetValue(EncryptKeyProperty); set => SetValue(EncryptKeyProperty, value); }

        /** Example Definitions **/

        public static readonly BindableProperty ImageViewSourceProperty = BindableProperty.Create(nameof(ImageViewSource), typeof(ImageSource), typeof(GeneralSettingsData), null);
        public ImageSource? ImageViewSource { get => (ImageSource?)GetValue(ImageViewSourceProperty); set => SetValue(ImageViewSourceProperty, value); }

        public ICommand? LeftSwipeCommand1 { get; }
        public ICommand? LeftSwipeCommand2 { get; }
        public ICommand? RightSwipeCommand1 { get; }
        public ICommand? RightSwipeCommand2 { get; }
        private static async void ExecuteLeftSwipeCommand1()
        {
            await MyMultiPlatformUtils.WriteToLog("Left Swipe Command 1 executed!");
            // Implement your desired action here
        }
        private static async void ExecuteLeftSwipeCommand2()
        {
            await MyMultiPlatformUtils.WriteToLog("Left Swipe Command 2 executed!");
            // Implement your desired action here
        }
        private static async void ExecuteRightSwipeCommand1()
        {
            await MyMultiPlatformUtils.WriteToLog("Left Swipe Command 1 executed!");
            // Implement your desired action here
        }
        private static async void ExecuteRightSwipeCommand2()
        {
            await MyMultiPlatformUtils.WriteToLog("Left Swipe Command 2 executed!");
            // Implement your desired action here
        }
    }

}