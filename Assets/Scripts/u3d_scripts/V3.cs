using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class V3
{
    public static Vector3 Of(Vec3 nv)
    {
        return new Vector3(nv.X, nv.Y, nv.Z);
    }
    public static Vector3 Of(Vector3 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }
    public static Vector3 ToVector3(Vec3 nv)
    {
        return new Vector3(nv.X, nv.Y, nv.Z);
    }
    public static Vec3 ToVec3(Vector3 v)
    {
        return new Vec3() { X = (int)v.x, Y = (int)v.y, Z = (int)v.z };
    }

}

