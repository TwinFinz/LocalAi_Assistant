using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Maui.Media;
using System;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Maui;
#if WINDOWS
using System.Speech.Synthesis;
using Windows.Media.Core;
using System.Text;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
#elif ANDROID
using Android.Media;
using Android.Content.Res;
using Android.Speech.Tts;
using System.IO;
using System.Threading.Tasks;
using Android.App;
#endif

namespace LocalAiAssistant.Utilities
{
    public partial class MyAudioUtils
    {
#pragma warning disable IDE0028 // Remove unused private members
#pragma warning disable IDE0305 // Remove unused private members
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0051 // Remove unused private members
        private readonly static ISpeechToText speechToText = SpeechToText.Default;
        private static Timer? silenceTimer;
        private static bool silenceDetected = false;
        private readonly static string SaveFolder = "OBS"; // Folder name to store the text file to use in OBS
        private readonly static int silenceTimeoutMilliseconds = 2000;
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0079 // Remove unnecessary suppression
#if WINDOWS

        public static async Task PlayAudioAsync(byte[] audioData, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var mediaPlayer = new MediaPlayer())
                using (var memoryStream = new MemoryStream(audioData))
                {
                    MediaSource mediaSource = MediaSource.CreateFromStream(memoryStream.AsRandomAccessStream(), "audio/wav");
                    mediaPlayer.Source = mediaSource;
                    var playbackCompletion = new TaskCompletionSource<bool>();
                    mediaPlayer.MediaEnded += (sender, args) => playbackCompletion.TrySetResult(true);
                    mediaPlayer.Play();
                    await Task.WhenAny(playbackCompletion.Task, Task.Delay(-1, cancellationToken));
                    mediaPlayer.Pause();
                }
            }
            catch (OperationCanceledException)
            {
                await Task.FromCanceled(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing audio: {ex.Message}");
            }
        }
        public static async Task<Task> SpeakText(string text, CancellationToken cancellationToken = default)
        {
            try
            {
                System.Speech.Synthesis.SpeechSynthesizer synthesizer = new();
                synthesizer.SelectVoiceByHints(System.Speech.Synthesis.VoiceGender.Female, VoiceAge.Senior); // You can customize the voice selection as per your requirements
                List<string> sentences = SplitIntoNaturalChunks(text);
                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SaveFolder);
                Directory.CreateDirectory(folderPath);
                string fileName = $"DisplayText.txt";
                string filePath = Path.Combine(folderPath, fileName);
                foreach (string sentence in sentences)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        File.WriteAllText(filePath, string.Empty);
                        return Task.FromCanceled(cancellationToken);
                    }
                    File.WriteAllText(filePath, sentence);
                    await Task.Delay(350, cancellationToken); // Fix Speech/Loop Sync
                    synthesizer.Speak(sentence);
                    File.WriteAllText(filePath, string.Empty);
                }
                return Task.CompletedTask;
            }
            catch (OperationCanceledException)
            {
                return Task.FromCanceled(cancellationToken);
            }
        }
        public static async Task<Task> SpeakTextWinRT(string text, CancellationToken cancellationToken = default)
        {
            try
            {
                _ = SaveFolder;
                Windows.Media.SpeechSynthesis.SpeechSynthesizer synthesizer = new();
                IReadOnlyList<VoiceInformation> voices = Windows.Media.SpeechSynthesis.SpeechSynthesizer.AllVoices;
                List<string> sentences = SplitIntoNaturalChunks(text);
                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SaveFolder);
                Directory.CreateDirectory(folderPath);
                await ApplicationData.Current.LocalFolder.CreateFolderAsync(SaveFolder, CreationCollisionOption.OpenIfExists);
                string fileName = $"DisplayText.txt";
                string filePath = Path.Combine(folderPath, fileName);

                using (var mediaPlayer = new MediaPlayer())
                {
                    foreach (string sentence in sentences)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            await FileIO.WriteTextAsync(await StorageFile.GetFileFromPathAsync(filePath), string.Empty);
                            return Task.FromCanceled(cancellationToken);
                        }
                        await FileIO.WriteTextAsync(await StorageFile.GetFileFromPathAsync(filePath), sentence);
                        SpeechSynthesisStream stream = await synthesizer.SynthesizeTextToStreamAsync(sentence);
                        var mediaSource = MediaSource.CreateFromStream(stream, stream.ContentType);
                        mediaPlayer.Source = mediaSource;
                        var playbackCompletion = new TaskCompletionSource<bool>();
                        mediaPlayer.MediaEnded += (sender, args) => playbackCompletion.TrySetResult(true);
                        mediaPlayer.Play();
                        await playbackCompletion.Task; // Wait until playback completes
                        mediaPlayer.Pause();
                        await FileIO.WriteTextAsync(await StorageFile.GetFileFromPathAsync(filePath), string.Empty);
                    }
                }
                return Task.CompletedTask;
            }
            catch (OperationCanceledException)
            {
                return Task.FromCanceled(cancellationToken);
            }
        }
        public static async Task<byte[]?> SynthesizeTextToAudioAsync(TtsRequest request, int timeoutInSeconds = 20)
        {
            if (string.IsNullOrEmpty(request.Text))
            {
                return null;
            }
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                var queryString = $"text={Uri.EscapeDataString(request.Text)}" +
                                  $"&speaker_id={request.SpeakerIdx}" +
                                  $"&language_id={request.LanguageIdx}";
                if (!string.IsNullOrWhiteSpace(request.StyleWav))
                {
                    queryString += $"&mood={Uri.EscapeDataString(request.StyleWav)}";
                }
                if (!string.IsNullOrWhiteSpace(request.AudioFormat))
                {
                    queryString += $"&audio_format={request.AudioFormat}";
                }                
                var response = await client.GetAsync($"{request.ServerUrl}/api/tts?{queryString}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    await MyMultiPlatformUtils.WriteToLog($"Synthesizer Error: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                await MyMultiPlatformUtils.WriteToLog($"An error occurred: {ex.Message}");
                return null;
            }
        }
        public static async Task SpeakTextUsingCoquiTts(string text, CancellationToken cancellationToken = default)
        {
            try
            {
                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SaveFolder);
                Directory.CreateDirectory(folderPath);
                string fileName = $"DisplayText.txt";
                string filePath = Path.Combine(folderPath, fileName);
                Queue<string> voiceChunks = new(SplitIntoNaturalChunks(text));
                Queue<(string Text, byte[]? AudioData)> speechChunks = new();
                TtsRequest request = new();
                byte[]? audioData;
                bool processingQueue = false;

                async Task QueueLoop()
                {
                    processingQueue = true;
                    while (voiceChunks.TryDequeue(out var chunk))
                    {
                        request.Text = chunk;
                        audioData = await SynthesizeTextToAudioAsync(request);
                        if (audioData == null)
                        {
                            request.ServerUrl = "http://192.168.0.106:5002";
                            audioData = await SynthesizeTextToAudioAsync(request);
                        }
                        speechChunks.Enqueue((chunk, audioData));
                    }
                    processingQueue = false;
                }
                async Task SpeechLoop()
                {
                    while (true)
                    {
                        while (speechChunks.TryDequeue(out var chunk))
                        {
                            File.WriteAllText(filePath, chunk.Text);
                            await Task.Delay(1000, cancellationToken);
                            if (chunk.AudioData != null)
                            {
                                await PlayAudioAsync(chunk.AudioData, cancellationToken);
                            }
                            else
                            {
                                await SpeakText(chunk.Text, cancellationToken);
                            }
                            File.WriteAllText(filePath, string.Empty);
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }
                        }
                        await Task.Delay(10, cancellationToken);
                        if (!processingQueue && speechChunks.Count < 1)
                        {
                            break;
                        }
                    }
                }
                Task queueTask = Task.Run(QueueLoop, cancellationToken);
                Task speechTask = Task.Run(SpeechLoop, cancellationToken);
                await Task.WhenAll(queueTask, speechTask);
            }
            catch (OperationCanceledException)
            {
                await Task.FromCanceled(cancellationToken);
            }
        }
        public static async Task SpeakTextUsingLocalAiTts(string text, string serverUrl = "", bool enableAuth = false, CancellationToken cancellationToken = default)
        {
            try
            {
                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SaveFolder);
                Directory.CreateDirectory(folderPath);
                string fileName = $"DisplayText.txt";
                string filePath = Path.Combine(folderPath, fileName);
                Queue<string> voiceChunks = new(SplitIntoNaturalChunks(text));
                Queue<(string Text, byte[]? AudioData)> speechChunks = new();
                TtsRequest request = new();
                byte[]? audioData;
                bool processingQueue = false;

                async Task QueueLoop()
                {
                    processingQueue = true;
                    while (voiceChunks.TryDequeue(out var chunk))
                    {
                        request.Text = chunk;
                        audioData = await SynthesizeTextToAudioAsync(request);
                        if (audioData == null)
                        {
                            request.ServerUrl = "http://192.168.0.106:5002";
                            audioData = await SynthesizeTextToAudioAsync(request);
                        }
                        speechChunks.Enqueue((chunk, audioData));
                    }
                    processingQueue = false;
                }
                async Task SpeechLoop()
                {
                    while (true)
                    {
                        while (speechChunks.TryDequeue(out var chunk))
                        {
                            File.WriteAllText(filePath, chunk.Text);
                            await Task.Delay(1000, cancellationToken);
                            if (chunk.AudioData != null)
                            {
                                await PlayAudioAsync(chunk.AudioData, cancellationToken);
                            }
                            else
                            {
                                await SpeakText(chunk.Text, cancellationToken);
                            }
                            File.WriteAllText(filePath, string.Empty);
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }
                        }
                        await Task.Delay(10, cancellationToken);
                        if (!processingQueue && speechChunks.Count < 1)
                        {
                            break;
                        }
                    }
                }
                Task queueTask = Task.Run(QueueLoop, cancellationToken);
                Task speechTask = Task.Run(SpeechLoop, cancellationToken);
                await Task.WhenAll(queueTask, speechTask);
            }
            catch (OperationCanceledException)
            {
                await Task.FromCanceled(cancellationToken);
            }
        }

        // Testing
        private static List<(string Voice, string Text)> SplitIntoNaturalVoiceChunks(string text)
        {
            List<(string Voice, string Text)> chunks = new();
            string currentVoice = "p230"; // Default voice is narrator
            string currentText = "";

            foreach (Match match in NaturalChunkSeparators().Matches(text).Cast<Match>())
            {
                string voiceTag = match.Groups[1].Value;
                string speechText = match.Groups[2].Value;
                string endTag = match.Groups[3].Value;

                if (voiceTag == "female")
                {
                    currentVoice = "p288"; // Female voice
                }
                else if (voiceTag == "male")
                {
                    currentVoice = "p253"; // Male voice
                }
                else if (voiceTag == "narrator")
                {
                    currentVoice = "p313"; // Default voice for narrator
                }

                // If the voice has changed or it's the last iteration, add the current chunk
                if (currentVoice != chunks.LastOrDefault().Voice || chunks.Count == 0)
                {
                    if (!string.IsNullOrEmpty(currentText))
                    {
                        chunks.Add((currentVoice, currentText));
                        currentText = "";
                    }
                }

                currentText += speechText;
            }

            // Add the last chunk if it's not empty
            if (!string.IsNullOrEmpty(currentText))
            {
                chunks.Add((currentVoice, currentText));
            }

            return chunks;
        }
        [GeneratedRegex("<(.*?)>(.*?)<\\/(.*?)>")]
        private static partial Regex NaturalChunkSeparators();
        public async Task SpeakTextUsingCoquiTtsSeparated(string text, CancellationToken cancellationToken = default)
        {
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SaveFolder);
            Directory.CreateDirectory(folderPath);
            string fileName = $"DisplayText.txt";
            string filePath = Path.Combine(folderPath, fileName);
            Queue<(string Voice, string Text)> voiceChunks = new(SplitIntoNaturalVoiceChunks(text));
            Queue<(string Text, byte[]? AudioData)> speechChunks = new();
            TtsRequest request = new()
            {
                Text = "hello"
            };
            byte[]? audioData;
            bool processingQueue = true;
            async Task QueueLoop()
            {
                while (voiceChunks.TryDequeue(out var chunk))
                {
                    request.SpeakerIdx = chunk.Voice;
                    request.Text = chunk.Text;
                    audioData = await SynthesizeTextToAudioAsync(request);
                    speechChunks.Enqueue((chunk.Text, audioData));
                }
                processingQueue = false;
            }
            async Task SpeechLoop()
            {
                while (true)
                {
                    while (speechChunks.TryDequeue(out var chunk))
                    {
                        File.WriteAllText(filePath, chunk.Text);

                        if (chunk.AudioData != null)
                        {
                            await PlayAudioAsync(chunk.AudioData, cancellationToken);
                        }
                        else
                        {
                            await SpeakText(chunk.Text, cancellationToken);
                        }
                        File.WriteAllText(filePath, string.Empty);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }
                    }
                    await Task.Delay(100, cancellationToken);
                    if (!processingQueue && speechChunks.Count < 1)
                    {
                        break;
                    }
                }
            }
            Task queueTask = Task.Run(QueueLoop, default);
            Task speechTask = Task.Run(SpeechLoop, default);
            await Task.WhenAll(queueTask, speechTask);
        }


         // # Testing End
