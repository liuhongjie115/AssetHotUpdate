﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class Assets_Scripts_Util_Single_Assets_Scripts_Core_LuaEngineWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(Assets.Scripts.Util.Single<Assets.Scripts.Core.LuaEngine>), typeof(System.Object), "Single_Assets_Scripts_Core_LuaEngine");
		L.RegFunction("DestroyInstance", DestroyInstance);
		L.RegFunction("New", _CreateAssets_Scripts_Util_Single_Assets_Scripts_Core_LuaEngine);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("Instance", get_Instance, null);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateAssets_Scripts_Util_Single_Assets_Scripts_Core_LuaEngine(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 0)
			{
				Assets.Scripts.Util.Single<Assets.Scripts.Core.LuaEngine> obj = new Assets.Scripts.Util.Single<Assets.Scripts.Core.LuaEngine>();
				ToLua.PushObject(L, obj);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to ctor method: Assets.Scripts.Util.Single<Assets.Scripts.Core.LuaEngine>.New");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int DestroyInstance(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 0);
			Assets.Scripts.Util.Single<Assets.Scripts.Core.LuaEngine>.DestroyInstance();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_Instance(IntPtr L)
	{
		try
		{
			ToLua.PushObject(L, Assets.Scripts.Util.Single<Assets.Scripts.Core.LuaEngine>.Instance);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}
