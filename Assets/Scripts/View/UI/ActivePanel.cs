using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivePanel : MonoBehaviour
{
    public Sprite VertexIcon;
    public Sprite EdgeIcon;
    public Sprite FaceIcon;


    Image icon;
    Text text;


    public void Init()
    {
        icon = transform.Find("Icon").GetComponent<Image>();
        text = transform.Find("Text").GetComponent<Text>();

        Clear();
    }

    public void Clear()
    {
        gameObject.SetActive(false);
        icon.sprite = null;
        text.text = "";
    }

    public void SetVertex(FormElement vertex)
    {
        if (vertex.count != 1)
            return;

        icon.sprite = VertexIcon;
        text.text = vertex.ToString();

        gameObject.SetActive(true);
    }

    public void SetEdge(FormElement edge)
    {
        if (edge.count != 2)
            return;

        icon.sprite = EdgeIcon;
        text.text = edge.ToString();

        gameObject.SetActive(true);
    }

    public void SetFace(FormElement face)
    {
        if (face.count < 3)
            return;

        icon.sprite = FaceIcon;
        text.text = face.ToString();

        gameObject.SetActive(true);
    }


}
