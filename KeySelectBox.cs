﻿using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Reflection;
using System.Windows.Media;

public enum KeyTypes
{
    Normal,
    Model,
    None
}

namespace FastHotKeyForWPF
{
    /// <summary>
    /// 组件☆
    /// <para>功能 接收用户按下的单个键，并在与其它KeySelectBox连接后，激活热键的全自动管理</para>
    /// <para>继承 TextBox类</para>
    /// <para>实现 Component接口</para>
    /// </summary>
    public class KeySelectBox : KeyBox
    {
        private Key _currentkey;
        /// <summary>
        /// 当前获取到的用户按键
        /// </summary>
        public Key CurrentKey
        {
            get { return _currentkey; }
            set
            {
                var olddate = BindingRef.GetKeysFromConnection(this);
                if (olddate.Item1 != null && olddate.Item2 != null)
                {
                    GlobalHotKey.DeleteByKeys((ModelKeys)olddate.Item1, (NormalKeys)olddate.Item2);
                }
                _currentkey = value;
                UpdateHotKey();
            }
        }

        /// <summary>
        /// 是否处于连接状态
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (LinkBox == null && Event_void == null && Event_return == null) return false;
                if (LinkBox != null && Event_void != null && Event_return == null) return true;
                if (LinkBox != null && Event_void == null && Event_return != null) return true;
                return false;
            }
        }

        /// <summary>
        /// 该组件的关联组件
        /// </summary>
        internal KeySelectBox? LinkBox;

        /// <summary>
        /// 当前按键的类型
        /// </summary>
        public KeyTypes KeyType
        {
            get
            {
                if (Enum.IsDefined(typeof(NormalKeys), CurrentKey.ToString()))
                {
                    return KeyTypes.Normal;
                }
                if (Enum.IsDefined(typeof(ModelKeys), CurrentKey.ToString()))
                {
                    return KeyTypes.Model;
                }
                return KeyTypes.None;
            }
        }

        internal KeySelectBox()
        {
            if (PrefabComponent.TempInfo == null) { return; }
            Width = 100;
            Height = 50;
            VerticalContentAlignment = VerticalAlignment.Center;
            HorizontalContentAlignment = HorizontalAlignment.Center;
            IsReadOnly = true;
            FontSize = PrefabComponent.TempInfo.FontSize;
            Foreground = PrefabComponent.TempInfo.Foreground;
            Background = PrefabComponent.TempInfo.Background;
            Margin = PrefabComponent.TempInfo.Margin;
            KeyDown += WhileKeyDown;
            MouseEnter += WhileMouseEnter;
            MouseLeave += WhileMouseLeave;
            keySelectBoxes.Add(this);
        }

        private void WhileKeyDown(object sender, KeyEventArgs e)
        {
            if (IsKeySelectBoxProtected || Protected) { return; }
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);
            if (!PrefabComponent.KeyToUint.ContainsKey(key)) { if (GlobalHotKey.IsDeBug) MessageBox.Show($"当前版本不支持这个按键【{key}】"); return; }
            CurrentKey = key;
            Text = key.ToString();
            if (GlobalHotKey.IsDeBug) { MessageBox.Show($"已更新为【{key}】"); }
            e.Handled = true;
        }

        /// <summary>
        /// 更新一次热键信息
        /// </summary>
        /// <returns>bool 表示是否成功更新</returns>
        internal bool UpdateHotKey()
        {
            bool result = false;
            if (IsConnected)
            {
                var date = BindingRef.GetKeysFromConnection(this);

                if (date.Item1 == null || date.Item2 == null) { if (GlobalHotKey.IsDeBug) MessageBox.Show("⚠不正确的键盘组合，无法注册"); return false; }

                if (Event_void != null)
                {
                    result = GlobalHotKey.Add((ModelKeys)date.Item1, (NormalKeys)date.Item2, Event_void).Item1;
                }
                else if (Event_return != null)
                {
                    result = GlobalHotKey.Add((ModelKeys)date.Item1, (NormalKeys)date.Item2, Event_return).Item1;
                }
            }
            return result;
        }
    }
}
