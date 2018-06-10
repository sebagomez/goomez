using System;
using System.IO;
using System.Net.Http;
using Terminal.Gui;

namespace GoomezTerminal
{
    class Program
    {
        static void Main(string[] args)
        {
			Application.Init();

			Window win = new Window(new Rect(0, 1, Application.Top.Frame.Width, Application.Top.Frame.Height - 1), "Goomez");
			TextField query = new TextField(10, 10, 25, "");

			HttpClient http = new HttpClient();

			Button button = new Button(40, 10, "Search")
			{
				IsDefault = true,
				Clicked = async() => 
				{
					//int i = MessageBox.Query(50, 25, "Info", $"Serch for {query.Text}");
					//http://sgomez-i7/goomez/api/Search/Search?pattern=SQL Server

					string url = $"http://sgomez-i7/goomez/api/Search/Search?pattern={query.Text}";
					HttpRequestMessage reqMsg = new HttpRequestMessage(HttpMethod.Get, url);
					HttpResponseMessage response = await http.SendAsync(reqMsg);
					string body = await response.Content.ReadAsStringAsync();

					
				}
			};

			win.Add(query);
			win.Add(button);

			Application.Top.Add(win);

			Application.Run();
        }
	}
}
