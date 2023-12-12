using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SerializableTypes{
    /// <summary> Serializable version of UnityEngine.Vector3. </summary>
    [Serializable]
    public struct SVector3
    {
        public float x;
        public float y;
        public float z;

        public SVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString()
            => $"[x, y, z]";

        public static implicit operator Vector3(SVector3 s)
            => new Vector3(s.x, s.y, s.z);

        public static implicit operator SVector3(Vector3 v)
            => new SVector3(v.x, v.y, v.z);


        public static SVector3 operator +(SVector3 a, SVector3 b) 
            => new SVector3(a.x + b.x, a.y + b.y, a.z + b.z);

        public static SVector3 operator -(SVector3 a, SVector3 b)
            => new SVector3(a.x - b.x, a.y - b.y, a.z - b.z);

        public static SVector3 operator -(SVector3 a)
            => new SVector3(-a.x, -a.y, -a.z);

        public static SVector3 operator *(SVector3 a, float m)
            => new SVector3(a.x * m, a.y * m, a.z * m);

        public static SVector3 operator *(float m, SVector3 a)
            => new SVector3(a.x * m, a.y * m, a.z * m);

        public static SVector3 operator /(SVector3 a, float d)
            => new SVector3(a.x / d, a.y / d, a.z / d);
    }

    [Serializable]
    public struct SVector2Int {
        public int x;
        public int y;

        public SVector2Int(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
            => $"[x, y, z]";

        public static implicit operator Vector2(SVector2Int s)
            => new Vector2(s.x, s.y);

        public static implicit operator SVector2Int(Vector3Int v)
            => new SVector2Int(v.x, v.y);


        public static SVector2Int operator +(SVector2Int a, SVector2Int b)
            => new SVector2Int(a.x + b.x, a.y + b.y);

        public static SVector2Int operator -(SVector2Int a, SVector2Int b)
            => new SVector2Int(a.x - b.x, a.y - b.y);

        public static SVector2Int operator -(SVector2Int a)
            => new SVector2Int(-a.x, -a.y);

        public static SVector2Int operator *(SVector2Int a, int m)
            => new SVector2Int(a.x * m, a.y * m);

        public static SVector2Int operator *(int m, SVector2Int a)
            => new SVector2Int(a.x * m, a.y * m);

        public static SVector2Int operator /(SVector2Int a, int d)
            => new SVector2Int(a.x / d, a.y / d);
    }

    /// <summary> Serializable version of UnityEngine.Quaternion. </summary>
    [Serializable]
    public struct SQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public SQuaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public override string ToString()
            => $"[{x}, {y}, {z}, {w}]";

        public static implicit operator Quaternion(SQuaternion s)
            => new Quaternion(s.x, s.y, s.z, s.w);

        public static implicit operator SQuaternion(Quaternion q)
            => new SQuaternion(q.x, q.y, q.z, q.w);
    }

    /// <summary> Serializable version of UnityEngine.Color32 without transparency. </summary>
    [Serializable]
    public struct SColor32
    {
        public byte r;
        public byte g;
        public byte b;

        public SColor32(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public SColor32(Color32 c)
        {
            r = c.r;
            g = c.g;
            b = c.b;
        }

        public override string ToString()
            => $"[{r}, {g}, {b}]";

        public static implicit operator Color32(SColor32 rValue)
            => new Color32(rValue.r, rValue.g, rValue.b, a: byte.MaxValue);

        public static implicit operator SColor32(Color32 rValue)
            => new SColor32(rValue.r, rValue.g, rValue.b);
    }
 }
