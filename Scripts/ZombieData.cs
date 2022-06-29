using UnityEngine;

// 좀비 생성시 사용할 셋업 데이터
[CreateAssetMenu(menuName = "Scriptable/ZombieData", fileName = "Zombie Data")]
public class ZombieData : ScriptableObject {
    public float fHealth = 100f; // 체력
    public float fDamage = 20f; // 공격력
    public float fSpeed = 2f; // 이동 속도
    public Color skinColor = Color.white; // 피부색
}
