using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace IconX
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public static string StartupArgument { get; set; }
        public static Dispatcher UIDispatcher { get; set; }

        public static bool IsElevated
        {
            get
            {
                return new WindowsPrincipal
                    (WindowsIdentity.GetCurrent()).IsInRole
                    (WindowsBuiltInRole.Administrator);
            }
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            UIDispatcher = Current.Dispatcher;
            StartupArgument = e.Args.FirstOrDefault();



            if (StartupArgument == "mipmap6")
            {
                ViewModel.IconData d = new ViewModel.IconData();
                try
                {
                    string file = e.Args[1];
                    string dest = Path.Combine(Path.GetDirectoryName(file), string.Format("{0}.ico", Path.GetFileNameWithoutExtension(file)));

                    d.LoadImage(e.Args[1]);

                    if (File.Exists(dest))
                    {
                        var r = MessageBox.Show($"File {dest} already exists, overwrite?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (r != MessageBoxResult.Yes)
                        {
                            App.Current.Shutdown();
                            return;
                        }
                    }

                    d.Save(dest);

                    MessageBox.Show($"File {dest} created.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                }

                App.Current.Shutdown();
            }
            else if (StartupArgument == "contextmenu")
            {
                try
                {
                    if (IsElevated)
                    {
                        string executingAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        ShellContextMenu scMenu = new ShellContextMenu();
                        scMenu.Items.Add(
                            new ShellContextMenuItem()
                            {
                                FileKey = "*",
                                Name = "iconxmipmap",
                                Caption = "IconX - CreateMipMap6",
                                Icon = executingAssembly,
                                ExePath = string.Format("\"{0}\" mipmap6 \"%V\"", executingAssembly),
                            });
                        scMenu.Items.Add(
                            new ShellContextMenuItem()
                            {
                                FileKey = "*",
                                Name = "iconxopen",
                                Caption = "IconX - Open",
                                Icon = executingAssembly,
                                ExePath = string.Format("\"{0}\" \"%V\"", executingAssembly),
                            });
                        scMenu.Save();

                        MessageBox.Show("Shell Context menu created");
                    }
                    else
                    {
                        MessageBox.Show("Needs to run with admin privileges.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                App.Current.Shutdown();
            }

            base.OnStartup(e);
        }
    }
}
