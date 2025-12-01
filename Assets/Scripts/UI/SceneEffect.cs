using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 이동용
using System.Collections;

public class SceneEffect : MonoBehaviour
{
    [Header("Settings")]
    public float fadeDuration = 0.75f;
    public float fadeOutDuration = 1.0f; // [추가] 페이드 아웃 시간 (1초)
    
    // [추가] 렉이 풀릴 때까지 기다릴 시간 (보통 0.5초면 충분)
    public float startDelay = 0.5f; 

    Image curtainImage;

    void Awake()
    {
        curtainImage = GetComponent<Image>();
        if (curtainImage != null)
        {
            curtainImage.color = Color.black;
            curtainImage.raycastTarget = true; // 클릭 방지 켜기
            curtainImage.gameObject.SetActive(true);
        }
    }

    void Start()
    {
        StartCoroutine(FadeInSequence());
    }

    IEnumerator FadeInSequence()
    {
        // 1단계: 게임이 시작되자마자 렉이 걸릴 수 있으므로, 
        // 화면이 검은 상태에서 잠시 대기합니다.
        // (이 시간 동안 유니티는 뒤에서 맵을 로딩하느라 렉이 걸려도, 플레이어는 검은 화면만 보게 됩니다.)
        yield return new WaitForSeconds(startDelay);

        // 2단계: 대기가 끝나면(렉이 풀리면) 부드럽게 페이드 인 시작
        float timer = 0f;
        Color startColor = Color.black;
        Color targetColor = new Color(0, 0, 0, 0);

        while (timer < fadeDuration)
        {
            // UnscaledDeltaTime을 쓰면 혹시라도 프레임이 떨어져도 시간은 정확히 흐릅니다.
            timer += Time.unscaledDeltaTime; 
            
            curtainImage.color = Color.Lerp(startColor, targetColor, timer / fadeDuration);
            yield return null;
        }

        // 3단계: 마무리
        curtainImage.color = targetColor;
        curtainImage.raycastTarget = false; 
    }

    // -------------------------------------------------------------
    // [추가] 외부에서 호출할 함수: 화면을 검게 만들고 씬을 이동시킨다.
    // -------------------------------------------------------------
    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(FadeOutSequence(sceneName));
    }

    IEnumerator FadeOutSequence(string sceneName)
    {
        // 1. 다시 클릭 못하게 막음
        curtainImage.raycastTarget = true;

        float timer = 0f;
        Color startColor = new Color(0, 0, 0, 0); // 투명
        Color targetColor = Color.black;          // 검정

        // 2. 서서히 검게 변함 (1초 동안)
        while (timer < fadeOutDuration)
        {
            timer += Time.unscaledDeltaTime;
            curtainImage.color = Color.Lerp(startColor, targetColor, timer / fadeOutDuration);
            yield return null;
        }

        // 3. 완전히 검게 변했으면 씬 이동
        curtainImage.color = targetColor;
        SceneManager.LoadScene(sceneName);
    }
}