using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AstrumApp.Services
{
    internal class FrozenImage
    {
        public static BitmapImage? CreateFromUri(string uri)
        {
            if (!Uri.TryCreate(uri, UriKind.Absolute, out var imageUri))
                return null;

            if (imageUri.IsFile && !File.Exists(imageUri.LocalPath))
                return null;

            try
            {
                var img = new BitmapImage();

                img.BeginInit();
                img.UriSource = imageUri;
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                img.EndInit();

                img.Freeze();
                return img;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
                return null;
            }
        }

        public static ImageSource CreateFromBytes(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);

            var image = new BitmapImage();

            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();

            image.Freeze();

            return image;
        }

        public static string SaveToFile(BitmapSource bitmap, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(fileStream);
            }
            return filePath;
        }

        public static bool DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                else
                {
                    //Debug.WriteLine($"Файл не найден: {filePath}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"Ошибка при удалении файла: {ex.Message}");
                return false;
            }
        }
    }
}
