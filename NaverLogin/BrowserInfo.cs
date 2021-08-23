namespace NaverLogin
{
    // 브라우저 정보 (User-Agent, 화면크기, 폰트목록, navigator 정보 등등)
    public class BrowserInfo
    {
        public string? UserAgent { get; set; }
        public IBvsdGenerator? BvsdGenerator { get; set; } = new DefaultBvsdGenerator();

        // 유효성 검사
        public bool IsValid()
        {
            return string.IsNullOrEmpty(UserAgent) == false 
                && BvsdGenerator != null;
        }
        
        // 필요하면 추가할 예정
    }
}