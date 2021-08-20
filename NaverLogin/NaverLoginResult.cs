using System;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace NaverLogin
{
    // 네이버 로그인 결과
    public class NaverLoginResult
    {
        public bool IsSuccess { get; set; }
        public string? CookieString { get; set; }
        public Uri? ResponseUri { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string? ErrorMessage { get; set; }
        
        public HttpContent Content { get; set; }

        public static NaverLoginResult FromHttpResponseMessage(HttpResponseMessage response)
        {
            response.Headers.TryGetValues("Set-Cookie", out var cookie);

            return new NaverLoginResult
            {
                IsSuccess = response.IsSuccessStatusCode,
                StatusCode = response.StatusCode,
                ResponseUri = response.RequestMessage?.RequestUri,
                Content = response.Content,

                CookieString = cookie?.First(),
                ErrorMessage = "" // TODO: 로그인 실패시 오류메세지 확인
            };
        }
    }
}