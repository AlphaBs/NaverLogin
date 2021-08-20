﻿using System;
using LZStringCSharp;
using Newtonsoft.Json.Linq;

namespace NaverLogin
{
    // BVSD 문자열 생성하는 클래스. 
    public class DefaultBvsdGenerator : IBvsdGenerator
    {
        private string generateEncData(string uuid, string id, string userAgent)
        {
            var t = "{\"a\":\"" + uuid + "\",\"b\":\"1.3.4\",\"c\":false,\"d\":[{\"i\":\"id\",\"a\":[\"0,d,i0,65\",\"125,u,i0,65\"],\"b\":{\"a\":[\"0,a\"],\"b\":0},\"c\":\"\",\"d\":\"a\",\"e\":false,\"f\":false},{\"i\":\"pw\",\"a\":[\"0,d,i0,\",\"124,u,i0,\"],\"b\":{\"a\":[\"0,\"],\"b\":0},\"c\":\"\",\"d\":\"\",\"e\":true,\"f\":false}],\"e\":{\"a\":{\"a\":444,\"b\":444,\"c\":444},\"b\":{\"a\":444,\"b\":444,\"c\":444}},\"f\":{\"a\":{\"a\":{\"a\":444,\"b\":444,\"c\":444},\"b\":{\"a\":444,\"b\":444,\"c\":444}},\"b\":{\"a\":{\"a\":444,\"b\":444,\"c\":444},\"b\":{\"a\":444,\"b\":444,\"c\":444}}},\"g\":{\"a\":[\"0|16|928|238\",\"0|0|-4|1\",\"0|8|-4|1\",\"0|83|-52|0\",\"0|5|-3|-1\",\"0|58|-25|-2\",\"0|5|-4|-1\",\"0|8|-5|0\",\"0|8|-6|-2\",\"0|8|-5|-2\",\"0|8|-6|-1\",\"0|8|-6|-3\",\"0|8|-7|-2\",\"0|8|-6|-3\",\"0|8|-5|-3\",\"0|8|-5|-3\",\"0|9|-3|-1\",\"0|7|-3|-1\",\"0|8|-1|-1\",\"0|8|-1|0\",\"0|8|-1|0\",\"0|8|-1|0\",\"0|8|0|-1\",\"0|8|-1|0\",\"0|8|0|-1\",\"0|8|-1|0\",\"0|16|-2|0\",\"0|8|0|-1\",\"0|8|-1|0\",\"0|321|-1|0\",\"0|7|-1|0\",\"0|8|-1|1\",\"0|9|-2|3\",\"0|8|-2|2\",\"0|8|-2|2\",\"0|8|-2|2\",\"0|8|-1|2\",\"0|8|-3|2\",\"0|7|-2|3\",\"0|9|-3|3\",\"0|8|-5|5\",\"0|8|-4|4\",\"0|8|-5|3\",\"0|8|-4|3\",\"0|8|-2|3\",\"0|8|-2|1\",\"0|8|0|1\",\"0|8|-1|0\",\"0|7|0|1\",\"0|9|-1|0\",\"0|8|0|1\",\"0|72|0|1\",\"0|8|-1|0\",\"0|7|0|2\",\"0|9|-1|1\",\"0|7|-1|2\",\"0|17|0|2\",\"1|88|0|0\",\"2|104|0|0\",\"0|152|3|0\",\"0|7|5|0\",\"0|9|6|0\",\"0|8|6|0\",\"0|8|7|-1\",\"0|8|9|0\",\"0|7|12|0\",\"0|8|21|0\",\"0|9|25|0\",\"0|7|34|-2\",\"0|9|36|0\",\"0|7|42|0\",\"0|8|47|-2\",\"0|9|46|-4\",\"0|8|45|-5\",\"0|7|44|-8\",\"0|9|43|-5\",\"0|8|52|-10\",\"0|8|40|-6\",\"0|7|35|-7\",\"0|9|41|-7\",\"0|8|32|-5\"],\"b\":80,\"c\":1354,\"d\":199,\"e\":1479,\"f\":0},\"j\":124,\"h\":\"3bc9811c0574daeb38bb5efcff957c37\",\"i\":{\"a\":\"Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36\",\"b\":\"ko-KR\",\"c\":24,\"d\":8,\"e\":1,\"f\":4,\"g\":[1920,1080],\"h\":[1920,1040],\"i\":-540,\"j\":1,\"k\":1,\"l\":1,\"z\":1,\"m\":\"unknown\",\"n\":\"Win32\",\"o\":\"unknown\",\"aa\":[\"Chrome PDF Plugin::Portable Document Format::application/x-google-chrome-pdf~pdf\",\"Chrome PDF Viewer::::application/pdf~pdf\",\"Native Client::::application/x-nacl~,application/x-pnacl~\"],\"p\":\"ab8663d8ae8192907f98c74fa8372720\",\"q\":\"3da28f517a8a5ba182fa1d4991a759ae\",\"r\":\"Google Inc.~ANGLE (NVIDIA GeForce GTX 1060 3GB Direct3D11 vs_5_0 ps_5_0)\",\"s\":false,\"t\":false,\"u\":false,\"v\":false,\"w\":false,\"x\":[0,false,false],\"y\":[\"Arial\",\"Arial Black\",\"Arial Narrow\",\"Calibri\",\"Cambria\",\"Cambria Math\",\"Comic Sans MS\",\"Consolas\",\"Courier\",\"Courier New\",\"Georgia\",\"Helvetica\",\"Impact\",\"Lucida Console\",\"Lucida Sans Unicode\",\"Microsoft Sans Serif\",\"MS Gothic\",\"MS PGothic\",\"MS Sans Serif\",\"MS Serif\",\"Palatino Linotype\",\"Segoe Print\",\"Segoe Script\",\"Segoe UI\",\"Segoe UI Light\",\"Segoe UI Semibold\",\"Segoe UI Symbol\",\"Tahoma\",\"Times\",\"Times New Roman\",\"Trebuchet MS\",\"Verdana\",\"Wingdings\"]}}";
            
            //var t = $"{{\"a\":\"{uuid}\",\"b\":\"1.3.4\",\"d\":[{{\"i\":\"id\",\"b\":{{\"a\":[\"0,{id}\"]}},\"d\":\"{id}\",\"e\":false,\"f\":false}},{{\"i\":\"pw\",\"e\":true,\"f\":false}}],\"h\":\"1f\",\"i\":{{\"a\":\"{userAgent}\"}}}}";
            return t;
        }
        
        public string Generate(NaverLoginForm form, BrowserInfo browserInfo)
        {
            var uuid = Guid.NewGuid() + "-0";
            return Generate(uuid, form, browserInfo);
        }
        
        public string Generate(string uuid, NaverLoginForm form, BrowserInfo browserInfo)
        {
            var encData = generateEncData(uuid, form.Id, browserInfo.UserAgent);
            encData = LZString.CompressToEncodedURIComponent(encData);

            var job = new JObject();
            job.Add("uuid", uuid);
            job.Add("encData", encData);
            return job.ToString();
        }
    }
}