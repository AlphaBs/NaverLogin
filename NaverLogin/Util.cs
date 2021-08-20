using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

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