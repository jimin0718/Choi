using UnityEngine;
using System.Collections.Generic; // 리스트 사용을 위해 필수

public class MapNode : MonoBehaviour
{
    [Header("Info")]
    public int stageID;
    
    // [추가] 스테이지 이름 (표시용)
    public string stageName = "스테이지 이름"; 
    
    // [추가] 이동할 씬 이름 (TestStage 등)
    public string sceneName = "Level_1"; 
    
    [Header("Connections")]
    // [핵심] 다음 스테이지가 여러 개일 수 있으므로 List로 선언합니다.
    public List<MapNode> nextNodes; 
    
    // [핵심] 뻗어나가는 길도 여러 개일 수 있습니다.
    public List<GameObject> outgoingPaths; 

    [Header("State (Read Only)")]
    public bool isCleared = false; // 클리어 했나?
    public bool isLocked = true;   // 잠겨 있나?

    SpriteRenderer sr;

    void Awake() // Start 대신 Awake 권장 (Manager보다 먼저 변수 할당)
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // 상태에 따라 색깔과 오브젝트를 켜고 끄는 함수
    public void UpdateNodeVisuals()
    {
        if (isLocked)
        {
            // 잠김: 숨기기
            gameObject.SetActive(false);
            foreach (GameObject path in outgoingPaths)
            {
                if(path != null) path.SetActive(false);
            }
        }
        else
        {
            // 해금됨: 보이기
            gameObject.SetActive(true);

            if (isCleared)
            {
                // [수정됨] 클리어 시 빨간색 -> 하얀색으로 변경
                sr.color = Color.white; 
                
                // 길 보여주기
                foreach (GameObject path in outgoingPaths)
                {
                    if(path != null) path.SetActive(true);
                }
            }
            else
            {
                // 아직 안 깼으면 노란색 (그대로 유지)
                sr.color = Color.yellow; 
                
                // 길 숨기기
                foreach (GameObject path in outgoingPaths)
                {
                    if(path != null) path.SetActive(false);
                }
            }
        }
    }
}