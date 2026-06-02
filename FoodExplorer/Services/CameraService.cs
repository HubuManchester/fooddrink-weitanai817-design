namespace FoodExplorer.Services;

/// <summary>
/// Hardware #1 — Camera capture and local photo selection.
/// Uses <c>MediaPicker</c> on mobile; Windows uses webcam, file picker, or system Camera app.
/// </summary>
public class CameraService : ICameraService
{
    public Task<CameraCaptureResult> CapturePhotoAsync(CancellationToken cancellationToken = default)
    {
#if WINDOWS
        return CaptureWindowsAsync(cancellationToken);
#else
        return CaptureMobileAsync(cancellationToken);
#endif
    }

    public Task<CameraCaptureResult> PickPhotoAsync(CancellationToken cancellationToken = default)
    {
#if WINDOWS
        return PickWindowsAsync(cancellationToken);
#else
        return PickMobileAsync(cancellationToken);
#endif
    }

#if WINDOWS
    private static async Task<CameraCaptureResult> CaptureWindowsAsync(CancellationToken cancellationToken)
    {
        var result = await Platforms.Windows.WindowsCameraHelper.CapturePhotoAsync(cancellationToken);
        if (!result.Success || result.ImageBytes is null)
            return CameraCaptureResult.Fail(result.ErrorMessage ?? "Photo capture failed.");

        var image = ImageSource.FromStream(() => new MemoryStream(result.ImageBytes));
        return CameraCaptureResult.Ok(image);
    }

    private static async Task<CameraCaptureResult> PickWindowsAsync(CancellationToken cancellationToken)
    {
        var result = await Platforms.Windows.WindowsCameraHelper.PickPhotoAsync(cancellationToken);
        if (!result.Success || result.ImageBytes is null)
            return CameraCaptureResult.Fail(result.ErrorMessage ?? "Photo selection failed.");

        var image = ImageSource.FromStream(() => new MemoryStream(result.ImageBytes));
        return CameraCaptureResult.Ok(image);
    }
#else
    private async Task<CameraCaptureResult> CaptureMobileAsync(CancellationToken cancellationToken)
    {
        try
        {
            var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (cameraStatus != PermissionStatus.Granted)
                cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();

            if (cameraStatus != PermissionStatus.Granted)
            {
                return CameraCaptureResult.Fail(
                    "Camera permission denied. Open Settings → Apps → FoodExplorer → Permissions and allow Camera.");
            }

            if (!MediaPicker.Default.IsCaptureSupported)
                return CameraCaptureResult.Fail("This device does not support camera capture.");

            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Capture your dish"
            });

            return await LoadPhotoAsync(photo, cancellationToken, "Photo capture was cancelled.");
        }
        catch (Exception ex) when (IsKnownPhotoException(ex, out var message))
        {
            return CameraCaptureResult.Fail(message);
        }
        catch (Exception ex)
        {
            return FailFromException(ex);
        }
    }

    private async Task<CameraCaptureResult> PickMobileAsync(CancellationToken cancellationToken)
    {
        try
        {
            await EnsurePhotosPermissionAsync();

            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Choose your dish photo"
            });

            return await LoadPhotoAsync(photo, cancellationToken, "Photo selection was cancelled.");
        }
        catch (Exception ex) when (IsKnownPhotoException(ex, out var message))
        {
            return CameraCaptureResult.Fail(message);
        }
        catch (Exception ex)
        {
            return FailFromException(ex);
        }
    }

    private static async Task EnsurePhotosPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
        if (status != PermissionStatus.Granted)
            await Permissions.RequestAsync<Permissions.Photos>();
    }

    private static async Task<CameraCaptureResult> LoadPhotoAsync(
        FileResult? photo,
        CancellationToken cancellationToken,
        string cancelledMessage)
    {
        if (photo is null)
            return CameraCaptureResult.Fail(cancelledMessage);

        await using var stream = await photo.OpenReadAsync();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        var imageBytes = memoryStream.ToArray();

        if (imageBytes.Length == 0)
            return CameraCaptureResult.Fail("The selected photo is empty. Please try another image.");

        var imageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
        return CameraCaptureResult.Ok(imageSource);
    }

    private static bool IsKnownPhotoException(Exception ex, out string message)
    {
        message = ex switch
        {
            PermissionException =>
                "Photo access denied. Enable Photos or Storage permission in device settings.",
            FeatureNotSupportedException =>
                "Photo selection is not supported on this device.",
            OperationCanceledException =>
                "Photo selection was cancelled.",
            _ => string.Empty
        };

        return message.Length > 0;
    }

    private static CameraCaptureResult FailFromException(Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"[CameraService] {ex}");
        var detail = string.IsNullOrWhiteSpace(ex.Message) ? ex.GetType().Name : ex.Message;
        return CameraCaptureResult.Fail($"Photo error: {detail}");
    }
#endif
}
