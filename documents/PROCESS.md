# PROCESS.md — 我的練習心得

> 一個原則：**寫「具體發生的事」，不寫感想文。**
> 貼上當時真實的 prompt、真實的數字、真實的錯誤訊息——三個月後的你（和你的同事）才用得上。

#### 使用的 agent 與模型：

- Codex / GPT-5

---

## 通用四問

### 1. 我的任務拆解

（開工前你把任務拆成哪幾步？實際做的時候順序有變嗎？為什麼變？）

- 先看 project 怎樣分 layer，然後先了解 code flow, 先讓 agent 幫我分成幾個 stage 去執行。
- 原本以為先弄 Codex config 就好，後來覺得先看 code 比較容易，至少知道 agent 講的對不對。


### 2. AI 幫上大忙的地方

（哪件事 agent 做得又快又好？**貼上當時的提問原文**，說明為什麼這樣問有效。）

- 這次我只是跟 agent 說：`帮我看下这个项目，你先理解一下，將你瞭解到的 分成一個一個階段。`
- 它很快就帮我抓到重点 file，还有哪里要看。对刚接手一个 project 的时候蛮有用，如果跟著普通的瞭解 project 方法，會讀很久。


### 3. AI 誤導我的地方，與我如何發現

（agent 說錯／改錯／過度自信的時刻。你靠什麼抓到——對照程式碼？頁面實測？跑測試？）

