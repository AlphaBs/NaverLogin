using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography;
using System.Text;
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

        // 다른 BVSD 생성 방법을 사용하고 싶으면 속성 값 변경
        public IBvsdGenerator BvsdGenerator { get; set; }
            = new DefaultBvsdGenerator();

        private string? referer;
        
        public NaverHeadlessLogin(HttpClient http, BrowserInfo browser)
        {
            this.http = http;
            this.browser = browser;
            
            this.http.DefaultRequestHeaders.Add("user-agent", browser.UserAgent);
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
                RequestHeaders = new HttpHeaderCollection
                {
                    {"referer", referer}
                },
                ResponseHandler = async res =>
                {
                    referer = res.RequestMessage?.RequestUri?.ToString();
                    return await res.Content.ReadAsStringAsync();
                }
            });
        }

        // GET: dynamicKey로 NaverLoginSessionKey 얻기
        private Task<NaverLoginSessionKey> getSessionKey(string dynamicKey)
            => http.SendActionAsync(new HttpAction<NaverLoginSessionKey>
            {
                Method = HttpMethod.Get,
                Host = "https://nid.naver.com",
                Path = $"dynamicKey/{dynamicKey}",
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
                RequestHeaders = new HttpHeaderCollection
                {
                    {"referer", referer}
                },
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
            return await LoginFromHtml(form, loginPageHtml);
        }

        public async Task<NaverLoginResult> LoginFromHtml(NaverLoginForm form, string loginPageHtml)
        {
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
            loginInputDict["bvsd"] = BvsdGenerator.Generate(form, browser);
            
            // submit
            var response = await submitLoginForm(new Uri(formAction), loginInputDict);
            while (true)
            {
                var tempReferer = response.RequestMessage?.RequestUri?.ToString();
                if (!string.IsNullOrEmpty(tempReferer))
                    referer = tempReferer;
                
                var content = await response.Content.ReadAsStringAsync();
                var redirect = Util.GetRedirectUrlFromHTML(content);
                
                // 최종 목적지 도착
                if (string.IsNullOrEmpty(redirect))
                    break;
                
                // 리다이렉트 처리
                response = await http.SendActionAsync(new HttpAction<HttpResponseMessage>
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(redirect),
                    RequestHeaders = new HttpHeaderCollection
                    {
                        {"referer", referer}
                    },
                    ResponseHandler = Task.FromResult
                });
            }
            
            return NaverLoginResult.FromHttpResponseMessage(response);
        }
    }
}