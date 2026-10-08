using System;
using UnityEngine;

namespace _Project.Scripts.Core.Interfaces
{
    public interface IEnemyHealth
    {
        event Action<float, float> OnHealthChanged;
        event Action<IEnemyHealth> OnDeath;
        
        float Current { get; }
        float Max { get; }
        Transform Transform { get; set; }
    }
}