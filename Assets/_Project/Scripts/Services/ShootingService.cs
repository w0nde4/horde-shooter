using _Project.Scripts.Core.Data;
using _Project.Scripts.Core.Interfaces;
using UnityEngine;

namespace _Project.Scripts.Services
{
    public class ShootingService : IShootingService
    {
        public ShotResult CalculateShotResult(Vector3 origin, Vector3 direction, float range, float spreadAngle = 0, LayerMask layerMask = default)
        {
            if (spreadAngle > 0f)
            {
                var spread = Random.insideUnitCircle * spreadAngle;
                direction = (direction + new Vector3(spread.x, spread.y, 0)).normalized;
            }

            var ray = new Ray(origin, direction);
            var result = new ShotResult
            {
                Start = origin,
                End = origin + direction * range,
                Hit = false
            };
            
            if (Physics.Raycast(ray, out var hit, range, layerMask))
            {
                result.Hit = true;
                result.HitPoint = hit.point;
                result.HitNormal = hit.normal;
                result.HitCollider = hit.collider;
                result.End = hit.point;
            }
            
            Debug.DrawRay(ray.origin, ray.direction * range, Color.green, 0.1f);
            
            return result;
        }
    }
}