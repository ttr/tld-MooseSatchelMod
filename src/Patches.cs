using HarmonyLib;
using Il2Cpp;
using UnityEngine;
using UnityEngine.UIElements;

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
                if (gi.name == "GEAR_Gut" || MooseSatchelMod.isPerishableFood(gi))
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
                if (gi.name == "GEAR_Gut" || MooseSatchelMod.isPerishableFood(gi))
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
                if (__instance.name == "GEAR_Gut" || MooseSatchelMod.isPerishableFood(__instance))
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
        
        [HarmonyPatch(typeof(ItemDescriptionPage), nameof(ItemDescriptionPage.m_ItemDescLabel), MethodType.Setter)]
        public class ItemDescriptionPage_BuildItemDescription
        {
            private static void Postfix(ItemDescriptionPage __instance)
            {
                MooseSatchelMod.Log("ItemDesc " + __instance.name);
                /*
                if ((__instance.name == "GEAR_Gut" || MooseSatchelMod.isPerishableFood(__instance)) && MooseSatchelMod.isBagged(__instance))
                {
                    MooseSatchelMod.Log("BagDesc " + __instance.name);
                    __result += "\n(This item is in Moose bag)";
                }
                else if (__instance.name == "GEAR_MooseHideBag") {
                  __result += "\n Scent reduced: " + MooseSatchelMod.baggedScent();
                }
                */
            }
        }
        [HarmonyPatch(typeof(ItemDescriptionPage), nameof(ItemDescriptionPage.UpdateGearItemDescription))]
        public class ItemDescriptionPage_UpdateGearItemDescription
        {
            private static void Postfix(ItemDescriptionPage __instance, GearItem gi)
            {
              MooseSatchelMod.Log("GearDesc: "+ gi.name);

                if ((gi.name == "GEAR_Gut" || MooseSatchelMod.isPerishableFood(gi)) && MooseSatchelMod.isBagged(gi))
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