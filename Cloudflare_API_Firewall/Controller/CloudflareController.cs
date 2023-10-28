using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Configuration;
using Cloudflare_API_Firewall.Model;
using System.Text;

namespace Cloudflare_API_Firewall.Controller
{
	internal class CloudflareController
	{
		View.View view = new View.View();

		internal async Task EditFirewall(string oldIp, string newIp)
		{
			string apiToken = ConfigurationManager.AppSettings["ApiToken"];
			string zoneId = ConfigurationManager.AppSettings["ZoneId"];
			string ruleId = ConfigurationManager.AppSettings["RuleId"];

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiToken}");
				// Fetch the existing firewall rule
				HttpResponseMessage getResponse = await client.GetAsync($"https://api.cloudflare.com/client/v4/zones/{zoneId}/firewall/rules/{ruleId}");

				if (getResponse.IsSuccessStatusCode)
				{
					view.ChangeColor(ConsoleColor.Green);
					view.Message("Connection established!");
					view.ChangeColor(ConsoleColor.White);
					view.Message("Fetching ip addresses...");

					string responseContent = await getResponse.Content.ReadAsStringAsync();

					// Extract and parse JSON to get the expression
					string expression = ExtractExpressionFromResponse(responseContent);

					string[] ipAddresses = ExtractIPAddresses(expression);

					// Display the extracted IP addresses
					view.ChangeColor(ConsoleColor.Green);
					view.Message("\nExtracted IP Addresses:");
					foreach (string ipAddress in ipAddresses)
					{
						view.Message(ipAddress);
					}

					view.Message("");

					// Remove the old IP address and add the new IP address
					expression = RemoveIPAddressFromExpression(expression, oldIp);
					expression = AddIPAddressToExpression(expression, newIp);

					// Update the firewall rule with the modified expression
					bool updated = await UpdateFirewallRule(client, zoneId, ruleId, expression);
					if (updated)
					{
						view.ChangeColor(ConsoleColor.Green);
						view.Message("Firewall rule updated successfully!");
					}
					else
					{
						view.ChangeColor(ConsoleColor.Red);
						view.Message("Error updating firewall rule.");
					}
                }
				else
				{
					view.ChangeColor(ConsoleColor.Red);
					view.Message($"Error fetching firewall rules. Status code: {getResponse.StatusCode}");
					string responseContent = await getResponse.Content.ReadAsStringAsync();
					view.Message($"Response content: {responseContent}");
				}
			}
		}

		static string ExtractExpressionFromResponse(string responseContent)
		{
			// Parse the JSON response to get the expression
			JObject jsonResponse = JObject.Parse(responseContent);
			string expression = jsonResponse["result"]["filter"]["expression"].ToString();
			return expression;
		}

		static string RemoveIPAddressFromExpression(string expression, string ipAddressToRemove)
		{
			// Remove the specified IP address from the expression
			return expression.Replace(ipAddressToRemove, "");
		}

		static string AddIPAddressToExpression(string expression, string ipAddressToAdd)
		{
			// Add the new IP address to the expression
			return expression + " " + ipAddressToAdd;
		}

		static async Task<bool> UpdateFirewallRule(HttpClient client, string zoneId, string ruleId, string newExpression)
		{
			// Build the JSON payload for the change
			var payload = new
			{
				expression = newExpression
			};

			string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);

			// Send a PATCH request to update the firewall rule with the new expression
			HttpResponseMessage response = await client.PatchAsync($"https://api.cloudflare.com/client/v4/zones/{zoneId}/firewall/rules/{ruleId}", new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

			return response.IsSuccessStatusCode;
		}

		static string[] ExtractIPAddresses(string expression)
		{
			string pattern = @"(\b\d{1,3}(\.\d{1,3}){3}\b|\b[0-9a-fA-F:]+\b)";
			MatchCollection matches = Regex.Matches(expression, pattern);

			List<string> ipAddresses = new List<string>();
			foreach (Match match in matches)
			{
				ipAddresses.Add(match.Value);
			}

			return ipAddresses.ToArray();
		}
	}
}
