using Photon.Pun;
using UnityEngine;

//총알을 충전하는 아이템
public class AmmoPack : MonoBehaviourPun, IItem 
{
    public int iAmmo = 30;//충전할 총알 수

    public void Use(GameObject target) 
    {
        //PlayerShooter 컴포넌트를 가져오기 
        PlayerShooter playerShooter = target.GetComponent<PlayerShooter>();

        //PlayerShooter 컴포넌트, 총 오브젝트 반환시
        if (playerShooter != null && playerShooter.gun != null)
        {
            //총의 남은 탄환 수를 ammo 만큼 더하기, 모든 클라이언트에서 실행
            playerShooter.gun.photonView.RPC("AddAmmo", RpcTarget.All, iAmmo);
        }

        //모든 클라이언트에서의 해당 아이템 파괴
        PhotonNetwork.Destroy(gameObject);
    }
}