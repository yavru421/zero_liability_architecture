using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace ZeroLiabilityArchitecture.Pages;

public partial class Index : ComponentBase, IAsyncDisposable
{
    [Inject]
    public IJSRuntime JSRuntime { get; set; } = default!;

    private DotNetObjectReference<Index>? dotNetHelper;
    
    public int CurrentCard { get; set; } = 0; // JS is 0-indexed
    public bool IsPlaying { get; set; } = false;
    public int ActiveTrackNum { get; set; } = 1;
    
    // Demo Interactive State
    public double Num1 { get; set; } = 5;
    public double Num2 { get; set; } = 3;
    public string Op { get; set; } = "+";
    public string TextVal { get; set; } = "Type anything here. The network is physically severed.";

    public string CalcResult
    {
        get
        {
            try
            {
                return Op switch
                {
                    "+" => (Num1 + Num2).ToString(),
                    "-" => (Num1 - Num2).ToString(),
                    "*" => (Num1 * Num2).ToString(),
                    "/" => Num2 != 0 ? (Num1 / Num2).ToString() : "Error",
                    _ => "0"
                };
            }
            catch
            {
                return "Error";
            }
        }
    }

    private readonly string[] rhymes = new[]
    {
        "There once was a Server named Sneeker-Mc-Snoot, Who rented out tools in exchange for your loot. 'Upload your data, your files, and your text! My servers will process and see what comes next!' But when your data is locked in his cloud, It’s watched by the Sneekers and sold to the crowd.",
        "He makes you create an account and a key, And lock up your secrets for hackers to see. For databases are targets for sneaks in the night, Who smash through the logins and run in a fright! If there is no server to hold what you make, There is nothing to steal and no passwords to break!",
        "So we built you a hammer, a physical tool, Delivered right to you, so simple and cool! There is no backend, no server to call, It runs in your browser, and that is just all. The tool does the work right in front of your face, With zero connections to any outside space!",
        "Don't believe it? Just sever the cord! Turn off your Wi-Fi and pull out the board! Type in your numbers and see what they do, The engine is local and powered by YOU! The processing happens right here on your screen, The most private computer that ever has been!",
        "We never take custody, never take hold, Your data remains yours, it can never be sold. We have zero liability, zero to hide, Your secrets are safe on your own local side! And when you are finished and close up the tab, Your data is wiped with no traces to grab!"
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            dotNetHelper = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("zlaInterop.initSwipeDeck", dotNetHelper);
            
            // Listeners in JS will autoplay on first touch/click, update state accordingly
            IsPlaying = true;
            StateHasChanged();
        }
    }

    [JSInvokable("OnCardSnapped")]
    public async Task OnCardSnapped(int cardIndex)
    {
        CurrentCard = cardIndex;
        IsPlaying = await JSRuntime.InvokeAsync<bool>("zlaInterop.isAudioPlaying");
        ActiveTrackNum = await JSRuntime.InvokeAsync<int>("zlaInterop.getCurrentTrackNum");
        StateHasChanged();
    }

    public async Task TogglePlay()
    {
        IsPlaying = await JSRuntime.InvokeAsync<bool>("zlaInterop.togglePlay");
        ActiveTrackNum = await JSRuntime.InvokeAsync<int>("zlaInterop.getCurrentTrackNum");
        StateHasChanged();
    }

    public async Task SelectTrack(int trackNum)
    {
        await JSRuntime.InvokeVoidAsync("zlaInterop.switchTrack", trackNum);
        ActiveTrackNum = trackNum;
        IsPlaying = true;
        StateHasChanged();
    }

    public async Task NextCard()
    {
        await JSRuntime.InvokeVoidAsync("zlaInterop.snapToNext");
    }

    public async Task TriggerConfetti(string elementId)
    {
        await JSRuntime.InvokeVoidAsync("zlaInterop.triggerConfetti", elementId);
    }

    public async ValueTask DisposeAsync()
    {
        dotNetHelper?.Dispose();
        GC.SuppressFinalize(this);
    }
}
