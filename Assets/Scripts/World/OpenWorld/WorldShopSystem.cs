using UnityEngine;
using NeonApocalypse.Combat;
using NeonApocalypse.Player;

namespace NeonApocalypse.World.OpenWorld
{
    public sealed class WorldShopSystem : MonoBehaviour
    {
        public enum ShopType { Armory, MedBay, Garage, BlackMarket }
        public ShopType shopType;
        public float interactRadius = 5f;
        public KeyCode interactKey = KeyCode.G;
        private Transform player;
        private float nextUse;

        private void Start() => player = GameObject.FindGameObjectWithTag("Player")?.transform;

        private void Update()
        {
            if (!player || Time.time < nextUse) return;
            if (Vector3.SqrMagnitude(player.position - transform.position) > interactRadius * interactRadius) return;
            if (Input.GetKeyDown(interactKey))
            {
                nextUse = Time.time + 0.5f;
                UseShop();
            }
        }

        private void UseShop()
        {
            switch (shopType)
            {
                case ShopType.Armory:
                    var weapon = player.GetComponentInChildren<Weapon>();
                    if (weapon && WorldEconomy.Instance?.TrySpend(350) == true)
                    {
                        weapon.RefillMagazine();
                        NeonApocalypse.UI.MobileHud.Instance?.ShowMessage("ARMORY: FULL MAGAZINE • 350 CR", 3f);
                    }
                    else NeonApocalypse.UI.MobileHud.Instance?.ShowMessage("ARMORY: 350 CR REQUIRED", 2f);
                    break;
                case ShopType.MedBay:
                    var health = player.GetComponent<Health>();
                    if (health && WorldEconomy.Instance?.TrySpend(450) == true)
                    {
                        health.Heal(99999f);
                        NeonApocalypse.UI.MobileHud.Instance?.ShowMessage("MED-BAY: FULL REPAIR • 450 CR", 3f);
                    }
                    break;
                case ShopType.Garage:
                    if (WorldEconomy.Instance?.TrySpend(250) == true)
                    {
                        if (VehicleEnterExit.ActiveVehicle)
                            VehicleEnterExit.ActiveVehicle.GetComponent<Health>()?.Heal(99999f);
                        WantedSystem.Instance?.ClearWanted();
                        NeonApocalypse.UI.MobileHud.Instance?.ShowMessage("GARAGE: REPAIRED + HEAT CLEARED", 3f);
                    }
                    break;
                case ShopType.BlackMarket:
                    if (WorldEconomy.Instance?.TrySpend(900) == true)
                    {
                        FactionSystem.Instance?.AddReputation(FactionId.FreeRunners, 5);
                        WantedSystem.Instance?.AddHeat(0.15f);
                        NeonApocalypse.UI.MobileHud.Instance?.ShowMessage("BLACK MARKET: ILLEGAL MOD ACQUIRED", 3f);
                    }
                    break;
            }
        }
    }
}
