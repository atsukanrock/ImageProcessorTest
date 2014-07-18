using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageProcessor;
using ImageProcessor.Imaging.Filters;

namespace ImageProcessorTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private string _sourceImageFilePath;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SourceImageOnPreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop, true) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void SourceImageOnDrop(object sender, DragEventArgs e)
        {
            _sourceImageFilePath = null;
            this.SourceImage.Source = null;
            this.PleaseDropMessageTextBlock.Visibility = Visibility.Visible;

            var filePaths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (filePaths == null || !filePaths.Any()) return;

            foreach (var filePath in filePaths)
            {
                ImageSource imageSource;
                try
                {
                    imageSource = new BitmapImage(new Uri(filePath));
                }
                catch (Exception ex)
                {
                    Log("{0} is not an image file.", filePath);
                    Log(ex.ToString());
                    continue;
                }
                _sourceImageFilePath = filePath;
                this.SourceImage.Source = imageSource;
                this.PleaseDropMessageTextBlock.Visibility = Visibility.Collapsed;
                return;
            }
        }

        private void ProcessButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (_sourceImageFilePath == null)
            {
                Log("No file is loaded.");
                return;
            }
            if (!File.Exists(_sourceImageFilePath))
            {
                Log("File is not found.");
                return;
            }

            try
            {
                var inBuffer = File.ReadAllBytes(_sourceImageFilePath);
                using (var inStream = new MemoryStream(inBuffer))
                {
                    using (var outStream = new MemoryStream())
                    {
                        using (var imageFactory = new ImageFactory())
                        {
                            var stopwatch = Stopwatch.StartNew();

                            Proccess(imageFactory.Load(inStream)).Save(outStream);

                            stopwatch.Stop();
                            Log("Processing time: {0:#,##0} ms", stopwatch.ElapsedMilliseconds);
                        }

                        outStream.Position = 0;
                        var imageSource = new BitmapImage();
                        imageSource.BeginInit();
                        imageSource.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        imageSource.CacheOption = BitmapCacheOption.OnLoad;
                        imageSource.UriSource = null;
                        imageSource.StreamSource = outStream;
                        imageSource.EndInit();
                        imageSource.Freeze();

                        this.DestImage.Source = imageSource;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("An error occured while processing the image.");
                Log(ex.ToString());
            }
        }

        private ImageFactory Proccess(ImageFactory imageFactory)
        {
            if (AlphaCheckBox.IsChecked.GetValueOrDefault())
            {
                imageFactory = imageFactory.Alpha(80);
            }
            if (BrightnessCheckBox.IsChecked.GetValueOrDefault())
            {
                imageFactory = imageFactory.Brightness(20);
            }
            if (ContrastCheckBox.IsChecked.GetValueOrDefault())
            {
                imageFactory = imageFactory.Contrast(20);
            }
            if (FilterComicCheckBox.IsChecked.GetValueOrDefault())
            {
                imageFactory = imageFactory.Filter(MatrixFilters.Comic);
            }
            if (FilterGothamCheckBox.IsChecked.GetValueOrDefault())
            {
                imageFactory = imageFactory.Filter(MatrixFilters.Gotham);
            }
            if (FilterLoSatchCheckBox.IsChecked.GetValueOrDefault())
            {
                imageFactory = imageFactory.Filter(MatrixFilters.LoSatch);
            }
            if (FilterHiSatchCheckBox.IsChecked.GetValueOrDefault())
            {
                imageFactory = imageFactory.Filter(MatrixFilters.HiSatch);
            }
            if (FilterPolaroidCheckBox.IsChecked.GetValueOrDefault())
            {
                imageFactory = imageFactory.Filter(MatrixFilters.Polaroid);
            }
            if (GaussianBlurCheckBox.IsChecked.GetValueOrDefault())
            {
                imageFactory = imageFactory.GaussianBlur(15);
            }
            return imageFactory;
        }

        private void Log(string format, params object[] args)
        {
            this.LogTextBox.AppendText(string.Format(format, args));
            this.LogTextBox.AppendText(Environment.NewLine);
            this.LogTextBox.ScrollToEnd();

            Debug.WriteLine(format, args);
        }
    }
}