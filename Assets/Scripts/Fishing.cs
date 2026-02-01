using UnityEngine;

public class Fishing : MonoBehaviour
{
    public string fishName = "AFish";
    public float fishPower = 1.0f;
    public float MaxSize = 10f;
    public float MinSize = 100f;
    public static System.Action OnFishDestroyed;
    
    // Called by the UI when the user clicks the result UI to remove this fish
    public void DestroyByUI()
    {
        Debug.Log($"{fishName}이(가) UI에 의해 삭제되었습니다.");
        OnFishDestroyed?.Invoke();
        Destroy(gameObject);
    }
}