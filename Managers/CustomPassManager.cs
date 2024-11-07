using SawTapes.Behaviours;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine;
using System.Linq;

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
            /*Renderer[] doorRenderers = FindObjectsOfType<Renderer>()
                .Where(r => Vector3.Distance(r.transform.position, enemy.transform.position) == 0f
                            && ConfigManager.rendererNames.Value.Any(n => r.name.StartsWith(n)))
                .ToArray();*/

            Renderer[] enemyRenderers = enemy.GetComponentsInChildren<Renderer>().ToArray();

            if (CustomPassVolume == null)
            {
                SawTapes.mls.LogError("CustomPassVolume is not assigned.");
                return;
            }

            wallhackPass = CustomPassVolume.customPasses.Find(pass => pass is WallhackCustomPass) as WallhackCustomPass;
            wallhackPass?.SetTargetRenderers(enemyRenderers, SawTapes.wallhackShader);
        }

        public static void RemoveAura()
        {
            wallhackPass?.ClearTargetRenderers();
        }
    }
}
