using System.Net.Http;

namespace NaverLogin
{
    public class NaverOAuthResult : NaverLoginResult
    {
        protected NaverOAuthResult(HttpResponseMessage response) : base(response)
        {
            
        }
        
        public string? State { get; private set; }
        public string? Code { get; private set; }

        public static NaverOAuthResult FromOAuthResponseMessage(HttpResponseMessage message,
            string code, string state)
        {
            var r = NaverLoginResult.FromHttpResponseMessage(message);
            return new NaverOAuthResult(message)
            {
                IsSuccess = r.IsSuccess,
                StatusCode = r.StatusCode,
                ResponseUri = r.ResponseUri,
                
                CookieString = r.CookieString,
                ErrorMessage = r.ErrorMessage,
                
                State = state,
                Code = code
            };
        }
    }
}