using SawTapes.Managers;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Games.HuntingGame;

public class BillyPuppetHunting : PhysicsProp
{
    public bool onCooldown = false;
    public int currentTimeLeft;

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        if (!buttonDown || onCooldown || playerHeldBy == null) return;
        if (SawTapes.sawTape == null || SawTapes.sawTape is not HuntingTape huntingTape) return;

        StartChronoServerRpc(ConfigManager.eyeCooldownHunting.Value);
        huntingTape.ShowAura(ConfigManager.eyeAuraDurationHunting.Value);
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
        if (playerHeldBy == null || isPocketed || playerHeldBy != GameNetworkManager.Instance.localPlayerController) return;

        string toolTip = onCooldown ? $"[On Cooldown : {currentTimeLeft}]" : "";
        HUDManager.Instance.ChangeControlTipMultiple(itemProperties.toolTips.Concat([toolTip]).ToArray(), holdingItem: true, itemProperties);
    }
}