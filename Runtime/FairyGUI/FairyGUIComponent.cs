﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FairyGUI;
using GameFrameX.Runtime;
using UnityEngine;

namespace GameFrameX.FairyGUI.Runtime
{
    /// <summary>
    /// 管理所有顶层UI, 顶层UI都是GRoot的孩子
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/FairyGUI")]
    [RequireComponent(typeof(FairyGUIPackageComponent))]
    public sealed class FairyGUIComponent : GameFrameworkComponent
    {
        private FUI _root;
        FUI HiddenRoot;
        FUI FloorRoot;
        FUI NormalRoot;
        FUI FixedRoot;
        FUI WindowRoot;
        FUI TipRoot;
        FUI BlackBoardRoot;
        FUI DialogueRoot;
        FUI GuideRoot;
        FUI LoadingRoot;
        FUI NotifyRoot;

        FUI SystemRoot;

        // public FUI UIRoot;


        private readonly Dictionary<UILayer, Dictionary<string, FUI>> _dictionary = new Dictionary<UILayer, Dictionary<string, FUI>>(16);
        private readonly Dictionary<string, FUI> _uiDictionary = new Dictionary<string, FUI>(64);

        private FairyGUIPackageComponent _packageComponent;

        private void Start()
        {
            _packageComponent = GetComponent<FairyGUIPackageComponent>();
            GameFrameworkGuard.NotNull(_packageComponent, nameof(_packageComponent));
        }

        public void OnDestroy()
        {
            _root.Dispose();
            _root = null;
        }

        /// <summary>
        /// 添加全屏UI对象
        /// </summary>
        /// <param name="creator">UI创建器</param>
        /// <param name="descFilePath">UI目录</param>
        /// <param name="layer">目标层级</param>
        /// <param name="userData">用户自定义数据</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>返回创建后的UI对象</returns>
        public T AddToFullScreen<T>(System.Func<object, T> creator, string descFilePath, UILayer layer, object userData = null) where T : FUI
        {
            return Add(creator, descFilePath, layer, true, userData);
        }

        /// <summary>
        /// 异步添加UI 对象
        /// </summary>
        /// <param name="creator">UI创建器</param>
        /// <param name="descFilePath">UI目录</param>
        /// <param name="layer">目标层级</param>
        /// <param name="isFullScreen">是否全屏</param>
        /// <param name="userData">用户自定义数据</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>返回创建后的UI对象</returns>
        public Task<T> AddAsync<T>(System.Func<object, T> creator, string descFilePath, UILayer layer, bool isFullScreen = false, object userData = null) where T : FUI
        {
            GameFrameworkGuard.NotNull(creator, nameof(creator));
            GameFrameworkGuard.NotNull(descFilePath, nameof(descFilePath));
            var ts = new TaskCompletionSource<T>();
            UIPackage.AddPackageAsync(descFilePath, (obj) =>
            {
                T ui = creator(userData);
                Add(ui, layer);
                if (isFullScreen)
                {
                    ui.MakeFullScreen();
                }

                ts.TrySetResult(ui);
            });
            return ts.Task;
        }

        /// <summary>
        /// 添加UI对象
        /// </summary>
        /// <param name="creator">UI创建器</param>
        /// <param name="descFilePath">UI目录</param>
        /// <param name="layer">目标层级</param>
        /// <param name="isFullScreen">是否全屏</param>
        /// <param name="userData">用户自定义数据</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>返回创建后的UI对象</returns>
        /// <exception cref="ArgumentNullException">创建器不存在,引发参数异常</exception>
        public T Add<T>(System.Func<object, T> creator, string descFilePath, UILayer layer, bool isFullScreen = false, object userData = null) where T : FUI
        {
            GameFrameworkGuard.NotNull(creator, nameof(creator));
            GameFrameworkGuard.NotNull(descFilePath, nameof(descFilePath));
            _packageComponent.AddPackage(descFilePath);
            T ui = creator(userData);
            Add(ui, layer);
            if (isFullScreen)
            {
                ui.MakeFullScreen();
            }

            return ui;
        }


