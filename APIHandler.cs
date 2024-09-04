using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DIYWorkerService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EmailGraphAPIWorkerService
{
    public class APIHandler
    {
        private DateTime _tokenExpirationTime;
        private AccessTokenModel accessToken;
        private static readonly Secrets key = new Secrets();
        private static readonly HttpClient client = new HttpClient();
        private readonly ILogger<APIHandler> _logger;

        public APIHandler(HttpClient client, ILogger<APIHandler> logger)
        {
           
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            accessToken = new AccessTokenModel();
        }

       

        public async Task ExecuteLogic(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                accessToken = await GetAccessToken();

                if (accessToken != null)
                {
                    List<MailMessageInfo> unreadMessages = await GetUnreadMailMessages(accessToken.access_token, key.userId);
                    // Process unread messages as needed
                }
                else
                {
                    throw new ApplicationException("Error While Getting Token [isTokenValid] false");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error occurred while requesting a new access token.");
                throw new ApplicationException("Error occurred while requesting a new access token due to network issues.", ex);
            }
            catch (ApplicationException ex)
            {
                _logger.LogInformation("ApplicationException occurred.");
                if (ex.Message.Contains("Error Not a valid token:"))
                {
                    _logger.LogInformation("Token is invalid or expired. Requesting new token.");
                    accessToken = await RequestNewAccessToken();
                }
                else
                {
                    _logger.LogError(ex, "An error occurred in ApplicationException.");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogInformation("UnauthorizedAccessException occurred.");
                // Handle unauthorized access
            }
            catch (AggregateException ex)
            {
                _logger.LogInformation("AggregateException occurred.");
                // Handle aggregate exception
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred in ExecuteLogic.");
                if (ex.Message.Contains("Error Not a valid token:"))
                {
                    accessToken = await RequestNewAccessToken();
                }
            }
        }

        public async Task<AccessTokenModel> GetAccessToken()
        {
            try
            {
                if (accessToken != null && IsTokenValid())
                {
                    return accessToken;
                }
                else
                {
                    _logger.LogInformation("Token is invalid or expired. Requesting new token.");
                    accessToken = await RequestNewAccessToken();
                    _tokenExpirationTime = DateTime.Now.AddSeconds(accessToken.expires_in);
                    return accessToken;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error in GetAccessToken: " + ex.Message, ex);
            }
        }

        private bool IsTokenValid()
        {
            bool isValid = _tokenExpirationTime > DateTime.Now;
            _logger.LogInformation($"Token validity check: {isValid}. Current time: {DateTime.Now}, Expiration time: {_tokenExpirationTime}");
            return isValid;
        }

        public async Task<AccessTokenModel> RequestNewAccessToken()
        {
            try
            {
                _logger.LogInformation("Requesting new access token.");

                string tokenEndpoint = $"https://login.microsoftonline.com/{key.tenantId}/oauth2/v2.0/token";
                var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", key.clientId),
                    new KeyValuePair<string, string>("client_secret", key.clientSecret),
                    new KeyValuePair<string, string>("scope", key.scope),
                    new KeyValuePair<string, string>("grant_type", key.grant_type)
                });

                HttpResponseMessage response = await client.PostAsync(tokenEndpoint, requestContent);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                try
                {
                    AccessTokenModel token = JsonConvert.DeserializeObject<AccessTokenModel>(responseBody);
                    _tokenExpirationTime = DateTime.Now.AddSeconds(token.expires_in);
                    return token;
                }
                catch (JsonException ex)
                {
                    throw new ApplicationException("Error deserializing access token response: " + ex.Message, ex);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new ApplicationException("Error requesting new access token: HttpRequestException", ex);
            }
        }

        public async Task<List<MailMessageInfo>> GetUnreadMailMessages(string accessToken, string userId)
        {
            try
            {
                DateTime twoDaysAgo = DateTime.UtcNow.AddDays(-2);
                string endpoint = $"https://graph.microsoft.com/v1.0/users/{userId}/mailFolders/inbox/messages?$filter=isRead eq false and createdDateTime ge {twoDaysAgo:yyyy-MM-ddTHH:mm:ssZ}&$select=id,createdDateTime,sentDateTime,sender,subject,body";

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                try
                {
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);
                    var unreadMessages = new List<MailMessageInfo>();
                    foreach (var message in jsonResponse.value)
                    {
                        DateTime createdDateTime = DateTime.Parse(message["createdDateTime"].ToString());
                        DateTime sentDateTime = DateTime.Parse(message["sentDateTime"].ToString());
                        TimeZoneInfo istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                        createdDateTime = TimeZoneInfo.ConvertTimeFromUtc(createdDateTime, istTimeZone);
                        sentDateTime = TimeZoneInfo.ConvertTimeFromUtc(sentDateTime, istTimeZone);

                        unreadMessages.Add(new MailMessageInfo
                        {
                            MessageId = message["id"].ToString(),
                            CreatedDateTime = createdDateTime,
                            SentDateTime = sentDateTime,
                            Sender = message["sender"]?["emailAddress"]?["address"].ToString() ?? "Unknown",
                            Subject = message["subject"].ToString(),
                            Body = message["body"].ToString()
                        });
                    }

                    return unreadMessages;
                }
                catch (JsonException ex)
                {
                    throw new ApplicationException("Error deserializing unread mail messages response: " + ex.Message, ex);
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("401") || ex.Message.Contains("400"))
                {
                    throw new ApplicationException("Error Not a valid token:");
                }
                else
                {
                    throw new ApplicationException("Error requesting unread mail messages: HttpRequestException", ex);
                }
            }
        }

        public async Task MarkMessageAsRead(string accessToken, string userId, string messageId)
        {
            try
            {
                string endpoint = $"https://graph.microsoft.com/v1.0/users/{userId}/messages/{messageId}";
                var content = new StringContent("{\"isRead\": true}", Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Content = content;

                HttpResponseMessage response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new ApplicationException("Error marking message as read: HttpRequestException", ex);
            }
        }
    }

    
}
