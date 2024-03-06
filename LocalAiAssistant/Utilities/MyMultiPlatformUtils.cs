using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Graphics;
using Microsoft.VisualBasic.FileIO;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using Microsoft.Maui.Graphics.Converters;
using System;
using System.IO;
using Microsoft.Maui.Graphics;
using System.Drawing;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Net;
using CommunityToolkit.Maui.Views;
using System.Threading;

#if ANDROID
using Android.Content;
using Android.Graphics;
using Android.Content.Res;
using Microsoft.Maui.Graphics.Platform;
#elif WINDOWS
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
#endif

namespace LocalAiAssistant.Utilities
{
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0300 // Simplify collection initialization
#pragma warning disable IDE0301 // Simplify collection initialization
#pragma warning disable IDE0305 // Simplify collection initialization
    internal class MyMultiPlatformUtils
    {
        internal static string savedPreferenceName = "LocalAiAssistant";
        internal static IPreferences mainPreferences = Preferences.Default;
        internal static string LogPath { get; set; } = System.IO.Path.Combine($"{SpecialDirectories.Temp}", $"{savedPreferenceName}.log");
        internal static async Task WriteToLog(string Message)
        {
            try
            {

                if (!MainThread.IsMainThread)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await WriteToLog(Message);
                    });
                }
                else
                {
                    Debug.WriteLine(Message);
                    if (!IsFolderWritable(SpecialDirectories.Temp))
                    {
                        throw new Exception($"Cannot write to Temp files, Please check to make sure you have read/write permission in {SpecialDirectories.Temp}");
                    }
                    if (File.Exists(LogPath))
                    {
                        await File.AppendAllTextAsync(LogPath, (Message + "\n"), System.Text.Encoding.UTF8);
                    }
                    else
                    {
                        await File.WriteAllTextAsync(LogPath, (Message + "\n"), System.Text.Encoding.UTF8);
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                await WriteToLog($"Error: {ex.Message}");
                Environment.Exit(ex.HResult);
            }
        }
        internal static async Task<string> WriteToFileAsync(string path, byte[] data, CancellationToken cancellationToken = default)
        {
            if (File.Exists(path))
            {
                string directoryPath = System.IO.Path.GetDirectoryName(path)!;
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(path);
                string extension = System.IO.Path.GetExtension(path);

                int count = 1;
                string newPath = path;
                while (File.Exists(newPath))
                {
                    newPath = System.IO.Path.Combine(directoryPath, $"{fileNameWithoutExtension}-{count}{extension}");
                    count++;
                }

                path = newPath;
            }
            using (FileStream stream = new(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await stream.WriteAsync(data, cancellationToken);
            }
            return System.IO.Path.GetFileName(path);
        }
        internal static async Task<string?> MessageBoxWithInput(Application application, string title, string promptMessage, string confirm = "OK", string cancel = "Cancel")
        {
            if (application.Dispatcher.IsDispatchRequired)
            {
                // Move the UI-related code to the UI thread
                await Microsoft.Maui.Controls.Application.Current!.Dispatcher.DispatchAsync(async () =>
                {
                    await MyMultiPlatformUtils.MessageBoxWithInput(application, title, promptMessage, confirm, cancel);
                    return true;
                });
            }
            else
            {
                string input = await Microsoft.Maui.Controls.Application.Current!.MainPage!.DisplayPromptAsync(title, promptMessage, confirm, cancel);
                if (string.IsNullOrWhiteSpace(input))
                {
                    throw new Exception("User Canceled Authorization");
                }

                return input;
            }
            return string.Empty;
        }
        internal static async Task<bool> MessageBoxWithOK(Application application, string title, string promptMessage, string cancel = "OK")
        {
            if (application.Dispatcher.IsDispatchRequired)
            {
                // Move the UI-related code to the UI thread
                await application.Dispatcher.DispatchAsync(() =>
                {
                    return true;
                });
            }
            else
            {
                await Microsoft.Maui.Controls.Application.Current!.MainPage!.DisplayAlert(title, promptMessage, cancel);
            }
            return false;
        }
        internal async Task<bool> MessageBoxWithYesNo(Application application, string title, string promptMessage, string confirm = "Yes", string cancel = "No")
        {
            bool result = false;
            if (application.Dispatcher.IsDispatchRequired)
            {
                // Move the UI-related code to the UI thread
                await Microsoft.Maui.Controls.Application.Current!.Dispatcher.DispatchAsync(() =>
                {
                    result = MessageBoxWithYesNo(application, title, promptMessage, confirm, cancel).Result;
                });
            }
            else
            {
                string resultString = await Microsoft.Maui.Controls.Application.Current!.MainPage!.DisplayPromptAsync(title, promptMessage, confirm, cancel);
                if (resultString == confirm)
                {
                    result = true;
                }
            }
            return result;
        }
        internal static bool IsFolderWritable(string folderPath)
        {
            try
            {
                string testFilePath = System.IO.Path.Combine(folderPath, "test.txt");
                using (var fileStream = System.IO.File.Create(testFilePath))
                {
                    fileStream.Close();
                    System.IO.File.Delete(testFilePath);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        internal static async Task<string> PickFolderAsync()
        {
            string startpath = Environment.CurrentDirectory;

            CancellationTokenSource source = new();
            CancellationToken token = source.Token;
            var selectedFolder = await FolderPicker.Default.PickAsync(startpath, token);
            if (selectedFolder != null)
            {
                string folderPath = selectedFolder.Folder!.Path;
                if (IsFolderWritable(folderPath))
                {
                    return folderPath;
                }
            }
            return string.Empty;
        }
        internal static async Task<byte[]?> PickImageAsync()
        {
            var result = await MediaPicker.PickPhotoAsync();

            if (result == null)
            {
                // No file was picked
                return null;
            }
            string filePath = result.FullPath;
            return File.ReadAllBytes(filePath);
        }
        internal static async Task<byte[]> GetAsByteArray(string url)
        {
            using HttpClient client = new();
            byte[] imageBytes = await client.GetByteArrayAsync(url);
            return imageBytes;
        }
        internal static async Task<byte[]> GetFileAsByteArray(string path, CancellationToken cancellationToken = default)
        {
            // Check if the file exists
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("The specified file does not exist.", path);
            }

            // Read the file bytes
            byte[] fileBytes;
            using (FileStream fileStream = new(path, FileMode.Open, FileAccess.Read))
            {
                fileBytes = new byte[fileStream.Length];
                await fileStream.ReadAsync(fileBytes, cancellationToken);
            }

            return fileBytes;
        }
        internal static Stream ConvertByteArrayToStream(byte[] byteArray)
        {
            if (byteArray != null)
            {
                return new MemoryStream(byteArray);
            }

            throw new ArgumentNullException(nameof(byteArray));
        }
        internal static async Task<byte[]> ConvertMediaSourceToByteArray(MediaSource mediaSource)
        {
            if (mediaSource is FileMediaSource fileMediaSource)
            {
                byte[] byteArray = File.ReadAllBytes(fileMediaSource!);
                return byteArray;
            }
            else if (mediaSource is UriMediaSource uriMediaSource)
            {
                using (HttpClient client = new())
                {
                    return await client.GetByteArrayAsync(uriMediaSource.Uri);
                }
            }
            throw new NotSupportedException("MediaSource type not supported.");
        }
        internal static async Task<MediaSource> ConvertByteArrayToMediaSource(byte[] byteArray)
        {
            if (byteArray != null)
            {
                string tempFilePath = System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.CacheDirectory, "temp_media");
                File.WriteAllBytes(tempFilePath, byteArray);
                return MediaSource.FromFile(tempFilePath);
            }
            await Task.Delay(0);
            throw new ArgumentNullException(nameof(byteArray));
        }
        internal static async Task<byte[]> ConvertImageSourceToByteArray(ImageSource imageSource)
        {
            if (imageSource is UriImageSource uriImageSource)
            {
                using (HttpClient client = new())
                {
                    return await client.GetByteArrayAsync(uriImageSource.Uri);
                }
            }
            else if (imageSource is FileImageSource fileImageSource)
            {
                byte[] byteArray = File.ReadAllBytes(fileImageSource.File);
                return byteArray;
            }
            else if (imageSource is StreamImageSource streamImageSource)
            {
                using (MemoryStream ms = new())
                {
                    Stream stream = await streamImageSource.Stream(CancellationToken.None);
                    if (stream != null)
                    {
                        await stream.CopyToAsync(ms);
                        return ms.ToArray();
                    }
                }
            }

            throw new NotSupportedException("ImageSource type not supported.");
        }
        internal static ImageSource ConvertByteArrayToImageSource(byte[] byteArray)
        {
            if (byteArray != null)
            {
                using (MemoryStream stream = new(byteArray))
                {
                    // (Workaround) Copy the content of the MemoryStream to another MemoryStream that supports seeking
                    MemoryStream seekableStream = new();
                    stream.CopyTo(seekableStream);
                    seekableStream.Position = 0;

                    return ImageSource.FromStream(() => seekableStream);
                }
            }
            throw new ArgumentNullException(nameof(byteArray));
        }
#pragma warning disable CA1416 // Validate platform compatibility
        public static void CreateBlankPng(string filePath, int width, int height)
        {
            using var bitmap = new System.Drawing.Bitmap(width, height);
            using var stream = new FileStream(filePath, FileMode.Create);
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png!);
        }
        public static byte[] CreateBlankPngAsync(int width, int height)
        {
            using var bitmap = new System.Drawing.Bitmap(width, height);
            using var stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png!);
            return stream.ToArray();
        }		
        public static async void LaunchUrl(string url)
        {
            try
            {
                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                // Handle any exceptions if the browser launch fails
                await MyMultiPlatformUtils.MessageBoxWithOK(Application.Current!, "Warning!", $"Failed to launch the default web browser: {ex.Message}", "OK");
            }
        }

#pragma warning restore CA1416 // Validate platform compatibility
        /* -------------- Encryption -------------- */
        
