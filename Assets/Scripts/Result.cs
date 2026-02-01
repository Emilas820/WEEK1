using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class Result : MonoBehaviour, IPointerClickHandler
{
    private GameObject currentFish;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI sizeText;


    // GameManager가 물고기를 생성한 직후 이 함수를 호출할 겁니다.
    public void SetResultData(GameObject fishObj)
    {
        gameObject.SetActive(true); // 여기서 자신을 켭니다.
        currentFish = fishObj;

        Fishing fishInfo = fishObj.GetComponent<Fishing>();
        if (fishInfo != null)
        {
            nameText.text = fishInfo.fishName;
            float randomSize = Random.Range(fishInfo.MinSize, fishInfo.MaxSize);
            sizeText.text = $"Size: {randomSize:F2} cm";
        }
    }

    // UI에서 클릭을 감지하여 물고기를 제거하도록 요청합니다.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentFish != null)
        {
            Fishing fishInfo = currentFish.GetComponent<Fishing>();
            if (fishInfo != null)
            {
                fishInfo.DestroyByUI();
                currentFish = null;
                return;
            }
        }

        HideResult();
    }
    
    void OnEnable() 
    {
        // 물고기 파괴 알림 구독 시작
        Fishing.OnFishDestroyed += HideResult;
    }

    void OnDisable() 
    {
        // 구독 해지 (메모리 누수 방지)
        Fishing.OnFishDestroyed -= HideResult;
    }

    public void HideResult() 
    {
        gameObject.SetActive(false);
    }
    
}