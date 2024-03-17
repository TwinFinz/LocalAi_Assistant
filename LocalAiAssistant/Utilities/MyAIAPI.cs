using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Data.SqlClient;
using System.Net.Http.Json;
using System.IO;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using Microsoft.Maui.Storage;

namespace LocalAiAssistant
{
    public class MyAIAPI
    {
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0300 // Simplify collection initialization
#pragma warning disable IDE0301 // Simplify collection initialization
#pragma warning disable IDE0305 // Simplify collection initialization
#pragma warning disable IDE0600 // Simplify collection initialization
#pragma warning disable IDE0601 // Simplify collection initialization
#pragma warning disable IDE0605 // Simplify collection initialization
        public MyAIAPI()
        {
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        #region StableDiffusion
        internal static string defaultNegativePrompt = "face, worst quality, normal quality, low quality, low res, blurry, text, watermark," +
         "logo, banner, extra digits, cropped, jpeg artifacts, signature, username, error, sketch, duplicate, ugly, monochrome, horror, geometry, mutation," +
         "disgusting, bad anatomy, bad hands, three hands, three legs, bad arms, missing legs, missing arms, poorly drawn face, bad face, fused face, cloned face," +
         "worst face, three crus, extra crus, fused crus, worst feet, three feet, fused feet, fused thigh, three thigh, fused thigh, extra thigh, worst thigh," +
         "missing fingers, extra fingers, ugly fingers, long fingers, horn, realistic photo, extra eyes, huge eyes, 2girl, amputation, disconnected limbs";

        internal static string defaultShortNegativePrompt = "worst face, missing legs, missing arms, duplicate, ugly";
        /*
        private readonly string samplersEndpoint = "/sdapi/v1/samplers";
        private readonly string optionsEndpoint = "/sdapi/v1/options";
        private readonly string interruptEndpoint = "/sdapi/v1/interrupt";
        private readonly string skipEndpoint = "/sdapi/v1/skip";
        private readonly string img2imgEndpoint = "/sdapi/v1/img2img";
        private readonly string upscalersEndpoint = "/sdapi/v1/upscalers";
        private readonly string modelsEndpoint = "/sdapi/v1/sd-models";
        private readonly string embeddingsEndpoints = "/sdapi/v1/embeddings";
        private readonly string faceRestorersEndpoint = "/sdapi/v1/face-restorers";
        private readonly string Endpoint = "/sdapi/v1/train/embedding";
        private readonly string scriptsEndpoint = "/sdapi/v1/scripts";
        private readonly string scriptInfoEndpoint = "/sdapi/v1/script-info";
        private readonly string infoEndpoint = "/info";
        */

        public static async Task<List<byte[]>> GenerateImageStableDiffusionAsync(string prompt, string negativePrompt = "", string model = "deliberate_v3", string samplerName = "DPM++ 2M Karras", int steps = 20, int cfgScale = 12, int seed = -1, int width = 512, int height = 512, bool enableHr = false, int batchSize = 1, int batchCount = 1, bool restoreFaces = false, bool tiling = false, bool overrideSettingsRestoreAfterwards = true, int timeoutInSeconds = 60, string serverUrl = "http://127.0.0.1:7860")
        {
            string endpoint = $"{serverUrl}/sdapi/v1/txt2img";
            prompt = prompt.Trim();
            if (!string.IsNullOrEmpty(negativePrompt.Trim()))
            {
                defaultNegativePrompt += $", {negativePrompt}";
            }
            StableDiffusionRequest requestPayload = new()
            {
                Prompt = prompt,
                Steps = steps,
                NegativePrompt = defaultNegativePrompt,
                SamplerName = samplerName,
                CfgScale = cfgScale,
                Seed = seed,
                Width = width,
                Height = height,
                EnableHr = enableHr,
                BatchSize = batchSize,
                NumberOfIterations = batchCount,
                RestoreFaces = restoreFaces,
                Tiling = tiling,
                OverrideSettingsRestoreAfterwards = overrideSettingsRestoreAfterwards,
                RefinerCheckpoint = model
            };
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
#if WINDOWS
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
            string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            string jsonRequest = JsonSerializer.Serialize(requestPayload);
            HttpResponseMessage response = await client.PostAsync(endpoint, new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json"));
            if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                string errorResponseBody = await response.Content.ReadAsStringAsync();
                ErrorResponseModel? errorResponse = JsonSerializer.Deserialize<ErrorResponseModel>(errorResponseBody) ?? throw new Exception("Failed to parse error data");
                string errorMsg = "";
                foreach (ErrorDetail error in (IEnumerable<ErrorDetail>)errorResponse)
                {
                    errorMsg += $"{error.Msg} || ";
                }
                throw new Exception(errorMsg);
            }
            else if (!response.IsSuccessStatusCode)
            {
                throw new Exception("API request failed with status code: " + (int)response.StatusCode);
            }
            string responseBody = await response.Content.ReadAsStringAsync();
            SuccessResponseModel? jsonResponse = JsonSerializer.Deserialize<SuccessResponseModel>(responseBody);
            if (jsonResponse != null && jsonResponse.Images != null)
            {
                List<byte[]> bytesToReturn = new();
                Parallel.ForEach(jsonResponse.Images, (item) =>
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        byte[] imageBytes = Convert.FromBase64String(item);
                        if (imageBytes != null)
                        {
                            lock (bytesToReturn)
                            {
                                bytesToReturn.Add(imageBytes);
                            }
                        }
                    }
                });
                return bytesToReturn;
            }
            else
            {
                throw new JsonException("Stable Diffusion Response was malformed.");
            }
        }
        public static async Task<List<byte[]>> GenerateImage2ImageStableDiffusionAsync(string prompt, string inputImage = "", string maskImage = "", string negativePrompt = "", string model = "deliberate_v3", string samplerName = "DPM++ 2M Karras", int steps = 20, int cfgScale = 12, int seed = -1, int width = 512, int height = 512, int batchSize = 1, int batchCount = 1, bool restoreFaces = false, bool tiling = false, bool overrideSettingsRestoreAfterwards = true, int timeoutInSeconds = 60, string serverUrl = "http://127.0.0.1:7860", string[]? styles = null, int subseed = -1, int subseedStrength = 0, int seedResizeFromH = -1, int seedResizeFromW = -1, double denoisingStrength = 0.75, double sMinUncond = 0, double sChurn = 0, double sTmax = 0, double sTmin = 0, double sNoise = 0, object? overrideSettings = null, int refinerSwitchAt = 0, bool disableExtraNetworks = false, string[]? initImages = null, int resizeMode = 0, int imageCfgScale = 0, int maskBlurX = 4, int maskBlurY = 4, int maskBlur = 0, int inpaintingFill = 0, bool inpaintFullRes = true, int inpaintingFullResPadding = 0, int inpaintingMaskInvert = 0, int initialNoiseMultiplier = 0, string latentMask = "", string samplerIndex = "Euler", bool includeInitImages = false, string scriptName = "", string[]? scriptArgs = null, bool sendImages = true, bool saveImages = false)
        {
            string endpoint = $"{serverUrl}/sdapi/v1/img2img";
            prompt = prompt.Trim();
            string defaultNegativePrompt = negativePrompt.Trim();
            string imageB64 = "";
            string maskB64 = "";
            if (!string.IsNullOrWhiteSpace(inputImage))
            {
                imageB64 = inputImage;
            }
            if (maskImage != null)
            {
                maskB64 = maskImage;
            }
            if (!string.IsNullOrEmpty(defaultNegativePrompt))
            {
                defaultNegativePrompt += $", {defaultNegativePrompt}";
            }
            StableDiffusionImg2ImgRequest requestPayload = new()
            {
                Prompt = prompt,
                NegativePrompt = defaultNegativePrompt,
                Styles = styles ?? Array.Empty<string>(),
                Seed = seed,
                Subseed = subseed,
                SubseedStrength = subseedStrength,
                SeedResizeFromH = seedResizeFromH,
                SeedResizeFromW = seedResizeFromW,
                SamplerName = samplerName,
                BatchSize = batchSize,
                NumberOfIterations = batchCount,
                Steps = steps,
                CfgScale = cfgScale,
                Width = width,
                Height = height,
                RestoreFaces = restoreFaces,
                Tiling = tiling,
                DoNotSaveSamples = false,
                DoNotSaveGrid = false,
                Eta = 0,
                DenoisingStrength = denoisingStrength,
                SMinUncond = sMinUncond,
                SChurn = sChurn,
                STmax = sTmax,
                STmin = sTmin,
                SNoise = sNoise,
                OverrideSettings = overrideSettings ?? new object(),
                OverrideSettingsRestoreAfterwards = overrideSettingsRestoreAfterwards,
                RefinerCheckpoint = model,
                RefinerSwitchAt = refinerSwitchAt,
                DisableExtraNetworks = disableExtraNetworks,
                Comments = new object(),
                InitImages = initImages ?? Array.Empty<string>(),
                ResizeMode = resizeMode,
                ImageCfgScale = imageCfgScale,
                Mask = maskB64,
                MaskBlurX = maskBlurX,
                MaskBlurY = maskBlurY,
                MaskBlur = maskBlur,
                InpaintingFill = inpaintingFill,
                InpaintFullRes = inpaintFullRes,
                InpaintingFullResPadding = inpaintingFullResPadding,
                InpaintingMaskInvert = inpaintingMaskInvert,
                InitialNoiseMultiplier = initialNoiseMultiplier,
                LatentMask = latentMask,
                SamplerIndex = samplerIndex,
                IncludeInitImages = includeInitImages,
                ScriptName = scriptName,
                ScriptArgs = scriptArgs ?? Array.Empty<string>(),
                SendImages = sendImages,
                SaveImages = saveImages,
                AlwaysOnScripts = new object()
            };

            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
#if WINDOWS
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
            string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            string jsonRequest = JsonSerializer.Serialize(requestPayload);
            HttpResponseMessage response = await client.PostAsync(endpoint, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("API request failed with status code: " + (int)response.StatusCode);
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            StableDiffusionImg2ImgResponse? jsonResponse = JsonSerializer.Deserialize<StableDiffusionImg2ImgResponse>(responseBody);
            if (jsonResponse != null && jsonResponse.Images != null)
            {
                List<byte[]> bytesToReturn = new();
                Parallel.ForEach(jsonResponse.Images, (item) =>
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        byte[] imageBytes = Convert.FromBase64String(item);
                        if (imageBytes != null)
                        {
                            lock (bytesToReturn)
                            {
                                bytesToReturn.Add(imageBytes);
                            }
                        }
                    }
                });
                return bytesToReturn;
            }
            else
            {
                throw new JsonException("Stable Diffusion Response was malformed.");
            }
        }
        public static async Task<StableDiffusionModel[]?> GetStableDiffusionModelsAsync(string serverUrl = "http://127.0.0.1:7860", int timeoutInSeconds = 60)
        {
            string endpoint = $"{serverUrl}/sdapi/v1/sd-models";
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
#if WINDOWS
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
            string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            HttpResponseMessage response = await client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("API request failed with status code: " + (int)response.StatusCode);
            }
            string responseBody = await response.Content.ReadAsStringAsync();
            StableDiffusionModel[]? models = JsonSerializer.Deserialize<StableDiffusionModel[]>(responseBody);
            return models;
        }
        public static async Task<List<string>> GetStableDiffusionModelsListAsync(string serverUrl = "http://127.0.0.1:7860")
        {
            string endpoint = $"{serverUrl}/sdapi/v1/sd-models";
            List<string> models = new();
            StableDiffusionModel[]? modelsResponse = await GetStableDiffusionModelsAsync(endpoint);
            if (modelsResponse != null)
            {
                Parallel.ForEach(modelsResponse, model =>
                {
                    lock (models)
                    {
                        models.Add(model.ModelName);
                    }
                });
            }
            return models;
        }
        public static async Task<ProgressResponse?> GetProgressAsync(string serverUrl = "http://127.0.0.1:7860", int timeoutInSeconds = 60)
        {
            string endpoint = $"{serverUrl}/sdapi/v1/progress";
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
#if WINDOWS
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
            string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            HttpResponseMessage response = await client.PostAsync(endpoint, null);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("API request failed with status code: " + (int)response.StatusCode);
            }
            string responseBody = await response.Content.ReadAsStringAsync();
            ProgressResponse? progressResponse = JsonSerializer.Deserialize<ProgressResponse>(responseBody);
            return progressResponse;
        }
        #region TypeClasses
        public class StableDiffusionRequest
        {
            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = "";
            [JsonPropertyName("negative_prompt")]
            public string NegativePrompt { get; set; } = "";
            [JsonPropertyName("styles")]
            public string[] Styles { get; set; } = new string[] { "" };
            [JsonPropertyName("seed")]
            public int Seed { get; set; } = -1;
            [JsonPropertyName("subseed")]
            public int Subseed { get; set; } = -1;
            [JsonPropertyName("subseed_strength")]
            public int SubseedStrength { get; set; } = 0;
            [JsonPropertyName("seed_resize_from_h")]
            public int SeedResizeFromH { get; set; } = -1;
            [JsonPropertyName("seed_resize_from_w")]
            public int SeedResizeFromW { get; set; } = -1;
            [JsonPropertyName("sampler_name")]
            public string SamplerName { get; set; } = "";
            [JsonPropertyName("batch_size")]
            public int BatchSize { get; set; } = 1;
            [JsonPropertyName("n_iter")]
            public int NumberOfIterations { get; set; } = 1;
            [JsonPropertyName("steps")]
            public int Steps { get; set; } = 50;
            [JsonPropertyName("cfg_scale")]
            public int CfgScale { get; set; } = 7;
            [JsonPropertyName("width")]
            public int Width { get; set; } = 512;
            [JsonPropertyName("height")]
            public int Height { get; set; } = 512;
            [JsonPropertyName("restore_faces")]
            public bool RestoreFaces { get; set; } = true;
            [JsonPropertyName("tiling")]
            public bool Tiling { get; set; } = true;
            [JsonPropertyName("do_not_save_samples")]
            public bool DoNotSaveSamples { get; set; } = false;
            [JsonPropertyName("do_not_save_grid")]
            public bool DoNotSaveGrid { get; set; } = false;
            [JsonPropertyName("eta")]
            public int Eta { get; set; } = 0;
            [JsonPropertyName("denoising_strength")]
            public int DenoisingStrength { get; set; } = 0;
            [JsonPropertyName("s_min_uncond")]
            public int SMinUncond { get; set; } = 0;
            [JsonPropertyName("s_churn")]
            public int SChurn { get; set; } = 0;
            [JsonPropertyName("s_tmax")]
            public int STmax { get; set; } = 0;
            [JsonPropertyName("s_tmin")]
            public int STmin { get; set; } = 0;
            [JsonPropertyName("s_noise")]
            public int SNoise { get; set; } = 0;
            [JsonPropertyName("override_settings")]
            public object OverrideSettings { get; set; } = new object();
            [JsonPropertyName("override_settings_restore_afterwards")]
            public bool OverrideSettingsRestoreAfterwards { get; set; } = true;
            [JsonPropertyName("refiner_checkpoint")]
            public string RefinerCheckpoint { get; set; } = "";
            [JsonPropertyName("refiner_switch_at")]
            public int RefinerSwitchAt { get; set; } = 0;
            [JsonPropertyName("disable_extra_networks")]
            public bool DisableExtraNetworks { get; set; } = false;
            [JsonPropertyName("comments")]
            public object Comments { get; set; } = new object();
            [JsonPropertyName("enable_hr")]
            public bool EnableHr { get; set; } = false;
            [JsonPropertyName("firstphase_width")]
            public int FirstPhaseWidth { get; set; } = 0;
            [JsonPropertyName("firstphase_height")]
            public int FirstPhaseHeight { get; set; } = 0;
            [JsonPropertyName("hr_scale")]
            public int HrScale { get; set; } = 2;
            [JsonPropertyName("hr_upscaler")]
            public string HrUpscaler { get; set; } = "";
            [JsonPropertyName("hr_second_pass_steps")]
            public int HrSecondPassSteps { get; set; } = 0;
            [JsonPropertyName("hr_resize_x")]
            public int HrResizeX { get; set; } = 0;
            [JsonPropertyName("hr_resize_y")]
            public int HrResizeY { get; set; } = 0;
            [JsonPropertyName("hr_checkpoint_name")]
            public string HrCheckpointName { get; set; } = "";
            [JsonPropertyName("hr_sampler_name")]
            public string HrSamplerName { get; set; } = "";
            [JsonPropertyName("hr_prompt")]
            public string HrPrompt { get; set; } = "";
            [JsonPropertyName("hr_negative_prompt")]
            public string HrNegativePrompt { get; set; } = "";
            [JsonPropertyName("sampler_index")]
            public string SamplerIndex { get; set; } = "Euler";
            [JsonPropertyName("script_name")]
            public string ScriptName { get; set; } = "";
            [JsonPropertyName("script_args")]
            public string[] ScriptArgs { get; set; } = Array.Empty<string>();
            [JsonPropertyName("send_images")]
            public bool SendImages { get; set; } = true;
            [JsonPropertyName("save_images")]
            public bool SaveImages { get; set; } = false;
            [JsonPropertyName("alwayson_scripts")]
            public object AlwaysOnScripts { get; set; } = new object();
        }
        public class SuccessResponseModel
        {
            [JsonPropertyName("images")]
            public string[]? Images { get; set; }
            [JsonPropertyName("parameters")]
            public Dictionary<string, object>? Parameters { get; set; } = new();
            [JsonPropertyName("info")]
            public string Info { get; set; } = string.Empty;
        }
        public class ErrorResponseModel
        {
            [JsonPropertyName("detail")]
            public List<ErrorDetail>? Detail { get; set; }
        }
        public class ErrorDetail
        {
            [JsonPropertyName("loc")]
            public List<string>? Loc { get; set; }
            [JsonPropertyName("msg")]
            public string? Msg { get; set; }
            [JsonPropertyName("type")]
            public string? Type { get; set; }
        }
        public class StableDiffusionImg2ImgRequest
        {
            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = "";
            [JsonPropertyName("negative_prompt")]
            public string NegativePrompt { get; set; } = "";
            [JsonPropertyName("styles")]
            public string[] Styles { get; set; } = new string[] { "" };
            [JsonPropertyName("seed")]
            public int Seed { get; set; } = -1;
            [JsonPropertyName("subseed")]
            public int Subseed { get; set; } = -1;
            [JsonPropertyName("subseed_strength")]
            public int SubseedStrength { get; set; } = 0;
            [JsonPropertyName("seed_resize_from_h")]
            public int SeedResizeFromH { get; set; } = -1;
            [JsonPropertyName("seed_resize_from_w")]
            public int SeedResizeFromW { get; set; } = -1;
            [JsonPropertyName("sampler_name")]
            public string SamplerName { get; set; } = "string";
            [JsonPropertyName("batch_size")]
            public int BatchSize { get; set; } = 1;
            [JsonPropertyName("n_iter")]
            public int NumberOfIterations { get; set; } = 1;
            [JsonPropertyName("steps")]
            public int Steps { get; set; } = 50;
            [JsonPropertyName("cfg_scale")]
            public int CfgScale { get; set; } = 7;
            [JsonPropertyName("width")]
            public int Width { get; set; } = 512;
            [JsonPropertyName("height")]
            public int Height { get; set; } = 512;
            [JsonPropertyName("restore_faces")]
            public bool RestoreFaces { get; set; } = true;
            [JsonPropertyName("tiling")]
            public bool Tiling { get; set; } = true;
            [JsonPropertyName("do_not_save_samples")]
            public bool DoNotSaveSamples { get; set; } = false;
            [JsonPropertyName("do_not_save_grid")]
            public bool DoNotSaveGrid { get; set; } = false;
            [JsonPropertyName("eta")]
            public int Eta { get; set; } = 0;
            [JsonPropertyName("denoising_strength")]
            public double DenoisingStrength { get; set; } = 0.75;
            [JsonPropertyName("s_min_uncond")]
            public double SMinUncond { get; set; } = 0;
            [JsonPropertyName("s_churn")]
            public double SChurn { get; set; } = 0;
            [JsonPropertyName("s_tmax")]
            public double STmax { get; set; } = 0;
            [JsonPropertyName("s_tmin")]
            public double STmin { get; set; } = 0;
            [JsonPropertyName("s_noise")]
            public double SNoise { get; set; } = 0;
            [JsonPropertyName("override_settings")]
            public object OverrideSettings { get; set; } = new object();
            [JsonPropertyName("override_settings_restore_afterwards")]
            public bool OverrideSettingsRestoreAfterwards { get; set; } = true;
            [JsonPropertyName("refiner_checkpoint")]
            public string RefinerCheckpoint { get; set; } = "string";
            [JsonPropertyName("refiner_switch_at")]
            public int RefinerSwitchAt { get; set; } = 0;
            [JsonPropertyName("disable_extra_networks")]
            public bool DisableExtraNetworks { get; set; } = false;
            [JsonPropertyName("comments")]
            public object Comments { get; set; } = new object();
            [JsonPropertyName("init_images")]
            public string[] InitImages { get; set; } = new string[] { "" };
            [JsonPropertyName("resize_mode")]
            public int ResizeMode { get; set; } = 0;
            [JsonPropertyName("image_cfg_scale")]
            public int ImageCfgScale { get; set; } = 0;
            [JsonPropertyName("mask")]
            public string Mask { get; set; } = "string";
            [JsonPropertyName("mask_blur_x")]
            public int MaskBlurX { get; set; } = 4;
            [JsonPropertyName("mask_blur_y")]
            public int MaskBlurY { get; set; } = 4;
            [JsonPropertyName("mask_blur")]
            public int MaskBlur { get; set; } = 0;
            [JsonPropertyName("inpainting_fill")]
            public int InpaintingFill { get; set; } = 0;
            [JsonPropertyName("inpaint_full_res")]
            public bool InpaintFullRes { get; set; } = true;
            [JsonPropertyName("inpaint_full_res_padding")]
            public int InpaintingFullResPadding { get; set; } = 0;
            [JsonPropertyName("inpainting_mask_invert")]
            public int InpaintingMaskInvert { get; set; } = 0;
            [JsonPropertyName("initial_noise_multiplier")]
            public int InitialNoiseMultiplier { get; set; } = 0;
            [JsonPropertyName("latent_mask")]
            public string LatentMask { get; set; } = "string";
            [JsonPropertyName("sampler_index")]
            public string SamplerIndex { get; set; } = "Euler";
            [JsonPropertyName("include_init_images")]
            public bool IncludeInitImages { get; set; } = false;
            [JsonPropertyName("script_name")]
            public string ScriptName { get; set; } = "string";
            [JsonPropertyName("script_args")]
            public string[] ScriptArgs { get; set; } = Array.Empty<string>();
            [JsonPropertyName("send_images")]
            public bool SendImages { get; set; } = true;
            [JsonPropertyName("save_images")]
            public bool SaveImages { get; set; } = false;
            [JsonPropertyName("alwayson_scripts")]
            public object AlwaysOnScripts { get; set; } = new object();
        }
        public class StableDiffusionImg2ImgResponse
        {
            [JsonPropertyName("images")]
            public string[] Images { get; set; } = new string[] { "" };
            [JsonPropertyName("parameters")]
            public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
            [JsonPropertyName("info")]
            public string Info { get; set; } = "string";
        }
        public class Img2Img422Response
        {
            public class DetailItem
            {
                [JsonPropertyName("loc")]
                public string[] Location { get; set; } = new string[] { "string", "0" };
                [JsonPropertyName("msg")]
                public string Message { get; set; } = "string";
                [JsonPropertyName("type")]
                public string Type { get; set; } = "string";
            }
            [JsonPropertyName("detail")]
            public List<DetailItem> Detail { get; set; } = new List<DetailItem>();
        }
        public class StableDiffusionModel
        {
            [JsonPropertyName("title")]
            public string Title { get; set; } = "";
            [JsonPropertyName("model_name")]
            public string ModelName { get; set; } = "";
            [JsonPropertyName("hash")]
            public string? Hash { get; set; } = null;
            [JsonPropertyName("sha256")]
            public string? Sha256 { get; set; } = null;
            [JsonPropertyName("filename")]
            public string Filename { get; set; } = "";
            [JsonPropertyName("config")]
            public string? Config { get; set; } = null;
        }
        public class ProgressResponse
        {
            [JsonPropertyName("progress")]
            public int Progress { get; set; }
            [JsonPropertyName("eta_ralative")]
            public int EtaRelative { get; set; }
            [JsonPropertyName("state")]
            public object? State { get; set; }
            [JsonPropertyName("current_image")]
            public string CurrentImage { get; set; } = "";
            [JsonPropertyName("textinfo")]
            public string TextInfo { get; set; } = "";
        }
        public class PngInfoResponse
        {
            [JsonPropertyName("info")]
            public string Info { get; set; } = "";
        }
        #endregion
        #endregion
        #region GptNeo
        public static async Task<string> GenerateGptNeoAsyncHttp(string prompt, int maxTokens = 2000, string serverUrl = "http://192.168.0.125:8000", int timeoutInSeconds = 60)
        {
            prompt = prompt.Trim();
            var requestPayload = new GptNeoRequest
            {
                Text = prompt.Trim(),
                Temperature = 0.1,
                MinLength = 50, // Adjust as needed
                MaxLength = maxTokens, // Adjust as needed
                DoSample = true
            };
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
#if WINDOWS
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
            string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            string endpoint = serverUrl + "/call"; // Adjust the endpoint URL
            string jsonRequest = JsonSerializer.Serialize(requestPayload);
            StringContent jsonContent = new(jsonRequest, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(endpoint, jsonContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("API request failed with status code: " + (int)response.StatusCode);
            }
            string jsonResponse = await response.Content.ReadAsStringAsync();
            GptNeoResponse? responsePayload = JsonSerializer.Deserialize<GptNeoResponse>(jsonResponse);
            return responsePayload?.GeneratedText ?? "";
        }
        #region TypeClasses
        public class GptNeoRequest
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = "";
            [JsonPropertyName("temperature")]
            public double Temperature { get; set; } = 0.8;
            [JsonPropertyName("min_length")]
            public int MinLength { get; set; } = 50;
            [JsonPropertyName("max_length")]
            public int MaxLength { get; set; } = 8000;
            [JsonPropertyName("do_sample")]
            public bool DoSample { get; set; } = false;
        }
        public class GptNeoResponse
        {
            [JsonPropertyName("generated_text")]
            public string GeneratedText { get; set; } = "";
        }
        #endregion
        #endregion
        #region OpenAIAPI
        #region EndPoints
        private static readonly string completionEndpoint = "/completions";
        private static readonly string TextGenerationEndpoint = "/chat/completions";
        private static readonly string editsEndpoint = "/edits";
        private static readonly string modelEndpoint = "/models";
        private static readonly string embeddingsEndpoint = "/embeddings";
        private static readonly string AudioGenerationEndpoint = "/tts";
        private static readonly string speechGenerationEndpoint = "/audio/speech";
        private static readonly string transcriptionEndpoint = "/audio/transcriptions";
        private static readonly string translationEndpoint = "/audio/translations";
        private static readonly string imageGenerationEndpoint = "/images/generations";
        private static readonly string imageVariationsEndpoint = "/images/variations";
        private static readonly string ModerationsEndpoint = "/moderations";
        private static readonly string imageEditsEndpoint = "/images/edits";
        private static readonly string filesEndpoint = "/files";
        private static readonly string fineTuningEndpoint = "/fine_tuning/jobs";
        #endregion
        readonly List<string> exampleTexts = new()
        {
            "This is a sample text string.",
            "Here's another text string.",
            "And yet another text string.",
            "This is a fourth text string.",
            "Lastly, a fifth text string."
        };
        public async Task<List<string>> SearchText(string query, List<float> embeddings, int k = 5) // NEEDS FIX
        {
            List<float> queryEmbedding = new(); //await GetEmbeddings(new List<string> { query }, "text-embedding-3-small");
            List<int> distances = embeddings.Select((e, i) => new { Index = i, Distance = VectorDistance(new List<float>() { e }, queryEmbedding) })
                .OrderBy(x => x.Distance).Take(k).Select(x => x.Index).ToList();
            await Task.Delay(0);
            return exampleTexts?.Where((t, i) => distances.Contains(i)).ToList() ?? new List<string>();
        }
        private static float VectorDistance(List<float> a, List<float> b)
        {
            return (float)Math.Sqrt(a.Zip(b, (x, y) => (x - y) * (x - y)).Sum());
        }
        public static double EstimateTokenCost(string input, double costPerToken = 0.01)
        {
            double tokenCount = Math.Ceiling((double)input.Length / 4);
            double cost = tokenCount * costPerToken;
            return cost;
        }
        public static async Task<bool> CheckTextForViolations(string prompt, string apiKey, int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            bool Violates = false;
            TextViolationCheckRequest payload = new()
            {
                Input = prompt,
                Model = "text-moderation-latest"
            };
            // create an HTTP client
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
            if (authEnabled)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }
            string json = JsonSerializer.Serialize(payload);
            StringContent jsonContent = new(json, Encoding.UTF8, "application/json");