        /* -------------- End Encryption -------------- */


        /* -------------- Preferences -------------- */
        #region Preferences
#if ANDROID

        public static bool CheckPreferenceContains(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Attempted to search preferences for null or empty string");
            }
            ISharedPreferences? prefs = Android.App.Application.Context.GetSharedPreferences(savedPreferenceName, FileCreationMode.Private); // This makes it NOT shared.
            return prefs!.Contains(name);
        }
        public static double CheckAgeOfPreference(string preference, ISharedPreferences preferences)
        {
            if (long.TryParse(preferences.GetString(preference, "0"), out long lastModifiedTimestamp))
            {
                if (lastModifiedTimestamp == 0)
                {
                    return double.NaN;
                }
                else
                {
                    TimeSpan timeSpan = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(lastModifiedTimestamp);
                    return timeSpan.TotalDays;
                }
            }
            else
            {
                return double.NaN;
            }
        }
        public static Task WriteToPreferences(string name, object obj, bool encrypt = false, string? encryptionKey = null)
        {
            ISharedPreferences? prefs = Android.App.Application.Context.GetSharedPreferences(savedPreferenceName, FileCreationMode.Private); // This makes it NOT shared.
            ISharedPreferencesEditor? prefEditor = prefs?.Edit();
            if (obj == null)
            {
                throw new Exception($"Attempted to write null to shared preference");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("SharedPreferences name can't be null or empty");
            }
            try
            {
                string json = JsonSerializer.Serialize(obj);
                if (encrypt)
                {
                    string encryptedJson = MyEncryptionUtils.RSAEncrypt(json, encryptionKey ?? MyEncryptionUtils.MyRSAEncryption.DefaultRSAkey);
                    prefEditor?.PutString(name, encryptedJson);
                    prefEditor?.Commit();
                }
                else
                {
                    prefEditor?.PutString(name, json);
                    prefEditor?.Commit();
                }
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }
        public static Task<T?> ReadFromPreferences<T>(string name, bool decrypt = false, string encryptionKey = "encrypt")
        {
            ISharedPreferences? prefs = Android.App.Application.Context.GetSharedPreferences(savedPreferenceName, FileCreationMode.Private); // This makes it NOT shared.
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("SharedPreferences name can't be null or empty");
            }
            try
            {
                string json = prefs?.GetString(name, string.Empty) ?? "";
                if (decrypt)
                {
                    string decryptedJson = MyEncryptionUtils.RSADecrypt(json, encryptionKey);
                    return Task.FromResult(JsonSerializer.Deserialize<T?>(decryptedJson) ?? default);
                }
                else
                {
                    return Task.FromResult(JsonSerializer.Deserialize<T?>(json) ?? default);
                }
            }
            catch (Exception e)
            {
                return (Task<T?>)Task.FromException(e);
            }
        }
        private static byte[] ConvertImageToPngFormat(byte[] imageBytes)
        {
            using (MemoryStream inputStream = new(imageBytes))
            {
                using (Android.Graphics.Bitmap? bitmap = Android.Graphics.BitmapFactory.DecodeStream(inputStream))
                {
                    using (MemoryStream outputStream = new())
                    {
                        bitmap?.Compress(Android.Graphics.Bitmap.CompressFormat.Png!, 0, outputStream);
                        return outputStream.ToArray();
                    }
                }
            }
        }
