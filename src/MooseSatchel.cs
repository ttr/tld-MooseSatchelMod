using MelonLoader;
using MelonLoader.TinyJSON;
using UnityEngine;
using ModData;
using Il2Cpp;
using Il2CppTLD.Gear;


namespace MooseSatchelMod
{
    public static class BuildInfo
    {
        public const string Name = "MooseSatchelMod";
        public const string Description = "A mod to reduce scent and decay for meat, fish and guts if added to Moose Bag.";
        public const string Author = "ttr";
        public const string Company = null;
        public const string Version = "2.2.0";
        public const string DownloadLink = null;
    }
    internal class MooseSatchelMod : MelonMod
    {
        private static int dataVersion = 1;
        private static float bagMaxWeight = 5f;
        private static Dictionary<string, MooseData> MD = new Dictionary<string, MooseData>();
        private static Dictionary<string, MooseBagData> MBD = new Dictionary<string, MooseBagData>();
        internal static ModDataManager SaveMgr = new ModDataManager("MooseSatchelMod", false);

        public override void OnInitializeMelon()
        {
            Debug.Log($"[{Info.Name}] Version {Info.Version} loaded!");
            Settings.OnLoad();
        }
        public static void Log(string msg)
        {
          //MelonLogger.Msg(msg);
        }
        internal static void LoadData()
        {
            MooseSatchelMod.Log("Loading data");
            MD.Clear();
            MBD.Clear();
            string? data = SaveMgr.Load("MD");

            if (!string.IsNullOrEmpty(data))
            {
                MooseSatchelMod.Log("JSON loaded " + data);
                var foo = JSON.Load(data);
                foreach (var entry in foo as ProxyObject)
                {
                    MooseData lMD = new MooseData();
                    entry.Value.Populate(lMD);
                    MD.Add(entry.Key, lMD);
                }
            }

            data = SaveMgr.Load("MBD");
            if (!string.IsNullOrEmpty(data))
            {
                MooseSatchelMod.Log("JSON loaded " + data);
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
                string guid = ObjectGuid.GetGuidFromGameObject(gi.gameObject);
                if (!string.IsNullOrEmpty(guid) && MD.ContainsKey(guid))
                {
                    applyStats(gi, true);
                }
            }
        }