#if WINDOWS
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
            string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            HttpResponseMessage response = await client.PostAsync($"{serverUrl}{ModerationsEndpoint}", jsonContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
            }
            string responseJson = await response.Content.ReadAsStringAsync();
            TextModerationResponse responseData = JsonSerializer.Deserialize<TextModerationResponse>(responseJson)!;

            foreach (TextModerationResponse.ModResult modResults in responseData.Results)
            {
                if (modResults.Flagged)
                {
                    Violates = true;
                }
            }

            return Violates;
        }
        public static async Task<Message> GenerateTextGenerationAsync(List<Message> messages, string apiKey, string model = "gpt-3.5-turbo", double temperature = 1.0, double topP = 1.0, int n = 1, int maxTokens = 4000, double presencePenalty = 0.0, double frequencyPenalty = 0.0, int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            if (messages.Count < 1)
            {
                throw new Exception("No messages found to create the completion.");
            }
            int inputCost = 0;
            foreach (Message message in messages)
            {
                inputCost += (message.Content.Length / 4);
            }
            int curMsg = 0;
            while ((maxTokens - inputCost) < 500 && curMsg < messages.Count)
            {
                if (messages[curMsg].Role != "system")
                {
                    inputCost -= messages[curMsg].Content.Length / 4;
                    messages.RemoveAt(curMsg);
                }
                else
                {
                    curMsg++;
                }
            }
            TextGenerationRequest payload = new()
            {
                Model = model,
                Messages = messages,
                Temperature = temperature,
                TopP = topP,
                NumOfResponse = n,
                MaxTokens = (maxTokens - inputCost),
                PresencePenalty = presencePenalty,
                FrequencyPenalty = frequencyPenalty
            };

            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
            if (authEnabled)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
#if WINDOWS
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
            string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            string json = JsonSerializer.Serialize(payload);
            StringContent jsonContent = new(json, Encoding.UTF8, "application/json");
            jsonContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await client.PostAsync($"{serverUrl}{TextGenerationEndpoint}", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();
                TextGenerationResult result = JsonSerializer.Deserialize<TextGenerationResult>(responseJson)!;
                return result.Choices[0].Message;
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
        }

        public static async Task<Message> GenerateStreamingTextGenerationAsync(List<Message> messages, string apiKey, bool stream = false, string model = "gpt-3.5-turbo", double temperature = 1.0, double topP = 1.0, int n = 1, int maxTokens = 4000, double presencePenalty = 0.0, double frequencyPenalty = 0.0, int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true, List<string>? stop = null, Dictionary<string, int>? logitBias = null, bool logprobs = false, int topLogprobs = int.MinValue, int seed = -1, List<string>? tools = null, CancellationToken cancellationToken = default)
        {
            string fullResponse = string.Empty;
            try
            {
                if (messages.Count < 1)
                {
                    throw new Exception("No messages found to create the completion.");
                }
                if (messages.LastOrDefault()?.Role == "assistant" && string.IsNullOrWhiteSpace(messages.LastOrDefault()?.Content))
                {
                    messages.Remove(messages.Last());
                }
                TextGenerationRequest payload = new()
                {
                    Model = model,
                    Messages = messages,
                    Temperature = temperature,
                    TopP = topP,
                    NumOfResponse = n,
                    MaxTokens = maxTokens,
                    PresencePenalty = presencePenalty,
                    FrequencyPenalty = frequencyPenalty,
                    Seed = seed,
                    Stream = stream
                };
                if (stop != null && stop.Count > 0)
                {
                    payload.Stop = stop;
                }
                if (logitBias != null && logitBias.Count > 0)
                {
                    payload.LogitBias = logitBias;
                }
                if (logprobs)
                {
                    payload.Logprobs = logprobs;
                }
                if (topLogprobs != int.MinValue)
                {
                    payload.TopLogprobs = topLogprobs;
                }
                if (tools != null && tools.Count > 0)
                {
                    payload.Tools = tools;
                }
                string json = JsonSerializer.Serialize(payload);

                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);

                if (authEnabled)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                StringContent jsonContent = new(json, Encoding.UTF8, "application/json");
                HttpRequestMessage httpRequest = new()
                {
                    RequestUri = new Uri($"{serverUrl}{TextGenerationEndpoint}"),
                    Method = HttpMethod.Post,
                    Content = jsonContent
                };
                if (cancellationToken == default)
                {
                    cancellationToken = requestCancellation.Token;
                }
                HttpResponseMessage response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                response.EnsureSuccessStatusCode();

                if (stream)
                {
                    using Stream dataStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    using StreamReader reader = new(dataStream);
                    string? line;
                    TextGenerationChunk previousChunk = new();
                    while ((line = await reader.ReadLineAsync(CancellationToken.None)) != null)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            requestCancellation = new();
                            break;
                        }
                        if (line.StartsWith("data:"))
                        {
                            int startIndex = line.IndexOf('{');
                            if (startIndex != -1)
                            {
                                string responseJson = line[startIndex..];
                                var chunk = JsonSerializer.Deserialize<TextGenerationChunk>(responseJson);
                                if (chunk != null && chunk.Choices != null && chunk.Choices.Count > 0)
                                {
                                    if (chunk == previousChunk)
                                    {
                                        break;
                                    }
                                    previousChunk = chunk;
                                    foreach (var choice in chunk.Choices)
                                    {
                                        OnDataReceived?.Invoke(null, choice.Delta.Content);
                                        fullResponse += choice.Delta.Content;
                                    }
                                }
                            }
                        }
                    }
                    return new Message { Role = "assistant", Content = fullResponse };
                }
                else
                {
                    string responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                    TextGenerationResult result = JsonSerializer.Deserialize<TextGenerationResult>(responseJson)!;
                    return result.Choices[0].Message;
                }
            }
            catch (OperationCanceledException)
            {
                requestCancellation = new();
                return new Message()
                {
                    Role = "assistant",
                    Content = ""
                };
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        public static event EventHandler<string>? OnDataReceived;
        internal static CancellationTokenSource requestCancellation = new();
        public class TextGenerationChunk
        {
            [JsonPropertyName("created")]
            public long Created { get; set; } = long.MinValue;

            [JsonPropertyName("object")]
            public string Object { get; set; } = string.Empty;

            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("choices")]
            public List<StreamChoice> Choices { get; set; } = new();
        }
        public class StreamChoice
        {
            [JsonPropertyName("index")]
            public int Index { get; set; } = int.MinValue;

            [JsonPropertyName("delta")]
            public DeltaContent Delta { get; set; } = new();
        }
        public class DeltaContent
        {
            [JsonPropertyName("content")]
            public string Content { get; set; } = string.Empty;
        }

        public static async Task<List<string>?> GetEmbeddingsAsync(string apiKey, string input, string model = "text-embedding-ada-002", int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            try
            {
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);

                if (authEnabled)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }
