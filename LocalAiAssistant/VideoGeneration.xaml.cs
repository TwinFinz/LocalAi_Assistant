using LocalAiAssistant.Utilities;

namespace LocalAiAssistant;

public partial class VideoGeneration : ContentPage
{
    public VideoGeneration()
    {
        InitializeComponent();
        BindingContext = UiData;
        this.Loaded += Txt2VideoLoaded;
        SizeChanged += OnPageSizeChanged;
    }
    #region Init/Events
    public ImageGenerationSettingsData UiData = new();
    private byte[]? curVideoBytes;
    private async void Txt2VideoLoaded(object? sender, EventArgs e)
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
        if (UiData.PlayerMediaSource != null)
        {
            byte[] videoBytes = await MyMultiPlatformUtils.ConvertMediaSourceToByteArray(UiData.PlayerMediaSource);
            string folderPath = await MyMultiPlatformUtils.PickFolderAsync();
            if (MyMultiPlatformUtils.IsFolderWritable(folderPath))
            {
                string baseFilePath = $"{folderPath}/output.mp4";
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
                await MyMultiPlatformUtils.WriteToFileAsync(filePath, videoBytes);
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
            //if (UiData.InputImage == null)
            //{
            //    await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Warning", "Please select an image or use Text2Image");
            //    return;
            //}
            //if (UiData.MaskImage != null)
            //{
            //    int invert = (UiData.InvertMask) ? 1 : 0;
            //}

            if (string.IsNullOrWhiteSpace(UiData.NegativePrompt))
            {
                UiData.NegativePrompt = MyAIAPI.defaultShortNegativePrompt;
            }
            string imageUrl = string.Empty;
            switch (UiData.ServerMode)
            {
                case ImageGenerationSettingsData.ServerModes.LocalAi:
                    {
                        // LocalAI Text2Video
                        //string imageBase64 = Convert.ToBase64String(UiData.InputImage);
                        //string imageUri = $"data:image/jpeg;base64,{imageBase64}";
                        imageUrl = await MyAIAPI.GenerateLocalAiText2VideoAsyncHttp(prompt: $"{UiData.Prompt} | {UiData.NegativePrompt}", apiKey: UiData.ApiKey, model: UiData.SelectedTxt2VideoModel, size: $"{UiData.ImageWidth}x{UiData.ImageHeight}", timeoutInSeconds: (int)UiData.TimeOutDelay, serverUrl: UiData.ServerUrlInput, authEnabled: UiData.AuthEnabled);
                        if (!string.IsNullOrWhiteSpace(imageUrl))
                        {
                            curVideoBytes = await MyMultiPlatformUtils.GetAsByteArray(imageUrl);
                        }
                        break;
                    }
                case ImageGenerationSettingsData.ServerModes.Automatic1111:
                    {
                        // Automatic1111 Text2Video
                        if (!string.IsNullOrWhiteSpace(imageUrl))
                        {
                            curVideoBytes = await MyMultiPlatformUtils.GetAsByteArray(imageUrl);
                        }
                        break;
                    }
                case ImageGenerationSettingsData.ServerModes.OpenAi:
                    {
                        imageUrl = "";//await MyAIAPI.EditImageAsyncHttp(prompt: $"{UiData.Prompt} | {UiData.NegativePrompt}", apiKey: UiData.ApiKey, size: $"{UiData.ImageWidth}x{UiData.ImageHeight}", timeoutInSeconds: 30, serverUrl: UiData.ServerUrlInput, authEnabled: UiData.AuthEnabled);
                        if (!string.IsNullOrWhiteSpace(imageUrl))
                        {
                            curVideoBytes = await MyMultiPlatformUtils.GetAsByteArray(imageUrl);
                        }
                        break;
                    }
            } // Process Appropriately
            if (curVideoBytes != null)
            {
                ImageSource? imageSource = MyMultiPlatformUtils.ConvertByteArrayToImageSource(curVideoBytes);
                if (imageSource != null)
                {
                    UiData.ImageViewSource = imageSource;
                    MediaPlayer.MaximumHeightRequest = UiData.ImageHeight;
                    MediaPlayer.MaximumWidthRequest = UiData.ImageWidth;
                    MediaOutput.IsVisible = true;
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