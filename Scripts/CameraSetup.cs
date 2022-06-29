using Cinemachine; // 시네머신 
using Photon.Pun; // PUN 관련 
using UnityEngine;

//시네머신 카메라가 로컬 플레이어를 추적
public class CameraSetup : MonoBehaviourPun 
{
    void Start() 
    {
        //자신이 로컬 플레이어일 때
        if (photonView.IsMine)
        {
            Camera camera = FindObjectOfType<Camera>();

            // 씬에 있는 시네머신 가상 카메라 찾기
            CinemachineVirtualCamera followCam =
                FindObjectOfType<CinemachineVirtualCamera>();

            // 가상 카메라의 추적 대상을 자신의 트랜스폼으로 설정
            followCam.transform.position = new Vector3(transform.position.x, 16.0f, transform.position.z);
            followCam.Follow = transform;
            followCam.LookAt = null;


        }
    }
}