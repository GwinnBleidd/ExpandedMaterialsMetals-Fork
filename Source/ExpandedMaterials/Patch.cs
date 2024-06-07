using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using HarmonyLib;
using Verse;

namespace ExpandedMaterials
{
    [StaticConstructorOnStartup]
    public class Patcher
    {
        static Patcher()
        {
            var harmony = new Harmony("Argon.VMEu");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(CompRefuelable))]
    [HarmonyPatch("Refuel")]
    [HarmonyPatch(new Type[] { typeof(List<Thing>) })]
    class Patch
    {
        static void Prefix(ref List<Thing> fuelThings)
        {
            foreach (Thing thing in fuelThings)
            {
                float finalFuelAmount;
                finalFuelAmount = thing.stackCount * (thing.def.HasModExtension<ThingDefExtension>() ? thing.def.GetModExtension<ThingDefExtension>().fuelPower : 1f);
                thing.stackCount = Convert.ToInt32(finalFuelAmount);
            }
        }
    }

	[HarmonyPatch(typeof(ThingSetMaker_Meteorite))]
	[HarmonyPatch("Reset")]
	[HarmonyPatch(new Type[] {})]
	class PatchMeteoriteReset {
		static bool Prefix() {
			List<string> ignoredNames = new List<string>() { "MineableComponentsIndustrial", "MineableSteel", "MineablePlasteel"};

			ThingSetMaker_Meteorite.nonSmoothedMineables.Clear();
			ThingSetMaker_Meteorite.nonSmoothedMineables.AddRange(
				from x in DefDatabase<ThingDef>.AllDefsListForReading
				where x.mineable && x != ThingDefOf.CollapsedRocks && x != ThingDefOf.RaisedRocks && !x.IsSmoothed && !ignoredNames.Contains(x.defName)
				select x
			);
			return false;
		}
	}

	[HarmonyPatch(typeof(ThingSetMaker_Meteorite))]
	[HarmonyPatch("FindRandomMineableDef")]
	class PatchMeteoriteFindRandom {
		static bool Prefix(ref ThingDef __result) {
			float value = Rand.Value;
			if (value <= 0.667f) {
				__result = (
					from x in ThingSetMaker_Meteorite.nonSmoothedMineables
					where !x.building.isResourceRock
					select x
				).RandomElement<ThingDef>();
			} else if(value <= 0.917f) {
				__result = (
					from x in ThingSetMaker_Meteorite.nonSmoothedMineables
					where x.building.isResourceRock && x.building.mineableThing.BaseMarketValue < 5f
					select x
				).RandomElement<ThingDef>();
			} else {
				__result = (
					from x in ThingSetMaker_Meteorite.nonSmoothedMineables
					where x.building.isResourceRock && x.building.mineableThing.BaseMarketValue >= 5f
					select x
				).RandomElement<ThingDef>();
			}
			return false;
		}
	}
}
