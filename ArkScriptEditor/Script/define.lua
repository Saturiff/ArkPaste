M = {}

M.Data = {}

Hide = true

--[[
    指令定義:
]]--

---@param time integer 單位毫秒 (1秒 = 1000毫秒)
-- 暫停腳本，等待 x 毫秒後才繼續執行
-- 範例: 等待 0.5 秒
--   H:Wait(500)
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
-- 範例: 等待螢幕位置 x=800, y=600 的位置出現顏色相似於 r=255, g=128, b=0 的顏色才繼續執行
--   H:WaitColor(800, 600, 255, 128, 0)
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
-- 範例: 設定滑鼠位置到 x=800, y=600
--   H:SetCursorPos(800, 600)
function M:SetCursorPos(x, y)
    table.insert(self.data, {
        type = "SetCursorPos",
        x = x,
        y = y,
    })
end

-- 按下左鍵
-- 範例:
--   H:LMBClick()
function M:LMBClick()
    table.insert(self.data, {
        type = "LMBClick",
    })
end

---@param keys string 欲輸入的英文按鍵或數字按鍵組合
-- 按下按鍵或輸入字串，支援英數[A-Z][0-9]，注意輸入法
-- 範例: 按下 F 開啟背包或全部拾取
--   H:PressKey("F")
-- 範例: 搜尋高級步槍子彈的片段
--   H:PressKey("ADV")
function M:PressKey(keys)
    table.insert(self.data, {
        type = "PressKey",
        keys = keys,
    })
end

---@param count integer 重複的次數
---@param actions table 重複的內容
-- 將範圍內的指令重複執行指定次數
-- 範例: 按下 A 後等待 50 秒 (支援整數或基礎運算)，重複 10 次
--   H:Repeat(10, function()
--       H:PressKey("A")
--       H:Wait(1000 * 50)
--   end)
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