#else
        public static double CheckAgeOfPreference(string preference, IPreferences preferences)
        {
            if (long.TryParse(preferences.Get(preference, "0"), out long lastModifiedTimestamp))
            {
                if (lastModifiedTimestamp == 0)
                {
                    return double.NaN;
                }
                else
                {
                    TimeSpan timeSpan = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(lastModifiedTimestamp);
                    return timeSpan.TotalDays;
                }
            }
            else
            {
                return double.NaN;
            }
        }
        public static bool CheckPreferenceContains(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Attempted to search preferences for null or empty string");
            }
            return mainPreferences.ContainsKey(name);
        }
        public static Task<T?> ReadFromPreferences<T>(string name, bool decrypt = false, string encryptionKey = "")
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("SharedPreferences name can't be null or empty");
            }
            string json = mainPreferences.Get(name, string.Empty) ?? "";
            if (decrypt)
            {
                if(string.IsNullOrEmpty(encryptionKey))
                {
                    encryptionKey = MyEncryptionUtils.MyRSAEncryption.DefaultRSAkey;
                }
                string decryptedJson = MyEncryptionUtils.MyRSAEncryption.Decrypt(json, encryptionKey).Result;
                return Task.FromResult(JsonSerializer.Deserialize<T>(decryptedJson) ?? default);
            }
            else
            {
                return Task.FromResult(JsonSerializer.Deserialize<T>(json) ?? default);
            }
        }
        public static Task WriteToPreferences(string name, object obj, bool encrypt = false, string encryptionKey = "")
        {

            if (obj == null)
            {
                throw new Exception($"Attempted to write null to shared preference");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("SharedPreferences name can't be null or empty");
            }
            string json = JsonSerializer.Serialize(obj);
            if (encrypt)
            {
                if (string.IsNullOrEmpty(encryptionKey))
                {
                    encryptionKey = MyEncryptionUtils.MyRSAEncryption.DefaultRSAkey;
                }
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                string encryptedJson = MyEncryptionUtils.MyRSAEncryption.Encrypt(json, out string keyUsed, encryptionKey).Result;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                mainPreferences.Set(name, encryptedJson);
            }
            else
            {
                mainPreferences.Set(name, json);
            }
            return Task.CompletedTask;
             
        }
