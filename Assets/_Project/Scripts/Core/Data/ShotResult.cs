using UnityEngine;

namespace _Project.Scripts.Core.Data
{
    public struct ShotResult
    {
        public Vector3 Start;
        public Vector3 End;
        public bool Hit;
        public Vector3 HitPoint;
        public Vector3 HitNormal;
        public Collider HitCollider;
    }
}