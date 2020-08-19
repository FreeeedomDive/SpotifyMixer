using System;
using System.Diagnostics;
using SpotifyMixer.Core;

namespace SpotifyMixer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            {
                var message = $"{eventArgs.Exception.Message}" +
                              $"\n{eventArgs.Exception.StackTrace}";
                Utility.ShowErrorMessage(message, "Exception");
            };
        }
    }
}