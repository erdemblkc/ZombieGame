using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Handles the physical spawning of enemies on the NavMesh and applies
/// wave difficulty multipliers (speed, damage, HP) to each spawned enemy.
/// Referenced by WaveManager, which drives the wave sequence.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private Transform _player;

    [Header("Spawn Area")]
    [Tooltip("Optional bounding collider. If assigned, enemies spawn inside it.")]
    [SerializeField] private Collider _spawnAreaCollider;

    [Tooltip("Radius used when no spawn area collider is assigned.")]
    [SerializeField] private float _spawnRadius = 25f;

    [Header("Spawn Rules")]
    [SerializeField] private float _minDistanceFromPlayer = 6f;
    [SerializeField] private float _navMeshSampleRange    = 8f;
    [SerializeField] private int   _triesPerEnemy         = 40;

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>
    /// Spawns all enemies defined in config, applying difficulty multipliers,
    /// and notifies waveManager for each spawned enemy.
    /// </summary>
    public IEnumerator SpawnWave(WaveConfig config, WaveManager waveManager)
    {
        if (_player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.transform;
        }

        if (config == null)
        {
            Debug.LogWarning("[EnemySpawner] SpawnWave called with null config.");
            yield break;
        }

        if (config.preparationTime > 0f)
            yield return new WaitForSeconds(config.preparationTime);

        foreach (var entry in config.enemies)
        {
            if (entry.enemyPrefab == null) continue;

            if (entry.groupDelay > 0f)
                yield return new WaitForSeconds(entry.groupDelay);

            for (int i = 0; i < entry.count; i++)
            {
                Vector3 pos    = FindSpawnPoint();
                GameObject enemy = Instantiate(entry.enemyPrefab, pos, Quaternion.identity);

                ApplyDifficultyMultipliers(enemy, config);

                // Assign player target to AI
                var ai = enemy.GetComponent<ZombieAI_Follow>();
                if (ai != null)
                {
                    ai.target     = _player;
                    ai.chaseRange = 999f;
                }

                var agent = enemy.GetComponent<NavMeshAgent>();
                if (agent != null) agent.Warp(pos);

                waveManager.RegisterEnemy(enemy);

                if (config.timeBetweenSpawns > 0f)
                    yield return new WaitForSeconds(config.timeBetweenSpawns);
            }
        }
    }

    // ── Internal ──────────────────────────────────────────────────────────

    /// <summary>Applies speed / damage / health multipliers from WaveConfig to a spawned enemy.</summary>
    void ApplyDifficultyMultipliers(GameObject enemy, WaveConfig config)
    {
        if (config.speedMultiplier != 1f)
        {
            var agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null) agent.speed *= config.speedMultiplier;
        }

        if (config.damageMultiplier != 1f)
        {
            var atk = enemy.GetComponentInChildren<ZombieAttackDamageTimed>();
            if (atk != null) atk.damage *= config.damageMultiplier;
        }

        if (config.healthMultiplier != 1f)
        {
            var hp = enemy.GetComponent<ZombieHealth1>();
            if (hp != null)
            {
                hp.maxHealth    *= config.healthMultiplier;
                hp.currentHealth = hp.maxHealth;
            }
        }
    }

    Vector3 FindSpawnPoint()
    {
        if (_spawnAreaCollider != null)
        {
            Bounds b = _spawnAreaCollider.bounds;
            for (int t = 0; t < _triesPerEnemy; t++)
            {
                float x           = Random.Range(b.min.x, b.max.x);
                float z           = Random.Range(b.min.z, b.max.z);
                Vector3 candidate = new Vector3(x, b.max.y + 2f, z);

                if (_player != null && Vector3.Distance(candidate, _player.position) < _minDistanceFromPlayer) continue;
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, _navMeshSampleRange, NavMesh.AllAreas))
                    return hit.position;
            }
            return transform.position;
        }

        Vector3 center = transform.position;
        for (int t = 0; t < _triesPerEnemy; t++)
        {
            Vector3 random = center + new Vector3(
                Random.Range(-_spawnRadius, _spawnRadius), 0f,
                Random.Range(-_spawnRadius, _spawnRadius));

            if (_player != null && Vector3.Distance(random, _player.position) < _minDistanceFromPlayer) continue;
            if (NavMesh.SamplePosition(random, out NavMeshHit hit, _navMeshSampleRange, NavMesh.AllAreas))
                return hit.position;
        }

        Debug.LogWarning("[EnemySpawner] Could not find valid NavMesh spawn point — falling back to spawner position.");
        return transform.position;
    }
}
