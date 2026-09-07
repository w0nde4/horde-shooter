using _Project.Scripts.Core.Data;
using UnityEngine;

namespace _Project.Scripts.Core.Interfaces
{
    public interface IShootingService
    {
        ShotResult CalculateShotResult(Vector3 origin, Vector3 direction, float range, float spreadAngle = 0f, LayerMask layerMask = default);
    }
}