using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Streaming_RTSP.Core.Enums;
using Streaming_RTSP.Services;
using Streaming_RTSP.Services.Interfaces;
using Xunit;

namespace Streaming_RTSP.Services.Tests
{
    /// <summary>
    /// Testes para o serviço de salvamento e recuperação de imagens locais.
    /// Implementa o padrão AAA (Arrange-Act-Assert) com melhores práticas de teste unitário.
    /// </summary>
    public class LocalImageServiceTests : IDisposable
    {
        private readonly ILocalImageService _service;

        public LocalImageServiceTests()
        {
            // Arrange: Inicializar o serviço de imagens
            _service = new LocalImageService();
        }

        #region SaveFrame Tests

        [Fact(DisplayName = "SaveFrame deve salvar imagem PNG com sucesso")]
        public void SaveFrame_WithPngFormat_ShouldSaveFileSuccessfully()
        {
            // Arrange
            var bitmapSource = CreateSampleBitmapSource(100, 100);

            // Act
            var filePath = _service.SaveFrame(bitmapSource, ImageFormat.PNG);

            // Assert
            Assert.NotNull(filePath);
            Assert.True(File.Exists(filePath), "Arquivo PNG deveria ter sido criado");
            Assert.EndsWith(".png", filePath.ToLower());
            Assert.True(new FileInfo(filePath).Length > 0, "Arquivo não deveria estar vazio");

            // Cleanup
            File.Delete(filePath);
        }

        [Fact(DisplayName = "SaveFrame deve salvar imagem JPEG com sucesso")]
        public void SaveFrame_WithJpegFormat_ShouldSaveFileSuccessfully()
        {
            // Arrange
            var bitmapSource = CreateSampleBitmapSource(100, 100);

            // Act
            var filePath = _service.SaveFrame(bitmapSource, ImageFormat.JPEG);

            // Assert
            Assert.NotNull(filePath);
            Assert.True(File.Exists(filePath), "Arquivo JPEG deveria ter sido criado");
            Assert.EndsWith(".jpeg", filePath.ToLower());
            Assert.True(new FileInfo(filePath).Length > 0, "Arquivo não deveria estar vazio");

            // Cleanup
            File.Delete(filePath);
        }

        [Fact(DisplayName = "SaveFrame deve lançar ArgumentNullException quando BitmapSource é nulo")]
        public void SaveFrame_WithNullBitmapSource_ShouldThrowArgumentNullException()
        {
            // Arrange
            BitmapSource? nullBitmap = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => 
                _service.SaveFrame(nullBitmap!, ImageFormat.PNG));
            
            Assert.Equal("source", ex.ParamName);
        }

