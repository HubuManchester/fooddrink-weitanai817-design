namespace FoodExplorer.Services;

/// <summary>
/// Hardware #1 — Camera capture.
/// Uses <c>MediaPicker.CapturePhotoAsync</c> to photograph the user's cooked dish,
/// with permission and device-capability error handling.
/// </summary>
public class CameraService : ICameraService
{
    /// <summary>Captures a photo from the device camera and returns it as an <see cref="ImageSource"/>.</summary>
    public async Task<CameraCaptureResult> CapturePhotoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
                return CameraCaptureResult.Fail("This device does not support camera capture.");

            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Capture your dish"
            });

            if (photo is null)
                return CameraCaptureResult.Fail("Photo capture was cancelled.");

            await using var stream = await photo.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);
            var imageBytes = memoryStream.ToArray();

            if (imageBytes.Length == 0)
                return CameraCaptureResult.Fail("The captured photo is empty. Please try again.");

            var imageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
            return CameraCaptureResult.Ok(imageSource);
        }
        catch (PermissionException)
        {
            return CameraCaptureResult.Fail(
                "Camera permission denied. Enable camera access in device settings.");
        }
        catch (FeatureNotSupportedException)
        {
            return CameraCaptureResult.Fail("Camera is not supported on this device.");
        }
        catch (OperationCanceledException)
        {
            return CameraCaptureResult.Fail("Photo capture was cancelled.");
        }
        catch (Exception ex)
        {
            return CameraCaptureResult.Fail($"Camera error: {ex.Message}");
        }
    }
}
