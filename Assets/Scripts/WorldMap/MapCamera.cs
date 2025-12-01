using UnityEngine;

public class MapCamera : MonoBehaviour
{
    [Header("Settings")]
    public float minX;
    public float maxX;
    public float dragSpeed = 1.0f; 

    private Vector3 dragOrigin; 
    private bool isDragging = false; // [추가] 드래그 중인지 판단하는 플래그

    void LateUpdate()
    {
        HandleMouseInput();
    }

    void HandleMouseInput()
    {
        // 1. 마우스를 처음 눌렀을 때
        if (Input.GetMouseButtonDown(0))
        {
            // [핵심] 클릭한 위치에 노드가 있는지 검사
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            // 무언가에 맞았고, 그것이 'MapNode' 컴포넌트를 가지고 있다면?
            if (hit.collider != null && hit.collider.GetComponent<MapNode>() != null)
            {
                // 노드를 눌렀으므로 드래그 금지!
                isDragging = false;
                return; 
            }

            // 노드가 아니라면 드래그 시작
            isDragging = true;
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        // 2. 마우스를 누른 채 움직일 때
        if (Input.GetMouseButton(0))
        {
            // 드래그가 허용된 상태(isDragging == true)일 때만 움직임
            if (isDragging)
            {
                Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 difference = dragOrigin - currentMousePos;
                Vector3 moveVector = new Vector3(difference.x * dragSpeed, 0, 0);
                
                transform.position += moveVector;

                float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
                transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
            }
        }

        // 3. 마우스를 뗐을 때
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false; // 드래그 종료
        }
    }
}