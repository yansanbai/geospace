using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

public class RecognizePanel : MonoBehaviour
{
    InputField input;
    InputPanel inputPanel;
    GameObject showFomula;
    TEXDraw texdraw;
    UnityEngine.UI.Image image;

    public void Init()
    {
        input = transform.Find("InputField").GetComponent<InputField>();
        inputPanel = GameObject.Find("/UI/CanvasBack").transform.Find("InputPanel").GetComponent<InputPanel>();
        texdraw= transform.Find("TEXDraw").GetComponent<TEXDraw>();
        Clear();
    }

    public void AddWord(string str)
    {
        input.text += str;
    }

    public void SetWord(string str)
    {
        input.text = str;
    }

    public void SetLatex(string str)
    {
        texdraw.text = str ;
    }

/*    public void AddImage(string base64) {
        showFomula.SetActive(true);
        Debug.Log(" ‰»Î¿∏ÃÌº”Õº∆¨");
        byte[] data = System.Convert.FromBase64String(base64);
        FileStream file = File.Open(Application.dataPath + "/temp/fomula.png", FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(data);
        file.Close();
        file.Dispose();
        //GameObject.Find("UI/CanvasFront/Text").GetComponent<Text>().text = Application.dataPath + "/temp/fomula.png";
        FileStream fs = new FileStream(Application.dataPath + "/temp/fomula.png", FileMode.Open, FileAccess.Read);
        int byteLength = (int)fs.Length;
        byte[] imgBytes = new byte[byteLength];
        fs.Read(imgBytes, 0, byteLength);
        fs.Close();
        fs.Dispose();
        Texture2D texture = new Texture2D(200, 60, TextureFormat.RGBA32, false);
        texture.LoadImage(imgBytes);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        image.sprite = sprite;

        image.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(((float)texture.width / (float)texture.height) * 60, 60);
        image.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(((float)texture.width / (float)texture.height) * 60 - 840,0,0);
    }*/

/*    public void AddEmpty()
    {
        showFomula.SetActive(true);
        FileStream fs = new FileStream(Application.dataPath + "/temp/empty.png", FileMode.Open, FileAccess.Read);
        int byteLength = (int)fs.Length;
        byte[] imgBytes = new byte[byteLength];
        fs.Read(imgBytes, 0, byteLength);
        fs.Close();
        fs.Dispose();
        Texture2D texture = new Texture2D(200, 60, TextureFormat.RGBA32, false);
        texture.LoadImage(imgBytes);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        image.sprite = sprite;
        image.gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (texture.width / texture.height) * 60);
        image.gameObject.GetComponent<RectTransform>().localPosition = new Vector3((texture.width / texture.height) * 60 - 840, 0, 0);
    }*/
    public void Clear()
    {
        gameObject.SetActive(false);
        input.text = "";
        texdraw.text = "";
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
