using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolPanel : MonoBehaviour
{
    public ButtonGroup ButtonGroupPrefab;
    public GameObject ToolTipPrefab;


    public Action<ToolGroupType, Tool> OnClickTool;


    ObjectPool<ButtonGroup> buttonGroupPool;
    GameObject buttonGroupPoolObject;


    List<ButtonGroup> buttonGroups;
    // Dictionary<int, ButtonGroup> buttonGroupMap;
    public List<ToolGroup> toolGroups;

    GameObject toolTip;


    public void Init()
    {
        // Object Pool
        buttonGroupPool = new ObjectPool<ButtonGroup>(CreateButtonGroup, ResetButtonGroup);
        buttonGroupPoolObject = new GameObject("ButtonGroupPool");
        buttonGroupPoolObject.transform.position = Vector3.zero;
        buttonGroupPoolObject.SetActive(false);
        buttonGroupPoolObject.transform.SetParent(transform);


        buttonGroups = new List<ButtonGroup>();

        toolTip = GameObject.Instantiate(ToolTipPrefab);
        toolTip.transform.SetParent(transform, false);

        toolTip.SetActive(false);


        StyleManager.OnStyleChange += () =>
        {
            SetTintColor();
        };

    }

    public void SetToolGroups(List<ToolGroup> toolGroups)
    {
        this.toolGroups = toolGroups;
    }

    public void RefreshButtonGroups()
    {
        for (int i = toolGroups.Count; i < buttonGroups.Count; i++)
            buttonGroupPool.PutObject(buttonGroups[i]);
        int removeCount = buttonGroups.Count - toolGroups.Count;
        if (removeCount >= 0)
            buttonGroups.RemoveRange(toolGroups.Count, removeCount);
        else
            for (int i = removeCount; i < 0; i++)
                buttonGroups.Add(null);


        for (int i = 0; i < toolGroups.Count; i++)
        {
            ButtonGroup buttonGroup = buttonGroups[i];
            if (buttonGroup == null)
            {
                buttonGroup = buttonGroupPool.GetObject();
                buttonGroups[i] = buttonGroup;
            }
            ButtonGroupAtIndex(buttonGroup, i);
        }

        // SetTintColor();
        RefreshLayout();

        toolTip.transform.SetAsLastSibling();
    }

    private void ButtonGroupAtIndex(ButtonGroup buttonGroup, int i)
    {
        buttonGroup.name = toolGroups[i].Name + "ButtonGroup";
        buttonGroup.transform.SetParent(transform, false);

        buttonGroup.SetIcon(toolGroups[i].Icon);

        buttonGroup.CountOfButtons = () => toolGroups[i].Tools.Count;
        buttonGroup.ButtonAtIndex = (button, buttonIndex) => ButtonAtGroup(button, i, buttonIndex);
        buttonGroup.RefreshButtons();

        buttonGroup.SetTintColor(StyleManager.Themes[toolGroups[i].Color]);

        buttonGroup.OnStateChange = RefreshLayout;
    }

    public ButtonGroup CreateButtonGroup()
    {
        GameObject groupObject = GameObject.Instantiate(ButtonGroupPrefab.gameObject);
        groupObject.transform.SetParent(buttonGroupPoolObject.transform, false);
        ButtonGroup buttonGroup = groupObject.GetComponent<ButtonGroup>();
        buttonGroup.Init();

        return buttonGroup;
    }

    public void ResetButtonGroup(ButtonGroup buttonGroup)
    {
        buttonGroup.transform.SetParent(buttonGroupPoolObject.transform, false);
    }

    private void ButtonAtGroup(ToolButton button, int groupIndex, int buttonIndex)
    {
        Tool tool = toolGroups[groupIndex].Tools[buttonIndex];

        button.gameObject.name = tool.Name;
        button.SetIcon(tool.Icon);

        ToolGroupType type = toolGroups[groupIndex].Type;
        button.OnClick = () =>
        {
            if (OnClickTool != null)
                OnClickTool(type, tool);
        };

        button.OnEnter = () =>
        {
            float buttonPosX = buttonGroups[groupIndex].GetPosX() + button.GetPosX();
            SetTip(tool.Description, buttonPosX);
            toolTip.SetActive(true);
        };

        button.OnExit = () =>
        {
            toolTip.SetActive(false);
        };

    }

    private void SetTip(string tip, float buttonPosX)
    {
        Font font = Resources.Load<Font>(UIConstants.TextFont);

        int fontsize = (int)UIConstants.TextFontSize;
        font.RequestCharactersInTexture(tip, fontsize, FontStyle.Normal);
        CharacterInfo characterInfo;
        float width = 0f;
        for (int i = 0; i < tip.Length; i++)
        {
            font.GetCharacterInfo(tip[i], out characterInfo, fontsize);
            width += characterInfo.advance;
        }


        Text textField = toolTip.transform.Find("Text").GetComponent<Text>();
        textField.text = tip;

        // float width = textField.cachedTextGenerator.GetPreferredWidth(tip, textField.GetGenerationSettings(GetComponent<RectTransform>().rect.size));

        width += UIConstants.ToolTipSpacing;
        RectTransform rt = toolTip.GetComponent<RectTransform>();
        float posX = buttonPosX + UIConstants.ToolButtonWidth / 2 + UIConstants.ToolButtonSpacing;
        float posY = UIConstants.ToolPlaneHeight / 2 + UIConstants.ToolButtonSpacing;
        rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, posX, width);
        rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, posY, rt.sizeDelta.y);
    }

    private void RefreshLayout()
    {
        float posX = 0;
        for (int i = 0; i < buttonGroups.Count; i++)
        {
            buttonGroups[i].SetPosX(posX);
            posX += buttonGroups[i].GetWidth();
        }
    }

    private void SetTintColor()
    {
        for (int i = 0; i < buttonGroups.Count; i++)
        {
            ToolGroup toolGroup = toolGroups[i];
            buttonGroups[i].SetTintColor(StyleManager.Themes[toolGroup.Color]);
        }
    }

    public List<ToolGroup> ToolGroups()
    {
        return toolGroups;
    }
}
