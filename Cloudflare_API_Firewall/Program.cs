using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Cloudflare_API_Firewall.Controller;

class Program
{
	static async Task Main()
	{
		await new MainController().Start();
	}
}
