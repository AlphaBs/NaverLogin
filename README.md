# NaverLogin
.NET 네이버 로그인 라이브러리

현재 GNU 라이센스 사용중입니다. 나중에 변경할 예정

## 공통

### [HttpClient](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-5.0)
모든 웹 요청은 `HttpClient` 으로 이루어집니다. `HttpClient` 객체 생성과 관리는 라이브러리에서 처리하지 않습니다.  
따라서 라이브러리를 사용하기 전 `HttpClient` 객체 생성과 관리를 위한 코드 작성이 필요합니다. 특히 쿠키 관리가 필요할 경우, `CookieContainer` 객체를 생성하여 관리하시길 바랍니다.

### BrowserInfo

브라우저와 클라이언트와 관련된 데이터는 모두 `BrowserInfo` 객체에서 저장합니다.  
아직까지는 `UserAgent` 속성 뿐이지만, 추후 창 크기, 폰트 목록, GPU 이름 등등 BVSD 처리를 위해 더 많은 속성이 추가될 수 있습니다. 

```c#
var browser = new BrowserInfo
{
    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246";
};
```

### NaverLoginForm

네이버 로그인을 위한 계정 정보, 로그인 옵션 등이 포함되어 있습니다.  
현재 아이디와 비밀번호 속성만 작동하며, 추후 IP 보안, 로그인 유지 옵션 등이 추가될 수 있습니다.

```c#
var loginForm = new NaverLoginForm
{
    Id = "네이버 계정 아이디",
    Password = "네이버 계정 비밀번호"
};
```

## NaverHeadlessLogin

네이버 내부 서비스를 이용하기 위한 로그인. (메일, 카페, 블로그 등등)

### 사용법

```c#
var http = new HttpClient();
var browser = new BrowserInfo
{
    UserAgent = "유저에이전트"
};

var naverLogin = new NaverHeadlessLogin(http, browser);
var loginForm = new NaverLoginForm
{
    Id = "네이버 아이디",
    Password = "네이버 비밀번호"
}

var loginResult = await naverLogin.Login(loginForm);

// loginResult.IsSuccess : 로그인 성공 여부
// loginResult.Response : HttpClient에서 반환한 HttpResponseMessage
```

#### bool AutoRedirect { get; set; }

리다이렉트를 자동으로 처리할지 여부. 3xx 리다이렉트는 HttpClient 의 설정에 따라 처리되며 이 속성은 자바스크립트를 이용한 리다이렉트(location.href)를 자동으로 처리할지의 여부입니다.  
기본값은 true 입니다.

#### Task<NaverLoginResult> Login(NaverLoginForm form)

form 정보를 사용하여 로그인 후 네이버 메인 페이지로 이동합니다.

#### Task<NaverLoginResult> Login(NaverLoginForm form, string redirect)

form 정보를 사용하여 로그인 후 redirect 페이지로 이동합니다.

#### Task<NaverLoginResult> LoginFromUrl(NaverLoginForm form, string url)

url 페이지에서 form 정보를 사용하여 로그인합니다.

#### Task<NaverLoginResult> LoginFromHtml(NaverLoginForm form, string loginPageHtml, string currentReferer="")

form: 로그인할 정보
loginPageHtml: 로그인 페이지의 HTML 코드  
currentReferer(선택): 로그인 요청을 보낼 때 referer 헤더에 설정할 값

## NaverHeadlessOAuth

네이버 외부 서비스를 이용하기 위한 로그인. OAuth 로그인 방식 처리

### 사용법

```c#
// http: HttpClient 객체, browser: BrowserInfo 객체. 문서 상단 '공통' 부분에 설명되어 있습니다.
var naverOAuth = new NaverHeadlessOAuth(http, browser);

// loginForm: NaverLoginForm 객체. 아이디와 비밀번호를 여기에 입력하세요.
// response_type, client_id 등등은 oauth 로그인 페이지의 url 에 있습니다.
var loginResult = await naverOAuth.Login(loginForm, "<response_type>", "<client_id>", "<redirect_url>", "<state>");

// loginResult.IsSuccess: 로그인 성공여부
// loginResult.Response: HttpClient의 HttpResponseMessage
```

#### Task<NaverOAuthResult> Login(NaverLoginForm form, string? responseType, string? clientId, string? redirectUri)

`responseType`, `clientId`, `redirectUri` 를 이용해 네이버 OAuth 로그인 페이지의 uri 를 생성하여 이동 후, 해당 페이지에서 form 정보를 사용하여 로그인합니다.  

#### Task<NaverOAuthResult> Login(NaverLoginForm form, string? responseType, string? clientId, string? redirectUri, string? state)

`responseType`, `clientId`, `redirectUri`, `state` 를 이용해 네이버 OAuth 로그인 페이지의 uri 를 생성하여 이동 후, 해당 페이지에서 form 정보를 사용하여 로그인합니다.  

#### Task<NaverOAuthResult> LoginFromUrl(NaverLoginForm form, string url)

url 페이지에서 form 정보를 사용하여 OAuth 로그인을 합니다.

## NaverSearchAdvisor

[네이버 서치어드바이저](https://searchadvisor.naver.com/) 에서 네이버 OAuth 를 이용한 로그인 기능을 제공합니다.

### 사용법

```c#
var searchAdvisor = new NaverSearchAdvisor(http, browser);
var loginResult = await searchAdvisor.Login(loginForm);

// loginResult 는 api/auth/login-token 요청 후 응답입니다. (access-token, refresh-token 포함)
// loginResult 는 HttpClient 의 HttpResponseMessage 형식입니다.  
```

#### Task<string> AuthConfirm(object loginData)

api/auth/confirm 요청. loginData 는 로그인 후 결과(JSON) 를 역직렬화한 객체를 의미합니다.
