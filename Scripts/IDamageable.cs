using UnityEngine;

// 데미지를 입을 수 있는 타입 인터페이스
public interface IDamageable 
{
    void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal);
}