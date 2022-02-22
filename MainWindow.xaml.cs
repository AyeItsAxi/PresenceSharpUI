using System;
using System.Windows;
using System.IO;
using DiscordRPC;
using DiscordRPC.Logging;
using Newtonsoft.Json;
namespace PresenceSharpUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (!File.Exists("./clientpreferences.json"))
            {
                InitializeComponent();
                MessageBox.Show("JSON not found, downloading example...");
            }
            if (File.Exists("./clientpreferences.json"))
            {
                string DATA = File.ReadAllText("./clientpreferences.json");
                LauncherCloud json = JsonConvert.DeserializeObject<LauncherCloud>(DATA);
                InitializeComponent();
                int charam = ClientID.Text.Length;
                if (charam != 18)
                {
                    cidtrue = false;
                }
                if (cidtrue = true & File.Exists("./clientpreferences.json"))
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
                if (!File.Exists("./clientpreferences.json"))
                {
                    MessageBox.Show("JSON not found, downloading example file...");
                }
            }
        }
        //its time to start writing button logic :tiredpepe:
        private void Save(object sender, RoutedEventArgs e)
        {
            int charam = ClientID.Text.Length;
            if (charam != 18)
            {
                MessageBox.Show("Not a valid client ID!");
                cidtrue = false;
            }
            if (cidtrue == false)
            {
                
            }
            if (cidtrue == true)
            {
                string DATA = File.ReadAllText("./clientpreferences.json");
                LauncherCloud json = JsonConvert.DeserializeObject<LauncherCloud>(DATA);
                //this is a literal living hell to look at
                if (!File.Exists("clientpreferences.json"))
                {
                    File.WriteAllText(@"clientpreferences.json", "{" + " " + "\"" + "cid\":" + "\"" + ClientID.Text + "\"" + "," + "\"" + "title\":" + "\"" + Title.Text + "\"" + "," + "\"" + "subtitle\":" + "\"" + Subtitle.Text + "\"" + "," + "\"" + "largeimagename\":" + "\"" + LIN.Text + "\"" + "," + "\"" + "largeimagename\":" + "\"" + LIT.Text + "\"" + "," + "\"" + "smallimagename\":" + "\"" + SIN.Text + "\"" + "," + "\"" + "smallimagetext\":" + "\"" + SIT.Text + "\"" + " " + "}");
                }
                else
                {
                    File.WriteAllText(@"clientpreferences.json", "{" + " " + "\"" + "cid\":" + "\"" + ClientID.Text + "\"" + "," + "\"" + "title\":" + "\"" + Title.Text + "\"" + "," + "\"" + "subtitle\":" + "\"" + Subtitle.Text + "\"" + "," + "\"" + "largeimagename\":" + "\"" + LIN.Text + "\"" + "," + "\"" + "largeimagetext\":" + "\"" + LIT.Text + "\"" + "," + "\"" + "smallimagename\":" + "\"" + SIN.Text + "\"" + "," + "\"" + "smallimagetext\":" + "\"" + SIT.Text + "\"" + " " + "}");
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
            }
        }
        public static DiscordRpcClient client;
        private bool cidtrue;

        private static void Initialize()
        {
            try
            {
                string DATA = File.ReadAllText("./clientpreferences.json");
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
    }
}
