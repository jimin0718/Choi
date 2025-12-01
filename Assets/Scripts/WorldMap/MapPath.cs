using UnityEngine;

[ExecuteInEditMode] // 에디터에서 플레이 안 해도 선이 보이게 함!
public class MapPath : MonoBehaviour
{
    public Transform nodeA; // 시작 스테이지 오브젝트
    public Transform nodeB; // 도착 스테이지 오브젝트

    LineRenderer lr;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        UpdatePath();
    }

    void Update()
    {
        // 에디터에서 노드 위치를 옮길 때 실시간으로 선도 따라오게 함
        if (!Application.isPlaying) 
        {
            UpdatePath();
        }
    }

    void UpdatePath()
    {
        if (lr == null || nodeA == null || nodeB == null) return;

        // 선의 점 개수를 2개로 고정
        lr.positionCount = 2;

        // 시작점과 끝점 좌표 입력
        lr.SetPosition(0, nodeA.position);
        lr.SetPosition(1, nodeB.position);

        // [추가] 선 두께를 코드로 강제 고정 (원하는 두께로 숫자를 바꾸세요)
        float pathWidth = 0.3f; 
        lr.startWidth = pathWidth;
        lr.endWidth = pathWidth;
    }
}