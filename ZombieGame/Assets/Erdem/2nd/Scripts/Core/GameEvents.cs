using System;
using UnityEngine;

/// <summary>
/// Static event hub — broadcast game-wide events so loose-coupled systems
/// can react without direct references.
/// </summary>
public static class GameEvents
{
    /// <summary>Fired when any enemy dies. Parameter: the killed enemy's GameObject.</summary>
    public static event Action<GameObject> OnEnemyKilled;

    /// <summary>Invoke from ZombieHealth1.Die() to notify all listeners.</summary>
    public static void FireEnemyKilled(GameObject enemy) => OnEnemyKilled?.Invoke(enemy);
}
