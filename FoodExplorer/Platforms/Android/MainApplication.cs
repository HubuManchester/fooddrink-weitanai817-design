using Android.App;
using Android.Runtime;

namespace FoodExplorer;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
		AndroidEnvironment.UnhandledExceptionRaiser += (_, e) =>
		{
			System.Diagnostics.Debug.WriteLine($"[FoodExplorer Android] Unhandled: {e.Exception}");
		};
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
