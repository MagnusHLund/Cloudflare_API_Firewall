using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloudflare_API_Firewall.Model
{
	internal class FileHandling
	{
		static string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\MagnusLund\CloudFlareFirewall\");
		string filePath = path + @"ip.txt";

		View.View view = new View.View();

		internal bool CreateFile()
		{
			if (!File.Exists(filePath))
			{
				Directory.CreateDirectory(path);
				File.Create(filePath).Close();

				return false;
			}

			return true;
		}

		internal string SaveIpAddress(string ip)
		{
			StreamWriter sw = new StreamWriter(filePath);
			sw.WriteLine(ip);
			sw.Close();

			return filePath;
		}

		internal string ReadIpAddress()
		{
			using (StreamReader sr = new StreamReader(filePath))
			{
				return sr.ReadToEnd();
			}
		}
	}
}
