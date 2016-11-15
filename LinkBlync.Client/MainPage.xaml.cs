using System;
using Xamarin.Forms;
using System.Linq;
using System.Windows.Input;
using System.Diagnostics;

namespace LinkBlync.Client
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            sendColorCommand = new Command<string>(SendColor);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ShuffleColors(this, null);
        }

        private static Random rnd = new Random();

        private ICommand sendColorCommand;

        private void ShuffleColors(object sender, System.EventArgs e)
        {
            var colors = Enumerable.Range(0, 16).Select(_ => Color.FromHsla(rnd.NextDouble(), 1, 0.5)).InSetsOf(4);
            ColorGrid.Children.Clear();
            var parent = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Center,
                Orientation = StackOrientation.Vertical
            };
            foreach (var grp in colors)
            {
                var sl = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.Center,
                    Orientation = StackOrientation.Horizontal
                };
                foreach (var color in grp)
                {
                    var r = (int)(color.R * 255);
                    var g = (int)(color.G * 255);
                    var b = (int)(color.B * 255);
                    var val = $"#{r:X2}{g:X2}{b:X2}";

                    Func<double, double> colorConv = c => c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
                    var l = 0.2126 * colorConv(color.R) + 0.587 * colorConv(color.G) + 0.114 * colorConv(color.B);

                    var btn = new Button
                    {
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        Text = val,
                        TextColor = l > 0.179 ? Color.Black : Color.White,
                        BackgroundColor = color,
                        Command = sendColorCommand,
                        CommandParameter = val
                    };
                    sl.Children.Add(btn);
                }
                parent.Children.Add(sl);
            }
            ColorGrid.Children.Add(parent);
        }

        private void SendColor(string colorValue)
        {
            Debug.WriteLine(colorValue);
        }
    }
}
