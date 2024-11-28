using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace ray.chatbot
{
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum Role
    {
        assistant, user, system
    }


    public class ChatGPT
    {

        /* AOAI參數
        const string AzureOpenAIEndpoint = "https://chatraypt.openai.azure.com";  //👉replace it with your Azure OpenAI Endpoint
        const string AzureOpenAIModelName = "gpt-4o-mini"; //👉repleace it with your Azure OpenAI Model Deploy Name
        const string AzureOpenAIToken = "sk-xxxxxx"; //👉repleace it with your Azure OpenAI API Key
        const string AzureOpenAIVersion = "2024-02-15-preview";  //👉replace  it with your Azure OpenAI API Version
        */

        public class GPTinfo
        {
            // public string apiUrl { get; set; } = $"https://api.openai.com/v1/chat/completions";
            public decimal temperature { get; set; } = 0.5m;
            public string model { get; set; }
            public string apiKey { get; set; }
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
            // Console.WriteLine(responseContent);
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);
            return obj.choices[0].message.content.Value;
        }

        public static string getResponseFromGPT_useOAI(GPTinfo gPTinfo, string Message, List<Message> chatHistory)
        {
            //建立對話紀錄
            var messages = new List<ChatMessage>
                    {
                        new ChatMessage {
                            role = Role.system ,
                            content = @"
                               你是一個請假機器人，員工會向你請假，你必須從員工的請假敘述中找到底下這些請假資訊。
                               找到的資訊必須覆述一次，如果有缺少的資訊，必須提醒員工缺少的部分，直到蒐集完所有資訊後，
                               要跟員工做最後的確認，並且問員工是否正確? 
                               如果員工回答不正確，則要重新蒐集資訊。
                               如果員工說正確，則將蒐集到的資料，整理成一個JSON字串，直接輸出，無須回覆其他文字。
                               
                               購票資訊包含:
                                * 請假起始時間(start_date_time)
                                * 請假結束時間(end_date_time)
                                * 假別(type)     
                                * 請假原因(reason)
                                * 代理人(agent)
                                
                               假別包含:
                                * 事假
                                * 病假
                                * 特休
                                * 公假
                                * 補休

                                回應時，請注意以下幾點:
                                * 不要說 '感謝你的來信' 之類的話，因為客戶是從對談視窗輸入訊息的，不是寫信來的
                                * 不要一直說抱歉或對不起，但可以說不好意思。
                                * 要能夠盡量解決員工的問題。
                                * 不要以回覆信件的格式書寫，請直接提供對談機器人可以直接給客戶的回覆
                                ----------------------
"
                        }
                    };

            //添加歷史對話紀錄
            foreach (var HistoryMessageItem in chatHistory)
            {
                //添加一組對話紀錄
                messages.Add(new ChatMessage()
                {
                    role = Role.user,
                    content = HistoryMessageItem.UserMessage
                });
                messages.Add(new ChatMessage()
                {
                    role = Role.assistant,
                    content = HistoryMessageItem.ResponseMessage
                });
            }
            messages.Add(new ChatMessage()
            {
                role = Role.user,
                content = Message
            });
            //回傳呼叫結果
            return ChatGPT.CallOpenAIChatAPI(gPTinfo.apiKey,
                new
                {
                    model = gPTinfo.model,
                    temperature = gPTinfo.temperature,
                    messages = messages
                }
            );
        }
    }

    public class ChatMessage
    {
        public Role role { get; set; }
        public string content { get; set; }
    }

}