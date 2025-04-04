using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace IT_WorkPlant.Models
{
    public class LineNotificationModel
    {
        private readonly string _messagingToken;

        public LineNotificationModel()
        {
            _messagingToken = ConfigurationManager.AppSettings["LineMessagingAccessToken"];
            if (string.IsNullOrWhiteSpace(_messagingToken))
            {
                throw new InvalidOperationException("Messaging AccessToken is not configured.");
            }
        }

        public async Task SendLineNotifyWithImageAsync(string message, HttpPostedFile imageFile)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));

            if (imageFile == null || imageFile.ContentLength == 0)
                throw new ArgumentException("Image file is empty.", nameof(imageFile));

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _messagingToken);

                using (var form = new MultipartFormDataContent())
                {
                    form.Add(new StringContent(message), "message");

                    var streamContent = new StreamContent(imageFile.InputStream);
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(imageFile.ContentType);
                    form.Add(streamContent, "imageFile", imageFile.FileName);

                    var response = await client.PostAsync("https://notify-api.line.me/api/notify", form);
                    string responseText = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"📥 LINE Notify Response: {response.StatusCode} - {responseText}");

                    response.EnsureSuccessStatusCode();
                }
            }
        }

        public async Task SendLineNotifyWithTextAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _messagingToken);

                var values = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("message", message)
                });

                var response = await client.PostAsync("https://notify-api.line.me/api/notify", values);
                string responseText = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"📥 LINE Notify Response: {response.StatusCode} - {responseText}");

                response.EnsureSuccessStatusCode();
            }
        }

        public async Task SendLineGroupMessageAsync(string groupId, string message, string imageUrl = null)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _messagingToken);

                var msgList = new List<object>
                {
                    new { type = "text", text = message }
                };

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    msgList.Add(new
                    {
                        type = "image",
                        originalContentUrl = imageUrl,
                        previewImageUrl = imageUrl
                    });
                }

                var body = new
                {
                    to = groupId,
                    messages = msgList
                };

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(body);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://api.line.me/v2/bot/message/push", content);
                string responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"📥 [LINE RESPONSE] {response.StatusCode} - {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"LINE Push failed: {response.StatusCode} {responseContent}");
                }
            }
        }

        public string GetLineUserIdByEmpID(string empID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
            string query = "SELECT LineUserID FROM Users WHERE UserEmpID = @EmpID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@EmpID", empID);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result?.ToString();
            }
        }
    }
}
