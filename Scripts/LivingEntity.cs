using System;
using Photon.Pun;
using UnityEngine;

//생명체로서 동작할 게임 오브젝트 베이스
public class LivingEntity : MonoBehaviourPun, IDamageable {
    public float fStartingHealth = 100f;//시작 체력
    public float fHealth { get; protected set; }//현재 체력
    public bool bDead { get; protected set; }//사망 상태
    public event Action onDeath;//사망시 발동할 이벤트


    // 호스트->모든 클라이언트 방향으로 체력과 사망 상태를 동기화
    [PunRPC]
    public void ApplyUpdatedHealth(float newHealth, bool newDead) {
        fHealth = newHealth;
        bDead = newDead;
    }

    // 생명체가 활성화될때 상태를 리셋
    protected virtual void OnEnable() {
        // 사망하지 않은 상태로 시작
        bDead = false;
        // 체력을 시작 체력으로 초기화
        fHealth = fStartingHealth;
    }

    //데미지 처리
    //호스트에서 먼저 단독 실행되고, 호스트를 통해 다른 클라이언트들에서 일괄 실행
    [PunRPC]
    public virtual void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal) 
    {
        //마스터클라일 때
        if (PhotonNetwork.IsMasterClient)
        {
            //데미지만큼 체력 감소
            fHealth -= damage;

            //호스트에서 클라이언트로 동기화
            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, fHealth, bDead);

            //다른 클라이언트들도 OnDamage를 실행
            photonView.RPC("OnDamage", RpcTarget.Others, damage, hitPoint, hitNormal);
        }

        // 체력이 0 이하 && 아직 죽지 않았다면 사망 처리 실행
        if (fHealth <= 0 && !bDead)
        {
            Die();
        }
    }


    
    [PunRPC]
    public virtual void RestoreHealth(float newHealth)//체력 회복
    {
        if (bDead)//이미 사망한 경우 체력을 회복불가
        {
            return;
        }

        // 호스트만 체력을 직접 갱신 가능
        if (PhotonNetwork.IsMasterClient)
        {
            //체력 추가
            fHealth += newHealth;

            //서버에서 클라이언트로 동기화
            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, fHealth, bDead);

            //다른 클라이언트들도 RestoreHealth를 실행
            photonView.RPC("RestoreHealth", RpcTarget.Others, newHealth);
        }
    }

    public virtual void Die() 
    {
        //onDeath 이벤트에 등록된 메서드가 있다면 실행
        if (onDeath != null)
        {
            onDeath();
        }

        //사망 상태를 참으로 변경
        bDead = true;
    }
}