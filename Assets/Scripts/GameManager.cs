using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [Header("저는 싱글톤입니다.")]
    public static GameManager instance;

    [Header("타이머")]
    [SerializeField] int timer = 1; // 몇 초에 한 번 추첨할 것인가?
    [SerializeField] float getFishrate = 20; // <- >확률로 낚시를 성공함.

    [Header("물고기")]
    public GameObject[] fish;

    [Header("UI")]
    public Text startText;
    public GameObject resultUI;

    [Header("이벤트")]
    public GameObject handle;
    public UnityEngine.UI.Image gage;
    public bool getFish;    // 물고기를 먹었냐?

    public bool eventStart = false;

    public int eventTimer = 10;

    private float acquireTimer = 0f; // 획득 타이머
    private GameObject currentHandle = null; // 현재 활성화된 handle
    public int acquireResult = -1; // 추첨 결과 저장 (-1: 미정, 0~: 물고기 인덱스)
    public float stringRate = 0f; // 게이지 상태 (0~1)
    
    private void Awake()
    {
        // 싱글톤 초기화
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        acquireTimer = timer;
    }

    private void Update()
    {
        AcquireTimerTick();
        CheckHandleStatus();
    }

    /// <summary>
    /// 획득 타이머 체크
    /// </summary>
    private void AcquireTimerTick()
    {
        // Fish 태그의 오브젝트가 존재하면 타이머 멈춤
        GameObject fishObject = GameObject.FindGameObjectWithTag("Fish");
        if (fishObject != null)
            return;
        
        // handle이 활성화되어 있으면 타이머 멈춤
        if (currentHandle != null)
            return;
        
        acquireTimer -= Time.deltaTime;

        if (acquireTimer <= 0f)
        {
            // 타이머 리셋
            acquireTimer = timer;

            // 확률 체크
            if (Random.Range(0f, 100f) < getFishrate)
            {
                TriggerFishAcquireEvent();
                eventStart = true;
                Debug.Log("잡았다!");
                Invoke("EventChecker", eventTimer);
            }
        }
    }

    /// <summary>
    /// 물고기 획득 이벤트 발생
    /// </summary>
    private void TriggerFishAcquireEvent()
    {
        // 시작!
        SoundManager.instance.StartFishingSound();
        // 랜덤한 물고기 선택
        acquireResult = Random.Range(0, fish.Length);
        Debug.Log($"{acquireResult+1}번 물고기 선택");

        // 낚시 성공 상태 초기화 (이전 상태가 남아있지 않도록 함)
        getFish = false;
        
        // stringRate 초기화
        stringRate = 0f;
        
        // gage 활성화
        gage.gameObject.SetActive(true);
        
        // startText 활성화
        if (startText != null)
        {
            startText.gameObject.SetActive(true);
        }

        // handle 생성
        currentHandle = Instantiate(handle, new Vector3(-1.2f, -3f, 0f), Quaternion.identity);
    }

    void EventChecker()
    {
        eventStart = false;
    }

    /// <summary>
    /// handle의 isGetFish 상태 체크
    /// </summary>
    private void CheckHandleStatus()
    {
        if (currentHandle == null)
            return;

        // handle 오브젝트에서 isGetFish 값을 확인
        var handleScript = currentHandle.GetComponent<Handle>();
        if (handleScript != null && getFish)
        {
            // 성공!
            SoundManager.instance.SuccessSound();
            // handle 삭제
            Destroy(currentHandle);
            gage.gameObject.SetActive(false);

            // 추가: 결과 창 활성화
            if (resultUI != null)
            {
                resultUI.SetActive(true);
                SpawnFish();
            }
            
            currentHandle = null;

            // stringRate 초기화
            stringRate = 0f;
        }
    }

    private void SpawnFish()
{
    if(getFish == true)
    {
        if (acquireResult >= 0 && acquireResult < fish.Length)
        {
            // 1. 물고기 생성
            GameObject spawnedFish = Instantiate(fish[acquireResult], new Vector2(0, -4), Quaternion.identity);

            // 2. Result UI를 가져와서 데이터 세팅 (싱글톤 활용)
            // resultUI 오브젝트에 Result 스크립트가 붙어있어야 합니다.
            Result resultScript = resultUI.GetComponent<Result>();
            if(resultScript != null)
            {
                resultScript.SetResultData(spawnedFish); 
            }
        }
    }        
}
}
