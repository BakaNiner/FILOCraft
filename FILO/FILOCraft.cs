using HarmonyLib;
using BepInEx;

namespace FILOCraft 
{
    [BepInPlugin("org.bepinex.plugins.filocraft", "FILO", "1.0.0")]
    public class FILOCraft : BaseUnityPlugin 
	{
        internal void Awake() 
		{
            var harmony = new Harmony("org.bepinex.plugins.filocraft");
            Harmony.CreateAndPatchAll(typeof(FILOCraft));
        }

		private static bool tempflag;
		private static int beforeLen;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MechaForge), "AddTask")]
		private static bool BeforeAddTask(MechaForge __instance, int recipeId, int count) 
		{
			if (!__instance.gameHistory.RecipeUnlocked(recipeId))
			{
				return false;
			}
			tempflag = __instance.TryAddTask(recipeId, count);
			if (tempflag)
			{
				beforeLen = __instance.tasks.Count;
				return true;
			}
			return false;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(MechaForge), "AddTask")]
		private static void AfterAddTask(MechaForge __instance, int recipeId, int count)
		{
			if (tempflag)
			{
				if (beforeLen == 0)
					return;

				int curLen = __instance.tasks.Count;
				int gap = curLen - beforeLen;

				if (gap == 0)
					return;

				for (int i = 0; i < beforeLen; i++)
				{
					if (__instance.tasks[i].parentTaskIndex != -1)
					{
						__instance.tasks[i].parentTaskIndex += gap;
					}
				}

				for (int i = beforeLen; i < curLen; i++)
				{
					ForgeTask t = __instance.tasks[curLen - 1];
					if (t.parentTaskIndex != -1)
					{
						t.parentTaskIndex -= beforeLen;
					}
					__instance.tasks.RemoveAt(curLen - 1);
					__instance.tasks.Insert(0, t);
				}

			}
		}
	}
}
