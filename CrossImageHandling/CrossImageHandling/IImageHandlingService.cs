using System.IO;
using System.Threading.Tasks;

namespace CrossImageHandling
{
    public interface IImageHandlingService
    {
        Task<Stream> PickImageFromGalleryAsync();
        int[] GetImageSizeFromStream(byte[] streamData);
        byte[] GetImagePixelsFromStream(byte[] streamData);
        byte[] GetImageStreamFromPixels(byte[] pixelData, int width, int height);
        byte[] GetImageStreamAtSizeFromStream(byte[] streamData, int targetWidth, int targetHeight);
        bool SaveImageToGallery(byte[] streamData);
    }
}