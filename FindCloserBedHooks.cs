using BlockEntities;
using BlockEntities.Implementations;
using Harmony;
using Pipliz;
using System.Collections.Generic;

namespace grasmanek94.FindCloserBed
{
	[HarmonyPatch(typeof(ColonyBeds))]
	[HarmonyPatch("Add")]
	public class ColonyBedsHookAdd
	{
		static void Prefix(ColonyBeds __instance, Vector3Int blockPosition)
		{
			if (__instance.Owner == null)
			{
				return;
			}

			var trackers = CustomBedTracker.trackers;
			lock (trackers)
			{
				CustomBedTracker tracker;
				if (!trackers.TryGetValue(__instance.Owner, out tracker))
				{
					tracker = new CustomBedTracker();
					trackers.Add(__instance.Owner, tracker);
				}

				lock (tracker)
				{
					tracker.Add(blockPosition);
				}
			}
		}
	}

	[HarmonyPatch(typeof(ColonyBeds))]
	[HarmonyPatch("Remove")]
	public class ColonyBedsHookRemove
	{
		static void Prefix(ColonyBeds __instance, Vector3Int blockPosition)
		{
			if (__instance.Owner == null)
			{
				return;
			}

			var trackers = CustomBedTracker.trackers;
			lock (trackers)
			{
				CustomBedTracker tracker;
				if (!trackers.TryGetValue(__instance.Owner, out tracker))
				{
					return;
				}

				lock (tracker)
				{
					tracker.Remove(blockPosition);
					if(tracker.Count == 1)
					{
						trackers.Remove(__instance.Owner);
					}
				}
			}
		}
	}

	[HarmonyPatch(typeof(ColonyBeds))]
	[HarmonyPatch("SetUseState")]
	public class ColonyBedsHookSetUseState
	{
		static void Prefix(ColonyBeds __instance, Vector3Int blockPosition, bool state)
		{
			if(__instance.Owner == null)
			{
				return;
			}

			var trackers = CustomBedTracker.trackers;
			lock (trackers)
			{
				CustomBedTracker tracker;
				if (!trackers.TryGetValue(__instance.Owner, out tracker))
				{
					return;
				}

				lock (tracker)
				{
					tracker.SetState(blockPosition, state);
				}
			}
		}
	}

	[HarmonyPatch(typeof(ColonyBeds))]
	[HarmonyPatch("TryGetClosestUnused")]
	public class ColonyBedsHookTryGetClosestUnused
	{
		static bool Prefix(ColonyBeds __instance, ref bool __result, Vector3Int position, out Vector3Int bedPosition, out BedTracker.Bed bed, int boxradius)
		{
			bed = null;
			bedPosition = Vector3Int.invalidPos;

			if (__instance.Owner == null)
			{
				return true;
			}

			var trackers = CustomBedTracker.trackers;
			lock (trackers)
			{
				CustomBedTracker tracker;
				if (!trackers.TryGetValue(__instance.Owner, out tracker))
				{
					return true;
				}

				lock (tracker)
				{
					if(tracker.TryGetClosestUnused(position, out bedPosition, out bed, boxradius))
					{
						__result = true;
						return false;
					}
				}
			}

			return true;
		}
	}
}
