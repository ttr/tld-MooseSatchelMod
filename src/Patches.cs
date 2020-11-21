using Harmony;
using System;
using MelonLoader;
using UnityEngine;

namespace MooseSatchelMod
{
    internal static class Patches
    {
        // load and save custom data
        [HarmonyPatch(typeof(SaveGameSystem), "RestoreGlobalData", new Type[] { typeof(string) })]
        internal class SaveGameSystemPatch_RestoreGlobalData
        {
            internal static void Postfix(string name)
            {
                MooseSatchelMod.LoadData(name);
            }
        }

        [HarmonyPatch(typeof(SaveGameSystem), "SaveGlobalData", new Type[] { typeof(SaveSlotType), typeof(string) })]
        internal class SaveGameSystemPatch_SaveGlobalData
        {
            public static void Postfix(SaveSlotType gameMode, string name)
            {
                MooseSatchelMod.SaveData(gameMode, name);
            }
        }

        [HarmonyPatch(typeof(Inventory), "AddGear")]
        public class Invetory_AddGear
        {
            public static void Postfix(GameObject go)
            {
                //MelonLogger.Log("AddGear " + go.name);
                GearItem gi = go.GetComponent<GearItem>();
                if (gi.name == "GEAR_Gut" || MooseSatchelMod.isPerishableFood(gi))
                {
                    MooseSatchelMod.addToBag(gi);
                }
            }
        }

        [HarmonyPatch(typeof(Inventory), "DestroyGear", typeof(GameObject))]
        public class Invetory_DestroyGear
        {
            public static void Postfix(GameObject go)
            {

                //MelonLogger.Log("DestroyGear " + go.name);
                GearItem gi = go.GetComponent<GearItem>();
                if (gi.name == "GEAR_Gut" || MooseSatchelMod.isPerishableFood(gi))
                {
                    MooseSatchelMod.removeFromBag(gi);
                }
            }
        }

        [HarmonyPatch(typeof(GearItem), "Drop")]
        public class GearItem_Drop
        {
            public static void Postfix(GearItem __instance)
            {
                //MelonLogger.Log("Drop " + __instance.name);
                if (__instance.name == "GEAR_Gut" || MooseSatchelMod.isPerishableFood(__instance))
                {
                    MooseSatchelMod.removeFromBag(__instance);
                }

            }
        }

        [HarmonyPatch(typeof(PlayerManager), "PutOnClothingItem")]
        public class PlayerManager_PutOnClothingItem
        {
            public static void Postfix(GearItem gi)
            {
                //MelonLogger.Log("PutOn " + gi.name);
                if (gi.name == "GEAR_MooseHideBag")
                {
                    MooseSatchelMod.putBag(gi);
                }

            }
        }
        [HarmonyPatch(typeof(PlayerManager), "TakeOffClothingItem")]
        public class PlayerManager_TakeOffClothingItem
        {
            public static void Postfix(GearItem gi)
            {
                //MelonLogger.Log("TakeOff " + gi.name);
                if (gi.name == "GEAR_MooseHideBag")
                {
                    MooseSatchelMod.removeBag(gi);
                }
            }
        }
        [HarmonyPatch(typeof(ItemDescriptionPage), "BuildItemDescription")]
        public class ItemDescriptionPage_BuildItemDescription
        {
            private static void Postfix(GearItem gi, ref string __result)
            {
                if ((gi.name == "GEAR_Gut" || MooseSatchelMod.isPerishableFood(gi)) && MooseSatchelMod.isBagged(gi))
                {
                    //MelonLogger.Log("BagDesc " + gi.name);
                    __result += "\n(This item is in Moose bag)";
                }
            }
        }
    }

}