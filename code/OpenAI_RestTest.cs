using OpenAiFormet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.Networking.UnityWebRequest;

public class OpenAI_RestTest : MonoBehaviour
{

    public  string KeyCode ="sk-";
    OpenAI_Rest api;
    AudioSource source;
    SpriteRenderer spriteRenderer;
    public async void test()
    {

        source = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        api = new OpenAI_Rest();
        api.OpenAI_Key = KeyCode;
        api.Init();


        //chat 
        ChatRequest chatRequest = new ChatRequest();


        chatRequest.Messages = new List<ChatMessage>
        {
            new ChatMessage() { Role = role.system, Message = "고양이 말투로말한다" },
            new ChatMessage() { Role = role.user, Message = "반갑다" },
            new ChatMessage() { Role = role.assistant, Message = "반갑다냥" },
            new ChatMessage() { Role = role.user, Message = "너의 이름은  Gpt고양이야" },
            new ChatMessage() { Role = role.system, Message = "맞아 내이름은 Gpt고양이야!" },
            new ChatMessage() { Role = role.user, Message = "이름이 뭐라고?" },
        };
        var data = ((await (api.ClieantResponseChat(chatRequest))).Choices);
        foreach (var choice in data)
        {
            Debug.Log(choice.Message.Message);
        }





        //tts
        TextToSpeechRequest textToSpeech = new TextToSpeechRequest() { Input = "테스트용 데이터입니다" };
        api.speechFile = Application.streamingAssetsPath + "/aaa.mp3";
        if (await (api.ClieantResponseTTS(textToSpeech)))
        {
            try
            {
                Ienumrator = LoadPath();
                StartCoroutine(Ienumrator);
                source.Play();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

        }




        //text to image
        ImageGenerateRequest imageGenerateResponseb64Json = new ImageGenerateRequest() { Prompt = "cat in the box,cartoon drawing style" };
        var imagedata  = await (api.ClieantResponseimageGeneratb64(imageGenerateResponseb64Json));

        //byte를 sprite로 변경
        byte[] imageBytes = Convert.FromBase64String(imagedata.Data[0].b64_json);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imageBytes);
        Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        spriteRenderer.sprite = sprite;
    }

    IEnumerator Ienumrator;
    IEnumerator LoadPath()
    {
        WWW www = new WWW(api.speechFile);
        yield return www;
        source.clip = www.GetAudioClip();
        source.Play();
        StopCoroutine(Ienumrator);
    }

    void Start()
    {
        test();
    }
}
