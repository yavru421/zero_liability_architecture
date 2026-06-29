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

    private readonly string[] rhymes = new[]
    {
        "There once was a Server named Sneeker-Mc-Snoot, Who gathered up data and kept it as loot. Give me your passwords! Give me your files! You'd upload and wait across miles and miles.",
        "But servers are targets for sneaks in the night, Who steal all your data and run in a fright. If there is no database locked in a box, There is nothing to steal, and no picking of locks!",
        "So we built a tool that stays right in your hand, The quickest and safest tool in the land! It runs in your browser, it lives on your screen, The cleanest machine that you ever have seen.",
        "You pour in your numbers, they spin and they slide, They calculate quickly right here inside. And once you are done and you close up the tab, Your data dissolves, there is nothing to grab!",
        "Your P D F's ready, your document's done! No logins, no signups, the victory's won! It's fast and it's private, beginning to end, With zero security debts to defend!"
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
    
    public async Task Dissolve()
    {
        await JSRuntime.InvokeVoidAsync("zlaInterop.triggerDissolve", "card4-numbers");
    }

    public async ValueTask DisposeAsync()
    {
        dotNetHelper?.Dispose();
        GC.SuppressFinalize(this);
    }
}
