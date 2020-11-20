using MelonLoader;
using MelonLoader.TinyJSON;
using UnityEngine;
using System.Collections.Generic;

namespace MooseSatchelMod
{
    internal class MooseSatchelMod : MelonMod
    {
        private static int dataVersion = 1;
        private const string SAVE_NAME = "mooseSatchel";
        private static float bagMaxWeight = 5f;
        private static Dictionary<string, MooseData> MD = new Dictionary<string, MooseData>();
        private static Dictionary<string, MooseBagData> MBD = new Dictionary<string, MooseBagData>();
        public override void OnApplicationStart()
        {
            Debug.Log($"[{InfoAttribute.Name}] Version {InfoAttribute.Version} loaded!");
            Settings.OnLoad();
        }

        internal static void LoadData(string name)
        {
            MD.Clear();
            string data = SaveGameSlots.LoadDataFromSlot(name+"d", SAVE_NAME);
            if (!string.IsNullOrEmpty(data))
            {
                MelonLogger.Log("JSON loaded " + data);
                var foo = JSON.Load(data);
                foreach (var entry in foo as ProxyObject)
                {
                    MooseData lMD = new MooseData();
                    entry.Value.Populate(lMD);
                    MD.Add(entry.Key, lMD);
                }
            }
            data = SaveGameSlots.LoadDataFromSlot(name + "b", SAVE_NAME);
            if (!string.IsNullOrEmpty(data))
            {
                MelonLogger.Log("JSON loaded " + data);
                var foo = JSON.Load(data);
                foreach (var entry in foo as ProxyObject)
                {
                    MooseBagData lMBD = new MooseBagData();
                    entry.Value.Populate(lMBD);
                    MBD.Add(entry.Key, lMBD);
                }
            }
        }

        internal static void SaveData(SaveSlotType gameMode, string name)
        {
            string data = JSON.Dump(MD, EncodeOptions.NoTypeHints);
            SaveGameSlots.SaveDataToSlot(gameMode, SaveGameSystem.m_CurrentEpisode, SaveGameSystem.m_CurrentGameId, name +"d", SAVE_NAME, data);
            data = JSON.Dump(MBD, EncodeOptions.NoTypeHints);
            SaveGameSlots.SaveDataToSlot(gameMode, SaveGameSystem.m_CurrentEpisode, SaveGameSystem.m_CurrentGameId, name + "b", SAVE_NAME, data);

        }

        internal static void addToBag(GearItem gi)
        {
            string bguid = findBagSpace(gi.m_WeightKG);
            if (bguid != null)
            {
                string guid = Utils.GetGuidFromGameObject(gi.gameObject);
                FoodItem fi = gi.GetComponent<FoodItem>();
                if (!MD.ContainsKey(guid))
                {
                    MooseData lMD = new MooseData();
                    MD.Add(guid, lMD);
                }
                MD[guid].timestamp = GameManager.GetTimeOfDayComponent().GetTODSeconds(GameManager.GetTimeOfDayComponent().GetSecondsPlayedUnscaled());
                MD[guid].ver = dataVersion;
                MD[guid].foodId = guid;
                MD[guid].bagId = bguid;
                MD[guid].foodDecayIndoor = fi.m_DailyHPDecayInside;
                MD[guid].foodDecayOutdoor = fi.m_DailyHPDecayOutside;
                MD[guid].scentIntensity = gi.m_ScentIntensity;
                MD[guid].weight = gi.m_WeightKG;
                MBD[bguid].weight += gi.m_WeightKG;

                fi.m_DailyHPDecayInside = MD[guid].foodDecayIndoor / 2f;
                fi.m_DailyHPDecayOutside = 0;
                gi.m_ScentIntensity = MD[guid].scentIntensity * 0.1f;
            }
        }
        internal static void removeFromBag(GearItem gi)
        {
            string guid = Utils.GetGuidFromGameObject(gi.gameObject);
            if (MD.ContainsKey(guid))
            {
                FoodItem fi = gi.GetComponent<FoodItem>();
                fi.m_DailyHPDecayInside = MD[guid].foodDecayIndoor;
                fi.m_DailyHPDecayOutside = MD[guid].foodDecayOutdoor;
                gi.m_ScentIntensity = MD[guid].scentIntensity;

                string bgid = MD[guid].bagId;
                MBD[bgid].weight -= MD[guid].weight;
                MD.Remove(guid);
            }

        }
        internal static void putBag(GearItem gi)
        {
            string guid = Utils.GetGuidFromGameObject(gi.gameObject);
            MooseBagData lMBD = new MooseBagData();
            MBD.Add(guid, lMBD);
            MBD[guid].ver = dataVersion;
            MBD[guid].timestamp = GameManager.GetTimeOfDayComponent().GetTODSeconds(GameManager.GetTimeOfDayComponent().GetSecondsPlayedUnscaled());
            MBD[guid].bagId = guid;
            MBD[guid].weight = 0;

            // loop via inventory and add stuff
        }
        internal static void removeBag(GearItem gi)
        {
            // loop via inventory and remove stuff

            string guid = Utils.GetGuidFromGameObject(gi.gameObject);
            MBD.Remove(guid);


        }
        public static string findBagSpace(float weight)
        {
            foreach (var entry in MBD)
            {
                string guid = entry.Key;
                if ((MBD[guid].weight + weight) <= bagMaxWeight)
                {
                    return guid;
                }
            }
            return null;
        }
    }
    internal class MooseData {
        public int ver;
        public float timestamp;
        public string bagId;
        public string foodId;
        public float foodDecayOutdoor;
        public float foodDecayIndoor;
        public float scentIntensity;
        public float weight;
    }
    internal class MooseBagData
    {
        public int ver;
        public float timestamp;
        public string bagId;
        public float weight;
    }
}