        public void RemoveAll()
        {
            foreach (var keyValuePair in _uiDictionary)
            {
                keyValuePair.Value.Dispose();
            }

            _uiDictionary.Clear();
            foreach (var kv in _dictionary)
            {
                foreach (var fui in kv.Value)
                {
                    Remove(fui.Key);
                }

                kv.Value.Clear();
            }
        }

        private FUI Add(FUI ui, UILayer layer)
        {
            GameFrameworkGuard.NotNull(ui, nameof(ui));
            if (!_uiDictionary.ContainsKey(ui.Name))
            {
                _uiDictionary[ui.Name] = ui;
            }

            _dictionary[layer][ui.Name] = ui;
            switch (layer)
            {
                case UILayer.Hidden:
                    HiddenRoot.Add(ui);
                    break;
                case UILayer.Floor:
                    FloorRoot.Add(ui);
                    break;
                case UILayer.Normal:
                    NormalRoot.Add(ui);
                    break;
                case UILayer.Fixed:
                    FixedRoot.Add(ui);
                    break;
                case UILayer.Window:
                    WindowRoot.Add(ui);
                    break;
                case UILayer.Tip:
                    TipRoot.Add(ui);
                    break;
                case UILayer.BlackBoard:
                    BlackBoardRoot.Add(ui);
                    break;
                case UILayer.Dialogue:
                    DialogueRoot.Add(ui);
                    break;
                case UILayer.Guide:
                    GuideRoot.Add(ui);
                    break;
                case UILayer.Loading:
                    LoadingRoot.Add(ui);
                    break;
                case UILayer.Notify:
                    NotifyRoot.Add(ui);
                    break;
                case UILayer.System:
                    SystemRoot.Add(ui);
                    break;
            }

            return ui;
        }

        public bool Remove(string uiName)
        {
            GameFrameworkGuard.NotNullOrEmpty(uiName, nameof(uiName));
            if (SystemRoot.Remove(uiName))
            {
                return true;
            }

            if (NotifyRoot.Remove(uiName))
            {
                return true;
            }

            if (HiddenRoot.Remove(uiName))
            {
                return true;
            }

            if (FloorRoot.Remove(uiName))
            {
                return true;
            }

            if (NormalRoot.Remove(uiName))
            {
                return true;
            }

            if (FixedRoot.Remove(uiName))
            {
                return true;
            }

            if (WindowRoot.Remove(uiName))
            {
                return true;
            }

            if (TipRoot.Remove(uiName))
            {
                return true;
            }

            if (BlackBoardRoot.Remove(uiName))
            {
                return true;
            }

            if (DialogueRoot.Remove(uiName))
            {
                return true;
            }

            if (GuideRoot.Remove(uiName))
            {
                return true;
            }

            if (LoadingRoot.Remove(uiName))
            {
                return true;
            }

            return false;
        }

        public void Remove(string uiName, UILayer layer)
        {
            GameFrameworkGuard.NotNullOrEmpty(uiName, nameof(uiName));
            switch (layer)
            {
                case UILayer.Hidden:
                    HiddenRoot.Remove(uiName);
                    break;
                case UILayer.Floor:
                    FloorRoot.Remove(uiName);
                    break;
                case UILayer.Normal:
                    NormalRoot.Remove(uiName);
                    break;
                case UILayer.Fixed:
                    FixedRoot.Remove(uiName);
                    break;
                case UILayer.Window:
                    WindowRoot.Remove(uiName);
                    break;
                case UILayer.Tip:
                    TipRoot.Remove(uiName);
                    break;
                case UILayer.BlackBoard:
                    BlackBoardRoot.Remove(uiName);
                    break;
                case UILayer.Dialogue:
                    DialogueRoot.Remove(uiName);
                    break;
                case UILayer.Guide:
                    GuideRoot.Remove(uiName);
                    break;
                case UILayer.Loading:
                    LoadingRoot.Remove(uiName);
                    break;
                case UILayer.Notify:
                    NotifyRoot.Remove(uiName);
                    break;
                case UILayer.System:
                    SystemRoot.Remove(uiName);
                    break;
            }
        }

