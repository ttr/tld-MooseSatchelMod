using HarmonyLib;
using Il2Cpp;
using Il2CppSteamworks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace MooseSatchelMod
{
  
    internal static class Patches
    {

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.DoExitToMainMenu))]
        [HarmonyPatch(typeof(GameManager), nameof(GameManager.LoadMainMenu))]
        private static class ModData_GameManager_MainMenu
        {
            private static void Postfix()
            {
                MooseSatchelMod.ClearData();
            }
        }
        [HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.RestoreGlobalData))]
        internal class SaveGameSystem_RestoreGlobalData
        {
            public static void Postfix(string name)
            {
                MooseSatchelMod.Log($"Loading save date {name}");
                MooseSatchelMod.ClearData();
                MooseSatchelMod.LoadData();
            }
        }
        [HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.SaveGlobalData))]
        internal class SaveGameSystem_SaveGlobalData
        {
            public static void Postfix(SlotData slot)
            {
                MooseSatchelMod.SaveData();
            }
        }
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddGear))]
        public class Invetory_AddGear
        {
            public static void Postfix(GearItem gi)
            {
                if (MooseSatchelMod.SaveDataLoaded) {
                    MooseSatchelMod.Log("AddGear " + gi.name);
                    if (MooseSatchelMod.CanBeBagged(gi))
                    {
                        MooseSatchelMod.addToBag(gi);
                    }
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
                if (MooseSatchelMod.GetScent(gi.name) > -1) // use GetScent to remove items from bag if setings where changed during gameplay
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
                if (MooseSatchelMod.GetScent(__instance.name) > -1 ) // use GetScent to remove items from bag if setings where changed during gameplay
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
                if (MooseSatchelMod.SaveDataLoaded && gi.name == "GEAR_MooseHideBag") {
                    MooseSatchelMod.Log("PutOn " + gi.name);
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
              MooseSatchelMod.Log("GearDesc: " + gi.name + "|");

                if (MooseSatchelMod.isBagged(gi))
                {
                  __instance.m_ItemDescLabel.text += "\n(This item is in Moose bag)";
                }
                else if (gi.name == "GEAR_MooseHideBag") {
                  __instance.m_ItemDescLabel.text += "\n Scent reduced (total): " + MooseSatchelMod.baggedScent();
                }
            }
        }
        
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.GetExtraScentIntensity))]
        internal class Inventory_GetExtraScentIntensity
        {
            private static void Postfix(Inventory __instance, ref float __result)
            {
                    __result -= MooseSatchelMod.baggedScent();
                    //MooseSatchelMod.Log("total:" + __instance.m_TotalScentIntensity);

            }
        }
    }
}