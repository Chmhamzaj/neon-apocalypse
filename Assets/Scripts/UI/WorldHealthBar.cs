using UnityEngine;
using UnityEngine.UI;
using NeonApocalypse.Combat;

public class WorldHealthBar : MonoBehaviour
{
    public Health target;
    public Slider slider;
    public Camera worldCamera;

    private void Start()
    {
        if (!target) target = GetComponentInParent<Health>();
        if (!worldCamera) worldCamera = Camera.main;
        if (target) target.Damaged += OnDamaged;
    }

    private void LateUpdate()
    {
        if (!target || !worldCamera) return;
        transform.forward = worldCamera.transform.forward;
        if (slider) slider.value = target.Current / target.maxHealth;
        gameObject.SetActive(!target.IsDead);
    }

    private void OnDamaged(float current, float max) { if (slider) slider.value = current / max; }
}
