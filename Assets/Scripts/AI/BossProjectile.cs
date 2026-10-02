using UnityEngine;
using NeonApocalypse.Combat;
using NeonApocalypse.Core;

namespace NeonApocalypse.AI
{
    public class BossProjectile : MonoBehaviour
    {
        public float speed = 11f;
        public float damage = 35f;
        public float lifetime = 6f;
        public float hitRadius = 0.55f;

        private Vector3 direction;
        private Transform target;
        private float expireAt;
        private System.Action<BossProjectile> release;

        public void SetPoolRelease(System.Action<BossProjectile> callback) => release = callback;

        public void Initialize(Vector3 targetPoint, float bonusDamage)
        {
            direction = (targetPoint - transform.position).normalized;
            damage = 35f + bonusDamage;
            transform.forward = direction;
            target = GameManager.Instance ? GameManager.Instance.PlayerTransform : null;
            expireAt = Time.time + lifetime;
        }

        private void Update()
        {
            if (Time.time >= expireAt) { ReleaseToPool(); return; }
            transform.position += direction * speed * Time.deltaTime;
            if (!target) return;
            Vector3 playerPoint = target.position + Vector3.up;
            if ((transform.position - playerPoint).sqrMagnitude <= (hitRadius + 0.6f) * (hitRadius + 0.6f))
            {
                target.GetComponent<Health>()?.Damage(damage);
                ReleaseToPool();
            }
        }

        public void ResetForPool()
        {
            target = null;
            expireAt = 0f;
        }

        private void ReleaseToPool()
        {
            if (release != null) release(this);
            else Destroy(gameObject);
        }
    }
}
