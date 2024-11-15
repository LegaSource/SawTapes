using SawTapes.Behaviours;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine;
using System.Linq;
using UnityEngine.ProBuilder;
using System.Collections.Generic;

namespace SawTapes.Managers
{
    public class CustomPassManager : MonoBehaviour
    {
        private static WallhackCustomPass wallhackPass;
        private static CustomPassVolume customPassVolume;

        public static CustomPassVolume CustomPassVolume
        {
            get
            {
                if (customPassVolume == null)
                {
                    customPassVolume = GameNetworkManager.Instance.localPlayerController.gameplayCamera.gameObject.AddComponent<CustomPassVolume>();
                    if (customPassVolume != null)
                    {
                        customPassVolume.targetCamera = GameNetworkManager.Instance.localPlayerController.gameplayCamera;
                        customPassVolume.injectionPoint = (CustomPassInjectionPoint)1;
                        customPassVolume.isGlobal = true;

                        wallhackPass = new WallhackCustomPass();
                        customPassVolume.customPasses.Add(wallhackPass);
                    }
                }
                return customPassVolume;
            }
        }

        public static void SetupCustomPassForEnemy(EnemyAI enemy)
        {
            LayerMask wallhackLayer = 524288;
            List<Renderer> enemyRenderers = enemy.GetComponentsInChildren<Renderer>().Where(r => (wallhackLayer & (1 << r.gameObject.layer)) != 0).ToList();
            if (enemyRenderers.Any(r => r.name.Contains("LOD"))) enemyRenderers.RemoveAll(r => !r.name.Contains("LOD"));

            if (enemyRenderers == null || enemyRenderers.Count == 0)
            {
                SawTapes.mls.LogError($"No renderer could be found on {enemy.enemyType.enemyName}.");
                return;
            }

            if (CustomPassVolume == null)
            {
                SawTapes.mls.LogError("CustomPassVolume is not assigned.");
                return;
            }

            wallhackPass = CustomPassVolume.customPasses.Find(pass => pass is WallhackCustomPass) as WallhackCustomPass;
            if (wallhackPass == null)
            {
                SawTapes.mls.LogError("WallhackCustomPass could not be found in CustomPassVolume.");
                return;
            }

            wallhackPass.SetTargetRenderers(enemyRenderers.ToArray(), SawTapes.wallhackShader);
        }

        public static void RemoveAura()
        {
            wallhackPass?.ClearTargetRenderers();
        }
    }
}
