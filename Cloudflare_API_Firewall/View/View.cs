using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloudflare_API_Firewall.View
{
	internal class View
	{
		internal void Message(string message)
		{
			Console.WriteLine(message);
		}

		internal void ChangeColor(ConsoleColor color)
		{
			Console.ForegroundColor = color;
		}
	}
}
