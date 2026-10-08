using UnityEngine;

namespace _Project.Scripts.Core.Signals
{
    public readonly struct GrenadeExplodedSignal
    {
        public Vector3 Position { get; }

        public GrenadeExplodedSignal(Vector3 position) => Position = position;
    }
}