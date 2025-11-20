using UnityEngine;

public interface IEnemy
{
    // retorna true se o inimigo estiver morto
    bool IsDead();
    
    // aplica dano ao inimigo
    void TakeDamage(float damage);
    
    // aplica efeito de slow (velocidade reduzida temporariamente)
    void ApplySlow(float slowAmount, float duration);
}
