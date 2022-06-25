using System;
using System.Windows;
using System.IO;
using DiscordRPC;
using DiscordRPC.Logging;
using Newtonsoft.Json;
using System.Net;

namespace PresenceSharpUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string clientpref = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\PresenceSharp\\UI\\clientpreferences.json";
        public MainWindow()
        {
            InitializeComponent();
            bHasTimer = false;
            string root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\PresenceSharp\\UI";
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            if (!File.Exists(clientpref))
            {
                InitializeComponent();
                try
                {
                    WebClient webclient = new WebClient();
                    webclient.DownloadFileAsync(new Uri("https://raw.githubusercontent.com/AyeItsAxi/PresenceSharpUI/main/reqfiles/clientpreferences.json"), clientpref);
                    MessageBox.Show("JSON not found, downloading example file...");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("A fatal error occurred (make sure you are connected to the internet: " + ex.Message);
                }
            }
            if (File.Exists(clientpref))
            {
                string DATA = File.ReadAllText(clientpref);
                LauncherCloud json = JsonConvert.DeserializeObject<LauncherCloud>(DATA);
                int charam = ClientID.Text.Length;
                if (charam != 18)
                {
                    cidtrue = false;
                }
                if (cidtrue = true & File.Exists(clientpref))
                {
                    Initialize();
                    ClientID.Text = json.cid;
                    Title.Text = json.title;
                    Subtitle.Text = json.subtitle;
                    LIN.Text = json.largeimagename;
                    LIT.Text = json.largeimagetext;
                    SIN.Text = json.smallimagename;
                    SIT.Text = json.smallimagetext;
                }
            }
        }
        //its time to start writing button logic :tiredpepe:
        private void Save(object sender, RoutedEventArgs e)
        {
            string DATA = File.ReadAllText(clientpref);
            LauncherCloud json = JsonConvert.DeserializeObject<LauncherCloud>(DATA);
            int charam = ClientID.Text.Length;
            if (charam != 18)
            {
                MessageBox.Show("Not a valid client ID!");
                ClientID.Text = json.cid;
                cidtrue = false;
            }
            if (cidtrue == false)
            {
                
            }
            bool charamtrue = charam == 18;
            if (cidtrue == true && charamtrue)
            {
                //this is a literal living hell to look at
                if (!File.Exists(clientpref))
                {
                    File.WriteAllText(clientpref, "{" + " " + "\"" + "cid\":" + "\"" + ClientID.Text + "\"" + "," + "\"" + "title\":" + "\"" + Title.Text + "\"" + "," + "\"" + "subtitle\":" + "\"" + Subtitle.Text + "\"" + "," + "\"" + "largeimagename\":" + "\"" + LIN.Text + "\"" + "," + "\"" + "largeimagename\":" + "\"" + LIT.Text + "\"" + "," + "\"" + "smallimagename\":" + "\"" + SIN.Text + "\"" + "," + "\"" + "smallimagetext\":" + "\"" + SIT.Text + "\"" + " " + "}");
                }
                else
                {
                    File.WriteAllText(clientpref, "{" + " " + "\"" + "cid\":" + "\"" + ClientID.Text + "\"" + "," + "\"" + "title\":" + "\"" + Title.Text + "\"" + "," + "\"" + "subtitle\":" + "\"" + Subtitle.Text + "\"" + "," + "\"" + "largeimagename\":" + "\"" + LIN.Text + "\"" + "," + "\"" + "largeimagetext\":" + "\"" + LIT.Text + "\"" + "," + "\"" + "smallimagename\":" + "\"" + SIN.Text + "\"" + "," + "\"" + "smallimagetext\":" + "\"" + SIT.Text + "\"" + " " + "}");
                    RefreshRPC();
                }
            }
        }
        private void Minimize (object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        public static DiscordRpcClient client;
        private bool cidtrue;

        private static void Initialize()
        {
            string clientpref = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\PresenceSharp\\UI\\clientpreferences.json";
            try
            {
                string DATA = File.ReadAllText(clientpref);
                LauncherCloud json = JsonConvert.DeserializeObject<LauncherCloud>(DATA);
                client = new DiscordRpcClient(json.cid);
                client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
                client.OnReady += (sender, e) =>
                {
                    Console.WriteLine("Received Ready from user {0}", e.User.Username);
                };
                client.OnPresenceUpdate += (sender, e) =>
                {
                    Console.WriteLine("Received Update! {0}", e.Presence);
                };
                client.Initialize();
                client.SetPresence(new RichPresence()
                {
                    Details = json.title,
                    State = json.subtitle,
                    Assets = new Assets()
                    {
                        LargeImageKey = json.largeimagename,
                        LargeImageText = json.largeimagetext,
                        SmallImageKey = json.smallimagename,
                        SmallImageText = json.smallimagetext
                    }
                });
                MessageBox.Show("RPC Initialized!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a fatal error starting the RPC: " + ex.ToString());
            }

        }
        private static void RefreshRPC()
        {
            string clientpref = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\PresenceSharp\\UI\\clientpreferences.json";
            string DATA = File.ReadAllText(clientpref);
            LauncherCloud json = JsonConvert.DeserializeObject<LauncherCloud>(DATA);
            client.SetPresence(new RichPresence()
            {
                Details = json.title,
                State = json.subtitle,
                Assets = new Assets()
                {
                    LargeImageKey = json.largeimagename,
                    LargeImageText = json.largeimagetext,
                    SmallImageKey = json.smallimagename,
                    SmallImageText = json.smallimagetext
                }
            });
            MessageBox.Show("RPC updated!", "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public class LauncherCloud
        {
            public string title { get; set; }
            public string subtitle { get; set; }
            public string cid { get; set; }
            public string largeimagename { get; set; }
            public string largeimagetext { get; set; }
            public string smallimagename { get; set; }
            public string smallimagetext { get; set; }
        }

        public bool bHasTimer;

        private void ClientCallUpdateStartTime(object sender, RoutedEventArgs e)
        {
            if (bHasTimer == true)
            {
                client.UpdateClearTime();
                bHasTimer = false;
            }
            else if (bHasTimer == false)
            {
                client.UpdateStartTime();
                bHasTimer = true;
            }
        }
    }
}