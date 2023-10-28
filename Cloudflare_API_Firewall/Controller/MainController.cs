using Cloudflare_API_Firewall.Model;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Cloudflare_API_Firewall.Controller
{
	internal class MainController
	{
		View.View view = new View.View();
		FileHandling fileSaving = new FileHandling();

		internal async Task Start()
		{
			try
			{
				view.Message("Checking if file is created");

				if (fileSaving.CreateFile())
				{
					view.ChangeColor(ConsoleColor.Blue);
					view.Message("Save file was already created");
				}
				else
				{
					view.ChangeColor(ConsoleColor.Green);
					view.Message("Save file did not exist. Was created.");
				}

				view.ChangeColor(ConsoleColor.White);
				view.Message("Reading save file...");
				string oldIp = fileSaving.ReadIpAddress();

				view.ChangeColor(ConsoleColor.Blue);
				view.Message("Previous IP Address retrieved");

				view.ChangeColor(ConsoleColor.White);
				string newIp = GetPublicIpAddress();

				view.ChangeColor(ConsoleColor.White);
				view.Message("Connecting to cloudflare...");
				CloudflareController cloudflare = new CloudflareController();
				await cloudflare.EditFirewall(oldIp, newIp);

			}
			catch (Exception ex)
			{
				view.ChangeColor(ConsoleColor.Red);
				Console.WriteLine($"{ex.Message}\nClosing program in 5 seconds...");
				Thread.Sleep(5000);
				Environment.Exit(69); // Nice
			}
		}

		private string GetPublicIpAddress()
		{
			view.Message("Fetching current public IP address...");
			string url = "https://icanhazip.com";
			WebClient client = new WebClient();

			// Uses trim because the string originally ends with "\n"
			string publicIPAddress = client.DownloadString(url).Trim();

			view.ChangeColor(ConsoleColor.Green);
			view.Message($"Your public IP address is: {publicIPAddress}");

			view.ChangeColor(ConsoleColor.White);
			view.Message("Saving file...");

			view.ChangeColor(ConsoleColor.Blue);
			view.Message($"File saved at: {fileSaving.SaveIpAddress(publicIPAddress)}");

			return publicIPAddress;
		}
	}
}
