using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HttpAction;
using HtmlAgilityPack;

namespace NaverLogin
{
    // 네이버 로그인
    public class NaverHeadlessLogin
    {
        private static readonly Random random = new Random();
        
        private readonly HttpClient http;
        private readonly BrowserInfo browser;
        
        // JavaScript Redirect 자동으로 처리할지
        public bool AutoRedirect { get; set; } = true;

        private string referer = "";
        
        public NaverHeadlessLogin(HttpClient http, BrowserInfo browser)
        {
            this.http = http;
            this.browser = browser;

            if (!browser.IsValid()) throw new ArgumentException(nameof(browser));
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
        
        // GET: 로그인 페이지, HTML 반환
        private Task<string> getLoginPage()
            => getLoginPage("form", "https://www.naver.com");

        private Task<string> getLoginPage(string redirect)
            => getLoginPage("form", redirect);

        private Task<string> getLoginPage(string mode, string redirect)
        {
            referer = redirect;
            return http.SendActionAsync(new HttpAction<string>
            {
                Method = HttpMethod.Get,
                Host = "https://nid.naver.com",
                Path = "nidlogin.login",
                Queries = new HttpQueryCollection
                {
                    {"mode", mode},
                    {"url", redirect}
                },
                RequestHeaders = getHeaders(),
                ResponseHandler = async res =>
                {
                    referer = res.RequestMessage?.RequestUri?.ToString() ?? "";
                    return await res.Content.ReadAsStringAsync();
                }
            });
        }

        private Task<string> getLoginPageFromUrl(string url)
            => http.SendActionAsync(new HttpAction<string>
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
                RequestHeaders = getHeaders(),
                ResponseHandler = async res =>
                {
                    referer = res.RequestMessage?.RequestUri?.ToString() ?? "";
                    return await res.Content.ReadAsStringAsync();
                }
            });

        // GET: dynamicKey로 NaverLoginSessionKey 얻기
        private Task<NaverLoginSessionKey> getSessionKey(string dynamicKey)
            => http.SendActionAsync(new HttpAction<NaverLoginSessionKey>
            {
                Method = HttpMethod.Get,
                Host = "https://nid.naver.com",
                Path = $"dynamicKey/{dynamicKey}",
                RequestHeaders = getHeaders(useReferer: false),
                ResponseHandler = async res =>
                {
                    var resStr = await res.Content.ReadAsStringAsync();
                    var keySplit = resStr.Split(',');
                    return new NaverLoginSessionKey
                    {
                        SessionKey = keySplit[0],
                        KeyName = keySplit[1],
                        EValue = keySplit[2],
                        NValue = keySplit[3]
                    };
                }
            });

        // POST: 로그인 정보를 서버에 전송
        private Task<HttpResponseMessage> submitLoginForm(Uri uri, Dictionary<string, string> formData)
        {
            var content = new FormUrlEncodedContent(formData);
            return submitLoginForm(uri, content);
        }
        
        private Task<HttpResponseMessage> submitLoginForm(Uri uri, HttpContent content)
            => http.SendActionAsync(new HttpAction<HttpResponseMessage>
            {
                Method = HttpMethod.Post,
                RequestUri = uri,
                Content = content,
                RequestHeaders = getHeaders(),
                ResponseHandler = Task.FromResult
            });


        // str 의 길이값을 아스키코드값으로 하여 문자 반환
        private string getLenChar(string str)
        {
            return new (new[] { (char)str.Length });
        }
        
        // 로그인
        public Task<NaverLoginResult> Login(NaverLoginForm form)
            => Login(form, "https://www.naver.com");
        
        public async Task<NaverLoginResult> Login(NaverLoginForm form, string redirect)
        {
            // get login page
            var loginPageHtml = await getLoginPage(redirect);
            return await LoginFromHtml(form, loginPageHtml, "https://www.naver.com/");
        }

        public async Task<NaverLoginResult> LoginFromUrl(NaverLoginForm form, string url)
        {
            var loginPageHtml = await getLoginPageFromUrl(url);
            return await LoginFromHtml(form, loginPageHtml);
        }
        
        public async Task<NaverLoginResult> LoginFromHtml(
            NaverLoginForm form, string loginPageHtml, string currentReferer="")
        {
            if (!string.IsNullOrEmpty(currentReferer))
                referer = currentReferer;
            
            // parse login page
            var loginHtmlDoc = new HtmlDocument();
            loginHtmlDoc.LoadHtml(loginPageHtml);

            var loginForm = loginHtmlDoc.DocumentNode.Descendants("form")
                .First(x => x.Id == "frmNIDLogin");
            var formAction = loginForm.GetAttributeValue("action", "https://nid.naver.com/nidlogin.login");
            var loginInputTags = loginForm.Descendants("input");
            
            var loginInputDict = new Dictionary<string, string>();
            foreach (var item in loginInputTags)
            {
                var inputType = item.GetAttributeValue("type", "");
                var inputName = item.GetAttributeValue("name", "");
                var inputValue = item.GetAttributeValue("value", "");
                
                if (inputType is "hidden" or "text" or "password")
                    loginInputDict[inputName] = inputValue;
            }

            //loginInputDict["privateMode"] = "true";
            
            // get session keys
            var sessionKey = await getSessionKey(loginInputDict["dynamicKey"]);
            loginInputDict["encnm"] = sessionKey.KeyName;

            // rsa encrypt
            var encPlain = getLenChar(sessionKey.SessionKey) + sessionKey.SessionKey + 
                           getLenChar(form.Id) + form.Id +
                           getLenChar(form.Password) + form.Password;
            
            var encpw = Util.EncryptRSA(sessionKey.EValue, sessionKey.NValue, encPlain);
            
            loginInputDict["encpw"] = encpw;
            loginInputDict["enctp"] = "1";
            
            // delay
            await Task.Delay(1000 + random.Next(10, 1000));
            
            // bvsd
            loginInputDict["bvsd"] = browser.BvsdGenerator?.Generate(form, browser) ?? "";
            
            // submit
            var response = await submitLoginForm(new Uri(formAction), loginInputDict);
            
            if (AutoRedirect)
                response = await Util.FinalRedirect(http, response, getHeaders());

            return NaverLoginResult.FromHttpResponseMessage(response);
        }
    }
}