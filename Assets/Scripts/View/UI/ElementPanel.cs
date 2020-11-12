using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElementPanel : MonoBehaviour
{
    public ButtonBoard ButtonBoardPrefab;
    public SignBoard SignBoardPrefab;

    public Sprite SignIcon;
    public Sprite ShowIcon;
    public Sprite HideIcon;
    public Sprite DeleteIcon;
    public Sprite CoordinateIcon;

    public Func<string> OnSignDefault;
    public Action<GeoElement, int> OnElementClickColor;
    public Action<GeoElement, int> OnElementClickStyle;
    public Action<bool> OnSignButtonChange; // open sign board or not

    public Action<string> OnSignInputChanged;
    public Func<string, bool> OnSignInputValidate;

    public Func<bool> OnElementVisible;
    public Action<bool> OnElementClickDisplay;
    public Action OnElementClickDelete;
    public Action OnElementClickCoordinate;

    public Action OnClose;

    private Overlay overlay;
    private RectTransform wrapper;

    private ButtonBoard rootBoard;
    private GameObject childBoard;
    private ButtonBoardCell activeRootButton;

    private GeoElement element;

    public void Init()
    {
        overlay = GetComponent<Overlay>();
        overlay.Init();
        wrapper = transform.Find("Wrapper").GetComponent<RectTransform>();

        overlay.SetActive(false);
        overlay.OnClick = HandleOverlayClick;
    }

    public void SetPositionByAnchor(Vector2 anchor)
    {
        wrapper.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, anchor.x + UIConstants.ElementBoardSpacing, 0);
        wrapper.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, anchor.y - UIConstants.ElementBoardSpacing, 0);
    }

    private void HandleOverlayClick()
    {
        Close();
    }

    public void Close()
    {
        if (activeRootButton)
            activeRootButton.SetActive(false);

        if (rootBoard)
            Destroy(rootBoard.gameObject);
        if (childBoard)
            Destroy(childBoard.gameObject);

        overlay.SetActive(false);

        if (OnClose != null)
            OnClose();
    }

    private delegate void ButtonAtRoot(ButtonBoardCell button);

    public void SetVertex(GeoVertex vertex)
    {
        element = vertex;

        ButtonBoard buttonBoard = InitRootButtonBoard();

        List<ButtonAtRoot> buttonAtRoot = new List<ButtonAtRoot>();
        buttonAtRoot.Add(ColorButton);
        buttonAtRoot.Add(StyleButton);
        buttonAtRoot.Add(DisplayButton);
        buttonAtRoot.Add(SignButton);

        if (!vertex.isBased)
            buttonAtRoot.Add(DeleteButton);

        if (vertex.isSpace)
            buttonAtRoot.Add(CoordinateButton);

        buttonBoard.CountOfButtons = () => buttonAtRoot.Count;
        buttonBoard.ButtonAtIndex = (button, i) => buttonAtRoot[i](button);
        buttonBoard.InitButtons();

        overlay.SetActive(true);
    }

    public void SetEdge(GeoEdge edge)
    {
        element = edge;

        ButtonBoard buttonBoard = InitRootButtonBoard();

        List<ButtonAtRoot> buttonAtRoot = new List<ButtonAtRoot>();
        buttonAtRoot.Add(ColorButton);
        buttonAtRoot.Add(StyleButton);
        buttonAtRoot.Add(DisplayButton);
        buttonAtRoot.Add(SignButton);

        if (!edge.isBased)
            buttonAtRoot.Add(DeleteButton);

        buttonBoard.CountOfButtons = () => buttonAtRoot.Count;
        buttonBoard.ButtonAtIndex = (button, i) => buttonAtRoot[i](button);
        buttonBoard.InitButtons();

        overlay.SetActive(true);
    }

    public void SetFace(GeoFace face)
    {
        element = face;

        ButtonBoard buttonBoard = InitRootButtonBoard();


        List<ButtonAtRoot> buttonAtRoot = new List<ButtonAtRoot>();
        buttonAtRoot.Add(ColorButton);
        buttonAtRoot.Add(StyleButton);
        buttonAtRoot.Add(DisplayButton);
        buttonAtRoot.Add(SignButton);

        if (!face.isBased)
            buttonAtRoot.Add(DeleteButton);

        buttonBoard.CountOfButtons = () => buttonAtRoot.Count;
        buttonBoard.ButtonAtIndex = (button, i) => buttonAtRoot[i](button);
        buttonBoard.InitButtons();

        overlay.SetActive(true);
    }

    private void ButtonToggleActive(ButtonBoardCell button)
    {
        bool isActive = button.IsActive();

        if (activeRootButton)
            activeRootButton.SetActive(false);
        if (childBoard)
            Destroy(childBoard);
        activeRootButton = null;

        if (!isActive)
        {
            button.SetActive(true);
            activeRootButton = button;
        }
    }

    private void ColorButton(ButtonBoardCell button)
    {
        button.SetIcon(null);
        button.SetColor(ColorOfIndex(element.color));

        button.OnClick += () =>
        {
            ButtonToggleActive(button);
        };

        button.OnActiveChanged += (active) =>
        {
            if (!active)
                return;
            ButtonBoard colorBoard = InitChildButtonBoard();
            colorBoard.CountOfButtons = () => StyleManager.Themes.Length + 1;
            colorBoard.ButtonAtIndex = (childButton, index) => ButtonAtColorBoard(button, childButton, index);
            colorBoard.InitButtons();
        };
    }

    private void StyleButton(ButtonBoardCell button)
    {
        button.SetIcon(StyleOfIndex(element.style).Icon);
        button.SetColor(ColorOfIndex(0));

        button.OnClick += () =>
        {
            ButtonToggleActive(button);
        };

        button.OnActiveChanged += (active) =>
        {
            if (!active)
                return;
            ButtonBoard styleBoard = InitChildButtonBoard();
            styleBoard.CountOfButtons = () => ElementStyle(element).Count;
            styleBoard.ButtonAtIndex = (childButton, index) => ButtonAtStyleBoard(button, childButton, index);
            styleBoard.InitButtons();
        };
    }

    private void SignButton(ButtonBoardCell button)
    {
        button.SetIcon(SignIcon);
        button.SetColor(StyleManager.Text);

        button.OnClick += () =>
        {
            ButtonToggleActive(button);
        };

        button.OnActiveChanged += (active) =>
        {
            if (OnSignButtonChange != null)
                OnSignButtonChange(active);

            if (!active)
                return;
            SignBoard signBoard = InitChildSignBoard();
            signBoard.OnInputChanged = OnSignInputChanged;
            signBoard.OnValidate = OnSignInputValidate;

            if (OnSignDefault != null)
                signBoard.SetSign(OnSignDefault());
        };
    }

    private void DisplayButton(ButtonBoardCell button)
    {
        bool visible = OnElementVisible();

        button.SetIcon(visible ? ShowIcon : HideIcon);
        button.SetColor(StyleManager.Text);

        button.OnClick += () =>
        {
            if (OnElementClickDisplay != null)
                OnElementClickDisplay(!visible);

            visible = OnElementVisible();
            button.SetIcon(visible ? ShowIcon : HideIcon);
        };
    }

    private void DeleteButton(ButtonBoardCell button)
    {
        button.SetIcon(DeleteIcon);
        button.SetColor(StyleManager.Text);

        button.OnClick += () =>
        {
            if (OnElementClickDelete != null)
                OnElementClickDelete();
        };
    }

    private void CoordinateButton(ButtonBoardCell button)
    {
        button.SetIcon(CoordinateIcon);
        button.SetColor(StyleManager.Text);

        button.OnClick += () =>
        {
            if (OnElementClickCoordinate != null)
                OnElementClickCoordinate();
        };
    }

    private void ButtonAtColorBoard(ButtonBoardCell rootButton, ButtonBoardCell button, int buttonIndex)
    {
        button.SetIcon(null);
        Color color = ColorOfIndex(buttonIndex);
        button.SetColor(color);

        button.OnClick += () =>
        {
            rootButton.SetColor(color);

            if (OnElementClickColor != null)
                OnElementClickColor(element, buttonIndex);
        };
    }

    private void ButtonAtStyleBoard(ButtonBoardCell rootButton, ButtonBoardCell button, int buttonIndex)
    {
        button.SetIcon(StyleOfIndex(buttonIndex).Icon);
        button.SetColor(ColorOfIndex(0));

        button.OnClick += () =>
        {
            rootButton.SetIcon(StyleOfIndex(buttonIndex).Icon);

            if (OnElementClickStyle != null)
                OnElementClickStyle(element, buttonIndex);
        };
    }

    private ButtonBoard InitRootButtonBoard()
    {
        GameObject boardObject = GameObject.Instantiate(ButtonBoardPrefab.gameObject);
        boardObject.transform.SetParent(wrapper.transform, false);
        ButtonBoard buttonBoard = boardObject.GetComponent<ButtonBoard>();
        buttonBoard.Init();

        rootBoard = buttonBoard;
        return buttonBoard;
    }
    private ButtonBoard InitChildButtonBoard()
    {
        GameObject boardObject = GameObject.Instantiate(ButtonBoardPrefab.gameObject);
        boardObject.transform.SetParent(wrapper.transform, false);
        RectTransform rectTransform = boardObject.GetComponent<RectTransform>();
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, UIConstants.ElementBottonHeight + UIConstants.ElementBoardSpacing, rectTransform.sizeDelta.y);
        ButtonBoard buttonBoard = boardObject.GetComponent<ButtonBoard>();
        buttonBoard.Init();

        childBoard = buttonBoard.gameObject;
        return buttonBoard;
    }

    private SignBoard InitChildSignBoard()
    {
        GameObject boardObject = GameObject.Instantiate(SignBoardPrefab.gameObject);
        boardObject.transform.SetParent(wrapper.transform, false);
        RectTransform rectTransform = boardObject.GetComponent<RectTransform>();
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, UIConstants.ElementBottonHeight + UIConstants.ElementBoardSpacing, rectTransform.sizeDelta.y);
        SignBoard signBoard = boardObject.GetComponent<SignBoard>();
        signBoard.Init();

        childBoard = signBoard.gameObject;
        return signBoard;
    }

    private Color ColorOfIndex(int index)
    {
        return index == 0 ? DefaultColor(element) : StyleManager.Themes[index - 1];
    }

    private Color DefaultColor(GeoElement element)
    {
        if (element is GeoVertex)
            return StyleManager.Point;
        else if (element is GeoEdge)
            return StyleManager.Line;
        else if (element is GeoFace)
            return StyleManager.Plane;

        return Color.white;
    }

    private ElementStyle StyleOfIndex(int index)
    {
        List<ElementStyle> list = ElementStyle(element);

        if (list != null)
            return list[index];

        return null;
    }

    private List<ElementStyle> ElementStyle(GeoElement element)
    {
        if (element is GeoVertex)
            return ConfigManager.VertexStyle;
        else if (element is GeoEdge)
            return ConfigManager.EdgeStyle;
        else if (element is GeoFace)
            return ConfigManager.FaceStyle;

        return null;
    }
}
