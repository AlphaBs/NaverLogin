using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using HttpAction;

namespace NaverLogin
{
    public class Util
    {
        private static readonly Regex locationReplaceUrlRegex
            = new Regex("location\\.replace\\([\\\"\\'](.*)[\\\"\\']\\);");

        private static readonly Random random = new Random();

        // modulus(hex string), exponent(hex string)를 키로 설정하여
        // plain을 RSA 암호화한 후 HEX 문자열로 반환
        public static string EncryptRSA(string modulus, string exponent, string plain)
        {
            using var rsa = new RSACryptoServiceProvider();

            var publicKey = new RSAParameters();
            publicKey.Modulus = Convert.FromHexString(modulus);
            publicKey.Exponent = Convert.FromHexString(exponent);

            var plainBytes = Encoding.UTF8.GetBytes(plain);
            rsa.ImportParameters(publicKey);

            var encryptBytes = rsa.Encrypt(plainBytes, false);
            return Convert.ToHexString(encryptBytes).ToLower();
        }

        // html 문서 내용이 다른 페이지로 리다이렉트하는 내용이라면
        // 리다이렉트할 페이지의 주소를 반환
        public static string? GetRedirectUrlFromHTML(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var body = doc.DocumentNode.Descendants("body")
                .FirstOrDefault();

            string? redirectUrl = null;

            if (body == null || string.IsNullOrWhiteSpace(body.InnerText))
            {
                var scripts = doc.DocumentNode.Descendants("script");
                foreach (var item in scripts)
                {
                    var matchResult = locationReplaceUrlRegex.Match(item.InnerHtml);
                    if (matchResult.Success && matchResult.Groups.Count >= 2)
                        redirectUrl = matchResult.Groups[1].Value;
                }
            }

            return redirectUrl;
        }

        // 리다이렉트를 모두 거쳐 최종 response 를 반환함
        public static Task<HttpResponseMessage> FinalRedirect(HttpClient http, HttpResponseMessage response,
            HttpHeaderCollection? header)
            => FinalRedirect(http, response, header, null);
        
        public static async Task<HttpResponseMessage> FinalRedirect(HttpClient http, HttpResponseMessage response,
            HttpHeaderCollection? header, Action<HttpResponseMessage>? middle)
        {
            string referer = "";
            while (true)
            {
                // 현재 주소를 referer 로 설정
                var reqUri = response.RequestMessage?.RequestUri;
                if (reqUri != null)
                {
                    Uri? curUri;
                    try
                    {
                        curUri = new Uri(referer);
                    }
                    catch
                    {
                        curUri = null;
                    }
                    
                    if (curUri != null && reqUri.Host != curUri.Host)
                        referer = reqUri.Scheme + reqUri.Host;
                    else
                        referer = reqUri.ToString();
                }

                // 리다이렉트 url 찾기
                var content = await response.Content.ReadAsStringAsync();
                var redirect = GetRedirectUrlFromHtml(content);
                
                //Console.WriteLine(redirect);
                
                // 최종 목적지 도착한 경우
                if (string.IsNullOrEmpty(redirect))
                    break;
                
                // 리다이렉트 처리
                if (header != null && !string.IsNullOrEmpty(referer))
                    header.Set("referer", referer);
                response = await http.SendActionAsync(new HttpAction<HttpResponseMessage>
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(redirect),
                    RequestHeaders = header,
                    ResponseHandler = Task.FromResult
                });
                
                middle?.Invoke(response);
            }

            return response;
        }

        // 길이가 length인 랜덤 hex문자열 반환
        public static string GenerateRandomHex(int digits)
        {
            byte[] buffer = new byte[digits / 2];
            random.NextBytes(buffer);
            string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
            if (digits % 2 == 0)
                return result;
            return result + random.Next(16).ToString("X");
        }
    }
}