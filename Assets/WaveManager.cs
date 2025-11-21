using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName = "Wave";
        
        [Header("Enemy Normal")]
        public GameObject enemyNormalPrefab;
        public int enemyNormalCount;
        public float enemyNormalSpawnDelay;
        
        [Header("Enemy Tank")]
        public GameObject enemyTankPrefab;
        public int enemyTankCount;
        public float enemyTankSpawnDelay;
        
        [Header("Enemy Voador")]
        public GameObject enemyVoadorPrefab;
        public int enemyVoadorCount;
        public float enemyVoadorSpawnDelay;
        
        [Header("Wave Settings")]
        public float delayBeforeNextWave;
    }

    [Header("Waves Configuration")]
    public Wave[] waves;
    
    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public bool autoStart = true;
    public float delayBeforeFirstWave = 20f;

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;
    private bool waveInProgress = false;
    
    // Singleton
    public static WaveManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("✅ WaveManager Instance criado!");
        }
        else
        {
            Debug.LogWarning("⚠️ WaveManager duplicado! Destruindo...");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (spawnPoint == null)
        {
            Debug.LogError("Spawn Point não configurado no WaveManager!");
            enabled = false;
            return;
        }

        if (waves == null || waves.Length == 0)
        {
            Debug.LogError("Nenhuma wave configurada no WaveManager!");
            enabled = false;
            return;
        }

        if (autoStart)
        {
            StartCoroutine(StartWavesWithDelay());
        }
        else
        {
            // Se não auto-start, inicia o timer
            if (WaveTimerUI.Instance != null)
            {
                WaveTimerUI.Instance.StartTimer();
            }
        }
    }

    private IEnumerator StartWavesWithDelay()
    {
        // Aguarda 1 segundo para garantir que tudo foi inicializado
        yield return new WaitForSeconds(1f);
        
        // Mostra popup de preparação
        if (WavePopup.Instance != null)
        {
            WavePopup.Instance.ShowPreparation();
            Debug.Log("📢 Popup de preparação exibido!");
        }
        else
        {
            Debug.LogError("❌ WavePopup.Instance é NULL!");
        }
        
        // Aguarda mais 2 segundos para o popup ser visível
        yield return new WaitForSeconds(2f);
        
        // Mostra o timer até a primeira wave
        if (WaveTimerUI.Instance != null)
        {
            WaveTimerUI.Instance.SetCustomTimer(delayBeforeFirstWave - 3f); // -3s pelos delays anteriores
            WaveTimerUI.Instance.StartTimer();
        }
        
        yield return new WaitForSeconds(delayBeforeFirstWave - 3f);
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (currentWaveIndex >= waves.Length)
        {
            Debug.Log("🎉 Todas as waves completadas! VITÓRIA!");
            // Para o timer
            if (WaveTimerUI.Instance != null)
            {
                WaveTimerUI.Instance.StopTimer();
            }
            return;
        }

        waveInProgress = true;
        Wave currentWave = waves[currentWaveIndex];
        
        // 🌊 Notifica ResourceManager para dar recompensa e incrementar wave
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.StartNextWave();
        }
        
        // 📢 Mostra popup de wave iniciando
        if (WavePopup.Instance != null)
        {
            WavePopup.Instance.ShowWaveStartNow(currentWaveIndex + 1);
        }
        
        // Para o timer durante a wave
        if (WaveTimerUI.Instance != null)
        {
            WaveTimerUI.Instance.StopTimer();
        }
        
        Debug.Log($"🌊 Iniciando {currentWave.waveName} (Wave {currentWaveIndex + 1})");
        
        StartCoroutine(SpawnWave(currentWave));
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        // Spawn Enemy Normal
        for (int i = 0; i < wave.enemyNormalCount; i++)
        {
            if (wave.enemyNormalPrefab != null)
            {
                SpawnEnemy(wave.enemyNormalPrefab);
                yield return new WaitForSeconds(wave.enemyNormalSpawnDelay);
            }
        }

        // Spawn Enemy Tank
        for (int i = 0; i < wave.enemyTankCount; i++)
        {
            if (wave.enemyTankPrefab != null)
            {
                SpawnEnemy(wave.enemyTankPrefab);
                yield return new WaitForSeconds(wave.enemyTankSpawnDelay);
            }
        }

        // Spawn Enemy Voador
        for (int i = 0; i < wave.enemyVoadorCount; i++)
        {
            if (wave.enemyVoadorPrefab != null)
            {
                SpawnEnemy(wave.enemyVoadorPrefab);
                yield return new WaitForSeconds(wave.enemyVoadorSpawnDelay);
            }
        }

        // Aguarda todos os inimigos serem derrotados
        Debug.Log($"Aguardando todos os inimigos morrerem. Atual: {enemiesAlive}");
        yield return new WaitUntil(() => enemiesAlive <= 0);
        
        Debug.Log("✅ Todos os inimigos foram derrotados!");
        
        // 💰 Dá recompensa de moedas pela wave completada
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.CompleteWave();
        }
        
        // 📢 Mostra popup de wave concluída
        if (WavePopup.Instance != null)
        {
            WavePopup.Instance.ShowWaveComplete(currentWaveIndex + 1);
        }
        
        waveInProgress = false;
        currentWaveIndex++;

        if (currentWaveIndex < waves.Length)
        {
            Debug.Log($"✅ Wave {currentWaveIndex} completa! Timer iniciado para próxima wave.");
            
            // 🔔 Inicia o timer para a próxima wave
            if (WaveTimerUI.Instance != null)
            {
                WaveTimerUI.Instance.OnWaveComplete();
            }
            else
            {
                // Fallback: se não houver timer, usa delay direto
                Debug.LogWarning("WaveTimerUI não encontrado! Usando delay padrão.");
                yield return new WaitForSeconds(wave.delayBeforeNextWave);
                StartNextWave();
            }
        }
        else
        {
            Debug.Log("🏆 Todas as waves foram completadas! VITÓRIA!");
            
            // 📢 Mostra popup de vitória
            if (WavePopup.Instance != null)
            {
                WavePopup.Instance.ShowVictory();
            }
            
            // Para o timer
            if (WaveTimerUI.Instance != null)
            {
                WaveTimerUI.Instance.StopTimer();
            }
        }
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        enemiesAlive++;
        
        Debug.Log($"Enemy spawned! Total vivos: {enemiesAlive}");

        // Tenta pegar o componente de health para registrar a morte
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.OnDeath += OnEnemyDied;
        }
        else
        {
            Debug.LogWarning($"Enemy {enemy.name} não tem componente EnemyHealth! Registrando OnDestroy.");
        }
        
        // Fallback: registra quando o GameObject é destruído (chegou no fim ou morreu)
        StartCoroutine(WaitForEnemyDestruction(enemy));
    }
    
    private IEnumerator WaitForEnemyDestruction(GameObject enemy)
    {
        yield return new WaitUntil(() => enemy == null);
        OnEnemyDied();
    }

    private void OnEnemyDied()
    {
        enemiesAlive--;
        if (enemiesAlive < 0) enemiesAlive = 0;
        
        Debug.Log($"Inimigo morreu. Restam: {enemiesAlive}");
    }

    void OnDestroy()
    {
        // Cleanup: desregistra eventos
        StopAllCoroutines();
    }
    
    #region Public Getters
    
    /// <summary>
    /// Retorna se há uma wave em progresso
    /// </summary>
    public bool IsWaveInProgress() => waveInProgress;
    
    /// <summary>
    /// Retorna o índice da wave atual (0-based)
    /// </summary>
    public int GetCurrentWaveIndex() => currentWaveIndex;
    
    /// <summary>
    /// Retorna o número total de waves
    /// </summary>
    public int GetTotalWaves() => waves != null ? waves.Length : 0;
    
    /// <summary>
    /// Retorna quantos inimigos ainda estão vivos
    /// </summary>
    public int GetEnemiesAlive() => enemiesAlive;
    
    #endregion

#if UNITY_EDITOR
    void OnValidate()
    {
        // Validação no editor
        if (waves != null)
        {
            foreach (var wave in waves)
            {
                if (wave != null)
                {
                    wave.enemyNormalCount = Mathf.Max(0, wave.enemyNormalCount);
                    wave.enemyTankCount = Mathf.Max(0, wave.enemyTankCount);
                    wave.enemyVoadorCount = Mathf.Max(0, wave.enemyVoadorCount);
                    wave.enemyNormalSpawnDelay = Mathf.Max(0.1f, wave.enemyNormalSpawnDelay);
                    wave.enemyTankSpawnDelay = Mathf.Max(0.1f, wave.enemyTankSpawnDelay);
                    wave.enemyVoadorSpawnDelay = Mathf.Max(0.1f, wave.enemyVoadorSpawnDelay);
                    wave.delayBeforeNextWave = Mathf.Max(0f, wave.delayBeforeNextWave);
                }
            }
        }
    }
#endif
}
