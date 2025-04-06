using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static MSLX.Core.Utils.HttpService;

namespace MSLX.Core.Utils
{
    public class MSLApi
    {
        public async static Task<(bool Success, string? Data, string? Msg)> GetDataAsync(string path)
        {
            var getResponse = await GetAsync(path);
            if (getResponse.IsSuccessStatusCode)
            {
                var content = getResponse.Content;
                if (content == null)
                    return (false, null, "内容为空");
                var json = JObject.Parse(content);
                return (true, json["data"]?.ToString(), json["message"]?.ToString());
            }
            else
            {
                return (false, null, "请求错误！");
            }
        }

        public async static Task<HttpResponse> GetAsync(string path)
        {
            using var service = new HttpService();
            // UA可以这样设置，也可以按照下面getRequest里注释掉的方式设置
            service.SetDefaultHeadersUA(UAManager.GetUA(UAManager.UAType.MSLX));
            var getRequest = new HttpRequest
            {
                Url = "https://api.mslmc.cn/v3" + path,
                Method = HttpMethod.Get
                /*
                Headers = new Dictionary<string, string>
                {
                    ["User-Agent"] = UAManager.GetUA(UAManager.UAType.MSLX)
                }
                */
            };
            var getResponse = await service.SendAsync(getRequest);
            service.Dispose();
            return getResponse;
        }

        public async static Task<HttpResponse> PostAsync(string path, PostContentType postContentType, object data)
        {
            using var service = new HttpService();
            service.SetDefaultHeadersUA(UAManager.GetUA(UAManager.UAType.MSLX));
            var postRequest = new HttpRequest
            {
                Url = "https://api.mslmc.cn/v3" + path,
                Method = HttpMethod.Post
            };
            /*
            // POST JSON示例
            var postRequest = new HttpService.HttpRequest
            {
                ContentType=PostContentType.Json,
                Data = new { Name = "Test", Value = 123 }
            };

            // POST 表单示例
            var formRequest = new HttpService.HttpRequest
            {
                ContentType = PostContentType.FormUrlEncoded,
                Data = new Dictionary<string, string>
                {
                    ["username"] = "test",
                    ["password"] = "123456"
                }
            };

            // POST TEXT示例
            var textRequest = new HttpService.HttpRequest
            {
                ContentType = PostContentType.Text,
                Data = "123123"
            };

            // POST OCTET示例
            var octetRequest = new HttpService.HttpRequest
            {
                ContentType = PostContentType.Octet,
                Data = new byte[] { 0x01, 0x02, 0x03 }
            };
            */
            postRequest.ContentType = postContentType;
            postRequest.Data = data;
            var postResponse = await service.SendAsync(postRequest);
            service.Dispose();
            return postResponse;
        }
    }

    public class HttpService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly HttpClientHandler _httpHandler;
        private bool _disposed = false;

        public HttpService(TimeSpan? timeout = null, bool? allowAutoRedirect = null, bool? useCookies = null, DecompressionMethods? decompressionMethods = null)
        {
            _httpHandler = new HttpClientHandler();
            if (allowAutoRedirect != null)
            {
                _httpHandler.AllowAutoRedirect = allowAutoRedirect.Value;
            }
            if (useCookies != null)
            {
                _httpHandler.UseCookies = useCookies.Value;
                _httpHandler.CookieContainer = new CookieContainer();
            }
            if (decompressionMethods != null)
            {
                _httpHandler.AutomaticDecompression = decompressionMethods.Value;
            }

            _httpClient = new HttpClient(_httpHandler);
            if (timeout != null)
            {
                _httpClient.Timeout = timeout.Value;
            }
        }

        public enum PostContentType { Json, FormUrlEncoded, Text, Octet }
        public class HttpRequest
        {
            public required string Url { get; set; }
            public HttpMethod Method { get; set; } = HttpMethod.Get;
            public Dictionary<string, string> Headers { get; set; } = new();
            public object? Data { get; set; }
            public PostContentType ContentType { get; set; } = PostContentType.Json;
        }

