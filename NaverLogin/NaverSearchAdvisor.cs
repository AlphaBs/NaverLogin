﻿using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HttpAction;
using NaverLogin;
using Newtonsoft.Json.Linq;

namespace NaverLoginTest
{
    // 네이버 서치어드바이저
    // https://searchadvisor.com
    public class NaverSearchAdvisor
    {
        // CSRF 토큰 가져오는 정규식
        private readonly Regex csrfTokenRegex = new Regex("csrfToken:\"([^\"]*)\"");

        private readonly HttpClient http;
        private readonly BrowserInfo browser;

        public string CsrfToken { get; private set; } = "";
        private string referer = "";
        
        public NaverSearchAdvisor(HttpClient http, BrowserInfo browser)
        {
            this.http = http;
            this.browser = browser;
        }

        private HttpHeaderCollection getHeaders(bool useReferer = true, bool useCsrf = false)
        {
            var header = new HttpHeaderCollection();
            
            if (useReferer && !string.IsNullOrEmpty(referer))
                header.Add("referer", referer);
            
            if (!string.IsNullOrEmpty(browser.UserAgent))
                header.Add("user-agent", browser.UserAgent);
            
            if (useCsrf && !string.IsNullOrEmpty(CsrfToken))
                header.Add("CSRF-Token", CsrfToken);

            return header;
        }

        // GET: login-token
        private Task<string> getLoginToken(string code, string state)
            => http.SendActionAsync(new HttpAction<string>
            {
                Method = HttpMethod.Post,
                Host = "https://searchadvisor.naver.com",
                Path = "api/auth/login-token",
                RequestHeaders = getHeaders(useCsrf: true),
                Content = new JsonHttpContent(new
                {
                    code = code,
                    state = state
                }),
                ResponseHandler = HttpResponseHandlers.GetStringResponseHandler()
            });

        // 로그인
        public async Task<string> Login(NaverLoginForm loginForm)
        {
            var login = new NaverHeadlessOAuth(http, browser);
            
            var url = "https://searchadvisor.naver.com/auth/login?caller=%2Fconsole%2Fboard";
            referer = url;
            var oauthResult = await login.LoginFromUrl(loginForm, url);
            
            var html = await oauthResult.Response.Content.ReadAsStringAsync();
            
            var regexTokenResult = csrfTokenRegex.Match(html);
            if (regexTokenResult.Success && regexTokenResult.Groups.Count >= 2)
                CsrfToken = regexTokenResult.Groups[1].Value;

            var loginTokenResponse = await getLoginToken(oauthResult.Code, oauthResult.State);
            return loginTokenResponse;
        }

        // GET: auth/confirm
        // 용도는 불명
        public Task<string> AuthConfirm(string json)
        {
            var loginData = JObject.Parse(json);
            return AuthConfirm(loginData);
        }
        
        public Task<string> AuthConfirm(object loginData)
            => http.SendActionAsync(new HttpAction<string>
            {
                Method = HttpMethod.Post,
                Host = "https://searchadvisor.naver.com",
                Path = "api/auth/confirm",
                RequestHeaders = getHeaders(),
                Content = new JsonHttpContent(new
                {
                    _csrf = CsrfToken,
                    loginData = loginData
                }),
                ResponseHandler = HttpResponseHandlers.GetStringResponseHandler()
            });
    }
}