﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FastHotKeyForWPF
{
    /// <summary>
    /// HotKeysBox.xaml 的交互逻辑
    /// </summary>
    public partial class HotKeysBox : UserControl
    {
        public HotKeysBox()
        {
            InitializeComponent();

            WhileInput += KeyHandling;
        }

        /// <summary>
        /// 系统键
        /// </summary>
        public Key CurrentKeyA { get; private set; }

        /// <summary>
        /// 普通键
        /// </summary>
        public Key CurrentKeyB { get; private set; }

        /// <summary>
        /// 用户按下Enter键时,您希望额外处理一些事情,例如弹出提示框以告诉用户完成了热键的设置
        /// </summary>
        public event Action? WhileInput = null;

        /// <summary>
        /// 若用户输入不受支持的Key，如何显示文本
        /// </summary>
        public string ErrorText { get; set; } = "Error";

        /// <summary>
        /// 连接左右Key的字符
        /// </summary>
        public string ConnectText { get; set; } = " + ";

        /// <summary>
        /// 反映该控件及其关联控件是否成功注册了热键
        /// </summary>
        public bool IsHotKeyRegistered { get; private set; } = false;

        /// <summary>
        /// 上一个成功注册热键的ID
        /// </summary>
        internal int LastHotKeyID { get; set; } = -1;

        internal event KeyInvoke_Return? HandleA;
        internal event KeyInvoke_Void? HandleB;

        /// <summary>
        /// 与指定的处理函数连接
        /// </summary>
        public void ConnectWith(KeyInvoke_Void handle)
        {
            HandleA = null;
            HandleB = null;

            HandleB = handle;
        }

        /// <summary>
        /// 与其它 HotKeyBox 连接
        /// </summary>
        public void ConnectWith(KeyInvoke_Return handle)
        {
            HandleA = null;
            HandleB = null;

            HandleA = handle;
        }

        /// <summary>
        /// 取消连接
        /// </summary>
        public void DisConnect()
        {
            HandleA = null;
            HandleB = null;

            GlobalHotKey.DeleteById(LastHotKeyID);
            IsHotKeyRegistered = false;
            LastHotKeyID = -1;
        }

        private void UserInput(object sender, KeyEventArgs e)
        {
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            if (key == Key.Return) { Keyboard.ClearFocus(); WhileInput?.Invoke(); return; }

            var result = KeyHelper.IsKeyValid(key);
            if (result.Item1)
            {
                switch (result.Item2)
                {
                    case KeyTypes.Model:
                        CurrentKeyA = key;
                        break;
                    case KeyTypes.Normal:
                        CurrentKeyB = key;
                        break;
                    case KeyTypes.None:
                        break;
                }
            }

            UpdateText();
        }

        private void TextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            FocusGet.Focus();
        }

        private void TextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            Keyboard.ClearFocus();
            KeyHandling();
        }

        private void KeyHandling()
        {
            GlobalHotKey.DeleteById(LastHotKeyID);

            IsHotKeyRegistered = false;
            LastHotKeyID = -1;

            var resultA = KeyHelper.IsKeyValid(CurrentKeyA);
            var resultB = KeyHelper.IsKeyValid(CurrentKeyB);

            if (resultA.Item1 && resultB.Item1)
            {
                if (HandleA != null)
                {
                    var register = GlobalHotKey.Add(KeyHelper.KeyToModelKeys[CurrentKeyA], KeyHelper.KeyToNormalKeys[CurrentKeyB], HandleA);
                    if (register.Item1)
                    {
                        IsHotKeyRegistered = true;
                        LastHotKeyID = register.Item2;
                        return;
                    }
                }
                if (HandleB != null)
                {
                    var register = GlobalHotKey.Add(KeyHelper.KeyToModelKeys[CurrentKeyA], KeyHelper.KeyToNormalKeys[CurrentKeyB], HandleB);
                    if (register.Item1)
                    {
                        IsHotKeyRegistered = true;
                        LastHotKeyID = register.Item2;
                        return;
                    }
                }
            }

            IsHotKeyRegistered = false;
            LastHotKeyID = -1;
        }

        private void UpdateText()
        {
            ActualText.Text = CurrentKeyA.ToString() + ConnectText + CurrentKeyB.ToString();
        }
    }
}