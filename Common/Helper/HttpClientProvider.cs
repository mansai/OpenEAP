using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Helper
{
    /// <summary>
    /// 可复用的 HttpClient 提供器：单例/可重复初始化的 HttpClient，包含 GET / POST(Form/JSON) 简便方法。
    /// - Initialize(bool ignoreSslErrors, TimeSpan? timeout) 可以多次调用；当设置不同会重建 HttpClient。
    /// - 使用 GetStringAsync/PostFormAsync/PostJsonAsync 进行请求。
    /// 注意：在高并发场景下应尽量复用 HttpClient；仅当忽略 SSL 时会重建带自定义 handler 的 HttpClient。
    /// </summary>
    public static class HttpClientProvider
    {
        private static readonly object _lock = new object();
        private static HttpClient _client;
        private static bool _ignoreSsl;
        private static TimeSpan? _timeout;

        /// <summary>
        /// 初始化或根据参数重建 HttpClient。默认 HTTP/1.1。
        /// </summary>
        public static void Initialize(TimeSpan? timeout = null)
        {
            lock (_lock)
            {
                // 如果已经初始化且设置相同，则只更新 timeout（如果指定）
                if (_client != null)
                {
                    if (timeout.HasValue)
                        _client.Timeout = timeout.Value;
                    return;
                }

                // Dispose 旧的 client（如果有），然后创建新的
                _client?.Dispose();

                var handler = new HttpClientHandler();

                // 危险：接受任何服务器证书，仅用于测试或你完全了解风险时使用
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                _client = new HttpClient(handler)
                {
                    DefaultRequestVersion = HttpVersion.Version11
                };

                if (timeout.HasValue)
                    _client.Timeout = timeout.Value;
                _timeout = timeout;
            }
        }

        private static HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    Initialize(TimeSpan.FromSeconds(10));
                }
                return _client;
            }
        }

        /// <summary>
        /// 发送 GET 请求并返回响应文本（确保成功状态码，否则抛出）。
        /// 可传入 headers（可选）。
        /// </summary>
        public static async Task<string> GetStringAsync(string url, IDictionary<string, string> headers = null)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                AddHeaders(request, headers);
                using (var response = await Client.SendAsync(request).ConfigureAwait(false))
                {
                    // 不要直接调用 EnsureSuccessStatusCode，先检查状态
                    if (!response.IsSuccessStatusCode)
                    {
                        // 读取错误响应内容
                        var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new HttpRequestException(errorContent);
                    }

                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// 发送 application/x-www-form-urlencoded POST 请求并返回响应文本（确保成功状态码，否则抛出）。
        /// </summary>
        public static async Task<string> PostFormAsync(string url, IDictionary<string, string> form, IDictionary<string, string> headers = null)
        {
            using (var content = new FormUrlEncodedContent(form ?? new Dictionary<string, string>()))
            using (var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content })
            {
                AddHeaders(request, headers);
                using (var response = await Client.SendAsync(request).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// 发送 application/json POST 请求并返回响应文本（确保成功状态码，否则抛出）。
        /// </summary>
        public static async Task<string> PostJsonAsync(string url, string json, IDictionary<string, string> headers = null, Encoding encoding = null, CancellationToken cancellationToken = default)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (_timeout.HasValue)
                linkedCts.CancelAfter(_timeout.Value);
            encoding = encoding ?? Encoding.UTF8;
            using (var content = new StringContent(json ?? "", encoding, "application/json"))
            using (var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content })
            {
                AddHeaders(request, headers);
                using (var response = await Client.SendAsync(request, HttpCompletionOption.ResponseContentRead, linkedCts.Token).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
        }

        private static void AddHeaders(HttpRequestMessage request, IDictionary<string, string> headers)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string> { ["Accept-Language"] = GlobalHelper.Language };
            }
            else
            {
                if (!headers.ContainsKey("Accept-Language"))
                    headers.Add("Accept-Language", GlobalHelper.Language);
            }
            foreach (var kv in headers)
            {
                // 如果是标准的 Content-Type 或类似的 header，通常应设置在 Content 上，但这里为了简单统一处理
                if (!request.Headers.TryAddWithoutValidation(kv.Key, kv.Value))
                {
                    if (request.Content != null)
                        request.Content.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                }
            }
        }
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public static async Task<byte[]> GetByteArrayAsync(string url, IDictionary<string, string>? headers = null)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                AddHeaders(request, headers);
                using (var response = await Client.SendAsync(request).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new HttpRequestException(errorContent);
                    }
                    return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                }
            }
        }
        /// <summary>
        /// 释放内部 HttpClient（如果你确实需要释放）。
        /// </summary>
        public static void DisposeClient()
        {
            lock (_lock)
            {
                _client?.Dispose();
                _client = null;
                _ignoreSsl = false;
                _timeout = null;
            }
        }
    }
}