        public bool Has(string uiName)
        {
            GameFrameworkGuard.NotNullOrEmpty(uiName, nameof(uiName));
            return Get(uiName) != null;
        }

        /// <summary>
        /// 判断UI是否已创建。如果创建则。返回UI对象
        /// </summary>
        /// <param name="uiName"></param>
        /// <param name="fui"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Has<T>(string uiName, out T fui) where T : FUI
        {
            GameFrameworkGuard.NotNullOrEmpty(uiName, nameof(uiName));
            var ui = Get(uiName);
            fui = ui as T;
            return fui != null;
        }

        public T Get<T>(string uiName) where T : FUI
        {
            GameFrameworkGuard.NotNullOrEmpty(uiName, nameof(uiName));
            if (_uiDictionary.TryGetValue(uiName, out var ui))
            {
                return ui as T;
            }

            return null;
        }

        public FUI Get(string uiName)
        {
            GameFrameworkGuard.NotNullOrEmpty(uiName, nameof(uiName));
            if (_uiDictionary.TryGetValue(uiName, out var ui))
            {
                return ui;
            }

            return null;
        }

        protected override void Awake()
        {
            base.Awake();
            _root = new FUI(GRoot.inst);
            _root.Show();
            _screenOrientation = Screen.orientation;
            HiddenRoot = CreateNode(GRoot.inst, UILayer.Hidden);
            FloorRoot = CreateNode(GRoot.inst, UILayer.Floor);
            NormalRoot = CreateNode(GRoot.inst, UILayer.Normal);
            FixedRoot = CreateNode(GRoot.inst, UILayer.Fixed);
            WindowRoot = CreateNode(GRoot.inst, UILayer.Window);
            TipRoot = CreateNode(GRoot.inst, UILayer.Tip);
            BlackBoardRoot = CreateNode(GRoot.inst, UILayer.BlackBoard);
            DialogueRoot = CreateNode(GRoot.inst, UILayer.Dialogue);
            GuideRoot = CreateNode(GRoot.inst, UILayer.Guide);
            LoadingRoot = CreateNode(GRoot.inst, UILayer.Loading);
            NotifyRoot = CreateNode(GRoot.inst, UILayer.Notify);
            SystemRoot = CreateNode(GRoot.inst, UILayer.System);


            _dictionary[UILayer.Hidden] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.Floor] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.Normal] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.Fixed] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.Window] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.Tip] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.BlackBoard] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.Dialogue] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.Guide] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.Loading] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.Notify] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.System] = new Dictionary<string, FUI>(64);
        }

        FUI CreateNode(GComponent root, UILayer layer)
        {
            GComponent component = new GComponent();
            root.AddChild(component);
            component.z = (int)layer * 100;

            var comName = layer.ToString();

            component.displayObject.name = comName;
            component.gameObjectName = comName;
            component.name = comName;
            component.MakeFullScreen();
            component.AddRelation(root, RelationType.Width);
            component.AddRelation(root, RelationType.Height);
            var ui = new FUI(component);
            ui.Show();
            return ui;
        }


        private ScreenOrientation _screenOrientation;


        void IsChanged(bool isLeft)
        {
            // GameApp.Event.Fire("ScreenOrientationChanged", isLeft);
        }

        public void Update()
        {
            var orientation = Screen.orientation;
            if (orientation == ScreenOrientation.LandscapeLeft || orientation == ScreenOrientation.LandscapeRight)
            {
                if (_screenOrientation != orientation)
                {
                    IsChanged(orientation == ScreenOrientation.LandscapeLeft);
                    _screenOrientation = orientation;
                }
            }
        }
    }
}