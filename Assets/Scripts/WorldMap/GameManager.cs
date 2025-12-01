using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // 싱글톤

    [Header("Prefabs")]
    public GameObject damagePopupPrefab; // 여기에 프리팹 연결

    void Awake()
    {
        if (instance == null) instance = this;
    }
}