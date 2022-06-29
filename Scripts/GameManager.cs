using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;//신관련

//점수와 게임 오버 여부, 게임 UI를 관리
public class GameManager : MonoBehaviourPunCallbacks, IPunObservable //(OnPhotonSerializeView 사용)
{
    //외부에서 싱글톤 오브젝트를 가져올때 사용할 프로퍼티
    public static GameManager instance
    {
        get
        {
            //싱글톤 변수에 오브젝트 미할당 시
            if (m_instance == null)
            {
                //씬에서 GameManager 오브젝트를 찾아 할당
                m_instance = FindObjectOfType<GameManager>();
            }
            return m_instance;
        }
    }

    private static GameManager m_instance;//싱글톤이 할당될 static 변수

    public GameObject playerPrefab;//플레이어 캐릭터 프리팹

    private int iScore = 0;//현재 게임 점수
    public bool bIsGameover { get; private set; }//게임 오버 상태

    //주기적으로 자동 실행 동기화 메서드
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) 
    {
        //로컬 오브젝트일 때 쓰기 부분 실행
        if (stream.IsWriting)
        {
            //네트워크를 통해 score 값을 보내기
            stream.SendNext(iScore);
        }
        else//리모트 오브젝트일 때 읽기 부분 실행     
        {
            //네트워크를 통해 score 값 받기
            iScore = (int) stream.ReceiveNext();
            //동기화된 점수를 UI로 표시
            UIManager.instance.UpdateScoreText(iScore);
        }
    }


    private void Awake() 
    {
        //씬에 싱글톤 오브젝트가 된 다른 GameManager 오브젝트가 있다면 자신을 파괴
        if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()//게임 시작과 동시에 플레이어 오브젝트를 생성
    {
        //생성할 위치 랜덤 지정, 위치 y값은 0
        Vector3 randomSpawnPos = Random.insideUnitSphere * 5f;
        randomSpawnPos.y = 0f;

        //네트워크 상의 모든 클라이언트들에서 생성 실행
        //해당 게임 오브젝트의 주도권은, 생성 메서드를 직접 실행한 클라이언트에게 있음
        PhotonNetwork.Instantiate(playerPrefab.name, randomSpawnPos, Quaternion.identity);
    }

    public void AddScore(int newScore)//점수를 추가하고 UI 갱신
    {
        // 게임 오버가 아닌 상태에서만 점수 증가 가능
        if (!bIsGameover)
        {
            iScore += newScore;//점수 추가
            UIManager.instance.UpdateScoreText(iScore);//점수 UI 텍스트 갱신
        }
    }

    public void EndGame()//게임 오버 처리
    {
        bIsGameover = true;//게임 오버 상태 true
        UIManager.instance.SetActiveGameoverUI(true);//게임 오버 UI 활성화
    }

    private void Update()//키보드 입력(Escape)을 감지하고 룸 Exit
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    //룸을 나갈때 자동 실행되는 메서드
    public override void OnLeftRoom() 
    {
        // 룸을 나가면 로비 씬으로 돌아감
        SceneManager.LoadScene("Lobby");
    }
}