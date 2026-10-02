using System.Collections.Generic;
using UnityEngine;

namespace NeonApocalypse.Combat
{
    public class DamageNumber : MonoBehaviour
    {
        public float defaultLifetime = 0.65f;
        public float riseSpeed = 1.2f;
        private float lifetime;
        private Camera cam;
        private static readonly Stack<DamageNumber> Pool = new Stack<DamageNumber>(24);
        private static Transform poolRoot;

        private void Awake() => cam = Camera.main;

        private void OnEnable()
        {
            lifetime = defaultLifetime;
            if (!cam) cam = Camera.main;
        }

        private void Update()
        {
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;
            if (cam) transform.forward = cam.transform.forward;
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f) Release();
        }

        public static void Spawn(Vector3 position, float value)
        {
            DamageNumber fx = null;
            while (Pool.Count > 0 && !fx)
                fx = Pool.Pop();

            if (!fx)
            {
                var go = new GameObject("DamageNumberPoolUnit");
                go.transform.SetParent(GetPoolRoot(), false);
                fx = go.AddComponent<DamageNumber>();
            }

            fx.transform.position = position + Vector3.up * 1.8f;
            fx.transform.localScale = Vector3.one * Mathf.Clamp(0.8f + value / 250f, 0.8f, 1.6f);
            fx.riseSpeed = 1.2f + Mathf.Clamp(value / 150f, 0f, 1.8f);
            fx.gameObject.SetActive(true);
        }

        private void Release()
        {
            gameObject.SetActive(false);
            transform.SetParent(GetPoolRoot(), false);
            if (Pool.Count < 24) Pool.Push(this);
            else Destroy(gameObject);
        }

        private static Transform GetPoolRoot()
        {
            if (poolRoot) return poolRoot;
            var go = new GameObject("FX_Pool_DamageNumbers");
            DontDestroyOnLoad(go);
            poolRoot = go.transform;
            return poolRoot;
        }
    }
}
