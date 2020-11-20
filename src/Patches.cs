using Harmony;
using MelonLoader;
using UnityEngine;

namespace MooseSatchelMod
{
    internal static class Patches
    {

        [HarmonyPatch(typeof(Inventory), "AddGear")]
        public class Invetory_AddGear
        {
            public static void Postfix(GameObject go)
            {
                GearItem gi = go.GetComponent<GearItem>();
                if (gi.m_FoodItem)
                {
                    FoodItem fi = go.GetComponent<FoodItem>();
                    MelonLogger.Log("AddGear" + go.name + " " + gi.m_FoodWeight + " " + gi.m_WeightKG + " " + fi.m_DailyHPDecayInside + " " + fi.m_DailyHPDecayOutside);
                }
                if (gi.name == "GEAR_MooseHideBag")
                {

                    MelonLogger.Log("AddGear" + go.name + " " + gi.m_FoodWeight + " " + gi.m_WeightKG + " " + gi.m_ClothingItem);
                }
            }
        }

        [HarmonyPatch(typeof(Inventory), "RemoveGear", typeof(GameObject))]
        public class Invetory_RemoveGear
        {
            public static void Postfix(GameObject go)
            {

                MelonLogger.Log("RemoveGear " + go.name);

            }
        }

        [HarmonyPatch(typeof(PlayerManager), "PutOnClothingItem")]
        public class PlayerManager_PutOnClothingItem
        {
            public static void Postfix(GearItem gi)
            {
                MelonLogger.Log("PutOn " + gi.name);
            }
        }
        [HarmonyPatch(typeof(PlayerManager), "TakeOffClothingItem")]
        public class PlayerManager_TakeOffClothingItem
        {
            public static void Postfix(GearItem gi)
            {
                MelonLogger.Log("TakeOff " + gi.name);
            }
        }

    }
}