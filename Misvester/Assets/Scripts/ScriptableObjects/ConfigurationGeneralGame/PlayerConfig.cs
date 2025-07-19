using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Scriptable Objects/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    [Serializable]
    public class BulletPack
    {
        [SerializeField]
        GameObject bullet;
        [SerializeField]
        int levelReg = 0;

        public GameObject Bullet { get => bullet; set => bullet = value; }
        public int LevelReg { get => levelReg; set => levelReg = value; }
    }

    [SerializeField]
    BulletPack[] bullets;

    [SerializeField]
    Sprite sprite;

    [SerializeField]
    RuntimeAnimatorController animator;

    [SerializeField] float hp;
    [SerializeField] float shield;
    [SerializeField] float speed;
    [SerializeField] float atkSpeed;

    public BulletPack[] Bullets { get => bullets; }
    public float Hp { get => hp; set => hp = value; }
    public float Speed { get => speed; set => speed = value; }
    public float AtkSpeed { get => atkSpeed; set => atkSpeed = value; }
    public Sprite Sprite { get => sprite; set => sprite = value; }
    public RuntimeAnimatorController Animator { get => animator; set => animator = value; }
    public float Shield { get => shield; set => shield = value; }
}
