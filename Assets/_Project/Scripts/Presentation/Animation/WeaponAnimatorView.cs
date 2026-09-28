using _Project.Scripts.Core.Signals;
using _Project.Scripts.Domain.Gameplay.Signals;
using _Project.Scripts.Gameplay.Weapons;
using DG.Tweening;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Presentation.Animation
{
    public class WeaponAnimatorView : NetworkBehaviour
    {
        public enum ReloadAnimationType
        {
            Magazine,
            Sequential
        }
        
        [Header("Reload Animation Type")]
        [SerializeField] private ReloadAnimationType reloadStyle = ReloadAnimationType.Magazine;

        [Header("Kickbash settings for shot")]
        [SerializeField] private Vector3 kickbackPunch = new Vector3(0, 0, -0.25f);
        [SerializeField] private Vector3 rotationPunch = new Vector3(12f, 0, -4f);
        [SerializeField] private float fireDuration = 0.22f;

        [Header("Reload settings for Magazine")]
        [SerializeField] private Vector3 reloadPositionOffset = new Vector3(0, -0.15f, -0.05f); 
        [SerializeField] private Vector3 reloadRotationOffset = new Vector3(-12f, 20f, 0);   
        [SerializeField] private float reloadTransitionTime = 0.15f;

        [Header("ReloadSettings for Sequential")]
        [SerializeField] private Vector3 shellInsertPunch = new Vector3(0, -0.03f, 0.01f);
        [SerializeField] private float insertDuration = 0.12f;
        
        [Inject] private SignalBus _signalBus;
        
        private string _weaponName;
        private Vector3 _originalLocalPosition;
        private Quaternion _originalLocalRotation;
        private Sequence _reloadSequence;

        private void Awake()
        {
            _weaponName = GetComponent<WeaponBase>().WeaponName;
            
            _originalLocalPosition = transform.localPosition;
            _originalLocalRotation = transform.localRotation;
        }

        public override void Spawned()
        {
            _signalBus.Subscribe<ShotFiredSignal>(OnShotFired);
            _signalBus.Subscribe<ReloadStateChangedSignal>(OnReloadStateChanged);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _signalBus.TryUnsubscribe<ShotFiredSignal>(OnShotFired);
            _signalBus.TryUnsubscribe<ReloadStateChangedSignal>(OnReloadStateChanged);

            _reloadSequence?.Kill();
            transform.DOKill();
        }

        private void OnShotFired(ShotFiredSignal signal)
        {
            if (signal.WeaponName != _weaponName || 
                signal.InputAuthority != Object.InputAuthority ||
                !gameObject.activeInHierarchy) return;
            
            transform.DOKill();
            transform.DOPunchPosition(kickbackPunch, fireDuration, vibrato: 1, elasticity: 0.5f);
            transform.DOPunchRotation(rotationPunch, fireDuration, vibrato: 1, elasticity: 0.5f);
        }

        private void OnReloadStateChanged(ReloadStateChangedSignal signal)
        {
            if (signal.WeaponName != _weaponName || 
                signal.InputAuthority != Object.InputAuthority ||
                !gameObject.activeInHierarchy) return;
            
            _reloadSequence?.Kill();

            if (reloadStyle == ReloadAnimationType.Magazine)
            {
                _reloadSequence = DOTween.Sequence().SetId(transform);
                if (signal.IsReloading)
                {
                    _reloadSequence = DOTween.Sequence()
                        .Join(transform.DOLocalMove(_originalLocalPosition + reloadPositionOffset, reloadTransitionTime)
                            .SetEase(Ease.OutCubic))
                        .Join(transform.DOLocalRotate(_originalLocalRotation.eulerAngles + reloadRotationOffset, reloadTransitionTime)
                            .SetEase(Ease.OutCubic));
                }
                else
                {
                    _reloadSequence = DOTween.Sequence()
                        .Join(transform.DOLocalMove(_originalLocalPosition, reloadTransitionTime)
                            .SetEase(Ease.InOutCubic))
                        .Join(transform.DOLocalRotate(_originalLocalRotation.eulerAngles, reloadTransitionTime)
                            .SetEase(Ease.InOutCubic));
                }
            }
            else
            {
                if (!signal.IsReloading)
                {
                    transform.DOPunchPosition(shellInsertPunch, insertDuration, vibrato: 1, elasticity: 0.3f);
                }
            }
        }
    }
}