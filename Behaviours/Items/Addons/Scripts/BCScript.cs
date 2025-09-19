using LegaFusionCore.Registries;
using LegaFusionCore.Utilities;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace SawTapes.Behaviours.Items.Addons.Scripts;

public class BCScript : NetworkBehaviour
{
    public GameObject entity;
    public int playerWhoHit;

    public Transform portal1;
    public Transform portal2;
    public Transform portal3;

    public Transform portalAttach1;
    public Transform portalAttach2;
    public Transform portalAttach3;

    public Transform entityAttach1;
    public Transform entityAttach2;
    public Transform entityAttach3;

    private void Start()
    {
        Vector3 position = entity.GetComponentInChildren<BoxCollider>()?.bounds.center ?? entity.transform.position;

        entityAttach1.position = position;
        entityAttach2.position = position;
        entityAttach3.position = position;

        entityAttach1.SetParent(entity.transform);
        entityAttach2.SetParent(entity.transform);
        entityAttach3.SetParent(entity.transform);

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

        int totalDamage = LFCUtilities.GetSafeComponent<EnemyAI>(entity) != null ? 100 : 10;
        LFCStatusEffectRegistry.ApplyStatus(entity, LFCStatusEffectRegistry.StatusEffectType.BLEEDING, playerWhoHit, 10, totalDamage);
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
