using GameNetcodeStuff;
using SawTapes.Behaviours;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace SawTapes.Managers;

public class CustomPassManager : MonoBehaviour
{
    public static AuraCustomPass auraPass;
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

                    auraPass = new AuraCustomPass();
                    customPassVolume.customPasses.Add(auraPass);
                }
            }
            return customPassVolume;
        }
    }

    public static void SetupAuraForObjects(GameObject[] objects, Material material)
    {
        Renderer[] renderers = GetFilteredRenderersFromObjects(objects);
        if (renderers.Length > 0) SetupCustomPass(renderers, material);
    }

    public static void SetupCustomPass(Renderer[] renderers, Material material)
    {
        if (CustomPassVolume == null)
        {
            SawTapes.mls.LogError("CustomPassVolume is not assigned.");
            return;
        }

        auraPass = CustomPassVolume.customPasses.Find(pass => pass is AuraCustomPass) as AuraCustomPass;
        if (auraPass == null)
        {
            SawTapes.mls.LogError("AuraCustomPass could not be found in CustomPassVolume.");
            return;
        }

        auraPass.AddTargetRenderers(renderers, material);
    }

    public static void RemoveAuraFromObjects(GameObject[] objects)
    {
        Renderer[] renderers = GetFilteredRenderersFromObjects(objects);
        if (renderers.Length > 0) RemoveAura(renderers);
    }

    private static Renderer[] GetFilteredRenderersFromObjects(GameObject[] objects)
    {
        LayerMask wallhackLayer = 524288;
        List<Renderer> collectedRenderers = [];

        foreach (GameObject obj in objects)
        {
            List<Renderer> renderers = obj.GetComponentsInChildren<Renderer>().ToList();

            if (obj.TryGetComponent<EnemyAI>(out _))
            {
                renderers = renderers.Where(r => (wallhackLayer & (1 << r.gameObject.layer)) != 0).ToList();
                if (renderers.Any(r => r.name.Contains("LOD")))
                    _ = renderers.RemoveAll(r => !r.name.Contains("LOD"));
            }
            else if (obj.TryGetComponent<PlayerControllerB>(out _))
            {
                renderers = renderers.Where(r => r is SkinnedMeshRenderer).ToList();
            }

            if (renderers.Count == 0)
            {
                SawTapes.mls.LogError($"No renderer could be found on {obj.name}.");
                continue;
            }

            collectedRenderers.AddRange(renderers);
        }

        return collectedRenderers.ToArray();
    }

    public static void RemoveAura(Renderer[] renderers)
        => auraPass?.RemoveTargetRenderers(renderers);

    public static void ClearAura()
        => auraPass?.ClearTargetRenderers();
}
