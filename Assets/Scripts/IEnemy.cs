using UnityEngine;

public interface IEnemy
{
    // retorna true se o inimigo estiver morto
    bool IsDead();
    
    // aplica dano ao inimigo
    void TakeDamage(float damage);
}