        public class HttpResponse
        {
            public string? Content { get; set; }
            public int StatusCode { get; set; }
            public Dictionary<string, string> Headers { get; set; } = new();
            public Dictionary<string, string> Cookies { get; set; } = new();
            public bool IsSuccessStatusCode { get; set; }
        }

        public async Task<HttpResponse> SendAsync(HttpRequest request)
        {
            using var message = new HttpRequestMessage(request.Method, request.Url);

            // 添加请求头
            foreach (var header in request.Headers)
            {
                message.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // 处理Post请求内容
            if (request.Method != HttpMethod.Get && request.Data != null)
            {
                _httpClient.DefaultRequestHeaders.Accept.TryParseAdd(HttpAcceptType(request.ContentType));
                message.Content = CreateHttpContent(request.ContentType, request.Data);
            }

            try
            {
                using var response = await _httpClient.SendAsync(message);
                return await CreateHttpResponse(response);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new TimeoutException("Request timed out", ex);
            }
        }

        private string HttpAcceptType(PostContentType contentType)
        {
            return contentType switch
            {
                PostContentType.Json => "application/json",
                PostContentType.FormUrlEncoded => "application/x-www-form-urlencoded",
                PostContentType.Text => "text/plain",
                PostContentType.Octet => "application/octet-stream",
                _ => throw new NotSupportedException($"Content type {contentType} is not supported")
            };
        }

        private HttpContent CreateHttpContent(PostContentType contentType, object data)
        {
            return contentType switch
            {
                PostContentType.Json => new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json"),
                PostContentType.FormUrlEncoded => new FormUrlEncodedContent(data as Dictionary<string, string> ?? new Dictionary<string, string>()),
                PostContentType.Text => new StringContent(data as string ?? "", Encoding.UTF8, "text/plain"),
                PostContentType.Octet => new ByteArrayContent(data as byte[] ?? new byte[0]),
                _ => throw new NotSupportedException($"Content type {contentType} is not supported")
            };
        }

        private async Task<HttpResponse> CreateHttpResponse(HttpResponseMessage response)
        {
            var result = new HttpResponse
            {
                StatusCode = (int)response.StatusCode,
                IsSuccessStatusCode = response.IsSuccessStatusCode,
                Headers = response.Headers
                    .Concat(response.Content.Headers)
                    .ToDictionary(h => h.Key, h => string.Join("; ", h.Value))
            };

            if (_httpHandler.CookieContainer != null)
            {
                var cookies = _httpHandler.CookieContainer.GetCookies(new Uri(response.RequestMessage?.RequestUri?.GetLeftPart(UriPartial.Authority) ?? string.Empty));
                result.Cookies = cookies.Cast<Cookie>()
                    .ToDictionary(c => c.Name, c => c.Value);
            }

            result.Content = await response.Content.ReadAsStringAsync();
            return result;
        }

        public void SetDefaultHeadersUA(string ua)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(ua);
        }

        public void ClearCookies()
        {
            _httpHandler.CookieContainer = new CookieContainer();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _httpClient?.Dispose();
            _httpHandler?.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    // UA管理器
    public class UAManager
    {
        public enum UAType { MSLX, Win, Linux, Mac, Android, IOS }

        private static readonly string _mslxUA = "MSLTeam-MSLX/" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        private static readonly string _winUA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36";
        private static readonly string _linuxUA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36";
        private static readonly string _macUA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36";
        private static readonly string _androidUA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36";
        private static readonly string _iosUA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36";

        public static string GetUA(UAType type = UAType.MSLX)
        {
            return type switch
            {
                UAType.MSLX => _mslxUA,
                UAType.Win => _winUA,
                UAType.Linux => _linuxUA,
                UAType.Mac => _macUA,
                UAType.Android => _androidUA,
                UAType.IOS => _iosUA,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
