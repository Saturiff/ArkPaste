H = require("Script.define")
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

---@type integer 動作全局間隔，最少 1 毫秒
-- 16 毫秒接近 60 FPS 的表現(1/60)，32 毫秒接近 30 FPS 的表現(1/30)
GlobalDelay = 16

--[[
    腳本本體:
    按照需要的動作撰寫
]]--
function Run()
    H.data = {}
    -- 在這個方法內新增你的腳本動作

    -- 等待 500 毫秒 (0.5秒)
    H:Wait(500)

    -- 持續等待，直到座標(800, 600)出現顏色(255, 128, 0)
    H:WaitColor(800, 600, 255, 128, 0)

    -- 滑鼠移動到座標(800, 600)
    H:SetCursorPos(800, 600)

    -- 按下滑鼠左鍵
    H:LMBClick()

    -- 按下按鍵 F
    H:PressKey("F")

    -- 連續按下按鍵 A, D, V
    H:PressKey("ADV")

    -- 內部動作重複執行 10 次
    H:Repeat(10, function()
        -- 按下按鍵 A
        H:PressKey("A")

        -- 等待 500 毫秒 (0.5秒)
        H:Wait(500)
    end)

    return H.data
end