#if WINDOWS
                string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
                string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                HttpRequestMessage request = new(HttpMethod.Post, $"{serverUrl}{embeddingsEndpoint}");
                request.Headers.Add("Content-Type", "application/json");
                EmbeddingRequest requestBody = new()
                {
                    Input = input,
                    Model = model
                };
                string json = JsonSerializer.Serialize(requestBody);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }

                string responseJson = await response.Content.ReadAsStringAsync();
                JsonSerializerOptions options = JsonOptions;
                EmbeddingResponse? apiResponse = JsonSerializer.Deserialize<EmbeddingResponse>(responseJson, options);

                if (apiResponse != null && apiResponse.Data != null)
                {
                    return apiResponse.Data.Select(e => string.Join(",", e.Embedding)).ToList();
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Connection failure: {ex.Message}");
            }
        }

        public static List<MultiModalMessage> ConvertToMultiModalMessages(List<Message> messages)
        {
            List<MultiModalMessage> multiModalMessages = new();

            foreach (var message in messages)
            {
                MultiModalMessage multiModalMessage = new()
                {
                    Role = message.Role
                };

                if (message.Role == "user")
                {
                    MultiModalContent textContent = new MultiModalTextContent
                    {
                        Type = "text",
                        Text = message.Content
                    };
                    multiModalMessage.Content.Add(textContent);
                }
                else
                {
                    MultiModalContent systemContent = new MultiModalTextContent
                    {
                        Type = "text",
                        Text = message.Content
                    };
                    multiModalMessage.Content.Add(systemContent);
                }

                multiModalMessages.Add(multiModalMessage);
            }

            return multiModalMessages;
        }
        public static async Task<Message> GenerateMultiModalTextGenerationAsync(List<MultiModalMessage> messages, string apiKey, string model = "gpt-4-vision-preview", double temperature = 1.0, double topP = 1.0, int n = 1, int maxTokens = 32768, double presencePenalty = 0.0, double frequencyPenalty = 0.0, int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            try
            {
                if (messages.Count < 1)
                {
                    throw new Exception("No messages found to create the completion.");
                }
                int inputCost = 0;
                foreach (MultiModalMessage message in messages)
                {
                    foreach (MultiModalContent curContent in message.Content)
                    {
                        if (curContent is MultiModalTextContent textContent && !string.IsNullOrWhiteSpace(textContent.Text))
                        {
                            inputCost += (textContent.Text.Length / 4);
                        }
                        else if (curContent is MultiModalImageContent imageContent && !string.IsNullOrWhiteSpace(imageContent.ImageUrl.Url))
                        {
                            //inputCost += (imageContent.ImageUrl.Url.Length / 4);
                        }
                    }
                }

                int curMsg = 0;
                while ((maxTokens - inputCost) < 20 && curMsg < messages.Count)
                {
                    if (messages[curMsg].Role != "system")
                    {
                        foreach (MultiModalContent curContent in messages[curMsg].Content)
                        {
                            if (curContent is MultiModalTextContent textContent && !string.IsNullOrWhiteSpace(textContent.Text))
                            {
                                inputCost -= textContent.Text.Length / 4;
                            }
                            else if (curContent is MultiModalImageContent imageContent && !string.IsNullOrWhiteSpace(imageContent.ImageUrl.Url))
                            {
                                inputCost -= imageContent.ImageUrl.Url.Length / 4;
                            }
                        }
                        messages.RemoveAt(curMsg);
                    }
                    else
                    {
                        curMsg++;
                    }
                }

                MultiModalTextGenerationRequest payload = new()
                {
                    Model = model,
                    Messages = messages
                };
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                if (authEnabled)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
#if WINDOWS
                string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
                string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

                string json = JsonSerializer.Serialize(payload);
                StringContent jsonContent = new(json, Encoding.UTF8, "application/json");
                jsonContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await client.PostAsync($"{serverUrl}{TextGenerationEndpoint}", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    MultiModalTextGenerationResult result = JsonSerializer.Deserialize<MultiModalTextGenerationResult>(responseJson)!;
                    return result.Choices[0].Message;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<string> GenerateImageAsyncHttp(string prompt, string apiKey = "sk-xxx", string size = "1024x1024", string model = "dall-e-2", int numOfImages = 1, string quality = "standard", string responseFormat = "url", string style = "vivid", int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            try
            {
                prompt = prompt.Trim();
                ImageGenerationRequest payload = new()
                {
                    Prompt = prompt,
                    Model = model,
                    N = numOfImages,
                    Quality = quality,
                    ResponseFormat = responseFormat,
                    Size = size,
                    Style = style
                };

                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                if (authEnabled)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }
#if WINDOWS
                string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
                string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                string json = JsonSerializer.Serialize(payload);
                StringContent jsonContent = new(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync($"{serverUrl}{imageGenerationEndpoint}", jsonContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }
                string responseJson = await response.Content.ReadAsStringAsync();
                ImageUrlResponse responseData = JsonSerializer.Deserialize<ImageUrlResponse>(responseJson)!;
                return responseData.Data.First().Url;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }

        public static async Task<byte[]> GenerateSpeech(string prompt, string apiKey = "sk-xxx", string voice = "onyx", string model = "tts-1", string responseFormat = "mp3", double speed = 1.0, int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(prompt))
                {
                    throw new Exception($"API request failed: Prompt is Null or EMPTY.");
                }
                if (prompt.Length > 4096)
                {
                    throw new Exception($"API request failed: Prompt is too long.");
                }
                if (authEnabled && string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception($"API request failed: API Key is Null or EMPTY.");
                }
                prompt = prompt.Trim();
                SpeechRequestOptional payload = new()
                {
                    Model = model,
                    Voice = voice,
                    Input = prompt,
                    ResponseFormat = responseFormat,
                    Speed = speed
                };

                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                if (authEnabled)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }
                string json = JsonSerializer.Serialize(payload);
                StringContent jsonContent = new(json, Encoding.UTF8, "application/json");
#if WINDOWS
                string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
                string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                HttpResponseMessage response = await client.PostAsync($"{serverUrl}{speechGenerationEndpoint}", jsonContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }
                byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();
                return responseBytes;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }

        public static async Task<string> ImageVariationsAsyncHttp(byte[] image, string apiKey = "sk-xxx", string size = "1024x1024", int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            try
            {
                using MultipartFormDataContent content = new();
                ByteArrayContent imageContent = new(image);
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                content.Add(imageContent, "image", "otter.png");
                content.Add(new StringContent(size), "size");
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                if (authEnabled)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }
#if WINDOWS
                string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
                string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                HttpResponseMessage response = await client.PostAsync($"{serverUrl}{imageVariationsEndpoint}", content);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }
                string responseJson = await response.Content.ReadAsStringAsync();
                ImageUrlResponse responseData = JsonSerializer.Deserialize<ImageUrlResponse>(responseJson)!;
                return responseData.Data.First().Url;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<string> EditImageAsyncHttp(string prompt, byte[] image, byte[] mask, string apiKey, string size = "1024x1024", int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            try
            {
                using (HttpClient client = new())
                {
                    client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                    if (authEnabled)
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    }
                    using (MultipartFormDataContent content = new())
                    {
                        ByteArrayContent imageContent = new(image);
                        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                        content.Add(imageContent, "image", "image.png");

                        if (mask != null)
                        {
                            ByteArrayContent maskContent = new(mask);
                            maskContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                            content.Add(maskContent, "mask", "mask.png");
                        }

                        content.Add(new StringContent(prompt), "prompt");
                        content.Add(new StringContent(1.ToString()), "n");
                        content.Add(new StringContent(size), "size");
#if WINDOWS
                        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
                        string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
                        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                        HttpResponseMessage response = await client.PostAsync($"{serverUrl}{imageEditsEndpoint}", content);
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception($"Failed to generate image edits: {response.ReasonPhrase}");
                        }
                        string responseJson = await response.Content.ReadAsStringAsync();
                        ImageUrlResponse responseData = JsonSerializer.Deserialize<ImageUrlResponse>(responseJson)!;
                        return responseData.Data.First().Url;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<string> EditImageAsyncHttp(string prompt, byte[] image, string apiKey = "sk-xxx", string size = "1024x1024", int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            try
            {
                using (HttpClient client = new())
                {
                    client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                    if (authEnabled)
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    }
                    using (MultipartFormDataContent content = new())
                    {
                        ByteArrayContent imageContent = new(image);
                        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                        content.Add(imageContent, "image", "image.png");

                        content.Add(new StringContent(prompt), "prompt");
                        content.Add(new StringContent(1.ToString()), "n");
                        content.Add(new StringContent(size), "size");
#if WINDOWS
                        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
                        string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
                        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                        HttpResponseMessage response = await client.PostAsync($"{serverUrl}{imageEditsEndpoint}", content);
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception($"Failed to generate image edits: {response.ReasonPhrase}");
                        }
                        string responseJson = await response.Content.ReadAsStringAsync();
                        ImageUrlResponse responseData = JsonSerializer.Deserialize<ImageUrlResponse>(responseJson)!;
                        return responseData.Data.First().Url;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }

        public static async Task<string> TranslateAudioAsyncHttp(byte[] audio, string filename, string apiKey, string model = "whisper-1", int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            try
            {
                string audioB64 = Convert.ToBase64String(audio);
                using (HttpClient client = new())
                {
                    client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                    if (authEnabled)
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    }
                    using (MultipartFormDataContent content = new())
                    {
                        ByteArrayContent audioContent = new(audio);
                        audioContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                        content.Add(audioContent, "file", filename);

                        content.Add(new StringContent(model), "model");

#if WINDOWS
                        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
                        string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
                        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                        HttpResponseMessage response = await client.PostAsync($"{serverUrl}{translationEndpoint}", content);
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception($"Failed to transcribe audio: {response.ReasonPhrase}");
                        }

                        string responseJson = await response.Content.ReadAsStringAsync();
                        TranscriptionResponse responseData = JsonSerializer.Deserialize<TranscriptionResponse>(responseJson)!;
                        return responseData.Text;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<string> TranscribeAudioAsyncHttp(byte[] audio, string filename, string apiKey, string model = "whisper-1", int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            try
            {
                string audioB64 = Convert.ToBase64String(audio);
                using (HttpClient client = new())
                {
                    client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                    if (authEnabled)
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    }
                    using (MultipartFormDataContent content = new())
                    {
                        ByteArrayContent audioContent = new(audio);
                        audioContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/mpeg");
                        content.Add(audioContent, "file", filename);

                        content.Add(new StringContent(model), "model");

#if WINDOWS
                        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
                        string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
                        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                        HttpResponseMessage response = await client.PostAsync($"{serverUrl}{transcriptionEndpoint}", content);
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception($"Failed to transcribe audio: {response.ReasonPhrase}");
                        }

                        string responseJson = await response.Content.ReadAsStringAsync();
                        TranscriptionResponse responseData = JsonSerializer.Deserialize<TranscriptionResponse>(responseJson)!;
                        return responseData.Text;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        
        public static async Task<List<string>> GetFilesListAsync(string apiKey = "sk-xxx", int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true, CancellationToken cancellationToken = default)
        {
            try
            {
                using HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                if (authEnabled)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }

                HttpResponseMessage response = await client.GetAsync($"{serverUrl}{filesEndpoint}", cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }

                string responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                FileListResponse? filesResponse = JsonSerializer.Deserialize<FileListResponse>(responseJson);
                if (filesResponse == null)
                {
                    throw new Exception("Json deserialization failed.");
                }
                List<string> filesList = new List<string>();
                foreach (var file in filesResponse.Data)
                {
                    filesList.Add(file.Id);
                }

                return filesList;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving files: {ex.Message}");
            }
        }
        public static async Task<List<MyAIAPI.FileInformation>> GetFilesAsync(string apiKey = "sk-xxx", int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true, CancellationToken cancellationToken = default)
        {
            try
            {
                using HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                if (authEnabled)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }

                HttpResponseMessage response = await client.GetAsync($"{serverUrl}{filesEndpoint}", cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }

                string responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                FileListResponse? filesResponse = JsonSerializer.Deserialize<FileListResponse>(responseJson);
                if (filesResponse == null)
                {
                    throw new Exception("Json deserialization failed.");
                }
                return filesResponse.Data;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving files: {ex.Message}");
            }
        }
        public static async Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string purpose, string apiKey = "sk-xxx", int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            try
            {
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);

                if (authEnabled)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }
                using MultipartFormDataContent formData = new();
                formData.Add(new StringContent(purpose), "purpose");
                formData.Add(new ByteArrayContent(fileBytes), "file", fileName);

                HttpResponseMessage response = await client.PostAsync($"{serverUrl}{filesEndpoint}", formData);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new Exception($"Status code: {(int)response.StatusCode}\n File may already exist");
                    }
                    else
                    {
                        throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                    }
                }

                string responseJson = await response.Content.ReadAsStringAsync();
                FileInformation responseData = JsonSerializer.Deserialize<FileInformation>(responseJson)!;

                return responseData.Filename;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<MyAIAPI.FileInformation> RetrieveFileDetailsAsyncHttp(string fileId, string apiKey = "sk-xxx", int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            try
            {
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                if (authEnabled)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }
#if WINDOWS
                string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
        string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                HttpResponseMessage response = await client.GetAsync($"{serverUrl}{filesEndpoint}/{fileId}");
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }
                string responseJson = await response.Content.ReadAsStringAsync();
                MyAIAPI.FileInformation fileInformation = JsonSerializer.Deserialize<MyAIAPI.FileInformation>(responseJson)!;
                return fileInformation;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving file details: {ex.Message}");
            }
        }
        public static async Task<string> RetrieveFileContentsAsyncHttp(string fileId, string apiKey = "sk-xxx", int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            try
            {
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                if (authEnabled)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }
#if WINDOWS
                string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
        string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                HttpResponseMessage response = await client.GetAsync($"{serverUrl}{filesEndpoint}/{fileId}/content");
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }
                string fileContent = await response.Content.ReadAsStringAsync();
                return fileContent;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving file contents: {ex.Message}");
            }
        }
        public static async Task DeleteFileAsync(string fileId , string serverUrl = "https://api.openai.com/v1", string apiKey = "sk-xxx", int timeoutInSeconds = 60, bool authEnabled = true)
        {
            try
            {
                // create an HTTP client
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                if (authEnabled)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }
#if WINDOWS
                string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
                string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                HttpResponseMessage response = await client.DeleteAsync($"{serverUrl}{filesEndpoint}/{fileId}");
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }
                Console.WriteLine("File deleted successfully");
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }

        public static async Task<string> GenerateTextAsyncHttp(string prompt, string apiKey = "sk-xxx", string model = "gpt-3.5-turbo", int maxTokens = 2000, int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            prompt = prompt.Trim();
            if (await CheckTextForViolations(prompt, apiKey))
            {
                throw new Exception("This input violates OpenAI moderation checks.");
            }
            //string stopSequence = @"\?\?\?";
            string returnString = string.Empty;
            switch (model)
            {
                case "text-devinci-003":
                    maxTokens = 4000;
                    break;
                case "code-devinci-002":
                    maxTokens = 4000;
                    break;
                case "gpt-3.5-turbo":
                    maxTokens = 4000;
                    break;
                default:
                    break;
            }
            int promptCost = (int)EstimateTokenCost(prompt);
            int remainingTokens = maxTokens - 150 - promptCost;
            if (remainingTokens <= 10)
            {
                throw new Exception("API request too long.\nOpenAI API would fail to respond.\n Please shorten and try again.");
            }

            CompletionRequest payload = new()
            {
                Prompt = prompt.Trim(),
                Model = model,
                Temperature = 0.1,
                MaxTokens = maxTokens,
                TopP = 1.0,
                Frequency_Penalty = 0.6,
                Presence_Penalty = 0.0,
                User = apiKey
            };
            // create an HTTP client
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
            if (authEnabled)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }
