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
    table.insert(self.data, {
        type = "Wait",
        time = time,
    })
end

---@param x integer 游標位置X
---@param y integer 游標位置Y
---@param r integer 紅色值
---@param g integer 藍色值
---@param b integer 綠色值
-- 暫停腳本，等待螢幕位置 x, y 為指定顏色後才繼續執行，內建容差值
-- 範例: WaitColor(800, 600, 255, 128, 0)
function M:WaitColor(x, y, r, g, b)
    table.insert(self.data, {
        type = "WaitColor",
        x = x,
        y = y,
        r = r,
        g = g,
        b = b,
    })
end

---@param x integer 游標位置X
---@param y integer 游標位置Y
-- 設定游標位置至 x, y
-- 範例: SetCursorPos(800, 600)
function M:SetCursorPos(x, y)
    table.insert(self.data, {
        type = "SetCursorPos",
        x = x,
        y = y,
    })
end

-- 按下左鍵
-- 範例: LMBClick()
function M:LMBClick()
    table.insert(self.data, {
        type = "LMBClick",
    })
end

---@param keys string 欲輸入的英文按鍵或數字按鍵組合
-- 按下按鍵或輸入字串，支援英數[A-Z][0-9]，注意輸入法
-- 範例: PressKey("F")
-- 範例: PressKey("ADV")
function M:PressKey(keys)
    table.insert(self.data, {
        type = "PressKey",
        keys = keys,
    })
end

---@param count integer 重複的次數
---@param actions table 重複的內容
-- 按下按鍵或輸入字串，支援英數[A-Z][0-9]，注意輸入法
-- 範例:
-- Repeat(10, {
--     PressKey("A"),
--     Wait(500),
-- })
function M:Repeat(count, func)
    -- 讓新容器承接repeat的內容
    local actions = {}
    local oldData = self.data
    self.data = actions
    func()
    self.data = oldData
    table.insert(self.data, {
        type = "Repeat",
        count = count,
        actions = actions
    })
end

return M
