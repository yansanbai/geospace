using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XANTools;

public class TestController : MonoBehaviour
{
    private List<GameObject> questions;
    public GameObject QuestionPanel;
    public GameObject Content;
    public GameObject Panel;
    public GameObject Question;
    private int index;
    public GameObject queprefab;
    public ResizePanel resize;
    private GeoController geoController;
    public Question currentQus;
    
    // Start is called before the first frame update
    void Start()
    {
        index = 0;
        questions = new List<GameObject>();
        geoController = GameObject.Find("/GeoController").GetComponent<GeoController>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ShowQuestionPanel() {
        //显示试题列表
        QuestionPanel.SetActive(true);
    }
    public void PackupQuestionPanel()
    {
        //收起试题集面板
        if (Content.activeSelf)
        {
            Content.SetActive(false);
            resize.SetResize(false);
            //Content.transform.localPosition = new Vector3(0,0,0);
        }
        else {
            Content.SetActive(true);
            resize.SetResize(true);
            //Content.transform.localPosition = new Vector3(0, 0, 0);
        }
    }
    public void ShutQuestionPanel()
    {
        //关闭试题集
        QuestionPanel.SetActive(false);
        Clear();
    }
    public void AddQuestion()
    {
        //添加试题
        readImage.Instance.onRead();
    }
    public void CreateQuestion(string name,Texture2D texture)
    {
        index++;
        //创建question物体
        GameObject ques = GameObject.Instantiate(queprefab);
        ques.name = "question" + index;
        ques.transform.parent=Panel.transform;
        ques.transform.localScale = new Vector3(1, 1, 1);
        //ques.transform.localPosition = new Vector3(0,240-80*index,0);
        
        Question question = ques.GetComponent<Question>();
        question.SetData(name,texture);
        questions.Add(ques);
        ques.SetActive(true);
        Refresh();
    }
    public void DeleteQuestion(GameObject question) {
        //删除试题
        questions.Remove(question);
        Refresh();
    }
    public void Refresh() {
        //刷新试题列表
        for (int i = 0; i < questions.Count; i++)
        {
            questions[i].transform.localPosition = new Vector3(0, 160 - 80 * i, 0);
        }
    }
    public void BackQuestion()
    {
        //回到试题列表
        Question.SetActive(false);
        Panel.SetActive(true);
    }
    public void SaveQuestion()
    {
        //保存操作
        //currentQus.SetAnswer(JsonConvert.SerializeObject(geoController.record));
        //Debug.Log(JsonConvert.SerializeObject(new Vector3(0,0,0)));
        Debug.Log(new Vector3(0, 0, 0));
        GameObject.Find("/UI/CanvasFront/Text").GetComponent<Text>().text = JsonConvert.SerializeObject(geoController.record);
    }
    public void Recover()
    {
        //恢复操作
        ArrayList ans= JsonConvert.DeserializeObject<ArrayList>(currentQus.GetAnswer());
        Debug.Log(ans.Count);
        for (int i = 0; i < ans.Count; i++)
        {
            Record rec = (Record)ans[i];
            switch (rec.type)
            {
                case ToolGroupType.Geometry:
                    geoController.GeometryOperation(rec.tool);
                    break;
                case ToolGroupType.Condition:
                    geoController.AddConditionOperation(rec.tool);
                    break;
                case ToolGroupType.Auxiliary:
                    geoController.AddAuxiliaryOperation(rec.tool);
                    break;
                case ToolGroupType.Measure:
                    geoController.AddMeasureOperation(rec.tool);
                    break;
            }
        }
    }
    public void Clear() {
        QuestionPanel.transform.localScale = new Vector3(1, 1, 1);
        QuestionPanel.transform.localPosition = new Vector3(-150, 0, 0);
        resize.Clear();
        Question.SetActive(false);
        Panel.SetActive(true);
    }
}
