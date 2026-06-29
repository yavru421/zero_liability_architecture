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
        "Other apps phone home. Every file, click, and keystroke goes to their servers.",
        "Online databases are giant targets for hackers. If they get breached, your data is exposed.",
        "ZLA apps run 100% inside your browser using your device's power. They never send your data to a server.",
        "You can turn off your internet completely right now. The tool will still function, because it does not rely on a remote server.",
        "You get your results directly on your screen. Because we don't store your files, we can't lose them. No accounts, no logins."
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
