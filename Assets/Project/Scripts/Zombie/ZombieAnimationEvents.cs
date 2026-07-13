using UnityEngine;

public class ZombieAnimationEvents : MonoBehaviour
{
    private ZombieAI zombieAI;

    private void Awake()
    {
        zombieAI = GetComponentInParent<ZombieAI>();
    }

    public void HitEnd()
    {
        zombieAI?.EndHit();
    }
}