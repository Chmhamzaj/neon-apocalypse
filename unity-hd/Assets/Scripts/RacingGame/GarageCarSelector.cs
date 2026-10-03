using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class GarageCarSelector : MonoBehaviour
    {
        [SerializeField] private GameObject[] carPrefabs;
        [SerializeField] private Transform displayRoot;
        [SerializeField] private SaveGame saveGame;
        private GameObject preview;

        public int SelectedIndex => saveGame ? saveGame.Data.selectedCar : 0;

        private void Awake()
        {
            if (!saveGame) saveGame = FindFirstObjectByType<SaveGame>();
            RefreshPreview();
        }

        public void SelectCar(int index)
        {
            if (carPrefabs == null || carPrefabs.Length == 0) return;
            index = Mathf.Clamp(index, 0, carPrefabs.Length - 1);
            if (saveGame) { saveGame.Data.selectedCar = index; saveGame.Commit(); }
            RefreshPreview();
        }

        private void RefreshPreview()
        {
            if (!displayRoot || carPrefabs == null || carPrefabs.Length == 0) return;
            if (preview) Destroy(preview);
            int index = Mathf.Clamp(SelectedIndex, 0, carPrefabs.Length - 1);
            preview = Instantiate(carPrefabs[index], displayRoot);
            preview.transform.localPosition = Vector3.zero;
            preview.transform.localRotation = Quaternion.Euler(0f, 145f, 0f);
            preview.transform.localScale = Vector3.one;
            var controller = preview.GetComponentInChildren<CarController>();
            if (controller) controller.enabled = false;
        }
    }
}