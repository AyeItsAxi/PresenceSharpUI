using System;
using System.IO;
using System.Net;
using DiscordRPC;
using System.Windows;
using Newtonsoft.Json;
using DiscordRPC.Logging;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;
using DiscordRPC.Events;
using System.Threading.Tasks;

namespace PresenceSharpUI
{
    public partial class MainWindow : Window
    {
        private readonly string clientpref = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\PresenceSharp\\UI\\UserPreferences.json";
        public MainWindow()
        {
            InitializeComponent();
            string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\PresenceSharp\\UI";
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            if (!File.Exists(clientpref))
            {
                PSUIUserData UDDefault = new()
                {
                    I64ApplicationID = 1061800604051189830,
                    StrTitle = "This is an example title",
                    StrSubtitle = "This is an example subtitle",
                    StrLargeImageName = "appicon",
                    StrLargeImageText = "Example text",
                    StrSmallImageName = "appicon",
                    StrSmallImageText = "Example text",
                    BUseTimer = false
                };
                File.WriteAllText(clientpref, JsonConvert.SerializeObject(UDDefault));
            }
            if (File.Exists(clientpref))
            {
                DeserializeApplicationPreferences();
            }
            Initialize();
            EnsurePresenceConnection();
        }
        private async void EnsurePresenceConnection()
        {
            await Task.Delay(20000);
            for (int i = 0; i < 50; i++)
            {
                if (BPresenceConnected == true)
                {
                    break;
                }
                if (BPresenceConnected == false)
                {
                    Initialize();
                    await Task.Delay(30000);
                }
            }
        }
        private void SerializeApplicationPreferences()
        {
            PSUIUserData UD = new()
            {
                I64ApplicationID = Convert.ToInt64(ClientIDTextBox.Text),
                StrTitle = TitleTextBox.Text,
                StrSubtitle = SubtitleTextBox.Text,
                StrLargeImageName = LargeImageNameTextBox.Text,
                StrLargeImageText = LargeImageHoverTextBox.Text,
                StrSmallImageName = SmallImageNameTextBox.Text,
                StrSmallImageText = SmallImageHoverTextBox.Text,
                BUseTimer = (bool)TimerCheckBox.IsChecked
            };
            File.WriteAllText(clientpref, JsonConvert.SerializeObject(UD));
        }
        private void DeserializeApplicationPreferences()
        {
            PSUIUserData prefs = JsonConvert.DeserializeObject<PSUIUserData>(File.ReadAllText(clientpref));
            ClientIDTextBox.Text = prefs.I64ApplicationID.ToString();
            TitleTextBox.Text = prefs.StrTitle;
            SubtitleTextBox.Text = prefs.StrSubtitle;
            LargeImageNameTextBox.Text = prefs.StrLargeImageName;
            LargeImageHoverTextBox.Text = prefs.StrLargeImageText;
            SmallImageNameTextBox.Text = prefs.StrSmallImageName;
            SmallImageHoverTextBox.Text = prefs.StrSmallImageText;
            TimerCheckBox.IsChecked = prefs.BUseTimer;
        }
        private void Minimize (object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        public static DiscordRpcClient client;

        private void Initialize()
        {
            try
            {
                client = new DiscordRpcClient(ClientIDTextBox.Text);
                client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
                if (client.IsInitialized)
                {
                    client.Dispose();
                }
                client.OnReady += (sender, e) =>
                {
                    Console.WriteLine("Received Ready from user {0}", e.User.Username);
                };
                client.OnPresenceUpdate += (sender, e) =>
                {
                    Console.WriteLine("Received Update! {0}", e.Presence);
                    Application.Current.Dispatcher.Invoke(RPCSuccess, System.Windows.Threading.DispatcherPriority.ContextIdle);
                    BPresenceConnected = true;
                };
                client.OnConnectionFailed += (sender, e) =>
                {
                    Console.WriteLine("Received Error! {0}", e.FailedPipe);
                    Application.Current.Dispatcher.Invoke(RPCFailure, System.Windows.Threading.DispatcherPriority.ContextIdle);
                    BPresenceConnected = false;
                };
                client.Initialize();
                client.SetPresence(new RichPresence()
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
                if ((bool)TimerCheckBox.IsChecked)
                {
                    client.UpdateStartTime();
                }
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
                MessageBox.Show("There was a fatal error starting the RPC: " + ex.ToString());
            }

        }

        public class PSUIUserData
        {
            public long I64ApplicationID { get; set; }
            public string StrTitle { get; set; }
            public string StrSubtitle { get; set; }
            public string StrLargeImageName { get; set; }
            public string StrLargeImageText { get; set; }
            public string StrSmallImageName { get; set; }
            public string StrSmallImageText { get; set; }
            public bool BUseTimer { get; set; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ClientIDTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyData != Keys.Back)
                e.SuppressKeyPress = !int.TryParse(Convert.ToString((char)e.KeyData), out int _);
        }

        private void ClientIDTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Initialize();
            SerializeApplicationPreferences();
        }
        private void RPCSuccess() 
        {
            UserName.Text = client.CurrentUser.Username;
            UserDiscriminator.Text = "#" + client.CurrentUser.Discriminator.ToString("0000");
            BitmapImage bmp = new();
            bmp.BeginInit();
            bmp.UriSource = new Uri(client.CurrentUser.GetAvatarURL(User.AvatarFormat.PNG, User.AvatarSize.x256));
            bmp.EndInit();
            UserProfilePictureImageSource.ImageSource = bmp;
            UserOnlineAppearance.StrokeThickness = 15;
            UserOnlineAppearance.Stroke = Brushes.Green;
            ServiceStatusEllipse.Fill = Brushes.Green;
        }
        private void RPCFailure()
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
                this.DragMove();
            }
        }
        private bool BPresenceConnected = false;
    }
}