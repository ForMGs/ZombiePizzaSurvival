using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerAttack playerAttack;

    private void Awake()
    {
        playerAttack = GetComponentInParent<PlayerAttack>();
    }

    // Animation Event에서 호출
    public void AttackHit()
    {
        playerAttack?.OnAttackHit();
    }

    public void SwingSound()
    {
        playerAttack?.OnSwingSound();
    }
}
