using System;
using System.IO;

namespace Santolibre.Map.Elevation.Lib
{
    public class HgtRaw : IDemFile
    {
        public const int HGT3601 = 25934402;
        public const int HGT1201 = 2884802;

        public byte[] Data { get; }

        protected HgtRaw(byte[] data)
        {
            if (data.Length != HGT1201 && data.Length != HGT3601)
            {
                throw new Exception("HGT file has no valid size");
            }

            Data = data;
        }

        public static IDemFile Create(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return Create(stream);
            }
        }

        public static IDemFile Create(Stream stream)
        {
            var data = new byte[stream.Length];
            stream.Read(data, 0, Convert.ToInt32(stream.Length));
            stream.Close();

            return Create(data);
        }

        public static IDemFile Create(byte[] data)
        {
            return new HgtRaw(data);
        }

        public virtual void Save(string path)
        {
            File.WriteAllBytes(path, Data);
        }

        public static string GetFilename(double latitude, double longitude)
        {
            char latDir;
            char lonDir;
            int latAdj;
            int lonAdj;

            if (latitude < 0)
            {
                latDir = 'S';
                latAdj = 1;
            }
            else
            {
                latDir = 'N';
                latAdj = 0;
            }
            if (longitude < 0)
            {
                lonDir = 'W';
                lonAdj = 1;
            }
            else
            {
                lonDir = 'E';
                lonAdj = 0;
            }

            var latString = latDir + ((int)Math.Floor(latitude + latAdj)).ToString("00");
            var lonString = lonDir + ((int)Math.Floor(longitude + lonAdj)).ToString("000");
            return latString + lonString + ".hgt";
        }

        public int GetElevation(double latitude, double longitude)
        {
            int latAdj = latitude < 0 ? 1 : 0;
            int lonAdj = longitude < 0 ? 1 : 0;

            switch (Data.Length)
            {
                case HGT1201:
                    return GetElevation(latitude, longitude, latAdj, lonAdj, 1200, 2402);
                default:
                    return GetElevation(latitude, longitude, latAdj, lonAdj, 3600, 7202);
            }
        }

        private int GetElevation(double latitude, double longitude, int latAdj, int lonAdj, int width, int stride)
        {
            double y = latitude;
            double x = longitude;
            var offset = ((int)((x - (int)x + lonAdj) * width) * 2 + (width - (int)((y - (int)y + latAdj) * width)) * stride);
            var h1 = Data[offset + 1] + Data[offset + 0] * 256;
            var h2 = Data[offset + 3] + Data[offset + 2] * 256;
            var h3 = Data[offset - stride + 1] + Data[offset - stride + 0] * 256;
            var h4 = Data[offset - stride + 3] + Data[offset - stride + 2] * 256;

            var m = Math.Max(h1, Math.Max(h2, Math.Max(h3, h4)));
            if (h1 == -32768)
                h1 = m;
            if (h2 == -32768)
                h2 = m;
            if (h3 == -32768)
                h3 = m;
            if (h4 == -32768)
                h4 = m;

            var fx = longitude - (int)(longitude);
            var fy = latitude - (int)(latitude);

            var elevation = (int)Math.Round((h1 * (1 - fx) + h2 * fx) * (1 - fy) + (h3 * (1 - fx) + h4 * fx) * fy);

            return elevation < -1000 ? 0 : elevation;
        }

        public (byte[] Data, int Width, int Height) GetData()
        {
            switch (Data.Length)
            {
                case HGT1201:
                    return (Data, 1201, 1201);
                default:
                    return (Data, 3601, 3601);
            }
        }
    }
}
