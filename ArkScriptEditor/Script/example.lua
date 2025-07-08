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

---@type integer 動作全局間隔，最低20
GlobalDelay = 50

--[[
    腳本本體:
    按照需要的動作撰寫
]]--
function Run()
    H.Data = {}
    -- 在這個方法內新增你的腳本動作


    H:Wait(10)
    H:Wait(100)
    H:Wait(1000)


    H:Repeat(10, {
        H:PressKey("A"),
        H:Wait(1000),
    })
    return H.Data
end
