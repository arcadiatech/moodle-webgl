using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Net.Http;

public class Utils
{
    public static string CleanJSON(string json)
    {
        string cleanedJson = json.Replace("\\", "");
        if (cleanedJson.StartsWith("\"") && cleanedJson.EndsWith("\""))
        {
            cleanedJson = cleanedJson.Substring(1, cleanedJson.Length - 2);
        }
        return cleanedJson;
    }

    public static T DecodeJWT<T>(string token, string secret)
    {
        try
        {
            string jsonPayload = JWT.JsonWebToken.Decode(token, secret);
            Debug.Log("String payload: " + jsonPayload);
            T payload = JsonUtility.FromJson<T>(CleanJSON(jsonPayload));
            return payload;
        }
        catch (JWT.SignatureVerificationException)
        {
            Debug.Log("Invalid token!");
            return default(T);
        }
    }

    public static async Task<string> GETRequest(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Send the GET request asynchronously
            var operation = webRequest.SendWebRequest();

            // Create a TaskCompletionSource to await the operation
            var tcs = new TaskCompletionSource<string>();

            // Set up a callback for when the operation is complete
            operation.completed += (asyncOperation) =>
            {
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    tcs.SetResult(webRequest.downloadHandler.text);
                }
                else
                {
                    tcs.SetException(new System.Exception(webRequest.error));
                }
            };

            // Return the task associated with the TaskCompletionSource
            return await tcs.Task;
        }
    }

    public static async Task<Texture2D> DownloadImage(string url)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            // Download the image data asynchronously
            byte[] imageData = await httpClient.GetByteArrayAsync(url);

            // Create a new texture and load the image data into it
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);

            return texture;
        }
    }
}
