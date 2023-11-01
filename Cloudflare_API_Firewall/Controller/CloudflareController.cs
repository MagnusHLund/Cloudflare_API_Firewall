using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Configuration;
using Cloudflare_API_Firewall.Model;
using System.Text;
using System.Linq.Expressions;

namespace Cloudflare_API_Firewall.Controller
{
	internal class CloudflareController
	{
		View.View view = new View.View();

		internal async Task EditFirewall(string oldIp, string newIp)
		{
			newIp = "11.11.11.11";

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
					view.ChangeColor(ConsoleColor.Blue);
					foreach (string ipAddress in ipAddresses)
					{
						view.Message(ipAddress);
					}

					view.Message("");

					// Remove the old IP address and add the new IP address
					expression = RemoveIPAddressFromExpression(expression, oldIp);
					expression = AddIPAddressToExpression(expression, newIp);

					// Update the firewall rule with the modified expression
					string updatedResponse = await UpdateFirewallRule(client, zoneId, ruleId, expression);
					if (updatedResponse.StartsWith("Error"))
					{
						view.ChangeColor(ConsoleColor.Red);
						view.Message(updatedResponse);
					}
					else
					{
						view.ChangeColor(ConsoleColor.Green);
						view.Message("Firewall rule updated successfully!");
						// You can parse and process the updated response as needed
					}

					
					HttpResponseMessage getResponse2 = await client.GetAsync($"https://api.cloudflare.com/client/v4/zones/{zoneId}/firewall/rules/{ruleId}");
					if (getResponse.IsSuccessStatusCode)
					{
						string responseContent2 = await getResponse2.Content.ReadAsStringAsync();

						string expression2 = ExtractExpressionFromResponse(responseContent2);

						string[] ipAddresses2 = ExtractIPAddresses(expression2);

						// Display the extracted IP addresses
						view.ChangeColor(ConsoleColor.Green);
						view.Message("\nExtracted IP Addresses:");
						view.ChangeColor(ConsoleColor.Blue);
						foreach (string ipAddress in ipAddresses2)
						{
							view.Message(ipAddress);
						}
						view.ChangeColor(ConsoleColor.Green);
						view.Message("");
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
			// Check if the expression already contains the new IP address
			if (expression.Contains(ipAddressToAdd))
			{
				return expression; // IP address is already in the expression
			}
			else
			{
				// Split the expression to get the part inside the curly braces
				int startIdx = expression.IndexOf("{");
				int endIdx = expression.LastIndexOf("}");
				if (startIdx >= 0 && endIdx >= 0)
				{
					string insideBraces = expression.Substring(startIdx + 1, endIdx - startIdx - 1);
					// Add the new IP address to the existing ones inside the braces
					insideBraces = string.Join(" ", insideBraces, ipAddressToAdd);
					// Reconstruct the expression with the updated IP addresses inside the braces
					return expression.Substring(0, startIdx + 1) + insideBraces + expression.Substring(endIdx);
				}
				else
				{
					// If the expression doesn't contain curly braces, just add the new IP address in curly braces
					return $"({expression} {ipAddressToAdd})";
				}
			}
		}



		internal async Task<string> UpdateFirewallRule(HttpClient client, string zoneId, string ruleId, string newExpression)
		{
			string filterId = ConfigurationManager.AppSettings["FilterId"];

			Console.WriteLine(newExpression);

			var updatePayload = new
			{
				id = ruleId,
				paused = false,
				description = "test",
				action = "block",
				filter = new
				{
					id = filterId,
					expression = newExpression,
					paused = false
				}
			};

			string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(updatePayload);
			HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
			string test = await content.ReadAsStringAsync();

			HttpResponseMessage response = await client.PutAsync($"https://api.cloudflare.com/client/v4/zones/{zoneId}/firewall/rules/{ruleId}", content);

			if (response.IsSuccessStatusCode)
			{
				string responseContent = await response.Content.ReadAsStringAsync();
				return responseContent;
			}
			else
			{
				// Handle the error case as needed
				return null;
			}
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
