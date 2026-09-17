using Fusion;
using UnityEngine;

namespace _Project.Scripts.Domain.Gameplay.Data
{
    public struct NetworkBulletData : INetworkStruct
    {
        public Vector3 HitPoint;
        public NetworkBool Hit;
    }
}