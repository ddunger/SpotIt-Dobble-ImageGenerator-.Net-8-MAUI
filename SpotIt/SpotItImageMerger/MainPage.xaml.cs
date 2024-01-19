using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Threading;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using Image = Microsoft.Maui.Controls.Image;
using Point = System.Drawing.Point;

namespace SpotItImageMerger
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        public int count { get; set; }

        private string _selectedSaveLocation { get; set; }
        public string selectedSaveLocation
        {
            get { return _selectedSaveLocation; }
            set
            {
                if (_selectedSaveLocation != value)
                {
                    _selectedSaveLocation = value;
                    OnPropertyChanged(nameof(selectedSaveLocation));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool rotationEnabled { get; set; }
        private string _imageNumber { get; set; }
        public string imageNumber
        {
            get { return _imageNumber; }
            set
            {
                if (_imageNumber != value)
                {
                    _imageNumber = value;
                    OnPropertyChanged(nameof(imageNumber));
                }
            }
        }

        private ObservableCollection<ImageSource> _imageCollection = new ObservableCollection<ImageSource>();

        public ObservableCollection<ImageSource> ImageCollection
        {
            get { return _imageCollection; }
            set
            {
                if (_imageCollection != value)
                {
                    _imageCollection = value;
                    OnPropertyChanged(nameof(ImageCollection));
                }
            }
        }
        public ObservableCollection<ImageSource> MergeCollection = new ObservableCollection<ImageSource>();


        //////////////////////////////////////////////////
        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            imageNumber = "0";
            rotationEnabled = false;
            selectedSaveLocation = string.Empty;
        }
        //////////////////////////////////////////////////

        async void OnImportImagesClicked(object sender, EventArgs e)
        {
            count = 0;
            imageNumber = count.ToString();
            ImageCollection.Clear();
            MergeCollection.Clear();
            Thumbnails.Clear();

            var result = await FilePicker.PickMultipleAsync(new PickOptions
            {
                PickerTitle = "Select images",
                FileTypes = FilePickerFileType.Images
            });

            if (result == null)
            {
                return;
            }

            if (result != null)
            {
                foreach (var file in result)
                {
                    DisplayThumbnail(file);
                    ImageCollection.Add(file.FullPath);
                    MergeCollection.Add(file.FullPath);
                    count++;
                }
                imageNumber = count.ToString();
            }
        }

        void OpenOutputFolder()
        {
            System.Diagnostics.Process.Start("explorer.exe", selectedSaveLocation);

        }

        void DisplayThumbnail(FileResult file)
        {
            // Resize the image to fit into a 100x100 square
            using (var stream = File.OpenRead(file.FullPath))
            using (var originalBitmap = new Bitmap(stream))
            {
                var resizedBitmap = ResizeImage(originalBitmap, 100, 100);

                // Display image thumbnails (you can customize this part as needed)
                var image = new Image
                {
                    Source = ImageSource.FromStream(() => GetStreamFromBitmap(resizedBitmap)),
                    WidthRequest = 100,
                    HeightRequest = 100,
                    Aspect = Aspect.AspectFit,
                };
                Thumbnails.Children.Add(image); // Display thumbnail.

                // Store the imported image path for later use
                Debug.WriteLine(file.FullPath.ToString());

                ImageCollection.Add(file.FullPath);
            }
        }

        Stream GetStreamFromBitmap(Bitmap bitmap)
        {
            var stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
        async void OnCreateCards13Clicked(object sender, EventArgs e)
        {
            if (count == 13)
            {
                MergeImagesAsync13();

            }
            else
            {
                await DisplayAlert("Alert", "13 images needed to generate cards, but you added " + count, "OK");
            }
        }
        async void OnCreateCards31Clicked(object sender, EventArgs e)
        {
            if (count == 31)
            {
                MergeImagesAsync31();
            }
            else
            {
                await DisplayAlert("Alert", "31 images needed to generate cards, but you added " + count, "OK");
            }
        }
        async void OnCreateCards57Clicked(object sender, EventArgs e)
        {
            if (count == 57)
            {
                MergeImagesAsync57();
            }
            else
            {
                await DisplayAlert("Alert", "57 images needed to generate cards, but you added " + count, "OK");
            }
        }
        async void OnSelectFolderClicked(object sender, EventArgs e)
        {
            CancellationTokenSource source = new();
            CancellationToken token = source.Token;
            var result = await FolderPicker.Default.PickAsync(token);

            if (result.IsSuccessful)
            {
                selectedSaveLocation = result.Folder.Path;
            }
        }
        Bitmap ResizeImage(Bitmap originalImage, int maxWidth, int maxHeight)
        {
            int newWidth, newHeight;
            float aspectRatio = (float)originalImage.Width / originalImage.Height;

            // Determine whether width or height is larger
            //if (originalImage.Width > originalImage.Height || originalImage.Width == originalImage.Height)
            //{
            //    // Width is larger
            //    newWidth = maxWidth;
            //    float floatNewHeight = maxWidth / aspectRatio;

            //    newHeight = Convert.ToInt32(floatNewHeight);

            //}
            //else
            //{
            //    newHeight = maxHeight;

            //    float floatNewWidth = newHeight / aspectRatio;
            //    newWidth = Convert.ToInt32(floatNewWidth);

            //}

            // odustao od konverzije, a da ostane aspectfit:

            newHeight = 250;
            newWidth = 250;

            Debug.WriteLine("širine i visine: " + newHeight + " x " + newWidth);
            // Create a new bitmap with the resized dimensions
            Bitmap resizedImage = new Bitmap(newWidth, newHeight);

            // Perform the resizing using the Graphics class
            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(originalImage, 0, 0, newWidth, newHeight);
            }

            return resizedImage;
        }

        Bitmap RotateImage(Bitmap originalImage, float rotationAngle)
        {
            Bitmap rotatedImage = new Bitmap(originalImage.Width, originalImage.Height);

            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                g.TranslateTransform(rotatedImage.Width / 2, rotatedImage.Height / 2);
                g.RotateTransform(rotationAngle);
                g.TranslateTransform(-rotatedImage.Width / 2, -rotatedImage.Height / 2);
                g.DrawImage(originalImage, new Point(0, 0));
            }

            return rotatedImage;
        }
        async Task MergeImagesAsync13()
        {
            if (string.IsNullOrEmpty(selectedSaveLocation))
            {
                DisplayAlert("Error", "Please select save location first", "OK");

                return;
            }

            // Load the images using System.Drawing.Common
            var images = new Bitmap[MergeCollection.Count]; // Assuming ImageCollection is a List or another collection type

            int index = 0;
            foreach (var imageSource in MergeCollection)
            {
                string filePath = (imageSource as FileImageSource)?.File;
                Debug.WriteLine("File path " + filePath);

                if (!string.IsNullOrEmpty(filePath))
                {
                    using (var stream = File.OpenRead(filePath))
                    using (var originalBitmap = new Bitmap(stream))
                    {
                        images[index] = ResizeImage(originalBitmap, 250, 250);
                    }
                    index++;
                }
                else
                {
                    DisplayAlert("Error", "File path not available for one or more images.", "OK");
                    return;
                }
            }

            // Define the merging pattern based on the provided index sets
            int[][] mergingPattern = new int[][]
            {
              new int[] {0, 1, 2, 9 },
              new int[] {9, 3, 4, 5 },
              new int[] {8, 9, 6, 7 },
              new int[] {0, 10, 3, 6 },
              new int[] {1, 10, 4, 7},
              new int[] {8, 2, 10, 5},
              new int[] {0, 8, 11, 4},
              new int[] {1, 11, 5, 6},
              new int[] {11, 2, 3, 7},
              new int[] {0, 12, 5, 7},
              new int[] {8, 1, 3, 12},
              new int[] {12, 2, 4, 6},
              new int[] {9, 10, 11, 12}

            };

            // Calculate the size of the merged image
            int squareSize = 360;
            int imageSquareSize = 250;
            int mergedWidth = squareSize * 2;
            int mergedHeight = squareSize * 2;

            Random random = new Random();

            for (int cardIndex = 0; cardIndex < 13; cardIndex++)
            {
                using (var mergedBitmap = new Bitmap(mergedWidth, mergedHeight))
                {
                    using (var g = Graphics.FromImage(mergedBitmap))
                    {
                        // Draw the images onto the merged bitmap based on the current merging pattern
                        for (int i = 0; i < 4; i++)
                        {
                            int imageIndex = mergingPattern[cardIndex][i];

                            // Calculate the starting position for each square
                            int squareX = i % 2 * squareSize;
                            int squareY = (i / 2) * squareSize;

                            // Calculate the center position for the image within the square
                            int centerX = squareX + squareSize / 2;
                            int centerY = squareY + squareSize / 2;

                            int rotationAngle = 0;

                            if (rotationEnabled == true)
                            {
                                rotationAngle = random.Next(0, 360);
                            }

                            // Calculate the bounding box for the rotated image
                            RectangleF destinationRect = new RectangleF(centerX - imageSquareSize / 2, centerY - imageSquareSize / 2, imageSquareSize, imageSquareSize);

                            using (var matrix = new System.Drawing.Drawing2D.Matrix())
                            {
                                matrix.RotateAt(rotationAngle, new System.Drawing.PointF(centerX, centerY));

                                // Apply the rotation without distortion
                                g.Transform = matrix;
                                g.DrawImage(images[imageIndex], destinationRect);
                                g.ResetTransform();
                            }
                        }
                    }



                    try
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            // Save the System.Drawing.Bitmap to the MemoryStream
                            mergedBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                            // Reset the position of the stream to the beginning
                            stream.Seek(0, SeekOrigin.Begin);

                            // Use the FileSaver to save the Stream to a file in the selectedSaveLocation
                            var outputPath = Path.Combine(selectedSaveLocation, $"MergedImage_{cardIndex + 1}.png");

                            using (var fileStream = File.Create(outputPath))
                            {
                                await stream.CopyToAsync(fileStream);
                            }

                            Debug.WriteLine($"Merged image {cardIndex + 1} saved to {outputPath}");
                        }

                    }
                    catch (Exception ex)
                    {
                        DisplayAlert("Error", $"Failed to save merged image {cardIndex + 1}: {ex.Message}", "OK");
                    }
                }
            }

            DisplayAlert("Success", $"All merged images saved to selected folder", "OK");
            OpenOutputFolder();
        }

        async Task MergeImagesAsync31()
        {
            if (string.IsNullOrEmpty(selectedSaveLocation))
            {
                DisplayAlert("Error", "Please select save location first", "OK");

                return;
            }

            // Load the images using System.Drawing.Common
            var images = new Bitmap[MergeCollection.Count]; // Assuming ImageCollection is a List or another collection type

            int index = 0;
            foreach (var imageSource in MergeCollection)
            {
                string filePath = (imageSource as FileImageSource)?.File;
                Debug.WriteLine("File path " + filePath);

                if (!string.IsNullOrEmpty(filePath))
                {
                    using (var stream = File.OpenRead(filePath))
                    using (var originalBitmap = new Bitmap(stream))
                    {
                        images[index] = ResizeImage(originalBitmap, 250, 250);
                    }
                    index++;
                }
                else
                {
                    DisplayAlert("Error", "File path not available for one or more images.", "OK");
                    return;
                }
            }

            // Define the merging pattern based on the provided index sets
            int[][] mergingPattern = new int[][]
            {
        new int[] {0, 1, 2, 3, 4, 25},
new int[] {5, 6, 7, 8, 9, 25},
new int[] {10, 11, 12, 13, 14, 25},
new int[] {15, 16, 17, 18, 19, 25},
new int[] {20, 21, 22, 23, 24, 25},
new int[] {0, 5, 10, 15, 20, 26},
new int[] {1, 6, 11, 16, 21, 26},
new int[] {2, 7, 12, 17, 22, 26},
new int[] {3, 8, 13, 18, 23, 26},
new int[] {4, 9, 14, 19, 24, 26},
new int[] {0, 6, 12, 18, 24, 27},
new int[] {1, 7, 13, 19, 20, 27},
new int[] {2, 8, 14, 15, 21, 27},
new int[] {3, 9, 10, 16, 22, 27},
new int[] {4, 5, 11, 17, 23, 27},
new int[] {0, 7, 14, 16, 23, 28},
new int[] {1, 8, 10, 17, 24, 28},
new int[] {2, 9, 11, 18, 20, 28},
new int[] {3, 5, 12, 19, 21, 28},
new int[] {4, 6, 13, 15, 22, 28},
new int[] {0, 8, 11, 19, 22, 29},
new int[] {1, 9, 12, 15, 23, 29},
new int[] {2, 5, 13, 16, 24, 29},
new int[] {3, 6, 14, 17, 20, 29},
new int[] {4, 7, 10, 18, 21, 29},
new int[] {0, 9, 13, 17, 21, 30},
new int[] {1, 5, 14, 18, 22, 30},
new int[] {2, 6, 10, 19, 23, 30},
new int[] {3, 7, 11, 15, 24, 30},
new int[] {4, 8, 12, 16, 20, 30},
new int[] {25, 26, 27, 28, 29, 30}
            };

            // Calculate the size of the merged image
            int squareSize = 360;
            int imageSquareSize = 250;
            int mergedWidth = squareSize * 3;
            int mergedHeight = squareSize * 2;

            Random random = new Random();

            for (int cardIndex = 0; cardIndex < 31; cardIndex++)
            {
                using (var mergedBitmap = new Bitmap(mergedWidth, mergedHeight))
                {
                    using (var g = Graphics.FromImage(mergedBitmap))
                    {
                        // Draw the images onto the merged bitmap based on the current merging pattern
                        for (int i = 0; i < 6; i++)
                        {
                            int imageIndex = mergingPattern[cardIndex][i];

                            // Calculate the starting position for each square
                            int squareX = i % 3 * squareSize;
                            int squareY = (i / 3) * squareSize;

                            // Calculate the center position for the image within the square
                            int centerX = squareX + squareSize / 2;
                            int centerY = squareY + squareSize / 2;

                            int rotationAngle = 0;

                            if (rotationEnabled == true)
                            {
                                rotationAngle = random.Next(0, 360);
                            }

                            // Calculate the bounding box for the rotated image
                            RectangleF destinationRect = new RectangleF(centerX - imageSquareSize / 2, centerY - imageSquareSize / 2, imageSquareSize, imageSquareSize);

                            using (var matrix = new System.Drawing.Drawing2D.Matrix())
                            {
                                matrix.RotateAt(rotationAngle, new System.Drawing.PointF(centerX, centerY));

                                // Apply the rotation without distortion
                                g.Transform = matrix;
                                g.DrawImage(images[imageIndex], destinationRect);
                                g.ResetTransform();
                            }
                        }
                    }



                    try
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            // Save the System.Drawing.Bitmap to the MemoryStream
                            mergedBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                            // Reset the position of the stream to the beginning
                            stream.Seek(0, SeekOrigin.Begin);

                            // Use the FileSaver to save the Stream to a file in the selectedSaveLocation
                            var outputPath = Path.Combine(selectedSaveLocation, $"MergedImage_{cardIndex + 1}.png");

                            using (var fileStream = File.Create(outputPath))
                            {
                                await stream.CopyToAsync(fileStream);
                            }

                            Debug.WriteLine($"Merged image {cardIndex + 1} saved to {outputPath}");
                        }

                    }
                    catch (Exception ex)
                    {
                        DisplayAlert("Error", $"Failed to save merged image {cardIndex + 1}: {ex.Message}", "OK");
                    }
                }
            }

            DisplayAlert("Success", $"All merged images saved to selected folder", "OK");
            OpenOutputFolder();

        }


        async Task MergeImagesAsync57()
        {
            if (string.IsNullOrEmpty(selectedSaveLocation))
            {
                DisplayAlert("Error", "Please select save location first", "OK");

                return;
            }

            // Load the images using System.Drawing.Common
            var images = new Bitmap[MergeCollection.Count]; // Assuming ImageCollection is a List or another collection type

            int index = 0;
            foreach (var imageSource in MergeCollection)
            {
                string filePath = (imageSource as FileImageSource)?.File;
                Debug.WriteLine("File path " + filePath);

                if (!string.IsNullOrEmpty(filePath))
                {
                    using (var stream = File.OpenRead(filePath))
                    using (var originalBitmap = new Bitmap(stream))
                    {
                        images[index] = ResizeImage(originalBitmap, 250, 250);
                    }
                    index++;
                }
                else
                {
                    DisplayAlert("Error", "File path not available for one or more images.", "OK");
                    return;
                }
            }

            // Define the merging pattern based on the provided index sets
            int[][] mergingPattern = new int[][]
            {
        new int[] {0, 1, 2, 3, 4, 5, 6, 49},
new int[] {7, 8, 9, 10, 11, 12, 13, 49},
new int[] {49, 14, 15, 16, 17, 18, 19, 20},
new int[] {49, 21, 22, 23, 24, 25, 26, 27},
new int[] {32, 33, 34, 49, 28, 29, 30, 31},
new int[] {35, 36, 37, 38, 39, 40, 41, 49},
new int[] {42, 43, 44, 45, 46, 47, 48, 49},
new int[] {0, 35, 7, 42, 14, 50, 21, 28},
new int[] {1, 36, 8, 43, 15, 50, 22, 29},
new int[] {2, 37, 9, 44, 16, 50, 23, 30},
new int[] {3, 38, 10, 45, 17, 50, 24, 31},
new int[] {32, 4, 39, 11, 50, 46, 18, 25},
new int[] {33, 5, 40, 12, 47, 50, 19, 26},
new int[] {34, 6, 41, 13, 48, 50, 20, 27},
new int[] {0, 32, 48, 8, 16, 40, 51, 24},
new int[] {1, 33, 41, 42, 17, 51, 9, 25},
new int[] {34, 35, 10, 43, 2, 18, 51, 26},
new int[] {51, 3, 36, 11, 44, 19, 27, 28},
new int[] {4, 37, 12, 45, 51, 20, 21, 29},
new int[] {5, 38, 13, 14, 51, 46, 22, 30},
new int[] {6, 39, 7, 15, 51, 23, 47, 31},
new int[] {0, 38, 9, 47, 18, 52, 27, 29},
new int[] {1, 39, 10, 48, 19, 52, 21, 30},
new int[] {2, 40, 42, 11, 20, 22, 52, 31},
new int[] {32, 3, 41, 43, 12, 14, 52, 23},
new int[] {33, 35, 4, 44, 13, 15, 52, 24},
new int[] {34, 36, 5, 7, 45, 16, 52, 25},
new int[] {37, 6, 8, 46, 17, 52, 26, 28},
new int[] {0, 33, 36, 10, 46, 20, 53, 23},
new int[] {1, 34, 37, 11, 14, 47, 53, 24},
new int[] {2, 38, 12, 15, 48, 53, 25, 28},
new int[] {3, 39, 42, 13, 16, 53, 26, 29},
new int[] {4, 7, 40, 43, 17, 53, 27, 30},
new int[] {5, 8, 41, 44, 18, 21, 53, 31},
new int[] {32, 35, 6, 9, 45, 19, 53, 22},
new int[] {0, 41, 11, 45, 15, 54, 26, 30},
new int[] {1, 35, 12, 46, 16, 54, 27, 31},
new int[] {32, 2, 36, 13, 47, 17, 21, 54},
new int[] {33, 3, 37, 7, 48, 18, 22, 54},
new int[] {34, 4, 38, 8, 42, 19, 54, 23},
new int[] {5, 39, 9, 43, 20, 54, 24, 28},
new int[] {6, 40, 10, 44, 14, 54, 25, 29},
new int[] {0, 34, 39, 44, 12, 17, 22, 55},
new int[] {1, 40, 55, 13, 45, 18, 23, 28},
new int[] {2, 7, 41, 46, 19, 55, 24, 29},
new int[] {3, 8, 47, 35, 20, 55, 25, 30},
new int[] {4, 9, 14, 48, 55, 36, 26, 31},
new int[] {32, 37, 10, 15, 55, 27, 42, 5},
new int[] {33, 43, 38, 6, 11, 16, 21, 55},
new int[] {0, 37, 43, 13, 19, 56, 25, 31},
new int[] {32, 1, 38, 7, 44, 20, 56, 26},
new int[] {33, 2, 39, 8, 45, 14, 56, 27},
new int[] {34, 3, 40, 9, 46, 15, 21, 56},
new int[] {4, 41, 10, 47, 16, 22, 56, 28},
new int[] {35, 5, 11, 48, 17, 23, 56, 29},
new int[] {36, 6, 42, 12, 56, 18, 24, 30},
new int[] {49, 50, 51, 52, 53, 54, 55, 56}
            };

            // Calculate the size of the merged image
            int squareSize = 360;
            int imageSquareSize = 250;
            int mergedWidth = squareSize * 4;
            int mergedHeight = squareSize * 2;

            Random random = new Random();

            for (int cardIndex = 0; cardIndex < 57; cardIndex++)
            {
                using (var mergedBitmap = new Bitmap(mergedWidth, mergedHeight))
                {
                    using (var g = Graphics.FromImage(mergedBitmap))
                    {
                        // Draw the images onto the merged bitmap based on the current merging pattern
                        for (int i = 0; i < 8; i++)
                        {
                            int imageIndex = mergingPattern[cardIndex][i];

                            // Calculate the starting position for each square
                            int squareX = i % 4 * squareSize;
                            int squareY = (i / 4) * squareSize;

                            // Calculate the center position for the image within the square
                            int centerX = squareX + squareSize / 2;
                            int centerY = squareY + squareSize / 2;

                            int rotationAngle = 0;

                            if (rotationEnabled == true)
                            {
                                rotationAngle = random.Next(0, 360);
                            }

                            // Calculate the bounding box for the rotated image
                            RectangleF destinationRect = new RectangleF(centerX - imageSquareSize / 2, centerY - imageSquareSize / 2, imageSquareSize, imageSquareSize);

                            using (var matrix = new System.Drawing.Drawing2D.Matrix())
                            {
                                matrix.RotateAt(rotationAngle, new System.Drawing.PointF(centerX, centerY));

                                // Apply the rotation without distortion
                                g.Transform = matrix;
                                g.DrawImage(images[imageIndex], destinationRect);
                                g.ResetTransform();
                            }
                        }
                    }



                    try
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            // Save the System.Drawing.Bitmap to the MemoryStream
                            mergedBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                            // Reset the position of the stream to the beginning
                            stream.Seek(0, SeekOrigin.Begin);

                            // Use the FileSaver to save the Stream to a file in the selectedSaveLocation
                            var outputPath = Path.Combine(selectedSaveLocation, $"MergedImage_{cardIndex + 1}.png");

                            using (var fileStream = File.Create(outputPath))
                            {
                                await stream.CopyToAsync(fileStream);
                            }

                            Debug.WriteLine($"Merged image {cardIndex + 1} saved to {outputPath}");
                        }

                    }
                    catch (Exception ex)
                    {
                        DisplayAlert("Error", $"Failed to save merged image {cardIndex + 1}: {ex.Message}", "OK");
                    }
                }
            }

            DisplayAlert("Success", $"All merged images saved to selected folder", "OK");
            OpenOutputFolder();

        }

    }
}

