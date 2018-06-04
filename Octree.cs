using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Octree : MonoBehaviour
{
    Material material;

    [System.NonSerialized]
    public OctreeInfo _rootOctree = null;
    Vector3 _extend = new Vector3(float.MinValue, float.MinValue, float.MinValue);
    const int OCTREEDEPTH = 3;

    [System.NonSerialized]
    public Vector3[] _boxWire = null;
    [System.NonSerialized]
    public Vector3[] _boxVertices = null;
    [System.NonSerialized]
    public int[] _indexBuffers = null;

    public class OctreeInfo
    {
        Octree octree = null;
        public OctreeInfo parent = null;
        public List<OctreeInfo> lsChild = new List<OctreeInfo>();
        public List<Vector3> lsVertex = new List<Vector3>();

        public Vector3 localPos = Vector3.zero;
        public Vector3 extend = Vector3.zero;
        public int depth = -1;

        public bool isDrawBoxWire = false;

        public OctreeInfo(Octree octree, Vector3 localPos, Vector3 extend, int depth, bool isAddMesh)
        {
            this.octree = octree;
            this.localPos = localPos;
            this.extend = extend;
            this.depth = depth;

            if (isAddMesh)
            {
                AddVertex();
            }
        }

        void AddVertex()
        {
            Vector3[] boxVertices = octree._boxVertices;
            int[] indexBuffers = octree._indexBuffers;
            Vector3[] checkPos = new Vector3[indexBuffers.Length];
            for (int i = 0; i < indexBuffers.Length; i++)
            {
                checkPos[i] = GetVertexPosition(boxVertices[indexBuffers[i]]);
            }

            Vector3[] checkPos2 = new Vector3[6];
            checkPos2[0] = GetVertexPosition(new Vector3(-1, 0, 0));
            checkPos2[1] = GetVertexPosition(new Vector3(1, 0, 0));
            checkPos2[2] = GetVertexPosition(new Vector3(0, -1, 0));
            checkPos2[3] = GetVertexPosition(new Vector3(0, 1, 0));
            checkPos2[4] = GetVertexPosition(new Vector3(0, 0, -1));
            checkPos2[5] = GetVertexPosition(new Vector3(0, 0, 1));

            Mesh mesh = octree.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;
            int[] tri = mesh.triangles;

            for (int i = 0; i < tri.Length; i += 3)
            {
                Vector3 edge1 = vertices[tri[i]];
                Vector3 edge2 = vertices[tri[i + 1]];
                Vector3 edge3 = vertices[tri[i + 2]];
                for (int k = 0; k < checkPos.Length; k += 3)
                {
                    Vector3 ver1 = checkPos[k];
                    Vector3 ver2 = checkPos[k + 1];
                    Vector3 ver3 = checkPos[k + 2];
                    HelperPhysics.Hit hit;
                    if (HelperPhysics.IntersectLineToTriangle(ver1, ver2, edge1, edge2, edge3, out hit)
                        || HelperPhysics.IntersectLineToTriangle(ver2, ver3, edge1, edge2, edge3, out hit)
                        || HelperPhysics.IntersectLineToTriangle(ver3, ver1, edge1, edge2, edge3, out hit))
                    {
                        lsVertex.Add(edge1);
                        lsVertex.Add(edge2);
                        lsVertex.Add(edge3);
                    }
                }

                for (int k = 0; k < 3; ++k)
                {
                    Vector3 edge = vertices[tri[i + k]];
                    if (checkPos2[0].x <= edge.x && checkPos2[1].x >= edge.x
                        && checkPos2[2].y <= edge.y && checkPos2[3].y >= edge.y
                        && checkPos2[4].z <= edge.z && checkPos2[5].z >= edge.z)
                    {
                        Vector3 tri1 = mesh.vertices[tri[i]];
                        Vector3 tri2 = mesh.vertices[tri[i + 1]];
                        Vector3 tri3 = mesh.vertices[tri[i + 2]];
                        lsVertex.Add(tri1);
                        lsVertex.Add(tri2);
                        lsVertex.Add(tri3);
                        break;
                    }
                }
            }
        }

        public void AddChild(OctreeInfo child)
        {
            lsChild.Add(child);
            child.parent = this;
        }

        public Vector3[] GetChildLocalPos()
        {
            Vector3 halfExtend = extend * 0.5f;

            Vector3[] arrPos = new Vector3[8];
            arrPos[0] = new Vector3(localPos.x + halfExtend.x, localPos.y + halfExtend.y, localPos.z + halfExtend.z);
            arrPos[1] = new Vector3(localPos.x + halfExtend.x, localPos.y + halfExtend.y, localPos.z - halfExtend.z);
            arrPos[2] = new Vector3(localPos.x + halfExtend.x, localPos.y - halfExtend.y, localPos.z + halfExtend.z);
            arrPos[3] = new Vector3(localPos.x + halfExtend.x, localPos.y - halfExtend.y, localPos.z - halfExtend.z);
            arrPos[4] = new Vector3(localPos.x - halfExtend.x, localPos.y + halfExtend.y, localPos.z + halfExtend.z);
            arrPos[5] = new Vector3(localPos.x - halfExtend.x, localPos.y + halfExtend.y, localPos.z - halfExtend.z);
            arrPos[6] = new Vector3(localPos.x - halfExtend.x, localPos.y - halfExtend.y, localPos.z + halfExtend.z);
            arrPos[7] = new Vector3(localPos.x - halfExtend.x, localPos.y - halfExtend.y, localPos.z - halfExtend.z);

            return arrPos;
        }

        public Vector3 GetWorldPosition(Vector3 localPos)
        {
            Vector3 pos = octree.gameObject.transform.position;
            Quaternion rot = octree.gameObject.transform.rotation;
            Vector3 scale = octree.gameObject.transform.localScale;
            return rot * Vector3.Scale(scale, localPos) + pos;
        }

        public Vector3 GetVertexPosition(Vector3 vertex)
        {
            Vector3 pos = octree.gameObject.transform.position;
            Quaternion rot = octree.gameObject.transform.rotation;
            Vector3 scale = octree.gameObject.transform.localScale;
            Vector3 worldPos = GetWorldPosition(localPos);

            return rot * Vector3.Scale(Vector3.Scale(scale, extend), vertex) + worldPos;
        }

        public void ReleaseDraw()
        {
            isDrawBoxWire = false;
            for (int i = 0; i < lsChild.Count; ++i)
            {
                lsChild[i].ReleaseDraw();
            }
        }

        public void DrawBoxWire()
        {
            if (isDrawBoxWire)
            {
                Vector3[] boxWire = octree._boxWire;
                for (int i = 0; i < boxWire.Length; ++i)
                {
                    Vector3 wirePos = GetVertexPosition(boxWire[i]);
                    GL.Vertex(wirePos);
                }

                if (lsChild.Count <= 0)
                {
                    for (int i = 0; i < lsVertex.Count; i += 3)
                    {
                        Vector3 vertex1 = GetWorldPosition(lsVertex[i]);
                        Vector3 vertex2 = GetWorldPosition(lsVertex[i + 1]);
                        Vector3 vertex3 = GetWorldPosition(lsVertex[i + 2]);
                        GL.Vertex(vertex1);
                        GL.Vertex(vertex2);
                        GL.Vertex(vertex2);
                        GL.Vertex(vertex3);
                        GL.Vertex(vertex3);
                        GL.Vertex(vertex1);
                    }
                }
            }          

            for (int i = 0; i < lsChild.Count; ++i)
            {
                lsChild[i].DrawBoxWire();
            }
        }

        public void DrawVerticeBox()
        {
            Vector3[] boxVertices = octree._boxVertices;
            int[] indexBuffers = octree._indexBuffers;

            for (int i = 0; i < indexBuffers.Length; i += 3)
            {
                Vector3 edge1 = GetVertexPosition(boxVertices[indexBuffers[i]]);
                Vector3 edge2 = GetVertexPosition(boxVertices[indexBuffers[i + 1]]);
                Vector3 edge3 = GetVertexPosition(boxVertices[indexBuffers[i + 2]]);

                GL.Vertex(edge1);
                GL.Vertex(edge2);
                GL.Vertex(edge2);
                GL.Vertex(edge3);
                GL.Vertex(edge3);
                GL.Vertex(edge1);
            }

            for (int i = 0; i < lsChild.Count; ++i)
            {
                lsChild[i].DrawVerticeBox();
            }
        }
    }

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
        CreateBoxWire();
        CreateBoxVertexBuffers();
        CalcExtend();
        CreateOctree();
    }

    void OnRenderObject()
    {
        GL.PushMatrix();
        material.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(Color.white);
        _rootOctree.DrawBoxWire();
        //_rootOctree.DrawVerticeBox();
        GL.End();
        GL.PopMatrix();
    }

    void CreateBoxWire()
    {
        Vector3[] baseVertices = new Vector3[8];
        int[] indices = new int[24] { 0, 1, 1, 2, 2, 3, 3, 0, 4, 5, 5, 6, 6, 7, 7, 4, 0, 4, 1, 5, 3, 7, 2, 6 };

        baseVertices[0] = new Vector3(-1.0f, 1.0f, 1.0f);
        baseVertices[1] = new Vector3(1.0f, 1.0f, 1.0f);
        baseVertices[2] = new Vector3(1.0f, 1.0f, -1.0f);
        baseVertices[3] = new Vector3(-1.0f, 1.0f, -1.0f);
        baseVertices[4] = new Vector3(-1.0f, -1.0f, 1.0f);
        baseVertices[5] = new Vector3(1.0f, -1.0f, 1.0f);
        baseVertices[6] = new Vector3(1.0f, -1.0f, -1.0f);
        baseVertices[7] = new Vector3(-1.0f, -1.0f, -1.0f);

        _boxWire = new Vector3[24];
        for (int i = 0; i < 24; i++)
        {
            _boxWire[i] = baseVertices[indices[i]];
        }
    }

    void CreateBoxVertexBuffers()
    {
        _boxVertices = new Vector3[]
        {
            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, 1.0f),  //top left front
            new Vector3(1.0f, 1.0f, 1.0f),  //top right front
            new Vector3(1.0f, -1.0f, 1.0f),  //bottom right front

            new Vector3(-1.0f, -1.0f, -1.0f), //bottom left back
            new Vector3(-1.0f, 1.0f, -1.0f), //top left back
            new Vector3(1.0f, 1.0f, -1.0f), //top right back
            new Vector3(1.0f, -1.0f, -1.0f), //bottom right back

            new Vector3(-1.0f, -1.0f, -1.0f), //bottom left back II
            new Vector3(-1.0f, 1.0f, -1.0f), //top left back II
            new Vector3(1.0f, 1.0f, -1.0f), //top right back II
            new Vector3(1.0f, -1.0f, -1.0f) //bottom right back II
        };

        _indexBuffers = new int[]
        {
            1, 0, 3, 2, 1, 3, //front
            2, 3, 7, 6, 2, 7, //right
            6, 7, 4, 5, 6, 4, //back
            5, 4, 1, 1, 4, 0, //left
            9, 1, 2, 10, 9, 2, //top
            0, 8, 11, 3, 0, 11 //bottom
        };
    }

    void CalcExtend()
    {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        Vector3[] vertices = meshFilter.mesh.vertices;
        for (int i = 0; i < vertices.Length; ++i)
        {
            float absValueX = Mathf.Abs(vertices[i].x);
            float absValueY = Mathf.Abs(vertices[i].y);
            float absValueZ = Mathf.Abs(vertices[i].z);

            _extend.x = Mathf.Max(_extend.x, absValueX);
            _extend.y = Mathf.Max(_extend.y, absValueY);
            _extend.z = Mathf.Max(_extend.z, absValueZ);
        }
    }

    void CreateOctree()
    {
        _rootOctree = new OctreeInfo(this, Vector3.zero, _extend, 0, false);
        if (OCTREEDEPTH > 1)
        {
            CreateChildOctree(_rootOctree, 2, OCTREEDEPTH);
        }
    }

    void CreateChildOctree(OctreeInfo octreeInfo, int index, int depth)
    {
        Vector3 extend = octreeInfo.extend * 0.5f;
        Vector3[] arrPos = octreeInfo.GetChildLocalPos();
        int childDepth = octreeInfo.depth + 1;
        for (int i = 0; i < arrPos.Length; ++i)
        {
            OctreeInfo childOctree = new OctreeInfo(this, arrPos[i], extend, childDepth, !(index < depth));
            octreeInfo.AddChild(childOctree);

            int tempIndex = index;
            if (index < depth)
            {
                CreateChildOctree(childOctree, ++tempIndex, depth);
            }
        }
    }
}
