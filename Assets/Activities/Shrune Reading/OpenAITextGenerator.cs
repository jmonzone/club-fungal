using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[CreateAssetMenu(fileName = "OpenAITextGenerator", menuName = "Club Fungal/Activities/Shrune Reading/OpenAI Text Generator")]
public class OpenAITextGenerator : ScriptableObject
{
    [SerializeField] private string openAiApiKey;

    private List<object> messageHistory = new List<object>();

    public IEnumerator GenerateText(string prompt, System.Action<string> callback)
    {
        return CallOpenAI(prompt, callback);
    }

    public void ClearConversationHistory()
    {
        messageHistory.Clear();
    }

    private IEnumerator CallOpenAI(string prompt, System.Action<string> callback)
    {
        if (string.IsNullOrEmpty(openAiApiKey))
        {
            callback("The spores whisper secrets of the unseen.");
            yield break;
        }

        bool shouldRhyme = Random.value < 0.2f;
        string systemContent = "A wanderer would like you to read their Shrunes. Your responses should be short, modern, and absurd. Keep responses concise.";
        if (shouldRhyme)
        {
            systemContent += " Your responses should rhyme.";
        }

        var messages = new List<object>();
        messages.Add(new { role = "system", content = systemContent });
        messages.AddRange(messageHistory);
        messages.Add(new { role = "user", content = prompt });

        var requestData = new
        {
            model = "gpt-3.5-turbo",
            messages = messages.ToArray(),
            max_tokens = 100,
            temperature = 0.7f
        }; string json = JsonConvert.SerializeObject(requestData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {openAiApiKey}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JObject.Parse(request.downloadHandler.text);
                string generatedText = response["choices"][0]["message"]["content"].ToString();
                callback(generatedText.Trim());
                messageHistory.Add(new { role = "user", content = prompt });
                messageHistory.Add(new { role = "assistant", content = generatedText.Trim() });
            }
            else
            {
                Debug.LogError($"OpenAI API Error: {request.error}");
                callback("The mycelium holds mysteries yet to unfold.");
            }
        }
    }
}