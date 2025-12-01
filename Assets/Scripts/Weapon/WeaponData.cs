using UnityEngine;

// [변경] 타입을 구체적인 무기 종류로 변경
public enum WeaponType { Sword, Bow, Whip, Staff } 

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Helper")]
    [Tooltip("이 체크박스를 클릭하면 아래 선택된 Type에 맞는 기본값으로 수치를 덮어씁니다.")]
    public bool loadDefaultValues = false; // 이 버튼을 누르면 기본값 로드

    [Header("Basic Info")]
    public string weaponName;
    public WeaponType type;
    public Sprite icon;

    [Header("Combat Stats")]
    public int damage = 1;
    public float cooldown = 0.5f;
    public int manaCost = 0;

    [Header("Melee Settings")]
    public Vector2 hitboxSize = new Vector2(1, 1);
    public float hitboxXOffset = 1.0f;
    public float hitboxYOffset = 0.5f;

    [Header("Visual Settings")]
    public GameObject effectPrefab; 
    public float visualXOffset = 0.5f; 
    public float visualYOffset = 0.5f; 

    [Header("Projectile Settings")]
    public float projectileSpeed = 10f;
    public float projectileRange = 10f;
    public bool isPiercing = false;

    // ------------------------------------------------------------------
    // [핵심 기능] 인스펙터에서 값이 바뀔 때 실행되는 함수
    // ------------------------------------------------------------------
    void OnValidate()
    {
        if (loadDefaultValues)
        {
            ApplyDefaults();
            loadDefaultValues = false; // 실행 후 체크박스 자동 해제
        }
    }

    void ApplyDefaults()
    {
        // 공통 초기화
        manaCost = 0;
        isPiercing = false;

        switch (type)
        {
            case WeaponType.Sword: // (Warrior)
                damage = 3;
                cooldown = 0.5f;
                
                hitboxSize = new Vector2(1.5f, 1.5f);
                hitboxXOffset = 1.5f;
                hitboxYOffset = 1.0f;
                
                visualXOffset = 0.5f;
                visualYOffset = 0.0f;
                break;

            case WeaponType.Whip: // (Whipper)
                damage = 1;
                cooldown = 0.75f;
                
                hitboxSize = new Vector2(1.5f, 1.0f);
                hitboxXOffset = 5.0f;
                hitboxYOffset = 0.5f;

                visualXOffset = 0.5f;
                visualYOffset = 0.5f;
                break;

            case WeaponType.Bow: // (Sniper) - 임시 기본값
                damage = 2;
                cooldown = 0.6f;
                projectileSpeed = 20f;
                projectileRange = 9f;
                isPiercing = true;
                break;

            case WeaponType.Staff: // (Magician) - 임시 기본값
                damage = 4;
                cooldown = 0.8f;
                projectileSpeed = 10f;
                projectileRange = 5f;
                manaCost = 1; // 마법사는 마나 소모
                break;
        }
        
        Debug.Log($"[{type}] 기본값이 적용되었습니다.");
    }
}