using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Question : MonoBehaviour
{
    public TestController test;
    private string quesname;
    private Texture2D texture;
    private string answer;
    private Text nametext;
    private Text timetext;
    private Button open;
    private Button delete;

    public void Start()
    {
        
    }
    public void SetData(string name, Texture2D img) {
        this.quesname = name;
        this.texture = img;
        nametext = this.transform.Find("name").gameObject.GetComponent<Text>();
        timetext = this.transform.Find("time").gameObject.GetComponent<Text>();
        nametext.text = name;
        DateTime NowTime = DateTime.Now.ToLocalTime();
        timetext.text= NowTime.ToString("yyyy年MM月dd日 HH:mm:ss");
        open = this.transform.Find("open").gameObject.GetComponent<Button>();
        delete = this.transform.Find("delete").gameObject.GetComponent<Button>();
        open.onClick.AddListener(Open);
        delete.onClick.AddListener(Delete);
    }
    public void Open() {
        test.currentQus = this;
        GameObject panel = GameObject.Find("UI/CanvasFront/QusetionPanel/Content/Panel");
        GameObject ques = GameObject.Find("UI/CanvasFront/QusetionPanel/Content/Ques");
        panel.SetActive(false);
        ques.SetActive(true);
        Text tex= GameObject.Find("UI/CanvasFront/QusetionPanel/Content/Ques/name").GetComponent<Text>();
        Image img= GameObject.Find("UI/CanvasFront/QusetionPanel/Content/Ques/Image").GetComponent<Image>();
        tex.text = this.quesname;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        img.sprite = sprite;
        img.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ((float)texture.height / (float)texture.width) * 1400);
    }
    public void Delete()
    {
        test.DeleteQuestion(gameObject);
        Destroy(gameObject);
    }
    public void SetAnswer(string ans) {
        this.answer = ans;
    }
    public string GetName()
    {
        return this.quesname;
    }
    public Texture2D GetImage()
    {
        return this.texture;
    }
    public string GetAnswer()
    {
        return this.answer;
    }
}