#if WINDOWS
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
            string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            string json = JsonSerializer.Serialize(payload);
            StringContent jsonContent = new(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync($"{serverUrl}{completionEndpoint}", jsonContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("API request failed with status code: " + (int)response.StatusCode);
            }
            string responseJson = await response.Content.ReadAsStringAsync();
            CompletionResult responseData = JsonSerializer.Deserialize<CompletionResult>(responseJson)!;
            if (await CheckTextForViolations(responseData!.Choices.First().Text, apiKey))
            {
                throw new Exception("This response Violates OpenAI Moderation checks.");
            }
            returnString += $"Using Model: {responseData.Model}\n";
            returnString += $"Took: {responseData.Usage.Total_Tokens} Total Tokens and {responseData.Usage.Completion_Tokens} Tokens were used for the completion.\n";
            returnString += $"Stop Reason: {responseData.Choices.First().Finish_Reason}\n";
            returnString += $"{responseData.Choices.First().Text}\n\n";
            return returnString;
        } // Depreciated 
        public static async Task<string> EditTextAsyncHttp(string model, string apiKey = "sk-xxx", string instruction = "", string input = "", int maxTokens = 2000, int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            try
            {
                input = input.Trim();
                string returnString = string.Empty;
                if (await CheckTextForViolations(input, apiKey))
                {
                    throw new Exception("This input violates OpenAI moderation checks.");
                }
                int inputCost = (int)EstimateTokenCost(input);
                int remainingTokens = maxTokens - 110 - inputCost;
                if (remainingTokens < (maxTokens / 2))
                {
                    throw new Exception("API request too long.\nOpenAI API would fail to respond.\n Please shorten and try again.");
                }
                TextEditRequest payload = new()
                {
                    Model = model,
                    Input = input,
                    Instruction = instruction
                };
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                if (authEnabled)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }
                string json = JsonSerializer.Serialize(payload);
                StringContent jsonContent = new(json, Encoding.UTF8, "application/json");

#if WINDOWS
                string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
                string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                HttpResponseMessage response = await client.PostAsync($"{serverUrl}{editsEndpoint}", jsonContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("API request failed with status code: " + (int)response.StatusCode);
                }
                string responseJson = await response.Content.ReadAsStringAsync();
                TextEditResponse responseData = JsonSerializer.Deserialize<TextEditResponse>(responseJson)!;
                double tokenPricePer1k = 0.02;
                double totalCost = responseData.Usages.Total_Tokens * (tokenPricePer1k / 1000);
                returnString += $"Took: {responseData.Usages.Total_Tokens} Total Tokens and {responseData.Usages.Completion_Tokens} Tokens were used for the completion.\n";
                returnString += $"Total Translation Cost: ${totalCost} at ${tokenPricePer1k} per 1000 tokens\n";
                returnString += responseData.Choices.First().Text;
                return returnString;
            }
            catch (HttpRequestException ex)
            {
#pragma warning disable CA2200 // Rethrow to preserve stack details
                throw ex;
#pragma warning restore CA2200 // Rethrow to preserve stack details
            }
        } // Depreciated 

        public static async Task<Model> RetrieveModelDetailsAsync(string modelId, string apiKey = "sk-xxx", int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            try
            {
                // create an HTTP client
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                if (authEnabled)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }
#if WINDOWS
                string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
                string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                HttpResponseMessage response = await client.GetAsync($"{serverUrl}{modelEndpoint}/{modelId}");
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }
                string responseJson = await response.Content.ReadAsStringAsync();
                Model responseData = JsonSerializer.Deserialize<Model>(responseJson)!;
                return responseData;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<List<Model>> GetModelsAsync(string apiKey, int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            try
            {
                List<Model> retList = new();
                // create an HTTP client
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                if (authEnabled)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }
#if WINDOWS
                string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
                string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                HttpResponseMessage response = await client.GetAsync($"{serverUrl}{modelEndpoint}");
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }
                string responseJson = await response.Content.ReadAsStringAsync();
                ModelListResponse responseData = JsonSerializer.Deserialize<ModelListResponse>(responseJson)!;
                retList = responseData.Data.ToList<Model>();
                return retList;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<List<Model>> GetModelList(string apiKey, int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            try
            {
                List<Model> models = await GetModelsAsync(apiKey, timeoutInSeconds, serverUrl, authEnabled);
                ConcurrentBag<Model> resultList = new();
                if (models != null)
                {
                    IEnumerable<Task<Model>> tasks = models.Select(model => RetrieveModelDetailsAsync(model.Id, apiKey, timeoutInSeconds, serverUrl, authEnabled));
                    Model[] details = await Task.WhenAll(tasks);

                    foreach (Model modelDetails in details)
                    {
                        if (modelDetails.Permissions.Any(p => p.AllowView))
                        {
                            resultList.Add(modelDetails);
                        }
                    }
                }

                return resultList.ToList();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur while trying to retrieve the models
                throw new Exception(ex.Message);
            }
        }
        public static async Task<List<string>> GetModelNameList(string apiKey, int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            List<string> models = new();
            try
            {
                List<Model> result = await GetModelList(apiKey, timeoutInSeconds, serverUrl, authEnabled);
                if (result != null)
                {
                    await Task.WhenAll(result.Select(async model =>
                    {
                        if (!models.Contains(model.Id))
                        {
                            models.Add(model.Id);
                            await Task.Delay(0);
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return models ?? new();
        }


        #region TypeClasses
        public class Message
        {
            [JsonPropertyName("role")]
            public string Role { get; set; } = string.Empty;
            [JsonPropertyName("content")]
            public string Content { get; set; } = string.Empty;
        }
        public class TextGenerationRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = "gpt-3.5-turbo";

            [JsonPropertyName("messages")]
            public List<Message> Messages { get; set; } = new();

            [JsonPropertyName("temperature")]
            public double Temperature { get; set; } = 1.0;

            [JsonPropertyName("top_p")]
            public double TopP { get; set; } = 1.0;

            [JsonPropertyName("n")]
            public int NumOfResponse { get; set; } = 1;

            [JsonPropertyName("stream")]
            public bool Stream { get; set; } = false;

            [JsonPropertyName("stop")]
            public List<string> Stop { get; set; } = new();

            [JsonPropertyName("max_tokens")]
            public int MaxTokens { get; set; } = 4096;

            [JsonPropertyName("presence_penalty")]
            public double PresencePenalty { get; set; } = 0.0;

            [JsonPropertyName("frequency_penalty")]
            public double FrequencyPenalty { get; set; } = 0.0;

            [JsonPropertyName("logit_bias")]
            public Dictionary<string, int>? LogitBias { get; set; } = null;

            [JsonPropertyName("logprobs")]
            public bool? Logprobs { get; set; } = false;

            [JsonPropertyName("top_logprobs")]
            public int TopLogprobs { get; set; } = int.MinValue;

            [JsonPropertyName("seed")]
            public int Seed { get; set; } = -1;

            [JsonPropertyName("tools")]
            public List<string> Tools { get; set; } = new();
        }
        public class LocalAiTextGenerationRequest : TextGenerationRequest
        {
            [JsonPropertyName("grammar")]
            public string Grammar { get; set; } = "root ::= (\"yes\" | \"no\")";
        }
        public class TextGenerationChoice
        {
            [JsonPropertyName("index")]
            public int Index { get; set; }
            [JsonPropertyName("message")]
            public Message Message { get; set; } = new();
            [JsonPropertyName("finish_reason")]
            public string FinishReason { get; set; } = string.Empty;
        }
        public class TextGenerationResult
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;
            [JsonPropertyName("object")]
            public string Object { get; set; } = string.Empty;
            [JsonPropertyName("created")]
            public long Created { get; set; }
            [JsonPropertyName("choices")]
            public List<TextGenerationChoice> Choices { get; set; } = new();
            [JsonPropertyName("usage")]
            public Usage Usage { get; set; } = new();
        }
        public class Usage
        {
            [JsonPropertyName("prompt_tokens")]
            public int Prompt_Tokens { get; set; }
            [JsonPropertyName("completion_tokens")]
            public int Completion_Tokens { get; set; }
            [JsonPropertyName("total_tokens")]
            public int Total_Tokens { get; set; }
        }

        public class OpenAiTextGenerationChunk : TextGenerationChunk
        {
            [JsonPropertyName("system_fingerprint")]
            public string SystemFingerprint { get; set; } = string.Empty;
        }
        public class OpenAiStreamChoice : StreamChoice
        {

            [JsonPropertyName("logprobs")]
            public object? Logprobs { get; set; } = null;

            [JsonPropertyName("finish_reason")]
            public object? FinishReason { get; set; } = null;
        }

        public class MultiModalMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; set; } = string.Empty;
            [JsonPropertyName("content")]
            public List<MultiModalContent> Content { get; set; } = new();
        }
        public class MultiModalContent
        {
            [JsonPropertyName("type")]
            public string Type { get; set; } = string.Empty;
        }
        public class MultiModalTextContent : MultiModalContent
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;
        }
        public class MultiModalImageContent : MultiModalContent
        {
            [JsonPropertyName("image_url")]
            public ImageUrl ImageUrl { get; set; } = new();
        }
        public class MultiModalTextGenerationRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("messages")]
            public List<MultiModalMessage> Messages { get; set; } = new();
        }
        public class MultiModalTextGenerationResult
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;
            [JsonPropertyName("object")]
            public string Object { get; set; } = string.Empty;
            [JsonPropertyName("created")]
            public long Created { get; set; }
            [JsonPropertyName("choices")]
            public List<TextGenerationChoice> Choices { get; set; } = new();
            [JsonPropertyName("usage")]
            public Usage Usage { get; set; } = new();
        }
        public class ImageUrl
        {
            [JsonPropertyName("url")]
            public string Url { get; set; } = string.Empty;
        }
        public class ImageUrl2 : ImageUrl
        {
            [JsonPropertyName("detail")]
            public string? Detail { get; set; }
        }
        public class EmbeddingRequest
        {
            [JsonPropertyName("input")]
            public string Input { get; set; } = string.Empty;

            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;
        }

        public class EmbeddingResponse
        {
            [JsonPropertyName("data")]
            public List<EmbeddingData> Data { get; set; } = new();
        }

        public class EmbeddingData
        {
            [JsonPropertyName("embedding")]
            public List<double> Embedding { get; set; } = new();
        }

        public class TextEditRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;
            [JsonPropertyName("input")]
            public string Input { get; set; } = string.Empty;
            [JsonPropertyName("instruction")]
            public string Instruction { get; set; } = string.Empty;
            [JsonPropertyName("n")]
            public int N { get; set; } = 1;
            [JsonPropertyName("temperature")]
            public double Temperature { get; set; } = 0;
            [JsonPropertyName("top_p")]
            public double TopP { get; set; } = 1;
        }
        public class TextEditResponse
        {
            [JsonPropertyName("object")]
            public object Object { get; set; } = new();
            [JsonPropertyName("created")]
            public long Created { get; set; }
            [JsonPropertyName("choices")]
            public List<TextEditChoice> Choices { get; set; } = new();
            [JsonPropertyName("usage")]
            public Usage Usages { get; set; } = new();
        }
        public class TextEditChoice
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;
            [JsonPropertyName("index")]
            public int Index { get; set; }
        }
        public class Choice
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;
            [JsonPropertyName("index")]
            public int Index { get; set; }
            [JsonPropertyName("logprobs")]
            public object Logprobs { get; set; } = new();
            [JsonPropertyName("finish_reason")]
            public string Finish_Reason { get; set; } = string.Empty;
        }
       
        public class ObjectsInImg
        {
            [JsonPropertyName("bbox")]
            public List<int> Bbox { get; set; } = new();
            [JsonPropertyName("object_id")]
            public object ObjectId { get; set; } = new();
        }
        public class TextViolationCheckRequest
        {
            [JsonPropertyName("input")]
            public string Input { get; set; } = string.Empty;
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;
            public TextViolationCheckRequest() { }
        }
        public class TextModerationResponse
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;
            [JsonPropertyName("results")]
            public List<ModResult> Results { get; set; } = new();
            public class ModResult
            {
                [JsonPropertyName("categories")]
                public Categories CategoriesBool { get; set; } = new();
                [JsonPropertyName("category_scores")]
                public CategoryScores Category_Scores { get; set; } = new();
                [JsonPropertyName("flagged")]
                public bool Flagged { get; set; }
                public class Categories
                {
                    [JsonPropertyName("hate")]
                    public bool Hate { get; set; }
                    [JsonPropertyName("hate/threatening")]
                    public bool Hate_Threatening { get; set; }
                    [JsonPropertyName("self-harm")]
                    public bool Self_Harm { get; set; }
                    [JsonPropertyName("sexual")]
                    public bool Sexual { get; set; }
                    [JsonPropertyName("sexual/minors")]
                    public bool Sexual_Minors { get; set; }
                    [JsonPropertyName("violence")]
                    public bool Violence { get; set; }
                    [JsonPropertyName("violence/graphic")]
                    public bool Violence_Graphic { get; set; }
                }
                public class CategoryScores
                {
                    [JsonPropertyName("hate")]
                    public double Hate { get; set; }
                    [JsonPropertyName("hate/threatening")]
                    public double Hate_Threatening { get; set; }
                    [JsonPropertyName("self_harm")]
                    public double Self_Harm { get; set; }
                    [JsonPropertyName("sexual")]
                    public double Sexual { get; set; }
                    [JsonPropertyName("sexual_minors")]
                    public double Sexual_Minors { get; set; }
                    [JsonPropertyName("violence")]
                    public double Violence { get; set; }
                    [JsonPropertyName("violence_graphic")]
                    public double Violence_Graphic { get; set; }
                }
            }
        }
        public class SpeechRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = "tts-1";
            [JsonPropertyName("voice")]
            public string Voice { get; set; } = "onyx";
            [JsonPropertyName("input")]
            public string Input { get; set; } = string.Empty;
        }
        public class SpeechRequestOptional : SpeechRequest
        {
            [JsonPropertyName("response_format")]
            public string ResponseFormat { get; set; } = "mp3";
            [JsonPropertyName("speed")]
            public double Speed { get; set; } = 1.0;
        }
        public class ImageGenerationRequest
        {
            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;

            [JsonPropertyName("model")]
            public string Model { get; set; } = "dall-e-2";

            [JsonPropertyName("n")]
            public int N { get; set; } = 1;

            [JsonPropertyName("quality")]
            public string Quality { get; set; } = "standard";

            [JsonPropertyName("response_format")]
            public string ResponseFormat { get; set; } = "b64_json"; // "url"

            [JsonPropertyName("size")]
            public string Size { get; set; } = "512x512";

            [JsonPropertyName("style")]
            public string Style { get; set; } = "vivid";

            [JsonPropertyName("user")]
            public string User { get; set; } = string.Empty;

            public ImageGenerationRequest() { }

            public ImageGenerationRequest(string prompt, int n, string size)
            {
                Prompt = prompt;
                N = n;
                Size = size;
            }
        }

        public class ImageEditRequest
        {
            [JsonPropertyName("image_url")]
            public string Image { get; set; } = string.Empty;

            [JsonPropertyName("mask")]
            public string Mask { get; set; } = string.Empty;

            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;

            [JsonPropertyName("model")]
            public string Model { get; set; } = "dall-e-2";

            [JsonPropertyName("n")]
            public int N { get; set; } = 1;

            [JsonPropertyName("size")]
            public string Size { get; set; } = "1024x1024";

            [JsonPropertyName("response_format")]
            public string ResponseFormat { get; set; } = "url";

            [JsonPropertyName("user")]
            public string User { get; set; } = string.Empty;
        }
        public class ImageVariationRequest
        {
            [JsonPropertyName("image")]
            public byte[] Image { get; set; } = Array.Empty<byte>();

            [JsonPropertyName("model")]
            public string Model { get; set; } = "dall-e-2";

            [JsonPropertyName("n")]
            public int N { get; set; } = 1;

            [JsonPropertyName("response_format")]
            public string ResponseFormat { get; set; } = "url";

            [JsonPropertyName("size")]
            public string Size { get; set; } = "1024x1024";

            [JsonPropertyName("user")]
            public string User { get; set; } = "";

            // Additional options from documentation with default values

            [JsonPropertyName("file")]
            public byte[] File { get; set; } = Array.Empty<byte>();

            public ImageVariationRequest()
            {
            }

            public ImageVariationRequest(byte[] image)
            {
                Image = image;
            }

            // Additional constructors if needed for other properties
        }
        public class ImageUrlResponse
        {
            [JsonPropertyName("created")]
            public long Created { get; set; }

            [JsonPropertyName("data")]
            public List<ImageUrlData> Data { get; set; } = new();
        }

        public class ImageUrlData
        {
            [JsonPropertyName("url")]
            public string Url { get; set; } = string.Empty;
        }

        public class ImageJsonResponse
        {
            [JsonPropertyName("created")]
            public long Created { get; set; }

            [JsonPropertyName("data")]
            public List<B64JsonData> Data { get; set; } = new();
        }

        public class B64JsonData
        {
            [JsonPropertyName("b64_json")]
            public string B64Json { get; set; } = string.Empty;
        }
        public class ModelListResponse
        {
            [JsonPropertyName("list")]
            public string List { get; set; } = string.Empty;
            [JsonPropertyName("data")]
            public List<Model> Data { get; set; } = new();
        }
        public class Model
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;
            [JsonPropertyName("object")]
            public object Object { get; set; } = string.Empty;
            [JsonPropertyName("owned_by")]
            public string OwnedBy { get; set; } = string.Empty;
            [JsonPropertyName("permission")]
            public List<Permission> Permissions { get; set; } = new();
        }
        public class Permission
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;
            [JsonPropertyName("object")]
            public object Object { get; set; } = string.Empty;
            [JsonPropertyName("created")]
            public int Created { get; set; }
            [JsonPropertyName("allow_create_engine")]
            public bool AllowCreateEngine { get; set; }
            [JsonPropertyName("allow_sampling")]
            public bool AllowSampling { get; set; }
            [JsonPropertyName("allow_logprobs")]
            public bool AllowLogprobs { get; set; }
            [JsonPropertyName("allow_search_indices")]
            public bool AllowSearchIndices { get; set; }
            [JsonPropertyName("allow_view")]
            public bool AllowView { get; set; }
            [JsonPropertyName("allow_fine_tuning")]
            public bool AllowFineTuning { get; set; }
            [JsonPropertyName("organization")]
            public string Organization { get; set; } = string.Empty;
            [JsonPropertyName("group")]
            public object Group { get; set; } = string.Empty;
            [JsonPropertyName("is_blocking")]
            public bool IsBlocking { get; set; }
        }
        public class SpeechGenerationRequest
        {
            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;
            [JsonPropertyName("format")]
            public string Format { get; set; } = string.Empty;
            [JsonPropertyName("max_tokens")]
            public int Max_Tokens { get; set; }
            [JsonPropertyName("stop")]
            public string Stop { get; set; } = string.Empty;

            public SpeechGenerationRequest()
            {
                Stop = "\"\"\"";
            }
        }

        public class FileListResponse
        {
            [JsonPropertyName("data")]
            public List<FileInformation> Data { get; set; } = new();
        }
        public class FileUploadRequest
        {
            [JsonPropertyName("purpose")]
            public string Purpose { get; set; } = string.Empty;
            [JsonPropertyName("file")]
            public string File { get; set; } = string.Empty;
        }
        public class FileInformation
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;
            [JsonPropertyName("object")]
            public object Object { get; set; } = string.Empty;
            [JsonPropertyName("bytes")]
            public int Bytes { get; set; }
            [JsonPropertyName("created_at")]
            public int CreatedAt { get; set; }
            [JsonPropertyName("filename")]
            public string Filename { get; set; } = string.Empty;
            [JsonPropertyName("purpose")]
            public string Purpose { get; set; } = string.Empty;
        }
        public class FileDeleteResponse
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;
            [JsonPropertyName("object")]
            public string File { get; set; } = string.Empty;
            [JsonPropertyName("deleted")]
            public bool Deleted { get; set; }
        }
