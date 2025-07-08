M = {}

M.Data = {}

Hide = true

--[[
    指令定義:
]]--

---@param time integer 單位毫秒 (1秒 = 1000毫秒)
-- 暫停腳本，等待 x 毫秒後才繼續執行
-- 範例: Wait(500)
function M:Wait(time)
    table.insert(M.Data, {
        type = "Wait",
        time = time,
    })
end

---@param x integer 游標位置X
---@param y integer 游標位置Y
---@param color string 顏色代號，共六碼
-- 暫停腳本，等待螢幕位置 x, y 為指定顏色後才繼續執行
-- 範例: WaitColor(800, 600, "ffffff")
function M:WaitColor(x, y, color)
    table.insert(M.Data, {
        type = "WaitColor",
        position = {x, y},
        color = color,
    })
end

---@param x integer 游標位置X
---@param y integer 游標位置Y
-- 設定游標位置至 x, y
-- 範例: SetCursorPos(800, 600)
function M:SetCursorPos(x, y)
    table.insert(M.Data, {
        type = "SetCursorPos",
        position = {x, y},
    })
end

-- 按下左鍵
-- 範例: LMBClick()
function M:LMBClick()
    table.insert(M.Data, {
        type = "LMBClick",
    })
end

---@param keys string 欲輸入的英文按鍵或數字按鍵組合
-- 按下按鍵或輸入字串，支援英數[A-Z][0-9]，注意輸入法
-- 範例: PressKey("F")
-- 範例: PressKey("ADV")
function M:PressKey(keys)
    table.insert(M.Data, {
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
function M:Repeat(times, actions)
    table.insert(M.Data, {
        type = "Repeat",
        times = times,
        actions = actions,
    })
end

return M
