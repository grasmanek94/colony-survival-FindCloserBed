using BlockEntities.Implementations;
using Harmony;
using Pipliz;

namespace grasmanek94.Statistics
{
	[HarmonyPatch(typeof(ColonyBeds))]
	[HarmonyPatch("TryGetClosestUnused")]
	public class ColonyBedsHookTryGetClosestUnused
	{
		static bool Prefix(ColonyBeds __instance, Vector3Int position, ref Vector3Int bedPosition, ref BedTracker.Bed bed, int boxradius)
		{

			return true;
		}

		static void Postfix(ColonyBeds __instance, bool __result, Vector3Int position, ref Vector3Int bedPosition, ref BedTracker.Bed bed, int boxradius)
		{

		}
	}
}
