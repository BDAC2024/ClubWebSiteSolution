using Microsoft.JSInterop;

namespace AnglingClubWebsite.Services
{
    public class BrowserService
    {
        private readonly IJSRuntime _js;

        public BrowserService(
            IJSRuntime js)
        {
            _js = js;
        }

        public BrowserDimension Dimensions { get; set; } = new BrowserDimension { Width = 300, Height = 240 };
        public bool IsMobile { get; set; } = false;

        public bool IsPortrait 
        { 
            get
            {
                return Dimensions.Width < Dimensions.Height;
            }
        }

        public async Task<BrowserDimension> GetDimensions()
        {
            Dimensions = await _js.InvokeAsync<BrowserDimension>("getDimensions");
            IsMobile = await _js.InvokeAsync<bool>("isDevice");
            return Dimensions;
        }

        public class BrowserDimension
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }

    }
}
