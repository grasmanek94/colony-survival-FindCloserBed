using Harmony;
using static ModLoader;

namespace grasmanek94.FindCloserBed
{
    [ModManager]
    public static class FindCloserBed
    {
        [ModCallback(EModCallbackType.OnAssemblyLoaded, "grasmanek94.FindCloserBed.OnAssemblyLoaded")]
        static void OnAssemblyLoaded(string assemblyPath)
        {
            var harmony = HarmonyInstance.Create("grasmanek94.FindCloserBed");
            harmony.PatchAll();
        }
    }
}
