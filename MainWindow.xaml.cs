using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.AspNetCore.SignalR.Client;
using RestSharp;

namespace Logger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static HubConnection connection;

        public MainWindow()
        {
            InitializeComponent();
            connection = new HubConnectionBuilder().WithUrl("https://oajibulu.azurewebsites.net/loghub").Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };
        }

        private void Scroll()
        {
            listerMain.ScrollIntoView(listerMain.Items.GetItemAt(listerMain.Items.Count - 1));
        }

        private void AddMain(string message)
        {
            listerMain.Items.Add(new TextBox() { Text = message, TextWrapping = TextWrapping.Wrap });
        }


        List<string> logs = new List<string>();
        List<string> logs_filter = new List<string>();
        private async Task Start()
        {
            try
            {
                if (connection.State == HubConnectionState.Disconnected)
                {
                    connection.On<string>(method.Text, (message) =>
                    {
                        logs.Add(message);
                        if (isSearching)
                        {
                            if (message.Contains(search_Phrase))
                            {
                                AddMain(message);
                            }
                        }
                        else
                        {
                            AddMain(message);
                            if (!hang)
                            {
                                //scroll to the button
                                Scroll();
                            }
                            if (listerMain.Items.Count > 5000)
                            {
                                listerMain.Items.Clear();
                            }
                        }
                    });

                    await connection.StartAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            listerMain.Items.Clear();
            logs.Clear();
            logs_filter.Clear();
        }

        private bool hang = false;
        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            hang = !hang;
            Button btn = (Button)sender;

            btn.Content = btn.Content.ToString() == "Freeze" ? "Un Freeze" : "Freeze";
            //if (btn.Content.ToString() == "Freeze")
            //     btn.Content = "Un Freeze";
            // else
            // {
            //     btn.Content = "Freeze";
            // }
        }

        private string search_Phrase = string.Empty;
        private bool isSearching = false;
        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                search_Phrase = search.Text;
                listerMain.Items.Clear();

                if (string.IsNullOrEmpty(search.Text))
                {
                    isSearching = false;
                    search_Phrase = string.Empty;
                    foreach (var pp in logs)
                        AddMain(pp);
                    if (!hang)
                    {
                        //scroll to the button
                        Scroll();
                    }
                }
                else
                {
                    isSearching = true;

                    //filter existing data
                    foreach (var pp in logs.Where(d => d.Contains(search_Phrase)))
                        AddMain(pp);
                    if (!hang)
                    {
                        //scroll to the button
                        Scroll();
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            RestClient client = new RestClient("https://oajibulu.azurewebsites.net/api/v1/ping");
            RestRequest request = new RestRequest(Method.GET);
            client.Execute(request);
        }
    }
}
