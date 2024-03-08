using LocalAiAssistant.Utilities;
using Microsoft.Maui;
using System.ComponentModel.Design;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;
using System.Xml.Linq;

#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0301 // Simplify collection initialization
namespace LocalAiAssistant
{
    public partial class TextGeneration : ContentPage
    {
        private readonly static MyAIAPI.Message SystemMessage = new()
        {
            Role = "system",
            Content = "You are a helpful assistant."
        };
        private static List<MyAIAPI.Message> curMessages = new() { SystemMessage };
        internal static TextGenerationSettingsData UiData = new();
        private GeneralSettingsData defaultData = new();
        DisplayInfo displayInfo = new();

        public TextGeneration()
        {
            InitializeComponent();
            BindingContext = UiData;
            this.Loaded += TextGenerationLoaded;
            this.SizeChanged += OnPageSizeChanged;
            DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
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
                buttonStack.Orientation = StackOrientation.Vertical;
#endif
            }
            else if (displayInfo.Orientation == DisplayOrientation.Landscape)
            {
#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"DisplayChanged Set Orientation: Horizontal");
                buttonStack.Orientation = StackOrientation.Horizontal;
#endif
            }
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
                    defaultData.DefaultTimeOutDelay = saveData.DefaultTimeOutDelay;
                }
            }
            if (MyMultiPlatformUtils.CheckPreferenceContains(TextGenerationSettings.TextGenerationPreference))
            {
                TextGenerationSettingsData? saveData = await MyMultiPlatformUtils.ReadFromPreferences<TextGenerationSettingsData>(TextGenerationSettings.TextGenerationPreference);
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
                if (UiData.MultiModal)
                {
                    SelectImageBtn.IsVisible = true;
                }
                else
                {
                    SelectImageBtn.IsVisible = false;
                }
                if (UiData.Stream) 
                { 
                    ContinueBtn.IsVisible = true;
                }
            }
            else
            {
                UiData.ServerUrlInput = defaultData.DefaultServerUrl;
                UiData.AuthEnabled = defaultData.AuthEnabled;
                UiData.ApiKey = defaultData.DefaultApiKey;
            }
        }
        private async void TextGenerationLoaded(object? sender, EventArgs e)
        {
            await LoadData();
        }
        private async void OnPageSizeChanged(object? sender, EventArgs e)
        {
            double aspectRatio = (double)Width / Height;
            if (aspectRatio > (4.0 / 3.0))
            {
#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"PageSize Set Orientation: Horizontal");
                buttonStack.Orientation = StackOrientation.Horizontal;
#endif
            }
            else
            {
#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"PageSize Set Orientation: Vertical");
                buttonStack.Orientation = StackOrientation.Vertical;
#endif
            }
        }
        private async void OnSelectImageBtnClicked(object sender, EventArgs e)
        {
            SemanticScreenReader.Announce(SelectImageBtn.Text);
            byte[]? imageBytes = await MyMultiPlatformUtils.PickImageAsync();
            if (imageBytes != null)
            {
#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"Selected Image");
#endif
                UiData.SelectedImage = imageBytes;
            }
        }
        private async void OnUserInputCompleted(object sender, EventArgs e)
        {
            SemanticScreenReader.Announce("Input Complete");
            string userInput = UiData.UserInput;
            UiData.UserInput = "";
            await ProcessUserMessage(userInput);
#if DEBUG
            await MyMultiPlatformUtils.WriteToLog($"Processed Message: {userInput}");
#endif
        }
        private async void OnSendClicked(object sender, EventArgs e)
        {
            SemanticScreenReader.Announce(SendBtn.Text);
            string userInput = UiData.UserInput;
            UiData.UserInput = "";
            await ProcessUserMessage(userInput);
#if DEBUG
            await MyMultiPlatformUtils.WriteToLog($"Processed Message: {userInput}");
#endif
        }
        private async void OnRetryBtnClicked(object sender, EventArgs e)
        {
            SemanticScreenReader.Announce(RetryBtn.Text);
            await ProcessUserMessage("Retrying", true);
#if DEBUG
            await MyMultiPlatformUtils.WriteToLog($"Retrying Message.");
#endif
        }
        private async void OnCancelBtnClicked(object sender, EventArgs e)
        {
            SemanticScreenReader.Announce(CancelBtn.Text);
            if (MyAIAPI.requestCancellation.Token.CanBeCanceled == true)
            {
                MyAIAPI.requestCancellation.Cancel();
            }
#if DEBUG
            await MyMultiPlatformUtils.WriteToLog($"Canceling Message.");
#endif
        }
        private async void OnContinueBtnClicked(object sender, EventArgs e)
        {
            SemanticScreenReader.Announce(ContinueBtn.Text);
            await ProcessUserMessage("continueing", continuePrevResponse: true);
#if DEBUG
            await MyMultiPlatformUtils.WriteToLog($"Continue Message.");
#endif
        }
        private async void OnResetClicked(object sender, EventArgs e)
        {
            curMessages = new() { SystemMessage };
            ChatStackLayout.Children.Clear();

#if DEBUG
            await MyMultiPlatformUtils.WriteToLog($"Chat Cleared");
#endif
        }
        private async Task ProcessUserMessage(string message, bool retrying = false, bool continuePrevResponse = false)
        {
            if (string.IsNullOrEmpty(message))
            {
#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"Message null or empty.");
#endif
                return;
            }
            if (continuePrevResponse && curMessages.LastOrDefault()?.Role != "assistant")
            {
#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"Tried to continue an empty request.");
#endif
                return;
            }
            if (retrying && curMessages.LastOrDefault()?.Role == "system")
            {
#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"Tried to retry an empty request.");
#endif
                return;
            }
            try
            {

#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"Process User Input: {message}");
#endif
                if (!retrying && !continuePrevResponse)
                {
                    await AddChatBubble(message, "User", true);
                }
                if (UiData.SelectedModelIndex >= 0 && UiData.SelectedModelIndex < UiData.ModelList.Count)
                {
                    UiData.Model = UiData.ModelList[UiData.SelectedModelIndex];
                }
                bool authEnabled = UiData.AuthEnabled;
#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"Auth enabled: {authEnabled}");
                await MyMultiPlatformUtils.WriteToLog($"Using base url: {UiData.ServerUrlInput}");
                await MyMultiPlatformUtils.WriteToLog($"Using API key: {UiData.ApiKey}");
                await MyMultiPlatformUtils.WriteToLog($"Using model: {UiData.Model}");
                await MyMultiPlatformUtils.WriteToLog($"Using multi-modal: {UiData.MultiModal}");
#endif
                MyAIAPI.Message response = new();
                if (UiData.MultiModal)
                {
                    List<MyAIAPI.MultiModalMessage> convertedMessages = MyAIAPI.ConvertToMultiModalMessages(curMessages);
                    List<MyAIAPI.MultiModalContent> userMessageContent = new();

                    if (UiData.SelectedImage != null && UiData.SelectedImage.Length > 0)
                    {
                        string imageFormat = "png"; // or whatever format you need
                        string imageBase64 = Convert.ToBase64String(UiData.SelectedImage);
                        string imageUri = $"data:image/{imageFormat};base64,{imageBase64}";
                        UiData.SelectedImage = Array.Empty<byte>();
                        MyAIAPI.MultiModalTextContent userTextContent = new()
                        {
                            Type = "text",
                            Text = message
                        };
                        userMessageContent.Add(userTextContent);
                        MyAIAPI.MultiModalImageContent userImageContent = new()
                        {
                            Type = "image_url",
                            ImageUrl = new MyAIAPI.ImageUrl()
                            {
                                Url = imageUri
                            }
                        };
                        userMessageContent.Add(userImageContent);
                    }
                    else
                    {
                        MyAIAPI.MultiModalTextContent userTextContent = new()
                        {
                            Type = "text",
                            Text = message
                        };
                        userMessageContent.Add(userTextContent);
                    }
                    MyAIAPI.MultiModalMessage userMessage = new()
                    {
                        Role = "user",
                        Content = userMessageContent
                    };
                    convertedMessages.Add(userMessage);

                    //response = await MyAIAPI.GenerateMultiModalTextGenerationAsync(messages: convertedMessages, apiKey: UiData.ApiKey, model: UiData.Model, timeoutInSeconds: (int)UiData.TimeOutDelay, serverUrl: UiData.ServerUrlInput, authEnabled: authEnabled, topP: UiData.TopP, maxTokens: UiData.MaxTokens, temperature: UiData.Temperature, presencePenalty: UiData.PresencePenalty, frequencyPenalty: UiData.FrequencyPenalty);

                    Task<MyAIAPI.Message> responseTask = MyAIAPI.GenerateMultiModalTextGenerationAsync(messages: convertedMessages, apiKey: UiData.ApiKey, model: UiData.Model, timeoutInSeconds: (int)UiData.TimeOutDelay, serverUrl: UiData.ServerUrlInput, authEnabled: authEnabled, topP: UiData.TopP, maxTokens: UiData.MaxTokens, temperature: UiData.Temperature, presencePenalty: UiData.PresencePenalty, frequencyPenalty: UiData.FrequencyPenalty);

                    await AddChatBubble("", "Assistant", false);
                    int dots = 1;
                    while (!responseTask.IsCompleted)
                    {
                        switch (dots)
                        {
                            case 1:
                                await EditLastAssistantMessage("Processing.");
                                dots++;
                                break;
                            case 2:
                                await EditLastAssistantMessage("Processing..");
                                dots++;
                                break;
                            case 3:
                                await EditLastAssistantMessage("Processing...");
                                dots = 1;
                                break;
                        }
                        await Task.Delay(500);
                    }
                }
                else
                {
                    string responseTxt = "";
                    if (retrying)
                    {
                        curMessages.Remove(curMessages.Last());
                        await RemoveLastChatBubble();
                    }
                    else if (continuePrevResponse)
                    {
                        if (!string.IsNullOrWhiteSpace(curMessages.LastOrDefault()?.Content))
                        {
                            responseTxt = $"{curMessages.Last().Content} ";
                            if (curMessages.First().Role == "system")
                            {
                                curMessages.First().Content = "Continue the response where you left off.";
                            }
                        }
                    }
                    else
                    {
                        MyAIAPI.Message userMessage = new()
                        {
                            Role = "user",
                            Content = message
                        };
                        curMessages.Add(userMessage);
                    }
                    if (!continuePrevResponse)
                    {
                        await AddChatBubble("", "Assistant", false);
                    }
                    int dots = 1;

                    if (UiData.Stream)
                    {
                        MyAIAPI.OnDataReceived += (sender, receivedMessage) =>
                        {
                            if (!string.IsNullOrEmpty(receivedMessage))
                            {
                                responseTxt += receivedMessage;
                            }
                        };
                    }
                    Task<MyAIAPI.Message> responseTask = MyAIAPI.GenerateStreamingTextGenerationAsync(messages: curMessages, apiKey: UiData.ApiKey, model: UiData.Model, timeoutInSeconds: (int)UiData.TimeOutDelay, serverUrl: UiData.ServerUrlInput, authEnabled: UiData.AuthEnabled, topP: UiData.TopP, maxTokens: UiData.MaxTokens, temperature: UiData.Temperature, stop: UiData.Stop, presencePenalty: UiData.PresencePenalty, frequencyPenalty: UiData.FrequencyPenalty, stream: UiData.Stream);

                    bool isEditing = false;
                    while (!responseTask.IsCompleted)
                    {
                        if (string.IsNullOrWhiteSpace(responseTxt))
                        {
                            switch (dots)
                            {
                                case 1:
                                    await EditLastAssistantMessage("Processing.");
                                    dots++;
                                    break;
                                case 2:
                                    await EditLastAssistantMessage("Processing..");
                                    dots++;
                                    break;
                                case 3:
                                    await EditLastAssistantMessage("Processing...");
                                    dots = 1;
                                    break;
                            }
                            await Task.Delay(500);
                        }
                        else
                        {
                            if (!isEditing)
                            {
                                isEditing = true;
                                await EditLastAssistantMessage(responseTxt);
                                await ScrollToLastChat();
                                await Task.Delay(1);
                                isEditing = false;
                            }
                        }
                    }
                    response = await responseTask;
                    if (continuePrevResponse && !string.IsNullOrWhiteSpace(responseTxt))
                    {
                        response!.Content = responseTxt;
                    }
                }
                if (continuePrevResponse)
                {
                    curMessages.Remove(curMessages.Last());
                }
                curMessages.Add(response);
                if (response != null)
                {
                    if (string.IsNullOrWhiteSpace(response.Content))
                    {
                        await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Warning!", "Failed to generate a response.");
                        await EditLastAssistantMessage("(Failed)");
                        return;
                    }
                    await EditLastAssistantMessage(response.Content);
                    await ScrollToLastChat();
                    if (UiData.TTSEnabled)
                    {
                        AudioGeneration audioGen = new();
                        await Task.Delay(20);
                        byte[]? curAudioBytes = await audioGen.GenerateTtsAudio(response.Content, MyAIAPI.requestCancellation.Token);
                        if (curAudioBytes != null)
                        {
                            await MyAudioUtils.PlayAudioAsync(curAudioBytes, cancellationToken: MyAIAPI.requestCancellation.Token);
                        }
                    }
                }

#if DEBUG
                await MyMultiPlatformUtils.WriteToLog($"Response: {response?.Content}");
#endif
            }
            catch (Exception ex)
            {
                await MyMultiPlatformUtils.MessageBoxWithOK(Microsoft.Maui.Controls.Application.Current!, "Failed", $"{ex}");
            }
        }

        private async Task AddChatBubble(string message, string author, bool sender = false)
        {
            if (!MainThread.IsMainThread)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await AddChatBubble(message, author, sender);
                });
            }
            else
            {
                Frame chatBubbleFrame = new()
                {
                    Padding = new Thickness(10),
                    Margin = new Thickness(0, 0, 0, 10),
                    BackgroundColor = sender ? Colors.DarkBlue : Colors.Green, // Sender | Reciever
                    CornerRadius = 10,
                    HasShadow = false
                };

                StackLayout messageLayout = new()
                {
                    Orientation = StackOrientation.Vertical,
                    Spacing = 5
                };

                Label nameLabel = new()
                {
                    Text = $"{author}:",
                    HorizontalTextAlignment = sender ? TextAlignment.End : TextAlignment.Start,
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = sender ? Colors.WhiteSmoke : Colors.WhiteSmoke
                };

                Label messageLabel = new()
                {
                    Text = message,
                    Style = Application.Current!.FindByName<Style>("ChatBubbleTextStyle")
                };

                Entry messageEntry = new()
                {
                    IsSpellCheckEnabled = true,
                    Style = Application.Current!.FindByName<Style>("ChatBubbleTextStyle"),
                    IsVisible = false
                };
                messageEntry.Completed += (s, e) =>
                {
                    messageLabel.IsVisible = true;
                    messageEntry.IsVisible = false;
                    messageLabel.Text = messageEntry.Text;
                    int index = ChatStackLayout.Children.IndexOf(chatBubbleFrame);
                    if (index >= 0 && index < curMessages.Count)
                    {
                        curMessages[index + 1].Content = messageEntry.Text;
                    }
                };

                messageLayout.Children.Add(nameLabel);
                messageLayout.Children.Add(messageLabel);
                messageLayout.Children.Add(messageEntry); // Add entry to layout
                chatBubbleFrame.Content = messageLayout;
                chatBubbleFrame.HorizontalOptions = sender ? LayoutOptions.End : LayoutOptions.Start;

                // Adding long press gesture recognizer
                TapGestureRecognizer tapGestureRecognizer = new()
                {
                    NumberOfTapsRequired = 2
                };
                tapGestureRecognizer.Tapped += (s, e) =>
                {
                    if (messageLabel.IsVisible)
                    {
                        messageEntry.Text = messageLabel.Text;
                        messageLabel.IsVisible = false;
                        messageEntry.IsVisible = true;
                    }
                    else
                    {
                        messageLabel.IsVisible = true;
                        messageEntry.IsVisible = false;
                        messageLabel.Text = messageEntry.Text;
                        int index = ChatStackLayout.Children.IndexOf(chatBubbleFrame);
                        if (index >= 0 && index < curMessages.Count)
                        {
                            curMessages[index + 1].Content = messageEntry.Text;
                        }
                    }
                };
                chatBubbleFrame.GestureRecognizers.Add(tapGestureRecognizer);

                ChatStackLayout.Children.Add(chatBubbleFrame);
                await ScrollToLastChat();
            }
        }

        private async Task RemoveLastChatBubble()
        {
            if (!MainThread.IsMainThread)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await RemoveLastChatBubble();
                });
            }
            else
            {
                if (ChatStackLayout.Children.Count > 0)
                {
                    ChatStackLayout.Children.RemoveAt(ChatStackLayout.Children.Count - 1);
                    return;
                }
                throw new Exception("No chat bubble to remove.");
            }
        }
        private async Task EditLastAssistantMessage(string newMessage)
        {
            if (!MainThread.IsMainThread)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await EditLastAssistantMessage(newMessage);
                });
            }
            else
            {
                if (ChatStackLayout.Children.Count > 0)
                {
                    for (int i = ChatStackLayout.Children.Count - 1; i >= 0; i--)
                    {
                        if (ChatStackLayout.Children[i] is Frame chatBubbleFrame && chatBubbleFrame.BackgroundColor == Colors.Green)
                        {
                            if (chatBubbleFrame.Content is StackLayout messageLayout && messageLayout.Children.Count > 1)
                            {
                                if (messageLayout.Children[1] is Label messageLabel)
                                {
                                    messageLabel.Text = newMessage;
                                    return;
                                }
                            }
                        }
                    }
                }
                throw new Exception("No message to edit.");
            }
        }
        private async Task ScrollToLastChat()
        {
            if (!MainThread.IsMainThread)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await ScrollToLastChat();
                });
            }
            else
            {
                await Task.Delay(50);
                if (ChatView.ContentSize.Height > ChatView.Height)
                {
                    double lastBubbleBottomY = ChatStackLayout.Bounds.Bottom;
                    _ = ChatView.ScrollToAsync(0, lastBubbleBottomY, true);
                }
            }
        }
    }
    public class TextGenerationSettingData2 : TextGenerationSettingsData
    {
        [JsonPropertyName("file")]
        public string File { get; set; } = string.Empty;
    }
}
#pragma warning restore IDE0028 // Simplify collection initialization
#pragma warning restore IDE0301 // Simplify collection initialization