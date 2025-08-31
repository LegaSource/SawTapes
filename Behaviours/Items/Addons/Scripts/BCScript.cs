using LegaFusionCore.Registries;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Items.Addons.Scripts;

public class BCScript : NetworkBehaviour
{
    public EnemyAI enemy;
    public int playerWhoHit;

    public Transform portal1;
    public Transform portal2;
    public Transform portal3;

    public Transform portalAttach1;
    public Transform portalAttach2;
    public Transform portalAttach3;

    public Transform enemyAttach1;
    public Transform enemyAttach2;
    public Transform enemyAttach3;

    private void Start()
    {
        Vector3 position = enemy.GetComponentInChildren<BoxCollider>()?.bounds.center ?? enemy.transform.position;
        enemyAttach1.position = position;
        enemyAttach2.position = position;
        enemyAttach3.position = position;

        enemyAttach1.SetParent(enemy.transform);
        enemyAttach2.SetParent(enemy.transform);
        enemyAttach3.SetParent(enemy.transform);

        portalAttach1.SetParent(portal1);
        portalAttach2.SetParent(portal2);
        portalAttach3.SetParent(portal3);

        _ = StartCoroutine(AttachToEnemyCoroutine());
    }

    public IEnumerator AttachToEnemyCoroutine()
    {
        GameObject audioObject = Instantiate(SawTapes.bleedingChainsAudio, transform.position, Quaternion.identity);
        audioObject.GetComponent<AudioSource>()?.Play();

        yield return new WaitForSecondsRealtime(1f);

        float timer = 0f;
        while (timer < 0.75f)
        {
            portal1.position += portal1.forward * 4f * Time.deltaTime;
            portal2.position += portal2.forward * 4f * Time.deltaTime;
            portal3.position += portal3.forward * 4f * Time.deltaTime;

            timer += Time.deltaTime;
            yield return null;
        }

        LFCStatusEffectRegistry.ApplyStatus(enemy.gameObject, LFCStatusEffectRegistry.StatusEffectType.BLEEDING, playerWhoHit, 10, 100);
        SpawnParticle();
        Destroy(gameObject);
    }

    public void SpawnParticle()
    {
        GameObject particleObject = Instantiate(SawTapes.redExplosionParticle, transform.position, Quaternion.identity);
        ParticleSystem explosionParticle = particleObject.GetComponent<ParticleSystem>();
        Destroy(particleObject, explosionParticle.main.duration + explosionParticle.main.startLifetime.constantMax);
    }
}
