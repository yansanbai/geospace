using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour
{
    StatePanel statePanel;

    List<State> geometryStates;
    Dictionary<Geometry, GeometryState> geometryMap;
    List<State> conditionStates;
    Dictionary<Condition, ConditionState> conditionMap;
    List<State> auxiliaryStates;
    Dictionary<Auxiliary, AuxiliaryState> auxiliaryMap;

    List<State> measureStates;
    Dictionary<Measure, MeasureState> measureMap;

    Dictionary<int, HashSet<State>> vertexStatesMap;


    public void Init(StatePanel statePanel)
    {
        this.statePanel = statePanel;
        // statePanel.OnDeleteState = HandleDeleteState;

        geometryStates = new List<State>();
        conditionStates = new List<State>();
        auxiliaryStates = new List<State>();
        measureStates = new List<State>();

        geometryMap = new Dictionary<Geometry, GeometryState>();
        conditionMap = new Dictionary<Condition, ConditionState>();
        auxiliaryMap = new Dictionary<Auxiliary, AuxiliaryState>();
        measureMap = new Dictionary<Measure, MeasureState>();

        vertexStatesMap = new Dictionary<int, HashSet<State>>();
    }

    public void ClearStates()
    {
        geometryStates.Clear();
        conditionStates.Clear();
        auxiliaryStates.Clear();

        geometryMap.Clear();
        conditionMap.Clear();
        auxiliaryMap.Clear();

        vertexStatesMap.Clear();

        statePanel.ClearStateCells();
    }

    public void AddGeometryState(GeometryState state)
    {
        int color = ConfigManager.GeometryGroup().Color;
        geometryMap.Add(state.geometry, state);
        AddState(state, geometryStates, color);
    }

    public void AddConditionState(ConditionState state)
    {
        int color = ConfigManager.ConditionGroup().Color;
        conditionMap.Add(state.condition, state);
        AddState(state, conditionStates, color);
    }

    public void AddAuxiliaryState(AuxiliaryState state)
    {
        int color = ConfigManager.AuxiliaryGroup().Color;
        auxiliaryMap.Add(state.auxiliary, state);
        AddState(state, auxiliaryStates, color);
    }

    public void AddMeasureState(MeasureState state)
    {
        int color = ConfigManager.AuxiliaryGroup().Color;
        measureMap.Add(state.measure, state);
        AddState(state, measureStates, color);
    }

    private void AddState(State state, List<State> stateList, int color)
    {
        stateList.Add(state);
        AddDependencies(state);

        statePanel.AddStateCell(state, color);

        RefreshLayout();
    }

    public void RemoveGeometryState(Geometry geometry)
    {
        State state = geometryMap[geometry];
        RemoveState(state, geometryStates);
    }

    public void RemoveConditionState(Condition condition)
    {
        State state = conditionMap[condition];
        RemoveState(state, conditionStates);
    }

    public void RemoveAuxiliaryState(Auxiliary auxiliary)
    {
        State state = auxiliaryMap[auxiliary];
        RemoveState(state, auxiliaryStates);
    }

    public void RemoveMeasureState(Measure measure)
    {
        State state = measureMap[measure];
        RemoveState(state, measureStates);
    }

    private void RemoveState(State state, List<State> stateList)
    {
        stateList.Remove(state);
        RemoveDependencies(state);

        statePanel.RemoveStateCell(state);

        RefreshLayout();
    }

    public void RefreshStateCellById(int id)
    {
        HashSet<State> states;
        vertexStatesMap.TryGetValue(id, out states);
        if (states == null)
            return;
        statePanel.RefreshStateCells(states);
    }

    public void RefreshStateCells()
    {
        statePanel.RefreshStateCells(geometryStates);
        statePanel.RefreshStateCells(conditionStates);
        statePanel.RefreshStateCells(auxiliaryStates);
        statePanel.RefreshStateCells(measureStates);
    }

    private void RefreshLayout()
    {
        int count = geometryStates.Count + conditionStates.Count + auxiliaryStates.Count + measureStates.Count;
        List<State> orderList = new List<State>(count);

        for (int i = 0; i < geometryStates.Count; i++)
            orderList.Add(geometryStates[i]);

        for (int i = 0; i < conditionStates.Count; i++)
            orderList.Add(conditionStates[i]);

        for (int i = 0; i < auxiliaryStates.Count; i++)
            orderList.Add(auxiliaryStates[i]);

        for (int i = 0; i < measureStates.Count; i++)
            orderList.Add(measureStates[i]);

        statePanel.RefreshLayout(orderList);
    }

    private void AddDependencies(State state)
    {
        int[] idSet = state.DependVertices();
        foreach (int id in idSet)
        {
            AddDependency(id, state);
        }
    }

    private void RemoveDependencies(State state)
    {
        int[] idSet = state.DependVertices();
        foreach (int id in idSet)
        {
            RemoveDependency(id, state);
        }
    }

    private void AddDependency(int id, State state)
    {
        HashSet<State> states;
        vertexStatesMap.TryGetValue(id, out states);

        if (states == null)
        {
            states = new HashSet<State>();
            vertexStatesMap[id] = states;
        }

        states.Add(state);
    }

    private void RemoveDependency(int id, State state)
    {
        HashSet<State> states;
        vertexStatesMap.TryGetValue(id, out states);

        if (states != null)
        {
            states.Remove(state);

            if (states.Count == 0)
                vertexStatesMap.Remove(id);
        }
    }

}
