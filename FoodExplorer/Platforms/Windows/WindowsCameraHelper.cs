using System.Runtime.InteropServices;
using Windows.Devices.Enumeration;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using WinRT.Interop;

namespace FoodExplorer.Platforms.Windows;

/// <summary>
/// Windows camera capture with webcam, file picker, and optional system Camera app fallbacks.
/// </summary>
internal static class WindowsCameraHelper
{
    private const string WindowsCameraAppPackageName = "Microsoft.WindowsCamera_8wekyb3d8bbwe";
    private const string WindowsCameraAppUri = "microsoft.windows.camera.picker:";
    private const string CacheFolderName = ".FoodExplorer.Camera";
    private const string CacheFileName = "capture";

    public static Task<(bool Success, byte[]? ImageBytes, string? ErrorMessage)> PickPhotoAsync(
        CancellationToken cancellationToken = default)
    {
        return MainThread.InvokeOnMainThreadAsync(() =>
            TryPickPhotoAsync(cancellationToken));
    }

    public static Task<(bool Success, byte[]? ImageBytes, string? ErrorMessage)> CapturePhotoAsync(
        CancellationToken cancellationToken = default)
    {
        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var errors = new List<string>();

            var webcamResult = await TryWebcamCaptureAsync(cancellationToken);
            if (webcamResult.Success)
                return webcamResult;
            if (!string.IsNullOrWhiteSpace(webcamResult.ErrorMessage))
                errors.Add(webcamResult.ErrorMessage);

            var pickResult = await TryPickPhotoAsync(cancellationToken);
            if (pickResult.Success)
                return pickResult;
            if (!string.IsNullOrWhiteSpace(pickResult.ErrorMessage)
                && !pickResult.ErrorMessage.Contains("cancelled", StringComparison.OrdinalIgnoreCase))
                errors.Add(pickResult.ErrorMessage);

            var cameraAppResult = await TryWindowsCameraAppAsync(cancellationToken);
            if (cameraAppResult.Success)
                return cameraAppResult;
            if (!string.IsNullOrWhiteSpace(cameraAppResult.ErrorMessage)
                && !cameraAppResult.ErrorMessage.Contains("cancelled", StringComparison.OrdinalIgnoreCase))
                errors.Add(cameraAppResult.ErrorMessage);

            var message = errors.Count > 0
                ? string.Join(" ", errors.Distinct())
                : "Unable to capture photo. Check that a camera is connected and camera access is enabled in Windows Settings > Privacy > Camera.";

            return (false, null, message);
        });
    }

    private static async Task<(bool Success, byte[]? ImageBytes, string? ErrorMessage)> TryWebcamCaptureAsync(
        CancellationToken cancellationToken)
    {
        MediaCapture? capture = null;
        try
        {
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            if (devices.Count == 0)
                return (false, null, "No camera was found on this device.");

            var settings = new MediaCaptureInitializationSettings
            {
                VideoDeviceId = devices[0].Id,
                StreamingCaptureMode = StreamingCaptureMode.Video,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                SharingMode = MediaCaptureSharingMode.SharedReadOnly
            };

            capture = new MediaCapture();
            await capture.InitializeAsync(settings);

            var folder = await StorageFolder.GetFolderFromPathAsync(FileSystem.CacheDirectory);
            var cameraFolder = await folder.CreateFolderAsync(
                CacheFolderName,
                CreationCollisionOption.OpenIfExists);
            var file = await cameraFolder.CreateFileAsync(
                $"{CacheFileName}_{Guid.NewGuid():N}.jpg",
                CreationCollisionOption.ReplaceExisting);

            await capture.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), file);
            return await ReadImageBytesAsync(file, cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            return (false, null,
                "Camera permission denied. Open Windows Settings > Privacy and security > Camera, then allow FoodExplorer.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WindowsCameraHelper] Webcam: {ex}");
            return (false, null, DescribeError("Webcam capture failed", ex));
        }
        finally
        {
            capture?.Dispose();
        }
    }

    private static async Task<(bool Success, byte[]? ImageBytes, string? ErrorMessage)> TryPickPhotoAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var hwnd = GetWindowHandle();
            if (hwnd == IntPtr.Zero)
                return (false, null, "Unable to open the photo picker window.");

            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".webp");
            InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file is null)
                return (false, null, "Photo selection was cancelled.");

            return await ReadImageBytesAsync(file, cancellationToken);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WindowsCameraHelper] Pick photo: {ex}");
            return (false, null, DescribeError("Photo picker failed", ex));
        }
    }

    private static async Task<(bool Success, byte[]? ImageBytes, string? ErrorMessage)> TryWindowsCameraAppAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var hwnd = GetWindowHandle();
            if (hwnd == IntPtr.Zero)
                return (false, null, "Unable to open the camera window.");

            var options = new LauncherOptions();
            InitializeWithWindow.Initialize(options, hwnd);
            options.TreatAsUntrusted = false;
            options.DisplayApplicationPicker = false;
            options.TargetApplicationPackageFamilyName = WindowsCameraAppPackageName;

            var tempLocation = await StorageFolder.GetFolderFromPathAsync(FileSystem.CacheDirectory);
            var tempFolder = await tempLocation.CreateFolderAsync(
                CacheFolderName,
                CreationCollisionOption.OpenIfExists);
            var tempFile = await tempFolder.CreateFileAsync(
                $"{CacheFileName}.jpg",
                CreationCollisionOption.GenerateUniqueName);
            var token = global::Windows.ApplicationModel.DataTransfer.SharedStorageAccessManager.AddFile(tempFile);

            try
            {
                var set = new ValueSet
                {
                    ["MediaType"] = "photo",
                    ["PhotoFileToken"] = token
                };

                var uri = new Uri(WindowsCameraAppUri);
                var result = await global::Windows.System.Launcher.LaunchUriForResultsAsync(uri, options, set);

                if (result.Status != LaunchUriStatus.Success || result.Result is null)
                    return (false, null, "Photo capture was cancelled.");

                return await ReadImageBytesAsync(tempFile, cancellationToken);
            }
            finally
            {
                global::Windows.ApplicationModel.DataTransfer.SharedStorageAccessManager.RemoveFile(token);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WindowsCameraHelper] Camera app: {ex}");
            return (false, null, DescribeError("System camera app failed", ex));
        }
    }

    private static async Task<(bool Success, byte[]? ImageBytes, string? ErrorMessage)> ReadImageBytesAsync(
        StorageFile file,
        CancellationToken cancellationToken)
    {
        using var stream = await file.OpenStreamForReadAsync();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        var bytes = memoryStream.ToArray();

        if (bytes.Length == 0)
            return (false, null, "The captured photo is empty. Please try again.");

        return (true, bytes, null);
    }

    private static IntPtr GetWindowHandle()
    {
        var window = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
        if (window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow)
            return WindowNative.GetWindowHandle(nativeWindow);

        return IntPtr.Zero;
    }

    private static string DescribeError(string prefix, Exception ex)
    {
        if (ex is COMException com && com.HResult != 0)
            return $"{prefix} (error 0x{com.HResult & 0xFFFFFFFF:X8}).";

        if (!string.IsNullOrWhiteSpace(ex.Message))
            return $"{prefix}: {ex.Message}";

        return $"{prefix} ({ex.GetType().Name}).";
    }
}
