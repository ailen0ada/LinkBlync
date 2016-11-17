using AppKit;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace LinkBlync.Client.Mac
{
	[Register("AppDelegate")]
	public class AppDelegate : FormsApplicationDelegate
	{
		private readonly NSWindow mainWindow;

		public AppDelegate()
		{
			var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

			var rect = new CoreGraphics.CGRect(200, 1000, 600, 400);
			mainWindow = new NSWindow(rect, style, NSBackingStore.Buffered, false);
			mainWindow.Title = "LinkBlync.Mac";
		}

		public override NSWindow MainWindow => mainWindow;

		public override void DidFinishLaunching(NSNotification notification)
		{
			Forms.Init();

			LoadApplication(new App());

			base.DidFinishLaunching(notification);
		}

		public override void WillTerminate(NSNotification notification)
		{
			// Insert code here to tear down your application
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;
	}
}
