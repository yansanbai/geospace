using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Specialized;

public class Img {
    public string[] image;
}
public class RecognizeController : MonoBehaviour
{
    WritingPanel writingPanel;
    PenBehaviour penBehaviour;

    private static readonly string clientId = "tgalsFoilzd4LTyNDf4Mq7mp";

    private static readonly string clientSecret = "FNfIOtGiAIKnib2FrHHeAaj8Aog98edZ";

    public void Init(WritingPanel writingPanel)
    {
        this.writingPanel = writingPanel;
    }

    public void Update()
    {
        
    }
    private static string FilterChar(string input)
    {
        Regex r = new Regex("^[0-9]{1,}$"); //正则表达式 表示数字的范围 ^符号是开始，$是关闭
        StringBuilder sb = new StringBuilder();
        foreach (var item in input)
        {
            if (item >= 0x4e00 && item <= 0x9fbb)//汉字范围
            {
                sb.Append(item);
            }

            if (Regex.IsMatch(item.ToString(), @"[A-Za-z0-9]"))
            {
                sb.Append(item);
            }
        }
        return sb.ToString();
    }

    //立体几何部分手写文字识别
    public string GetRecognizeResult(string base64)
    {

        try
        {
            string img = WebUtility.UrlEncode(base64);
            string token = GetAccessToken();

            token = new Regex(
                    "\"access_token\":\"(?<token>[^\"]*?)\"",
                    RegexOptions.CultureInvariant
                    | RegexOptions.Compiled
                    ).Match(token).Groups["token"].Value.Trim();

            Debug.Log(token);
            //var url = "https://aip.baidubce.com/rest/2.0/ocr/v1/handwriting";
            string host = "https://aip.baidubce.com/rest/2.0/ocr/v1/handwriting?access_token=" + token;
            Encoding encoding = Encoding.Default;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(host);
            request.Method = "post";
            request.KeepAlive = true;
            String str = "image=" + img;
            byte[] buffer = encoding.GetBytes(str);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);
            string result = reader.ReadToEnd();
            Debug.Log(result);
            // var list = new List<KeyValuePair<string, string>>
            //                {
            //                    new KeyValuePair<string, string>("access_token", token),
            //                    new KeyValuePair<string, string>("image", img),
            //                    new KeyValuePair<string, string>("language_type", "CHN_ENG")
            //                };
            // var data = new List<string>();
            // foreach (var pair in list)
            //     data.Add(pair.Key + "=" + pair.Value);
            // string json = HttpPost(url, string.Join("&", data.ToArray()));
            // Debug.Log(json);
            /*var regex = new Regex(
               "\"words\": \"(?<word>[\\s\\S]*?)\"",
               RegexOptions.CultureInvariant
               | RegexOptions.Compiled
               );
            var recognize = new StringBuilder();
            foreach (Match match in regex.Matches(result))
            {
                recognize.AppendLine(match.Groups["word"].Value.Trim());
            }

            String res = recognize.ToString();*/

            // 去除其中换行符，空格，制表符
            // res = res.Replace("\n", "").Replace(" ","").Replace("\t","").Replace("\r","");
            // 只保留数字、字母、汉字
            /*res = FilterChar(res);*/
            JObject jo = JObject.Parse(result);
            string res = (string)jo["words_result"][0]["words"];
            //Debug.Log(res);
            // 输出测试
            //Debug.Log("识别结果：" + res + ", bytelen=" + System.Text.Encoding.Default.GetByteCount(res) + ", charlen=" + res.Length);
            // char[] arr = res.ToCharArray();

            // 平台判断
            // if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            // {
            //     Debug.Log("Windows 平台");
            //     if (res.Length > 1)
            //     {
            //         //res = res.Substring(0, res.Length - 1);
            //     }
            // }
            // else if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
            // {
            //     Debug.Log("OSX 平台");
            // }


            // return res;
            return res;
        }
        catch (Exception ex)
        {

            Debug.Log(ex.Message);
            return "";
        }
    }

    //
