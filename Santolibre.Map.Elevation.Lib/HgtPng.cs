using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Santolibre.Map.Elevation.Lib
{
    public class HgtPng : HgtRaw
    {
        protected HgtPng(byte[] data) : base(data)
        {
        }

        public new static IDemFile Create(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return Create(stream);
            }
        }

        public new static IDemFile Create(Stream stream)
        {
            using (var image = Image.Load<L16>(stream))
            {
                if (image.DangerousTryGetSinglePixelMemory(out var pixelSpan))
                {
                    var pixelArray = pixelSpan.ToArray();
                    var data = MemoryMarshal.AsBytes(pixelSpan.Span).ToArray();
                    return Create(data);
                }
                else
                {
                    throw new Exception("Can't load HGT PNG");
                }
            }
        }

        public new static IDemFile Create(byte[] data)
        {
            return new HgtPng(data);
        }

        public override void Save(string path)
        {
            var size = _data.Length == HGT1201 ? 1201 : 3601;

            using (var image = Image.LoadPixelData<L16>(_data, size, size))
            {
                image.Save(path, new PngEncoder());
            }
        }

        public new static string GetFilename(double latitude, double longitude)
        {
            return HgtRaw.GetFilename(latitude, longitude).Replace(".hgt", ".png");
        }
    }
}
