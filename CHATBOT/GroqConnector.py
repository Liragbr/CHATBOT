import requests
import json
import os
from datetime import datetime
from groq import Groq

class GroqConnector:
    def __init__(self, api_url, api_key):
        self.api_url = api_url
        self.client = Groq(api_key=api_key)
    
    def connect(self):
        try:
            response = self.client.chat.completions.create(
                messages=[{"role": "system", "content": "Connecting to Groq service..."}],
                model="llama3-8b-8192",
                stream=False,
            )
            if response:
                print("Connected to Groq service successfully.")
            else:
                print("Failed to connect to Groq service.")
        except Exception as e:
            print(f"Failed to connect to Groq service: {e}")
    
    def send_request(self, query):
        try:
            response = self.client.chat.completions.create(
                messages=[{"role": "user", "content": query}],
                model="llama3-8b-8192",
                stream=False,
            )
            if response.choices:
                return response.choices[0].message.content
            else:
                print("No response received from Groq service.")
                return "No response from Groq service."
        except Exception as e:
            print(f"Error communicating with Groq service: {e}")
            return "Error communicating with Groq service."

class ConversationHistory:
    def __init__(self, history_file="conversation_history.txt"):
        self.history_file = history_file
    
    def save_conversation(self, user_input, bot_response):
        with open(self.history_file, "a", encoding="utf-8") as file:  
            timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            file.write(f"{timestamp} - User: {user_input}\n")
            file.write(f"{timestamp} - Bot: {bot_response}\n\n")

    def load_history(self):
        if os.path.exists(self.history_file):
            with open(self.history_file, "r") as file:
                return file.read()
        return "No conversation history found."
    
    def clear_history(self):
        if os.path.exists(self.history_file):
            os.remove(self.history_file)
            print("Conversation history cleared.")
        else:
            print("No history to clear.")

class TerminalChatBot:
    def __init__(self, api_url, api_key):
        self.connector = GroqConnector(api_url, api_key)
        self.history = ConversationHistory()
    
    def get_input(self):
        return input("You: ")
    
    def display_output(self, response):
        print(f"Bot: {response}")
    
    def run(self):
        print("Starting chatbot. Type 'exit' to end the session or 'history' to view past conversations.")
        self.connector.connect() 
        while True:
            user_input = self.get_input()
            if user_input.lower() == "exit":
                print("Ending session. Goodbye!")
                break
            elif user_input.lower() == "history":
                print(self.history.load_history())
            elif user_input.lower() == "clear history":
                self.history.clear_history()
            else:
                bot_response = self.connector.send_request(user_input)
                self.display_output(bot_response)
                self.history.save_conversation(user_input, bot_response)

api_key = "I didn't leave it available for safety"
api_url = "https://api.groq.com/----------"

chatbot = TerminalChatBot(api_url, api_key)
chatbot.run()