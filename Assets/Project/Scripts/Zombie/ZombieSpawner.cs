using System.Collections;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private ZombieHealth zombiePrefab;
    [SerializeField] private float respawnDelay = 10f;
    [SerializeField] private Transform target;
    private ZombieHealth currentZombie;

    private void Start()
    {
        SpawnZombie();
    }

    private void SpawnZombie()
    {
        currentZombie = Instantiate(
            zombiePrefab,
            transform.position,
            transform.rotation
        );

        currentZombie.SetSpawner(this);

        ZombieAI zombieAI = currentZombie.GetComponent<ZombieAI>();

        if (zombieAI != null)
        {
            zombieAI.SetTarget(target);
        }
    }

    public void RequestRespawn()
    {
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnZombie();
    }

}