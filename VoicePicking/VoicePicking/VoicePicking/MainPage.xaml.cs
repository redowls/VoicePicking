using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using VoicePicking.Services;
using Microsoft.CognitiveServices.Speech;
using Xamarin.Essentials;

namespace VoicePicking
{
    public partial class MainPage : ContentPage
    {
        SpeechRecognizer recognizer;
        IMicrophoneService micService;
        bool isTranscribing = false;

        public MainPage()
        {
            InitializeComponent();
            //bool isMicEnabled = await micService.GetPermissionAsync();
            //if (!isMicEnabled)
            //{
            //    UpdateTranscription("Please grant access to the microphone!");
            //    return;
            //}

            //micService = DependencyService.Resolve<IMicrophoneService>();
            //var config = SpeechConfig.FromSubscription(Constants.CognitiveServicesApiKey, Constants.CognitiveServicesRegion);
            //recognizer = new SpeechRecognizer(config);
            //recognizer.Recognized += async (obj, args) =>
            //{
            //    string text = args.Result.Text.ToLower();
            //    if (text.Contains("ready"))
            //    {
            //        try
            //        {
            //            UpdateTranscription(args.Result.Text);
            //            await recognizer.StopContinuousRecognitionAsync();
            //        }
            //        catch (Exception ex)
            //        {
            //            UpdateTranscription(ex.Message);
            //        }
            //        isTranscribing = false;
            //        UpdateDisplayState();
            //    }
            //};
        }

        async void transcribeButton_Clicked(object sender, EventArgs e)
        {
            bool isMicEnabled = await micService.GetPermissionAsync();

            // EARLY OUT: make sure mic is accessible
            if (!isMicEnabled)
            {
                UpdateTranscription("Please grant access to the microphone!");
                return;
            }

            // initialize speech recognizer 
            if (recognizer == null)
            {
                var config = SpeechConfig.FromSubscription(Constants.CognitiveServicesApiKey, Constants.CognitiveServicesRegion);
                recognizer = new SpeechRecognizer(config);
                recognizer.Recognized += (obj, args) =>
                {
                    UpdateTranscription(args.Result.Text);
                };
            }

            // if already transcribing, stop speech recognizer
            if (isTranscribing)
            {
                try
                {
                    await recognizer.StopContinuousRecognitionAsync();
                }
                catch (Exception ex)
                {
                    UpdateTranscription(ex.Message);
                }
                isTranscribing = false;
            }

            // if not transcribing, start speech recognizer
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    InsertDateTimeRecord();
                });
                try
                {
                    await recognizer.StartContinuousRecognitionAsync();
                }
                catch (Exception ex)
                {
                    UpdateTranscription(ex.Message);
                }
                isTranscribing = true;
            }
            UpdateDisplayState();
        }

        void UpdateTranscription(string newText)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (!string.IsNullOrWhiteSpace(newText))
                {
                    transcribedText.Text += $"{newText}\n";
                }
            });
        }

        void InsertDateTimeRecord()
        {
            //var msg = $"=================\n{DateTime.Now.ToString()}\n=================";
            var msg = $"";
            UpdateTranscription(msg);
        }

        void UpdateDisplayState()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (isTranscribing)
                {
                    transcribeButton.Text = "Stop";
                    transcribeButton.BackgroundColor = Color.Red;
                    transcribingIndicator.IsRunning = true;
                }
                else
                {
                    transcribeButton.Text = "Transcribe";
                    transcribeButton.BackgroundColor = Color.Green;
                    transcribingIndicator.IsRunning = false;
                }
            });
        }

        private void speakButton_Clicked(object sender, EventArgs e)
        {
            IsBusy = true;
            SpeakMultiple();
        }
        private void SpeakMultiple()
        {
            Task.Run(async () =>
            {
                await TextToSpeech.SpeakAsync(transcribedText.Text);
                IsBusy = false;
            });
        }
        private async Task SpeakNowDefaultSettings()
        {
            await TextToSpeech.SpeakAsync(transcribedText.Text);
        }
    }
}
