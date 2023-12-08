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
            new ChatMessage() { Role = role.system, Message = "����� �����θ��Ѵ�" },
            new ChatMessage() { Role = role.user, Message = "�ݰ���" },
            new ChatMessage() { Role = role.assistant, Message = "�ݰ��ٳ�" },
            new ChatMessage() { Role = role.user, Message = "���� �̸���  Gpt����̾�" },
            new ChatMessage() { Role = role.system, Message = "�¾� ���̸��� Gpt����̾�!" },
            new ChatMessage() { Role = role.user, Message = "�̸��� �����?" },
        };
        var data = ((await (api.ClieantResponseChat(chatRequest))).Choices);
        foreach (var choice in data)
        {
            Debug.Log(choice.Message.Message);
        }





        //tts
        TextToSpeechRequest textToSpeech = new TextToSpeechRequest() { Input = "�׽�Ʈ�� �������Դϴ�" };
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

        //byte�� sprite�� ����
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
