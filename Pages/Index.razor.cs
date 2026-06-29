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
    public string TestInput { get; set; } = "";
    public bool IsWiped { get; set; } = false;

    private readonly string[] rhymes = new[]
    {
        "Nearly every app you use 'phones home.' Every file you select, every word you type, and every button you click gets sent to a server somewhere. The moment your data leaves your device, you lose control of it.",
        "Because other apps collect and store your data on their servers, they create giant targets for hackers. If their server gets compromised, your private information is leaked.",
        "ZLA apps are different. They never phone home. The entire app runs directly inside your web browser using your own device's power. It does not send a single byte of your work to a server.",
        "Type anything below. You can even turn off your internet completely. The app still does the work, because it is running 100% on your device, not on a remote server.",
        "You get your results instantly, on your own device, in your own hands. No signups, no accounts, and no servers in the middle. Because we don't store your files, we can't lose them."
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
    
    public async Task HandleWipe()
    {
        IsWiped = true;
        await JSRuntime.InvokeVoidAsync("zlaInterop.triggerDissolve", "card4-numbers");
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        dotNetHelper?.Dispose();
        GC.SuppressFinalize(this);
    }
}
