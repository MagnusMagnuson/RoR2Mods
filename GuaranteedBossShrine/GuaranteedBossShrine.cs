using BepInEx;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GuaranteedBossShrine
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.MagnusMagnuson.GuaranteedBossShrine", "GuaranteedBossShrine", "0.1.0")]
    public class GuaranteedBossShrine : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.SceneDirector.PopulateScene += (orig, self) =>
            {
                orig(self);
                if (SceneInfo.instance.countsAsStage)
                {
                    Type[] arr = ((IEnumerable<System.Type>)typeof(ChestRevealer).Assembly.GetTypes()).Where<System.Type>((Func<System.Type, bool>)(t => typeof(IInteractable).IsAssignableFrom(t))).ToArray<System.Type>();


                    if (!BossShrineExists(arr))
                    {
                        Xoroshiro128Plus xoroshiro128Plus = new Xoroshiro128Plus(self.GetFieldValue<Xoroshiro128Plus>("rng").nextUlong);
                        SpawnCard card = Resources.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscShrineBoss");
                        GameObject gameObject3 = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(card, new DirectorPlacementRule
                        {
                            placementMode = DirectorPlacementRule.PlacementMode.Random
                        }, xoroshiro128Plus));
                    }
                }
            };
        }

        private bool BossShrineExists(Type[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                foreach (UnityEngine.MonoBehaviour instances in InstanceTracker.FindInstancesEnumerable(arr[i]))
                {
                    if (((IInteractable)instances).ShouldShowOnScanner())
                    {
                        string item = ((IInteractable)instances).ToString().ToLower();
                        if (item.Contains("shrineboss"))
                        {
                            return true;
                        }

                    }
                }
            }

            return false;
        }
    }
}
