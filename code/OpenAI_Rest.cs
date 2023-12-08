using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenAiFormet;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEditor.PackageManager.Requests;
using UnityEngine;


public class OpenAI_Rest
{
    public string OpenAI_Key = "sk-key...";

    private static HttpClient client;
    private static string JsonFilelocation = Application.streamingAssetsPath + "/Json/JsonData.json";

    public delegate void StringEvent(string _string);
    public StringEvent CompletedRepostEvent;

    private string API_Url = "";

    private const string AuthorizationHeader = "Bearer";
    private const string UserAgentHeader = "User-Agent";

    public string speechFile = Application.streamingAssetsPath + "/TestSound.mp3";
    public void Init()
    {
        CreateHttpClient();
    }
    private void CreateHttpClient()
    {
        client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(AuthorizationHeader, OpenAI_Key);
        client.DefaultRequestHeaders.Add(UserAgentHeader, "okgodoit/dotnet_openai_api");
    }

    private async Task<string> ClieantResponse<SendData>(SendData request)
    {
        if (client == null)
        {
            CreateHttpClient();
        }

        API_Url = ((URL)request).Get_API_Url();

        string jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
        var stringContent = new StringContent(jsonContent, UnicodeEncoding.UTF8, "application/json");

        Debug.Log(API_Url);
        Debug.Log(stringContent);
        using (var response = await client.PostAsync(API_Url, stringContent))
        {

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new HttpRequestException("Error calling OpenAi API to get completion.  HTTP status code: " + response.StatusCode.ToString() + ". Request body: " + jsonContent);
            }
        }
    }
    public async Task<bool> ClieantResponseTTS(TextToSpeechRequest textToSpeechRequest)
    {

        bool result = false;
        try
        {
            API_Url = ((URL)textToSpeechRequest).Get_API_Url();

            string jsonContent = JsonConvert.SerializeObject(textToSpeechRequest, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            var stringContent = new StringContent(jsonContent, UnicodeEncoding.UTF8, "application/json");


            using (HttpResponseMessage response = await client.PostAsync(API_Url, stringContent))
            {
                if (response.IsSuccessStatusCode)
                {

                    byte[] mp3Bytes = await response.Content.ReadAsByteArrayAsync();

                    try
                    {
                        using (FileStream fs = File.OpenWrite(speechFile))
                        {

                            await fs.WriteAsync(mp3Bytes, 0, mp3Bytes.Length);
                            Debug.Log($"Speech file saved to {speechFile}");
                        }
                        result = true;
                    }
                    catch (MissingReferenceException ex)
                    {
                        File.Create(speechFile).Close();
                        Debug.LogError(ex);
                        using (FileStream fs = File.OpenWrite(speechFile))
                        {
                            await fs.WriteAsync(mp3Bytes, 0, mp3Bytes.Length);
                            Debug.Log($"Speech file saved to {speechFile}");
                        }
                        result = true;
                    }

                }
                else
                {
                    Debug.LogError($"Error: {response.StatusCode} - {response.ReasonPhrase}");

                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }

        return result;
    }
    public async Task<ChatResponse> ClieantResponseChat(ChatRequest r)
    {
        return JsonConvert.DeserializeObject<ChatResponse>(await ClieantResponse(r));
    }
    public async Task<ImageGenerateResponseURL> ClieantResponseimageGenerat(ImageGenerateRequest r)
    {
        return JsonConvert.DeserializeObject<ImageGenerateResponseURL>(await ClieantResponse(r));
    }

    public async Task<ImageGenerateResponseb64json> ClieantResponseimageGeneratb64(ImageGenerateRequest r)
    {
        return JsonConvert.DeserializeObject<ImageGenerateResponseb64json>(await ClieantResponse(r));
    }

    public async Task<ChatResponse> ClieantResponseChatAnalyze(ChatMessageAnalyze r)
    {
        return JsonConvert.DeserializeObject<ChatResponse>(await ClieantResponse(r));
    }


}



namespace OpenAiFormet
{
    interface URL
    {
        public string Get_API_Url();
    }

    #region Chat
    public class ChatRequest : URL
    {
        public const string API_Url = "https://api.openai.com/v1/chat/completions";

        [JsonProperty("model")]
        public string Model { get; set; } = "gpt-3.5-turbo";

        [JsonProperty("messages")]
        public List<ChatMessage> Messages { get; set; }

        public string Get_API_Url()
        {
            return API_Url;
        }
    }
    public enum role
    {
        [EnumMember(Value = "system")]
        system,
        [EnumMember(Value = "user")]
        user,
        [EnumMember(Value = "assistant")]
        assistant
    }

    public class ChatMessage
    {
        [JsonProperty("role"), JsonConverter(typeof(StringEnumConverter)), XmlAttribute("role")]
        public role Role;
        [JsonProperty("content"), XmlAttribute("content")]
        public string Message = "";
    }

    public class ChatResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("created")]
        public int Created { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("system_fingerprint")]
        public string SystemFingerprint { get; set; }

        [JsonProperty("choices")]
        public List<ChatChoice> Choices { get; set; }

        [JsonProperty("usage")]
        public ChatUsage Usage { get; set; }
    }
    public class ChatChoice
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("message")]
        public ChatMessage Message { get; set; }

        [JsonProperty("finish_reason")]
        public string FinishReason { get; set; }
    }
    public class ChatUsage
    {
        [JsonProperty("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonProperty("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonProperty("total_tokens")]
        public int TotalTokens { get; set; }
    }



    public class ImageAnalyzeRequest : URL
    {

        private const string API_Url = "https://api.openai.com/v1/chat/completions";
        [JsonProperty("model")]

        public const string model = "gpt-4-vision-preview";
        [JsonProperty("messages")]
        public List<ChatMessageAnalyze> Messages { get; set; }

        public string Get_API_Url()
        {
            return API_Url;
        }
    }

    public class ChatMessageAnalyze
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        public ContentAnalyze Content { get; set; }
    }

    public class ContentAnalyze
    {

        [JsonProperty("text")]
        public string text { get; set; }

        [JsonProperty("image_url")]
        public string image_url { get; set; }
    }


    #endregion

    #region imageGenerate
    public class ImageGenerateRequest : URL
    {
        private const string API_Url = "https://api.openai.com/v1/images/generations";

        [JsonProperty("prompt")]
        public string Prompt { get; set; } // Required

        [JsonProperty("model")]
        public string Model { get; set; } = "dall-e-2"; // Optional, defaults to dall-e-2

        [JsonProperty("n")]
        public int N { get; set; } = 1; // Optional, defaults to 1

        [JsonProperty("quality")]
        public string Quality { get; set; } = "standard"; // Optional, defaults to standard

        [JsonProperty("response_format")]
        public string ResponseFormat { get; set; } = "b64_json"; // Optional, defaults to url or  b64_json

        [JsonProperty("size")]
        public string Size { get; set; } = "1024x1024"; // Optional, defaults to 1024x1024

        [JsonProperty("style")]
        public string Style { get; set; } = "vivid"; // Optional, defaults to vivid or natural

        [JsonProperty("user")]
        public string User { get; set; } // Optional

        public string Get_API_Url()
        {
            return API_Url;
        }
    }


    public class ImageGenerateResponseURL
    {
        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("data")]
        public List<ImageData> Data { get; set; }
    }

    public class ImageData
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
    public class ImageGenerateResponseb64json
    {
        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("data")]
        public List<ImageDatab64json> Data { get; set; }
    }

    public class ImageDatab64json
    {
        [JsonProperty("b64_json")]
        public string b64_json { get; set; }
    }
    #endregion

    #region TTS
    public class TextToSpeechRequest : URL
    {
        public const string API_Url = "https://api.openai.com/v1/audio/speech";

        [JsonProperty("model")]
        public string Model { get; set; } = "tts-1";

        [JsonProperty("voice")]
        public string Voice { get; set; } = "alloy";

        [JsonProperty("input")]
        public string Input { get; set; }
        public string Get_API_Url()
        {
            return API_Url;
        }
    }
    #endregion


}
//public class ImageEditRequest
//{
//    public const string API_Url_ImageEdits = "https://api.openai.com/v1/images/edits";

//    //[JsonProperty("image")]
//    //public IFormFile Image { get; set; }

//    //[JsonProperty("mask")]
//    //public IFormFile Mask { get; set; }

//    [JsonProperty("model")]
//    public string Model { get; set; } = "dall-e-2";

//    [JsonProperty("prompt")]
//    public string Prompt { get; set; }

//    [JsonProperty("n")]
//    public int N { get; set; } = 1;

//    [JsonProperty("size")]
//    public string Size { get; set; } = "1024x1024";
//}
//public class ImageEditResponse
//{

//}

//public class ImagevariationsRequest
//{
//    private const string API_Url_Image = "https://api.openai.com/v1/images/variations";
//    public const string model = "";
//}



////public struct ImagevariationsResponse : IA
////{
////    //public string mdoel { get mdoel; set => mdoel; }
////}
//public interface IA
//{
//    public string mdoel { get; set; }
//}


