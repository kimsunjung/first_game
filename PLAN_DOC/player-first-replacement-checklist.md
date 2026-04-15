# player.tscn 기준 1차 교체 체크리스트

## 문서 목적
- `Franuka` 에셋 적용 시 `player.tscn`을 첫 교체 대상으로 삼아 확인할 항목을 정리한다.
- 이후 `enemy.tscn`, `NPC`, 맵 교체의 기준점을 만든다.

작성일: 2026-04-15

---

## 1. 준비물
- `Fantasy RPG Heroes Pack`
- 현재 `Scenes/Characters/player.tscn`
- 현재 플레이어 로직:
  - `Scripts/Entities/Player/PlayerController.cs`
  - `PlayerController.Animation.cs`
  - `PlayerController.Combat.cs`
  - `PlayerController.Camera.cs`

---

## 2. 교체 전 확인

### 에셋 구조 확인
- 방향 수: 4방향인지
- 필수 애니메이션 존재 여부:
  - idle
  - run/walk
  - attack
  - hit
  - death
- 프레임 크기와 프레임 수 기록
- 스프라이트 기준점이 바닥 중심인지 확인

### 현재 씬 확인
- `Scenes/Characters/player.tscn`
  - `AnimatedSprite2D`
  - `CollisionShape2D`
  - `Camera2D`
- 현재 카메라 값:
  - `zoom = Vector2(3, 3)`

---

## 3. 1차 교체 작업

### Step 1. 새 리소스 추가
- `Resources/Franuka/Heroes/...` 경로로 가져오기
- 원본 파일명은 가급적 유지

### Step 2. 플레이어 애니메이션 세트 구성
- `AnimatedSprite2D` 또는 `SpriteFrames`에 새 애니메이션 연결
- 방향별 이름 규칙을 코드와 맞춘다

예시 확인 항목:
- `idle_down`
- `idle_up`
- `idle_side`
- `run_down`
- `run_up`
- `run_side`
- `attack_*`

### Step 3. 좌우 반전 전략 확인
- `side` 애니메이션 1세트만 두고 `FlipH`로 처리 가능한지 확인
- 별도 좌/우 애니메이션이 있다면 코드 수정 필요 여부 판단

### Step 4. 충돌체 재확인
- `CollisionShape2D`가 발 아래 기준과 맞는지 확인
- 머리/무기 크기 때문에 충돌체를 키우지 말고, 발 위치 기준으로 유지

### Step 5. 카메라와 화면 크기 확인
- `zoom=(3,3)`에서 플레이어가 너무 크거나 작은지 확인
- 필요 시 우선 `scale`로 맞추고, 카메라 줌은 마지막에 조정

---

## 4. 기능 체크리스트

### 이동
- idle -> run 전환 시 크기 점프가 없는가
- 좌/우 이동 시 뒤집힘이 자연스러운가
- 상/하 이동 시 방향 전환이 맞는가

### 공격
- 공격 애니메이션이 실제 공격 방향과 맞는가
- 공격 중 캐릭터가 너무 앞으로 튀거나 잘리지 않는가
- 무기 연출과 히트 판정이 어색하지 않은가

### 피격/사망
- 피격 플래시와 애니메이션이 정상적으로 보이는가
- 죽을 때 기준점이 흔들리지 않는가

### 스킬
- 스킬 사용 중 캐릭터가 과하게 깨져 보이지 않는가
- 대시, 근접 스킬, 원거리 스킬 중 화면 튐이 없는가

### UI/월드 조화
- 기존 HUD와 붙여 놨을 때 이질감이 과하지 않은가
- 기존 배경과 비교해 너무 작거나 너무 화려하지 않은가

---

## 5. 실패 신호
- idle과 run의 프레임 크기가 달라서 순간 점프가 보임
- 공격 시 발 기준이 바뀌어 캐릭터가 붕 뜸
- `FlipH` 처리와 애니메이션 방향이 충돌함
- 충돌체와 스프라이트 발 위치가 달라 벽에 박히는 느낌이 남
- 현재 `town`, `field_1` 배경과 너무 이질적임

이 중 2개 이상이면 전체 교체를 바로 진행하지 말고, 플레이어 교체를 먼저 다듬는다.

---

## 6. 완료 기준
- 이동, 공격, 피격, 사망이 모두 자연스럽다
- 카메라 구도와 스케일이 유지된다
- HUD와 함께 봐도 크게 어색하지 않다
- 이후 `enemy.tscn` 교체 기준으로 삼을 수 있다

---

## 7. 다음 단계
플레이어 교체가 통과하면 다음 순서로 간다.
1. `enemy.tscn`
2. `shop_npc.tscn`, `skill_shop_npc.tscn`, `blacksmith_npc.tscn`
3. `town.tscn`

---

## 8. 한 줄 결론
`Franuka` 전환의 성공 여부는 `player.tscn` 1차 교체 품질에서 거의 결정된다. 여기서 애니메이션, 기준점, 스케일을 먼저 안정화해야 한다.
