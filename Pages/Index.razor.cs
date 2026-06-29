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
    public string TextVal { get; set; } = "Zero-Liability Architecture runs entirely on-device.";

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
        "There once was a Server named Sneeker-Mc-Snoot, Who gathered up data and kept it as loot. 'Upload your files to my cloud in the sky! Send them to me!' was his welcoming cry. But when you upload to his box far away, Who knows what he does with your files at the day?",
        "For databases are targets for sneaks in the night, Who steal all your data and run in a fright. If there is no server to hold what you make, There is nothing to steal and no database to break!",
        "So we built a tool that stays right in your hand, The quickest and safest tool in the land! It runs in your browser, it lives on your screen, The cleanest machine that you ever have seen. It never phones home, and it never will send A byte of your work to a server or friend!",
        "You can turn off your internet, shut down the line, The tool will still function, completely and fine! It processes locally, safe on your screen, No phoning to Sneeker, no servers in-between!",
        "Your calculations are finished, your safety is won, Without sending your files, the processing's done! We don't store your files, we have nothing to grab, Wiped clean from your screen when you close up the tab!"
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
