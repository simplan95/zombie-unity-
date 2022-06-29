using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/GunData", fileName = "Gun Data")]
public class GunData : ScriptableObject
{
    public AudioClip shotClip; // 발사 소리
    public AudioClip reloadClip; // 재장전 소리

    public float fDamage = 25; // 공격력

    public int iStartAmmoRemain = 100; // 처음에 주어질 전체 탄약
    public int iMagCapacity = 25; // 탄창 용량

    public float fTimeBetFire = 0.12f; // 총알 발사 간격
    public float fReloadTime = 1.8f; // 재장전 소요 시간
}