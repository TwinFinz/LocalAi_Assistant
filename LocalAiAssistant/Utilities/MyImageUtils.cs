using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
#if ANDROID
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Android.Content;
#endif
namespace LocalAiAssistant.Utilities
{
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0051 // Remove unused private members
    internal class MyImageUtils
    {

#if ANDROID
        
        internal static async Task<byte[]?> SelectImageFile()
        {
            FileResult? file = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Please select an image file.", FileTypes = FilePickerFileType.Images });
            if (file != null)
            {
                Toast.MakeText(Android.App.Application.Context, $"Selected: {file.FullPath}", ToastLength.Long)!.Show();
                return MyFileUtils.ReadBinaryFile(file.FullPath);
            }
            else
            {
                return null;
            }
        }


        public static async Task<byte[]> ProcessImage(string apiKey, string prompt, byte[] imageBytes)
        {
            int width = 0;
            int height = 0;
            using (MemoryStream memoryStream = new(imageBytes))
            {
                using (Android.Graphics.Bitmap bitmap = BitmapFactory.DecodeStream(memoryStream)!)
                {
                    width = bitmap.Width;
                    height = bitmap.Height;
                }
            }

            int sectionSize = 1024;
            int sectionWidth = (width / sectionSize) + (width % sectionSize > 0 ? 1 : 0);
            int sectionHeight = (height / sectionSize) + (height % sectionSize > 0 ? 1 : 0);

            byte[][] processedSections = new byte[sectionWidth * sectionHeight][];
            for (int x = 0; x < sectionWidth; x++)
            {
                for (int y = 0; y < sectionHeight; y++)
                {
                    int startX = x * sectionSize;
                    int startY = y * sectionSize;
                    int workSectionWidth = Math.Min(sectionSize, width - startX);
                    int workSectionHeight = Math.Min(sectionSize, height - startY);
                    byte[] sectionBytes = GetSectionBytes(imageBytes, startX, startY, workSectionWidth, workSectionHeight);
                    byte[]? processedSection = await ProcessSection(apiKey, prompt, sectionBytes, sectionSize);
                    processedSections[(y * sectionWidth) + x] = processedSection!;
                    Toast.MakeText(Android.App.Application.Context, $"Current Section: X:{startX}-{startX + workSectionWidth}, Y:{startY}-{startY + workSectionHeight}...", ToastLength.Long)!.Show();
                }
            }

            byte[] finalBytes = new byte[width * height * 4];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int sectionX = x / sectionSize;
                    int sectionY = y / sectionSize;
                    int sectionIndex = (sectionY * sectionWidth) + sectionX;
                    int offsetX = x % sectionSize;
                    //int offsetY = y % sectionSize;

                    byte[] processedSection = processedSections[sectionIndex];
                    int processedSectionX = Math.Min(offsetX, (processedSection.Length / 4) - 1);
                    int finalIndex = ((y * width) + x) * 4;
                    finalBytes[finalIndex + 0] = processedSection[(processedSectionX * 4) + 0];
                    finalBytes[finalIndex + 1] = processedSection[(processedSectionX * 4) + 1];
                    finalBytes[finalIndex + 2] = processedSection[(processedSectionX * 4) + 2];
                    finalBytes[finalIndex + 3] = processedSection[(processedSectionX * 4) + 3];
                }
            }
            return finalBytes;
        }
        public static byte[] CreateBlankPNG(int width, int height)
        {
            using (MemoryStream ms = new())
            {
                using (Android.Graphics.Bitmap bmp = Android.Graphics.Bitmap.CreateBitmap(width, height, Android.Graphics.Bitmap.Config.Argb8888!)!)
                {
                    bmp.Compress(Android.Graphics.Bitmap.CompressFormat.Png!, 100, ms);
                    return ms.ToArray();
                }
            }
        }        
        private static bool IsValidPng(byte[] imageBytes)
        {
            try
            {
                using (MemoryStream ms = new(imageBytes))
                {
                    Android.Graphics.Bitmap image = BitmapFactory.DecodeStream(ms)!;
                    return image != null;
                }
            }
            catch
            {
                return false;
            }
        }
        private static byte[] ConvertToPng(byte[] imageBytes)
        {
            using (MemoryStream ms = new(imageBytes))
            {
                Android.Graphics.Bitmap? image = BitmapFactory.DecodeStream(ms);
                using (MemoryStream pngMs = new())
                {
                    image!.Compress(Android.Graphics.Bitmap.CompressFormat.Png!, 100, pngMs);
                    return pngMs.ToArray();
                }
            }
        }
        public static async Task<byte[]> CropAndResizeImage(byte[] imageBytes)
        {
            using MemoryStream memoryStream = new(imageBytes);
            using var originalBitmap = BitmapFactory.DecodeStream(memoryStream);
            int width = originalBitmap!.Width;
            int height = originalBitmap.Height;
            int size = Math.Min(width, height);
            int x = (width - size) / 2;
            int y = (height - size) / 2;
            using var squareBitmap = Android.Graphics.Bitmap.CreateBitmap(originalBitmap, x, y, size, size);
            using var resizedBitmap = Android.Graphics.Bitmap.CreateScaledBitmap(squareBitmap!, 1024, 1024, false);
            using MemoryStream resizedStream = new();
            resizedBitmap!.Compress(Android.Graphics.Bitmap.CompressFormat.Png!, 100, resizedStream);

            await Task.Delay(0);
            return resizedStream.ToArray();
        }
        public static  async Task<byte[]> CropAndResizeImage2(byte[] imageBytes)
        {
            using (MemoryStream memoryStream = new(imageBytes))
            {
                using (Android.Graphics.Bitmap originalBitmap = BitmapFactory.DecodeStream(memoryStream)!)
                {
                    // Display a dialog to allow the user to select a square area of the image
                    int[] squareSelection = await ShowSquareSelectionDialog();
                    int x = squareSelection[0];
                    int y = squareSelection[1];
                    int size = squareSelection[2];

                    // Crop the image to the selected square
                    using (Android.Graphics.Bitmap squareBitmap = Android.Graphics.Bitmap.CreateBitmap(originalBitmap, x, y, size, size)!)
                    {
                        // Resize the cropped image
                        using (Android.Graphics.Bitmap resizedBitmap = Android.Graphics.Bitmap.CreateScaledBitmap(squareBitmap, 1024, 1024, false)!)
                        {
                            using (MemoryStream resizedStream = new())
                            {
                                resizedBitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png!, 100, resizedStream);
                                return resizedStream.ToArray();
                            }
                        }
                    }
                }
            }
        }
