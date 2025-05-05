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
        public const string Version = "2.3.2";
        public const string DownloadLink = null;
    }
    internal class MooseSatchelMod : MelonMod
    {
        private static int dataVersion = 4;
        private static float bagMaxWeight = 5f;
        private static Dictionary<string, MooseData> MD = new Dictionary<string, MooseData>();
        private static Dictionary<string, MooseBagData> MBD = new Dictionary<string, MooseBagData>();
        internal static ModDataManager SaveMgr = new ModDataManager("MooseSatchelMod", false);
        internal static bool SaveDataLoaded = false;

        public override void OnInitializeMelon()
        {
            Debug.Log($"[{Info.Name}] Version {Info.Version} loaded!");
            Settings.OnLoad();
        }
        public static void Log(string msg)
        {
          //MelonLogger.Msg(msg);
        }
        internal static void ClearData(){
            MD.Clear();
            MBD.Clear();
            SaveDataLoaded = false;
            MooseSatchelMod.Log("data cleared");
        }
        internal static void LoadData()
        {
            MooseSatchelMod.Log("Loading data");
            ClearData();
            string? data = SaveMgr.Load("MD");

            if (!string.IsNullOrEmpty(data))
            {
                MooseSatchelMod.Log("JSON loaded " + data);
                var foo = JSON.Load(data);
                foreach (var entry in foo as ProxyObject)
                {
                    MooseData lMD = new MooseData();
                    entry.Value.Populate(lMD);
                    if (lMD.ver == dataVersion) {
                        MD.Add(entry.Key, lMD);
                    }
                    
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
                    if (lMBD.ver == dataVersion) {
                        MBD.Add(entry.Key, lMBD);
                    }
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
            SaveDataLoaded = true;
        }

        internal static void SaveData()
        {
            MooseSatchelMod.Log("saving data");
            SaveMgr.Save(JSON.Dump(MD, EncodeOptions.NoTypeHints), "MD");
            SaveMgr.Save(JSON.Dump(MBD, EncodeOptions.NoTypeHints), "MBD");
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
                if (gi.m_FoodItem)
                {
                    FoodItem fi = gi.GetComponent<FoodItem>();
                    fi.m_DailyHPDecayInside = MD[guid].foodDecayIndoor * Settings.options.indoor;
                    fi.m_DailyHPDecayOutside = MD[guid].foodDecayOutdoor * Settings.options.outdoor;
                }
            }
            else
            {
                if (gi.m_FoodItem)
                {
                    FoodItem fi = gi.GetComponent<FoodItem>();
                    fi.m_DailyHPDecayInside = MD[guid].foodDecayIndoor;
                    fi.m_DailyHPDecayOutside = MD[guid].foodDecayOutdoor;
                }
            }
            MooseSatchelMod.Log("applyStats: " + gi.name + " " + freeze);

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
                    MooseSatchelMod.Log("addtobag: " + gi.name + " " + gikg + " guid: " + guid + " bgid: " + bgid + " scent: " + gi.m_Scent?.GetRange() + "/" + GetScent(gi.name));
                    MD[guid].timestamp = GameManager.GetTimeOfDayComponent().GetTODSeconds(GameManager.GetTimeOfDayComponent().GetSecondsPlayedUnscaled());
                    MD[guid].ver = dataVersion;
                    MD[guid].foodId = guid;
                    MD[guid].bagId = bgid;
                    //MD[guid].scentIntensity = gi.m_Scent.GetRange();
                    MD[guid].weight = gikg;
                    MBD[bgid].weight += gikg;
                    if (gi.m_Scent) 
                    {
                        MBD[bgid].scent += GetScent(gi.name); //gi.m_Scent.GetRange();
                    }

                    if (gi.m_FoodItem)
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
                MooseSatchelMod.Log("removefrombag: " + gi.name + " " + MD[guid].weight + "guid: " + guid + "bgid: " + bgid);
                applyStats(gi, false);

                MBD[bgid].weight -= MD[guid].weight;
                if (gi.m_Scent) 
                {
                    MBD[bgid].scent -= GetScent(gi.name); //gi.m_Scent.GetRange();
                }

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
            if (!MBD.ContainsKey(guid)) { // scene change - bag is 're-equipped after data is loaded'
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
                    if (CanBeBagged(gi))
                    {
                        addToBag(gi);
                    }
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
                    if (GetScent(gi.name) > -1) // use GetScent to remove items from bag if setings where changed during gameplay
                    {
                        removeFromBag(gi);
                    }
                }

                MooseSatchelMod.Log("removeBag: " + guid);
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
            return "";
        }
        public static float baggedScent()
        {
            float scent = 0f;
            foreach (string bag in MBD.Keys)
            {
                scent += MBD[bag].scent;
            }
            return scent * (1f - Settings.options.scent);

        }
        public static int GetScent(string gi)
        {
            if ( gi == "GEAR_Gut" ) { return 20; }
            else if ( gi == "GEAR_AnimalFat" ) { return 5; }
            else if ( gi.StartsWith("GEAR_RawMeat") || gi == "GEAR_RawCohoSalmon" || gi == "GEAR_RawLakeWhiteFish" || gi == "GEAR_RawRainbowTrout" || gi == "GEAR_RawBurbot" || gi == "GEAR_RawGoldeye" || gi == "GEAR_RawRockfish" || gi == "GEAR_RawSmallMouthBass" || gi == "GEAR_RawRedIrishLord") { return 15;} 
            else if ( gi.StartsWith("GEAR_CookedMeat") || gi == "GEAR_CookedCohoSalmon" || gi == "GEAR_CookedLakeWhiteFish" || gi == "GEAR_CookedRainbowTrout" || gi == "GEAR_CookedBurbot" || gi == "GEAR_CookedGoldeye" || gi == "GEAR_CookedRockfish" || gi == "GEAR_CookedSmallMouthBass" || gi == "GEAR_CookedRedIrishLord") { return 5;}
            else if ( gi == "GEAR_CookedPieVenison" || gi == "GEAR_CookedPieRabbit" || gi == "GEAR_CookedPiePtarmigan" || gi == "GEAR_CookedPieMeat" || gi == "GEAR_CookedPiePredator" || gi == "GEAR_CookedFishcakes") { return 5;}
            else if ( gi == "GEAR_CookedPieFishermans") { return -1;}
            else if ( gi.StartsWith("GEAR_CookedPie") || gi.StartsWith("GEAR_Cured") ) { return 0;}
            return -1;
        }
        public static bool CanBeBagged(GearItem gi) {
            int treshold = 0;
            if (Settings.options.storeNonSmellyCooked) {
                treshold = -1;
            }
            if (GetScent(gi.name) > treshold) {
                return true;
            }
            return false;
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
