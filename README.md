# ZombiePizzaSurvival

# Unity 프로젝트 폴더 구조 및 협업 규칙

프로젝트명 예시: `ZombiePizzaSurvival`

이 문서는 Unity 2인 개발 프로젝트에서 파일이 섞이지 않도록 관리하기 위한 폴더 구조와 네이밍 규칙을 정리한 문서입니다.

---

## 1. 기본 원칙

### 직접 만든 파일은 `_Project` 안에 넣기

Unity 프로젝트에서 직접 만든 스크립트, 씬, 프리팹, UI, 아트 리소스는 모두 아래 폴더 안에서 관리합니다.

```text
Assets/_Project
```

외부 에셋을 Import하면 `Assets` 바로 아래에 별도 폴더가 생길 수 있습니다.

예시:

```text
Assets/TextMesh Pro
Assets/Plugins
Assets/LowPolyZombiePack
```

외부 에셋 폴더는 그대로 두고, 우리 프로젝트에서 직접 관리하는 파일만 `_Project` 안에 넣습니다.

---

## 2. 전체 폴더 구조

```text
Assets
└── _Project
    ├── Animations
    ├── Art
    │   ├── Characters
    │   ├── Icons (추가 2026-07-09)
    │   ├── Zombies
    │   ├── Props
    │   ├── Weapons
    │   └── PizzaMachine
    ├── Audio
    ├── Effects
    ├── Materials
    ├── Prefabs
    │   ├── Player
    │   ├── Zombies
    │   ├── Items
    │   ├── Weapons
    │   └── Base
    ├── Scenes
    ├── Scripts
    │   ├── Player
    │   ├── Zombie
    │   ├── Combat
    │   ├── Delivery
    │   ├── Farming
    │   ├── Inventory
    │   ├── Crafting
    │   ├── Upgrade
    │   ├── item (추가 2026-07-10)
    │   ├── UI
    │   └── Save
    ├── ScriptableObjects
    └── UI
```

---

# 3. 폴더별 설명

## 3-1. Animations

경로:

```text
Assets/_Project/Animations
```

캐릭터, 좀비, 무기, 기계 등의 애니메이션 파일을 넣는 폴더입니다.

예시:

```text
Player_Idle.anim
Player_Run.anim
Player_Attack.anim
Zombie_Walk.anim
Zombie_Attack.anim
PizzaMachine_Working.anim
PizzaMachine_Broken.anim
```

규칙:

```text
플레이어 애니메이션: Player_
좀비 애니메이션: Zombie_
피자기계 애니메이션: PizzaMachine_
```

---

## 3-2. Art

경로:

```text
Assets/_Project/Art
```

3D 모델, 텍스처, 원본 아트 리소스를 넣는 폴더입니다.

중요한 구분:

```text
Art = 모델 원본 또는 시각 리소스
Prefabs = 게임에서 실제로 작동하는 완성 오브젝트
```

예를 들어 `Art/Zombies/Zombie_Normal.fbx`는 모델 원본이고, `Prefabs/Zombies/Zombie_Normal.prefab`은 AI, 체력, 공격 기능이 붙은 실제 좀비 오브젝트입니다.

---

### Art/Characters

경로:

```text
Assets/_Project/Art/Characters
```

플레이어, 생존자 NPC 같은 사람형 캐릭터 모델을 넣습니다.

예시:

```text
Player_DeliveryMan.fbx
Survivor_A.fbx
Survivor_B.fbx
```

규칙:

```text
플레이어 모델: Player_
생존자 모델: Survivor_
```

---

### Art/Zombies

경로:

```text
Assets/_Project/Art/Zombies
```

좀비 모델 원본을 넣습니다.

예시:

```text
Zombie_Normal.fbx
Zombie_Fast.fbx
Zombie_Big.fbx
```

규칙:

```text
좀비 모델은 Zombie_종류명 형태로 작성
```

---

### Art/Props

경로:

```text
Assets/_Project/Art/Props
```

맵에 배치되는 배경 오브젝트 모델을 넣습니다.

예시:

```text
TrashCan.fbx
BrokenCar.fbx
StreetLight.fbx
WoodBox.fbx
Barricade.fbx
DeliverySign.fbx
```

규칙:

```text
기능이 없는 장식용 오브젝트는 Art/Props
기능이 있는 오브젝트는 Prefabs에 따로 제작
```

예시:

```text
Art/Props/TrashCan.fbx
Prefabs/Items/Loot_TrashCan.prefab
```

---

### Art/Weapons

경로:

```text
Assets/_Project/Art/Weapons
```

무기 3D 모델 원본을 넣습니다.

예시:

```text
Weapon_PizzaCutter.fbx
Weapon_BaseballBat.fbx
Weapon_FryingPan.fbx
```

