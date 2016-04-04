using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;

namespace LagerLista
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                "Sorry, an error occured causing program to close.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK);

            string putanja2 = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            FileStream fs2 = new FileStream(putanja2 + @"\Lager lista files\izuzeci.txt", FileMode.Append, FileAccess.Write);
            StreamWriter fssw2 = new StreamWriter(fs2);
            fssw2.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + "************************** UNHANDLED " + System.DateTime.Now.ToString() + Environment.NewLine + e.Exception.Message + Environment.NewLine + Environment.NewLine + e.Exception.StackTrace + Environment.NewLine);
            fssw2.Close();
        }
    }
}