#endif
        public static async Task<byte[]> GetAsByteArray(string imageUrl)
        {
            using HttpClient client = new();
            byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);
            return imageBytes;
        }
        internal static async Task<int[]> ShowSquareSelectionDialog()
        {
            var completionSource = new TaskCompletionSource<int[]>();
            /* var selectedArea = new int[3]; Android.Graphics.Bitmap originalBitmap
             var dialog = new Dialog(Application.Context, Resource.Style.SquareSelectionDialogStyle);
             var view = LayoutInflater.From(Application.Context).Inflate(Resource.Layout.SquareSelectionDialog, null);
             var imageView = view.FindViewById<ImageView>(Resource.Id.SquareSelectionImageView);
             imageView.SetImageBitmap(originalBitmap);
             var squareSelectionView = view.FindViewById<SquareSelectionView>(Resource.Id.SquareSelectionView);
             squareSelectionView.SelectionChanged += (sender, e) =>
             {
                 selectedArea[0] = squareSelectionView.Selection.Left;
                 selectedArea[1] = squareSelectionView.Selection.Top;
                 selectedArea[2] = squareSelectionView.Selection.Size;
             };
             var doneButton = view.FindViewById<Button>(Resource.Id.DoneButton);
             doneButton.Click += (sender, e) =>
             {
                 dialog.Dismiss();
                 completionSource.SetResult(selectedArea);
             };
             dialog.SetContentView(view);
             dialog.Show(); */
            return await completionSource.Task;
        }
        private static async Task<byte[]?> ProcessSection(string apiKey, string prompt, byte[] imageBytes, int size, bool useMask = false)
        {
            try
            {
                byte[]? maskBytes = null;
                string? imageUrl = null;
                byte[]? processedSection = null;
                MyAIAPI openAI = new();
#if ANDROID
                // Check if the imageBytes are already a valid PNG encoded byte array
                if (!IsValidPng(imageBytes))
                {
                    // If not, convert it to a valid PNG encoded byte array
                    imageBytes = ConvertToPng(imageBytes);
                }
#endif

                if (useMask)
                {
#if ANDROID
                    maskBytes = CreateBlankPNG(size, size);
#endif
                    if (maskBytes != null)
                    {
                        imageUrl = await MyAIAPI.EditImageAsyncHttp(prompt, imageBytes, maskBytes, apiKey, $"{size}x{size}");
                    }
                }
                else
                {
                    imageUrl = await MyAIAPI.EditImageAsyncHttp(prompt, imageBytes, apiKey, $"{size}x{size}");
                }
                if (imageUrl != null) 
                {
                    processedSection = await GetAsByteArray(imageUrl);
                    return processedSection;
                }
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message.Contains("400") || ex.Message.Contains("BAD REQUEST")
                    ? await ProcessSection(apiKey, prompt, imageBytes, size, true)
                    : throw new Exception(ex.Message);
            }
        }
        private static byte[] GetSectionBytes(byte[] imageBytes, int width, int height, int startX, int startY)
        {
            int sectionWidth = 1024;
            int sectionHeight = 1024;
            int overlap = 50;

            int endX = startX + sectionWidth;
            int endY = startY + sectionHeight;

            if (endX > width)
            {
                endX = width;
                sectionWidth = endX - startX + overlap;
            }

            if (endY > height)
            {
                endY = height;
                sectionHeight = endY - startY + overlap;
            }

            // Create a new byte array for the section of the image
            byte[] sectionBytes = new byte[sectionWidth * sectionHeight * 4];

            // Copy the image data for the section into the new array
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    int sourceIndex = ((y * width) + x) * 4;
                    int destinationIndex = (((y - startY) * sectionWidth) + (x - startX)) * 4;
                    sectionBytes[destinationIndex] = imageBytes[sourceIndex];
                    sectionBytes[destinationIndex + 1] = imageBytes[sourceIndex + 1];
                    sectionBytes[destinationIndex + 2] = imageBytes[sourceIndex + 2];
                    sectionBytes[destinationIndex + 3] = imageBytes[sourceIndex + 3];
                }
            }

            return sectionBytes;
        }
    }
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0079 // Remove unnecessary suppression
}