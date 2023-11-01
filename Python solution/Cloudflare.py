import os
import requests
import json
import re
import time 
from configparser import ConfigParser

class MainController:
    def __init__(self):
        self.view = View()
        self.file_saving = FileHandling()

    def start(self):
        try:
            self.view.message("Checking if file is created")

            if self.file_saving.create_file():
                self.view.change_color("Blue")
                self.view.message("Save file was already created")
            else:
                self.view.change_color("Green")
                self.view.message("Save file did not exist. Was created.")

            self.view.change_color("White")
            self.view.message("Reading save file...")
            old_ip = self.file_saving.read_ip_address().strip()

            self.view.change_color("Blue")
            self.view.message("Previous IP Address retrieved")

            self.view.change_color("White")
            new_ip = self.get_public_ip_address()

            self.view.change_color("White")
            self.view.message("Connecting to Cloudflare...")
            cloudflare = CloudflareController()
            cloudflare.edit_firewall(old_ip, new_ip)

        except Exception as ex:
            self.view.change_color("Red")
            print(f"{ex}\nClosing program in 5 seconds...")
            time.sleep(5)
            exit(69)  # Nice

    def get_public_ip_address(self):
        self.view.message("Fetching current public IP address...")
        url = "https://icanhazip.com"
        response = requests.get(url)
        public_ip_address = response.text.strip()

        self.view.change_color("Green")
        self.view.message(f"Your public IP address is: {public_ip_address}")

        self.view.change_color("White")
        self.view.message("Saving file...")

        self.view.change_color("Blue")
        self.view.message(f"File saved at: {self.file_saving.save_ip_address(public_ip_address)}")

        return public_ip_address

class View:
    def message(self, message):
        print(message)

    def change_color(self, color):
        # Implement the color change logic here
        pass

class FileHandling:
    def __init__(self):
        self.path = os.path.join(os.path.expanduser("~"), "MagnusLund", "CloudFlareFirewall")
        self.file_path = os.path.join(self.path, "ip.txt")
        self.view = View()

    def create_file(self):
        if not os.path.exists(self.file_path):
            os.makedirs(self.path)
            open(self.file_path, "w").close()
            return False
        return True

    def save_ip_address(self, ip):
        with open(self.file_path, "w") as file:
            file.write(ip)
        return self.file_path

    def read_ip_address(self):
        with open(self.file_path, "r") as file:
            return file.read()

class CloudflareController:
    def __init__(self):
        self.view = View()
        self.config = ConfigParser()
        self.config.read("config.ini")

    def edit_firewall(self, old_ip, new_ip):

        new_ip = "11.11.11.11"

        api_token = self.config.get("api", "ApiToken")
        zone_id = self.config.get("api", "ZoneId")
        rule_id = self.config.get("api", "RuleId")

        headers = {
            "Authorization": f"Bearer {api_token}"
        }

        get_response = requests.get(f"https://api.cloudflare.com/client/v4/zones/{zone_id}/firewall/rules/{rule_id}", headers=headers)

        if get_response.status_code == 200:
            self.view.change_color("Green")
            self.view.message("Connection established!")
            self.view.change_color("White")
            self.view.message("Fetching IP addresses...")

            response_content = get_response.text
            expression = self.extract_expression_from_response(response_content)

            ip_addresses = self.extract_ip_addresses(expression)

            self.view.change_color("Green")
            self.view.message("\nExtracted IP Addresses:")
            self.view.change_color("Blue")
            for ip_address in ip_addresses:
                self.view.message(ip_address)

            self.view.message("")

            expression = self.remove_ip_address_from_expression(expression, old_ip)
            expression = self.add_ip_address_to_expression(expression, new_ip)

            updated_response = self.update_firewall_rule(zone_id, rule_id, expression)
            if updated_response.startswith("Error"):
                self.view.change_color("Red")
                self.view.message(updated_response)
            else:
                self.view.change_color("Green")
                self.view.message("Firewall rule updated successfully")

        else:
            self.view.change_color("Red")
            self.view.message(f"Error fetching firewall rules. Status code: {get_response.status_code}")
            self.view.message(f"Response content: {get_response.text}")

    def extract_expression_from_response(self, response_content):
        response_json = json.loads(response_content)
        expression = response_json["result"]["filter"]["expression"]
        return expression

    def remove_ip_address_from_expression(self, expression, ip_address):
        return expression.replace(ip_address, "")

    def add_ip_address_to_expression(self, expression, ip_address):
        if ip_address in expression:
            return expression

        start_idx = expression.find("{")
        end_idx = expression.rfind("}")

        if start_idx >= 0 and end_idx >= 0:
            inside_braces = expression[start_idx + 1:end_idx]
            inside_braces = " ".join([inside_braces, ip_address])
            return expression[:start_idx + 1] + inside_braces + expression[end_idx:]
        else:
            return f"({expression} {ip_address})"

    def update_firewall_rule(self, zone_id, rule_id, new_expression):
        filter_id = self.config.get("api", "FilterId")

        new_expression = new_expression.strip()

        # Convert the expression to a character array
        char_array = list(new_expression)

        # Iterate through the characters to remove extra spaces
        i = 0
        while i < len(char_array) - 1:
            if char_array[i] == ' ' and char_array[i + 1] == ' ':
                del char_array[i]
            else:
                i += 1

        # Reconstruct the expression
        new_expression = ''.join(char_array)

        payload = {
            "id": rule_id,
            "paused": False,
            "description": "doesnt work",
            "action": "block",
            "priority": 1,
            "filter": {
                "id": filter_id,
                "expression": '(http.host eq "data.magnuslund.com" and not ip.src in {2a01:599:122:1b72:c40:8643:b00f:eddd 2a02:5c21:a43b::f88a 11.11.11.11})',
                "paused": False,
                "description": "not /api"
            }
        }

        headers = {
            "Authorization": f"Bearer {self.config.get('api', 'ApiToken')}",
            "Content-Type": "application/json"
        }

        update_response = requests.put(f"https://api.cloudflare.com/client/v4/zones/{zone_id}/firewall/rules/{rule_id}",
                                    data=json.dumps(payload),
                                    headers=headers)

        if update_response.status_code == 200:
            return update_response.text
        else:
            # Handle the error case as needed
            return f"Error: {update_response.status_code}"


    def extract_ip_addresses(self, expression):
        pattern = r"(\b\d{1,3}(\.\d{1,3}){3}\b|\b[0-9a-fA-F:]+\b)"
        ip_addresses = re.findall(pattern, expression)
        return [ip[0] for ip in ip_addresses]

if __name__ == "__main__":
    controller = MainController()
    controller.start()
