using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName = "Wave 1";
        
        [Header("Enemy Normal")]
        public GameObject enemyNormalPrefab;
        public int enemyNormalCount = 3;
        public float enemyNormalSpawnDelay = 1f;
        
        [Header("Enemy Tank")]
        public GameObject enemyTankPrefab;
        public int enemyTankCount = 1;
        public float enemyTankSpawnDelay = 2f;
        
        [Header("Enemy Voador")]
        public GameObject enemyVoadorPrefab;
        public int enemyVoadorCount = 2;
        public float enemyVoadorSpawnDelay = 1.5f;
        
        [Header("Wave Settings")]
        public float delayBeforeNextWave = 5f;
    }

    [Header("Waves Configuration")]
    public Wave[] waves;
    
    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public bool autoStart = true;
    public float delayBeforeFirstWave = 3f;

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;
    private bool waveInProgress = false;

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
    }

    private IEnumerator StartWavesWithDelay()
    {
        yield return new WaitForSeconds(delayBeforeFirstWave);
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (currentWaveIndex >= waves.Length)
        {
            Debug.Log("Todas as waves completadas!");
            return;
        }

        waveInProgress = true;
        Wave currentWave = waves[currentWaveIndex];
        
        Debug.Log($"Iniciando {currentWave.waveName}");
        
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
        
        Debug.Log("Todos os inimigos foram derrotados!");
        
        waveInProgress = false;
        currentWaveIndex++;

        if (currentWaveIndex < waves.Length)
        {
            Debug.Log($"Wave {currentWaveIndex} completa! Próxima wave ({waves[currentWaveIndex].waveName}) em {wave.delayBeforeNextWave}s");
            yield return new WaitForSeconds(wave.delayBeforeNextWave);
            StartNextWave();
        }
        else
        {
            Debug.Log("Todas as waves foram completadas! Vitória!");
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