#endif
#endregion
        /* -------------- End Preferences -------------- */

        /* -------------- Sorting -------------- */
        public static void BubbleSort(int[] arr)
        {
            int n = arr.Length;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (arr[j] > arr[j + 1])
                    {
                        // Swap arr[j] and arr[j+1]
                        (arr[j + 1], arr[j]) = (arr[j], arr[j + 1]);
                    }
                }
            }
        }
        public static void SelectionSort(int[] arr)
        {
            int n = arr.Length;
            for (int i = 0; i < n - 1; i++)
            {
                int minIndex = i;
                for (int j = i + 1; j < n; j++)
                {
                    if (arr[j] < arr[minIndex])
                    {
                        minIndex = j;
                    }
                }
                // Swap the found minimum element with the first element
                (arr[i], arr[minIndex]) = (arr[minIndex], arr[i]);
            }
        }
        public static void InsertionSort(int[] arr)
        {
            int n = arr.Length;
            for (int i = 1; i < n; ++i)
            {
                int key = arr[i];
                int j = i - 1;

                // Move elements of arr[0..i-1], that are greater than key,
                // to one position ahead of their current position
                while (j >= 0 && arr[j] > key)
                {
                    arr[j++] = arr[j];
                    j--;
                }
                arr[j + 1] = key;
            }
        }
        public static void MergeSort(int[] arr)
        {
            if (arr.Length <= 1)
                return;

            int mid = arr.Length / 2;
            int[] left = new int[mid];
            int[] right = new int[arr.Length - mid];

            Array.Copy(arr, 0, left, 0, mid);
            Array.Copy(arr, mid, right, 0, arr.Length - mid);

            MergeSort(left);
            MergeSort(right);
            MergeSorted(arr, left, right);
        }
        private static void MergeSorted(int[] arr, int[] left, int[] right)
        {
            int i = 0, j = 0, k = 0;
            while (i < left.Length && j < right.Length)
            {
                if (left[i] < right[j])
                    arr[k++] = left[i++];
                else
                    arr[k++] = right[j++];
            }

            while (i < left.Length)
                arr[k++] = left[i++];

            while (j < right.Length)
                arr[k++] = right[j++];
        }
        public static void QuickSort(int[] arr, int low, int high)
        {
            if (low < high)
            {
                int pi = Partition(arr, low, high);

                QuickSort(arr, low, pi - 1);
                QuickSort(arr, pi + 1, high);
            }
        }
        private static int Partition(int[] arr, int low, int high)
        {
            int pivot = arr[high];
            int i = low - 1;
            for (int j = low; j < high; j++)
            {
                if (arr[j] < pivot)
                {
                    i++;
                    // Swap arr[i] and arr[j]
                    (arr[j], arr[i]) = (arr[i], arr[j]);
                }
            }
            // Swap arr[i+1] and arr[high] (or pivot)
            (arr[high], arr[i + 1]) = (arr[i + 1], arr[high]);
            return i + 1;
        }
        public static void HeapSort(int[] arr)
        {
            int n = arr.Length;

            // Build max heap
            for (int i = n / 2 - 1; i >= 0; i--)
                Heapify(arr, n, i);

            // Extract elements from heap one by one
            for (int i = n - 1; i > 0; i--)
            {
                // Move current root to end
                (arr[i], arr[0]) = (arr[0], arr[i]);

                // Call max heapify on the reduced heap
                Heapify(arr, i, 0);
            }
        }
        private static void Heapify(int[] arr, int n, int i)
        {
            int largest = i;
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            if (left < n && arr[left] > arr[largest])
                largest = left;

            if (right < n && arr[right] > arr[largest])
                largest = right;

            if (largest != i)
            {
                // Swap arr[i] and arr[largest]
                (arr[largest], arr[i]) = (arr[i], arr[largest]);

                // Recursively heapify the affected sub-tree
                Heapify(arr, n, largest);
            }
        }
        public static int LinearSearch(int[] arr, int target)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == target)
                    return i;
            }
            return -1; // Element not found
        }
        public static int BinarySearch(int[] arr, int target)
        {
            int left = 0;
            int right = arr.Length - 1;
            while (left <= right)
            {
                int mid = left + (right - left) / 2;

                // Check if the target is present at mid
                if (arr[mid] == target)
                    return mid;

                // If target greater, ignore left half
                if (arr[mid] < target)
                    left = mid + 1;

                // If target is smaller, ignore right half
                else
                    right = mid - 1;
            }
            return -1; // Element not found
        }
        public static void BubbleSort(string[] arr)
        {
            int n = arr.Length;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (string.Compare(arr[j], arr[j + 1], StringComparison.Ordinal) > 0)
                    {
                        // Swap arr[j] and arr[j+1]
                        (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
                    }
                }
            }
        }
        public static void SelectionSort(string[] arr)
        {
            int n = arr.Length;
            for (int i = 0; i < n - 1; i++)
            {
                int minIndex = i;
                for (int j = i + 1; j < n; j++)
                {
                    if (string.Compare(arr[j], arr[minIndex], StringComparison.Ordinal) < 0)
                    {
                        minIndex = j;
                    }
                }
                // Swap the found minimum element with the first element
                (arr[i], arr[minIndex]) = (arr[minIndex], arr[i]);
            }
        }
        public static void InsertionSort(string[] arr)
        {
            int n = arr.Length;
            for (int i = 1; i < n; ++i)
            {
                string key = arr[i];
                int j = i - 1;

                // Move elements of arr[0..i-1], that are greater than key,
                // to one position ahead of their current position
                while (j >= 0 && string.Compare(arr[j], key, StringComparison.Ordinal) > 0)
                {
                    arr[j + 1] = arr[j];
                    j--;
                }
                arr[j + 1] = key;
            }
        }
        public static void QuickSort(string[] arr, int low, int high)
        {
            if (low < high)
            {
                int pi = Partition(arr, low, high);

                QuickSort(arr, low, pi - 1);
                QuickSort(arr, pi + 1, high);
            }
        }
        private static int Partition(string[] arr, int low, int high)
        {
            string pivot = arr[high];
            int i = low - 1;
            for (int j = low; j < high; j++)
            {
                if (string.Compare(arr[j], pivot, StringComparison.Ordinal) < 0)
                {
                    i++;
                    // Swap arr[i] and arr[j]
                    (arr[j], arr[i]) = (arr[i], arr[j]);
                }
            }
            // Swap arr[i+1] and arr[high] (or pivot)
            (arr[high], arr[i + 1]) = (arr[i + 1], arr[high]);
            return i + 1;
        }
        public static int LinearSearch(string[] arr, string target)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == target)
                    return i;
            }
            return -1; // Element not found
        }
        public static int BinarySearch(string[] arr, string target)
        {
            int left = 0;
            int right = arr.Length - 1;
            while (left <= right)
            {
                int mid = left + (right - left) / 2;

                // Check if the target is present at mid
                if (string.Compare(arr[mid], target, StringComparison.Ordinal) == 0)
                    return mid;

                // If target greater, ignore left half
                if (string.Compare(arr[mid], target, StringComparison.Ordinal) < 0)
                    left = mid + 1;

                // If target is smaller, ignore right half
                else
                    right = mid - 1;
            }
            return -1; // Element not found
        }

        /*
        public class Graph
        {
            private static int V;
            private static List<int>[] adj;

            public static Graph(int v)
            {
                V = v;
                adj = new List<int>[v];
                for (int i = 0; i < v; ++i)
                    adj[i] = new List<int>();
            }

            public static void AddEdge(int v, int w)
            {
                adj[v].Add(w);
            }

            private static void DFSUtil(int v, bool[] visited)
            {
                visited[v] = true;
                Console.Write(v + " ");

                foreach (int n in adj[v])
                {
                    if (!visited[n])
                        DFSUtil(n, visited);
                }
            }

            public static void DFS(int v)
            {
                bool[] visited = new bool[V];
                DFSUtil(v, visited);
            }
        }
        public static void BFS(int start)
        {
            bool[] visited = new bool[V];
            Queue<int> queue = new ();
            visited[start] = true;
            queue.Enqueue(start);

            while (queue.Count != 0)
            {
                start = queue.Dequeue();
                Console.Write(start + " ");

                foreach (int n in adj[start])
                {
                    if (!visited[n])
                    {
                        visited[n] = true;
                        queue.Enqueue(n);
                    }
                }
            }
        }
        public static void Dijkstra(int[,] graph, int start)
        {
            int V = graph.GetLength(0);
            int[] dist = new int[V];
            bool[] sptSet = new bool[V];

            for (int i = 0; i < V; ++i)
            {
                dist[i] = int.MaxValue;
                sptSet[i] = false;
            }

            dist[start] = 0;

            for (int count = 0; count < V - 1; ++count)
            {
                int u = MinDistance(dist, sptSet);
                sptSet[u] = true;

                for (int v = 0; v < V; ++v)
                {
                    if (!sptSet[v] && graph[u, v] != 0 && dist[u] != int.MaxValue &&
                        dist[u] + graph[u, v] < dist[v])
                    {
                        dist[v] = dist[u] + graph[u, v];
                    }
                }
            }
        }

        private static int MinDistance(int[] dist, bool[] sptSet)
        {
            int min = int.MaxValue, minIndex = -1;

            for (int v = 0; v < dist.Length; ++v)
            {
                if (sptSet[v] == false && dist[v] <= min)
                {
                    min = dist[v];
                    minIndex = v;
                }
            }

            return minIndex;
        }
        public static void BellmanFord(int[,] graph, int start)
        {
            int V = graph.GetLength(0);
            int[] dist = new int[V];
            for (int i = 0; i < V; ++i)
                dist[i] = int.MaxValue;
            dist[start] = 0;

            for (int count = 0; count < V - 1; ++count)
            {
                for (int u = 0; u < V; ++u)
                {
                    for (int v = 0; v < V; ++v)
                    {
                        if (graph[u, v] != 0 && dist[u] != int.MaxValue &&
                            dist[u] + graph[u, v] < dist[v])
                        {
                            dist[v] = dist[u] + graph[u, v];
                        }
                    }
                }
            }
        }
        public static void FloydWarshall(int[,] graph)
        {
            int V = graph.GetLength(0);
            int[,] dist = new int[V, V];

            for (int i = 0; i < V; ++i)
                for (int j = 0; j < V; ++j)
                    dist[i, j] = graph[i, j];

            for (int k = 0; k < V; ++k)
            {
                for (int i = 0; i < V; ++i)
                {
                    for (int j = 0; j < V; ++j)
                    {
                        if (dist[i, k] + dist[k, j] < dist[i, j])
                            dist[i, j] = dist[i, k] + dist[k, j];
                    }
                }
            }
        }
        public static void PrimMST(int[,] graph)
        {
            int V = graph.GetLength(0);
            int[] parent = new int[V];
            int[] key = new int[V];
            bool[] mstSet = new bool[V];

            for (int i = 0; i < V; ++i)
            {
                key[i] = int.MaxValue;
                mstSet[i] = false;
            }

            key[0] = 0;
            parent[0] = -1;

            for (int count = 0; count < V - 1; ++count)
            {
                int u = MinKey(key, mstSet);
                mstSet[u] = true;

                for (int v = 0; v < V; ++v)
                {
                    if (graph[u, v] != 0 && mstSet[v] == false && graph[u, v] < key[v])
                    {
                        parent[v] = u;
                        key[v] = graph[u, v];
                    }
                }
            }
        }
        private static int MinKey(int[] key, bool[] mstSet)
        {
            int min = int.MaxValue, minIndex = -1;
            for (int v = 0; v < key.Length; ++v)
            {
                if (mstSet[v] == false && key[v] < min)
                {
                    min = key[v];
                    minIndex = v;
                }
            }
            return minIndex;
        }
        public class Graph
        {
            private int V, E;
            private List<Edge> edges;

            public Graph(int v, int e)
            {
                V = v;
                E = e;
                edges = new(e);
            }

            public void AddEdge(int src, int dest, int weight)
            {
                Edge edge = new();
                edge.Source = src;
                edge.Destination = dest;
                edge.Weight = weight;
                edges.Add(edge);
            }

            private class Edge
            {
                public int Source, Destination, Weight;
            }

            private class Subset
            {
                public int Parent, Rank;
            }

            public class HuffmanNode
            {
                public char Character;
                public int Frequency;
                public HuffmanNode Left, Right;
            }

            public static class HuffmanCoding
            {
                public static HuffmanNode BuildHuffmanTree(Dictionary<char, int> freq)
                {
                    var priorityQueue = new PriorityQueue<HuffmanNode>(freq.Count, new HuffmanNodeComparer());

                    foreach (var entry in freq)
                    {
                        priorityQueue.Enqueue(new HuffmanNode { Character = entry.Key, Frequency = entry.Value });
                    }

                    while (priorityQueue.Count() > 1)
                    {
                        HuffmanNode left = priorityQueue.Dequeue();
                        HuffmanNode right = priorityQueue.Dequeue();

                        HuffmanNode mergedNode = new HuffmanNode
                        {
                            Character = '$', // Special character to indicate internal nodes
                            Frequency = left.Frequency + right.Frequency,
                            Left = left,
                            Right = right
                        };

                        priorityQueue.Enqueue(mergedNode);
                    }

                    return priorityQueue.Dequeue();
                }
                // HuffmanNode comparer to be used in the priority queue
                private class HuffmanNodeComparer : IComparer<HuffmanNode>
                {
                    public static int Compare(HuffmanNode x, HuffmanNode y)
                    {
                        return x.Frequency - y.Frequency;
                    }
                }
            }
            public class Item
            {
                public int Weight;
                public int Value;
            }

            public static double FractionalKnapsack(Item[] items, int capacity)
            {
                Array.Sort(items, (x, y) => y.Value * x.Weight - x.Value * y.Weight);

                double totalValue = 0;
                foreach (var item in items)
                {
                    if (item.Weight <= capacity)
                    {
                        totalValue += item.Value;
                        capacity -= item.Weight;
                    }
                    else
                    {
                        totalValue += (double)item.Value * capacity / item.Weight;
                        break;
                    }
                }

                return totalValue;
            }
            public class NQueensProblem
            {
                public static void SolveNQueens(int n)
                {
                    int[] board = new int[n];
                    SolveNQueensUtil(board, 0, n);
                }
                private static void SolveNQueensUtil(int[] board, int col, int n)
                {
                    if (col == n)
                    {
                        PrintSolution(board);
                        return;
                    }

                    for (int i = 0; i < n; i++)
                    {
                        if (IsSafe(board, i, col, n))
                        {
                            board[col] = i;
                            SolveNQueensUtil(board, col + 1, n);
                            board[col] = 0;
                        }
                    }
                }
                private static bool IsSafe(int[] board, int row, int col, int n)
                {
                    for (int prevCol = 0; prevCol < col; prevCol++)
                    {
                        if (board[prevCol] == row || board[prevCol] - prevCol == row - col || board[prevCol] + prevCol == row + col)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                private static void PrintSolution(int[] board)
                {
                    int n = board.Length;
                    for (int i = 0; i < n; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            if (board[i] == j)
                                Console.Write("Q ");
                            else
                                Console.Write(". ");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                }
            }
            public class SudokuSolver
            {
                public static bool SolveSudoku(int[,] board)
                {
                    int row, col;
                    if (!FindEmptyLocation(board, out row, out col))
                    {
                        return true; // No empty location, puzzle solved
                    }

                    for (int num = 1; num <= 9; num++)
                    {
                        if (IsSafe(board, row, col, num))
                        {
                            board[row, col] = num;

                            if (SolveSudoku(board))
                            {
                                return true;
                            }

                            board[row, col] = 0; // If the current placement doesn't lead to a solution, backtrack
                        }
                    }

                    return false; // No solution exists
                }
                private static bool FindEmptyLocation(int[,] board, out int row, out int col)
                {
                    for (row = 0; row < 9; row++)
                    {
                        for (col = 0; col < 9; col++)
                        {
                            if (board[row, col] == 0)
                            {
                                return true; // Empty location found
                            }
                        }
                    }

                    row = -1;
                    col = -1;
                    return false; // No empty location
                }
                private static bool IsSafe(int[,] board, int row, int col, int num)
                {
                    // Check if the number is not in the current row, column, and the 3x3 grid
                    return !UsedInRow(board, row, num) && !UsedInColumn(board, col, num) && !UsedInBox(board, row - row % 3, col - col % 3, num);
                }
                private static bool UsedInRow(int[,] board, int row, int num)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        if (board[row, col] == num)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                private static bool UsedInColumn(int[,] board, int col, int num)
                {
                    for (int row = 0; row < 9; row++)
                    {
                        if (board[row, col] == num)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                private static bool UsedInBox(int[,] board, int boxStartRow, int boxStartCol, int num)
                {
                    for (int row = 0; row < 3; row++)
                    {
                        for (int col = 0; col < 3; col++)
                        {
                            if (board[row + boxStartRow, col + boxStartCol] == num)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }            
            }
            public class RabinKarpAlgorithm
            {
                private const int Prime = 101;
                public static List<int> Search(string text, string pattern)
                {
                    int patternHash = CalculateHash(pattern, pattern.Length);
                    int textHash = CalculateHash(text, pattern.Length);
                    List<int> occurrences = new();

                    for (int i = 0; i <= text.Length - pattern.Length; i++)
                    {
                        if (patternHash == textHash && AreEqual(text, i, pattern))
                        {
                            occurrences.Add(i);
                        }

                        if (i < text.Length - pattern.Length)
                        {
                            textHash = RecalculateHash(text, i, pattern.Length, textHash);
                        }
                    }

                    return occurrences;
                }
                private static int CalculateHash(string str, int length)
                {
                    int hash = 0;
                    for (int i = 0; i < length; i++)
                    {
                        hash += str[i] * (int)Math.Pow(Prime, i);
                    }
                    return hash;
                }
                private static int RecalculateHash(string str, int oldIndex, int length, int oldHash)
                {
                    int newHash = oldHash - str[oldIndex];
                    newHash /= Prime;
                    newHash += str[oldIndex + length] * (int)Math.Pow(Prime, length - 1);
                    return newHash;
                }
                private static bool AreEqual(string str, int startIndex, string pattern)
                {
                    for (int i = 0; i < pattern.Length; i++)
                    {
                        if (str[startIndex + i] != pattern[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            */

        /* -------------- End Sorting -------------- */
    }
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0028 // Simplify collection initialization
#pragma warning restore IDE0300 // Simplify collection initialization
#pragma warning restore IDE0301 // Simplify collection initialization
#pragma warning restore IDE0305 // Simplify collection initialization
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0079 // Remove unnecessary suppression
}
