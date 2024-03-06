
using CommunityToolkit.Maui.Views;
using LocalAiAssistant.Utilities;

namespace LocalAiAssistant
{
    public partial class ImageGenerationSettings : ContentPage
    {
        public ImageGenerationSettingsData UiData { get; set; } = new();
        internal static string StableDiffusionPreference = "LocalAiAssistant-StableDiffusion";
        public ImageGenerationSettings()
        {
            InitializeComponent();
            BindingContext = UiData;
            OnLoadBtnClicked(this, new EventArgs());
            ApiKeyInput.Unfocused += ApiKeyInput_Unfocused;
            SizeChanged += OnPageSizeChanged;
            DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
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
                    UiData.ModelList = await MyAIAPI.GetLocalAiModelNameList(apiKey: UiData.ApiKey, timeoutInSeconds: 3, serverUrl: UiData.ServerUrlInput, authEnabled: UiData.AuthEnabled);
                    UiData.ModelListUpdatedUsingUrl = UiData.ServerUrlInput;
                    UiData.TimeModelListUpdated = DateTime.Now;
                }
                catch (Exception ex)
                {
                    await MyMultiPlatformUtils.WriteToLog($"Retrieving model list failed,\nPlease make sure the Server Url is set in settings.\n\nLocalAi: {ex}");
                }
            }
        }
        private void TimeOutDelaySliderChanged(object sender, ValueChangedEventArgs e)
        {
            double roundedValue = Math.Round(e.NewValue);
            UiData.TimeOutDelay = roundedValue;
        }
        private async void OnPageSizeChanged(object? sender, EventArgs e)
        {
            double aspectRatio = (double)Width / Height;
            if (aspectRatio > (4.0 / 3.0))
            {
                ModelPickers.Orientation = StackOrientation.Horizontal;
#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"DisplayChanged Set Orientation: Horizontal");
#endif
            }
            else
            {
                ModelPickers.Orientation = StackOrientation.Vertical;
#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"DisplayChanged Set Orientation: Vertical");
#endif
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

        private async void OnSaveBtnClicked(object? sender, EventArgs e)
        {
            SemanticScreenReader.Announce(SaveBtn.Text);
            if (UiData != null)
            {
                await MyMultiPlatformUtils.WriteToPreferences(StableDiffusionPreference, UiData);
                await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Save Settings", "Success");
            }
        }
        private async void OnLoadBtnClicked(object? sender, EventArgs e)
        {
            SemanticScreenReader.Announce(LoadBtn.Text);
            if (MyMultiPlatformUtils.CheckPreferenceContains(StableDiffusionPreference))
            {
                ImageGenerationSettingsData? saveData = await MyMultiPlatformUtils.ReadFromPreferences<ImageGenerationSettingsData>(StableDiffusionPreference);
                if (saveData != null)
                {
                    UiData.ApiKey = saveData.ApiKey;
                    UiData.ServerUrlInput = saveData.ServerUrlInput;
                    UiData.TimeOutDelay = saveData.TimeOutDelay;
                    UiData.AuthEnabled = saveData.AuthEnabled;
                    UiData.TTSEnabled = saveData.TTSEnabled;
                    UiData.Prompt = saveData.Prompt;
                    UiData.NegativePrompt = saveData.NegativePrompt;
                    UiData.SelectedTxt2ImgModel = saveData.SelectedTxt2ImgModel;
                    UiData.SelectedImg2ImgModel = saveData.SelectedImg2ImgModel;
                    UiData.SamplingMethod = saveData.SamplingMethod;
                    UiData.SamplingSteps = saveData.SamplingSteps;
                    UiData.CFGScale = saveData.CFGScale;
                    UiData.ImageHeight = saveData.ImageHeight;
                    UiData.ImageWidth = saveData.ImageWidth;
                    UiData.BatchCount = saveData.BatchCount;
                    UiData.BatchSize = saveData.BatchSize;
                    UiData.Seed = saveData.Seed;
                    UiData.TilingEnabled = saveData.TilingEnabled;
                    UiData.RestoreFacesEnabled = saveData.RestoreFacesEnabled;
                    UiData.HiResFixEnabled = saveData.HiResFixEnabled;
                    UiData.IsPortrait = saveData.IsPortrait;
                    UiData.IsLandscape = saveData.IsLandscape;
                    UiData.ImageUrl = saveData.ImageUrl;
                    UiData.ImageViewSource = saveData.ImageViewSource;
                    UiData.MaskImage = saveData.MaskImage;
                    UiData.Styles = saveData.Styles;
                    UiData.Subseed = saveData.Subseed;
                    UiData.SubseedStrength = saveData.SubseedStrength;
                    UiData.SeedResizeFromH = saveData.SeedResizeFromH;
                    UiData.SeedResizeFromW = saveData.SeedResizeFromW;
                    UiData.DenoisingStrength = saveData.DenoisingStrength;
                    UiData.SMinUncond = saveData.SMinUncond;
                    UiData.SChurn = saveData.SChurn;
                    UiData.STmax = saveData.STmax;
                    UiData.STmin = saveData.STmin;
                    UiData.SigmaNoise = saveData.SigmaNoise;
                    UiData.OverrideSettings = saveData.OverrideSettings;
                    UiData.RefinerSwitchAt = saveData.RefinerSwitchAt;
                    UiData.DisableExtraNetworks = saveData.DisableExtraNetworks;
                    UiData.InitImages = saveData.InitImages;
                    UiData.ResizeMode = saveData.ResizeMode;
                    UiData.ImgCFGScale = saveData.ImgCFGScale;
                    UiData.MaskBlurX = saveData.MaskBlurX;
                    UiData.MaskBlurY = saveData.MaskBlurY;
                    UiData.MaskBlur = saveData.MaskBlur;
                    UiData.InpaintingFill = saveData.InpaintingFill;
                    UiData.InpaintFullRes = saveData.InpaintFullRes;
                    UiData.InpaintingFullResPadding = saveData.InpaintingFullResPadding;
                    UiData.InpaintingMaskInvert = saveData.InpaintingMaskInvert;
                    UiData.InitialNoiseMultiplier = saveData.InitialNoiseMultiplier;
                    UiData.LatentMask = saveData.LatentMask;
                    UiData.SamplerIndex = saveData.SamplerIndex;
                    UiData.IncludeInitImages = saveData.IncludeInitImages;
                    UiData.ScriptName = saveData.ScriptName;
                    UiData.ScriptArgs = saveData.ScriptArgs;
                    UiData.SendImages = saveData.SendImages;
                    UiData.SaveImages = saveData.SaveImages;
                    UiData.OverrideSettingsRestoreAfterwards = saveData.OverrideSettingsRestoreAfterwards;
                    UiData.InvertMask = saveData.InvertMask;
                    UiData.MediaSource = saveData.MediaSource;
                    UiData.ModelList = saveData.ModelList;
                    UiData.SelectedTxt2ImgModelIndex = saveData.SelectedTxt2ImgModelIndex;
                    if (UiData.ModelList.Count > 1 && UiData.SelectedTxt2ImgModelIndex > 0 && UiData.SelectedTxt2ImgModelIndex < UiData.ModelList.Count)
                    {
                        UiData.SelectedTxt2ImgModel = saveData.ModelList[saveData.SelectedTxt2ImgModelIndex];
                    }
                    UiData.SelectedImg2ImgModelIndex = saveData.SelectedImg2ImgModelIndex;
                    if (UiData.ModelList.Count > 1 && UiData.SelectedImg2ImgModelIndex > 0 && UiData.SelectedImg2ImgModelIndex < UiData.ModelList.Count)
                    {
                        UiData.SelectedImg2ImgModel = saveData.ModelList[saveData.SelectedImg2ImgModelIndex];
                    }
                    UiData.SelectedTxt2VideoModelIndex = saveData.SelectedTxt2VideoModelIndex;
                    if (UiData.ModelList.Count > 1 && UiData.SelectedTxt2VideoModelIndex > 0 && UiData.SelectedTxt2VideoModelIndex < UiData.ModelList.Count)
                    {
                        UiData.SelectedTxt2VideoModel = saveData.ModelList[saveData.SelectedTxt2VideoModelIndex];
                    }
                    UiData.SelectedImg2VideoModelIndex = saveData.SelectedImg2VideoModelIndex;
                    if (UiData.ModelList.Count > 1 && UiData.SelectedImg2VideoModelIndex > 0 && UiData.SelectedImg2VideoModelIndex < UiData.ModelList.Count)
                    {
                        UiData.SelectedImg2VideoModel = saveData.ModelList[saveData.SelectedImg2VideoModelIndex];
                    }
                    if (UiData.ServerModeNames.Count > 1 && saveData.SelectedServerModeIndex < UiData.ServerModeNames.Count)
                    {
                        UiData.ServerMode = (ImageGenerationSettingsData.ServerModes)saveData.SelectedServerModeIndex;
                        UiData.SelectedServerModeIndex = saveData.SelectedServerModeIndex;
                        if (UiData.ServerMode == ImageGenerationSettingsData.ServerModes.Automatic1111)
                        {
                            Automatic1111Switches.IsVisible = true;
                        }
                        else
                        {
                            Automatic1111Switches.IsVisible = false;
                        }
                    }
                    else
                    {
                        UiData.ServerMode = ImageGenerationSettingsData.ServerModes.OpenAi;
                    }

                }
            }
        }
    }
    public class ImageGenerationSettingsData : BindableObject
    {
        public enum ServerModes
        {
            OpenAi,
            Automatic1111,
            LocalAi
        }
        public static readonly BindableProperty ServerModeNamesProperty = BindableProperty.Create(nameof(ServerModeNames), typeof(List<string>), typeof(ImageGenerationSettingsData), Enum.GetNames(typeof(ServerModes)).ToList());
        public List<string> ServerModeNames { get => (List<string>)GetValue(ServerModeNamesProperty); set => SetValue(ServerModeNamesProperty, value); }

        public static readonly BindableProperty SelectedServerModeIndexIndexProperty = BindableProperty.Create(nameof(SelectedServerModeIndex), typeof(int), typeof(ImageGenerationSettingsData), int.MinValue);
        public int SelectedServerModeIndex { get => (int)GetValue(SelectedServerModeIndexIndexProperty); set => SetValue(SelectedServerModeIndexIndexProperty, value); }

        public static readonly BindableProperty ServerModeProperty = BindableProperty.Create(nameof(ServerMode), typeof(ServerModes), typeof(ImageGenerationSettingsData), ServerModes.OpenAi);
        public ServerModes ServerMode { get => (ServerModes)GetValue(ServerModeProperty); set => SetValue(ServerModeProperty, value); }

        public static readonly BindableProperty ServerUrlProperty = BindableProperty.Create(nameof(ServerUrlInput), typeof(string), typeof(ImageGenerationSettingsData), "https://api.openai.com/v1");
        public string ServerUrlInput { get => (string)GetValue(ServerUrlProperty); set => SetValue(ServerUrlProperty, value); }

        public static readonly BindableProperty AuthEnabledProperty = BindableProperty.Create(nameof(AuthEnabled), typeof(bool), typeof(ImageGenerationSettingsData), false);
        public bool AuthEnabled { get => (bool)GetValue(AuthEnabledProperty); set => SetValue(AuthEnabledProperty, value); }

        public static readonly BindableProperty TTSEnabledProperty = BindableProperty.Create(nameof(TTSEnabled), typeof(bool), typeof(ImageGenerationSettingsData), false);
        public bool TTSEnabled { get => (bool)GetValue(TTSEnabledProperty); set => SetValue(TTSEnabledProperty, value); }

        public static readonly BindableProperty ApiKeyProperty = BindableProperty.Create(nameof(ApiKey), typeof(string), typeof(ImageGenerationSettingsData), string.Empty);
        public string ApiKey { get => (string)GetValue(ApiKeyProperty); set => SetValue(ApiKeyProperty, value); }

        public static readonly BindableProperty TimeOutDelayProperty = BindableProperty.Create(nameof(TimeOutDelay), typeof(double), typeof(ImageGenerationSettingsData), 240.0);
        public double TimeOutDelay { get => (double)GetValue(TimeOutDelayProperty); set => SetValue(TimeOutDelayProperty, value); }

        public static readonly BindableProperty modelListProperty = BindableProperty.Create(nameof(ModelList), typeof(List<string>), typeof(ImageGenerationSettingsData), new List<string>());
        public List<string> ModelList { get => (List<string>)GetValue(modelListProperty); set => SetValue(modelListProperty, value); }

        public static readonly BindableProperty TimeModelListUpdatedProperty = BindableProperty.Create(nameof(TimeModelListUpdated), typeof(DateTime), typeof(ImageGenerationSettingsData), DateTime.MinValue);
        public DateTime TimeModelListUpdated { get => (DateTime)GetValue(TimeModelListUpdatedProperty); set => SetValue(TimeModelListUpdatedProperty, value); }

        public static readonly BindableProperty ModelListUpdatedUsingUrlProperty = BindableProperty.Create(nameof(ModelListUpdatedUsingUrl), typeof(string), typeof(ImageGenerationSettingsData), string.Empty);
        public string ModelListUpdatedUsingUrl { get => (string)GetValue(ModelListUpdatedUsingUrlProperty); set => SetValue(ModelListUpdatedUsingUrlProperty, value); }

        public static readonly BindableProperty SelectedTxt2ImgModelIndexProperty = BindableProperty.Create(nameof(SelectedTxt2ImgModelIndex), typeof(int), typeof(ImageGenerationSettingsData), int.MinValue);
        public int SelectedTxt2ImgModelIndex { get => (int)GetValue(SelectedTxt2ImgModelIndexProperty); set => SetValue(SelectedTxt2ImgModelIndexProperty, value); }

        public static readonly BindableProperty SelectedTxt2ImgModelProperty = BindableProperty.Create(nameof(SelectedTxt2ImgModel), typeof(string), typeof(ImageGenerationSettingsData), "dall-e-2");
        public string SelectedTxt2ImgModel { get => (string)GetValue(SelectedTxt2ImgModelProperty); set => SetValue(SelectedTxt2ImgModelProperty, value); }

        public static readonly BindableProperty SelectedImg2ImgModelProperty = BindableProperty.Create(nameof(SelectedImg2ImgModel), typeof(string), typeof(ImageGenerationSettingsData), "dall-e-2");
        public string SelectedImg2ImgModel { get => (string)GetValue(SelectedImg2ImgModelProperty); set => SetValue(SelectedImg2ImgModelProperty, value); }

        public static readonly BindableProperty SelectedImg2ImgModelIndexProperty = BindableProperty.Create(nameof(SelectedImg2ImgModelIndex), typeof(int), typeof(ImageGenerationSettingsData), int.MinValue);
        public int SelectedImg2ImgModelIndex { get => (int)GetValue(SelectedImg2ImgModelIndexProperty); set => SetValue(SelectedImg2ImgModelIndexProperty, value); }

        public static readonly BindableProperty SelectedTxt2VideoModelProperty = BindableProperty.Create(nameof(SelectedTxt2VideoModel), typeof(string), typeof(ImageGenerationSettingsData), "dall-e-2");
        public string SelectedTxt2VideoModel { get => (string)GetValue(SelectedTxt2VideoModelProperty); set => SetValue(SelectedTxt2VideoModelProperty, value); }

        public static readonly BindableProperty SelectedTxt2VideoModelIndexProperty = BindableProperty.Create(nameof(SelectedTxt2VideoModelIndex), typeof(int), typeof(ImageGenerationSettingsData), int.MinValue);
        public int SelectedTxt2VideoModelIndex { get => (int)GetValue(SelectedTxt2VideoModelIndexProperty); set => SetValue(SelectedTxt2VideoModelIndexProperty, value); }

        public static readonly BindableProperty SelectedImg2VideoModelProperty = BindableProperty.Create(nameof(SelectedImg2VideoModel), typeof(string), typeof(ImageGenerationSettingsData), "dall-e-2");
        public string SelectedImg2VideoModel { get => (string)GetValue(SelectedImg2VideoModelProperty); set => SetValue(SelectedImg2VideoModelProperty, value); }

        public static readonly BindableProperty SelectedImg2VideoModelIndexProperty = BindableProperty.Create(nameof(SelectedImg2VideoModelIndex), typeof(int), typeof(ImageGenerationSettingsData), int.MinValue);
        public int SelectedImg2VideoModelIndex { get => (int)GetValue(SelectedImg2VideoModelIndexProperty); set => SetValue(SelectedImg2VideoModelIndexProperty, value); }

        public static readonly BindableProperty PromptProperty = BindableProperty.Create(nameof(Prompt), typeof(string), typeof(ImageGenerationSettingsData), string.Empty);
        public string Prompt { get => (string)GetValue(PromptProperty); set => SetValue(PromptProperty, value); }

        public static readonly BindableProperty NegativePromptProperty = BindableProperty.Create(nameof(NegativePrompt), typeof(string), typeof(ImageGenerationSettingsData), string.Empty);
        public string NegativePrompt { get => (string)GetValue(NegativePromptProperty); set => SetValue(NegativePromptProperty, value); }

        public static readonly BindableProperty SamplingMethodProperty = BindableProperty.Create(nameof(SamplingMethod), typeof(string), typeof(ImageGenerationSettingsData), "DPM++ 2M Karras");
        public string SamplingMethod { get => (string)GetValue(SamplingMethodProperty); set => SetValue(SamplingMethodProperty, value); }

        public static readonly BindableProperty SamplingStepsProperty = BindableProperty.Create(nameof(SamplingSteps), typeof(int), typeof(ImageGenerationSettingsData), 20);
        public int SamplingSteps { get => (int)GetValue(SamplingStepsProperty); set => SetValue(SamplingStepsProperty, value); }

        public static readonly BindableProperty CFGScaleProperty = BindableProperty.Create(nameof(CFGScale), typeof(int), typeof(ImageGenerationSettingsData), 12);
        public int CFGScale { get => (int)GetValue(CFGScaleProperty); set => SetValue(CFGScaleProperty, value); }

        public static readonly BindableProperty ImageHeightProperty = BindableProperty.Create(nameof(ImageHeight), typeof(int), typeof(ImageGenerationSettingsData), 512);
        public int ImageHeight { get => (int)GetValue(ImageHeightProperty); set => SetValue(ImageHeightProperty, value); }

        public static readonly BindableProperty ImageWidthProperty = BindableProperty.Create(nameof(ImageWidth), typeof(int), typeof(ImageGenerationSettingsData), 512);
        public int ImageWidth { get => (int)GetValue(ImageWidthProperty); set => SetValue(ImageWidthProperty, value); }

        public static readonly BindableProperty BatchCountProperty = BindableProperty.Create(nameof(BatchCount), typeof(int), typeof(ImageGenerationSettingsData), 1);
        public int BatchCount { get => (int)GetValue(BatchCountProperty); set => SetValue(BatchCountProperty, value); }

        public static readonly BindableProperty BatchSizeProperty = BindableProperty.Create(nameof(BatchSize), typeof(int), typeof(ImageGenerationSettingsData), 1);
        public int BatchSize { get => (int)GetValue(BatchSizeProperty); set => SetValue(BatchSizeProperty, value); }

        public static readonly BindableProperty SeedProperty = BindableProperty.Create(nameof(Seed), typeof(int), typeof(ImageGenerationSettingsData), -1);
        public int Seed { get => (int)GetValue(SeedProperty); set => SetValue(SeedProperty, value); }

        public static readonly BindableProperty TilingEnabledProperty = BindableProperty.Create(nameof(TilingEnabled), typeof(bool), typeof(ImageGenerationSettingsData), false);
        public bool TilingEnabled { get => (bool)GetValue(TilingEnabledProperty); set => SetValue(TilingEnabledProperty, value); }

        public static readonly BindableProperty RestoreFacesEnabledProperty = BindableProperty.Create(nameof(RestoreFacesEnabled), typeof(bool), typeof(ImageGenerationSettingsData), false);
        public bool RestoreFacesEnabled { get => (bool)GetValue(RestoreFacesEnabledProperty); set => SetValue(RestoreFacesEnabledProperty, value); }

        public static readonly BindableProperty HiResFixEnabledProperty = BindableProperty.Create(nameof(HiResFixEnabled), typeof(bool), typeof(ImageGenerationSettingsData), false);
        public bool HiResFixEnabled { get => (bool)GetValue(HiResFixEnabledProperty); set => SetValue(HiResFixEnabledProperty, value); }

        public static readonly BindableProperty IsPortraitEnabledProperty = BindableProperty.Create(nameof(IsPortrait), typeof(bool), typeof(ImageGenerationSettingsData), true);
        public bool IsPortrait { get => (bool)GetValue(IsPortraitEnabledProperty); set { if (value != IsPortrait) { SetValue(IsPortraitEnabledProperty, value); OnPropertyChanged(nameof(IsPortrait)); if (value) { IsLandscape = false; } } } }

        public static readonly BindableProperty IsLandscapeProperty = BindableProperty.Create(nameof(IsLandscape), typeof(bool), typeof(ImageGenerationSettingsData), false);
        public bool IsLandscape { get => (bool)GetValue(IsLandscapeProperty); set { if (value != IsLandscape) { SetValue(IsLandscapeProperty, value); OnPropertyChanged(nameof(IsLandscape)); if (value) { IsPortrait = false; } } } }

        public static readonly BindableProperty ImageUrlProperty = BindableProperty.Create(nameof(ImageUrl), typeof(string), typeof(ImageGenerationSettingsData), string.Empty);
        public string ImageUrl { get => (string)GetValue(ImageUrlProperty); set => SetValue(ImageUrlProperty, value); }

        public static readonly BindableProperty ImageViewSourceProperty = BindableProperty.Create(nameof(ImageViewSource), typeof(ImageSource), typeof(ImageGenerationSettingsData), null);
        public ImageSource? ImageViewSource { get => (ImageSource?)GetValue(ImageViewSourceProperty); set => SetValue(ImageViewSourceProperty, value); }

        public static readonly BindableProperty MediaSourceProperty = BindableProperty.Create(nameof(MediaSource), typeof(MediaSource), typeof(ImageGenerationSettingsData), null);
        public MediaSource? MediaSource { get => (MediaSource?)GetValue(MediaSourceProperty); set => SetValue(MediaSourceProperty, value); }

        public static readonly BindableProperty InputImageProperty = BindableProperty.Create(nameof(InputImage), typeof(byte[]), typeof(ImageGenerationSettingsData), null);
        public byte[]? InputImage { get => (byte[]?)GetValue(InputImageProperty); set => SetValue(InputImageProperty, value); }

        public static readonly BindableProperty MaskImageProperty = BindableProperty.Create(nameof(MaskImage), typeof(byte[]), typeof(ImageGenerationSettingsData), null);
        public byte[]? MaskImage { get => (byte[]?)GetValue(MaskImageProperty); set => SetValue(MaskImageProperty, value); }

        public static readonly BindableProperty StylesProperty = BindableProperty.Create(nameof(Styles), typeof(string[]), typeof(ImageGenerationSettingsData), null);
        public string[]? Styles { get => (string[]?)GetValue(StylesProperty); set => SetValue(StylesProperty, value); }

        public static readonly BindableProperty SubseedProperty = BindableProperty.Create(nameof(Subseed), typeof(int), typeof(ImageGenerationSettingsData), -1);
        public int Subseed { get => (int)GetValue(SubseedProperty); set => SetValue(SubseedProperty, value); }

        public static readonly BindableProperty SubseedStrengthProperty = BindableProperty.Create(nameof(SubseedStrength), typeof(int), typeof(ImageGenerationSettingsData), 0);
        public int SubseedStrength { get => (int)GetValue(SubseedStrengthProperty); set => SetValue(SubseedStrengthProperty, value); }

        public static readonly BindableProperty SeedResizeFromHProperty = BindableProperty.Create(nameof(SeedResizeFromH), typeof(int), typeof(ImageGenerationSettingsData), -1);
        public int SeedResizeFromH { get => (int)GetValue(SeedResizeFromHProperty); set => SetValue(SeedResizeFromHProperty, value); }

        public static readonly BindableProperty SeedResizeFromWProperty = BindableProperty.Create(nameof(SeedResizeFromW), typeof(int), typeof(ImageGenerationSettingsData), -1);
        public int SeedResizeFromW { get => (int)GetValue(SeedResizeFromWProperty); set => SetValue(SeedResizeFromWProperty, value); }

        public static readonly BindableProperty DenoisingStrengthProperty = BindableProperty.Create(nameof(DenoisingStrength), typeof(double), typeof(ImageGenerationSettingsData), 0.75);
        public double DenoisingStrength { get => (double)GetValue(DenoisingStrengthProperty); set => SetValue(DenoisingStrengthProperty, value); }

        public static readonly BindableProperty SMinUncondProperty = BindableProperty.Create(nameof(SMinUncond), typeof(int), typeof(ImageGenerationSettingsData), 0);
        public int SMinUncond { get => (int)GetValue(SMinUncondProperty); set => SetValue(SMinUncondProperty, value); }

        public static readonly BindableProperty SChurnProperty = BindableProperty.Create(nameof(SChurn), typeof(int), typeof(ImageGenerationSettingsData), 0);
        public int SChurn { get => (int)GetValue(SChurnProperty); set => SetValue(SChurnProperty, value); }

        public static readonly BindableProperty STmaxProperty = BindableProperty.Create(nameof(STmax), typeof(int), typeof(ImageGenerationSettingsData), 0);
        public int STmax { get => (int)GetValue(STmaxProperty); set => SetValue(STmaxProperty, value); }

        public static readonly BindableProperty STminProperty = BindableProperty.Create(nameof(STmin), typeof(int), typeof(ImageGenerationSettingsData), 0);
        public int STmin { get => (int)GetValue(STminProperty); set => SetValue(STminProperty, value); }

        public static readonly BindableProperty SNoiseProperty = BindableProperty.Create(nameof(SigmaNoise), typeof(int), typeof(ImageGenerationSettingsData), 0);
        public int SigmaNoise { get => (int)GetValue(SNoiseProperty); set => SetValue(SNoiseProperty, value); }

        public static readonly BindableProperty OverrideSettingsProperty = BindableProperty.Create(nameof(OverrideSettings), typeof(object), typeof(ImageGenerationSettingsData), null);
        public object? OverrideSettings { get => GetValue(OverrideSettingsProperty); set => SetValue(OverrideSettingsProperty, value); }

        public static readonly BindableProperty RefinerSwitchAtProperty = BindableProperty.Create(nameof(RefinerSwitchAt), typeof(int), typeof(ImageGenerationSettingsData), 0);
        public int RefinerSwitchAt { get => (int)GetValue(RefinerSwitchAtProperty); set => SetValue(RefinerSwitchAtProperty, value); }

        public static readonly BindableProperty DisableExtraNetworksProperty = BindableProperty.Create(nameof(DisableExtraNetworks), typeof(bool), typeof(ImageGenerationSettingsData), false);
        public bool DisableExtraNetworks { get => (bool)GetValue(DisableExtraNetworksProperty); set => SetValue(DisableExtraNetworksProperty, value); }

        public static readonly BindableProperty InitImagesProperty = BindableProperty.Create(nameof(InitImages), typeof(string[]), typeof(ImageGenerationSettingsData), null);
        public string[]? InitImages { get => (string[]?)GetValue(InitImagesProperty); set => SetValue(InitImagesProperty, value); }

        public static readonly BindableProperty ResizeModeProperty = BindableProperty.Create(nameof(ResizeMode), typeof(int), typeof(ImageGenerationSettingsData), 0);
        public int ResizeMode { get => (int)GetValue(ResizeModeProperty); set => SetValue(ResizeModeProperty, value); }

        public static readonly BindableProperty ImageCfgScaleProperty = BindableProperty.Create(nameof(ImgCFGScale), typeof(int), typeof(ImageGenerationSettingsData), 0);
        public int ImgCFGScale { get => (int)GetValue(ImageCfgScaleProperty); set => SetValue(ImageCfgScaleProperty, value); }

        public static readonly BindableProperty MaskBlurXProperty = BindableProperty.Create(nameof(MaskBlurX), typeof(int), typeof(ImageGenerationSettingsData), 4);
        public int MaskBlurX { get => (int)GetValue(MaskBlurXProperty); set => SetValue(MaskBlurXProperty, value); }

        public static readonly BindableProperty MaskBlurYProperty = BindableProperty.Create(nameof(MaskBlurY), typeof(int), typeof(ImageGenerationSettingsData), 4);
        public int MaskBlurY { get => (int)GetValue(MaskBlurYProperty); set => SetValue(MaskBlurYProperty, value); }

        public static readonly BindableProperty MaskBlurProperty = BindableProperty.Create(nameof(MaskBlur), typeof(int), typeof(ImageGenerationSettingsData), 0);
        public int MaskBlur { get => (int)GetValue(MaskBlurProperty); set => SetValue(MaskBlurProperty, value); }

        public static readonly BindableProperty InpaintingFillProperty = BindableProperty.Create(nameof(InpaintingFill), typeof(int), typeof(ImageGenerationSettingsData), 0);
        public int InpaintingFill { get => (int)GetValue(InpaintingFillProperty); set => SetValue(InpaintingFillProperty, value); }

        public static readonly BindableProperty InpaintFullResProperty = BindableProperty.Create(nameof(InpaintFullRes), typeof(bool), typeof(ImageGenerationSettingsData), true);
        public bool InpaintFullRes { get => (bool)GetValue(InpaintFullResProperty); set => SetValue(InpaintFullResProperty, value); }

        public static readonly BindableProperty InpaintingFullResPaddingProperty = BindableProperty.Create(nameof(InpaintingFullResPadding), typeof(int), typeof(ImageGenerationSettingsData), 0);
        public int InpaintingFullResPadding { get => (int)GetValue(InpaintingFullResPaddingProperty); set => SetValue(InpaintingFullResPaddingProperty, value); }

        public static readonly BindableProperty InpaintingMaskInvertProperty = BindableProperty.Create(nameof(InpaintingMaskInvert), typeof(int), typeof(ImageGenerationSettingsData), 0);
        public int InpaintingMaskInvert { get => (int)GetValue(InpaintingMaskInvertProperty); set => SetValue(InpaintingMaskInvertProperty, value); }

        public static readonly BindableProperty InitialNoiseMultiplierProperty = BindableProperty.Create(nameof(InitialNoiseMultiplier), typeof(int), typeof(ImageGenerationSettingsData), 0);
        public int InitialNoiseMultiplier { get => (int)GetValue(InitialNoiseMultiplierProperty); set => SetValue(InitialNoiseMultiplierProperty, value); }

        public static readonly BindableProperty LatentMaskProperty = BindableProperty.Create(nameof(LatentMask), typeof(string), typeof(ImageGenerationSettingsData), string.Empty);
        public string LatentMask { get => (string)GetValue(LatentMaskProperty); set => SetValue(LatentMaskProperty, value); }

        public static readonly BindableProperty SamplerIndexProperty = BindableProperty.Create(nameof(SamplerIndex), typeof(string), typeof(ImageGenerationSettingsData), "Euler");
        public string SamplerIndex { get => (string)GetValue(SamplerIndexProperty); set => SetValue(SamplerIndexProperty, value); }

        public static readonly BindableProperty IncludeInitImagesProperty = BindableProperty.Create(nameof(IncludeInitImages), typeof(bool), typeof(ImageGenerationSettingsData), false);
        public bool IncludeInitImages { get => (bool)GetValue(IncludeInitImagesProperty); set => SetValue(IncludeInitImagesProperty, value); }

        public static readonly BindableProperty ScriptNameProperty = BindableProperty.Create(nameof(ScriptName), typeof(string), typeof(ImageGenerationSettingsData), string.Empty);
        public string ScriptName { get => (string)GetValue(ScriptNameProperty); set => SetValue(ScriptNameProperty, value); }

        public static readonly BindableProperty ScriptArgsProperty = BindableProperty.Create(nameof(ScriptArgs), typeof(string[]), typeof(ImageGenerationSettingsData), null);
        public string[]? ScriptArgs { get => (string[]?)GetValue(ScriptArgsProperty); set => SetValue(ScriptArgsProperty, value); }

        public static readonly BindableProperty SendImagesProperty = BindableProperty.Create(nameof(SendImages), typeof(bool), typeof(ImageGenerationSettingsData), true);
        public bool SendImages { get => (bool)GetValue(SendImagesProperty); set => SetValue(SendImagesProperty, value); }

        public static readonly BindableProperty SaveImagesProperty = BindableProperty.Create(nameof(SaveImages), typeof(bool), typeof(ImageGenerationSettingsData), false);
        public bool SaveImages { get => (bool)GetValue(SaveImagesProperty); set => SetValue(SaveImagesProperty, value); }

        public static readonly BindableProperty OverrideSettingsRestoreAfterwardsProperty = BindableProperty.Create(nameof(OverrideSettingsRestoreAfterwards), typeof(bool), typeof(ImageGenerationSettingsData), false);
        public bool OverrideSettingsRestoreAfterwards { get => (bool)GetValue(OverrideSettingsRestoreAfterwardsProperty); set => SetValue(OverrideSettingsRestoreAfterwardsProperty, value); }

        public static readonly BindableProperty InvertMaskProperty = BindableProperty.Create(nameof(InvertMask), typeof(bool), typeof(ImageGenerationSettingsData), false);
        public bool InvertMask { get => (bool)GetValue(InvertMaskProperty); set => SetValue(InvertMaskProperty, value); }

        public static readonly BindableProperty PlayerMediaSourceProperty = BindableProperty.Create(nameof(PlayerMediaSource), typeof(MediaSource), typeof(AudioGenerationSettingsData), null);
        public MediaSource PlayerMediaSource { get => (MediaSource)GetValue(PlayerMediaSourceProperty); set => SetValue(PlayerMediaSourceProperty, value); }

        public static readonly BindableProperty CurImageBytesProperty = BindableProperty.Create(nameof(CurImageBytes), typeof(byte[]), typeof(AudioGenerationSettingsData), null);
        public byte[]? CurImageBytes { get => (byte[]?)GetValue(CurImageBytesProperty); set => SetValue(CurImageBytesProperty, value); }

    }
}