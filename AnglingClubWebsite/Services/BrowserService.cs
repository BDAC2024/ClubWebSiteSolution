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
            return Dimensions;
        }

        public async Task<bool> IsMobile()
        {
            return await _js.InvokeAsync<bool>("isDevice");
        }


        public class BrowserDimension
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }

    }
}
