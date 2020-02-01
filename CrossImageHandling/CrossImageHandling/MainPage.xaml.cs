using System;
using System.ComponentModel;
using System.IO;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;

namespace CrossImageHandling
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private readonly IImageHandlingService imageHandlingService = DependencyService.Get<IImageHandlingService>();

        public MainPage()
        {
            InitializeComponent();
        }

        async void OnSelectImageButtonClicked(object sender, EventArgs e)
        {
            (sender as Button).IsEnabled = false;

            //Request permissions
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    if (await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage) == PermissionStatus.Granted)
                    {
                        PerformTests();
                    }
                    else
                    {
                        await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);

                        if (await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage) == PermissionStatus.Granted)
                            PerformTests();
                        else
                            Info.Text = "Library access denied";
                    }
                    break;
                case Device.iOS:
                    if (await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Photos) == PermissionStatus.Granted)
                    {
                        PerformTests();
                    }
                    else
                    {
                        await CrossPermissions.Current.RequestPermissionsAsync(Permission.Photos);

                        if (await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Photos) == PermissionStatus.Granted)
                            PerformTests();
                        else
                            Info.Text = "Library access denied";
                    }
                    break;
            }

            (sender as Button).IsEnabled = true;
        }

        async void PerformTests()
        {
            int[] size;
            byte[] streamData;
            byte[] pixelData;

            //Let the user pick an image from the gallery
            Stream pickImageFromGalleryStream = null;
            try
            {
                pickImageFromGalleryStream = await imageHandlingService.PickImageFromGalleryAsync();
            }
            catch
            {
                Info.Text = "Error: failed to load image file";
                return;
            }

            if (pickImageFromGalleryStream != null)
            {
                //Convert and save the image stream for further usage
                MemoryStream stream = new MemoryStream();
                pickImageFromGalleryStream.CopyTo(stream);
                pickImageFromGalleryStream.Dispose();
                streamData = stream.ToArray();
                stream.Dispose();

                //display the selected image
                Image1.Source = ImageSource.FromStream(() => new MemoryStream(streamData));

                //get the image width and height in pixels
                size = imageHandlingService.GetImageSizeFromStream(streamData);

                //get the pixel data as an array in RGBA format
                pixelData = imageHandlingService.GetImagePixelsFromStream(streamData);
                //directly edit the pixel data (for demonstration purposes)
                for (int i = 0; i < pixelData.Length; i += 10)
                    pixelData[i] = 0;

                //convert the edited pixel data back into a stream object
                streamData = imageHandlingService.GetImageStreamFromPixels(pixelData, size[0], size[1]);

                //display the edited image
                Image2.Source = ImageSource.FromStream(() => new MemoryStream(streamData));

                //save the edited image to the gallery
                bool success;
                try
                {
                    success = imageHandlingService.SaveImageToGallery(streamData);
                }
                catch
                {
                    Info.Text = "Error: failed to save image file";
                }

                //scale the edited image down to half its original size
                streamData = imageHandlingService.GetImageStreamAtSizeFromStream(streamData, size[0] / 2, size[1] / 2);

                //display the edited and resized image
                Image3.Source = ImageSource.FromStream(() => new MemoryStream(streamData));

                //save the edited and resized image
                try
                {
                    success = imageHandlingService.SaveImageToGallery(streamData);
                }
                catch
                {
                    Info.Text = "Error: failed to save image file";
                }

                Info.Text = "All operations completed successfully";
            }
            else
                Info.Text = "No image selected";
        }
    }
}