/*    public string TryRecognize(string base64,string host) {
        try
        {
            Encoding encoding = Encoding.Default;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(host);
            request.Method = "post";
            request.KeepAlive = false;
            request.Timeout=10000;

            byte[] buffer = encoding.GetBytes(base64);
            request.ContentLength = buffer.Length;
            Stream stream = request.GetRequestStream();
            stream.Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);
            string str = reader.ReadToEnd();
            Debug.Log(str);
            return str;
        }
        catch (WebException ex)
        {
            Debug.Log(ex);
            return "";
        }
    }*/

    int partcode = 0;
    //函数部分识别公式
    public void GetRecognizeFomula(string base64)
    {
        partcode = 1;
        penBehaviour = GameObject.Find("UI/CanvasFront/WritingPanel/Wrapper").GetComponent<PenBehaviour>();
        WWWForm form = new WWWForm();
        form.AddField("image", base64);
        StartCoroutine(Post("http://120.27.145.45:5000/recognize",form));
    }
    public void GetPositon(string latex)
    {
        partcode = 2;
        penBehaviour = GameObject.Find("UI/CanvasFront/WritingPanel/Wrapper").GetComponent<PenBehaviour>();
        WWWForm form = new WWWForm();
        form.AddField("latex", latex);
        StartCoroutine(Post("http://120.27.145.45:5000/getDataSet", form));
    }
    //函数部分识别系数常量变换
    public void GetRecognizeChange(string latex, string fomula)
    {
        Debug.Log(latex);
        Debug.Log(fomula);
        partcode = 3;
        penBehaviour = GameObject.Find("UI/CanvasFront/WritingPanel/Wrapper").GetComponent<PenBehaviour>();
        WWWForm form = new WWWForm();
        form.AddField("transform_content", latex);
        form.AddField("latex", fomula);
        StartCoroutine(Post("http://120.27.145.45:5000/transform", form));
    }

    IEnumerator Post(string host, WWWForm form)
    {
        //请求链接，并将form对象发送到远程服务器
        UnityWebRequest webRequest = UnityWebRequest.Post(host,form);
        yield return webRequest.SendWebRequest();
        string result = "";
        string image = "";
        Vector3[] positions = new Vector3[10];
        if (webRequest.isHttpError || webRequest.isNetworkError)
        {
            Debug.Log(webRequest.error);
            //penBehaviour.ExecuteFomula(result, positions, image);
        }
        else
        {
            HandleData(webRequest.downloadHandler.text);
        }
    }

    private  void HandleData(string data) {
        Debug.Log(data);
        JObject jo = JObject.Parse(data);
        string result = "";
        string image = "";
        Vector3[] positions = new Vector3[10];
        if ((int)jo["code"] != -1)
        {
            if (jo["data"]["latex"]!=null) {
                result = (string)jo["data"]["latex"];
            }
            if (jo["data"]["points"] != null)
            {
                int length = ((JArray)jo["data"]["points"]).Count;
                positions = new Vector3[length];
                for (int i = 0; i < length; i++)
                {
                    positions[i][0] = (float)jo["data"]["points"][i][0];
                    positions[i][1] = (float)jo["data"]["points"][i][1];
                    positions[i][2] = 0;
                }
            }
        }
        if (partcode == 1)
        {
            penBehaviour.ExecuteFomula(result);
        }
        else if (partcode == 2)
        {
            penBehaviour.SetPositions(positions);
        }
        else 
        {
            penBehaviour.ExecuteChange(result, positions);
        }
    }

   /* public string GetRecognizeTick(string base64)
    {
        string str= TryRecognize(base64, "http://121.196.158.2:5000/circle_tick_recongize");
        JObject jo = JObject.Parse(str);
        string res = (string)jo["data"]["result"];
        return res;
    }*/

    public string GetAccessToken()
    {
        string url = "https://aip.baidubce.com/oauth/2.0/token";
        var list = new List<KeyValuePair<string, string>>
                           {
                               new KeyValuePair<string, string>("grant_type", "client_credentials"),
                               new KeyValuePair<string, string>("client_id", clientId),
                               new KeyValuePair<string, string>("client_secret", clientSecret)
                           };
        var data = new List<string>();
        foreach (var pair in list)
            data.Add(pair.Key + "=" + pair.Value);

        var res = StartCoroutine(GetRequest(url, string.Join("&", data.ToArray())));
        
        return HttpGet(url, string.Join("&", data.ToArray()));
    }

    public IEnumerator GetRequest(string uri, string data)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri + (data == "" ? "" : "?") + data))
        {
            Debug.Log(uri + (data == "" ? "" : "?") + data);
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else
            {
                // Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.data);
                Debug.Log(webRequest.downloadHandler.text);

            }
        }
    }




    public static string HttpGet(string url, string data)
    {
        var request = (HttpWebRequest)WebRequest.Create(url + (data == "" ? "" : "?") + data);
        request.Method = "GET";
        request.ContentType = "text/html;charset=UTF-8";
        using (var response = (HttpWebResponse)request.GetResponse())
        {
            Stream stream = response.GetResponseStream();
            string s = null;
            if (stream != null)
            {
                using (var reader = new StreamReader(stream, Encoding.GetEncoding("utf-8")))
                {
                    s = reader.ReadToEnd();
                    reader.Close();
                }
                stream.Close();
            }
            return s;
        }
    }

    public static string HttpPost(string url, string data)
    {
        var request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "POST";
        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = Encoding.UTF8.GetByteCount(data);
        Stream stream = request.GetRequestStream();
        var writer = new StreamWriter(stream, Encoding.GetEncoding("gb2312"));
        writer.Write(data);
        writer.Close();

        using (var response = (HttpWebResponse)request.GetResponse())
        {
            Stream res = response.GetResponseStream();
            if (res != null)
            {
                var reader = new StreamReader(res, Encoding.GetEncoding("utf-8"));
                string retString = reader.ReadToEnd();
                reader.Close();
                res.Close();
                return retString;
            }
        }
        return "";
    }


    public static String getFileBase64(String fileName)
    {
        FileStream filestream = new FileStream(fileName, FileMode.Open);
        byte[] arr = new byte[filestream.Length];
        filestream.Read(arr, 0, (int)filestream.Length);
        string baser64 = Convert.ToBase64String(arr);
        filestream.Close();
        return baser64;
    }


}