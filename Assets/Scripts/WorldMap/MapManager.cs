using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동용
using TMPro; // 텍스트 제어용
public class MapManager : MonoBehaviour
{
    public Transform playerIcon;

    [Header("UI Components")]
    public GameObject tooltipObject;   // 아까 만든 StageNameTooltip 오브젝트
    public TextMeshPro tooltipText;    // 그 안의 TextMeshPro 컴포넌트
    // [추가] 선택 커서 (빨간 테두리)
    public GameObject selectionCursor; 

    [Header("Settings")]
    // [추가] 플레이어 위치 보정값 (Y를 0.5~1.0 정도로 설정하면 노드 위에 뜹니다)
    public Vector3 playerOffset = new Vector3(0, 0.75f, 0);
    int debugNextStageID = 1;
    public float tooltipOffsetY = 0.75f; // 노드 위 0.75 높이
    // 현재 선택된 노드 (없으면 null)
    private MapNode selectedNode;

    void Start()
    {
        RefreshMapState();
        MovePlayerToSavedNode();
        // 시작할 때 툴팁 숨기기
        if(tooltipObject != null) tooltipObject.SetActive(false);
    }

    void Update()
    {
        // 마우스 왼쪽 클릭 시
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            // 1. Shift가 눌려있다면? -> 초기화 (리셋)
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                ResetAllData();
            }
            // 2. Shift가 안 눌려있다면? -> 치트 클리어
            else
            {
                CheatClearStage();
            }
        }
    }
    void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        debugNextStageID = 1;
        RefreshMapState();
        Debug.Log("<color=red>[Cheat] 모든 데이터가 초기화되었습니다.</color>");
    }
    // [추가] 강제 클리어 함수
    void CheatClearStage()
    {
        // 현재 debugNextStageID를 클리어 처리
        PlayerPrefs.SetInt($"Stage_{debugNextStageID}_Cleared", 1);
        PlayerPrefs.Save();

        Debug.Log($"<color=cyan>[Cheat] 스테이지 {debugNextStageID} 클리어!</color>");

        debugNextStageID++; // 다음 번호로 이동
        RefreshMapState();  // 맵 새로고침
    }

    void HandleMouseClick()
    {
        // 1. 마우스 위치로 레이저 발사 (2D)
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        // 2. 무언가 맞았다면?
        if (hit.collider != null)
        {
            // 맞은 게 MapNode인지 확인
            MapNode clickedNode = hit.collider.GetComponent<MapNode>();

            if (clickedNode != null)
            {
                // 잠겨있는 노드면 반응 안 함
                if (clickedNode.isLocked) return;

                OnNodeClicked(clickedNode);
                return; // 함수 종료 (빈 공간 클릭 처리 안 함)
            }
        }

        // 3. 아무것도 안 맞았거나, 노드가 아닌 걸 눌렀다면 -> 선택 해제
        DeselectNode();
    }

    void OnNodeClicked(MapNode node)
    {
        // 경우 A: 이미 선택된 노드를 또 눌렀다 -> 입장!
        if (selectedNode == node)
        {
            EnterStage(node);
        }
        // 경우 B: 새로운 노드를 눌렀다 -> 이름 띄우기
        else
        {
            SelectNode(node);
        }
    }

    void SelectNode(MapNode node)
    {
        selectedNode = node;

        tooltipText.text = node.stageName;
        tooltipObject.transform.position = node.transform.position + new Vector3(0, tooltipOffsetY, 0);
        tooltipObject.SetActive(true);
        
        // [추가] 빨간 테두리 커서 이동 및 표시
        if (selectionCursor != null)
        {
            selectionCursor.transform.position = node.transform.position; // 위치 동기화
            selectionCursor.SetActive(true); // 켜기
        }
    }

    void DeselectNode()
    {
        selectedNode = null;
        if(tooltipObject != null) tooltipObject.SetActive(false);
        // [추가] 빨간 테두리 숨기기
        if (selectionCursor != null) selectionCursor.SetActive(false);
    }

    void EnterStage(MapNode node)
    {
        Debug.Log($"스테이지 입장: {node.stageName}");
        
        // 현재 위치 저장 (돌아올 때를 위해)
        PlayerPrefs.SetInt("CurrentStageID", node.stageID);
        PlayerPrefs.Save();

        // 씬 이동
        // (Build Settings에 씬이 등록되어 있어야 함)
        SceneManager.LoadScene(node.sceneName);
    }



    // 맵의 모든 잠금/해제 상태를 새로고침하는 핵심 함수
    public void RefreshMapState()
    {
        // 1. 모든 노드 찾기 (Inactive 포함)
        MapNode[] allNodes = FindObjectsByType<MapNode>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        // 2. 초기화 (일단 다 잠금)
        foreach (MapNode node in allNodes)
        {
            node.isLocked = true;
            node.isCleared = (PlayerPrefs.GetInt($"Stage_{node.stageID}_Cleared", 0) == 1);

            foreach (GameObject path in node.outgoingPaths)
            {
                if(path != null) path.SetActive(false);
            }
        }

        // 3. 시작 노드 해금
        foreach (MapNode node in allNodes)
        {
            if (node.stageID == 1) 
            {
                node.isLocked = false;
                break;
            }
        }

        // 4. [수정됨] 연쇄 해금 로직
        // 순서 문제 해결: 잠겨있는지(!isLocked) 확인하지 말고, 
        // 클리어 기록(isCleared)만 있으면 무조건 다음 길을 엽니다.
        foreach (MapNode node in allNodes)
        {
            if (node.isCleared) // <-- && !node.isLocked 삭제!
            {
                foreach (MapNode nextNode in node.nextNodes)
                {
                    if (nextNode != null) 
                    {
                        nextNode.isLocked = false;
                    }
                }
            }
        }

        // 5. 비주얼 업데이트
        foreach (MapNode node in allNodes)
        {
            node.UpdateNodeVisuals();
        }
    }

    void MovePlayerToSavedNode()
    {
        int currentID = PlayerPrefs.GetInt("CurrentStageID", 1);
        MapNode[] allNodes = FindObjectsByType<MapNode>(FindObjectsSortMode.None);
        
        foreach (MapNode node in allNodes)
        {
            if (node.stageID == currentID)
            {
                playerIcon.position = node.transform.position + playerOffset;
                break;
            }
        }
    }
}