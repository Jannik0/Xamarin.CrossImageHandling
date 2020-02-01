using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(CrossImageHandling.iOS.ImageHandlingService))]
namespace CrossImageHandling.iOS
{
    class ImageHandlingService : IImageHandlingService
    {
        TaskCompletionSource<Stream> taskCompletionSource;
        UIImagePickerController imagePicker;

        public Task<Stream> PickImageFromGalleryAsync()
        {
            imagePicker = new UIImagePickerController
            {
                SourceType = UIImagePickerControllerSourceType.PhotoLibrary,
                MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary)
            };

            imagePicker.FinishedPickingMedia += OnImagePickerFinishedPickingMedia;
            imagePicker.Canceled += OnImagePickerCanceled;

            UIWindow window = UIApplication.SharedApplication.KeyWindow;
            UIViewController viewController = window.RootViewController;
            viewController.PresentModalViewController(imagePicker, true);

            taskCompletionSource = new TaskCompletionSource<Stream>();
            return taskCompletionSource.Task;
        }

        void OnImagePickerFinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs args)
        {
            UIImage image = args.EditedImage ?? args.OriginalImage;

            if (image != null)
            {
                NSData data;
                if (args.ReferenceUrl.PathExtension.Equals("PNG") || args.ReferenceUrl.PathExtension.Equals("png"))
                    data = image.AsPNG();
                else
                    data = image.AsJPEG(1);
                Stream stream = data.AsStream();

                UnregisterEventHandlers();

                taskCompletionSource.SetResult(stream);
            }
            else
            {
                UnregisterEventHandlers();

                taskCompletionSource.SetResult(null);
            }

            imagePicker.DismissModalViewController(true);
        }

        void OnImagePickerCanceled(object sender, EventArgs args)
        {
            UnregisterEventHandlers();

            taskCompletionSource.SetResult(null);

            imagePicker.DismissModalViewController(true);
        }

        void UnregisterEventHandlers()
        {
            imagePicker.FinishedPickingMedia -= OnImagePickerFinishedPickingMedia;
            imagePicker.Canceled -= OnImagePickerCanceled;
        }

        public int[] GetImageSizeFromStream(byte[] streamData)
        {
            Stream stream = new MemoryStream(streamData);
            UIImage uiImage = UIImage.LoadFromData(NSData.FromStream(stream));
            stream.Dispose();

            int width = (int)uiImage.CGImage.Width;
            int height = (int)uiImage.CGImage.Height;

            return new int[] { width, height };
        }

        public byte[] GetImagePixelsFromStream(byte[] streamData)
        {
            Stream stream = new MemoryStream(streamData);
            UIImage uiImage = UIImage.LoadFromData(NSData.FromStream(stream));
            stream.Dispose();

            int width = (int)uiImage.CGImage.Width;
            int height = (int)uiImage.CGImage.Height;
            byte[] pixelData = new byte[width * height * 4];

            using (CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB())
            {
                IntPtr rawData = Marshal.AllocHGlobal(width * height * 4);
                using (CGBitmapContext bitmapContext = new CGBitmapContext(rawData, width, height, 8, 4 * width, colorSpace, CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast))
                {
                    bitmapContext.DrawImage(new CGRect(0, 0, width, height), uiImage.CGImage);
                    Marshal.Copy(rawData, pixelData, 0, pixelData.Length);
                    Marshal.FreeHGlobal(rawData);
                }
            }

            return pixelData;
        }

        public byte[] GetImageStreamFromPixels(byte[] pixelData, int width, int height)
        {
            byte[] streamData;

            using (CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB())
            {
                using (CGBitmapContext bitmapContext = new CGBitmapContext(pixelData, width, height, 8, 4 * width, colorSpace, CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast))
                {
                    CGImage cgImage = bitmapContext.ToImage();
                    UIImage uiImage = new UIImage(cgImage);
                    NSData nsData = uiImage.AsPNG();
                    MemoryStream stream = new MemoryStream();
                    nsData.AsStream().CopyTo(stream);
                    streamData = stream.ToArray();
                    stream.Dispose();
                }
            }

            return streamData;
        }

        public byte[] GetImageStreamAtSizeFromStream(byte[] streamData, int targetWidth, int targetHeight)
        {
            Stream stream = new MemoryStream(streamData);
            UIImage uiImage = UIImage.LoadFromData(NSData.FromStream(stream));
            stream.Dispose();

            UIGraphics.BeginImageContext(new CGSize(targetWidth, targetHeight));
            uiImage.Draw(new CGRect(0, 0, targetWidth, targetHeight));
            UIImage scaledUIImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            NSData nsData = scaledUIImage.AsPNG();
            MemoryStream scaledStream = new MemoryStream();
            nsData.AsStream().CopyTo(scaledStream);
            byte[] scaledStreamData = scaledStream.ToArray();
            scaledStream.Dispose();

            return scaledStreamData;
        }

        public bool SaveImageToGallery(byte[] streamData)
        {
            bool success = true;

            Stream stream = new MemoryStream(streamData);
            UIImage uiImage = UIImage.LoadFromData(NSData.FromStream(stream));
            stream.Dispose();

            uiImage.SaveToPhotosAlbum((returnValue, error) =>
            {
                UIImage image = returnValue as UIImage;
                if (error != null)
                    success = false;
            });

            return success;
        }
    }
}