#elif ANDROID

        public static async Task PlayAudioAsync(byte[] audioData, CancellationToken cancellationToken = default)
        {
            MediaPlayer mediaPlayer = new();
            try
            {
                // Create a temporary audio file from the byte array
                string tempAudioFile = Path.Combine(Android.App.Application.Context.CacheDir!.AbsolutePath, "tempaudio.wav");
                File.WriteAllBytes(tempAudioFile, audioData);

                // Set the data source to the temporary audio file
                await Task.Run(() => mediaPlayer.SetDataSource(tempAudioFile));

                // Prepare and start the media player
                mediaPlayer.Prepare();
                mediaPlayer.Start();

                // Wait until playback completes
                TaskCompletionSource<bool> playbackCompletion = new();
                mediaPlayer.Completion += (sender, args) =>
                {
                    playbackCompletion.TrySetResult(true);
                    mediaPlayer.Release();
                };
                await playbackCompletion.Task;

                // Delete the temporary audio file after playback
                File.Delete(tempAudioFile);
            }
            catch (Exception ex)
            {
                await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Error", $"Error playing audio: {ex.Message}");
            }
        }
        public static async Task<byte[]?> SynthesizeTextToAudioAsync(TtsRequest request, int timeoutInSeconds = 20)
        {
            if (string.IsNullOrEmpty(request.Text))
            {
                return null;
            }
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                var queryString = $"text={Uri.EscapeDataString(request.Text)}" +
                                  $"&speaker_id={request.SpeakerIdx}" +
                                  $"&language_id={request.LanguageIdx}";
                if (!string.IsNullOrWhiteSpace(request.StyleWav))
                {
                    queryString += $"&mood={Uri.EscapeDataString(request.StyleWav)}";
                }
                if (!string.IsNullOrWhiteSpace(request.AudioFormat))
                {
                    queryString += $"&audio_format={request.AudioFormat}";
                }
                var response = await client.GetAsync($"{request.ServerUrl}/api/tts?{queryString}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    await MyMultiPlatformUtils.WriteToLog($"Synthesizer Error: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Error", $"Error generating speech: {ex.Message}");
                return null;
            }
        }
        public static async Task SpeakTextUsingCoquiTts(string text, CancellationToken cancellationToken = default)
        {
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SaveFolder);
            Directory.CreateDirectory(folderPath);
            string fileName = $"DisplayText.txt";
            string filePath = Path.Combine(folderPath, fileName);
            Queue<string> voiceChunks = new(SplitIntoNaturalChunks(text));
            Queue<(string Text, byte[]? AudioData)> speechChunks = new();
            TtsRequest request = new();
            byte[]? audioData;
            bool processingQueue = false;

            async Task QueueLoop()
            {
                processingQueue = true;
                while (voiceChunks.TryDequeue(out var chunk))
                {
                    request.Text = chunk;
                    audioData = await SynthesizeTextToAudioAsync(request);
                    if (audioData == null)
                    {
                        request.ServerUrl = "http://192.168.0.106:5002";
                        audioData = await SynthesizeTextToAudioAsync(request);
                    }
                    speechChunks.Enqueue((chunk, audioData));
                }
                processingQueue = false;
            }
            async Task SpeechLoop()
            {
                while (true)
                {
                    while (speechChunks.TryDequeue(out var chunk))
                    {
                        File.WriteAllText(filePath, chunk.Text);
                        await Task.Delay(1000, cancellationToken);
                        if (chunk.AudioData != null)
                        {
                            await PlayAudioAsync(chunk.AudioData);
                        }
                        else
                        {
                            await SpeakText(chunk.Text, cancellationToken);
                        }
                        File.WriteAllText(filePath, string.Empty);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }
                    }
                    await Task.Delay(10, cancellationToken);
                    if (!processingQueue && speechChunks.Count < 1)
                    {
                        break;
                    }
                }
            }
            Task queueTask = Task.Run(QueueLoop, default);
            Task speechTask = Task.Run(SpeechLoop, default);
            await Task.WhenAll(queueTask, speechTask);
        }
        public static async Task<Task> SpeakText(string text, CancellationToken cancellationToken = default)
        {
            TextToSpeechService ttsService = new();
            await ttsService.SpeakText(text, cancellationToken);
            return Task.CompletedTask;
        }
