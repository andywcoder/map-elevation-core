using BitMiracle.LibTiff.Classic;

namespace Santolibre.Map.Elevation.Lib
{
    public class DisableOutputTiffErrorHandler : TiffErrorHandler
    {
        public override void WarningHandler(Tiff tif, string method, string format, params object[] args)
        {
        }

        public override void WarningHandlerExt(Tiff tif, object clientData, string method, string format, params object[] args)
        {
        }
    }
}
