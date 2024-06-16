using BitMiracle.LibTiff.Classic;
using System;
using System.IO;

namespace Santolibre.Map.Elevation.Lib
{
    public class GeoTiff : IDemFile
    {
        private readonly short[] _data;

        public byte[] Data { get { throw new NotImplementedException(); } }

        public int SizeInBytes { get { return _data.Length * 2; } }

        private GeoTiff(short[] data)
        {
            _data = data;
        }

        public static IDemFile Create(string path)
        {
            using (var fileStream = File.Create(path))
            {
                return Create(fileStream);
            }
        }

        public static IDemFile Create(Stream stream)
        {
            using (var inputImage = Tiff.ClientOpen("geotiff", "r", stream, new TiffStream()))
            {
                var width = inputImage.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                var height = inputImage.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                var bitsPerSample = inputImage.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();
                if (inputImage.IsBigEndian())
                {
                    throw new Exception("Tiff has to be little-endian format");
                }
                if (bitsPerSample != 16)
                {
                    throw new Exception("Tiff pixel format has to be 16 bit grayscale");
                }
                var bytes = new byte[width * height * bitsPerSample / 8];
                var offset = 0;
                for (int i = 0; i < inputImage.NumberOfStrips(); i++)
                {
                    offset += inputImage.ReadRawStrip(i, bytes, offset, (int)inputImage.RawStripSize(i));
                }
                var data = new short[bytes.Length / 2];
                for (var i = 0; i < data.Length; i++)
                {
                    data[i] = (short)(ushort)(bytes[i * 2] + (ushort)(bytes[i * 2 + 1] << 8));
                }
                return Create(data);
            }
        }

        public static IDemFile Create(short[] data)
        {
            return new GeoTiff(data);
        }

        public void Save(string path)
        {
            throw new NotImplementedException();
        }

        public static string GetFilename(double lat, double lon)
        {
            var latIndex = (int)((60 - lat) / 5) + 1;
            var lonIndex = (int)(lon / 5) + 37;
            if (lon < 0)
                lonIndex--;
            return "srtm_" + lonIndex.ToString("00") + "_" + latIndex.ToString("00") + ".tif";
        }

        public int GetElevation(double latitude, double longitude)
        {
            double minLon = (int)longitude - (int)longitude % 5;
            double maxLat = (int)latitude - (int)latitude % 5 + 5;
            if (latitude < 0)
                maxLat -= 5;

            var x = (int)Math.Round((longitude - 5.0 / 6000 - minLon) / (5.0 / 6000));
            var y = (int)Math.Round((maxLat - latitude) / (5.0 / 6000));
            y = Math.Min(y, 5999);

            var elevation = _data[y * 6000 + x];

            return elevation < -1000 ? 0 : elevation;
        }
    }
}
