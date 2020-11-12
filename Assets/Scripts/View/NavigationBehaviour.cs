using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationBehaviour : MonoBehaviour
{
    public Mesh CenterMesh;
    public Mesh AxisMesh;

    const string LAYER = "Navigation";
    Material axisNMaterial;
    Material axisHMaterial;

    GeoCamera geoCamera;
    public void Init(GeoCamera camera)
    {
        geoCamera = camera;

        CreatCenter();
        CreateNavAxis();
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }

    private void CreatCenter()
    {
        int layer = LayerMask.NameToLayer(LAYER);

        GameObject centerObject = new GameObject("center");
        centerObject.layer = layer;
        centerObject.transform.SetParent(transform);
        centerObject.transform.position = Vector3.zero;

        NavCenterBehaviour center = centerObject.AddComponent<NavCenterBehaviour>();
        center.mesh = CenterMesh;
        center.Init();
        center.OnClick = () => geoCamera.TriggerCenterRAnimation();
    }

    private void CreateNavAxis()
    {
        int layer = LayerMask.NameToLayer(LAYER);

        Mesh meshX = (Mesh)Instantiate(AxisMesh);
        Mesh meshY = (Mesh)Instantiate(AxisMesh);
        Mesh meshZ = (Mesh)Instantiate(AxisMesh);
        Mesh meshW = (Mesh)Instantiate(AxisMesh);

        SetMeshUV(meshX, new Vector4(1, 0, 0, 0));
        SetMeshUV(meshY, new Vector4(0, 1, 0, 0));
        SetMeshUV(meshZ, new Vector4(0, 0, 1, 0));
        SetMeshUV(meshW, new Vector4(0, 0, 0, 1));

        string[] axisNames = new string[] { "X", "Y", "Z", "-X", "-Y", "-Z" };
        Vector3[] axisPositions = {
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 1),
            new Vector3(-1, 0, 0),
            new Vector3(0, -1, 0),
            new Vector3(0, 0, -1),
        };
        Vector3[] axisRotations = {
            new Vector3(0, 0, 90),
            new Vector3(180, 0, 0),
            new Vector3(-90, 0, 0),
            new Vector3(0, 0, -90),
            new Vector3(0, 0, 0),
            new Vector3(90, 0, 0),
        };
        Vector3 axisScale = Vector3.one;
        Vector2[] axisCamera = {
            new Vector2(270, 0),
            new Vector2(270, 90),
            new Vector2(180, 0),
            new Vector2(90, 0),
            new Vector2(270, -90),
            new Vector2(0, 0),
        };

        Mesh[] axisMeshes = new Mesh[] { meshX, meshY, meshZ, meshW, meshW, meshW };

        for (int i = 0; i < axisNames.Length; i++)
        {
            GameObject axisObject = new GameObject(axisNames[i]);
            axisObject.layer = layer;
            axisObject.transform.SetParent(transform);
            axisObject.transform.position = axisPositions[i] * 0.5f;
            axisObject.transform.eulerAngles = axisRotations[i];
            axisObject.transform.localScale = axisScale;

            NavAxisBehaviour axis = axisObject.AddComponent<NavAxisBehaviour>();
            axis.mesh = axisMeshes[i];
            axis.Init(axisCamera[i]);
            axis.OnClick = (rotation) => geoCamera.TriggerRotateAnimation(rotation.x, rotation.y);
        }
    }

    private void SetMeshUV(Mesh mesh, Vector4 uv)
    {
        List<Vector4> uvs = new List<Vector4>();
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            uvs.Add(uv);
        }
        mesh.SetUVs(0, uvs);

        // Color[] colors = new Color[mesh.vertexCount];
        // Color color1 = new Color(1, 0, 0, 0);
        // Color color2 = new Color(0, 0, 0, 0);
        // for (int i = 0; i < 20; i++)
        // {
        //     colors[i] = color1;
        // }
        // for (int i = 20; i < mesh.vertexCount; i++)
        // {
        //     colors[i] = color2;
        // }
        // mesh.colors = colors;
    }

    // private void Test()
    // {
    //     // log vertex
    //     Vector3[] vertices = ConeMesh.vertices;
    //     for (int i = 0; i < ConeMesh.vertexCount; i++)
    //     {
    //         Debug.Log(i + ": " + vertices[i]);
    //     }
    // }

}
