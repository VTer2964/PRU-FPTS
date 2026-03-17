# FPTSim - Project Rules

## My Role
Tôi (user) chịu trách nhiệm **tạo minigame** cho project FPTSim.

---

## Minigame Creation Rules

### 1. Cấu trúc bắt buộc
Mỗi minigame PHẢI:
- Tạo một class kế thừa `MinigameBase` (namespace `FPTSim.Minigames`)
- Đặt script tại `Assets/Scripts/MiniGames/`
- Có một scene riêng, đặt tại `Assets/Scenes/` với prefix `Minigame_` (ví dụ: `Minigame_FCode`)
- Scene phải được thêm vào **Build Settings**

### 2. Template tối thiểu
```csharp
using UnityEngine;
using FPTSim.Core;

namespace FPTSim.Minigames
{
    public class XxxMinigame : MinigameBase
    {
        protected override void Start()
        {
            minigameId = "Xxx";   // ID duy nhất
            timeLimit   = 30f;    // giây
            base.Start();
        }

        protected override void Update()
        {
            // logic gameplay
            base.Update(); // BẮT BUỘC gọi để đếm timer
        }

        protected override void OnTimeUp()
        {
            FinishWithResult(); // tính medal khi hết giờ
        }

        private void FinishWithResult()
        {
            Medal medal = CalculateMedal(/* score */);
            Finish(new MinigameResult
            {
                minigameId  = minigameId,
                medal       = medal,
                scoreAwarded = 0,
                success      = medal != Medal.None
            });
        }

        private Medal CalculateMedal(int score)
        {
            if (score >= goldTarget)   return Medal.Gold;
            if (score >= silverTarget) return Medal.Silver;
            if (score >= bronzeTarget) return Medal.Bronze;
            return Medal.None;
        }
    }
}
```

### 3. Medal thresholds
- Mỗi minigame tự định nghĩa `goldTarget`, `silverTarget`, `bronzeTarget`
- Gợi ý: Bronze = dễ đạt, Gold = thử thách

### 4. Kết thúc minigame
- Luôn gọi `Finish(result)` từ `MinigameBase` — KHÔNG tự load scene
- `Finish()` sẽ tự `RegisterMinigameResult` và load lại `Campus`

### 5. Kết nối với NPC/Dialogue
- Tạo `DialogueNodeSO` với `triggerAction = true`, `actionType = StartMinigameScene`
- `actionParam` = tên scene (ví dụ: `Minigame_FCode`)
- Gán vào graph của NPC tương ứng

### 6. Danh sách minigame đã có
| ID | Scene | Adapter GO | Scene path |
|----|-------|-----------|------------|
| SpaceNhanh | MG_SpaceSpam | - | Assets/Scenes/MG_SpaceSpam.unity |
| Caro | Minigame_Caro | `[CaroAdapter]` | Assets/Scenes/Minigame_Caro.unity |
| CountryGuess | Minigame_CountryGuess | `CountryGameManager` | Assets/Scenes/Minigame_CountryGuess.unity |
| Memory | Minigame_Memory | `[MemoryAdapter]` | Assets/Scenes/Minigame_Memory.unity |
| StackTower | Minigame_StackTower | `StackTowerGameManager` | Assets/Scenes/Minigame_StackTower.unity |
| FlappyVocab | Minigame_FlappyVocab | `[FlappyVocabAdapter]` | Assets/Scenes/Minigame_FlappyVocab.unity |

### 7. Pattern tích hợp game có sẵn (Adapter)
Khi game đã có scene/logic riêng, dùng pattern adapter:
- **Scene gốc** nằm tại `Assets/MiniGames/XxxGame/Scenes/`
- **Copy** sang `Assets/Scenes/Minigame_Xxx.unity` (KHÔNG dùng scene gốc trực tiếp)
- **Tạo GO riêng** tên `[XxxAdapter]` — KHÔNG gắn adapter vào GameManager của game gốc
- Adapter dùng `timeLimit = 99999f` để vô hiệu hóa timer của MinigameBase
- Adapter subscribe event của game (`OnGameEnded`, `OnVictory`...) → map sang `Medal` → `StartCoroutine(FinishAfterDelay(medal, 3f))`
- Assign reference manager vào field serialized (không chỉ dùng FindFirstObjectByType)

### 8. Tạo scene hàng loạt
- Script setup nằm tại `Assets/Editor/MinigameSetup.cs` — menu **FPTSim/Setup ALL Minigames**
- Script fix adapter nằm tại `Assets/Editor/MinigameAdapterFix.cs` — menu **FPTSim/Fix Adapter Placement**
- Khi invoke menu item qua MCP có thể timeout do Unity load scene → chạy thủ công trong Unity Editor

---

## Kết nối NPC/TriggerZone với Minigame

### DialogueTriggerZone (walk-in, dùng để test)
1. Tạo `DialogueNodeSO`: `triggerAction=true`, `actionType=StartMinigameScene`, `actionParam=Minigame_Xxx`
2. Tạo `DialogueGraphSO`: `entryNode` = node trên
3. Tạo empty GO trong Campus, add `BoxCollider` (isTrigger=true) + `DialogueTriggerZone`
4. Assign `runner` (DialogueRunner trong Campus) + `graph`
5. `completedFlag` = **để trống** khi test (có thể trigger nhiều lần)

### NPCDialogueInteractable_Graph (press E, dùng cho game thật)
- Thêm component `NPCDialogueInteractable_Graph` vào NPC GO
- Assign `npcName`, `dialogueGraph`, `runner`

---

## Quy ước đặt tên
- Script: `[TênMinigame]Minigame.cs` — ví dụ: `FCodeMinigame.cs`
- Scene: `Minigame_[TênMinigame]` — ví dụ: `Minigame_FCode`
- minigameId: PascalCase ngắn gọn — ví dụ: `"FCode"`
- Adapter GO: `[XxxAdapter]` — ví dụ: `[CaroAdapter]`
- Dialogue assets: `Assets/Dialogue/Minigames/Node_StartXxx.asset`, `Graph_StartXxx.asset`

---

## Không cần làm (đã có sẵn)
- GameManager, SaveSystem, Medal system → KHÔNG sửa
- DialogueRunner, DialogueUI → KHÔNG sửa
- HUDController → KHÔNG sửa
- Player controller → KHÔNG sửa
