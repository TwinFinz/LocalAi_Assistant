using LocalAiAssistant.Utilities;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Input;
using LocalAiAssistant;
using CommunityToolkit.Maui.Converters;
using System.Threading.Tasks;

namespace LocalAiAssistant
{
    public partial class StableDiffusionImg2Img : ContentPage
    {
        #region Init/Events
        public ImageGenerationSettingsData UiData = new();
        private byte[]? CurImageBytes;
        public StableDiffusionImg2Img()
        {
            InitializeComponent();
            BindingContext = UiData;
            this.Loaded += StableDiffusionLoaded;
            SizeChanged += OnPageSizeChanged;
        }
        private async void StableDiffusionLoaded(object? sender, EventArgs e)
        {
            await LoadData();
        }
        private async Task LoadData()
        {
            if (MyMultiPlatformUtils.CheckPreferenceContains(ImageGenerationSettings.StableDiffusionPreference))
            {
                ImageGenerationSettingsData? saveData = await MyMultiPlatformUtils.ReadFromPreferences<ImageGenerationSettingsData>(ImageGenerationSettings.StableDiffusionPreference);
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
                    UiData.SelectedImg2ImgModelIndex = saveData.SelectedImg2ImgModelIndex;
                    if (UiData.ServerModeNames.Count > 1 && saveData.SelectedServerModeIndex < UiData.ServerModeNames.Count)
                    {
                        UiData.ServerMode = (ImageGenerationSettingsData.ServerModes)saveData.SelectedServerModeIndex;
                        UiData.SelectedServerModeIndex = saveData.SelectedServerModeIndex;
                    }
                    if (UiData.ServerMode == ImageGenerationSettingsData.ServerModes.Automatic1111)
                    {
                        Automatic1111Switches.IsVisible = true;
                    }
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
        #endregion
        #region Button Events/Animations
        private async void OnSaveImageBtnClicked(object? sender, EventArgs e)
        {
            SemanticScreenReader.Announce(SaveImageBtn.Text);
            if (UiData.ImageViewSource != null)
            {
                byte[] imageBytes = await MyMultiPlatformUtils.ConvertImageSourceToByteArray(UiData.ImageViewSource);
                string folderPath = await MyMultiPlatformUtils.PickFolderAsync();
                if (MyMultiPlatformUtils.IsFolderWritable(folderPath))
                {
                    await MyMultiPlatformUtils.WriteToFileAsync($"{folderPath}/output.png", imageBytes);
                }
            }
        }
        private async void OnGenerateBtnClicked(object? sender, EventArgs e)
        {
            SemanticScreenReader.Announce(GenerateBtn.Text);
            await GenerateImageStableDiffusion();
        }
        private async void OnSelectBtnClicked(object? sender, EventArgs e)
        {
            SemanticScreenReader.Announce(GenerateBtn.Text);
            try
            {
                UiData.InputImage = await MyMultiPlatformUtils.PickImageAsync() ?? throw new Exception("Image failed to load, returned null.");
                //await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Select Mask", $"If you want to use a mask select it now.");
                //UiData.MaskImage = await MyMultiPlatformUtils.PickImageAsync() ?? MyMultiPlatformUtils.CreateBlankPngAsync(UiData.ImageWidth, UiData.ImageHeight);
            }
            catch (Exception ex)
            {
                await MyMultiPlatformUtils.MessageBoxWithOK(Application.Current!, "Error", $"Failed to generate image: {ex.Message}");
            }
        }
        private async Task GenerateImageStableDiffusion()
        {
            try
            {
                if (UiData.InputImage == null)
                {
                    await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Warning", "Please select an image or use Text2Image");
                    return;
                }
                if (string.IsNullOrWhiteSpace(UiData.NegativePrompt))
                {
                    UiData.NegativePrompt = MyAIAPI.defaultShortNegativePrompt;
                }
                string imageUrl = string.Empty;
                switch (UiData.ServerMode)
                {
                    case ImageGenerationSettingsData.ServerModes.LocalAi:
                        {
                            // LocalAI Image2Image
                            if (UiData.MaskImage != null)
                            {
                                int invert = (UiData.InvertMask) ? 1 : 0;
                            }
                            string imageBase64 = Convert.ToBase64String(UiData.InputImage);
                            string imageUri = $"data:image/jpeg;base64,{imageBase64}";
                            imageUrl = await MyAIAPI.GenerateLocalAiImageAsyncHttp(prompt: $"{UiData.Prompt} | {UiData.NegativePrompt}",  apiKey: UiData.ApiKey, image: imageUri, model: UiData.SelectedImg2ImgModel, size: $"{UiData.ImageWidth}x{UiData.ImageHeight}", timeoutInSeconds: 30, serverUrl: UiData.ServerUrlInput, authEnabled: UiData.AuthEnabled);
                            if (!string.IsNullOrWhiteSpace(imageUrl))
                            {
                                CurImageBytes = await MyMultiPlatformUtils.GetAsByteArray(imageUrl);
                            }
                            break;
                        }
                    case ImageGenerationSettingsData.ServerModes.Automatic1111:
                        {
                            // Automatic1111 Image2Image
                            if (!string.IsNullOrWhiteSpace(imageUrl))
                            {
                                CurImageBytes = await MyMultiPlatformUtils.GetAsByteArray(imageUrl);
                            }
                            break;
                        }
                    case ImageGenerationSettingsData.ServerModes.OpenAi:
                        {
                            if (UiData.MaskImage != null)
                            {
                                int invert = (UiData.InvertMask) ? 1 : 0;
                                imageUrl = await MyAIAPI.EditImageAsyncHttp(prompt: $"{UiData.Prompt} | {UiData.NegativePrompt}", image: UiData.InputImage, mask: UiData.MaskImage, apiKey: UiData.ApiKey, size: $"{UiData.ImageWidth}x{UiData.ImageHeight}", timeoutInSeconds: 30, serverUrl: UiData.ServerUrlInput, authEnabled: UiData.AuthEnabled);
                            }
                            if (!string.IsNullOrWhiteSpace(imageUrl))
                            {
                                CurImageBytes = await MyMultiPlatformUtils.GetAsByteArray(imageUrl);
                            }
                            break;
                        }
                } // Process Appropriately
                if (CurImageBytes != null)
                {
                    ImageSource? imageSource = MyMultiPlatformUtils.ConvertByteArrayToImageSource(CurImageBytes);
                    if (imageSource != null)
                    {
                        UiData.ImageViewSource = imageSource;
                        ImageOutputView.MaximumHeightRequest = UiData.ImageHeight;
                        ImageOutputView.MaximumWidthRequest = UiData.ImageWidth;
                        ImageOutput.IsVisible = true;
                    }
                }
                else
                {
                    throw new Exception("Image generation returned: Null");
                }
            }
            catch (Exception ex)
            {
                await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Error", $"Failed to generate image: {ex.Message}");
            }
        }
        #endregion
    }
}