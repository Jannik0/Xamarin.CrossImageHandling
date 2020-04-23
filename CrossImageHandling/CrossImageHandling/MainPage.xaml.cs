using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CrossImageHandling
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private readonly IImageHandlingService imageHandlingService = DependencyService.Get<IImageHandlingService>();

        private MainPageViewModel viewModel;

        public MainPage()
        {
            InitializeComponent();
            viewModel = new MainPageViewModel();
            BindingContext = viewModel;
        }

        async void OnSelectImageButtonClicked(object sender, EventArgs e)
        {
            (sender as Button).IsEnabled = false;

            //Request permissions
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    if (await Permissions.CheckStatusAsync<Permissions.StorageWrite>() == PermissionStatus.Granted)
                    {
                        await Task.Run(() => PerformTests());
                    }
                    else
                    {
                        await Permissions.RequestAsync<Permissions.StorageWrite>();

                        if (await Permissions.CheckStatusAsync<Permissions.StorageWrite>() == PermissionStatus.Granted)
                            await Task.Run(() => PerformTests());
                        else
                            viewModel.Info = "Library access denied";
                    }
                    break;
                case Device.iOS:
                    if (await Permissions.CheckStatusAsync<Permissions.Photos>() == PermissionStatus.Granted)
                    {
                        await Task.Run(() => PerformTests());
                    }
                    else
                    {
                        await Permissions.RequestAsync<Permissions.Photos>();

                        if (await Permissions.CheckStatusAsync<Permissions.Photos>() == PermissionStatus.Granted)
                            await Task.Run(() => PerformTests());
                        else
                            viewModel.Info = "Library access denied";
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
                viewModel.Info = "Error: failed to load image file";
                return;
            }

            if (pickImageFromGalleryStream != null)
            {
                viewModel.Info = "Operations are running...";
                viewModel.Info1 = "picked image:";
                viewModel.Busy1 = true;
                viewModel.Info2 = "edited image:";
                viewModel.Busy2 = true;
                viewModel.Info3 = "edited and resized image:";
                viewModel.Busy3 = true;

                //Convert and save the image stream for further usage
                MemoryStream stream = new MemoryStream();
                pickImageFromGalleryStream.CopyTo(stream);
                pickImageFromGalleryStream.Dispose();
                streamData = stream.ToArray();
                stream.Dispose();

                //display the selected image
                viewModel.Image1 = ImageSource.FromStream(() => new MemoryStream(streamData));
                viewModel.Busy1 = false;

                //get the image width and height in pixels
                size = imageHandlingService.GetImageSizeFromStream(streamData);

                //get the pixel data as an array in RGBA format
                pixelData = imageHandlingService.GetImagePixelsFromStream(streamData);
                //directly edit the pixel data (for demonstration purposes remove red)
                for (int i = 0; i < pixelData.Length; i += 4)
                    pixelData[i] = 0;

                //convert the edited pixel data back into a stream object
                streamData = imageHandlingService.GetImageStreamFromPixels(pixelData, size[0], size[1]);

                //display the edited image
                viewModel.Image2 = ImageSource.FromStream(() => new MemoryStream(streamData));
                viewModel.Busy2 = false;

                //save the edited image to the gallery
                bool success;
                try
                {
                    success = imageHandlingService.SaveImageToGallery(streamData);
                }
                catch
                {
                    viewModel.Info = "Error: failed to save image file";
                }

                //scale the edited image down to 500 pixels wide
                streamData = imageHandlingService.GetImageStreamAtSizeFromStream(streamData, 500, (int)(size[1] / (size[0] / 500.0)));

                //display the edited and resized image
                viewModel.Image3 = ImageSource.FromStream(() => new MemoryStream(streamData));
                viewModel.Busy3 = false;

                //save the edited and resized image
                try
                {
                    success = imageHandlingService.SaveImageToGallery(streamData);
                }
                catch
                {
                    viewModel.Info = "Error: failed to save image file";
                }

                viewModel.Info = "All operations completed successfully";
            }
            else
                viewModel.Info = "No image selected";
        }
    }
}