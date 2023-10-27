using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cloudflare_API_Firewall.Controller
{
	internal class MainController
	{
		View.View view = new View.View();

		internal void Start()
		{
			GetIpAddress();
		}

		/// <summary>
		/// This method gets the public ip address, which gets provided from a website.
		/// </summary>
		/// <returns></returns>
		private string GetIpAddress()
		{
			view.Message("Fetching your public IP address...");
			string url = "https://icanhazip.com";
			WebClient client = new WebClient();

			// Uses trim because the string originally ends with "\n"
			string publicIPAddress = client.DownloadString(url).Trim();

			view.Message($"Your public IP address is: {publicIPAddress}");

			Model.SaveCurrentPublicIp saveIp = new Model.SaveCurrentPublicIp();
			saveIp.SaveIp(publicIPAddress);

			return publicIPAddress;
		}
	}
}
