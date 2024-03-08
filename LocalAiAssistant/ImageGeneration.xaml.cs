// MIT License
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Graphics;
using Microsoft.VisualBasic.FileIO;
using System.Net.Http.Json;
using Microsoft.Maui.Graphics.Converters;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using LocalAiAssistant.Utilities;
using System.Net;
using System.Text.Json.Nodes;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Maui;
using System.IO;

// Apache License
#if WINDOWS
using System.Speech.Synthesis;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
#elif ANDROID
using Android.Content;
using Android.Graphics;
using Android.Content.Res;
using Microsoft.Maui.Graphics.Platform;
using Android.App;
using Android.Views;
using Android.Widget;
#endif

namespace LocalAiAssistant
{
    public partial class ImageGeneration : ContentPage
    {
        #region Init/Events
        public ImageGenerationSettingsData UiData = new();
        private GeneralSettingsData defaultData = new();
        public ImageGeneration()
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
            if (MyMultiPlatformUtils.CheckPreferenceContains(ImageGenerationSettings.StableDiffusionPreference))
            {
                ImageGenerationSettingsData? saveData = await MyMultiPlatformUtils.ReadFromPreferences<ImageGenerationSettingsData>(ImageGenerationSettings.StableDiffusionPreference);
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
                    UiData.Prompt = saveData.Prompt;
                    UiData.NegativePrompt = saveData.NegativePrompt;
                    UiData.SelectedTxt2ImgModel = saveData.SelectedTxt2ImgModel;
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
                    UiData.ModelList = saveData.ModelList;
                    UiData.SelectedTxt2ImgModelIndex = saveData.SelectedTxt2ImgModelIndex;
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
                        UiData.SelectedImg2ImgModel = saveData.ModelList[saveData.SelectedTxt2VideoModelIndex];
                    }
                    UiData.SelectedImg2VideoModelIndex = saveData.SelectedImg2VideoModelIndex;
                    if (UiData.ModelList.Count > 1 && UiData.SelectedImg2VideoModelIndex > 0 && UiData.SelectedImg2VideoModelIndex < UiData.ModelList.Count)
                    {
                        UiData.SelectedImg2ImgModel = saveData.ModelList[saveData.SelectedImg2VideoModelIndex];
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
        #endregion
        #region Button Events/Animations
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
                await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Error", $"Failed to generate image: {ex.Message}");
            }
        }
        private async void OnSaveImageBtnClicked(object? sender, EventArgs e)
        {
            SemanticScreenReader.Announce(SaveImageBtn.Text);
            if (ImageOutputView.Source != null)
            {
                byte[]? imageBytes = UiData.CurImageBytes;//await MyMultiPlatformUtils.ConvertImageSourceToByteArray(ImageOutputView.Source);
                string folderPath = await MyMultiPlatformUtils.PickFolderAsync();
                if (MyMultiPlatformUtils.IsFolderWritable(folderPath) && imageBytes != null)
                {
                    string fileName = await MyMultiPlatformUtils.WriteToFileAsync($"{folderPath}/output.png", imageBytes);
                    await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Image Saved", $"Saved to: {System.IO.Path.Combine(folderPath,fileName)}");
                }
            }
        }
        private async void OnGenerateBtnClicked(object? sender, EventArgs e)
        {
            SemanticScreenReader.Announce(GenerateBtn.Text);
            await GenerateImageStableDiffusion();
        }
        private async Task GenerateImageStableDiffusion()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(UiData.NegativePrompt))
                {
                    UiData.NegativePrompt = MyAIAPI.defaultShortNegativePrompt;
                }
                switch (UiData.ServerMode)
                {
                    case ImageGenerationSettingsData.ServerModes.LocalAi:
                        {
                            string imageUrl = string.Empty;
                            if (UiData.InputImage != null)
                            {
                                // Img2Img
                                string b64Image = Convert.ToBase64String(UiData.InputImage);
                                UiData.InputImage = null;
                                imageUrl = await MyAIAPI.GenerateLocalAiImageAsyncHttp(prompt: $"{UiData.Prompt} | {UiData.NegativePrompt}", image: b64Image, size: $"{UiData.ImageWidth}x{UiData.ImageHeight}", model: UiData.SelectedTxt2ImgModel, step: UiData.SamplingSteps, timeoutInSeconds: (int)UiData.TimeOutDelay, serverUrl: UiData.ServerUrlInput, apiKey: UiData.ApiKey, authEnabled: UiData.AuthEnabled);
                            }
                            else
                            {
                                imageUrl = await MyAIAPI.GenerateLocalAiImageAsyncHttp(prompt: $"{UiData.Prompt} | {UiData.NegativePrompt}", size: $"{UiData.ImageWidth}x{UiData.ImageHeight}", model: UiData.SelectedTxt2ImgModel, step: UiData.SamplingSteps, timeoutInSeconds: (int)UiData.TimeOutDelay, serverUrl: UiData.ServerUrlInput, apiKey: UiData.ApiKey, authEnabled: UiData.AuthEnabled);
                            }
                            UiData.CurImageBytes = await MyMultiPlatformUtils.GetAsByteArray(imageUrl);
                            break;
                        }
                    case ImageGenerationSettingsData.ServerModes.Automatic1111:
                        {
                            if (UiData.InputImage != null)
                            {
                                string imageBase64 = Convert.ToBase64String(UiData.InputImage);
                                UiData.InputImage = null;
                                List<byte[]> imagesGenerated = await MyAIAPI.GenerateImage2ImageStableDiffusionAsync(prompt: UiData.Prompt, negativePrompt: UiData.NegativePrompt, model: UiData.SelectedTxt2ImgModel, samplerName: UiData.SamplingMethod,
                                steps: UiData.SamplingSteps, batchCount: UiData.BatchCount, batchSize: UiData.BatchSize, seed: UiData.Seed, height: UiData.ImageHeight, width: UiData.ImageWidth, cfgScale: UiData.CFGScale,
                                inputImage: imageBase64, restoreFaces: UiData.RestoreFacesEnabled, tiling: UiData.TilingEnabled, serverUrl: UiData.ServerUrlInput);
                                UiData.CurImageBytes = imagesGenerated.FirstOrDefault();
                            }
                            else
                            {
                                List<byte[]> imagesGenerated = await MyAIAPI.GenerateImageStableDiffusionAsync(prompt: UiData.Prompt, negativePrompt: UiData.NegativePrompt, model: UiData.SelectedTxt2ImgModel, samplerName: UiData.SamplingMethod,
                                steps: UiData.SamplingSteps, batchCount: UiData.BatchCount, batchSize: UiData.BatchSize, seed: UiData.Seed, height: UiData.ImageHeight, width: UiData.ImageWidth, cfgScale: UiData.CFGScale,
                                enableHr: UiData.HiResFixEnabled, restoreFaces: UiData.RestoreFacesEnabled, tiling: UiData.TilingEnabled, serverUrl: UiData.ServerUrlInput);
                                UiData.CurImageBytes = imagesGenerated.FirstOrDefault();
                            }
                            break;
                        }
                    case ImageGenerationSettingsData.ServerModes.OpenAi:
                        {
                            if (UiData.InputImage != null)
                            {
                                string imageBase64 = Convert.ToBase64String(UiData.InputImage);
                                UiData.InputImage = null;
                                //string imageUrl = await MyAIAPI.EditImageAsyncHttp(prompt: $"{UiData.Prompt}", apiKey: UiData.ApiKey, size: $"{UiData.ImageWidth}x{UiData.ImageHeight}", model: UiData.SelectedTxt2ImgModel, responseFormat: "url", timeoutInSeconds: 30, serverUrl: UiData.ServerUrlInput, authEnabled: UiData.AuthEnabled);
                                //CurImageBytes = await MyMultiPlatformUtils.GetAsByteArray(imageUrl);
                            }
                            else
                            {
                                string imageUrl = await MyAIAPI.GenerateImageAsyncHttp(prompt: $"{UiData.Prompt}", apiKey: UiData.ApiKey, size: $"{UiData.ImageWidth}x{UiData.ImageHeight}", model: UiData.SelectedTxt2ImgModel, responseFormat: "url", timeoutInSeconds: 30, serverUrl: UiData.ServerUrlInput, authEnabled: UiData.AuthEnabled);
                                UiData.CurImageBytes = await MyMultiPlatformUtils.GetAsByteArray(imageUrl);
                            }
                            break;
                        }
                } // Process Appropriately
                if (UiData.CurImageBytes != null)
                {
                    ImageSource? imageSource = MyMultiPlatformUtils.ConvertByteArrayToImageSource(UiData.CurImageBytes);
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