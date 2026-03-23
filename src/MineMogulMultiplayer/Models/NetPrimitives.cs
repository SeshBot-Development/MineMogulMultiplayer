using MessagePack;
using UnityEngine;

namespace MineMogulMultiplayer.Models
{
    /// <summary>
    /// Compact Vector3 for network serialization (avoids Unity Vector3 overhead).
    /// </summary>
    [MessagePackObject]
    public struct NetVector3
    {
        [Key(0)] public float X;
        [Key(1)] public float Y;
        [Key(2)] public float Z;

        public NetVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public NetVector3(Vector3 v)
        {
            X = v.x;
            Y = v.y;
            Z = v.z;
        }

        public Vector3 ToUnity() => new Vector3(X, Y, Z);

        public override string ToString() => $"({X:F2},{Y:F2},{Z:F2})";

        public static implicit operator NetVector3(Vector3 v) => new NetVector3(v);
        public static implicit operator Vector3(NetVector3 v) => v.ToUnity();
    }

    /// <summary>
    /// Compact Quaternion for network serialization.
    /// </summary>
    [MessagePackObject]
    public struct NetQuaternion
    {
        [Key(0)] public float X;
        [Key(1)] public float Y;
        [Key(2)] public float Z;
        [Key(3)] public float W;

        [SerializationConstructor]
        public NetQuaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public NetQuaternion(Quaternion q)
        {
            X = q.x;
            Y = q.y;
            Z = q.z;
            W = q.w;
        }

        public Quaternion ToUnity() => new Quaternion(X, Y, Z, W);

        public static implicit operator NetQuaternion(Quaternion q) => new NetQuaternion(q);
        public static implicit operator Quaternion(NetQuaternion q) => q.ToUnity();
    }
}
