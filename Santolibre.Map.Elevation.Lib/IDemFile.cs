namespace Santolibre.Map.Elevation.Lib
{
    public interface IDemFile
    {
        byte[] Data { get; }
        int SizeInBytes { get; }

        void Save(string path);
        int GetElevation(double latitude, double longitude);
    }
}
