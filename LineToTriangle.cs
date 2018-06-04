using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineToTriangle : MonoBehaviour
{
    Material material;
    public Transform polygon = null;
    public Transform[] line = null;
    public Transform[] point = null;
    List<Vector3> lsTriPoint = new List<Vector3>();

    float _addTime = 0.0f;
    float _changeTime = 4.4f;
    Vector3 _moveDir = Vector3.down * 0.5f;
    Vector3 _moveDir2 = Vector3.forward * 0.5f;

    void CreateMaterial()
    {
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        material = new Material(shader);
        material.hideFlags = HideFlags.HideAndDontSave;
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Back);
        material.SetInt("_ZWrite", 0);
    }

    void Start()
    {
        CreateMaterial();

        Mesh mesh = polygon.GetComponent<MeshFilter>().mesh;
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            lsTriPoint.Add(mesh.vertices[mesh.triangles[i]]);
            lsTriPoint.Add(mesh.vertices[mesh.triangles[i + 1]]);
            lsTriPoint.Add(mesh.vertices[mesh.triangles[i + 2]]);
        }
    }

    void Update()
    {
        _addTime += Time.deltaTime;
        if (_addTime > _changeTime)
        {
            _moveDir = -_moveDir;
            _moveDir2 = -_moveDir2;
            _addTime = 0.0f;
        }
        line[0].Translate(_moveDir * Time.deltaTime);
        line[1].Translate(_moveDir * Time.deltaTime);
        line[0].Translate(-_moveDir2 * Time.deltaTime);
        line[1].Translate(_moveDir2 * Time.deltaTime);

        polygon.transform.Rotate(Vector3.one * 1.0f);

        List<HelperPhysics.Hit> lsHit = new List<HelperPhysics.Hit>();
        for (int i = 0; i < lsTriPoint.Count; i += 3)
        {
            Vector3 edge1 = (polygon.rotation * Vector3.Scale(lsTriPoint[i], polygon.localScale) + polygon.position);
            Vector3 edge2 = (polygon.rotation * Vector3.Scale(lsTriPoint[i + 1], polygon.localScale) + polygon.position);
            Vector3 edge3 = (polygon.rotation * Vector3.Scale(lsTriPoint[i + 2], polygon.localScale) + polygon.position);

            HelperPhysics.Hit hit3;
            if (HelperPhysics.IntersectLineToTriangle(line[0].position, line[1].position, edge1, edge2, edge3, out hit3))
            {
                lsHit.Add(hit3);
            }
        }

        for(int i = 0; i < point.Length; ++i)
        {
            point[i].gameObject.SetActive(false);
        }

        for(int i = 0; i < lsHit.Count; ++i)
        {
            point[i].gameObject.SetActive(true);
            point[i].position = lsHit[i].point;
        }
    }

    void OnRenderObject()
    {
        GL.PushMatrix();
        material.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(Color.white);
        GL.Vertex(line[0].position);
        GL.Vertex(line[1].position);

        GL.End();
        GL.PopMatrix();
    }
}
