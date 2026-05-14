using System;
using System.IO;
using DiscordRPC;
using System.Windows;
using Newtonsoft.Json;
using DiscordRPC.Logging;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Application;

namespace PresenceSharpUI
{
    public partial class MainWindow
    {
        private readonly string _clientpref = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\PresenceSharp\UI\UserPreferences.json";
        public MainWindow()
        {
            InitializeComponent();
            var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\PresenceSharp\UI";
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            if (!File.Exists(_clientpref))
            {
                PsuiUserData udDefault = new()
                {
                    I64ApplicationId = 1061800604051189830,
                    StrTitle = "This is an example title",
                    StrSubtitle = "This is an example subtitle",
                    StrLargeImageName = "appicon",
                    StrLargeImageText = "Example text",
                    StrSmallImageName = "appicon",
                    StrSmallImageText = "Example text"
                };
                File.WriteAllText(_clientpref, JsonConvert.SerializeObject(udDefault));
            }
            if (File.Exists(_clientpref))
            {
                DeserializeApplicationPreferences();
            }
            Initialize();
            EnsurePresenceConnection();
        }
        private async void EnsurePresenceConnection()
        {
            try
            {
                await Task.Delay(20000);
                for (var i = 0; i < 50; i++)
                {
                    // Are we already connected? Break the loop if we are.
                    if (_bPresenceConnected) break;
                    
                    // Try again if not
                    Initialize();
                    await Task.Delay(30000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private void SerializeApplicationPreferences()
        {
            PsuiUserData userData = new()
            {
                I64ApplicationId = Convert.ToInt64(ClientIDTextBox.Text),
                StrTitle = TitleTextBox.Text,
                StrSubtitle = SubtitleTextBox.Text,
                StrLargeImageName = LargeImageNameTextBox.Text,
                StrLargeImageText = LargeImageHoverTextBox.Text,
                StrSmallImageName = SmallImageNameTextBox.Text,
                StrSmallImageText = SmallImageHoverTextBox.Text
            };
            File.WriteAllText(_clientpref, JsonConvert.SerializeObject(userData));
        }
        private void DeserializeApplicationPreferences()
        {
            PsuiUserData prefs = JsonConvert.DeserializeObject<PsuiUserData>(File.ReadAllText(_clientpref));
            ClientIDTextBox.Text = prefs.I64ApplicationId.ToString();
            TitleTextBox.Text = prefs.StrTitle;
            SubtitleTextBox.Text = prefs.StrSubtitle;
            LargeImageNameTextBox.Text = prefs.StrLargeImageName;
            LargeImageHoverTextBox.Text = prefs.StrLargeImageText;
            SmallImageNameTextBox.Text = prefs.StrSmallImageName;
            SmallImageHoverTextBox.Text = prefs.StrSmallImageText;
        }
        private void Minimize (object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private static DiscordRpcClient _client;

        private void Initialize()
        {
            try
            {
                _client = new DiscordRpcClient(ClientIDTextBox.Text);
                _client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
                if (_client.IsInitialized)
                {
                    _client.Dispose();
                }
                _client.OnReady += (_, e) =>
                {
                    Console.WriteLine("Received Ready from user {0}", e.User.Username);
                };
                _client.OnPresenceUpdate += (_, e) =>
                {
                    Console.WriteLine("Received Update! {0}", e.Presence);
                    Application.Current.Dispatcher.Invoke(RpcSuccess, System.Windows.Threading.DispatcherPriority.ContextIdle);
                    _bPresenceConnected = true;
                };
                _client.OnConnectionFailed += (_, e) =>
                {
                    Console.WriteLine("Received Error! {0}", e.FailedPipe);
                    Application.Current.Dispatcher.Invoke(RpcFailure, System.Windows.Threading.DispatcherPriority.ContextIdle);
                    _bPresenceConnected = false;
                };
                _client.Initialize();
                _client.SetPresence(new RichPresence()
                {
                    Details = TitleTextBox.Text,
                    State = SubtitleTextBox.Text,
                    Assets = new Assets()
                    {
                        LargeImageKey = LargeImageNameTextBox.Text,
                        LargeImageText = LargeImageHoverTextBox.Text,
                        SmallImageKey = SmallImageNameTextBox.Text,
                        SmallImageText = SmallImageHoverTextBox.Text
                    }
                });
                switch (SmallImageNameTextBox.Text.Length)
                {
                    case 0:
                        SmallImageBackdropEllipse.Visibility = Visibility.Hidden;
                        SmallImageEllipse.Visibility = Visibility.Hidden;
                        break;
                    default:
                        SmallImageBackdropEllipse.Visibility = Visibility.Visible;
                        SmallImageEllipse.Visibility = Visibility.Visible;
                        break;
                }
                UserActivityName.Content = "Cool Application Name";
                UserActivityText.Text = TitleTextBox.Text;
                UserActivityStatus.Text = TitleTextBox.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a fatal error starting the RPC: " + ex);
            }

        }

        public class PsuiUserData
        {
            public long I64ApplicationId { get; init; }
            public string StrTitle { get; init; }
            public string StrSubtitle { get; init; }
            public string StrLargeImageName { get; init; }
            public string StrLargeImageText { get; init; }
            public string StrSmallImageName { get; init; }
            public string StrSmallImageText { get; init; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _client.Deinitialize();
            _client.Dispose();
            Close();
        }

        private void ClientIDTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static readonly Regex Regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !Regex.IsMatch(text);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Initialize();
            SerializeApplicationPreferences();
        }
        private void RpcSuccess() 
        {
            UserName.Text = _client.CurrentUser.Username;
            BitmapImage bmp = new();
            bmp.BeginInit();
            bmp.UriSource = new Uri(_client.CurrentUser.GetAvatarURL(User.AvatarFormat.PNG, User.AvatarSize.x256));
            bmp.EndInit();
            UserProfilePictureImageSource.ImageSource = bmp;
            UserOnlineAppearance.StrokeThickness = 15;
            UserOnlineAppearance.Stroke = Brushes.Green;
            ServiceStatusEllipse.Fill = Brushes.Green;
        }
        private void RpcFailure()
        {
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri("https://cdn.discordapp.com/embed/avatars/0.png");
            bmp.EndInit();
            UserProfilePictureImageSource.ImageSource = bmp;
            ServiceStatusEllipse.Fill = Brushes.Red;
        }

        private void DragBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private bool _bPresenceConnected;
    }
}