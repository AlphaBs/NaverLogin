using System;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace NaverLogin
{
    // 네이버 로그인 결과
    public class NaverLoginResult
    {
        protected NaverLoginResult(HttpResponseMessage response)
        {
            this.Response = response;
        }
        
        public bool IsSuccess { get; protected set; }
        public string? CookieString { get; protected set; }
        public Uri? ResponseUri { get; protected set; }
        public HttpStatusCode StatusCode { get; protected set; }
        public string? ErrorMessage { get; protected set; }
        public HttpResponseMessage Response { get; private set; }

        public static NaverLoginResult FromHttpResponseMessage(HttpResponseMessage response)
        {
            response.Headers.TryGetValues("Set-Cookie", out var cookie);

            return new NaverLoginResult(response)
            {
                IsSuccess = response.IsSuccessStatusCode,
                StatusCode = response.StatusCode,
                ResponseUri = response.RequestMessage?.RequestUri,

                CookieString = cookie?.First(),
                ErrorMessage = "" // TODO: 로그인 실패시 오류메세지 확인
            };
        }
    }
}