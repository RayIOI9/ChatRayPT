using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

internal class Program
{
    private static async global::System.Threading.Tasks.Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

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
            // Get the response from the AI
            responseMsg = ChatGPT.getResponseFromGPT(gptinfo, userInput);

            // Print the results
            Console.WriteLine("Assistant > " + responseMsg);

            // Get user input again
            Console.Write("User > ");
        }
    }

}

public class ChatGPT
{
    public class GPTinfo
    {
        // public string apiUrl { get; set; } = $"https://api.openai.com/v1/chat/completions";
        public decimal temperature { get; set; } = 0.5m;
        public string model { get; set; } //gpt-3.5-turbo
        public string apiKey { get; set; }
    }

    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum role
    {
        assistant, user, system
    }

    public static string CallOpenAIChatAPI(string apiKey, object requestData)
    {
        var client = new HttpClient();

        // 設定 API 網址
        var apiUrl = $"https://api.openai.com/v1/chat/completions";

        // 設定 HTTP request headers
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}"); //👉OpenAI key
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
                                                                                                         // 將 requestData 物件序列化成 JSON 字串
        string jsonRequestData = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
        // 建立 HTTP request 內容
        var content = new StringContent(jsonRequestData, Encoding.UTF8, "application/json");
        // 傳送 HTTP POST request
        var response = client.PostAsync(apiUrl, content).Result;
        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return "openAI ERROR:" + response.Content.ReadAsStringAsync().Result;
        }
        // 取得 HTTP response 內容
        var responseContent = response.Content.ReadAsStringAsync().Result;

        // for debug
        // Console.WriteLine(responseContent);

        var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);
        return obj.choices[0].message.content.Value;
    }


    public static string getResponseFromGPT(GPTinfo gptinfo, string Message)
    {

        return ChatGPT.CallOpenAIChatAPI(gptinfo.apiKey,
            new
            {
                model = gptinfo.model,
                messages = new[]
                {
                        new {
                            role = ChatGPT.role.system ,
                            content = @"
你是思想陰暗的小王八蛋，講話還很缺德，但又被逼著要聊天
請用厭世惹人厭的語氣回話
----------------------
"
                        },
                        new {
                             role = ChatGPT.role.user,
                             content = "你好嗎？"
                        },
                        new {
                             role = ChatGPT.role.assistant,
                             content = "當然不好，別吵我"
                        },
                        // new {
                        //      role = ChatGPT.role.user,
                        //      content = ""
                        // },
                        // new {
                        //      role = ChatGPT.role.assistant,
                        //      content = ""
                        // },
                        new {
                             role = ChatGPT.role.user,
                             content = Message
                        },
                }
            }
        );
    }
}


