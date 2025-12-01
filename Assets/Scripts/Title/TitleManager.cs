using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필수!

public class TitleManager : MonoBehaviour
{
    [Header("Settings")]
    public string gameSceneName = "Level_1"; // 이동할 게임 씬 이름 (정확해야 함)

    // 플레이 버튼
    public void ClickPlay()
    {
        // 나중에는 여기서 세이브 파일 선택 창을 띄우겠지만,
        // 지금은 바로 게임 씬으로 넘어갑니다.
        SceneManager.LoadScene(gameSceneName);
    }

    // 설정 버튼
    public void ClickSettings()
    {
        Debug.Log("설정 창 열기 (아직 구현 안 됨)");
        // 나중에 설정 팝업창(Panel)을 SetActive(true) 하는 코드가 들어갑니다.
    }

    // 종료 버튼
    public void ClickQuit()
    {
        Debug.Log("게임 종료");
        
        // 에디터에서는 게임 종료가 안 되므로 힌트 표시
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit(); // 실제 빌드된 게임에서 꺼짐
        #endif
    }
}