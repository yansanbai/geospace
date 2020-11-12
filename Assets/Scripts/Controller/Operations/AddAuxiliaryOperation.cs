using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddAuxiliaryOperation : Operation
{
    GeoController geoController;
    StateController stateController;

    GeoCamera geoCamera;

    Geometry geometry;
    GeometryBehaviour geometryBehaviour;
    InputPanel inputPanel;
    Tool tool;

    AuxiliaryTool auxiliaryTool;
    GeoUI geoUI;
    List<Measure> measure_list = new List<Measure>();
    List<ElementBehaviour> elementBehaviour_list = new List<ElementBehaviour>();
    FormInput writeInput;
    public AddAuxiliaryOperation(GeoController geoController,GeoCamera geoCamera, StateController stateController, Geometry geometry, GeometryBehaviour geometryBehaviour, GeoUI geoUI, Tool tool)
    {
        CanRotateCamera = true;
        CanActiveElement = true;

        this.geoController = geoController;

        this.geoCamera = geoCamera;

        this.stateController = stateController;
        this.geometry = geometry;
        this.geometryBehaviour = geometryBehaviour;
        this.inputPanel = geoUI.inputPanel;
        this.tool = tool;
        this.geoUI = geoUI;
        Type type = Type.GetType(tool.Name + "AuxiliaryTool");
        if (type != null)
            auxiliaryTool = (AuxiliaryTool)Activator.CreateInstance(type);
    }

    public override void Start()
    {
        if (auxiliaryTool == null)
        {
            Debug.LogWarning(tool.Name + " Error!");
            geoController.EndOperation();
            return;
        }
        if (writeInput == null)
        {
            
            FormInput formInput = auxiliaryTool.FormInput();

            if (formInput != null)
            {
                inputPanel.SetFormForInput(formInput);

                inputPanel.OnValidate = (form) =>
                {
                    return auxiliaryTool.ValidateInput(geometry, form);
                };

                inputPanel.OnClickSubmit = (form) =>
                {
                    addAuxiliary(geometry, form);
                };

                inputPanel.OnClickCancel = (form) =>
                {
                    geoController.EndOperation();
                };
            }
            else
            {
                addAuxiliary(geometry, null);
            }
        }
        else {
            addAuxiliary(geometry, writeInput);
        }
    }

    public void addAuxiliary(Geometry geometry, FormInput form)
    {
        //Debug.Log(form.inputs[0] + " " + form.inputs[1] + " " + form.inputs[2] + " " + form.inputs[3]);
        //Debug.Log(form.inputs[0].GetType() + " " + form.inputs[1].GetType() + " " + form.inputs[2].GetType() + " " + form.inputs[3].GetType());
        //Debug.Log(form.inputs[0].GetType() + " " + form.inputs[1].GetType() + " " + form.inputs[2].GetType());
        Auxiliary auxiliary = auxiliaryTool.GenerateAuxiliary(geometry, form);
        if (auxiliary == null) {
            geoController.EndOperation();
            return;
        }
        auxiliary.InitWithGeometry(geometry);

        VertexUnit[] units = auxiliary.units;
        GeoElement[] elements = auxiliary.elements;

        bool result = geometry.Assistor.AddAuxiliary(auxiliary);

        if (result)
        {
            foreach (VertexUnit unit in units)
                geometry.AddVertexUnit(unit);

            geometry.RefreshGeoEdges();
            foreach (GeoElement element in elements)
                geometry.AddElement(element);

            AddState(auxiliary,form);

            geometryBehaviour.UpdateElements();
            foreach (GeoElement element in elements)
                geometryBehaviour.AddElement(element);

            geometryBehaviour.UpdateSignsPosition();
            foreach (VertexUnit unit in units)
                geometryBehaviour.AddSign(unit.id);


            Gizmo[] gizmos = auxiliary.gizmos;
            if (gizmos != null)
            {
                foreach (Gizmo gizmo in gizmos)
                {
                    geometry.AddGizmo(gizmo);
                    geometryBehaviour.AddGizmo(gizmo);
                }
            }

            geometryBehaviour.UpdateGeometryShade();

        }
        else
        {
            // TODO
            //Debug.Log("pp");
        }

        geoController.EndOperation();
    }

    public override void End()
    {
        inputPanel.Clear();
    }

    public override void OnClickElement(GeoElement element)
    {
        FormElement form = null;
        if (element is GeoVertex)
            form = geoController.VertexForm((GeoVertex)element);
        else if (element is GeoEdge)
            form = geoController.EdgeForm((GeoEdge)element);
        else if (element is GeoFace)
            form = geoController.FaceForm((GeoFace)element);
        if (form != null)
            inputPanel.InputFields(form.fields);
    }

    public void SetWriteInput(FormInput writeInput) {
        this.writeInput = writeInput;
    }

    private void AddState(Auxiliary auxiliary,FormInput form)
    {
        Type type = Type.GetType(tool.Name + "AuxiliaryState");
        if (type != null)
        {
            AuxiliaryState auxiliaryState = (AuxiliaryState)Activator.CreateInstance(type, tool, auxiliary, geometry);
            auxiliaryState.OnClickDelete = () => geoController.RemoveAuxiliaryOperation(auxiliary);
            
            //state单击
            auxiliaryState.DoubleClick = () => this.TurnToFront(auxiliary,form);

            //Action OnElementHighLight  0925 Requirement_1
            auxiliaryState.OnElementHighlight = () =>   
            {
            
            //Hightlight face
            ChangeFaceColorIndex((GeoFace)auxiliary.elements[0],1);
            
            //Hide coordinate
            geoUI.navPanel.OnCoordinateButtonClick(1);
            geoUI.navPanel.SetCoordinateButtonStatus(1); //Change Button status

            //Hide grid
            geoUI.navPanel.OnGridButtonClick(1);
            geoUI.navPanel.SetGridButtonStatus(1);  

            
            //Measure Line length
            if(auxiliary.elements[0] is GeoFace) 
            {
                GeoFace geoFace = (GeoFace)auxiliary.elements[0];
                for(int i = 0;i < geoFace.Ids.Length; i++)
                {

                int vertex1 = i;
                int vertex2 = (i+1) % geoFace.Ids.Length;

                Measure measure = new LineLengthMeasure(geoFace.Ids[vertex1], geoFace.Ids[vertex2]);
                
                measure_list.Add((Measure)measure);
                
                measure.InitWithGeometry(geometry);
                bool result = geometry.Implement.AddMeasure(measure);
                if (result)
                {
                    List<ToolGroup> toolGroups = geoUI.toolPanel.ToolGroups();
                    Tool lineLengthMeasureTool = toolGroups[3].Tools[0];
                    AddState_Measure(measure,lineLengthMeasureTool);

                    Gizmo[] gizmos = measure.gizmos;
                    if (gizmos != null)
                    {
                        foreach (Gizmo gizmo in gizmos)
                        {
                            geometry.AddGizmo(gizmo);
                            geometryBehaviour.AddGizmo(gizmo);
                        }
                    }
                }
                else
                {
                    // TODO
                }

                }
            }
            //Measure CornerAngle
            if(auxiliary.elements[0] is GeoFace) 
            {
                GeoFace geoFace = (GeoFace)auxiliary.elements[0];
                for(int i = 0;i < geoFace.Ids.Length; i++) 
                {
                int vertex1 = i;
                int vertex2 = (i+1)%geoFace.Ids.Length;
                int vertex3 = (i+2)%geoFace.Ids.Length;

                Measure measure = new CornerAngleMeasure(geoFace.Ids[vertex1], geoFace.Ids[vertex2], geoFace.Ids[vertex3]);
                
                measure_list.Add((Measure)measure);
                
                measure.InitWithGeometry(geometry);


                bool result = geometry.Implement.AddMeasure(measure);

                if (result)
                {
                    List<ToolGroup> toolGroups = geoUI.toolPanel.ToolGroups();
                    Tool cornerAngleMeasureTool = toolGroups[3].Tools[1];
                    AddState_Measure(measure,cornerAngleMeasureTool);

                    Gizmo[] gizmos = measure.gizmos;
                    if (gizmos != null)
                    {
                        foreach (Gizmo gizmo in gizmos)
                        {
                            geometry.AddGizmo(gizmo);
                            geometryBehaviour.AddGizmo(gizmo);
                        }
                    }
                }
                else
                {
                    // TODO
                } 
                }  
            }        
            //Measure Plane Area
            if(auxiliary.elements[0] is GeoFace) 
            {
                GeoFace geoFace = (GeoFace)auxiliary.elements[0];
                //Debug.Log(geoFace.Ids);
                Measure measure = new PlaneAreaMeasure(geoFace.Ids);

                measure_list.Add(measure);
                
                measure.InitWithGeometry(geometry);


                bool result = geometry.Implement.AddMeasure(measure);

                if (result)
                {
                    List<ToolGroup> toolGroups = geoUI.toolPanel.ToolGroups();
                    Tool planeAreaMeasureTool = toolGroups[3].Tools[2];
                    AddState_Measure(measure,planeAreaMeasureTool);

                    Gizmo[] gizmos = measure.gizmos;
                    if (gizmos != null)
                    {
                        foreach (Gizmo gizmo in gizmos)
                        {
                            geometry.AddGizmo(gizmo);
                            geometryBehaviour.AddGizmo(gizmo);
                        }
                    }
                }
                else
                {
                    // TODO
                }
            }

            // Hide elements beyond the Highlighted face
             GeoVertex[] geoVertices = geometry.GeoVertices();
             if(auxiliary.elements[0] is GeoFace) 
             {
             GeoFace geoFace = (GeoFace)auxiliary.elements[0];
             VertexUnit[] faceVertices = geoFace.get_vertices();
             Vector3 side1 = faceVertices[0].Position() - faceVertices[1].Position();
             Vector3 side2 = faceVertices[2].Position() - faceVertices[1].Position();
             Vector3 normalVector  = Vector3.Cross(side1,side2);
             
                if (normalVector.x < 0) {
                    normalVector.x = -normalVector.x;
                    normalVector.y = -normalVector.y;
                    normalVector.z = -normalVector.z;
                }
                if (normalVector.x == 0) {
                    if (normalVector.z < 0) {
                        normalVector.y = -normalVector.y;
                        normalVector.z = -normalVector.z;
                    }
                }

             Vector3 anchor = faceVertices[1].Position() + normalVector*2;
            
            float min = 1000;
            GeoEdge[] edges = geometry.GeoEdges();
            foreach(VertexUnit vertexUnit in geoFace.get_vertices())
            {
                float face_vertex_distance = Vector3.Distance(vertexUnit.Position(),anchor);
                if(face_vertex_distance<min)
                min = face_vertex_distance;
            }
             foreach(GeoVertex geoVertex in geoVertices)
             {
                 float distance = Vector3.Distance(geoVertex.VertexUnit().Position(),anchor);
                if(distance<min)
                {
                    //隐藏该顶点
                    ElementBehaviour vertexElementBehaviour = geometryBehaviour.elementMap[geoVertex];
                    vertexElementBehaviour.SetVisible(false);
                    elementBehaviour_list.Add(vertexElementBehaviour);
                    //隐藏该顶点起始的所有边
                    foreach (GeoEdge geoEdge in edges)
                    {
                        if(geoVertex.Id == geoEdge.Id1 || geoVertex.Id == geoEdge.Id2)
                        {
                            ElementBehaviour edgeElementBehaviour = geometryBehaviour.elementMap[geoEdge];
                            edgeElementBehaviour.SetVisible(false);
                            elementBehaviour_list.Add(edgeElementBehaviour);
                        }
                    }
                }
                
             }            
            }
            };

            auxiliaryState.UndoFaceHighlight = () =>
            {
            //Undo Hightlight face
            ChangeFaceColorIndex((GeoFace)auxiliary.elements[0],0);

            
            //Undo Hide coordinate
            geoUI.navPanel.OnCoordinateButtonClick(0);
            geoUI.navPanel.SetCoordinateButtonStatus(0); //Change Button status
                        
            //Hide grid
            geoUI.navPanel.OnGridButtonClick(0);
            geoUI.navPanel.SetGridButtonStatus(0); 

            //Claer All Face_MeasureStates
            foreach(Measure measure in measure_list)
            geoController.RemoveMeasure(measure);
            foreach(ElementBehaviour elementBehaviour in elementBehaviour_list)
            elementBehaviour.SetVisible(true);
            };



            stateController.AddAuxiliaryState(auxiliaryState);
        }
    }


        private void AddState_Measure(Measure measure,Tool tool)
    {
        Type type = Type.GetType(tool.Name + "MeasureState");
        if (type != null)
        {
            MeasureState measureState = (MeasureState)Activator.CreateInstance(type, tool, measure, geometry);
            measureState.OnClickDelete = () => geoController.RemoveMeasureOperation(measure);

            stateController.AddMeasureState(measureState);
        }
    }

    public void TurnToFront(Auxiliary auxiliary,FormInput form) {
        Vector3 rotateAngle = new Vector3(0, 0, 0);
        if (auxiliary.elements[0].name == "Face")
        {
            rotateAngle = GetRotateAngle(auxiliary);
            Debug.Log("change");
        }
        geoCamera.TriggerRotateAnimation(rotateAngle.x,rotateAngle.y);  //动画旋转镜头
    }

    public Vector3 GetRotateAngle(Auxiliary auxiliary)
    {
        VertexUnit[] units = auxiliary.dependencies.ToArray();
        //平面三个不共线点
        Vector3 A = new Vector3();
        Vector3 B = new Vector3();
        Vector3 C = new Vector3();
        //if (auxiliary is PlaneAuxiliary)
        //{
        GeoElement[] elements = auxiliary.elements;
        Vector3[] vertexs = new Vector3[units.Length];
        for (int i = 0; i < units.Length; i++)
        {
            vertexs[i] = units[i].Position();
        }
        for (int i = 0; i < units.Length - 2; i++)
        {
            A = vertexs[i];
            B = vertexs[i + 1];
            C = vertexs[i + 2];

            //if (((B.x - A.x) / (C.x - A.x) == (B.y - A.y) / (C.y - A.y)) && ((B.x - A.x) / (C.x - A.x) == (B.z - A.z) / (C.z - A.z)))
            if(ThreePointsCollinear(A,B,C))
            {
                continue;
            }
            else
            {
                break;
            }
        }
        Debug.Log(A + "**" + B + "**" + C);
        //求平面法线
        Vector3 normalVector;  //平面法向量
        Vector3 AB = new Vector3(B.x - A.x, B.y - A.y, B.z - A.z);
        Vector3 AC = new Vector3(C.x - A.x, C.y - A.y, C.z - A.z);
        normalVector.x = AB.y * AC.z - AC.y * AB.z;
        normalVector.y = AB.z * AC.x - AB.x * AC.z;
        normalVector.z = AB.x * AC.y - AB.y * AC.x;
        if (normalVector.x < 0) {
            normalVector.x = -normalVector.x;
            normalVector.y = -normalVector.y;
            normalVector.z = -normalVector.z;
        }
        if (normalVector.x == 0) {
            if (normalVector.z < 0) {
                normalVector.y = -normalVector.y;
                normalVector.z = -normalVector.z;
            }
        }
        float rotateX = Vector3.Angle(new Vector3(1, 0, 0), new Vector3(normalVector.x,0f,normalVector.z));
        float rotateY = 90f - Vector3.Angle(new Vector3(0, 1, 0), normalVector);
        //float rotateZ = 90f - Vector3.Angle(new Vector3(0, 0, 1), normalVector);
        //float rotateZ = 0f;
        if (normalVector.z < 0) {
           rotateX = -rotateX;
        }
        
        //return new Vector3(rotateX, rotateY, rotateZ);
        return new Vector3(-rotateX-90,rotateY,0f);


    }

    public bool ThreePointsCollinear(Vector3 A, Vector3 B, Vector3 C) {
        double edge_A = PointsDistance(A, B);
        double edge_B = PointsDistance(B, C);
        double edge_C = PointsDistance(A, C);
        double p = 0.5 * (edge_A + edge_B + edge_C);
        if (p * (p - edge_A) * (p - edge_B) * (p - edge_C) == 0) //area==0
            return true; 
        return false;
    }
    public double PointsDistance(Vector3 A, Vector3 B)
    {
        var x1 = A.x - B.x;
        var y1 = A.y - B.y;
        var z1 = A.z - B.z;
        return System.Math.Sqrt(x1 * x1 + y1 * y1 + z1 * z1);
    }

    public void ChangeFaceColorIndex(GeoFace geoface,int colorindex)
    {
        geometry.SetElementColor(geoface, colorindex);  
        geometryBehaviour.GeometryElementColorChange(geoface, colorindex);
    }

}