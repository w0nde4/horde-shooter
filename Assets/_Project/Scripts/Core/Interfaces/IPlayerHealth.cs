using System;

namespace _Project.Scripts.Core.Interfaces
{
    public interface IPlayerHealth
    {
        bool IsDead { get; }
        event Action OnDeath;
        event Action OnRespawned;
    }
}