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
    public void AttackEnd()
    {
        zombieAI?.EndAttack();
    }

    public void AttackHit()
    {
        zombieAI?.OnAttackHit();
    }
}
