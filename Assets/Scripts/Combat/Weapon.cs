using System;
using System.Collections;
using UnityEngine;
using NeonApocalypse.AI;
using NeonApocalypse.World.OpenWorld;

namespace NeonApocalypse.Combat
{
    public class Weapon : MonoBehaviour
    {
        [Header("Ballistics")]
        public Transform muzzle;
        public float damage = 24f;
        public float fireRate = 8f;
        public float range = 120f;
        public float critChance = 0.12f;
        public float critMultiplier = 1.8f;
        public LayerMask hitMask = ~0;
        public float tracerSeconds = 0.045f;

        [Header("Magazine")]
        public int magazineSize = 30;
        public float reloadSeconds = 1.5f;

        public int CurrentAmmo { get; private set; }
        public bool IsReloading { get; private set; }
        public event Action<int, int> AmmoChanged;
        public event Action Shot;
        public event Action ReloadStarted;
        public event Action ReloadFinished;
        public event Action<float> HitConfirmed;

        private float nextFire;
        private float reloadFinishTime;
        private LineRenderer tracer;
        private float tracerHideAt;

        private void Awake()
        {
            CurrentAmmo = magazineSize;
            AmmoChanged?.Invoke(CurrentAmmo, magazineSize);
        }

        private void Update()
        {
            if (tracer && Time.time >= tracerHideAt) tracer.enabled = false;
            if (IsReloading && Time.time >= reloadFinishTime)
            {
                IsReloading = false;
                CurrentAmmo = magazineSize;
                AmmoChanged?.Invoke(CurrentAmmo, magazineSize);
                ReloadFinished?.Invoke();
            }
        }

        public bool TryFire(Vector3 direction)
        {
            if (IsReloading || Time.time < nextFire) return false;
            if (CurrentAmmo <= 0) { BeginReload(); return false; }
            nextFire = Time.time + 1f / Mathf.Max(0.1f, fireRate);
            CurrentAmmo--;
            AmmoChanged?.Invoke(CurrentAmmo, magazineSize);
            Shot?.Invoke();

            Vector3 origin = muzzle ? muzzle.position : transform.position + transform.forward * 0.5f;
            AIStimulusBus.Report(origin, 48f, 0.85f, AIStimulusType.Gunshot);
            CrimeEvidenceSystem.Instance?.Report(EvidenceType.Gunshot, origin, 0.32f);
            if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
            {
                var health = hit.collider.GetComponentInParent<Health>();
                if (health && !ReferenceEquals(health.gameObject, gameObject))
                {
                    float finalDamage = damage * (UnityEngine.Random.value < critChance ? critMultiplier : 1f);
                    health.Damage(finalDamage);
                    DamageNumber.Spawn(hit.point, finalDamage);
                    HitConfirmed?.Invoke(finalDamage);
                }
                CreateTracer(origin, hit.point);
            }
            else
            {
                CreateTracer(origin, origin + direction.normalized * range);
            }
            return true;
        }

        private void CreateTracer(Vector3 start, Vector3 end)
        {
            if (!tracer)
            {
                GameObject go = new GameObject("ShotTracer");
                go.transform.SetParent(transform, false);
                tracer = go.AddComponent<LineRenderer>();
                tracer.positionCount = 2;
                tracer.startWidth = 0.018f;
                tracer.endWidth = 0.004f;
                tracer.material = new Material(Shader.Find("Sprites/Default"));
                tracer.startColor = Color.cyan;
                tracer.endColor = Color.white;
                tracer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                tracer.receiveShadows = false;
                tracer.enabled = false;
            }
            tracer.SetPosition(0, start);
            tracer.SetPosition(1, end);
            tracer.enabled = true;
            tracerHideAt = Time.time + tracerSeconds;
        }

        public void RefillMagazine()
        {
            IsReloading = false;
            CurrentAmmo = magazineSize;
            AmmoChanged?.Invoke(CurrentAmmo, magazineSize);
        }

        public void BeginReload()
        {
            if (IsReloading || CurrentAmmo >= magazineSize) return;
            IsReloading = true;
            reloadFinishTime = Time.time + reloadSeconds;
            ReloadStarted?.Invoke();
        }
    }
}
