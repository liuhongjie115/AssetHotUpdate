using Asset.Scripts.Util;
using Assets.Scripts.Core.Manager;
using Assets.Scripts.Util;
using LuaInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public class LuaEngine:Single<LuaEngine>
    {
        Action _luaComplete;
        LuaState _luaState;

        private LuaFunction getTracebackFunc;
        private LuaFunction _loopFunc;

        public ObjectTranslator luaTranslator;
        private MonoBehaviour _mono;
        private string _luaFileRootDir;

        public void InitEngine(MonoBehaviour mono, Action luacompCallback = null)
        {
            _luaState = new LuaState();
            _luaState.LuaSetTop(0);
            luaTranslator = _luaState.translator;
            _mono = mono;
            _luaFileRootDir = SystemConfig.localRes + "Lua";
            SystemConfig.PACK_IN_RES_LUA_PATH = _luaFileRootDir;
            Start(luacompCallback);

        }

        public void Start(Action callBack = null)
        {
            Debugger.useLog = false;
            _luaComplete = callBack;
            ALog.Info("Start luaState");
            _luaState.Start();
            _luaState.LuaSetTop(0);
            ALog.Info("Start Lua bind");
            Bind();
            ALog.Info("Start Lua main");
            StartMain();
            LuaLoadComplete();
        }

        private void Bind()
        {
            LuaBinder.Bind(_luaState);
            LuaCoroutine.Register(_luaState, _mono);
            DelegateFactory.Init();
        }

        protected void StartMain()
        {
            _luaState.DoFile("Main");
            getTracebackFunc = _luaState.GetFunction("Main.getTraceback");
            ALog.SetLuaStackInfoCB(GetLuaTraceback);
            LuaFunction main = _luaState.GetFunction("Main.main");
            main.Call();
            main.Dispose();
            _loopFunc = _luaState.GetFunction("Main.loop");
        }

        public string GetLuaTraceback()
        {
            return getTracebackFunc.Invoke<string>();
        }

        public void StartLoop()
        {
            TimerManager.AddTimerFrame("luaLoop", LuaLoop);
        }

        private void LuaLoop()
        {
            _loopFunc.Call();
        }

        public void LuaLoadComplete()
        {
            StartLoop();
            if (_luaComplete != null)
            {
                _luaComplete();
            }
        }
    }
}

