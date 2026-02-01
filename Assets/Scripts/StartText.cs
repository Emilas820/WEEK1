using UnityEngine;
using UnityEngine.UI;

public class StartText : MonoBehaviour
{
    private Text textComponent;
    
    // 애니메이션 설정값들...
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private float animationDuration = 1f;
    
    private float scaleTimer = 0f;
    private float eventStartTime = 0f;
    private Color originalColor = Color.white;

    private void Awake() // Start보다 빠르게 참조 확보
    {
        textComponent = GetComponent<Text>();
        if (textComponent != null) originalColor = textComponent.color;
    }

    private void Update()
    {
        // 1. 싱글톤 체크 (Find 함수 삭제)
        if (GameManager.instance == null || !GameManager.instance.eventStart)
        {
            // 이벤트가 끝났으면 스스로를 비활성화 하거나 초기화
            return;
        }

        // 2. 낚시 성공(getFish) 시 즉시 꺼지도록 보완
        if (GameManager.instance.getFish)
        {
            gameObject.SetActive(false);
            return;
        }
        
        UpdateScale();
        UpdateColor();
    }
    
    private void UpdateScale()
    {
        scaleTimer = (scaleTimer + Time.deltaTime) % animationDuration;
        float sinValue = Mathf.Sin((scaleTimer / animationDuration) * Mathf.PI);
        float scale = Mathf.Lerp(minScale, maxScale, sinValue);
        transform.localScale = new Vector3(scale, scale, 1f);
    }
    
    private void UpdateColor()
    {
        eventStartTime += Time.deltaTime;
        // 싱글톤을 통해 직접 접근
        float colorLerpValue = Mathf.Clamp01(eventStartTime / GameManager.instance.eventTimer);
        if (textComponent != null)
        {
            textComponent.color = Color.Lerp(originalColor, Color.red, colorLerpValue);
        }
    }

    public void ResetAnimation()
    {
        scaleTimer = 0f;
        eventStartTime = 0f;
        transform.localScale = Vector3.one;
        if (textComponent != null) textComponent.color = originalColor;
    }

    private void OnDisable()
    {
        ResetAnimation();
    }
}