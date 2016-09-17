using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace IconX.ViewModel
{
    public class Icon : BaseViewModel
    {
        private int _Width;
        public int Width
        {
            get { return _Width; }
            set
            {
                if (value != _Width)
                {
                    _Width = value;
                    RaisePropertyChanged("Width");
                }
            }
        }
        private int _Height;
        public int Height
        {
            get { return _Height; }
            set
            {
                if (value != _Height)
                {
                    _Height = value;
                    RaisePropertyChanged("Height");
                }
            }
        }
        public BitmapSource Source { get; set; }

        private WriteableBitmap _source = null;

        public void Update(WriteableBitmap source)
        {
            _source = source;
            Source = source;
            Width = source.PixelWidth;
            Height = source.PixelHeight;
        }
    }
}
