using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Java.Nio;
using Xamarin.Forms;

[assembly: Dependency(typeof(CrossImageHandling.Droid.ImageHandlingService))]
namespace CrossImageHandling.Droid
{
    class ImageHandlingService : IImageHandlingService
    {
        public Task<Stream> PickImageFromGalleryAsync()
        {
            Intent intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);

            MainActivity.Instance.StartActivityForResult(Intent.CreateChooser(intent, "Select Image"), MainActivity.PickImageId);

            MainActivity.Instance.PickImageTaskCompletionSource = new TaskCompletionSource<Stream>();

            return MainActivity.Instance.PickImageTaskCompletionSource.Task;

        }

        public int[] GetImageSizeFromStream(byte[] streamData)
        {
            Stream stream = new MemoryStream(streamData);
            Bitmap bitmap = BitmapFactory.DecodeStream(stream);
            stream.Dispose();

            int width = bitmap.Width;
            int height = bitmap.Height;

            return new int[] { width, height };
        }

        public byte[] GetImagePixelsFromStream(byte[] streamData)
        {
            Stream stream = new MemoryStream(streamData);
            Bitmap bitmap = BitmapFactory.DecodeStream(stream);
            stream.Dispose();

            int width = bitmap.Width;
            int height = bitmap.Height;
            byte[] pixelData = new byte[width * height * 4];

            ByteBuffer byteBuffer = ByteBuffer.AllocateDirect(width * height * 4);
            bitmap.CopyPixelsToBuffer(byteBuffer);
            Marshal.Copy(byteBuffer.GetDirectBufferAddress(), pixelData, 0, width * height * 4);
            byteBuffer.Dispose();

            return pixelData;
        }

        public byte[] GetImageStreamFromPixels(byte[] bytes, int width, int height)
        {
            ByteBuffer byteBuffer = ByteBuffer.AllocateDirect(width * height * 4);
            Marshal.Copy(bytes, 0, byteBuffer.GetDirectBufferAddress(), bytes.Length);
            Bitmap bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
            bitmap.CopyPixelsFromBuffer(byteBuffer);
            byteBuffer.Dispose();

            MemoryStream stream = new MemoryStream();
            bitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
            byte[] streamData = stream.ToArray();
            stream.Dispose();

            return streamData;
        }

        public byte[] GetImageStreamAtSizeFromStream(byte[] streamData, int targetWidth, int targetHeight)
        {
            Stream stream = new MemoryStream(streamData);
            Bitmap bitmap = BitmapFactory.DecodeStream(stream);
            stream.Dispose();

            Bitmap scaledBitmap = Bitmap.CreateScaledBitmap(bitmap, targetWidth, targetHeight, false);

            MemoryStream scaledStream = new MemoryStream();
            scaledBitmap.Compress(Bitmap.CompressFormat.Png, 100, scaledStream);
            byte[] scaledStreamData = scaledStream.ToArray();
            scaledStream.Dispose();

            return scaledStreamData;
        }

        public bool SaveImageToGallery(byte[] streamData)
        {
            bool success = true;

            Stream stream = new MemoryStream(streamData);
            Bitmap bitmap = BitmapFactory.DecodeStream(stream);
            stream.Dispose();

            var folder = new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), "CrossImageHandling");
            if (!folder.Exists())
                folder.Mkdirs();

            DateTime dateTime = DateTime.Now;
            var file = new Java.IO.File(folder, dateTime.ToString("yyyyMMddHHmmss") + ".png");
            int n = 0;
            while (file.Exists())
            {
                n++;
                file = new Java.IO.File(folder, dateTime.ToString("yyyyMMddHHmmss") + "_" + n.ToString() + ".png");
            }

            using (FileStream fileStream = new FileStream(file.AbsolutePath, FileMode.CreateNew))
            {
                success = bitmap.Compress(Bitmap.CompressFormat.Png, 100, fileStream);
            }

            Android.Media.MediaScannerConnection.ScanFile(Android.App.Application.Context, new String[] { file.AbsolutePath }, null, null);

            return success;
        }
    }
}