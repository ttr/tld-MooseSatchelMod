using MelonLoader;
using MelonLoader.TinyJSON;
using UnityEngine;
using System.Collections.Generic;
using System;

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
            MBD.Clear();
            string data = SaveGameSlots.LoadDataFromSlot(name + "d", SAVE_NAME);
            if (!string.IsNullOrEmpty(data))
            {
                //MelonLogger.Log("JSON loaded " + data);
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
                //MelonLogger.Log("JSON loaded " + data);
                var foo = JSON.Load(data);
                foreach (var entry in foo as ProxyObject)
                {
                    MooseBagData lMBD = new MooseBagData();
                    entry.Value.Populate(lMBD);
                    MBD.Add(entry.Key, lMBD);
                }
            }
            // look for items in player inverntory and apply stats
            Inventory inventoryComponent = GameManager.GetInventoryComponent();
            foreach (GearItemObject item in inventoryComponent.m_Items)
            {
                GearItem gi = item;
                string guid = Utils.GetGuidFromGameObject(gi.gameObject);
                if (!string.IsNullOrEmpty(guid) && MD.ContainsKey(guid))
                {
                    applyStats(gi, true);
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

        public static bool isPerishableFood(GearItem gi)
        {
            string name = gi.name.ToLower();
            if (gi.m_FoodItem)
            {
                if (
                    name.Contains("meatbear") ||
                    name.Contains("meatdeer") ||
                    name.Contains("meatrabbit") ||
                    name.Contains("meatwolf") ||
                    name.Contains("meatmoose") ||
                    name.Contains("cohosalmon") ||
                    name.Contains("lakewhitefish") ||
                    name.Contains("rainbowtrout") ||
                    name.Contains("smallmouthbass")
                    )
                {
                    return true;

                }
            }
            return false;
        }

        public static bool isBagged(GearItem gi)
        {
            string guid = Utils.GetGuidFromGameObject(gi.gameObject);
            if (!string.IsNullOrEmpty(guid) && MD.ContainsKey(guid))
            {
                return true;
            }
            return false;
        }
        internal static void applyStats(GearItem gi, bool freeze)
        {
            string guid = Utils.GetGuidFromGameObject(gi.gameObject);

            if (freeze)
            {
                if (isPerishableFood(gi))
                {
                    FoodItem fi = gi.GetComponent<FoodItem>();
                    fi.m_DailyHPDecayInside = MD[guid].foodDecayIndoor * Settings.options.indoor;
                    fi.m_DailyHPDecayOutside = MD[guid].foodDecayOutdoor * Settings.options.outdoor;
                }
                gi.m_ScentIntensity = MD[guid].scentIntensity * Settings.options.scent;
            }
            else
            {
                if (isPerishableFood(gi))
                {
                    FoodItem fi = gi.GetComponent<FoodItem>();
                    fi.m_DailyHPDecayInside = MD[guid].foodDecayIndoor;
                    fi.m_DailyHPDecayOutside = MD[guid].foodDecayOutdoor;
                }
                gi.m_ScentIntensity = MD[guid].scentIntensity;
            }

        }
        internal static void addToBag(GearItem gi)
        {
            string bgid = findBagSpace(gi.m_WeightKG);
            if (!string.IsNullOrEmpty(bgid))
            {
                string guid = Utils.GetGuidFromGameObject(gi.gameObject);
                if (string.IsNullOrEmpty(guid))
                {
                    Utils.SetGuidForGameObject(gi.gameObject, Guid.NewGuid().ToString());
                    guid = Utils.GetGuidFromGameObject(gi.gameObject);
                }
                //MelonLogger.Log("addtobag: " + gi.name + " " + gi.m_WeightKG + "guid: " + guid + "bgid: " + bgid);

                if (!MD.ContainsKey(guid))
                {
                    MooseData lMD = new MooseData();
                    MD.Add(guid, lMD);
                }
                MD[guid].timestamp = GameManager.GetTimeOfDayComponent().GetTODSeconds(GameManager.GetTimeOfDayComponent().GetSecondsPlayedUnscaled());
                MD[guid].ver = dataVersion;
                MD[guid].foodId = guid;
                MD[guid].bagId = bgid;
                MD[guid].scentIntensity = gi.m_ScentIntensity;
                MD[guid].weight = gi.m_WeightKG;
                MBD[bgid].weight += gi.m_WeightKG;

                if (isPerishableFood(gi))
                {
                    FoodItem fi = gi.GetComponent<FoodItem>();
                    MD[guid].foodDecayIndoor = fi.m_DailyHPDecayInside;
                    MD[guid].foodDecayOutdoor = fi.m_DailyHPDecayOutside;
                }
                applyStats(gi, true);

            }
        }
        internal static void removeFromBag(GearItem gi)
        {
            string guid = Utils.GetGuidFromGameObject(gi.gameObject);
            if (!string.IsNullOrEmpty(guid) && MD.ContainsKey(guid))
            {
                string bgid = MD[guid].bagId;
                //MelonLogger.Log("removefrombag: " + gi.name + " " + gi.m_WeightKG + "guid: " + guid + "bgid: " + bgid);
                applyStats(gi, false);

                MBD[bgid].weight -= MD[guid].weight;
                MD.Remove(guid);

            }
        }
        internal static void putBag(GearItem bag)
        {
            string guid = Utils.GetGuidFromGameObject(bag.gameObject);
            if (string.IsNullOrEmpty(guid))
            {
                Utils.SetGuidForGameObject(bag.gameObject, Guid.NewGuid().ToString());
                guid = Utils.GetGuidFromGameObject(bag.gameObject);
            }
            MooseBagData lMBD = new MooseBagData();
            MBD.Add(guid, lMBD);
            MBD[guid].ver = dataVersion;
            MBD[guid].timestamp = GameManager.GetTimeOfDayComponent().GetTODSeconds(GameManager.GetTimeOfDayComponent().GetSecondsPlayedUnscaled());
            MBD[guid].bagId = guid;
            MBD[guid].weight = 0;

            // loop via inventory and add stuff
            Inventory inventoryComponent = GameManager.GetInventoryComponent();
            foreach (GearItemObject item in inventoryComponent.m_Items)
            {
                GearItem gi = item;
                if (gi.name == "GEAR_Gut" || isPerishableFood(gi))
                {
                    addToBag(gi);
                }
            }
        }
        internal static void removeBag(GearItem bag)
        {

            string guid = Utils.GetGuidFromGameObject(bag.gameObject);
            if (!string.IsNullOrEmpty(guid))
            {
                // loop via inventory and remove stuff
                Inventory inventoryComponent = GameManager.GetInventoryComponent();
                foreach (GearItemObject item in inventoryComponent.m_Items)
                {
                    GearItem gi = item;
                    if (gi.name == "GEAR_Gut" || isPerishableFood(gi))
                    {
                        removeFromBag(gi);
                    }
                }

                //MelonLogger.Log(guid);
                MBD.Remove(guid);
            }
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