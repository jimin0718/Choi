using UnityEngine;

public class BossUnit : MonoBehaviour
{
    // 씬에 있는 표지판을 연결
    public Signpost targetSignpost; 

    // Enemy.cs의 Die() 함수가 호출될 때 이 함수도 같이 실행되어야 합니다.
    // 방법 1: Enemy.cs의 Die()를 수정해서 이벤트를 호출하거나
    // 방법 2: OnDestroy()를 활용합니다. (가장 쉬움)

    void OnDestroy()
    {
        // 보스 오브젝트가 파괴될 때(죽을 때) 실행
        // (단, 씬 이동으로 파괴될 때는 실행 안 되게 주의해야 함. 보통은 죽어서 Destroy될 때 실행됨)
        if (targetSignpost != null && gameObject.scene.isLoaded) 
        {
            targetSignpost.UnlockSignpost();
        }
    }
}