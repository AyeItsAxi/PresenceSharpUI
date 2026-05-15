using System;
using DiscordRPC;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;
using PresenceSharpUI.Models;
using PresenceSharpUI.Helpers;
using PresenceSharpUI.Services;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;

namespace PresenceSharpUI.Views
{
    public partial class MainWindow
    {
        private const string DefaultAvatarUrl =
            "https://cdn.discordapp.com/embed/avatars/0.png";

        private readonly DiscordPresenceService _presenceService = new();

        private string _lastAvatarUrl;
        private bool _bPresenceConnected;
        private readonly bool _isFullyLoaded;

        private static BitmapImage _defaultAvatarImage;

        private static BitmapImage DefaultAvatarImage =>
            _defaultAvatarImage ??= BitmapHelper.Create(DefaultAvatarUrl);

        public MainWindow()
        {
            InitializeComponent();

            RegisterPresenceEvents();
            LoadPreferences();
            
            _isFullyLoaded = true;

            InitializePresence();
            _ = EnsurePresenceConnectionAsync();
        }

        private void RegisterPresenceEvents()
        {
            _presenceService.PresenceUpdated += OnPresenceUpdated;
            _presenceService.ConnectionFailed += OnConnectionFailed;
        }

        private void LoadPreferences()
        {
            PresencePreferencesService.EnsureExists();
            ApplyUserDataToUi(PresencePreferencesService.Load());
        }

        private void SavePreferences()
        {
            PresencePreferencesService.Save(GetUserDataFromInput());
        }

        private void InitializePresence()
        {
            try
            {
                _presenceService.Initialize(ClientIDTextBox.Text);

                var includeAssets = _isFullyLoaded;
                _presenceService.SetPresence(CreatePresence(includeAssets));

                UpdateSmallImageVisibility();
                UpdateActivityPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There was a fatal error starting the RPC: {ex}");
            }
        }

        private async Task EnsurePresenceConnectionAsync()
        {
            await Task.Delay(20_000);

            while (!_bPresenceConnected)
            {
                InitializePresence();
                await Task.Delay(30_000);
            }
        }

        private void OnPresenceUpdated()
        {
            Dispatcher.Invoke(() =>
            {
                RpcSuccess();
                _bPresenceConnected = true;
            });
        }

        private void OnConnectionFailed()
        {
            Dispatcher.Invoke(() =>
            {
                RpcFailure();
                _bPresenceConnected = false;
            });
        }

        private RichPresence CreatePresence(bool includeAssets)
        {
            return new RichPresence
            {
                Details = TitleTextBox.Text,
                State = SubtitleTextBox.Text,
                Assets = includeAssets ? CreateAssets() : null
            };
        }
        
        private Assets CreateAssets()
        {
            var hasLargeImage = !string.IsNullOrWhiteSpace(LargeImageNameTextBox.Text);
            var hasSmallImage = !string.IsNullOrWhiteSpace(SmallImageNameTextBox.Text);

            if (!hasLargeImage || !hasSmallImage)
                return null;

            return new Assets
            {
                LargeImageKey = LargeImageNameTextBox.Text,
                LargeImageText = LargeImageHoverTextBox.Text,
                SmallImageKey = SmallImageNameTextBox.Text,
                SmallImageText = SmallImageHoverTextBox.Text
            };
        }

        private PsuiUserData GetUserDataFromInput()
        {
            return new PsuiUserData
            {
                I64ApplicationId = long.Parse(ClientIDTextBox.Text),
                StrTitle = TitleTextBox.Text,
                StrSubtitle = SubtitleTextBox.Text,
                StrLargeImageName = LargeImageNameTextBox.Text,
                StrLargeImageText = LargeImageHoverTextBox.Text,
                StrSmallImageName = SmallImageNameTextBox.Text,
                StrSmallImageText = SmallImageHoverTextBox.Text
            };
        }

        private void ApplyUserDataToUi(PsuiUserData prefs)
        {
            ClientIDTextBox.Text = prefs.I64ApplicationId.ToString();
            TitleTextBox.Text = prefs.StrTitle;
            SubtitleTextBox.Text = prefs.StrSubtitle;
            LargeImageNameTextBox.Text = prefs.StrLargeImageName;
            LargeImageHoverTextBox.Text = prefs.StrLargeImageText;
            SmallImageNameTextBox.Text = prefs.StrSmallImageName;
            SmallImageHoverTextBox.Text = prefs.StrSmallImageText;
        }

        private void UpdateSmallImageVisibility()
        {
            var visibility = string.IsNullOrWhiteSpace(SmallImageNameTextBox.Text)
                ? Visibility.Hidden
                : Visibility.Visible;

            SmallImageBackdropEllipse.Visibility = visibility;
            SmallImageEllipse.Visibility = visibility;
        }

        private void UpdateActivityPreview()
        {
            UserActivityName.Content = "Cool Application Name";
            UserActivityText.Text = TitleTextBox.Text;
            UserActivityStatus.Text = SubtitleTextBox.Text;
        }

        private void RpcSuccess()
        {
            var user = _presenceService.CurrentUser;

            if (user is null)
                return;

            var avatarUrl = user.GetAvatarURL(User.AvatarFormat.PNG, User.AvatarSize.x256);

            UserName.Text = user.Username;

            if (_lastAvatarUrl != avatarUrl)
            {
                _lastAvatarUrl = avatarUrl;
                UserProfilePictureImageSource.ImageSource = BitmapHelper.Create(avatarUrl);
            }

            SetOnlineStatus(Brushes.Green);
        }

        private void RpcFailure()
        {
            _lastAvatarUrl = null;
            UserProfilePictureImageSource.ImageSource = DefaultAvatarImage;
            SetOnlineStatus(Brushes.Red);
        }

        private void SetOnlineStatus(Brush brush)
        {
            UserOnlineAppearance.StrokeThickness = 15;
            UserOnlineAppearance.Stroke = brush;
            ServiceStatusEllipse.Fill = brush;
        }

        private void RefreshPresence(object sender, RoutedEventArgs e)
        {
            SavePreferences();
            InitializePresence();
        }

        private void ClientIDTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) =>
            e.Handled = !RegexHelper.IsTextAllowed(e.Text);

        private void Minimize(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void CloseWindow(object sender, RoutedEventArgs e) =>
            Close();

        private void DragBar_MouseDown(object sender, MouseButtonEventArgs e) =>
            DragMove();

        protected override void OnClosed(EventArgs e)
        {
            _presenceService.Dispose();
            base.OnClosed(e);
        }
    }
}