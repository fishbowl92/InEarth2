# InEarth2 핵심 코드 흐름

## 1. 랜덤 맵 생성

중심 파일은 [`MapManager.cs`](../MapManager.cs)와 [`Maptile.cs`](../ScriptTable/Maptile.cs)입니다.

### 처리 순서

1. `GenerateWorld`가 `width`와 `height` 크기의 `TileNode[,]` 격자를 생성합니다.
2. `chanceToStartAlive` 확률에 따라 각 타일을 벽 또는 빈 공간으로 초기화합니다.
3. `doSimulationStep`이 각 타일 주변 8칸의 벽 수를 계산합니다.
4. 벽 타일은 주변 벽 수가 `deathLimit`보다 작으면 제거합니다.
5. 빈 타일은 주변 벽 수가 `birthLimit`보다 크면 벽으로 변경합니다.
6. 위 과정을 `numberOfSteps`만큼 반복합니다.
7. 맵 경계를 벽으로 고정하고, 주변 상태를 기준으로 아이템 배치 후보 위치를 수집합니다.

이 방식은 셀룰러 오토마타(Cellular Automata, 주변 상태 규칙을 반복 적용해 전체 형태를 만드는 방식)의 기본 원리를 격자형 맵 생성에 적용한 것입니다.

## 2. 연쇄 번개

관련 파일은 [`Skill.cs`](../ScriptTable/Skill.cs)와 [`Player.cs`](../Player.cs)입니다.

### 처리 순서

1. `ChainLightning`이 피해량과 연쇄 횟수를 `TriggerData`에 기록합니다.
2. 플레이어의 `makeChainLight` 코루틴을 시작합니다.
3. `returnNearEnemy`가 현재 타격 위치의 인접 타일에서 다음 적을 찾습니다.
4. 매 프레임 번개 효과를 다음 대상 방향으로 이동합니다.
5. 효과가 도착하면 피해를 적용하고, 도착 지점을 다음 탐색의 시작점으로 변경합니다.
6. 연쇄 횟수가 끝나거나 다음 적이 없으면 이펙트를 비활성화합니다.

코루틴은 별도 스레드가 아니라 Unity의 주 실행 흐름에서 `yield` 지점마다 실행을 나누는 방식입니다. 이 구현에서는 계산 성능 자체보다 타격과 시각 효과를 순서대로 표현하고, 각 타격 시점의 맵 상태를 다시 확인하는 데 사용했습니다.

## 3. 타일 상태 조회

`MapManager`는 타일의 숫자 상태를 다음 기능에서 공통으로 사용합니다.

- 이동 가능 여부 확인
- 플레이어와 몬스터 위치 기록
- 적 사망 상태 처리
- 아이템 존재 여부 확인
- 스킬 대상 타일 판정

`ReadCode`와 `getTileData`가 상태 해석을 담당하고, `tileNodeSortSet`이 상태 변경을 담당합니다. 여러 전투 기능이 같은 격자 정보를 참조한다는 점이 핵심입니다.
