using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using ray.chatbot;

internal class Program
{
    private static async global::System.Threading.Tasks.Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        // 設定userId
        string userId = "Ray";

        // 設定OpenAI 
        ChatGPT.GPTinfo gptinfo = new ChatGPT.GPTinfo()
        {
            model = "gpt-4o-mini",
            apiKey = "sk-xxxxxx",
        };

        // 開始對談
        string? userInput, responseMsg;
        Console.Write("User > ");
        while (!string.IsNullOrEmpty(userInput = Console.ReadLine()))
        {
            if (userInput.Contains("/reset"))
            {
                ChatHistoryManager.DeleteIsolatedStorageFile();
                Console.WriteLine("system > " + "遺忘之前的所有對談!");
                
            }
            else
            {
                var chatHistory = ChatHistoryManager.GetMessagesFromIsolatedStorage(userId);

                // Get the response from the AI
                responseMsg = ChatGPT.getResponseFromGPT_useOAI(gptinfo, userInput, chatHistory);

                // Print the results
                Console.WriteLine("Assistant > " + responseMsg);

                // Add user input andthe message from the agent to the chat history
                ChatHistoryManager.SaveMessageToIsolatedStorage(
                    System.DateTime.Now, userId, userInput, responseMsg);
            }

            // debug
            // Console.WriteLine("userInput > " + userInput); 
            // Console.WriteLine("responseMsg > " + responseMsg); 

            // Get user input again
            Console.Write("User > ");
        }
    }
}