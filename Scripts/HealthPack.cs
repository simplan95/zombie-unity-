using Photon.Pun;
using UnityEngine;

//체력을 회복하는 아이템
public class HealthPack : MonoBehaviourPun, IItem 
{
    public float health = 50;//체력을 회복할 수치

    public void Use(GameObject target) 
    {
        //전달받은 게임 오브젝트로부터 LivingEntity 컴포넌트 가져오기 
        LivingEntity life = target.GetComponent<LivingEntity>();

        //LivingEntity컴포넌트 반환시
        if (life != null)
        {
            life.RestoreHealth(health);//체력 회복 실행
        }

        //모든 클라이언트에서의 자신 파괴
        PhotonNetwork.Destroy(gameObject);
    }
}