using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Blynclight;
using Microsoft.ServiceBus.Messaging;

namespace LinkBlync.Host
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BlynclightController controller = new BlynclightController();

        private int numberOfDevices;
        
        private const string ConnectionString = "Endpoint=sb://link-blync.servicebus.windows.net/;SharedAccessKeyName=subscriber;SharedAccessKey=Asyv1Qr5g/efQ6JAexYQdWDz0bfphjDrE5JuIV12qPk=;EntityPath=colors";

        private QueueClient client;

        private IDisposable topicSubscriber;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            numberOfDevices = controller.InitBlyncDevices();
            if (numberOfDevices < 1)
            {
                MessageBox.Show("No Blynclight device was found.");
                Environment.Exit(1);
            }

            controller.ResetLight(0);
            StatusLabel.Text = controller.aoDevInfo[0].szDeviceName;
            
            controller.TurnOnGreenLight(0);
            
            client = QueueClient.CreateFromConnectionString(ConnectionString);

            topicSubscriber = Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(100))
                .Select(_ => client.Receive())
                .Where(msg => msg != null)
                .Timestamp()
                .Do(t => Dispatcher.Invoke(() => StatusLabel.Text = $"{t.Timestamp.LocalDateTime:yyyy/MM/dd HH:mm:ss}"))
                .Select(t =>
                {
                    t.Value.Complete();
                    using (var sr = new StreamReader(t.Value.GetBody<Stream>()))
                    {
                        var val = sr.ReadToEnd();
                        return ToColor(val);
                    }
                })
                .Catch(Observable.Return(Color.FromArgb(0xff, 0xff, 0x00, 0x00)))
                .Subscribe(c =>
                {
                    Dispatcher.Invoke(() => CurrentColorRect.Background = new SolidColorBrush(c));
                    controller.TurnOnRGBLights(0, c.R, c.G, c.B);
                });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            topicSubscriber?.Dispose();
            client.Close();
            controller.ResetLight(0);
            controller.CloseDevices(numberOfDevices);
            base.OnClosing(e);
        }

        private async void ResetButtonOnClick(object sender, RoutedEventArgs e)
        {
            var message = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes("#FFFFFF")), true);
            await client.SendAsync(message);
        }

        private static Color ToColor(string value)
        {
            var converted = ColorConverter.ConvertFromString(value);
            if (converted != null) return (Color) converted;
            return Color.FromRgb(0xff, 0x00, 0x00);
        }
    }
}
