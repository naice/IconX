using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace IconX.ViewModel
{
    public class IconData : BaseViewModel
    {
        public ObservableCollection<Icon> Icons { get; set; } = new ObservableCollection<Icon>();
        public bool HasIcons { get { return _micon != null && _micon.Count > 0; } }
        private MultiIcon _micon;
        private WriteableBitmap _sourceImage;

        public DateTime LastUpdate { get; set; } = DateTime.MinValue;
        public SemaphoreSlim UILock { get; set; } = new SemaphoreSlim(1, 1);
        public TimeSpan UpdateTimeout { get; set; } = TimeSpan.FromMinutes(1);

        public void LoadIcon(string file)
        {
            Icons.Clear();

            _micon = new MultiIcon();
            _micon.Load(file);

            for (int i = 0; i < _micon.Count; i++)
            {
                var icon = new Icon();
                icon.Update(_micon[i]);
                Icons.Add(icon);
            }
        }

        public void LoadImage(string file)
        {
            Icons.Clear();


            _micon = new MultiIcon();
            var uri = new System.Uri(file);
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = uri;
            bmp.UriCachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();

            _sourceImage = new WriteableBitmap(bmp);
            CreateFromSource();
        }

        private void CreateFromSource()
        {
            _micon.CreateMipMap6(_sourceImage);

            for (int i = 0; i < _micon.Count; i++)
            {
                var icon = new Icon();
                icon.Update(_micon[i]);
                Icons.Add(icon);
            }
        }

        public void Save(string file)
        {
            if (HasIcons)
            {
                _micon.Save(file);
            }
            else
            {
                throw new Exception("No icons loaded.");
            }
        }

        public void Clear()
        {
            Icons.Clear();
            _micon = null;
        }
    }
}
