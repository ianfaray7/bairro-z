using UnityEngine;

/// <summary>
/// Destrói o GameObject após a animação terminar
/// Útil para efeitos visuais como explosões
/// </summary>
public class DestroyAfterAnimation : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 0f; // Delay extra após animação
    
    void Start()
    {
        // Tenta pegar duração da animação
        Animator animator = GetComponent<Animator>();
        
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            // Pega a duração da primeira animação
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            
            if (clips.Length > 0)
            {
                float duration = clips[0].length;
                Destroy(gameObject, duration + destroyDelay);
                return;
            }
        }
        
        // Fallback: destrói após 1 segundo se não encontrar animação
        Destroy(gameObject, 1f + destroyDelay);
    }
}