        [Fact(DisplayName = "SaveFrame deve criar diretório automaticamente se não existir")]
        public void SaveFrame_WithMissingDirectory_ShouldCreateDirectory()
        {
            // Arrange
            var bitmapSource = CreateSampleBitmapSource(100, 100);

            // Act
            var filePath = _service.SaveFrame(bitmapSource, ImageFormat.PNG);

            // Assert
            var directory = Path.GetDirectoryName(filePath);
            Assert.NotNull(directory);
            Assert.True(Directory.Exists(directory), "Diretório deveria ter sido criado automaticamente");

            // Cleanup
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        #endregion

        #region GetSavedImagesPaths Tests

        [Fact(DisplayName = "GetSavedImagesPaths deve retornar lista vazia ou apenas imagens existentes")]
        public void GetSavedImagesPaths_ShouldReturnListType()
        {
            // Act
            var paths = _service.GetSavedImagesPaths();

            // Assert
            Assert.NotNull(paths);
            Assert.IsType<List<string>>(paths);
        }

        [Fact(DisplayName = "GetSavedImagesPaths deve retornar apenas PNG e JPEG")]
        public void GetSavedImagesPaths_WithSavedImages_ShouldReturnOnlyImageFormats()
        {
            // Arrange
            var bitmapSource = CreateSampleBitmapSource(100, 100);
            
            // Salvar imagens
            var pngPath = _service.SaveFrame(bitmapSource, ImageFormat.PNG);
            var jpegPath = _service.SaveFrame(bitmapSource, ImageFormat.JPEG);

            // Act
            var paths = _service.GetSavedImagesPaths();

            // Assert
            Assert.NotNull(paths);
            Assert.Contains(pngPath, paths);
            Assert.Contains(jpegPath, paths);

            // Cleanup
            File.Delete(pngPath);
            File.Delete(jpegPath);
        }

        [Fact(DisplayName = "GetSavedImagesPaths deve retornar caminhos completos válidos")]
        public void GetSavedImagesPaths_ShouldReturnValidFullPaths()
        {
            // Arrange
            var bitmapSource = CreateSampleBitmapSource(100, 100);
            var savedPath = _service.SaveFrame(bitmapSource, ImageFormat.PNG);

            // Act
            var paths = _service.GetSavedImagesPaths();

            // Assert
            Assert.NotEmpty(paths);
            foreach (var path in paths)
            {
                Assert.True(Path.IsPathRooted(path), "Caminho deveria ser absoluto");
                Assert.True(File.Exists(path), "Arquivo deveria existir no caminho retornado");
            }

            // Cleanup
            if (File.Exists(savedPath))
                File.Delete(savedPath);
        }

        #endregion

        #region Integration Tests

        [Fact(DisplayName = "Fluxo completo: Salvar imagem e recuperar da lista")]
        public void IntegrationTest_SaveAndRetrieve_ShouldWorkCorrectly()
        {
            // Arrange
            var bitmapSource = CreateSampleBitmapSource(200, 150);
            var initialPaths = _service.GetSavedImagesPaths();
            var initialCount = initialPaths.Count;

            // Act
            var savedPath = _service.SaveFrame(bitmapSource, ImageFormat.PNG);
            var updatedPaths = _service.GetSavedImagesPaths();

            // Assert
            Assert.True(updatedPaths.Count >= initialCount + 1);
            Assert.Contains(savedPath, updatedPaths);
            Assert.True(File.Exists(savedPath));

            // Cleanup
            if (File.Exists(savedPath))
                File.Delete(savedPath);
        }

        [Fact(DisplayName = "Fluxo completo: Salvar múltiplas imagens em diferentes formatos")]
        public void IntegrationTest_SaveMultipleFormats_ShouldStoreAllFormats()
        {
            // Arrange
            var bitmapSource = CreateSampleBitmapSource(100, 100);
            var savedPaths = new List<string>();

            // Act
            savedPaths.Add(_service.SaveFrame(bitmapSource, ImageFormat.PNG));
            savedPaths.Add(_service.SaveFrame(bitmapSource, ImageFormat.JPEG));
            savedPaths.Add(_service.SaveFrame(bitmapSource, ImageFormat.PNG));

            var retrievedPaths = _service.GetSavedImagesPaths();

            // Assert
            Assert.Equal(3, savedPaths.Count);
            foreach (var path in savedPaths)
            {
                Assert.Contains(path, retrievedPaths);
            }

            var pngFiles = savedPaths.Where(p => p.EndsWith(".png")).Count();
            var jpegFiles = savedPaths.Where(p => p.EndsWith(".jpeg")).Count();
            
            Assert.Equal(2, pngFiles);
            Assert.Equal(1, jpegFiles);

            // Cleanup
            foreach (var path in savedPaths)
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        #endregion

        #region Helper Methods

        private static BitmapSource CreateSampleBitmapSource(int width, int height)
        {
            var bitmap = new WriteableBitmap(
                width, 
                height, 
                96, 
                96, 
                System.Windows.Media.PixelFormats.Bgra32, 
                null);

            int bytesPerPixel = 4; // BGRA
            byte[] pixelData = new byte[width * height * bytesPerPixel];
            
            for (int i = 0; i < pixelData.Length; i += bytesPerPixel)
            {
                pixelData[i] = 255;
                pixelData[i + 1] = 255; 
                pixelData[i + 2] = 255; 
                pixelData[i + 3] = 255; 
            }

            bitmap.WritePixels(
                new System.Windows.Int32Rect(0, 0, width, height),
                pixelData,
                width * bytesPerPixel,
                0);

            return bitmap;
        }

        #endregion

        public void Dispose()
        {
            
        }
    }
}
