using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace MooseSatchelMod
{
  
    internal static class Patches
    {

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.AllScenesLoaded))]
        internal class GameManager_Awake
        {
            public static void Prefix()
            {
                if (!InterfaceManager.IsMainMenuEnabled())
                {
                  MooseSatchelMod.LoadData();
                }
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddGear))]
        public class Invetory_AddGear
        {
            public static void Postfix(GearItem gi)
            {
                MooseSatchelMod.Log("AddGear " + gi.name);
                if (MooseSatchelMod.isPerishableGroup1(gi) || MooseSatchelMod.isPerishableGroup2(gi) || MooseSatchelMod.isPerishableGroup3(gi))
                {
                    MooseSatchelMod.addToBag(gi);
                }
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.DestroyGear), typeof(GameObject))]
        public class Invetory_DestroyGear
        {
            public static void Postfix(GameObject go)
            {

                MooseSatchelMod.Log("DestroyGear " + go.name);
                GearItem gi = go.GetComponent<GearItem>();
                if (MooseSatchelMod.isPerishableGroup1(gi) || MooseSatchelMod.isPerishableGroup2(gi) || MooseSatchelMod.isPerishableGroup3(gi))
                {
                    MooseSatchelMod.removeFromBag(gi);
                }
            }
        }

        [HarmonyPatch(typeof(GearItem), nameof(GearItem.Drop))]
        public class GearItem_Drop
        {
            public static void Postfix(GearItem __instance)
            {
                MooseSatchelMod.Log("Drop " + __instance.name);
                if (MooseSatchelMod.isPerishableGroup1(__instance) || MooseSatchelMod.isPerishableGroup2(__instance) || MooseSatchelMod.isPerishableGroup3(__instance))
                {
                    MooseSatchelMod.removeFromBag(__instance);
                }

            }
        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.PutOnClothingItem))]
        public class PlayerManager_PutOnClothingItem
        {
            public static void Postfix(GearItem gi)
            {
                MooseSatchelMod.Log("PutOn " + gi.name);
                if (gi.name == "GEAR_MooseHideBag")
                {
                    MooseSatchelMod.putBag(gi);
                }

            }
        }
        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.TakeOffClothingItem))]
        public class PlayerManager_TakeOffClothingItem
        {
            public static void Postfix(GearItem gi)
            {
                MooseSatchelMod.Log("TakeOff " + gi.name);
                if (gi.name == "GEAR_MooseHideBag")
                {
                    MooseSatchelMod.removeBag(gi);
                }
            }
        }
        

        [HarmonyPatch(typeof(ItemDescriptionPage), nameof(ItemDescriptionPage.UpdateGearItemDescription))]
        public class ItemDescriptionPage_UpdateGearItemDescription
        {
            private static void Postfix(ItemDescriptionPage __instance, GearItem gi)
            {
              MooseSatchelMod.Log("GearDesc: "+ gi.name);

                if (MooseSatchelMod.isBagged(gi))
                {
                  __instance.m_ItemDescLabel.text += "\n(This item is in Moose bag)";
                }
                else if (gi.name == "GEAR_MooseHideBag") {
                  __instance.m_ItemDescLabel.text += "\n Scent reduced: " + MooseSatchelMod.baggedScent();
                }
                
            }
        }
        
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