#pragma warning disable IDE0290 // Use primary constructor
        public class FileObject
        {
            [JsonPropertyName("file")]
            public byte[] File { get; set; }
            public FileObject(byte[] file)
            {
                File = file;
            }
        }
#pragma warning restore IDE0290 // Use primary constructor
        public class TranscriptionRequest
        {
            [JsonPropertyName("file")]
            public string File { get; set; } = string.Empty;
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;
            public TranscriptionRequest()
            {
            }
        }
        public class TranscriptionResponse
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;
            public TranscriptionResponse()
            {
            }
        }
        public class CompletionRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;
            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;
            [JsonPropertyName("suffix")]
            public string Suffix { get; set; } = string.Empty;
            [JsonPropertyName("max_tokens")]
            public int MaxTokens { get; set; }
            [JsonPropertyName("temperature")]
            public double Temperature { get; set; } = 0;
            [JsonPropertyName("top_p")]
            public double TopP { get; set; } = 1;
            [JsonPropertyName("n")]
            public int N { get; set; } = 1;
            [JsonPropertyName("stream")]
            public bool Stream { get; set; } = false;
            [JsonPropertyName("logprobs")]
            public object Logprobs { get; set; } = new();
            [JsonPropertyName("echo")]
            public bool Echo { get; set; } = false;
            [JsonPropertyName("stop")]
            public object Stop { get; set; } = $"`?`?";
            [JsonPropertyName("presence_penalty")]
            public double Presence_Penalty { get; set; } = 0;
            [JsonPropertyName("frequency_penalty")]
            public double Frequency_Penalty { get; set; } = 0;
            [JsonPropertyName("best_of")]
            public int Best_Of { get; set; } = 1;
            //[JsonPropertyName("logit_bias")]
            //public object Logit_Bias { get; set; } = null;
            [JsonPropertyName("user")]
            public string User { get; set; } = string.Empty;
            public CompletionRequest()
            {
            }
        }
        public class CompletionResult
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;
            [JsonPropertyName("object")]
            public object Object { get; set; } = new();
            [JsonPropertyName("created")]
            public long Created { get; set; }
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;
            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; } = new();
            [JsonPropertyName("usage")]
            public Usage Usage { get; set; } = new();
            public CompletionResult()
            {
            }
        }

        public class FineTuningRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("training_file")]
            public string TrainingFile { get; set; } = string.Empty;
        }
        public class FineTuningRequestFull : FineTuningRequest
        {
            [JsonPropertyName("hyperparameters")]
            public Hyperparameters Hyperparameters { get; set; } = new Hyperparameters();

            [JsonPropertyName("suffix")]
            public string? Suffix { get; set; } = null;

            [JsonPropertyName("validation_file")]
            public string? ValidationFile { get; set; } = null;
        }
        public class Hyperparameters
        {
            [JsonPropertyName("batch_size")]
            public int BatchSize { get; set; } = int.MinValue;

            [JsonPropertyName("learning_rate_multiplier")]
            public int LearningRateMultiplier { get; set; } = int.MinValue;

            [JsonPropertyName("n_epochs")]
            public int NEpochs { get; set; } = int.MinValue;


        }
        public class FineTuningJobResponse
        {
            [JsonPropertyName("object")]
            public string Object { get; set; } = string.Empty;

            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("created_at")]
            public long CreatedAt { get; set; } = long.MinValue;

            [JsonPropertyName("fine_tuned_model")]
            public string? FineTunedModel { get; set; } = null;

            [JsonPropertyName("organization_id")]
            public string OrganizationId { get; set; } = string.Empty;

            [JsonPropertyName("result_files")]
            public List<string> ResultFiles { get; set; } = new List<string>();

            [JsonPropertyName("status")]
            public string Status { get; set; } = string.Empty;

            [JsonPropertyName("validation_file")]
            public string? ValidationFile { get; set; } = null;

            [JsonPropertyName("training_file")]
            public string TrainingFile { get; set; } = string.Empty;
        }
        public class FineTuningJobResponseWithError
        {
            [JsonPropertyName("object")]
            public string Object { get; set; } = "fine_tuning.job";

            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("created_at")]
            public long CreatedAt { get; set; } = long.MinValue;

            [JsonPropertyName("error")]
            public ErrorDetails? Error { get; set; }

            [JsonPropertyName("fine_tuned_model")]
            public string? FineTunedModel { get; set; }

            [JsonPropertyName("organization_id")]
            public string OrganizationId { get; set; } = string.Empty;

            [JsonPropertyName("result_files")]
            public List<string> ResultFiles { get; set; } = new List<string>();

            [JsonPropertyName("status")]
            public string Status { get; set; } = string.Empty;

            [JsonPropertyName("trained_tokens")]
            public int? TrainedTokens { get; set; }

            [JsonPropertyName("training_file")]
            public string TrainingFile { get; set; } = string.Empty;

            [JsonPropertyName("validation_file")]
            public string? ValidationFile { get; set; }

            public class ErrorDetails
            {
                [JsonPropertyName("code")]
                public string Code { get; set; } = string.Empty;

                [JsonPropertyName("message")]
                public string Message { get; set; } = string.Empty;

                [JsonPropertyName("param")]
                public string? Parameter { get; set; }
            }
        }
        public class FineTuningEvent
        {
            [JsonPropertyName("id")]
            public string ID { get; set; } = string.Empty;

            [JsonPropertyName("created_at")]
            public int CreatedAt { get; set; } = int.MinValue;

            [JsonPropertyName("level")]
            public string Level { get; set; } = string.Empty;

            [JsonPropertyName("message")]
            public string Message { get; set; } = string.Empty;

            [JsonPropertyName("object")]
            public string Object { get; set; } = string.Empty;
        }
        #endregion
        #endregion
        #region LocalAi
        public static async Task<string> GenerateLocalAiImageAsyncHttp(string prompt, string apiKey = "sk-xxx", string model = "stablediffusionaccelerated", string size = "1024x1024", string image = "", int numOfImages = 1, int step = 20, string quality = "standard", string style = "vivid", int cfgScale = 12, string scheduler = "euler_a", string responseFormat = "url", int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            try
            {
                prompt = prompt.Trim();
                string json = string.Empty;
                if (!string.IsNullOrWhiteSpace(image))
                {
                    LocalAiImg2ImgGenerationRequest payload = new()
                    {
                        Prompt = prompt,
                        File = image,
                        Model = model,
                        Size = size,
                        Step = step,
                        N = numOfImages,
                        Style = style,
                        Quality = quality,
                        SchedulerType = scheduler,
                        CfgScale = cfgScale,
                        User = apiKey
                    };
                    json = JsonSerializer.Serialize(payload);
                }
                else
                {
                    LocalAiImageGenerationRequest payload = new()
                    {
                        Prompt = prompt,
                        Model = model,
                        Size = size,
                        Step = step,
                        N = numOfImages,
                        Style = style,
                        Quality = quality,
                        SchedulerType = scheduler,
                        CfgScale = cfgScale,
                        User = apiKey
                    };
                    json = JsonSerializer.Serialize(payload);
                }
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                if (authEnabled)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }
                StringContent jsonContent = new(json, Encoding.UTF8, "application/json");
#if WINDOWS
                string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
                string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                HttpResponseMessage response = await client.PostAsync($"{serverUrl}{imageGenerationEndpoint}", jsonContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }
                string responseJson = await response.Content.ReadAsStringAsync();
                ImageUrlResponse responseData = JsonSerializer.Deserialize<ImageUrlResponse>(responseJson)!;
                return responseData.Data.First().Url;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<string> GenerateLocalAiText2VideoAsyncHttp(string prompt, string apiKey = "sk-xxx", string model = "stablediffusionaccelerated", string size = "1024x1024", string image = "", int numOfImages = 1, int step = 20, int cfgScale = 12, string responseFormat = "url", int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            try
            {
                prompt = prompt.Trim();
                var payload = new LocalAiText2VideoGenerationRequest
                {
                    Prompt = prompt,
                    Model = model,
                    File = image,
                    Size = size
                };

                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                if (authEnabled)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }
                string json = JsonSerializer.Serialize(payload);
                StringContent jsonContent = new(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync($"{serverUrl}{imageGenerationEndpoint}", jsonContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }
                string responseJson = await response.Content.ReadAsStringAsync();
                ImageUrlResponse responseData = JsonSerializer.Deserialize<ImageUrlResponse>(responseJson)!;
                return responseData.Data.First().Url;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<byte[]> GenerateLocalAiTTS(string prompt, string apiKey = "sk-xxx", string backend = "", string model = "cloned-voice", string responseFormat = "url", int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(prompt))
                {
                    throw new Exception($"API request failed: Prompt is Null or EMPTY.");
                }
                if (authEnabled && string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception($"API request failed: API Key is Null or EMPTY.");
                }
                prompt = prompt.Trim();

                string json = string.Empty;
                if (!string.IsNullOrWhiteSpace(backend))
                {
                    TtsRequestFull payload = new()
                    {
                        Model = model,
                        Backend = backend,
                        Input = prompt
                    };
                    json = JsonSerializer.Serialize(payload);
                }
                else
                {
                    TtsRequest payload = new()
                    {
                        Model = model,
                        Input = prompt
                    };
                    json = JsonSerializer.Serialize(payload);
                }
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                if (authEnabled)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }
                StringContent jsonContent = new(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync($"{serverUrl.Replace("/v1", "")}{AudioGenerationEndpoint}", jsonContent, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }
                byte[] responseBytes = await response.Content.ReadAsByteArrayAsync(CancellationToken.None);
                //UrlResponse responseData = JsonSerializer.Deserialize<ImageUrlResponse>(responseJson)!;
                return responseBytes;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public static async Task<List<LocalAiModel>?> GetLocalAiModelsAsync(string apiKey = "sk-xxx", int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            try
            {
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);

                if (authEnabled)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }
#if WINDOWS
                string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Safari/537.36";
#elif ANDROID
                string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.6029.110 Mobile Safari/537.36";
#endif
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                HttpResponseMessage response = await client.GetAsync($"{serverUrl}{modelEndpoint}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status code: {(int)response.StatusCode}");
                }

                string responseJson = await response.Content.ReadAsStringAsync();
                JsonSerializerOptions options = JsonOptions;
                LocalAiModelListResponse? apiResponse = JsonSerializer.Deserialize<LocalAiModelListResponse>(responseJson, options);

                if (apiResponse != null && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Connection failure: {ex.Message}");
            }
        }
        public static async Task<List<string>> GetLocalAiModelNameList(string apiKey = "sk-xxx", int timeoutInSeconds = 60, string serverUrl = "https://api.openai.com/v1", bool authEnabled = true)
        {
            List<LocalAiModel>? result = await GetLocalAiModelsAsync(apiKey, timeoutInSeconds, serverUrl, authEnabled);
            if (result != null)
            {
                List<string> modelNames = result
                    .Where(model => !model.Id!.EndsWith(".py") && !model.Id.EndsWith(".bin") && !model.Id.EndsWith(".gguf") && !model.Id.EndsWith(".onnx"))
                    .Select(model => model.Id)
                    .ToList()!;

                modelNames.Sort(); // Sort the list alphabetically
                return modelNames;
            }
            return new List<string>() { "Not Found!" };
        }
        #region TypeClasses
        public class TtsRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = "cloned-voice";
            [JsonPropertyName("input")]
            public string Input { get; set; } = string.Empty;
        }
        public class TtsRequestFull : TtsRequest
        {
            [JsonPropertyName("backend")]
            public string Backend { get; set; } = string.Empty;
        }
        public class LocalAiModel
        {
            public string? Id { get; set; }
            public string? Object { get; set; }
        }

        public class LocalAiModelListResponse
        {
            public string? Object { get; set; }
            public List<LocalAiModel>? Data { get; set; }
        }
        public class LocalAiImageGenerationRequest
        {
            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;

            [JsonPropertyName("image")]
            public string Image { get; set; } = string.Empty;

            [JsonPropertyName("n")]
            public int N { get; set; } = 1;

            [JsonPropertyName("step")]
            public int Step { get; set; } = 20;

            [JsonPropertyName("size")]
            public string Size { get; set; } = "1024x1024";

            [JsonPropertyName("quality")]
            public string Quality { get; set; } = "standard";

            [JsonPropertyName("style")]
            public string Style { get; set; } = "vivid";

            [JsonPropertyName("model")]
            public string Model { get; set; } = "stablediffusionaccelerated";

            [JsonPropertyName("scheduler_type")]
            public string SchedulerType { get; set; } = "euler_a";

            [JsonPropertyName("cfg_scale")]
            public int CfgScale { get; set; } = 12;

            [JsonPropertyName("user")]
            public string User { get; set; } = string.Empty;
        }
        public class LocalAiImg2ImgGenerationRequest : LocalAiImageGenerationRequest
        {
            [JsonPropertyName("file")]
            public string File { get; set; } = string.Empty;
        }
        public class LocalAiText2VideoGenerationRequest
        {
            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;

            [JsonPropertyName("file")]
            public string File { get; set; } = string.Empty;

            [JsonPropertyName("step")]
            public int Step { get; set; } = 20;

            [JsonPropertyName("size")]
            public string Size { get; set; } = "1024x1024";

            [JsonPropertyName("model")]
            public string Model { get; set; } = "stablediffusionaccelerated";

            [JsonPropertyName("scheduler_type")]
            public string SchedulerType { get; set; } = "euler_a";

            [JsonPropertyName("cfg_scale")]
            public int CfgScale { get; set; } = 12;

            [JsonPropertyName("user")]
            public string User { get; set; } = string.Empty;
        }
        #endregion
        #endregion
    }
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning restore IDE0028 // Simplify collection initialization
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning restore IDE0300 // Simplify collection initialization
#pragma warning restore IDE0301 // Simplify collection initialization
#pragma warning restore IDE0305 // Simplify collection initialization
#pragma warning restore IDE0600 // Simplify collection initialization
#pragma warning restore IDE0601 // Simplify collection initialization
#pragma warning restore IDE0605 // Simplify collection initialization
#pragma warning restore IDE0079 // Remove unnecessary suppression
}