#else
public static Task SpeakText(string text, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
#endif
        public MyAudioUtils()
        {
        }
        public static async Task<string> GetSpeechToText(CancellationToken cancellationToken = default)
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            Progress<string> progress = new(async text =>
            {
                await MyMultiPlatformUtils.WriteToLog($"{text}");
                await ResetSilenceTimer();
            });
            silenceDetected = false;
            silenceTimer = new Timer(_ => silenceDetected = true, null, Timeout.Infinite, Timeout.Infinite);
            try
            {
                bool isGranted = await speechToText.RequestPermissions(cancellationToken);
                if (!isGranted)
                {
                    return string.Empty;
                }
                SpeechToTextResult recognitionResult = await speechToText.ListenAsync(culture, progress, cancellationToken);

                silenceTimer.Change(silenceTimeoutMilliseconds, Timeout.Infinite);
                while (!silenceDetected)
                {
                    if (recognitionResult.Text != null)
                    {
                        await ResetSilenceTimer();
                        return recognitionResult.Text;
                    }
                    else if (recognitionResult.Exception != null)
                    {
                        await MyMultiPlatformUtils.WriteToLog($"Speech-to-Text Error: {recognitionResult.Exception.Message}");
                        break;
                    }
                    await Task.Delay(300, cancellationToken);
                }
                await MyMultiPlatformUtils.WriteToLog("Speech-to-Text: Recognition timed out due to silence.");
            }
            catch (Exception ex)
            {
                await MyMultiPlatformUtils.WriteToLog($"Speech-to-Text Error: {ex.Message}");
            }
            finally
            {
                silenceTimer.Dispose();
            }

            return string.Empty;
        }
        private static Task ResetSilenceTimer()
        {
            // Reset the silenceDetected flag and restart the timer
            silenceDetected = false;
            silenceTimer?.Change(3000, Timeout.Infinite);
            return Task.CompletedTask;
        }
        private static List<string> SplitIntoNaturalChunks(string text)
        {
#pragma warning disable CA1861 // Avoid constant arrays as arguments
            List<string> chunks = text.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
#pragma warning restore CA1861 // Avoid constant arrays as arguments
            List<string> result = new();
            string currentChunk = "";
            foreach (string chunk in chunks)
            {
                currentChunk += chunk + ".";
                bool isAbbreviation = Abbreviations.Any(abbr => chunk.Trim().Equals(abbr, StringComparison.OrdinalIgnoreCase));
                bool endsWithAbbreviation = Abbreviations.Any(abbr => chunk.TrimEnd().EndsWith(abbr, StringComparison.OrdinalIgnoreCase));
                if (!isAbbreviation && !endsWithAbbreviation)
                {
                    result.Add(currentChunk.Trim());
                    currentChunk = "";
                }
            }
            if (!string.IsNullOrWhiteSpace(currentChunk))
            {
                result.Add(currentChunk.Trim());
            }
            if (result.First() == "")
            {
                result.Remove(result.First());
            }
            if (result.Last().ToLower() is "to be continued." or "to be continued")
            {
                result.Remove(result.Last());
            }
            return result;
        }
        internal static readonly List<string> Abbreviations = new()
          {
           "Mr", "Mrs", "Dr", "etc", "e.g", "\""
           // Add other abbreviations as needed
          };
        public class TtsRequest
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = ""; // The text to be synthesized.
            [JsonPropertyName("speakerIdx")]
            public string SpeakerIdx { get; set; } = "p230"; // Speaker identifier.
            [JsonPropertyName("languageIdx")]
            public string LanguageIdx { get; set; } = "en"; // Language identifier.
            [JsonPropertyName("stylewav")]
            public string? StyleWav { get; set; } // Language identifier.
            [JsonPropertyName("audioFormat")]
            public string AudioFormat { get; set; } = "wav"; // Output audio format.
            [JsonPropertyName("serverUrl")]
            public string ServerUrl { get; set; } = "http://192.168.0.125:5002"; // Coqui TTS server URL.
        }
    }