- Agent 的 summary 不可以照单全收。比如 Gold discount，它讲得像是正常，但我自己 follow code 才看到 price 被 discount 两次，100 最后变 81，不是 90。
- 所以之后还是要自己 check code 和 test，agent 可以帮我快一点找方向，但不能代替我 confirm。
- 練習 3 遇到另一種誤導：`dotnet build` 一直報 CS0579 重複 attribute，我先以為是 repo obj 目錄 ACL 唯讀造成（`icacls` 確認 `BUILTIN\Users` 只有 RX），後來才發現真正原因是各專案目錄下多了一個沒有被 git 追蹤、之前留下的壞掉 `$buildTemp\obj\$(MSBuildProjectName)\` 資料夾（推測是先前有人跑 `-p:BaseIntermediateOutputPath` 之類指令時沒跳脫變數，字面上建出這個資料夾），把它清掉、並改用 `-p:IntermediateOutputPath` / `-p:OutputPath` 導到可寫路徑之後才真正 build 成功。單看第一個錯誤訊息很容易誤判成別的問題，要對照 `icacls` 和實際檔案才抓到根因。


### 4. 我會帶回日常工作的一招

（一個具體、可複製的做法，不要寫「要多驗證」這種口號——寫出**操作步驟**。）

- 我觉得最实用的一招是：先看症状，再 trace flow，最后才改。
- 不要看到 error 就直接改。先看 data 在哪里变掉、现有 test 有没有 cover，confirm 了才開始改。


## 自我驗證（做到哪個階段答哪題）

### 第一階段 — Agentic Coding

練習 1

1. 我能不看筆記說出三個專案（Web/Core/Infrastructure）各自的職責
   - 已經大概可以分到 Web、Core、Infrastructure 各自做什么。
2. 我核對過 agent 描述的建單流程，且**至少找出一處不精確或過度簡化的說法**
   - 也有找到一个 Gold discount 被算两次的问题, agent 也有給 summary 但是不是很清楚。
3. 我知道商業邏輯應該放在哪一層、新增頁面要動哪些地方
   - 之后加功能会先跟著现有 pattern，不会直接在 Controller 里面塞 business logic。

練習 2

1. 三個客訴都先在頁面上重現過，才開始找程式
   - 客訴 1：在 `/Orders` 新增兩筆訂單後，第一頁找不到；共 202 筆、每頁 20 筆時，第 11 / 11 頁是空白。
   - 客訴 2 也已重現：商品 SKU-1001 原價 NT$1,420，Gold 訂單 #204 與 Silver 訂單 #205 使用同一商品各 1 件。
   - 客訴 3：取消訂單 #206（SKU-1002 × 1）後，商品頁庫存沒有加回；當時頁面顯示庫存 101。
2. 我給 agent 的資訊包含具體觀察（頁碼／金額數字／庫存數字），而不是只貼客訴原文
   - 我提供的觀察是：「我新加了 2 個訂單，的確都找不到；最後一頁必定空白。」之後確認頁面顯示共 202 筆、第 1 / 11 頁，最後一頁 0 筆。
   - 客訴 2 的實際數字是：#204 Gold 單價快照 NT$1,278、小計 NT$1,278、應付 NT$1,150.20；#205 Silver 單價快照 NT$1,420、應付 NT$1,349。
   - 客訴 3 的實際觀察是：#206 狀態為「已取消」、商品為 SKU-1002、數量 1，產品頁目前庫存為 101。
3. 每個修復都回到頁面驗證過症狀消失
   - 修正後，第 1 頁最前面的訂單編號是 202、201；第 11 / 11 頁有 2 筆訂單，不再空白。
   - 客訴 2 的回歸測試已確認原價快照與只折扣一次；頁面最終仍需重新建立 Gold 訂單確認單價快照 NT$1,420、折扣 NT$142、應付 NT$1,278。既有 #204 不會自動改寫。
   - 客訴 3 修正後需重新建立一筆商品訂單，確認庫存依序為「建立前 N、建立後 N-1、取消後 N」。既有 #206 不會自動補回庫存。
4. 每個 bug 都補了一個回歸測試，`dotnet test` 全綠
   - 客訴 1 補了第 1 頁應回傳最新 20 筆、最後一頁應回傳剩餘 5 筆的測試；`dotnet test OrderHub.sln --no-restore -m:1` 結果為 29 passed、0 failed。
   - 客訴 2 補 `CreateOrder_GoldSnapshotsListPriceAndAppliesDiscountOnce`：驗證快照為 1000、總額為 900；完整測試結果為 30 passed、0 failed。
   - 客訴 3 補 `CancelOrder_RestoresProductStock`：驗證庫存由 10 減為 9，再恢復為 10；完整測試結果為 31 passed、0 failed。
5. 三個獨立 commit，message 說明症狀與根因
   - 客訴 1、2、3 已各自建立獨立 commit（`7f5f77e`、`64c475a`、`6ab9e7d`）。
6. （思考題）為什麼原本的測試沒抓到這三個 bug？
   - 客訴 1 的原有測試只驗證 `TotalCount` 和 `TotalPages`，沒有驗證第 1 頁與最後一頁實際回傳的訂單；因此錯誤的 `Skip(page * pageSize)` 仍可通過測試。
   - 客訴 2 原有測試分開驗證折扣率、快照原價與 `CalculateTotal`，沒有把 Gold 建單、快照與總額串在同一個流程驗證，所以重複折扣未被抓到。
   - 客訴 3 原有測試只驗證訂單狀態變成 `Cancelled`，沒有驗證商品庫存，因此狀態測試會通過但庫存錯誤未被發現。

練習 3

我先開 `/Products/LowStock` 看不帶參數的結果，頁面使用門檻 10，顯示 5 筆商品，庫存是 2、2、3、4、4，順序由低到高。之後改了幾個 threshold：`2` 時 0 筆、`3` 時 2 筆、`5` 時 5 筆、`20` 時 7 筆。這也確認了條件是庫存小於門檻，剛好等於門檻的商品不會出現。

輸入 `threshold=0` 和 `threshold=-1` 時，頁面仍然回傳 200，輸入框下面顯示「門檻必須大於 0」，沒有進到 500 錯誤頁。

售出數量的測試放了三筆資料：30 天內的 Confirmed 訂單數量 5、30 天內的 Cancelled 訂單數量 3，以及 31 天前的 Confirmed 訂單數量 2。最後只計算 5，表示取消訂單和超過 30 天的訂單都有排除。另放了一筆低庫存但 `IsActive = false` 的商品，查詢結果也沒有列出它。

這次的分層沿用原本 Products 功能：Controller 負責接收 threshold、檢查 ModelState 和組 ViewModel；查詢與售出數量統計放在 repository；View 使用自己的 ViewModel。我看過 diff，沒有把查詢或庫存判斷塞進 Controller。

補上的三個測試是 `GetLowStock_FiltersByThresholdAndOrdersByStockAscending`、`GetLowStock_ExcludesInactiveProducts` 和 `GetLowStock_SoldLast30Days_ExcludesCancelledAndOutOfWindowOrders`。目前本機執行 `dotnet test` 會被 `obj` 目錄的寫入權限擋住，所以不能把完整測試結果寫成已確認；之前在可寫入的乾淨副本執行過的結果，和目前 repository 的狀態要分開記錄。

練習 4

這次只整理 `OrderService.CreateOrderAsync` 裡的驗證流程，沒有改訂單建立的規則。原本客戶、明細、數量、重複商品和商品庫存的檢查全部堆在同一個方法裡；重構後把輸入本身的檢查抽成小方法，商品逐筆檢查和建立訂單明細仍留在原本的流程中。

我重新看過 diff，確認折扣、庫存扣除、錯誤訊息和 `ServiceResult` 的行為沒有一起被改掉。測試目前仍受到 `obj` 目錄 ACL 影響，能在可寫入的測試副本重新執行後，才補上完整的通過數字。

---

## 附錄：值得留下的對話片段

（貼 1–2 段最有代表性的 prompt 與回應**摘要**——不用貼全文，重點是「我怎麼問」和「它怎麼答」。）
