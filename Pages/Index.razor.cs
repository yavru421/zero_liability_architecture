using System;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace ZeroLiabilityArchitecture.Pages
{
    public partial class Index : IDisposable
    {
        private int activeScene = 1;
        private bool isUploading = false;
        private int uploadProgress = 0;
        private bool isAttacked = false;
        private string pourState = "None"; // None, SaaS, ZLA, Complete
        private bool jigDissolved = false;
        private bool celebrateActive = false;

        [JSImport("triggerConfetti", "zla-interop")]
        internal static partial void TriggerConfettiJS();

        [JSImport("speakText", "zla-interop")]
        internal static partial void SpeakTextJS(string text);

        private async Task SimulateUpload()
        {
            if (isUploading) return;
            isUploading = true;
            uploadProgress = 0;
            StateHasChanged();
            
            while (uploadProgress < 100)
            {
                await Task.Delay(200);
                uploadProgress += 10;
                StateHasChanged();
            }
            isUploading = false;
            isAttacked = true;
            StateHasChanged();
        }

        private void NextScene()
        {
            if (activeScene < 5)
            {
                activeScene++;
                StateHasChanged();
                PlayNarrator();
            }
        }

        public void PlayNarrator()
        {
            string rhyme = activeScene switch
            {
                1 => "There once was a Server named Sneeker-Mc-Snoot, who gathered up data and kept it as loot. Give me your passwords! Give me your files! You'd upload and wait across miles and miles.",
                2 => "But servers are targets for sneaks in the night, who steal all your data and run in a fright. If there is no database locked in a box, there is nothing to steal, and no picking of locks!",
                3 => "So we built a tool that stays right in your hand, the quickest and safest tool in the land! It runs in your browser, it lives on your screen, the cleanest machine that you ever have seen.",
                4 => "You pour in your numbers, they spin and they slide, they calculate quickly right here inside. And once you are done and you close up the tab, your data dissolves, there is nothing to grab!",
                5 => "Your P D F's ready, your document's done! No logins, no signups, the victory's won! It's fast and it's private, beginning to end, with zero security debts to defend!",
                _ => ""
            };
            
            if (!string.IsNullOrEmpty(rhyme))
            {
                try
                {
                    SpeakTextJS(rhyme);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"TTS Error: {ex.Message}");
                }
            }
        }

        private void SetPourState(string state)
        {
            pourState = state;
            if (state == "ZLA")
            {
                jigDissolved = true;
            }
            StateHasChanged();
        }

        private void Celebrate()
        {
            celebrateActive = true;
            try
            {
                TriggerConfettiJS();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Confetti Trigger Error: {ex.Message}");
            }
            StateHasChanged();
        }

        public void Dispose()
        {
            // Clean up resources if necessary
        }
    }
}
