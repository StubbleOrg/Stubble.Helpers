using System;
using Stubble.Core.Settings;

namespace Stubble.Helpers
{
    public class HelperRendererSettings
    {
        private readonly RendererSettings _rendererSettings;

        public HelperRendererSettings(RendererSettings rendererSettings)
        {
            _rendererSettings = rendererSettings;
        }

        public Func<string, string> EncodingFuction => _rendererSettings.EncodingFuction;
    }
}
