using SawTapes.Behaviours;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace SawTapes.Managers;

public class CustomPassManager : MonoBehaviour
{
    public static WallhackCustomPass wallhackPass;
    public static CustomPassVolume customPassVolume;

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

    public static void SetupCustomPassForObjects(GameObject[] objects)
    {
        LayerMask wallhackLayer = 524288;
        List<Renderer> allRenderers = [];

        foreach (GameObject obj in objects)
        {
            List<Renderer> renderers = obj.GetComponentsInChildren<Renderer>().ToList();

            // Filtrer les renderers avec le LayerMask (uniquement pour les ennemis)
            if (obj.TryGetComponent<EnemyAI>(out _))
            {
                renderers = renderers.Where(r => (wallhackLayer & (1 << r.gameObject.layer)) != 0).ToList();
                if (renderers.Any(r => r.name.Contains("LOD"))) _ = renderers.RemoveAll(r => !r.name.Contains("LOD"));
            }

            if (renderers.Count == 0)
            {
                SawTapes.mls.LogError($"No renderer could be found on {obj.name}.");
                continue;
            }

            allRenderers.AddRange(renderers);
        }

        if (allRenderers.Count > 0) SetupCustomPass(allRenderers.ToArray());
    }

    public static void SetupCustomPass(Renderer[] renderers)
    {
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

        wallhackPass.SetTargetRenderers(renderers, SawTapes.wallhackShader);
    }

    public static void RemoveAura()
        => wallhackPass?.ClearTargetRenderers();
}
