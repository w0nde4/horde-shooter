using _Project.Scripts.Domain.Gameplay.Signals;
using _Project.Scripts.Gameplay.Weapons;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Presentation.Visuals
{
    public class WeaponPumpView : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;

        [Header("Pump Settings")]
        [SerializeField] private Vector3 pumpOffset = new Vector3(0, 0, -0.07f);
        [SerializeField] private float timeToPullBack = 0.08f;
        [SerializeField] private float timeToPushForward = 0.12f;

        private string _weaponName;
        private Vector3 _initialLocalPosition;
        private Tween _pumpTween;

        private void Awake()
        {
            _initialLocalPosition = transform.localPosition;
            var weapon = GetComponentInParent<WeaponBase>();
            _weaponName = weapon != null ? weapon.WeaponName : string.Empty;
        }

        private void OnEnable()
        {
            _signalBus.Subscribe<ShotFiredSignal>(OnShotFired);
        }

        private void OnDisable()
        {
            _signalBus.TryUnsubscribe<ShotFiredSignal>(OnShotFired);
            _pumpTween?.Kill();
            transform.DOKill();
        }

        private void OnShotFired(ShotFiredSignal signal)
        {
            if (signal.WeaponName != _weaponName || !gameObject.activeInHierarchy) return;
            
            _pumpTween?.Kill();
            transform.DOLocalMove(_initialLocalPosition, 0f);

            _pumpTween = DOTween.Sequence()
                .Append(transform.DOLocalMove(_initialLocalPosition + pumpOffset, timeToPullBack).SetEase(Ease.OutCubic))
                .Append(transform.DOLocalMove(_initialLocalPosition, timeToPushForward).SetEase(Ease.InOutCubic));
        }
    }
}