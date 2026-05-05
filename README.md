# Project : MetalSwordGrand

### [Navigation]

상세한 설계 의도와 트러블 슈팅 사례는 **[노션 포트폴리오](https://www.notion.so/3D-RPG-MetalSwordGrand-353fa4e779b7801dbd38ff918afb4611?source=copy_link)**에서 확인하실 수 있습니다.  
이 README는 노션 문서의 기술적 구현체를 빠르게 확인하기 위한 가이드라인입니다.

| 기능 구분 | 핵심 스크립트 / 폴더 위치 | 주요 구현 및 트러블 슈팅 포인트 |
| :--- | :--- | :--- |
| **Player 시스템** | [/Scripts/Player](https://github.com/fasd0114/MetalSword/tree/main/MetalSword/Assets/Scripts/Player) | 상태 제어 및 전투 로직 관리 |
| **중앙 집중식 AI 제어** | [/Scripts/Monster](https://github.com/fasd0114/MetalSword/tree/main/MetalSword/Assets/Scripts/Monster) | FSM 기반의 Monster AI 관리 |
| **성능 최적화** | [MonsterAI.cs](https://github.com/fasd0114/MetalSword/blob/main/MetalSword/Assets/Scripts/Monster/MonsterAI.cs) | 애니메이션 클립 데이터 캐싱을 통한 런타임 최적화 |
| **데이터 기반 설계** | [MonsterData.cs](https://github.com/fasd0114/MetalSword/blob/main/MetalSword/Assets/Scripts/Monster/MonsterData.cs) | ScriptableObject를 활용한 시스템 확장성 확보 |
| **데이터 영속성 관리** | [DataManager.cs](https://github.com/fasd0114/MetalSword/blob/main/MetalSword/Assets/Scripts/Managers/DataManager.cs)| 세이브 시스템 및 싱글톤 기반의 데이터 연속성 유지 |
| **UI 시스템** | [Scripts/UI](https://github.com/fasd0114/MetalSword/tree/main/MetalSword/Assets/Scripts/UI)| 이벤트 기반 반응형 UI 시스템 |

<br>

> 본 프로젝트는 **Unity 2022.3.61f1** 환경에서 **C#** 을 사용하여 개발되었습니다.
