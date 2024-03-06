#if ANDROID
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Speech;
#endif
namespace LocalAiAssistant.Utilities
{
    internal class MyEssentialUtils
    {
#if ANDROID
        /* Needs XamarinEssentials
        public static Task<string> GetSpeechInput()
        {
            string speechText = "";
            try
            {
                Intent voiceIntent = new(RecognizerIntent.ActionRecognizeSpeech);
                voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
                voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, "Speak now");
                voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
                voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
                voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
                voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
                voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
                Activity activity = (Activity)Application.ActivityService;
                activity.StartActivityForResult(voiceIntent, 0);
            }
            catch (Exception e)
            {
                return Task.FromException<string>(e);
            }
            return Task<string>.FromResult(speechText);
        }
        public static async Task<Task> SpeakText(string text)
        {
            try
            {
                Xamarin.Essentials.SpeechOptions settings = new() { Volume = 0.0f, Pitch = 1.0f };
                await Xamarin.Essentials.TextToSpeech.SpeakAsync(text, settings);
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }
        */
#endif
    }
}