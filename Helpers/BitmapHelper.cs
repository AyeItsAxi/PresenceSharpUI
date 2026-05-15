using System;
using System.Windows.Media.Imaging;

namespace PresenceSharpUI.Helpers;

public static class BitmapHelper
{
    public static BitmapImage Create(string uri)
    {
        var bitmap = new BitmapImage();

        bitmap.BeginInit();
        bitmap.UriSource = new Uri(uri, UriKind.Absolute);
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();

        return bitmap;
    }
}