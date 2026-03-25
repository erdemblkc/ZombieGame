using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject describing a single wave: which enemies to spawn, how,
/// and what difficulty multipliers to apply to spawned enemies.
/// Create via Assets → Create → WaveSystem → WaveConfig.
/// </summary>
[CreateAssetMenu(fileName = "Wave_00", menuName = "WaveSystem/WaveConfig")]
public class WaveConfig : ScriptableObject
{
    [Tooltip("Display number shown in the Wave HUD (e.g. 1, 2, 3).")]
    public int waveNumber = 1;

    [Tooltip("List of enemy groups to spawn for this wave.")]
    public List<EnemySpawnEntry> enemies = new List<EnemySpawnEntry>();

    [Tooltip("Seconds to wait between each individual enemy spawn.")]
    public float timeBetweenSpawns = 0.5f;

    [Tooltip("Countdown (seconds) shown before wave enemies start spawning.")]
    public float preparationTime = 0f;

    [Header("Difficulty Multipliers (tüm spawned düşmanlara uygulanır)")]
    [Tooltip("NavMeshAgent.speed çarpanı. 1 = değişmez.")]
    public float speedMultiplier  = 1f;
    [Tooltip("ZombieAttackDamageTimed.damage çarpanı.")]
    public float damageMultiplier = 1f;
    [Tooltip("ZombieHealth1.maxHealth çarpanı.")]
    public float healthMultiplier = 1f;
}

/// <summary>Describes one group of enemies to spawn within a wave.</summary>
[System.Serializable]
public class EnemySpawnEntry
{
    [Tooltip("Enemy prefab to instantiate.")]
    public GameObject enemyPrefab;

    [Tooltip("How many of this enemy type to spawn.")]
    public int count = 5;

    [Tooltip("Extra delay (seconds) before this group begins spawning.")]
    public float groupDelay = 0f;
}
