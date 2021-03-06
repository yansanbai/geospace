﻿using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Reflection;
using UnityEngine.UI;

public class readImage : MonoBehaviour
{
    private TestController test;
    private static readImage _instance;
    public static readImage Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        _instance = this;
        test = GameObject.Find("TestController").GetComponent<TestController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onRead()
    {
        OpenFileName ofn = new OpenFileName();

        ofn.structSize = Marshal.SizeOf(ofn);

        ofn.filter = "图片文件(*.jpg*.png)\0*.jpg;*.png";

        ofn.file = new string(new char[256]);

        ofn.maxFile = ofn.file.Length;

        ofn.fileTitle = new string(new char[64]);

        ofn.maxFileTitle = ofn.fileTitle.Length;
        string path = Application.streamingAssetsPath;
        path = path.Replace('/', '\\');
        //默认路径
        ofn.initialDir = path;

        ofn.title = "Open Project";

        ofn.defExt = "JPG";//显示文件的类型
                           //注意 一下项目不一定要全选 但是0x00000008项不要缺少
        ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR

        if (WindowDll.GetOpenFileName(ofn))
        {
            StartCoroutine(Load(ofn.file));
        }

    }
    IEnumerator Load(string path)
    {
        WWW www = new WWW("file:///" + path);
            Texture2D texture = www.texture;
            string[] array = path.Split('\\');
            array = array[array.Length - 1].Split('.');
            string name = array[0];
            test.CreateQuestion(name,texture);
        yield return www;
    }
}