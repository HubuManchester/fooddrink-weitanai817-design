namespace FoodExplorer.Services;

public record CameraCaptureResult(bool Success, ImageSource? Image, string? ErrorMessage)
{
    public static CameraCaptureResult Ok(ImageSource image) => new(true, image, null);
    public static CameraCaptureResult Fail(string error) => new(false, null, error);
}

public interface ICameraService
{
    Task<CameraCaptureResult> CapturePhotoAsync(CancellationToken cancellationToken = default);
}
