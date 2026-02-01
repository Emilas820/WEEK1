using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public AudioSource soundplayer;     // AudioSource 컴포넌트 변수
    public AudioClip Duck; // 낚시 시작!
    public AudioClip Happy;      // 성공
    public AudioClip Sad;      // 실패

    private void Awake()
    {
        // 싱글톤 초기화
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        soundplayer = GetComponent<AudioSource>();
    }

    public void StartFishingSound()
    {
        soundplayer.PlayOneShot(Duck);
    }

    public void SuccessSound()
    {
        soundplayer.PlayOneShot(Happy);
    }

    public void FailSound()
    {
        soundplayer.PlayOneShot(Sad);
    }
}