#if ANDROID
    public class TextToSpeechService : UtteranceProgressListener, Android.Speech.Tts.TextToSpeech.IOnInitListener
    {
        private readonly Android.Speech.Tts.TextToSpeech _textToSpeech;
        private TaskCompletionSource<bool> _tcs = new();
        public TextToSpeechService()
        {
            _textToSpeech = new (Android.App.Application.Context, this);
        }
        public async Task SpeakText(string text, CancellationToken cancellationToken = default)
        {
            _tcs = new();

            if (_textToSpeech.Engines == null)
            {
                _textToSpeech.SetPitch(1.0f);
                _textToSpeech.SetSpeechRate(1.0f);
            }

            _textToSpeech.Speak(text, QueueMode.Flush, null, Guid.NewGuid().ToString());

            await _tcs.Task; // Wait until utterance completes
        }
        public void OnInit(OperationResult status)
        {
            if (status.Equals(OperationResult.Success))
            {
                _tcs.SetResult(true);
            }
            else
            {
                _tcs.SetException(new System.Exception("TextToSpeech initialization failed."));
            }
        }
        public override void OnDone(string? utteranceId)
        {
            _tcs.SetResult(true);
        }
        [Obsolete("OnError Override is obsolete")]
        public override void OnError(string? utteranceId)
        {
            _tcs.SetException(new System.Exception($"TextToSpeech error for utterance ID: {utteranceId}"));
        }

        public override void OnStart(string? utteranceId)
        {
            // Do nothing on start
        }
    }
