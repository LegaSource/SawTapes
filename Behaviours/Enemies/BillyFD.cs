using LegaFusionCore.Managers;
using SawTapes.Managers;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Enemies;

public class BillyFD : BillyBike
{
    public bool isMoving = true;
    public EnemyAI targetedEnemy;

    public AudioSource billyAudio;
    public AudioClip tickingSound;
    public AudioClip laughSound;

    public Coroutine tickingCoroutine;

    public override void Start()
    {
        base.Start();

        if (IsServer)
        {
            int randomDuration = new System.Random().Next(10, 15);
            int startTime = (int)(tickingSound.length - randomDuration);
            StartTickingClientRpc(startTime);
        }
    }

    [ClientRpc]
    public void StartTickingClientRpc(int startTime)
    {
        if (tickingCoroutine != null) return;
        tickingCoroutine = StartCoroutine(StartTickingCoroutine(startTime));
    }

    public IEnumerator StartTickingCoroutine(int startTime)
    {
        billyAudio.clip = tickingSound;
        billyAudio.time = Mathf.Max(0f, startTime);
        billyAudio.Play();

        yield return new WaitUntil(() => billyAudio.isPlaying);
        yield return new WaitUntil(() => !billyAudio.isPlaying);

        billyAudio.Stop();

        isMoving = false;
        moveTowardsDestination = false;
        movingTowardsTargetPlayer = false;
        targetPlayer = null;
        agent.speed = 0f;
        creatureSFX.Stop();
        creatureAnimator?.SetBool("isMoving", false);

        billyAudio.clip = laughSound;
        billyAudio.time = 0f;
        billyAudio.Play();

        yield return new WaitUntil(() => billyAudio.isPlaying);
        yield return new WaitUntil(() => !billyAudio.isPlaying);

        Landmine.SpawnExplosion(transform.position + Vector3.up, spawnExplosionEffect: true, 12f, 12f);

        if (IsServer)
        {
            yield return null;

            foreach (Collider hitCollider in Physics.OverlapSphere(transform.position, 15f, 524288, QueryTriggerInteraction.Collide))
            {
                EnemyAI enemy = hitCollider.GetComponent<EnemyAICollisionDetect>()?.mainScript;
                if (enemy == null || enemy.isEnemyDead) continue;

                EnemyType enemyType = enemy.enemyType;
                if (enemyType == null || ConfigManager.finalDetonationEnemiesExclusions.Value.Contains(enemyType.enemyName)) continue;

                enemy.KillEnemyOnOwnerClient(!LFCEnemyManager.CanDie(enemy));
            }

            yield return new WaitForSeconds(0.2f);

            thisNetworkObject.Despawn();
        }
    }

    public override void Update()
    {
        base.Update();
        if (isMoving && targetedEnemy != null) MoveTowardsEnemy();
    }

    public void MoveTowardsEnemy()
    {
        PlayMovementSound();
        creatureAnimator?.SetBool("isMoving", true);
        _ = SetDestinationToPosition(targetedEnemy.transform.position);
    }
}