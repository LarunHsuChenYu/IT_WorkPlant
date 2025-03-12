using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace IT_WorkPlant.Models
{
    public class LineNotificationModel
    {
        private readonly string _accessToken;

        public LineNotificationModel()
        {
            _accessToken = ConfigurationManager.AppSettings["LineNotifyAccessToken"];
            if (string.IsNullOrWhiteSpace(_accessToken))
            {
                throw new InvalidOperationException("AccessToken is not configured in the app settings.");
            }
        }

        /// <summary>
        /// 發送訊息到 Line Notify，可附加圖片。
        /// </summary>
        /// <param name="message">訊息內容</param>
        /// <param name="uploadedImage">可選的上傳圖片</param>
        /// <returns>Task</returns>
        public async Task SendLineNotifyAsync(string message, HttpPostedFile uploadedImage = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));

            using (HttpClient httpClient = new HttpClient())
            {
                string apiUrl = "https://notify-api.line.me/api/notify";
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                var formData = new MultipartFormDataContent();

                // 加入訊息
                formData.Add(new StringContent(message), "message");

                // 如果有上傳的圖片，加入圖片
                if (uploadedImage != null && uploadedImage.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(uploadedImage.FileName);
                    byte[] fileData;
                    using (var binaryReader = new BinaryReader(uploadedImage.InputStream))
                    {
                        fileData = binaryReader.ReadBytes(uploadedImage.ContentLength);
                    }

                    var byteArrayContent = new ByteArrayContent(fileData);
                    byteArrayContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                    formData.Add(byteArrayContent, "imageFile", fileName);
                }

                // 發送請求
                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, formData);
                response.EnsureSuccessStatusCode(); // 確保請求成功
            }
        }

        /// <summary>
        /// 發送僅包含訊息的通知。
        /// </summary>
        /// <param name="message">訊息內容</param>
        /// <returns>Task</returns>
        public Task SendLineNotifyAsync(string message)
        {
            return SendLineNotifyAsync(message, null);
        }

        /// <summary>
        /// 發送通知到 Line Messaging API (未來擴展)
        /// </summary>
        /// <param name="to">接收者 ID</param>
        /// <param name="message">通知內容</param>
        /// <returns>Task</returns>
        public async Task SendLineMessagingAsync(string to, string message)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Recipient ID cannot be null or empty.", nameof(to));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));

            string apiUrl = "https://api.line.me/v2/bot/message/push";

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                var payload = new
                {
                    to = to,
                    messages = new[]
                    {
                        new { type = "text", text = message }
                    }
                };

                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(payload),
                    System.Text.Encoding.UTF8,
                    "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode(); // 確保回應成功，否則拋出異常
            }
        }
    }
}
