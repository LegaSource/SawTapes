using GameNetcodeStuff;
using SawTapes.Managers;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Games.SurvivalGame;

public class BillyPuppetSurvival : PhysicsProp
{
    public bool onCooldown = false;
    public int currentTimeLeft;

    public AudioSource billyLaugh;
    public EnemyAI aimedEnemy;

    public override void Start()
    {
        base.Start();

        if (billyLaugh == null) billyLaugh = GetComponent<AudioSource>();
        if (billyLaugh == null) SawTapes.mls.LogError("billyLaugh is not assigned and could not be found.");
    }

    public override void Update()
    {
        base.Update();

        if (!isHeld || isPocketed || onCooldown || playerHeldBy == null) return;
        if (playerHeldBy != GameNetworkManager.Instance.localPlayerController) return;
        if (SawTapes.sawTape == null || SawTapes.sawTape is not SurvivalTape) return;

        STUtilities.ShowAuraAimedEnemy(playerHeldBy, SawTapes.modName, ConfigManager.survivalItemDistance.Value, ref aimedEnemy);
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        if (!buttonDown || onCooldown || playerHeldBy == null) return;
        if (SawTapes.sawTape == null || SawTapes.sawTape is not SurvivalTape survivalTape || aimedEnemy == null) return;

        PlayerControllerB player = survivalTape.players?.Where(p => p != null && p != playerHeldBy)
            .OrderByDescending(p => Vector3.Distance(playerHeldBy.transform.position, p.transform.position))
            .FirstOrDefault();
        if (player == null || player == playerHeldBy) return;

        billyLaugh.Play();
        StartChronoEveryoneRpc(ConfigManager.survivalItemCooldown.Value);
        survivalTape.TeleportEnemyServerRpc(aimedEnemy.thisNetworkObject, player.transform.position);
        STUtilities.RemoveAuraFromEnemy(SawTapes.modName, ref aimedEnemy);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void StartChronoEveryoneRpc(int cooldown)
    {
        onCooldown = true;
        currentTimeLeft = cooldown;
        _ = StartCoroutine(StartChronoCoroutine());
    }

    public IEnumerator StartChronoCoroutine()
    {
        while (currentTimeLeft > 0)
        {
            yield return new WaitForSecondsRealtime(1f);

            currentTimeLeft--;
            SetControlTipsForItem();
        }

        onCooldown = false;
        SetControlTipsForItem();
    }

    public override void SetControlTipsForItem()
    {
        if (playerHeldBy == null || playerHeldBy != GameNetworkManager.Instance.localPlayerController) return;

        string toolTip = onCooldown ? $"[On Cooldown : {currentTimeLeft}]" : "";
        HUDManager.Instance.ChangeControlTipMultiple(itemProperties.toolTips.Concat([toolTip]).ToArray(), holdingItem: true, itemProperties);
    }

    public override void PocketItem()
    {
        base.PocketItem();
        STUtilities.RemoveAuraFromEnemy(SawTapes.modName, ref aimedEnemy);
    }

    public override void DiscardItem()
    {
        base.DiscardItem();
        STUtilities.RemoveAuraFromEnemy(SawTapes.modName, ref aimedEnemy);
    }
}
