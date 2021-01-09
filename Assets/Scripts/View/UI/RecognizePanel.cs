using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System.Drawing;

public class RecognizePanel : MonoBehaviour
{
    InputField input;
    InputPanel inputPanel;
    GameObject showFomula;
    UnityEngine.UI.Image image;

    public void Init()
    {
        input = transform.Find("InputField").GetComponent<InputField>();
        inputPanel = GameObject.Find("/UI/CanvasBack").transform.Find("InputPanel").GetComponent<InputPanel>();
        Clear();
    }

    public void AddWord(string str)
    {
        input.text += str;
    }

    public void AddImage(string base64) {
        Debug.Log(" ‰»Î¿∏ÃÌº”Õº∆¨");
        showFomula = gameObject.transform.Find("ShowFomula").gameObject;
        showFomula.SetActive(true);
        image=showFomula.transform.Find("Image").gameObject.GetComponent<UnityEngine.UI.Image>();
        byte[] data = System.Convert.FromBase64String(base64);
        FileStream file = File.Open(Application.dataPath + "/temp/fomula/1.png", FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(data);
        file.Close();
        file.Dispose();
        FileStream fs = new FileStream(Application.dataPath + "/temp/fomula/1.png", FileMode.Open, FileAccess.Read);
        int byteLength = (int)fs.Length;
        byte[] imgBytes = new byte[byteLength];
        fs.Read(imgBytes, 0, byteLength);
        fs.Close();
        fs.Dispose();
        System.Drawing.Image img = System.Drawing.Image.FromStream(new MemoryStream(imgBytes));
        Texture2D texture = new Texture2D((img.Width/img.Height)*60, 60, TextureFormat.RGBA32, false);
        texture.LoadImage(imgBytes);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        image.sprite = sprite;
        image.gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (img.Width / img.Height) * 60);
        image.gameObject.GetComponent<RectTransform>().position = new Vector3(50+ texture.width/2,0,0);
    }
    public void Clear()
    {
        gameObject.SetActive(false);
        input.text = "";
    }

    public void showRecognizePanel()
    {
        gameObject.SetActive(true);
        inputPanel.Clear();
    }

    public string GetWords()
    {
        return input.text;
    }

    public void OnDestroy()
    {
    }
}
