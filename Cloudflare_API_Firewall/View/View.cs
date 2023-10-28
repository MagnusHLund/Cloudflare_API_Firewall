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

		/// <summary>
		/// This method is responsible for changing the text colors.
		/// Each color has a specific meaning:
		/// error = red, success = green, Statement = blue, fetching = white
		/// </summary>
		/// <param name="color"></param>
		internal void ChangeColor(ConsoleColor color)
		{
			Console.ForegroundColor = color;
		}
	}
}
