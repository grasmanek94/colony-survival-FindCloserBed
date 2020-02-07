using BlockEntities.Implementations;
using Harmony;
using NPC;
using Pipliz;
using System.Reflection;

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

			// Log.WriteWarning("FindCloserBed: Did not find any available beds");
			return true;
		}

		static void Postfix(ColonyBeds __instance, ref bool __result, Vector3Int position, ref Vector3Int bedPosition, ref BedTracker.Bed bed, int boxradius)
		{
			if (!__result)
			{
				// Log.WriteWarning("   ... But game did not find a bed either");
			}
		}
	}

	[HarmonyPatch(typeof(NPCBase))]
	[HarmonyPatch("CalculateGoalLocation")]
	public class NPCBaseHookNPCGoal
	{
		static bool Prefix(NPCBase __instance, ref Vector3Int __result, NPCBase.NPCGoal goal)
		{
			switch (goal)
			{
				case NPCBase.NPCGoal.Bed:
				{
					if (__instance.UsedBed != null && __instance.UsedBed.IsValid)
					{
						return true;
					}

					if(__instance.Job == null)
					{
						return true;
					}

					Vector3Int jobLocation;
					BedTracker.Bed bed;
					if (__instance.Colony.BedTracker.TryGetClosestUnused(__instance.Job.GetJobLocation(), out jobLocation, out bed, 200))
					{
						Log.WriteWarning("Redirected NPC Goal");

						ClassUtility.Call(__instance, "ClearBed", new object[] { });
						ClassUtility.SetProperty(__instance, "UsedBed", bed);
						ClassUtility.SetProperty(__instance, "UsedBedLocation", jobLocation);
						bed.SetUseState(jobLocation, true);
						__result =  jobLocation;
						return false;
					}
					break;
				}
			}

			return true;
		}
	}
}
