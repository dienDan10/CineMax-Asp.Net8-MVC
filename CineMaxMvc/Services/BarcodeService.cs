using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using ZXing;
using ZXing.Common;
using ZXing.ImageSharp.Rendering;

namespace CineMaxMvc.Services
{
    public class BarcodeService
    {
        public byte[] GenerateBarcodeImage(string text, int width = 300, int height = 100)
        {
            var writer = new BarcodeWriter<Image<Rgba32>>
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 10
                },
                Renderer = new ImageSharpRenderer<Rgba32>()
            };

            using (Image<Rgba32> image = writer.Write(text))
            using (var ms = new MemoryStream())
            {
                image.Save(ms, new PngEncoder());
                return ms.ToArray();
            }
        }
    }
}