규칙:

```text
무기 모델은 Weapon_ 으로 시작
```

---

### Art/PizzaMachine

경로:

```text
Assets/_Project/Art/PizzaMachine
```

피자기계, 오븐, 기계 부품 관련 모델을 넣습니다.

예시:

```text
PizzaMachine.fbx
PizzaOven.fbx
MachinePart.fbx
BrokenMachinePart.fbx
```

규칙:

```text
피자기계 관련 모델은 PizzaMachine_ 또는 Machine_ 으로 시작
```

---

## 3-3. Audio

경로:

```text
Assets/_Project/Audio
```

효과음과 배경음악을 넣습니다.

예시:

```text
BGM_Base.wav
BGM_DangerZone.wav
SFX_PlayerHit.wav
SFX_ZombieAttack.wav
SFX_DeliverySuccess.wav
SFX_MachineBroken.wav
SFX_ItemPickup.wav
```

규칙:

```text
배경음악: BGM_
효과음: SFX_
UI 소리: UI_
```

---

## 3-4. Effects

경로:

```text
Assets/_Project/Effects
```

FX, 즉 시각 효과 파일을 넣습니다.

예시:

```text
FX_Hit.prefab
FX_ZombieDeath.prefab
FX_MachineSmoke.prefab
FX_ItemPickup.prefab
FX_DeliverySuccess.prefab
FX_RepairSpark.prefab
```

사용 예시:

```text
좀비 피격 효과
좀비 사망 효과
피자기계 고장 연기
기계 수리 스파크
아이템 획득 반짝임
배달 성공 효과
무기 공격 궤적
```

규칙:

```text
이펙트 이름은 FX_ 로 시작
```

---

## 3-5. Materials

경로:

```text
Assets/_Project/Materials
```

3D 모델에 입힐 Material 파일을 넣습니다.

예시:

```text
M_Player.mat
M_ZombieSkin.mat
M_PizzaMachine.mat
M_Blood.mat
M_Road.mat
M_Building.mat
```

규칙:

```text
Material은 M_ 로 시작
```

---

## 3-6. Prefabs

경로:

```text
Assets/_Project/Prefabs
```

게임에서 재사용할 완성 오브젝트를 넣는 폴더입니다.

핵심 구분:

```text
Art 폴더 = 모델 원본
Prefabs 폴더 = 게임 안에서 실제로 작동하는 완성품
```

예시:

```text
Art/Zombies/Zombie_Normal.fbx
Prefabs/Zombies/Zombie_Normal.prefab
```

`Zombie_Normal.prefab`에는 모델뿐 아니라 Collider, Rigidbody 또는 NavMeshAgent, ZombieAI, ZombieHealth, ZombieAttack 같은 컴포넌트가 붙습니다.

---

### Prefabs/Player

경로:

```text
Assets/_Project/Prefabs/Player
```

플레이어 관련 프리팹을 넣습니다.

예시:

```text
Player.prefab
PlayerWeaponHolder.prefab
PlayerCameraTarget.prefab
```

포함 요소:

```text
플레이어 모델
Character Controller 또는 Rigidbody
PlayerController.cs
PlayerHealth.cs
PlayerCombat.cs
카메라 타겟
무기 장착 위치
```

규칙:

```text
플레이어 최종 프리팹은 Player.prefab 하나로 관리
```

---

### Prefabs/Zombies

경로:

```text
Assets/_Project/Prefabs/Zombies
```

실제로 게임에 등장하는 좀비 프리팹을 넣습니다.

예시:

```text
Zombie_Normal.prefab
Zombie_Fast.prefab
ZombieSpawner.prefab
```

포함 요소:

```text
좀비 모델
Collider
Rigidbody 또는 NavMeshAgent
ZombieAI.cs
ZombieHealth.cs
ZombieAttack.cs
죽음 이펙트
드랍 아이템 설정
```

규칙:

```text
모델만 있는 파일은 Art/Zombies
AI까지 붙은 완성 좀비는 Prefabs/Zombies
```

---

### Prefabs/Items

경로:

```text
Assets/_Project/Prefabs/Items
```

아이템, 파밍 오브젝트, 드랍 아이템을 넣습니다.

예시:

```text
Item_Dough.prefab
Item_Cheese.prefab
Item_Sauce.prefab
Item_Scrap.prefab
Item_Wire.prefab
Item_MachinePart.prefab
Loot_Box.prefab
Loot_TrashCan.prefab
```

규칙:

```text
아이템: Item_
파밍 가능한 오브젝트: Loot_
```

---

### Prefabs/Weapons

경로:

```text
Assets/_Project/Prefabs/Weapons
```

