using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeRay : MonoBehaviour
{
    public List<Octree> _lsOctree = new List<Octree>();
    public Transform _point = null;

    int[] _indexBuffers = null;
    Vector3[] _boxVertices = null;
    List<Octree.OctreeInfo> _lsOctreeInfo = new List<Octree.OctreeInfo>();
    Vector3 origin = Vector3.zero;

    private void Update()
    {
        if (_indexBuffers == null)
        {
            _indexBuffers = _lsOctree[0]._indexBuffers;
            _boxVertices = _lsOctree[0]._boxVertices;
            return;
        }

        _point.gameObject.SetActive(false);
        _lsOctreeInfo.Clear();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        origin = ray.origin;
        Vector3 line2 = ray.origin + ray.direction * 1000;

        HelperPhysics.Hit hit;
        for (int i = 0; i < _lsOctree.Count; ++i)
        {
            Octree.OctreeInfo info = _lsOctree[i]._rootOctree;
            info.ReleaseDraw();
            if (BoxCast(ray, info, origin, line2, out hit))
            {
                info.isDrawBoxWire = true;
            }
        }
  
        _lsOctreeInfo.Sort(SorOctreeInfo);
        if (_lsOctreeInfo.Count > 0)
        {
            Octree.OctreeInfo info = _lsOctreeInfo[0];
            info.isDrawBoxWire = true;
            SetPoint(info, line2);
        }
    }

    private bool BoxCast(Ray ray, Octree.OctreeInfo octreeInfo, Vector3 origin, Vector3 line2, out HelperPhysics.Hit hit)
    {
        hit = new HelperPhysics.Hit();
        for (int i = 0; i < _indexBuffers.Length; i += 3)
        {
            Vector3 edge1 = octreeInfo.GetVertexPosition(_boxVertices[_indexBuffers[i]]);
            Vector3 edge2 = octreeInfo.GetVertexPosition(_boxVertices[_indexBuffers[i + 1]]);
            Vector3 edge3 = octreeInfo.GetVertexPosition(_boxVertices[_indexBuffers[i + 2]]);

            HelperPhysics.Hit hit1;
            if (HelperPhysics.IntersectLineToTriangle(origin, line2, edge1, edge2, edge3, out hit1))
            {
                Vector3 dir1 = edge2 - edge1;
                Vector3 dir2 = edge3 - edge2;

                float dot = Vector3.Dot(ray.direction, Vector3.Cross(dir1, dir2));
                if (dot < 0.0f)
                {
                    hit = hit1;

                    if (octreeInfo.lsChild.Count <= 0)
                    {
                        _lsOctreeInfo.Add(octreeInfo);
                    }

                    for (int k = 0; k < octreeInfo.lsChild.Count; ++k)
                    {
                        HelperPhysics.Hit hit2;
                        if (BoxCast(ray, octreeInfo.lsChild[k], hit1.point, line2, out hit2))
                        {
                            hit = hit2;
                        }
                    }

                    return true;
                }
            }
        }

        return false;
    }

    public void SetPoint(Octree.OctreeInfo info, Vector3 line2)
    {
        for (int i = 0; i < info.lsVertex.Count; i += 3)
        {
            Vector3 edge1 = info.GetWorldPosition(info.lsVertex[i]);
            Vector3 edge2 = info.GetWorldPosition(info.lsVertex[i + 1]);
            Vector3 edge3 = info.GetWorldPosition(info.lsVertex[i + 2]);

            HelperPhysics.Hit hit3;
            if (HelperPhysics.IntersectLineToTriangle(origin, line2, edge1, edge2, edge3, out hit3))
            {
                _point.position = hit3.point;
                _point.gameObject.SetActive(true);
                break;
            }
        }
    }

    private int SorOctreeInfo(Octree.OctreeInfo a, Octree.OctreeInfo b)
    {       
        Vector3 pos1 = a.GetWorldPosition(a.localPos);
        Vector3 pos2 = b.GetWorldPosition(b.localPos);
        float sqr1 = Vector3.Distance(origin, pos1);
        float sqr2 = Vector3.Distance(origin, pos2);
        if (sqr1 < sqr2)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }
}
