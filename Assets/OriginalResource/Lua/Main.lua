package.cpath = package.cpath .. ';C:/Users/alpha/AppData/Roaming/JetBrains/IdeaIC2020.1/plugins/intellij-emmylua/classes/debugger/emmy/windows/x64/?.dll'
local dbg = require('emmy_core')
dbg.tcpConnect('localhost', 9966)

--主入口函数。从这里开始lua逻辑
---@module Main
module("Main",package.seeall);

function preLoad()
	require("Core.LuaDefine")
end

function main()
	preLoad();
	ALog.Info("进入Lua main");
	AssetLoader.LoadNormal("UI/Hero/Prefabs/HeroCube",function (res)
		local obj = res:GetInstance();
		obj.transform.position = Vector3(0,0,0);
	end)
end

function getTraceback()
	return debug.traceback();
end

function loop()
	local obj
	AssetLoader.LoadNormal("UI/Hero/Prefabs/HeroCube",function (res)
		local obj = res:GetInstance();
		obj.transform.position = Vector3(0,0,0);
	end)
	ALog.Info(obj);
end

function OnApplicationQuit()
end