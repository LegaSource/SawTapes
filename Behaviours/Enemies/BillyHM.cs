using LegaFusionCore.Behaviours.Shaders;
using LegaFusionCore.Managers;
using LegaFusionCore.Registries;
using SawTapes.Managers;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace SawTapes.Behaviours.Enemies;

public class BillyHM : BillyBike
{
    public bool isMoving = false;
    public float totalDuration = 30f;

    public AudioSource laughAudio;

    public enum PhaseType { Move, MoveWithAura, Laugh }

    public override void Start()
    {
        base.Start();

        StartSearch(transform.position);
        _ = StartCoroutine(HuntingCoroutine());
    }

    public IEnumerator HuntingCoroutine()
    {
        float laughDuration = 3f;
        float auraDuration = 5f;
        float waitDuration = 1f;
        float elapsed = 0f;

        // Premier déplacement sans aura
        yield return PhaseCoroutine(laughDuration, PhaseType.Laugh);
        elapsed += laughDuration;

        while (elapsed <= totalDuration)
        {
            yield return PhaseCoroutine(auraDuration, PhaseType.MoveWithAura);
            elapsed += auraDuration;

            yield return PhaseCoroutine(waitDuration, PhaseType.Move);
            elapsed += waitDuration;

            yield return PhaseCoroutine(laughDuration, PhaseType.Laugh);
            elapsed += laughDuration;
        }

        CustomPassManager.RemoveAuraByTag($"{SawTapes.modName}HunterMark");
        LFCGlobalManager.PlayParticle($"{LegaFusionCore.LegaFusionCore.modName}{LegaFusionCore.LegaFusionCore.smokeParticle.name}", transform.position, Quaternion.Euler(-90, 0, 0));
        Destroy(gameObject);
    }

    public IEnumerator PhaseCoroutine(float seconds, PhaseType type)
    {
        ApplyPhase(type);
        yield return new WaitForSeconds(seconds);
    }

    public void ApplyPhase(PhaseType type)
    {
        switch (type)
        {
            case PhaseType.Move:
                SetMoving(true);
                CustomPassManager.RemoveAuraByTag($"{SawTapes.modName}HunterMark");
                break;

            case PhaseType.MoveWithAura:
                SetMoving(true);
                GameObject[] enemiesObj = LFCSpawnRegistry.GetAllAs<EnemyAI>()
                    .Where(e => e.enemyType != null && !ConfigManager.hunterMarkEnemiesExclusions.Value.Contains(e.enemyType.enemyName))
                    .Select(e => e.gameObject)
                    .ToArray();
                CustomPassManager.SetupAuraForObjects(enemiesObj, LegaFusionCore.LegaFusionCore.wallhackShader, $"{SawTapes.modName}HunterMark", Color.red);
                break;

            case PhaseType.Laugh:
                SetMoving(false);
                laughAudio.Play();
                CustomPassManager.RemoveAuraByTag($"{SawTapes.modName}HunterMark");
                break;
        }
    }

    public void SetMoving(bool move)
    {
        isMoving = move;
        moveTowardsDestination = move;
        movingTowardsTargetPlayer = move;
        creatureAnimator?.SetBool("isMoving", move);

        GameObject[] enemiesObj = LFCSpawnRegistry.GetAllAs<EnemyAI>().Select(e => e.gameObject).ToArray();
        if (move)
        {
            agent.speed = 1f;
            creatureSFX.Play();
        }
        else
        {
            agent.speed = 0f;
            creatureSFX.Stop();
            laughAudio.Play();
        }
    }

    public override void Update()
    {
        base.Update();
        if (isMoving) PlayMovementSound();
    }
}