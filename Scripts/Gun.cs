using System.Collections;
using Photon.Pun;
using UnityEngine;

//총 구현
public class Gun : MonoBehaviourPun, IPunObservable //(OnPhotonSerializeView 사용)
{
    //총의 상태 타입
    public enum State {
        Ready, //발사 준비됨
        Empty, //탄창이 빔
        Reloading //재장전 중
    }

    public State eState { get; private set; }//현재 총의 상태

    public Transform fireTransform;//총알이 발사될 위치
    public ParticleSystem muzzleFlashEffect;//총구 화염 효과
    public ParticleSystem shellEjectEffect;//탄피 배출 효과

    private LineRenderer bulletLineRenderer;//총알 궤적을 그리기 위한 렌더러
    private AudioSource gunAudioPlayer;//총 소리 재생기
    
    public GunData gunData;//총의 현재 데이터
    
    private float fireDistance = 50f;//사정거리

    public int iAmmoRemain = 100;//남은 전체 탄약
    public int iMagAmmo;//현재 탄창에 남아있는 탄약

    private float lastFireTime;//총을 마지막으로 발사한 시점

    //주기적으로 자동 실행 동기화 메서드
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) 
    {
        //로컬 오브젝트일 때 쓰기 부분 실행
        if (stream.IsWriting)
        {
            stream.SendNext(iAmmoRemain);//남은 탄약수를 네트워크를 통해 보내기
            stream.SendNext(iMagAmmo);//탄창의 탄약수를 네트워크를 통해 보내기
            stream.SendNext(eState);//현재 총의 상태를 네트워크를 통해 보내기
        }
        else//리모트 오브젝트일 때 읽기 부분 실행 
        {
            iAmmoRemain = (int) stream.ReceiveNext();//남은 탄약수를 네트워크를 통해 받기
            iMagAmmo = (int) stream.ReceiveNext();//탄창의 탄약수를 네트워크를 통해 받기
            eState = (State) stream.ReceiveNext();//현재 총의 상태를 네트워크를 통해 받기
        }
    }

    //남은 탄약을 추가
    [PunRPC]
    public void AddAmmo(int ammo) 
    {
        iAmmoRemain += ammo;
    }

    private void Awake() 
    {
        gunAudioPlayer = GetComponent<AudioSource>();
        bulletLineRenderer = GetComponent<LineRenderer>();

        bulletLineRenderer.positionCount = 2;//사용할 점을 두개로 변경
        bulletLineRenderer.enabled = false;//라인 렌더러를 비활성화
    }


    private void OnEnable() 
    {
        iAmmoRemain = gunData.iStartAmmoRemain;//전체 예비 탄약 양을 초기화
        iMagAmmo = gunData.iMagCapacity;//현재 탄창을 가득채우기

        eState = State.Ready;//총의 현재 상태를 총을 쏠 준비가 된 상태로 변경
        lastFireTime = 0;//마지막으로 총을 쏜 시점을 초기화
    }


    public void Fire()//총 발사 
    {
        //현재 상태가 발사 가능한 상태 및 마지막 총 발사 시점에서 timeBetFire 이상의 시간이 지남
        if (eState == State.Ready && Time.time >= lastFireTime + gunData.fTimeBetFire)
        {
            lastFireTime = Time.time;//마지막 총 발사 시점을 갱신
            Shot();//실제 발사 처리 실행
        }
    }

    private void Shot()//실제 발사처리 실행
    {
        //실제 발사 처리는 호스트에게 대리
        photonView.RPC("ShotProcessOnServer", RpcTarget.MasterClient);

        // 남은 탄환의 수를 -1
        iMagAmmo--;
        //탄창에 남은 탄약이 없을 때 총의 현재 상태를 Empty으로 갱신
        if (iMagAmmo <= 0)
        {
            eState = State.Empty;
        }
    }

    //호스트에서 실행되는, 실제 발사 처리
    [PunRPC]
    private void ShotProcessOnServer() 
    {
        //레이캐스트에 의한 충돌 정보를 저장하는 컨테이너
        RaycastHit hit;
        //총알이 맞은 곳을 저장할 Vector
        Vector3 hitPosition = Vector3.zero;

        //레이캐스트(시작지점, 방향, 충돌 정보 컨테이너, 사정거리)
        if (Physics.Raycast(fireTransform.position,fireTransform.forward, out hit, fireDistance))
        {
            //충돌한 상대방으로부터 IDamageable 오브젝트를 가져오기 
            IDamageable target = hit.collider.GetComponent<IDamageable>();

            //IDamageable 오브젝트 반환시
            if (target != null)
            {
                //대상 데미지 처리
                target.OnDamage(gunData.fDamage, hit.point, hit.normal);
            }

            //레이가 충돌한 위치 저장
            hitPosition = hit.point;
        }
        else
        {
            // 총알이 최대 사정거리까지 날아갔을때의 위치를 충돌 위치로 사용
            hitPosition = fireTransform.position + fireTransform.forward * fireDistance;
        }

        // 발사 이펙트 재생, 이펙트 재생은 모든 클라이언트들에서 실행
        photonView.RPC("ShotEffectProcessOnClients", RpcTarget.All, hitPosition);
    }

    //이펙트 재생 코루틴을 랩핑하는 메서드
    [PunRPC]
    private void ShotEffectProcessOnClients(Vector3 hitPosition) 
    {
        StartCoroutine(ShotEffect(hitPosition));
    }

    //발사 이펙트와 소리를 재생하고 총알 궤적 그리기
    private IEnumerator ShotEffect(Vector3 hitPosition) 
    {
        muzzleFlashEffect.Play();//총구 화염 효과 재생
        shellEjectEffect.Play();//탄피 배출 효과 재생

        gunAudioPlayer.PlayOneShot(gunData.shotClip);//총격 소리 재생

        bulletLineRenderer.SetPosition(0, fireTransform.position);//선의 시작점은 총구의 위치
        bulletLineRenderer.SetPosition(1, hitPosition);//선의 끝점은 입력으로 들어온 충돌 위치
        bulletLineRenderer.enabled = true;//라인 렌더러를 활성화하여 총알 궤적을 그림

        //0.03초 동안 잠시 처리를 대기
        yield return new WaitForSeconds(0.03f);

        bulletLineRenderer.enabled = false;//라인 렌더러를 비활성화하여 총알 궤적 지우기
    }

    public bool Reload()//재장전 시도
    {
        //이미 재장전 중이거나, 남은 총알이 없거나 탄창에 총알이 이미 가득한 경우 재장전불가
        if (eState == State.Reloading || iAmmoRemain <= 0 || iMagAmmo >= gunData.iMagCapacity)
        {
            return false;
        }

        //재장전 처리 실행
        StartCoroutine(ReloadRoutine());
        return true;
    }

    private IEnumerator ReloadRoutine()//실제 재장전 처리를 진행
    {
        eState = State.Reloading;//현재 상태를 재장전 중 상태로 전환
        gunAudioPlayer.PlayOneShot(gunData.reloadClip);//재장전 소리 재생

        //재장전 소요 시간 만큼 처리를 wait
        yield return new WaitForSeconds(gunData.fReloadTime);

        //탄창에 채울 탄약을 계산
        int ammoToFill = gunData.iMagCapacity - iMagAmmo;

        //탄창에 채워야할 탄약이 남은 탄약보다 많으면 채워야할 탄약 수를 남은 탄약 수에 맞춰 줄임
        if (iAmmoRemain < ammoToFill)
        {
            ammoToFill = iAmmoRemain;
        }

        iMagAmmo += ammoToFill;//탄창을 채움
        iAmmoRemain -= ammoToFill;//남은 탄약에서, 탄창에 채운만큼 탄약을 뺌
        eState = State.Ready;//총의 현재 상태를 발사 준비된 상태로 변경
    }
}