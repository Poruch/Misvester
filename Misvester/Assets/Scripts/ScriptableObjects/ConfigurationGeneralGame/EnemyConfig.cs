
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Scriptable Objects/EnemyConfig")]
public class EnemyConfig : ScriptableObject
{
    [SerializeField]
    string enemyName;
    [SerializeField]
    int enemyLevel;
    [SerializeField]
    GameObject enemyObject;
    [SerializeField]
    GameObject deathEffect;
    [SerializeField]
    float angleSprite = 90;

    [SerializeField] float hp;
    [SerializeField] float shield;
    [SerializeField] float speed;
    [SerializeField] float AtkSpeed;
    public float Hp { get => hp; set => hp = value; }
    public float Speed { get => speed; set => speed = value; }
    public string Name { get => enemyName; set => enemyName = value; }
    public float AtkSpeed1 { get => AtkSpeed; set => AtkSpeed = value; }
    public int EnemyLevel { get => enemyLevel; set => enemyLevel = value; }
    public float AngleSprite { get => angleSprite; set => angleSprite = value; }
    public GameObject DeathEffect { get => deathEffect; set => deathEffect = value; }
    public float Shield { get => shield; set => shield = value; }
    public GameObject EnemyObject { get => enemyObject; set => enemyObject = value; }
}