실제로 장착 가능하고 공격 기능이 있는 무기 프리팹을 넣습니다.

예시:

```text
Weapon_PizzaCutter.prefab
Weapon_BaseballBat.prefab
Weapon_FryingPan.prefab
```

포함 요소:

```text
무기 모델
공격 범위 Collider
Weapon.cs
데미지 값
공격 속도
타격 FX
타격 사운드
```

규칙:

```text
무기 모델은 Art/Weapons
실제 공격 기능이 있는 무기는 Prefabs/Weapons
```

---

### Prefabs/Base

경로:

```text
Assets/_Project/Prefabs/Base
```

기지와 피자기계 관련 프리팹을 넣습니다.

예시:

```text
BaseController.prefab
PizzaMachine.prefab
UpgradeStation.prefab
RepairStation.prefab
DeliveryBoard.prefab
StorageBox.prefab
```

사용 예시:

```text
피자기계
기지 업그레이드 장치
수리대
배달 주문 게시판
보관함
```

---

## 3-7. Scenes

경로:

```text
Assets/_Project/Scenes
```

Unity 씬 파일을 넣습니다.

예시:

```text
TitleScene.unity
MainScene.unity
Test_Player.unity
Test_Zombie.unity
Test_Art.unity
Test_FX.unity
```

추천 씬 구성:

```text
TitleScene: 시작 화면
MainScene: 실제 게임 플레이
Test_Player: 플레이어 테스트용
Test_Zombie: 좀비 AI 테스트용
Test_Art: 팀원 아트 확인용
Test_FX: 이펙트 테스트용
```

중요 규칙:

```text
MainScene은 한 명만 수정
팀원은 Test_Art, Test_FX에서 작업
완성된 리소스는 Prefab으로 만들어 MainScene에 적용
```

---

## 3-8. Scripts

경로:

```text
Assets/_Project/Scripts
```

C# 코드 파일을 넣습니다.

---

### Scripts/Player

플레이어 관련 코드입니다.

예시:

```text
PlayerController.cs
PlayerHealth.cs
PlayerCombat.cs
PlayerInteraction.cs
PlayerInventory.cs
```

역할:

```text
이동
체력
공격
상호작용
인벤토리 연결
```

---

### Scripts/Zombie

좀비 관련 코드입니다.

예시:

```text
ZombieAI.cs
ZombieHealth.cs
ZombieAttack.cs
ZombieSpawner.cs
ZombieDropItem.cs
```

역할:

```text
좀비 추적
공격
사망
스폰
아이템 드랍
```

---

### Scripts/Combat

전투 공통 시스템 코드입니다.

예시:

```text
Damageable.cs
Hitbox.cs
Weapon.cs
AttackData.cs
DamageCalculator.cs
```

역할:

```text
데미지 처리
공격 판정
무기 데이터
피격 처리
```

---

### Scripts/Delivery

피자 배달 시스템 코드입니다.

예시:

```text
DeliveryManager.cs
DeliveryQuest.cs
DeliveryTarget.cs
DeliveryReward.cs
DeliveryTimer.cs
```

역할:

```text
주문 생성
목적지 설정
배달 시간 제한
배달 성공/실패
보상 지급
```

---

### Scripts/Farming

파밍 시스템 코드입니다.

예시:

```text
LootBox.cs
LootTable.cs
ItemDropper.cs
GatherableObject.cs
```

역할:

```text
상자 열기
쓰레기더미 조사
아이템 드랍
획득 확률 관리
```

---

### Scripts/Inventory

인벤토리 코드입니다.

예시:

```text
InventoryManager.cs
InventorySlot.cs
Item.cs
ItemType.cs
ItemPickup.cs
```

역할:

```text
아이템 보유 수량 관리
아이템 획득
아이템 사용
UI에 수량 표시
```

---

### Scripts/Crafting

제작 시스템 코드입니다.

예시:

```text
PizzaCraftingManager.cs
PizzaRecipe.cs
CraftingStation.cs
PizzaMachine.cs
```

역할:

```text
피자 제작
재료 소모
레시피 확인
피자기계 작동
```

---

### Scripts/Upgrade

업그레이드 코드입니다.

예시:

```text
UpgradeManager.cs
WeaponUpgrade.cs
BaseUpgrade.cs
PizzaMachineUpgrade.cs
UpgradeCost.cs
```

역할:

```text
무기 레벨업
기지 강화
피자기계 내구도 증가
업그레이드 비용 계산
```

---

### Scripts/UI

UI 관련 코드입니다.

예시:

```text
HUDController.cs
HealthBarUI.cs
InventoryUI.cs
DeliveryUI.cs
UpgradeUI.cs
GameOverUI.cs
```

역할:

