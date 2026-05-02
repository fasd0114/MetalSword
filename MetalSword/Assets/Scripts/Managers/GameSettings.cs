using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterClass
{
    Archer,   
    Wizard,  
    Swordsman
}

public class GameSettings : MonoBehaviour
{
    [Header("테스트용 직업 선택")]
    [SerializeField] private CharacterClass startingClass;

    // 시간, 리소스 부족 등의 이유로 직업 분리 폐기
    // 그러나 직업에 따른 플레이어 캐릭터 동적 생성 기능은 유지 (Archer 클래스 및 Swordsman 클래스에 동일한 프리펩 사용하여 확인함)
    public CharacterClass StartingClass => startingClass;
}
