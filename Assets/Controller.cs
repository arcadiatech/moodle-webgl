using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class Controller : MonoBehaviour
{
    public TMP_Text userText;
    public TMP_Text displayText;
    public TMP_Text timeDisplay;
    public GameObject profilePanel;
    public TMP_Text profileText;
    public Image profileImage;
    public int count = 0;
    public float gameDuration = 10;
    private float time;
    private bool isPlaying = false;
    private const string apiToken = "cb537ee57138f698e1106678b965edcc";
    private string userId = "-1";

    void Start()
    {
        profilePanel.SetActive(false);

        time = gameDuration;
        ChangeTime(time);
        updateText();

        // Get the URL of the current page
        string url = Application.absoluteURL;
        Debug.Log(url);
        // Parse the URL to extract query parameters
        string[] parts = url.Split('?');
        Dictionary<string, string> Params = new Dictionary<string, string>();

        if (parts.Length > 1)
        {
            string queryString = parts[1]; // Extract the query string part
            string[] queryParams = queryString.Split('&'); // Split query string into key-value pairs

            // Process each key-value pair
            foreach (string param in queryParams)
            {
                string[] keyValue = param.Split('='); // Split key-value pair
                string key = keyValue[0];
                string value = keyValue.Length > 1 ? keyValue[1] : ""; // Handle case where value is missing
                Params[key] = value;
            }
        }

        var token = Params.ContainsKey("token") ? Params["token"] : null;

        if (token != null)
        {
            var secretKey = "your_secret_key";
            JWTPayload payload = Utils.DecodeJWT<JWTPayload>(token, secretKey);
            userId = payload.id;
            Debug.Log("Id: " + payload.id);
            Debug.Log("Username: " + payload.username);
            userText.text = payload.username;

            getUserProfile("2", payload.id);
        }
    }

    private void Update()
    {
        if (isPlaying)
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                time = 0;
                isPlaying = false;
                sendScore(count, userId);
            }
            ChangeTime(time);
        }
    }

    async Task getUserProfile(string courseId, string userId)
    {
        string baseUrl = "http://localhost/moodle/webservice/rest/server.php?wstoken={2}&wsfunction=core_user_get_course_user_profiles&moodlewsrestformat=json&userlist[0][userid]={1}&userlist[0][courseid]={0}";
        string moodleUrl = string.Format(baseUrl, courseId, userId, apiToken);
        
        string response = await Utils.GETRequest(moodleUrl);
        if (response == null)
        {
            Debug.LogError("Null response from Moodle");
            return;
        }

        UserProfile userProfile = JsonConvert.DeserializeObject<UserProfile[]>(response).ToList().FirstOrDefault();
        if (userProfile != null)
        {
            profilePanel.SetActive(true);
            profileText.text = userProfile.fullname + " (" + userProfile.email + ")";

            try
            {
                // Download the image from the URL
                Texture2D texture = await Utils.DownloadImage(userProfile.profileimageurl);

                // Create a sprite from the downloaded texture
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                // Assign the sprite to the Image component
                profileImage.sprite = sprite;
            }
            catch (Exception e)
            {
                Debug.LogError("Error loading image: " + e.Message);
            }
        }
    }

    async Task sendScore(int score, string userId)
    {
        string baseUrl = "http://localhost/moodle/webservice/rest/server.php?wstoken={2}&wsfunction=mod_url_save_score&moodlewsrestformat=json&userid={0}&score={1}";
        string moodleUrl = string.Format(baseUrl, userId, score, apiToken);

        string response = await Utils.GETRequest(moodleUrl);
        if (response == null)
        {
            Debug.LogError("Null response from Moodle");
            return;
        }
    }

    public void updateText()
    {
        ChangeText(count.ToString());
    }

    public void ChangeText(string newText)
    {
        displayText.text = newText;
    }

    public void ChangeTime(float newText)
    {
        timeDisplay.text = newText.ToString("F0");
    }

    public void OnPlusButtonClick()
    {
        if (!isPlaying) return;
        count++;
        updateText();
    }

    public void OnResetButtonClick()
    {
        time = gameDuration;
        isPlaying = true;
        count = 0;
        updateText();
    }
}
