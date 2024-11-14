using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class GroqConnector
{
    private readonly string apiUrl;
    private readonly HttpClient client;

    public GroqConnector(string apiUrl, string apiKey)
    {
        this.apiUrl = apiUrl;
        client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task ConnectAsync()
    {
        try
        {
            var response = await client.PostAsync($"{apiUrl}/connect", null);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Connected to Groq service successfully.");
            }
            else
            {
                Console.WriteLine("Failed to connect to Groq service.");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to connect to Groq service: {e.Message}");
        }
    }

    public async Task<string> SendRequestAsync(string query)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(new { content = query }), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync(apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GroqResponse>(responseBody);
                return result?.Choices[0].Message.Content ?? "No response from Groq service.";
            }
            else
            {
                return "Failed to get response from Groq service.";
            }
        }
        catch (Exception e)
        {
            return $"Error communicating with Groq service: {e.Message}";
        }
    }
}

public class GroqResponse
{
    public Choice[] Choices { get; set; }

    public class Choice
    {
        public Message Message { get; set; }
    }

    public class Message
    {
        public string Content { get; set; }
    }
}

public class ConversationHistory
{
    private readonly string historyFile;

    public ConversationHistory(string historyFile = "conversation_history.txt")
    {
        this.historyFile = historyFile;
    }

    public void SaveConversation(string userInput, string botResponse)
    {
        using (StreamWriter file = new StreamWriter(historyFile, true))
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            file.WriteLine($"{timestamp} - User: {userInput}");
            file.WriteLine($"{timestamp} - Bot: {botResponse}");
            file.WriteLine();
        }
    }

    public string LoadHistory()
    {
        if (File.Exists(historyFile))
        {
            return File.ReadAllText(historyFile);
        }
        return "No conversation history found.";
    }

    public void ClearHistory()
    {
        if (File.Exists(historyFile))
        {
            File.Delete(historyFile);
            Console.WriteLine("Conversation history cleared.");
        }
        else
        {
            Console.WriteLine("No history to clear.");
        }
    }
}

public class TerminalChatBot
{
    private readonly GroqConnector connector;
    private readonly ConversationHistory history;

    public TerminalChatBot(string apiUrl, string apiKey)
    {
        connector = new GroqConnector(apiUrl, apiKey);
        history = new ConversationHistory();
    }

    private string GetInput()
    {
        Console.Write("You: ");
        return Console.ReadLine();
    }

    private void DisplayOutput(string response)
    {
        Console.WriteLine($"Bot: {response}");
    }

    public async Task RunAsync()
    {
        Console.WriteLine("Starting chatbot. Type 'exit' to end the session or 'history' to view past conversations.");
        await connector.ConnectAsync();
        while (true)
        {
            string userInput = GetInput();
            if (userInput.ToLower() == "exit")
            {
                Console.WriteLine("Ending session. Goodbye!");
                break;
            }
            else if (userInput.ToLower() == "history")
            {
                Console.WriteLine(history.LoadHistory());
            }
            else if (userInput.ToLower() == "clear history")
            {
                history.ClearHistory();
            }
            else
            {
                string botResponse = await connector.SendRequestAsync(userInput);
                DisplayOutput(botResponse);
                history.SaveConversation(userInput, botResponse);
            }
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        string apiKey = "I didn't leave it available for safety";
        string apiUrl = "https://api.groq.com/----------";

        TerminalChatBot chatbot = new TerminalChatBot(apiUrl, apiKey);
        await chatbot.RunAsync();
    }
}
