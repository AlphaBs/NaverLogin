using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using HttpAction;

namespace NaverLogin
{
    public class NaverHeadlessOAuth
    {
        public static string t = "111";
        
        private readonly NaverHeadlessLogin naverLogin;
        private readonly HttpClient http;
        private readonly BrowserInfo browser;

        private string referer = "";
        
        public NaverHeadlessOAuth(HttpClient http, BrowserInfo browser)
        {
            this.http = http;
            this.browser = browser;

            // NaverHeadlessOAuth 는 내부적으로 NaverHeadlessLogin 을 사용하여 처리
            naverLogin = new NaverHeadlessLogin(http, browser)
            {
                AutoRedirect = false // 리다이렉트시 code, state 얻기 위해 직접 처리
            };
        }

        private HttpHeaderCollection getHeaders(bool useReferer=true)
        {
            var header = new HttpHeaderCollection();
            
            if (useReferer && !string.IsNullOrEmpty(referer))
                header.Add("referer", referer);
            
            if (!string.IsNullOrEmpty(browser.UserAgent))
                header.Add("user-agent", browser.UserAgent);

            return header;
        }
        
        private Task<string> getOAuthStart(
            string? responseType, string? clientId, string? redirectUri, string? state)
            => http.SendActionAsync(new HttpAction<string>
            {
                Host = "https://nid.naver.com",
                Path = "oauth2.0/authorize",
                Queries = new HttpQueryCollection
                {
                    {"response_type", responseType ?? ""},
                    {"client_id", clientId ?? ""},
                    {"redirect_uri", redirectUri ?? ""},
                    {"state", state ?? ""}
                }
            });

        // 로그인 성공 후 목적지로 리다이렉트, state와 code를 찾기
        private async Task<NaverOAuthResult> getResult(NaverLoginResult res)
        {
            var httpResponse = res.Response;
            string state = "", code = "";
            
            httpResponse = await Util.FinalRedirect(http, httpResponse, message =>
            {
                // 리다이렉트한 URL 가져오기
                var resUri = message.RequestMessage?.RequestUri;
                if (resUri == null)
                    return;

                // parse query string
                var queryStr = resUri.Query;
                var query = HttpUtility.ParseQueryString(queryStr);

                // extract state, code
                var nState = query.Get("state");
                var nCode = query.Get("code");

                if (!string.IsNullOrEmpty(nState))
                    state = nState;
                if (!string.IsNullOrEmpty(nCode))
                    code = nCode;
            });

            return NaverOAuthResult.FromOAuthResponseMessage(httpResponse, code, state);
        }
        
        public Task<NaverOAuthResult> Login(NaverLoginForm form,
            string? responseType, string? clientId, string? redirectUri)
        {
            var state = Util.GenerateRandomHex(16);
            return Login(form, responseType, clientId, redirectUri, state);
        }
        
        public async Task<NaverOAuthResult> Login(NaverLoginForm form, 
            string? responseType, string? clientId, string? redirectUri, string? state)
        {
            var oauthPage = await getOAuthStart(responseType, clientId, redirectUri, state);
            var result = await naverLogin.LoginFromHtml(form, oauthPage); // TODO: error handling
            return await getResult(result);
        }

        public async Task<NaverOAuthResult> LoginFromUrl(NaverLoginForm form, string url)
        {
            var oauthPage = await http.SendActionAsync(new HttpAction<HttpResponseMessage>
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
                RequestHeaders = getHeaders(),
                ResponseHandler = Task.FromResult
            });

            oauthPage.EnsureSuccessStatusCode();
            var oauthPageHtml = await oauthPage.Content.ReadAsStringAsync();
            
            var result = await naverLogin.LoginFromHtml(form, oauthPageHtml);
            return await getResult(result);
        }
    }
}