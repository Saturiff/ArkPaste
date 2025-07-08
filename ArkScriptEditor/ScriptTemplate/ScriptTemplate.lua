--[[
    腳本設定:
    按照需求更改值
    Description 腳本描述
    Hide        是否隱藏 (true: 隱藏 / false: 顯示)
    GlobalDelay 每個動作之間的間隔，單位毫秒 (1秒 = 1000毫秒)
]]--

---@type string 腳本描述
Description = "替換成你的描述"

---@type boolean 是否隱藏
Hide = false

---@type integer 動作全局間隔
GlobalDelay = 50

-- 請勿修改
Data = {}

--[[
    腳本本體:
    按照需要的動作撰寫
]]--

function Script()



end

--[[
    指令定義:
]]--

---@param time integer 單位毫秒 (1秒 = 1000毫秒)
-- 等待 x 毫秒
-- 範例: Wait(500)
function Wait(time)
    table.insert(Data, {
        type = "Wait",
        time = time,
    })
end

---@param x integer 游標位置X
---@param y integer 游標位置Y
-- 設定游標位置至 x, y
-- 範例: SetCursorPos(800, 600)
function SetCursorPos(x, y)
    table.insert(Data, {
        type = "SetCursorPos",
        position = {x, y},
    })
end

---@param x integer 游標位置X
---@param y integer 游標位置Y
---@param color string 顏色代號，共六碼
-- 等待螢幕位置 x, y 為指定顏色，成功匹配後繼續執行
-- 範例: WaitColor(800, 600, "ffffff")
function WaitColor(x, y, color)
    table.insert(Data, {
        type = "WaitColor",
        position = {x, y},
        color = color,
    })
end

-- 按下左鍵
-- 範例: LMBClick()
function LMBClick()
    table.insert(Data, {
        type = "LMBClick",
    })
end

---@param keys string 欲輸入的英文按鍵或數字按鍵組合
-- 按下按鍵或輸入字串，支援英數[A-Z][0-9]，注意輸入法
-- 範例: PressKey("F")
-- 範例: PressKey("ADV")
function PressKey(keys)
    table.insert(Data, {
        type = "PressKey",
        keys = keys,
    })
end

---@param times integer 重複的次數
---@param actions table 重複的內容
-- 按下按鍵或輸入字串，支援英數[A-Z][0-9]，注意輸入法
-- 範例:
-- Repeat(10, {
--     PressKey("A"),
--     Wait(500),
-- })
-- 範例: PressKey("ADV")
function Repeat(times, actions)
    table.insert(Data, {
        type = "Repeat",
        times = times,
        actions = actions,
    })
end
