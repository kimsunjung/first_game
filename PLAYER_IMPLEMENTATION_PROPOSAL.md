# [기획] 플레이어 캐릭터 및 데이터 구조 구현 제안

이 문서는 고도 4.x .NET 환경에서 **플레이어 이동 로직**과 **데이터 구조**를 구현하기 위한 제안서입니다.

## 1. 개요
*   **목적**: 플레이어 캐릭터의 기본 이동(8방향)과 능력치(Stats) 데이터를 분리하여 구현.
*   **핵심 철학**: 
    *   **Controller**: 입력 처리 및 물리 이동 (`CharacterBody2D`)
    *   **Data**: 순수 데이터 컨테이너 (`Resource`) - WinForms의 DTO/Model 역할

## 2. 변경 사항 상세

### A. 프로젝트 설정 (Project Settings)
*   **Autoload 등록**: `GameManager.cs`를 싱글톤으로 등록하여 어디서든 접근 가능하게 설정.
    *   Path: `res://Scripts/Core/GameManager.cs`
    *   Node Name: `GameManager`

### B. 스크립트 설계

#### 1. PlayerController.cs
*   **위치**: `Scripts/Entities/Player/PlayerController.cs`
*   **역할**: 캐릭터의 물리적 이동 담당.
*   **구현 상세**:
    *   `CharacterBody2D` 상속.
    *   `_PhysicsProcess`에서 입력 감지 (`Input.GetVector`).
    *   `MoveAndSlide()` 메서드로 충돌 처리 포함 이동.
    *   `PlayerStats` 리소스를 변수로 보유하여 속도 등 데이터 참조.

```csharp
public partial class PlayerController : CharacterBody2D
{
    [Export] public PlayerStats Stats { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        Velocity = inputDir * Stats.MoveSpeed;
        MoveAndSlide();
    }
}
```

#### 2. PlayerStats.cs
*   **위치**: `Scripts/Data/PlayerStats.cs`
*   **역할**: 플레이어의 스탯 데이터 정의 (ScriptableObject 유사).
*   **구현 상세**:
    *   `Resource` 상속.
    *   `[GlobalClass]` 어트리뷰트 사용 (인스펙터에서 생성 가능).
    *   속성: `MoveSpeed`, `MaxHealth`, `CurrentHealth`.

```csharp
[GlobalClass]
public partial class PlayerStats : Resource
{
    [Export] public float MoveSpeed { get; set; } = 300.0f;
    [Export] public int MaxHealth { get; set; } = 100;
}
```

## 3. 검토 요청 사항
*   **데이터 분리**: Controller와 Data(Stats)를 분리하는 구조가 적절한지?
*   **입력 처리**: `Input.GetVector` 방식이 모바일 조작계 확장성(터치 패드 등)에 유리한지?
