using System.IO;
using UnityEngine;

namespace NeonApocalypse.Save
{
    [System.Serializable]
    public class SaveData
    {
        public int version = 2;
        public int level = 1;
        public int xp = 0;
        public int perkPoints = 0;
        public int credits = 5000;
        public int helixRep = 0;
        public int chromeRep = 0;
        public int nullRep = 0;
        public int freeRunnerRep = 0;
        public int campaignMissionIndex = 0;
    }

    public static class SaveSystem
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "neon_apocalypse_save.json");

        public static void Save(SaveData data)
        {
            try
            {
                File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
            }
            catch (IOException e)
            {
                Debug.LogWarning($"Save failed: {e.Message}");
            }
        }

        public static SaveData Load()
        {
            if (!File.Exists(SavePath)) return null;
            try
            {
                return JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Save data invalid: {e.Message}");
                return null;
            }
        }
    }
}
