using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatePanel : MonoBehaviour
{
    public StateCell StateCellPrefab;

    Dictionary<State, StateCell> stateCellsMap;

    public void Init()
    {
        stateCellsMap = new Dictionary<State, StateCell>();
    }

    public void AddStateCell(State state, int color)
    {
        StateCell stateCell = InitStateCell(state);

        stateCellsMap.Add(state, stateCell);

        stateCell.OnClickDelete = () =>
        {
            if (state.OnClickDelete != null)
                state.OnClickDelete();
        };

        stateCell.UndoFaceHighlight = () =>
        {
            if (state.UndoFaceHighlight != null)
                state.UndoFaceHighlight();
        };

        stateCell.OnElementHighlight = () =>
        {   
            if (state.OnElementHighlight != null)
                state.OnElementHighlight();
        };
        


        
        stateCell.DoubleClick = () =>
        {
            if(state.DoubleClick != null)
                state.DoubleClick();
        };
        

        RefreshStateCell(state);

        SetCellTintColor(stateCell, color);
        StyleManager.OnStyleChange += () =>
        {
            SetCellTintColor(stateCell, color);
        };

    }

    public void RemoveStateCell(State state)
    {
        StateCell stateCell = stateCellsMap[state];

        stateCellsMap.Remove(state);

        Destroy(stateCell.gameObject);
    }

    private StateCell InitStateCell(State state)
    {
        GameObject cellObject = GameObject.Instantiate(StateCellPrefab.gameObject);
        cellObject.name = state.tool.Name;
        cellObject.transform.SetParent(transform, false);

        StateCell stateCell = cellObject.GetComponent<StateCell>();
        stateCell.Init();

        return stateCell;
    }

    public void ClearStateCells()
    {
        foreach (KeyValuePair<State, StateCell> pair in stateCellsMap)
            Destroy(pair.Value.gameObject);

        stateCellsMap.Clear();
    }

    public void RefreshStateCells(IEnumerable<State> states)
    {
        foreach (State state in states)
            RefreshStateCell(state);
    }

    public void RefreshStateCell(State state)
    {
        StateCell stateCell = stateCellsMap[state];

        stateCell.SetIcon(state.tool.Icon);
        stateCell.SetForm(state.Title());
    }

    public void RefreshLayout(List<State> states)
    {
        float posY = 0;

        for (int i = 0; i < states.Count; i++)
        {
            StateCell stateCell = stateCellsMap[states[i]];
            stateCell.SetPosY(posY);
            posY += stateCell.GetHeight();
        }
    }

    private void SetCellTintColor(StateCell stateCell, int color)
    {
        stateCell.SetTintColor(StyleManager.Themes[color]);
    }

}