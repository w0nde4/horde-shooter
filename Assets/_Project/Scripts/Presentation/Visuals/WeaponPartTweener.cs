using _Project.Scripts.Domain.Gameplay.Signals;
using _Project.Scripts.Gameplay.Weapons;
using DG.Tweening;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Presentation.Visuals
{
    public class WeaponPartTweener : NetworkBehaviour
    {
        public enum AnimationMode
        {
            PunchPosition,
            PunchRotation 
        }

        [Inject] private SignalBus _signalBus;

        [Header("Part Animation Mode")]
        [SerializeField] private AnimationMode animMode = AnimationMode.PunchPosition;

        [Header("Punch Vector)")]
        [SerializeField] private Vector3 punchVector = new Vector3(0, 0, -0.05f);
        
        [Header("Timings")]
        [SerializeField] private float duration = 0.08f;
        [SerializeField] private int vibrato = 1;
        [SerializeField] private float elasticity = 0.1f;
        
        [Header("Delay before animation")]
        [SerializeField] private float shotDelay = 0f;

        private string _weaponName;
        private Vector3 _initialLocalPosition;
        private Vector3 _initialLocalRotation;

        private void Awake()
        {
            var weapon = GetComponentInParent<WeaponBase>();
            _weaponName = weapon != null ? weapon.WeaponName : string.Empty;
            
            _initialLocalPosition = transform.localPosition;
            _initialLocalRotation = transform.localRotation.eulerAngles;
        }

        public override void Spawned()
        {
            _signalBus.Subscribe<ShotFiredSignal>(OnShotFired);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _signalBus.TryUnsubscribe<ShotFiredSignal>(OnShotFired);
            
            transform.DOKill();
        }

        private void OnShotFired(ShotFiredSignal signal)
        {
            if (signal.WeaponName != _weaponName || 
                signal.InputAuthority != Object.InputAuthority ||
                !gameObject.activeInHierarchy) return;

            transform.DOKill(complete: true);
            transform.localPosition = _initialLocalPosition;

            if (animMode == AnimationMode.PunchRotation)
            {
                transform.localRotation = Quaternion.Euler(_initialLocalRotation);
            }
            
            if (shotDelay <= 0f)
            {
                if (animMode == AnimationMode.PunchPosition)
                {
                    transform.DOPunchPosition(punchVector, duration, vibrato, elasticity);
                }
                else
                {
                    transform.DOPunchRotation(punchVector, duration, vibrato, elasticity);
                }
                return;
            }

            var backTime = duration * 0.4f;
            var forwardTime = duration * 0.6f;

            if (animMode == AnimationMode.PunchPosition)
            {
                DOTween.Sequence()
                    .AppendInterval(shotDelay)
                    .Append(transform.DOLocalMove(_initialLocalPosition + punchVector, backTime).SetEase(Ease.OutCubic))
                    .Append(transform.DOLocalMove(_initialLocalPosition, forwardTime).SetEase(Ease.InOutCubic));
            }
            else
            {
                DOTween.Sequence()
                    .AppendInterval(shotDelay)
                    .Append(transform.DOLocalRotate(_initialLocalRotation + punchVector, backTime)
                        .SetEase(Ease.OutCubic))
                    .Append(transform.DOLocalRotate(_initialLocalRotation, forwardTime)
                        .SetEase(Ease.InOutCubic));
            }
        }
    }
}