using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Handle : MonoBehaviour
{
    // 낚시줄의 최대/최소 길이
    [SerializeField] private float maxLineLength = 10f;
    [SerializeField] private float minLineLength = 1f;
    
    // 문자열 감기 설정
    [SerializeField] private float maxRollingRate = 20f; // 최대 바퀴수 (기본값: 1바퀴)
    [SerializeField] private float reelSensitivity = 1.0f; // 감기 감도 (높을수록 게이지가 빨리 참)
    
    // UI 게이지
    private Image gageImage; // Gage UI Image (태그로 런타임에 할당됨)
    private Image fishGageImage; // Gage UI Image (태그로 런타임에 할당됨)
    
    // 손잡이 (자식) - 회전이 적용될 오브젝트
    [SerializeField] private Transform handleChild; // 손잡이 (자식) 오브젝트
    
    // 현재 낚시줄 길이
    private float currentLineLength;
    
    // 룰러 회전 각도 (Z축)
    private float currentRotation = 0f;
    private float totalAccumulatedRotation = 0f; // 누적 회전값 (StringRate 계산용)
    
    // 문자열 감기 상태
    private float stringRate = 0f; // 0 = 기본, 1 = 최대 감김, -1 = 최대 풀림
    
    // 마우스 입력 상태
    private bool isDragging = false;
    private float previousMouseAngle = 0f;

    [Header("물고기")]
    public GameObject[] fish;

    // startText (태그로 런타임에 할당됨)
    private Text startText;

    // 낚시 성공 조건 1 - 게이지 유지
    public float getRate = 0.5f;

    // 낚시 성공 조건 2 - 몇 초간?
    public float getTime = 3.0f;
    
    private GameObject backImage;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI sizeText;

    // 물고기 파워 (게이지 감소 속도)
    private float fishPower = 0f; // 0~5, 높을수록 빠르게 감소
    
    // 성공 조건 체크용 변수
    private float successTimer = 0f; // 게이지 유지 시간 체크

        void Start()
    {
        currentLineLength = maxLineLength / 2f;
        currentRotation = 0f;
        
        if (handleChild == null && transform.childCount > 0)
        {
            handleChild = transform.GetChild(0);
        }

        // 1. 싱글톤에서 바로 값을 가져옵니다.
        if (GameManager.instance != null)
        {
            stringRate = GameManager.instance.stringRate;
            // 1. 가져온 stringRate에 맞춰 누적 회전값 역산 (데이터 동기화)
            float maxRotationLimit = 360f * maxRollingRate;
            totalAccumulatedRotation = stringRate * maxRotationLimit;
        }

        // 2. 태그로 찾던 무거운 코드 대신 싱글톤에 등록된 UI를 바로 씁니다.
        // (GameManager에 gageImage가 등록되어 있다고 가정)
        gageImage = GameManager.instance.gage;
        fishGageImage = GameManager.instance.fishgageBar;
        
        if (gageImage != null)
        {
            gageImage.fillAmount = stringRate;
        }
        
        if (fishGageImage != null)
        {
            fishGageImage.fillAmount = 0f;
        }
        
    }

    void Update()
    {
        // 3. Find 대신 싱글톤 인스턴스로 즉시 확인
        if (GameManager.instance == null || !GameManager.instance.eventStart)
        {
            // 이벤트가 종료되었는데 아직 물고기를 잡지 못한 상태(getFish가 false)라면 실패 처리
            if (GameManager.instance != null && !GameManager.instance.getFish)
            {
                SoundManager.instance.FailSound();
                Debug.Log("낚시 실패!");
            }

            ResetValues();
            Destroy(gameObject);
            return;
        }
        
        HandleMouseInput();
        UpdateReelRotation();
        ApplyFishPowerDecay();
        
        // 4. 값 업데이트도 싱글톤에 직접 수행
        GameManager.instance.stringRate = stringRate;
        
        CheckSuccessCondition();
        UpdateGageUI();
    }
    
    private void HandleMouseInput()
{
    // 마우스 좌클릭 시작
    if (Input.GetMouseButtonDown(0))
    {
        isDragging = true;
        previousMouseAngle = GetMouseAngleFromWheelCenter();
    }
    
    // 마우스 드래그 중
    if (Input.GetMouseButton(0) && isDragging)
    {
        float currentMouseAngle = GetMouseAngleFromWheelCenter();
        float angleDifference = currentMouseAngle - previousMouseAngle;
        
        // 각도 차이를 -180 ~ 180 범위로 보정
        if (angleDifference > 180f) angleDifference -= 360f;
        else if (angleDifference < -180f) angleDifference += 360f;
        
        // 1. 시각적인 핸들 회전은 제한 없이 적용 (핸들이 안 멈춤)
        currentRotation += angleDifference;
        
        // 2. 누적 데이터 계산 (0 ~ 최대치 사이로 가두기)
        float maxRotationLimit = 360f * maxRollingRate;
        totalAccumulatedRotation = Mathf.Clamp(totalAccumulatedRotation - (angleDifference * reelSensitivity), 0f, maxRotationLimit);
        
        // 3. 게이지 값 갱신
        stringRate = totalAccumulatedRotation / maxRotationLimit;

        // 4. 나머지 처리
        previousMouseAngle = currentMouseAngle;
    }
    
    // 마우스 좌클릭 종료
    if (Input.GetMouseButtonUp(0))
    {
        isDragging = false;
    }
}
    

    // 휠 중심(피봇)으로부터 마우스의 각도를 계산합니다 (0도 = 오른쪽, 90도 = 위쪽)
    private float GetMouseAngleFromWheelCenter()
    {
        Vector3 wheelWorldPos = transform.position;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // 휠 중심으로부터 마우스까지의 벡터
        Vector2 direction = (mouseWorldPos - wheelWorldPos);
        
        // 각도 계산 (Atan2는 -180 ~ 180 범위)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        return angle;
    }

    // 마우스 거리로부터 낚시줄 길이를 업데이트합니다
    private void UpdateLineLengthFromDistance()
    {
        Vector3 wheelWorldPos = transform.position;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        float distance = Vector3.Distance(wheelWorldPos, mouseWorldPos);
        
        // 거리를 낚시줄 길이로 매핑
        currentLineLength = Mathf.Clamp(distance, minLineLength, maxLineLength);
    }
    
    private void UpdateReelRotation()
    {
        // Z축 기준 회전 적용 (손잡이 자식에만 적용)
        if (handleChild != null)
        {
            handleChild.localRotation = Quaternion.Euler(0, 0, currentRotation);
        }
    }
    

    /// 물고기 파워에 따른 stringRate 감소 적용
    private void ApplyFishPowerDecay()
    {
        if (GameManager.instance == null)
            return;
        
        // GameManager에서 현재 물고기 정보 가져오기
        int acquireResult = GameManager.instance.acquireResult;
        
        // 유효한 물고기 인덱스인 경우
        if (acquireResult >= 0 && acquireResult < GameManager.instance.fish.Length)
        {
            // 해당 물고기 오브젝트에서 fishPower 값을 가져옴
            Fishing fishScript = GameManager.instance.fish[acquireResult].GetComponent<Fishing>();
            if (fishScript != null)
            {
                fishPower = fishScript.fishPower;

                // fishPower에 따라 totalAccumulatedRotation 감소 (stringRate의 근본 데이터)
                float maxRotationLimit = 360f * maxRollingRate;
                float decayAmount = (fishPower * 0.1f) * maxRotationLimit * Time.deltaTime;

                totalAccumulatedRotation = Mathf.Max(totalAccumulatedRotation - decayAmount, 0f);

                // 감소된 누적 회전값을 바탕으로 stringRate 갱신
                stringRate = totalAccumulatedRotation / maxRotationLimit;
            }
        }
    }
    
    // 낚시 성공 조건 체크
    // stringRate가 getRate 이상을 getTime 동안 유지하면 성공
    private void CheckSuccessCondition()
    {
        // stringRate가 getRate 이상을 만족할 때만 타이머 증가
        if (stringRate >= getRate - 0.01f)
        {
            // 타이머 증가
            successTimer += Time.deltaTime;
            
            if (successTimer >= getTime)
            {
                GameManager.instance.getFish = true;
                Debug.Log("낚시 성공! 물고기를 낚았습니다.");
            }
        }
        else
        {
            // getRate 미만이면 즉시 0으로 초기화 (fishGageImage도 0)
            successTimer = 0f;
        }
    }
    
    /// <summary>
    /// Handle의 모든 값을 초기값으로 리셋합니다
    /// </summary>
    private void ResetValues()
    {
        currentRotation = 0f;
        totalAccumulatedRotation = 0f;
        stringRate = 0f;
        successTimer = 0f;
        fishPower = 0f;
        isDragging = false;
        
        // GameManager의 stringRate도 초기화
        if (GameManager.instance != null)
        {
            GameManager.instance.stringRate = 0f;
            GameManager.instance.acquireResult = -1;
            
            // Gage 이미지 끄기
            if (GameManager.instance.gage != null)
            {
                GameManager.instance.gage.gameObject.SetActive(false);
            }
            
            // Fish Gage 이미지 초기화
            if (GameManager.instance.fishgage != null)
            {
                GameManager.instance.fishgage.fillAmount = 0f;
                GameManager.instance.fishgage.gameObject.SetActive(false);
            }
            
            // StartText 비활성화
            if (GameManager.instance.startText != null)
            {
                GameManager.instance.startText.gameObject.SetActive(false);
            }
        }
        
        Debug.Log("Handle 값들이 초기화되었습니다.");
    }
    
    /// <summary>
    /// UI 게이지 Fill Amount를 업데이트합니다
    /// gageImage: StringRate (0 ~ 1)를 Fill Amount (0 ~ 1)로 직접 매핑
    /// fishGageImage: successTimer가 getRate를 충족하는 시간동안 상승 (0 ~ 1)
    /// </summary>
    private void UpdateGageUI()
    {
        if (gageImage != null)
        {
            // StringRate (0 ~ 1)를 Fill Amount (0 ~ 1)로 직접 매핑
            gageImage.fillAmount = Mathf.Clamp01(stringRate);
        }
        if (fishGageImage != null)
        {
            fishGageImage.fillAmount = Mathf.Clamp01(stringRate);
            float fillAmount = Mathf.Clamp01(successTimer / getTime);
            Debug.Log($"stringRate: {stringRate}, getRate: {getRate}, successTimer: {successTimer}, getTime: {getTime}, fillAmount: {fillAmount}");
        
            // ← 여기! 실제로 할당해야 함
            fishGageImage.fillAmount = fillAmount;
        }
    }
    
    // 현재 낚시줄 길이 반환
    public float GetLineLength()
    {
        return currentLineLength;
    }
    
    // 현재 룰러 회전각 반환
    public float GetRotation()
    {
        return currentRotation;
    }
    
    // 회전각을 0~360 범위로 정규화하여 반환
    public float GetNormalizedRotation()
    {
        return Mathf.Repeat(currentRotation, 360f);
    }
    
    // 문자열 감기 상태 반환
    public float GetStringRate()
    {
        return stringRate;
    }
    
    // 최대 감기 바퀴수 반환
    public float GetMaxRollingRate()
    {
        return maxRollingRate;
    }
    
    // Handle이 파괴될 때 현재 stringRate를 GameManager에 저장
    private void OnDestroy()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.stringRate = stringRate;
        }
    }
}
