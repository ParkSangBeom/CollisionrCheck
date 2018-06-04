using UnityEngine;
using System.Collections;

public class LineToLine : MonoBehaviour
{
    Material material;
    public Transform[] trans = null;
    public Transform[] dot = null;
    public Transform point = null;

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
    }

    void Update()
    {
        HelperPhysics.Hit hit;
        Vector3 point1, point2;
        float range = 0.3f;
        trans[0].RotateAround(Vector3.zero, Vector3.one, range);
        trans[1].RotateAround(Vector3.zero, -Vector3.one, range);
        trans[2].RotateAround(Vector3.zero, Vector3.one, range);
        trans[3].RotateAround(Vector3.zero, -Vector3.one, range);

        if (HelperPhysics.IntersectLineToLine(trans[0].position, trans[1].position, trans[2].position, trans[3].position, out hit, out point1, out point2))
        {
            point.gameObject.SetActive(true);
            point.position = hit.point;
        }
        else
        {
            point.gameObject.SetActive(false);
        }

        if (!float.IsNaN(Vector3.Dot(point1, point2)))
        {
            dot[0].gameObject.SetActive(true);
            dot[1].gameObject.SetActive(true);
            dot[0].position = point1;
            dot[1].position = point2;
        }
        else
        {
            dot[0].gameObject.SetActive(false);
            dot[1].gameObject.SetActive(false);
        }
    }

    void OnRenderObject()
    {
        GL.PushMatrix();
        material.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(Color.white);
        GL.Vertex(trans[0].position);
        GL.Vertex(trans[1].position);
        GL.Vertex(trans[2].position);
        GL.Vertex(trans[3].position);

        if (dot[0].gameObject.activeSelf && dot[1].gameObject.activeSelf)
        {
            GL.Color(Color.green);
            GL.Vertex(dot[0].position);
            GL.Vertex(dot[1].position);
        }

        GL.End();
        GL.PopMatrix();
    }
}
