namespace NaverLogin
{
    // BVSD 문자열 생성 인터페이스
    public interface IBvsdGenerator
    {
        string Generate(NaverLoginForm form, BrowserInfo browser);
    }
}