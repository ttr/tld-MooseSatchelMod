using HarmonyLib;
using UnityEngine;
using Il2Cpp;
using MelonLoader;


namespace MooseSatchelMod
{
    internal static class Patches
    {
 
        [HarmonyPatch(typeof(GameManager), "AllScenesLoaded")]
        internal class GameManager_Awake
        {
            public static void Prefix()
            {
                //if (!InterfaceManager.IsMainMenuEnabled())
                if (GameManager.GetInventoryComponent())
                {
                    MooseSatchelMod.LoadData();
                }
                MooseSatchelMod.LoadData();
            }
        }
        [HarmonyPatch(typeof(Inventory), "AddGear")]
        public class Invetory_AddGear
        {
            public static void Postfix(GearItem gi)
            {
                //MelonLogger.Msg("AddGear " + gi.name);
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

                //MelonLogger.Msg("DestroyGear " + go.name);
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
                //MelonLogger.Msg("Drop " + __instance.name);
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
                //MelonLogger.Msg("PutOn " + gi.name);
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
                //MelonLogger.Msg("TakeOff " + gi.name);
                if (gi.name == "GEAR_MooseHideBag")
                {
                    MooseSatchelMod.removeBag(gi);
                }
            }
        }
        /*
        [HarmonyPatch(typeof(ItemDescriptionPage), "BuildItemDescription")]
        public class ItemDescriptionPage_BuildItemDescription
        {
            private static void Postfix(GearItem gi, ref string __result)
            {
                MelonLogger.Msg("ItemDesc " + gi.name);
                if ((gi.name == "GEAR_Gut" || MooseSatchelMod.isPerishableFood(gi)) && MooseSatchelMod.isBagged(gi))
                {
                    //MelonLogger.Msg("BagDesc " + gi.name);
                    __result += "\n(This item is in Moose bag)";
                }
            }
        }
        */
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.GetExtraScentIntensity))]
        internal class Inventory_GetExtraScentIntensity
        {
            private static void Postfix(ref float __result)
            {
                    __result -= MooseSatchelMod.baggedScent();
            }
        }
    }

}