using Photon.Pun;
using UnityEngine;

//게임 점수 증가 아이템
public class Coin : MonoBehaviourPun, IItem 
{
    public int iScore = 200;//증가 점수

    public void Use(GameObject target) 
    {
        //게임 매니저로 점수 추가
        GameManager.instance.AddScore(iScore);

        //모든 클라이언트에서 자신 파괴
        PhotonNetwork.Destroy(gameObject);
    }
}