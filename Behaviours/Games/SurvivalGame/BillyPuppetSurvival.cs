using GameNetcodeStuff;
using LegaFusionCore.Behaviours.Shaders;
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

        ShowAuraTargetedEnemy();
    }

    public void ShowAuraTargetedEnemy()
    {
        if (Physics.Raycast(new Ray(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward), out RaycastHit hit, ConfigManager.survivalItemDistance.Value, 524288, QueryTriggerInteraction.Collide))
        {
            EnemyAICollisionDetect enemyCollision = hit.collider.GetComponent<EnemyAICollisionDetect>();
            if (enemyCollision == null || enemyCollision.mainScript == null) return;

            if (aimedEnemy != null && aimedEnemy != enemyCollision.mainScript) RemoveAuraFromEnemy();
            aimedEnemy = enemyCollision.mainScript;
            CustomPassManager.SetupAuraForObjects([enemyCollision.mainScript.gameObject], LegaFusionCore.LegaFusionCore.transparentShader, SawTapes.modName, Color.red);
            return;
        }
        RemoveAuraFromEnemy();
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        if (!buttonDown || onCooldown || playerHeldBy == null) return;
        if (SawTapes.sawTape == null || SawTapes.sawTape is not SurvivalTape survivalTape || aimedEnemy == null) return;

        PlayerControllerB player = survivalTape.players.Where(p => p != playerHeldBy && !p.isPlayerDead)
            .OrderByDescending(p => Vector3.Distance(playerHeldBy.transform.position, p.transform.position))
            .FirstOrDefault();
        if (player == null || player == playerHeldBy) return;

        billyLaugh.Play();
        StartChronoServerRpc(ConfigManager.survivalItemCooldown.Value);
        survivalTape.TeleportEnemyServerRpc(aimedEnemy.thisNetworkObject, player.transform.position);
        RemoveAuraFromEnemy();
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartChronoServerRpc(int cooldown)
        => StartChronoClientRpc(cooldown);

    [ClientRpc]
    public void StartChronoClientRpc(int cooldown)
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
        RemoveAuraFromEnemy();
    }

    public override void DiscardItem()
    {
        base.DiscardItem();
        RemoveAuraFromEnemy();
    }

    public void RemoveAuraFromEnemy()
    {
        if (aimedEnemy == null) return;

        CustomPassManager.RemoveAuraFromObjects([aimedEnemy.gameObject], SawTapes.modName);
        aimedEnemy = null;
    }
}
