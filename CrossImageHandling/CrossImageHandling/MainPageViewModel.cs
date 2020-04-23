using System.ComponentModel;
using Xamarin.Forms;

namespace CrossImageHandling
{
    class MainPageViewModel : INotifyPropertyChanged
    {
        private string _info;
        public string Info
        {
            get { return _info; }
            set
            {
                _info = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Info)));
            }
        }

        private string _info1;
        public string Info1
        {
            get { return _info1; }
            set
            {
                _info1 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Info1)));
            }
        }

        private string _info2;
        public string Info2
        {
            get { return _info2; }
            set
            {
                _info2 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Info2)));
            }
        }

        private string _info3;
        public string Info3
        {
            get { return _info3; }
            set
            {
                _info3 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Info3)));
            }
        }

        private ImageSource _image1;
        public ImageSource Image1
        {
            get { return _image1; }
            set
            {
                _image1 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image1)));
            }
        }

        private ImageSource _image2;
        public ImageSource Image2
        {
            get { return _image2; }
            set
            {
                _image2 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image2)));
            }
        }

        private ImageSource _image3;
        public ImageSource Image3
        {
            get { return _image3; }
            set
            {
                _image3 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image3)));
            }
        }

        private bool _busy1;
        public bool Busy1
        {
            get { return _busy1; }
            set
            {
                _busy1 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Busy1)));
            }
        }

        private bool _busy2;
        public bool Busy2
        {
            get { return _busy2; }
            set
            {
                _busy2 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Busy2)));
            }
        }

        private bool _busy3;
        public bool Busy3
        {
            get { return _busy3; }
            set
            {
                _busy3 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Busy3)));
            }
        }


        public MainPageViewModel()
        {
            Info = "Select an image to start the demo";
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}