#endif

#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0028 // Remove unnecessary suppression
#pragma warning restore IDE0305 // Remove unnecessary suppression
}

/* Azure Voice/Style List
-------Voices---------
______________________
en-US-JennyMultilingualNeural3 (Female)
en-US-JennyNeural (Female)
en-US-GuyNeural (Male)
en-US-AriaNeural (Female)
en-US-DavisNeural (Male)
en-US-AmberNeural (Female)
en-US-AnaNeural (Female, Child)
en-US-AshleyNeural (Female)
en-US-BrandonNeural (Male)
en-US-ChristopherNeural (Male)
en-US-CoraNeural (Female)
en-US-ElizabethNeural (Female)
en-US-EricNeural (Male)
en-US-JacobNeural (Male)
en-US-JaneNeural (Female)
en-US-JasonNeural (Male)
en-US-MichelleNeural (Female)
en-US-MonicaNeural (Female)
en-US-NancyNeural (Female)
en-US-RogerNeural (Male)
en-US-SaraNeural (Female)
en-US-SteffanNeural (Male)
en-US-TonyNeural (Male)
en-US-AIGenerate1Neural (Male)
en-US-AIGenerate2Neural (Female)
en-US-BlueNeural(Neutral)
en-US-JennyMultilingualV2Neural (Female)
en-US-RyanMultilingualNeural (Male)

-------Styles---------
______________________
en-US-AriaNeural	angry, chat, cheerful, customerservice, empathetic, excited, friendly, hopeful, narration-professional, newscast-casual, newscast-formal, sad, shouting, terrified, unfriendly, whispering
en-US-DavisNeural	angry, chat, cheerful, excited, friendly, hopeful, sad, shouting, terrified, unfriendly, whispering
en-US-GuyNeural	angry, cheerful, excited, friendly, hopeful, newscast, sad, shouting, terrified, unfriendly, whispering
en-US-JaneNeural	angry, cheerful, excited, friendly, hopeful, sad, shouting, terrified, unfriendly, whispering
en-US-JasonNeural	angry, cheerful, excited, friendly, hopeful, sad, shouting, terrified, unfriendly, whispering
en-US-JennyNeural	angry, assistant, chat, cheerful, customerservice, excited, friendly, hopeful, newscast, sad, shouting, terrified, unfriendly, whispering
en-US-NancyNeural	angry, cheerful, excited, friendly, hopeful, sad, shouting, terrified, unfriendly, whispering
en-US-SaraNeural	angry, cheerful, excited, friendly, hopeful, sad, shouting, terrified, unfriendly, whispering
en-US-TonyNeural	angry, cheerful, excited, friendly, hopeful, sad, shouting, terrified, unfriendly, whispering
*/