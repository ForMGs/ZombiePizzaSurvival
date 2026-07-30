using System.Collections;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private ZombieHealth zombiePrefab;
    [Tooltip("활성화하면 씬 시작 시 자동으로 좀비를 생성합니다.")]
    [SerializeField] private bool spawnOnStart = true;
    [Tooltip("활성화하면 생성한 좀비가 죽은 뒤 Respawn Delay만큼 기다렸다가 다시 생성합니다.")]
    [SerializeField] private bool respawnAfterDeath = true;
    [SerializeField] private float respawnDelay = 10f;
    [SerializeField] private Transform target;
    private ZombieHealth currentZombie;

    private void Start()
    {
        if (spawnOnStart)
            SpawnZombie();
    }

    // 이미 이 스포너가 관리하는 좀비가 살아 있다면 중복 생성하지 않습니다.
    public bool SpawnZombie()
    {
        if (currentZombie != null)
            return false;
        if (zombiePrefab == null)
        {
            Debug.LogWarning("ZombieSpawner에 Zombie Prefab이 연결되지 않았습니다.", this);
            return false;
        }

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

        return true;
    }

    public void RequestRespawn()
    {
        if (respawnAfterDeath)
            StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnZombie();
    }

}
