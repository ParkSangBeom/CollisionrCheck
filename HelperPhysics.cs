using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperPhysics
{
    public struct Hit
    {
        public Vector3 point { get; set; }
        public Vector3 normal { get; set; }
        public Vector3[] pos { get; set; }
    }

    public static bool IntersectDotToLine(Vector3 dot, Vector3 linePos1, Vector3 linePos2, out Hit hit)
    {
        hit = new Hit();
        
        Vector3 dir1 = (linePos2 - dot);
        Vector3 dir2 = (linePos2 - linePos1);

        Vector3 oProj = Vector3.Cross(dir1, dir2);
        if (oProj == Vector3.zero)
        {
            Vector3 dir3 = (linePos1 - dot);
            float iProj = Vector3.Dot(dir1, dir3);
            if (iProj < Mathf.Epsilon)
            {
                hit.point = dot;
                return true;
            }
        }

        return false;
    }

    public static bool IntersectLineToLine(Vector3 linePos1, Vector3 linePos2, Vector3 linePos3, Vector3 linePos4, out Hit hit)
    {
        hit = new Hit();
        Vector3 empty1 = Vector3.zero;
        Vector3 empty2 = Vector3.zero;
        if (IntersectLineToLine(linePos1, linePos2, linePos3, linePos4, out hit, out empty1, out empty2))
        {
            return true;
        }
        return false;
    }

    public static bool IntersectLineToLine(Vector3 linePos1, Vector3 linePos2, Vector3 linePos3, Vector3 linePos4, out Hit hit, out Vector3 point1, out Vector3 point2)
    {
        hit = new Hit();
        point1 = Vector3.zero;
        point2 = Vector3.zero;

        Vector3 dir1 = linePos2 - linePos1;
        Vector3 dir1Nor = dir1.normalized;
        Vector3 dir2 = linePos4 - linePos3;
        Vector3 dir2Nor = dir2.normalized;

        // 평면을 만들기 위함.
        Vector3 oProj = Vector3.Cross(dir1, dir2);

        // 평면 노멀구함.
        Vector3 pNor1 = Vector3.Cross(oProj, dir2).normalized;
        float t1 = Vector3.Dot(linePos3 - linePos1, pNor1);
        float t2 = t1 / Vector3.Dot(dir1Nor, pNor1);

        // 포인트 구함.
        point1 = linePos1 + dir1Nor * t2;

        // 평면 노멀구함.
        Vector3 pNor2 = Vector3.Cross(oProj, dir1).normalized;
        float t3 = Vector3.Dot(linePos2 - linePos3, pNor2);
        float t4 = t3 / Vector3.Dot(dir2Nor, pNor2);

        // 포인트 구함.
        point2 = linePos3 + dir2Nor * t4;

        if (float.IsNaN(t2) && float.IsNaN(t4))
        {
            return false;
        }

        if (IntersectDotToLine(point1, linePos3, linePos4, out hit) && IntersectDotToLine(point2, linePos1, linePos2, out hit))
        {
            return true;
        }

        Hit hit2;
        if (!IntersectDotToLine(point1, linePos1, linePos2, out hit2))
        {
            if (Vector3.SqrMagnitude(point2 - linePos1) < Vector3.SqrMagnitude(point1 - linePos2))
            {
                point1 = linePos1;
            }
            else
            {
                point1 = linePos2;
            }
        }
        if (!IntersectDotToLine(point2, linePos3, linePos4, out hit2))
        {
            if (Vector3.SqrMagnitude(point2 - linePos3) < Vector3.SqrMagnitude(point2 - linePos4))
            {
                point2 = linePos3;
            }
            else
            {
                point2 = linePos4;
            }
        }

        return false;
    }

    public static bool IntersectDotToTriangle(Vector3 dot, Vector3 triPos1, Vector3 triPos2, Vector3 triPos3, out Hit hit)
    {
        hit = new Hit();

        if (IntersectDotToLine(dot, triPos1, triPos2, out hit))
        {
            hit.point = dot;
            return true;
        }

        if (IntersectDotToLine(dot, triPos2, triPos3, out hit))
        {
            hit.point = dot;
            return true;
        }

        if (IntersectDotToLine(dot, triPos3, triPos1, out hit))
        {
            hit.point = dot;
            return true;
        }

        Vector3 dir1 = triPos2 - triPos1;
        Vector3 dir2 = triPos3 - triPos1;
        Vector3 dir3 = triPos2 - dot;
        Vector3 dir4 = triPos3 - dot;
        Vector3 planNor1 = Vector3.Cross(dir1, dir2).normalized;
        Vector3 planNor2 = Vector3.Cross(dir3, dir4).normalized;
        float iProj = Vector2.Dot(planNor1, planNor2);

        if (iProj >= 0)
        {
            Vector3 c1 = triPos2 - triPos1;
            Vector3 c2 = dot - triPos2;
            Vector3 proj1 = Vector3.Cross(c1, c2);

            Vector3 c3 = triPos3 - triPos2;
            Vector3 c4 = dot - triPos3;
            Vector3 proj2 = Vector3.Cross(c3, c4);

            Vector3 c5 = triPos1 - triPos3;
            Vector3 c6 = dot - triPos1;
            Vector3 proj3 = Vector3.Cross(c5, c6);

            if (Vector3.Dot(proj1, proj2) >= 0 && Vector3.Dot(proj1, proj3) >= 0)
            {
                hit.point = dot;
                return true;
            }
        }

        return false;
    }

    public static bool IntersectLineToTriangle(Vector3 linePos1, Vector3 linePos2, Vector3 triPos1, Vector3 triPos2, Vector3 triPos3, out Hit hit)
    {
        hit = new Hit();

        Vector3 rayPoint = linePos1;
        Vector3 rayDir = (linePos2 - linePos1).normalized;

        Vector3 dir1 = triPos2 - triPos1;
        Vector3 dir2 = triPos3 - triPos1;
        Vector3 planNor = Vector3.Cross(dir1, dir2).normalized;

        float t3 = Vector3.Dot(triPos1 - rayPoint, planNor);
        float t4 = t3 / Vector3.Dot(rayDir, planNor);
        Vector3 hitPos = linePos1 + rayDir * t4;

        Vector3 dir3 = (linePos1 - hitPos).normalized;
        Vector3 dir4 = (linePos2 - hitPos).normalized;
        float iProj = Vector3.Dot(dir3, dir4);
        if (Mathf.Approximately(iProj, 1.0f))
        {
            return false;
        }

        if (IntersectDotToTriangle(hitPos, triPos1, triPos2, triPos3, out hit))
        {
            return true;
        }

        return false;
    }
}