using BlockEntities;
using BlockEntities.Implementations;
using Harmony;
using Pipliz;
using System.Collections.Generic;

namespace grasmanek94.Statistics
{
	public class BedState
	{
		public BedTracker.Bed Bed { get; set; }
		public bool State { get; set; }

		public BedState(BedTracker.Bed bed, bool state)
		{
			Bed = bed;
			State = state;
		}
	}

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

			var beds = ColonyBedsHookTryGetClosestUnused.beds;
			lock (beds)
			{
				Dictionary<Vector3Int, BedState> bedPositions;
				if (!beds.TryGetValue(__instance.Owner, out bedPositions))
				{
					bedPositions = new Dictionary<Vector3Int, BedState>();
					beds.Add(__instance.Owner, bedPositions);
				}

				lock (bedPositions)
				{
					Chunk chunk = World.GetChunk(blockPosition.ToChunk());
					BlockEntityTracker.EntityChunk entityChunk = chunk?.GetEntities();
					IBlockEntity blockEntity;
					BedTracker.Bed bed;
					if (entityChunk != null && entityChunk.TryGet(blockPosition.ToChunkLocal(), out blockEntity) && (bed = (blockEntity as BedTracker.Bed)) != null)
					{
						bedPositions.Add(blockPosition, new BedState(bed, false));
					}
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

			var beds = ColonyBedsHookTryGetClosestUnused.beds;
			lock (beds)
			{
				Dictionary<Vector3Int, BedState> bedPositions;
				if (!beds.TryGetValue(__instance.Owner, out bedPositions))
				{
					return;
				}

				lock (bedPositions)
				{
					if (bedPositions.Count <= 1)
					{
						beds.Remove(__instance.Owner);
					}
					else
					{
						bedPositions.Remove(blockPosition);
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

			var beds = ColonyBedsHookTryGetClosestUnused.beds;
			lock (beds)
			{
				Dictionary<Vector3Int, BedState> bedPositions;
				if (!beds.TryGetValue(__instance.Owner, out bedPositions))
				{
					return;
				}

				lock (bedPositions)
				{
					BedState bedState;

					if (!bedPositions.TryGetValue(blockPosition, out bedState))
					{
						return;
					}
					bedState.State = state;
				}
			}
		}
	}

	[HarmonyPatch(typeof(ColonyBeds))]
	[HarmonyPatch("TryGetClosestUnused")]
	public class ColonyBedsHookTryGetClosestUnused
	{
		public static Dictionary<Colony, Dictionary<Vector3Int, BedState>> beds = new Dictionary<Colony, Dictionary<Vector3Int, BedState>>();

		static bool Prefix(ColonyBeds __instance, ref bool __result, Vector3Int position, out Vector3Int bedPosition, out BedTracker.Bed bed, int boxradius)
		{
			bed = null;
			bedPosition = Vector3Int.invalidPos;

			if (__instance.Owner == null)
			{
				return true;
			}

			Dictionary<Vector3Int, BedState> localBeds;

			if (!beds.TryGetValue(__instance.Owner, out localBeds))
			{
				return true;
			}

			lock (localBeds)
			{
				if (localBeds.Count == 0)
				{
					return true;
				}
			}

			Pipliz.Collections.SortedList<Vector3Int, ColonyBeds.BedChunk> bedChunks = __instance.BedChunks;

			int resultDistance = boxradius * boxradius;

			lock (beds)
			{
				foreach(var bedEntry in localBeds)
				{
					var localBed = bedEntry.Value;

					if(localBed.State || !localBed.Bed.IsValid)
					{
						continue;
					}

					var localBedPos = bedEntry.Key;

					int distance = (localBedPos - position).Magnitude;
					if (distance < resultDistance)
					{
						resultDistance = distance;
						bedPosition = localBedPos;
						bed = localBed.Bed;
					}
				}
			}

			if(bed == null)
			{
				return true;
			}

			__result = true;

			return false;
		}
	}
}
