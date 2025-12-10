using Streaming_RTSP.Core.Enums;
using Streaming_RTSP.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;

namespace Streaming_RTSP.Services
{
    public class LocalImageService : ILocalImageService
    {
        private readonly string _baseDirectory;

        public LocalImageService()
        {
            string programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            _baseDirectory = Path.Combine(programDataPath, "streamingRTSP", "Snapshots");

            EnsureDirectoryExists();
        }

        /// <summary>
        /// Cria o diretório caso não exista.
        /// </summary>
        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_baseDirectory))
            {
                Directory.CreateDirectory(_baseDirectory);
            }
        }

        public string SaveFrame(BitmapSource source, ImageFormat format)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), "BitmapSource não pode ser nulo.");

            BitmapEncoder encoder = format == ImageFormat.PNG
                ? (BitmapEncoder)new PngBitmapEncoder()
                : new JpegBitmapEncoder();

            string extension = format.ToString().ToLower();
            string fileName = $"frame_{DateTime.Now:yyyyMMdd_HHmmssfff}.{extension}";
            string fullPath = Path.Combine(_baseDirectory, fileName);

            try
            {
                encoder.Frames.Add(BitmapFrame.Create(source));

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    encoder.Save(stream);
                }
                return fullPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar imagem: {ex.Message}");
                throw;
            }
        }

        public List<string> GetSavedImagesPaths()
        {
            string[] searchPatterns = new string[] { "*.png", "*.jpeg" };

            var files = new List<string>();

            foreach (var pattern in searchPatterns)
            {
                files.AddRange(Directory.EnumerateFiles(_baseDirectory, pattern, SearchOption.TopDirectoryOnly));
            }

            return files.OrderByDescending(f => f).ToList();
        }
    }
}
