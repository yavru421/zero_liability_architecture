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
        "When you use standard cloud apps, you have to upload your files to their computers. The moment your file leaves your device, you lose control. You don't know who is looking at it, where it is copied, or where it will end up.",
        "Cloud companies store millions of files in giant online databases. This makes them massive targets for hackers. If their server gets hacked, your private documents get stolen. If a company doesn't store your files, they can't lose them.",
        "Our apps work like a physical calculator. The tool runs directly inside your web browser using your phone or computer's power. You use the tool, but you never upload your files to our servers. We never see your data.",
        "Type some text or numbers below to process it directly on your screen. The moment you click wipe, the memory is deleted. Zero bytes are sent to our servers. Zero bytes are stored online.",
        "Your finished document is generated directly on your screen and saved to your device. No signups, no accounts to create, and no passwords to remember. Pure utility, with zero privacy risks."
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
