using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;//내비메시

// 주기적으로 아이템을 플레이어 근처에 생성하는 스크립트
public class ItemSpawner : MonoBehaviourPun 
{
    public GameObject[] items;//생성할 아이템들

    public float fMaxDistance = 5f;//플레이어 위치로부터 아이템이 배치될 최대 반경
    public float fTimeBetSpawnMax = 7f;//최대 시간 간격
    public float fTimeBetSpawnMin = 2f;//최소 시간 간격

    private float fTimeBetSpawn;//생성 간격
    private float fTastSpawnTime;//마지막 생성 시점

    private void Start() 
    {
        // 생성 간격과 마지막 생성 시점 초기화
        fTimeBetSpawn = Random.Range(fTimeBetSpawnMin, fTimeBetSpawnMax);
        fTastSpawnTime = 0;
    }

    // 주기적으로 아이템 생성 처리 실행
    private void Update() 
    {
        //호스트에서만 아이템 직접 생성 가능
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        //스폰주기 갱신시
        if (Time.time >= fTastSpawnTime + fTimeBetSpawn)
        {
            fTastSpawnTime = Time.time;//마지막 생성 시간 갱신
            fTimeBetSpawn = Random.Range(fTimeBetSpawnMin, fTimeBetSpawnMax);//생성 주기를 랜덤으로 변경
            Spawn();//실제 아이템 생성
        }
    }

    private void Spawn()//실제 아이템 생성 처리
    {
        //(0,0,0)을 기준으로 maxDistance 안에서 내비메시위의 랜덤 위치 지정, 바닥에서 0.5만큼 위로 올리기
        Vector3 spawnPosition = GetRandomPointOnNavMesh(Vector3.zero, fMaxDistance);
        spawnPosition += Vector3.up * 0.5f;

        //생성할 아이템을 무작위로 하나 선택
        GameObject itemToCreate = items[Random.Range(0, items.Length)];

        //네트워크의 모든 클라이언트에서 해당 아이템 생성
        //PhotonNetwork.Instantiate 메서드는 프리펩을 직접 못받으므로 프리펩의 이름을 받도록함
        GameObject item = PhotonNetwork.Instantiate(itemToCreate.name, spawnPosition,Quaternion.identity);

        //생성한 아이템을 5초 뒤에 파괴
        //PhotonNetwork.Destroy 메서드는 지연기능이 없으므로 코루틴으로 실행
        StartCoroutine(DestroyAfter(item, 5f));
    }

    // 포톤의 PhotonNetwork.Destroy()를 지연 실행하는 코루틴 
    IEnumerator DestroyAfter(GameObject target, float delay) 
    {
        //delay 만큼 대기
        yield return new WaitForSeconds(delay);

        //target이 파괴되지 않았으면 파괴 실행
        if (target != null)
        {
            PhotonNetwork.Destroy(target);
        }
    }

    //내비 메시 위의 랜덤한 위치를 반환
    private Vector3 GetRandomPointOnNavMesh(Vector3 center, float distance) 
    {
        //center를 중심으로 반지름이 maxDinstance인 구 안에서의 랜덤한 위치 하나를 저장
        //Random.insideUnitSphere는 반지름이 1인 구 안에서의 랜덤한 한 점을 반환하는 프로퍼티
        Vector3 randomPos = Random.insideUnitSphere * distance + center;

        //내비 메시 샘플링의 결과 정보를 저장하는 변수
        NavMeshHit hit;

        //randomPos를 기준으로 maxDistance 반경 안에서, randomPos에 가장 가까운 내비 메시 위의 한 점을 찾음
        NavMesh.SamplePosition(randomPos, out hit, distance, NavMesh.AllAreas);

        //찾은 점 반환
        return hit.position;
    }
}