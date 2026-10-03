using System;
using UnityEngine;

namespace NitroStreetRush.Racing
{
    [Serializable]
    public sealed class SaveData
    {
        public int credits = 1000;
        public int bestTimeMs = int.MaxValue;
        public int selectedCar;
        public float nitroUpgrade = 1f;
    }

    public sealed class SaveGame : MonoBehaviour
    {
        private const string Key = "NitroStreetRush.Save";
        public SaveData Data { get; private set; }

        private void Awake()
        {
            string json = PlayerPrefs.GetString(Key, "");
            Data = string.IsNullOrEmpty(json) ? new SaveData() : JsonUtility.FromJson<SaveData>(json);
            if (Data == null) Data = new SaveData();
        }

        public void Commit()
        {
            if (Data == null) Data = new SaveData();
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
        }

        private void OnApplicationPause(bool pause) { if (pause) Commit(); }
    }
}
