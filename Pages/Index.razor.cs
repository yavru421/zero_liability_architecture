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
        "When you upload your files to a cloud in the sky, (Even to Google Drive, you cannot deny), Your data is shipped to a server out there, And what happens in-between... does anyone care?",
        "Once it is stored in their black box of data, It's indexed and read, now or sometime later. If a hacker breaks in and the security locks fail, Your private files are suddenly up for sale!",
        "ZLA apps are built to run right in your hand, The safest and cleanest tools in the land! They run in your browser, on your own device, Processing locally, clean and precise.",
        "Your files stay home on your own CPU, No servers in-between reading secrets from you. And once you are done and you close up the tab, The memory dissolves — there is nothing to grab!",
        "Your document's ready, the victory's won, Without sending your files, the processing's done! It's fast and it's private, beginning to end, With zero security debts to defend!"
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