        internal static void SaveData()
        {
            //MooseSatchelMod.Log("saving data");
            SaveMgr.Save(JSON.Dump(MD, EncodeOptions.NoTypeHints), "MD");
            SaveMgr.Save(JSON.Dump(MBD, EncodeOptions.NoTypeHints), "MBD");
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
                    name.Contains("meatptarmigan") ||
                    name.Contains("cohosalmon") ||
                    name.Contains("lakewhitefish") ||
                    name.Contains("rainbowtrout") ||
                    name.Contains("burbot") ||
                    name.Contains("goldeye") ||
                    name.Contains("rockfish") ||
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
            string guid = ObjectGuid.GetGuidFromGameObject(gi.gameObject);
            if (!string.IsNullOrEmpty(guid) && MD.ContainsKey(guid))
            {
                return true;
            }
            return false;
        }
        internal static void applyStats(GearItem gi, bool freeze)
        {
            string guid = ObjectGuid.GetGuidFromGameObject(gi.gameObject);

            if (freeze)
            {
                if (isPerishableFood(gi))
                {
                    FoodItem fi = gi.GetComponent<FoodItem>();
                    fi.m_DailyHPDecayInside = MD[guid].foodDecayIndoor * Settings.options.indoor;
                    fi.m_DailyHPDecayOutside = MD[guid].foodDecayOutdoor * Settings.options.outdoor;
                }
            }
            else
            {
                if (isPerishableFood(gi))
                {
                    FoodItem fi = gi.GetComponent<FoodItem>();
                    fi.m_DailyHPDecayInside = MD[guid].foodDecayIndoor;
                    fi.m_DailyHPDecayOutside = MD[guid].foodDecayOutdoor;
                }
            }

        }
        internal static void addToBag(GearItem gi)
        {
            float gikg = gi.GetItemWeightKG().ToQuantity(1);
            string bgid = findBagSpace(gikg);
            if (!string.IsNullOrEmpty(bgid))
            {
                string guid = ObjectGuid.GetGuidFromGameObject(gi.gameObject);
                if (string.IsNullOrEmpty(guid))
                {
                    ObjectGuid.MaybeAttachObjectGuidAndRegister(gi.gameObject, Guid.NewGuid().ToString());
                    guid = ObjectGuid.GetGuidFromGameObject(gi.gameObject);
                }


                if (!MD.ContainsKey(guid))
                {
                    MooseData lMD = new MooseData();
                    MD.Add(guid, lMD);
                    MooseSatchelMod.Log("addtobag: " + gi.name + " " + gikg + " guid: " + guid + " bgid: " + bgid);
                    MD[guid].timestamp = GameManager.GetTimeOfDayComponent().GetTODSeconds(GameManager.GetTimeOfDayComponent().GetSecondsPlayedUnscaled());
                    MD[guid].ver = dataVersion;
                    MD[guid].foodId = guid;
                    MD[guid].bagId = bgid;
                    //MD[guid].scentIntensity = gi.m_Scent.GetRange();
                    MD[guid].weight = gikg;
                    MBD[bgid].weight += gikg;
                    MBD[bgid].scent += gi.m_Scent.GetRange();

                    if (isPerishableFood(gi))
                    {
                        FoodItem fi = gi.GetComponent<FoodItem>();
                        MD[guid].foodDecayIndoor = fi.m_DailyHPDecayInside;
                        MD[guid].foodDecayOutdoor = fi.m_DailyHPDecayOutside;
                    }
                    applyStats(gi, true);
                    SaveData();
                }
            }
        }
        internal static void removeFromBag(GearItem gi)
        {
            string guid = ObjectGuid.GetGuidFromGameObject(gi.gameObject);
            if (!string.IsNullOrEmpty(guid) && MD.ContainsKey(guid))
            {
                string bgid = MD[guid].bagId;
                MooseSatchelMod.Log("removefrombag: " + gi.name + " " + gi.WeightKG + "guid: " + guid + "bgid: " + bgid);
                applyStats(gi, false);

                MBD[bgid].weight -= MD[guid].weight;
                MBD[bgid].scent -= gi.m_Scent.GetRange();
                MD.Remove(guid);
                SaveData();
            }
        }
        internal static void putBag(GearItem bag)
        {
            string guid = ObjectGuid.GetGuidFromGameObject(bag.gameObject);
            if (string.IsNullOrEmpty(guid))
            {
                ObjectGuid.MaybeAttachObjectGuidAndRegister(bag.gameObject, Guid.NewGuid().ToString());
                guid = ObjectGuid.GetGuidFromGameObject(bag.gameObject);
            }
            MooseBagData lMBD = new MooseBagData();
            MBD.Add(guid, lMBD);
            MBD[guid].ver = dataVersion;
            MBD[guid].timestamp = GameManager.GetTimeOfDayComponent().GetTODSeconds(GameManager.GetTimeOfDayComponent().GetSecondsPlayedUnscaled());
            MBD[guid].bagId = guid;
            MBD[guid].weight = 0;
            MBD[guid].scent = 0;

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

            string guid = ObjectGuid.GetGuidFromGameObject(bag.gameObject);
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

                MooseSatchelMod.Log(guid);
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
        public static float baggedScent()
        {
            float scent = 0f;
            foreach (string bag in MBD.Keys)
            {
                scent += MBD[bag].scent;
            }
            MooseSatchelMod.Log("BagScent " + scent);
            return scent * (1f - Settings.options.scent);

        }
    }
    internal class MooseData {
        public int ver;
        public float timestamp;
        public string bagId;
        public string foodId;
        public float foodDecayOutdoor;
        public float foodDecayIndoor;
        public float weight;
    }
    internal class MooseBagData
    {
        public int ver;
        public float timestamp;
        public string bagId;
        public float weight;
        public float scent;
    }
}