```text
체력바 표시
배달 제한 시간 표시
아이템 수량 표시
업그레이드 창 표시
게임오버 화면 표시
```

---

### Scripts/Save

저장/불러오기 코드입니다.

예시:

```text
SaveManager.cs
SaveData.cs
JsonSaveSystem.cs
```

역할:

```text
게임 저장
게임 불러오기
로컬 JSON 저장
Steam Cloud 연동 준비
```

---

## 3-9. ScriptableObjects

경로:

```text
Assets/_Project/ScriptableObjects
```

게임 데이터를 에셋으로 관리하는 폴더입니다.

사용 예시:

```text
아이템 데이터
무기 데이터
피자 레시피 데이터
좀비 데이터
배달 의뢰 데이터
업그레이드 데이터
루팅 테이블 데이터
```

예시 파일:

```text
SO_Item_Dough.asset
SO_Item_Cheese.asset
SO_Weapon_PizzaCutter.asset
SO_Zombie_Normal.asset
SO_PizzaRecipe_Basic.asset
SO_Upgrade_WeaponLevel1.asset
SO_LootTable_TrashCan.asset
```

규칙:

```text
ScriptableObject 파일은 SO_ 로 시작
```

---

## 3-10. UI

경로:

```text
Assets/_Project/UI
```

UI 이미지, 아이콘, 폰트, UI 프리팹을 넣습니다.

예시:

```text
Icon_Dough.png
Icon_Cheese.png
Icon_Scrap.png
Icon_Health.png
UI_HealthBar.prefab
UI_InventoryPanel.prefab
UI_DeliveryTimer.prefab
```

규칙:

```text
아이콘: Icon_
UI 프리팹: UI_
```

---

# 4. 네이밍 규칙 정리

| 종류 | 규칙 | 예시 |
|---|---|---|
| 스크립트 | PascalCase | `PlayerController.cs` |
| 프리팹 | 기능_이름 | `Zombie_Normal.prefab` |
| 모델 | 종류_이름 | `Weapon_PizzaCutter.fbx` |
| 머티리얼 | `M_` | `M_ZombieSkin.mat` |
| 이펙트 | `FX_` | `FX_Hit.prefab` |
| 아이콘 | `Icon_` | `Icon_Dough.png` |
| ScriptableObject | `SO_` | `SO_Item_Dough.asset` |
| 씬 | 역할명 | `MainScene.unity` |
| 배경음악 | `BGM_` | `BGM_Base.wav` |
| 효과음 | `SFX_` | `SFX_ZombieAttack.wav` |

---

# 5. 협업 규칙

## 본인 담당

```text
Scripts 폴더 중심으로 작업
MainScene 관리
게임 시스템 개발
프리팹에 기능 연결
ScriptableObjects로 데이터 관리
팀원이 만든 Art/FX를 게임 기능과 연결
```

## 팀원 담당

```text
Art 폴더에 모델 정리
Effects 폴더에 FX 정리
Materials 폴더에 재질 정리
Test_Art, Test_FX 씬에서 테스트
완성된 리소스는 Prefab으로 만들어 전달
MainScene 직접 수정은 최소화
```

## 씬 관리 규칙

```text
MainScene은 동시에 수정하지 않기
팀원은 Test_Art, Test_FX에서 작업하기
완성된 오브젝트는 Prefab으로 전달하기
최종 적용은 MainScene 담당자가 진행하기
```

## Git 작업 규칙

```text
작업 전 항상 pull 받기
기능 단위로 commit 하기
커밋 메시지는 구체적으로 작성하기
큰 리소스 파일은 Git LFS로 관리하기
Library, Temp, Build 폴더는 Git에 올리지 않기
```

좋은 커밋 메시지 예시:

```text
Add player movement system
Add zombie chase AI
Create pizza delivery manager
Add loot box prefab
Add machine smoke effect
```

나쁜 커밋 메시지 예시:

```text
수정
작업
최종
진짜최종
버그
```

---

# 6. Git에 올릴 폴더와 올리지 않을 폴더

## Git에 올릴 것

```text
Assets/
Packages/
ProjectSettings/
```

## Git에 올리지 않을 것

```text
Library/
Temp/
Obj/
Build/
Builds/
Logs/
UserSettings/
.vs/
```

---

# 7. 핵심 요약

```text
Art는 원본 리소스
Prefabs는 실제 게임 오브젝트
Scripts는 기능 코드
Scenes는 게임 화면
ScriptableObjects는 게임 데이터
UI는 화면 표시 리소스
Effects는 시각 효과
Audio는 사운드
Materials는 3D 재질
```

이 규칙을 지키면 파일이 섞이지 않고, 2인 협업 중 Git 충돌을 줄일 수 있습니다.
