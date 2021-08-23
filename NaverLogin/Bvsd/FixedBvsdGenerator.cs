using System;
using LZStringCSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NaverLogin.Bvsd
{
    // 고정된 문자열을 기반으로 한 BVSD 생성기
    public class FixedBvsdGenerator : IBvsdGenerator
    {
        // rawEncData: bvsd json 데이터에서 encData 속성의 인코딩되지 않은 값
        public FixedBvsdGenerator(string rawEncData)
        {
            this.RawEncData = rawEncData;
        }

        // uuid: bvsd json 데이터에서 uuid 속성의 값
        // rawEncData: bvsd json 데이터에서 encData 속성의 인코딩되지 않은 값
        public FixedBvsdGenerator(string uuid, string rawEncData)
        {
            this.UUID = uuid;
            this.RawEncData = rawEncData;
        }
        
        // uuid 속성의 값. null인 경우 임의의 UUID 문자열 생성
        public string? UUID { get; private set; }
        // encData 속성의 인코딩되지 않은 값
        public string RawEncData { get; private set; }

        public string Generate(NaverLoginForm form, BrowserInfo browser)
        {
            var encData = RawEncData;
            encData = LZString.CompressToEncodedURIComponent(encData);

            var job = new JObject();
            job.Add("uuid", UUID ?? Guid.NewGuid().ToString());
            job.Add("encData", encData);
            return job.ToString(Formatting.None);
        }
    }
}