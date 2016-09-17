using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IconX.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public static MainViewModel Instance { get; private set; }

        // compliments
        public IconData IconList { get; set; } = new IconData();

        // this commadn is for testing purpose if no microphone or whatsoever
        public RelayCommand<object> Clicked { get; set; }
        // this command will start our update mechanism and init some basics e.g. speech recognition
        public RelayCommand<object> Initzialize { get; set; }
        
        public MainViewModel()
        {
            Instance = this;
            Load(App.StartupArgument);
        }
        
        #region DATA Processing
        public void Load(string uri)
        {
            if (!string.IsNullOrEmpty(uri))
            {
                try
                {
                    IconList.LoadIcon(uri);
                }
                catch
                {
                    IconList.Clear();

                    try
                    {
                        IconList.LoadImage(uri);
                    }
                    catch
                    {
                        IconList.Clear();
                    }
                }
            }
        }

        public void Save(string file)
        {
            IconList.Save(file);
        }
        #endregion
        
    }
}
