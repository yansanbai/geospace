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
    private Texture2D answer;
    private Text nametext;
    private Text timetext;
    private Button open;
    private Button delete;
/*    public Question(string name,Texture2D img) {
         this.name = name;
         this.image = img;
         open = this.transform.Find("open").gameObject.GetComponent<Button>();
         delete = this.transform.Find("delete").gameObject.GetComponent<Button>();
         open.onClick.AddListener(Open);
         delete.onClick.AddListener(Delete);
    }*/
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
        Debug.Log("打开问题");
        GameObject panel = GameObject.Find("UI/CanvasFront/QusetionPanel/Panel");
        GameObject ques = GameObject.Find("UI/CanvasFront/QusetionPanel/Ques");
        Text tex= GameObject.Find("UI/CanvasFront/QusetionPanel/Ques/name").GetComponent<Text>();
        Image img= GameObject.Find("UI/CanvasFront/QusetionPanel/Ques/Image").GetComponent<Image>();
        panel.SetActive(false);
        ques.SetActive(true);
        tex.text = this.quesname;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        img.sprite = sprite;
    }
    public void Delete()
    {
        Debug.Log("删除问题");
        test.DeleteQuestion(gameObject);
        Destroy(gameObject);
    }
    public void SetAnswer(Texture2D ans) {
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
    public Texture2D GetAnswer()
    {
        return this.answer;
    }
}
