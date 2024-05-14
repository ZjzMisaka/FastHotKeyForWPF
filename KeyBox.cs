﻿using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FastHotKeyForWPF
{
    /// <summary>
    /// 抽象类型，继承自TextBox并实现Component接口
    /// <para>热键设置中，接收用户按下键的组件是基于这个做下去的</para>
    /// </summary>
    public abstract class KeyBox : TextBox, Component
    {
        internal static bool IsKeySelectBoxProtected = false;// KeySelectBox组件是否处于保护中（公共）
        internal static List<KeySelectBox> keySelectBoxes = new List<KeySelectBox>();

        internal static bool IsKeysSelectBoxProtected = false;// KeysSelectBox组件是否处于保护中（公共）
        internal static List<KeysSelectBox> keysSelectBoxes = new List<KeysSelectBox>();

        /// <summary>
        /// Key => NormalKeys 字典
        /// </summary>
        public static readonly Dictionary<Key, NormalKeys> KeyToNormalKeys = new Dictionary<Key, NormalKeys>()
        {
        { Key.Up, NormalKeys.UP },
        { Key.Down, NormalKeys.DOWN },
        { Key.Left, NormalKeys.LEFT },
        { Key.Right, NormalKeys.RIGHT },

        {Key.Space, NormalKeys.SPACE },

        { Key.A, NormalKeys.A },
        { Key.B, NormalKeys.B },
        { Key.C, NormalKeys.C },
        { Key.D, NormalKeys.D },
        { Key.E, NormalKeys.E },
        { Key.F, NormalKeys.F },
        { Key.G, NormalKeys.G },

        { Key.H, NormalKeys.H },
        { Key.I, NormalKeys.I },
        { Key.J, NormalKeys.J },
        { Key.K, NormalKeys.K },
        { Key.L, NormalKeys.L },
        { Key.M, NormalKeys.M },
        { Key.N, NormalKeys.N },

        { Key.O, NormalKeys.O },
        { Key.P, NormalKeys.P },
        { Key.Q, NormalKeys.Q },
        { Key.R, NormalKeys.R },
        { Key.S, NormalKeys.S },
        { Key.T, NormalKeys.T },

        { Key.U, NormalKeys.U },
        { Key.V, NormalKeys.V },
        { Key.W, NormalKeys.W },
        { Key.X, NormalKeys.X },
        { Key.Y, NormalKeys.Y },
        { Key.Z, NormalKeys.Z },

        { Key.D0, NormalKeys.Zero },
        { Key.D1, NormalKeys.One },
        { Key.D2, NormalKeys.Two },
        { Key.D3, NormalKeys.Three },
        { Key.D4, NormalKeys.Four },
        { Key.D5, NormalKeys.Five },
        { Key.D6, NormalKeys.Six },
        { Key.D7, NormalKeys.Seven },
        { Key.D8, NormalKeys.Eight },
        { Key.D9, NormalKeys.Nine },

        { Key.F1, NormalKeys.F1 },
        { Key.F2, NormalKeys.F2 },
        { Key.F3, NormalKeys.F3 },
        { Key.F4, NormalKeys.F4 },
        { Key.F5, NormalKeys.F5 },
        { Key.F6, NormalKeys.F6 },
        { Key.F7, NormalKeys.F7 },

        { Key.F9, NormalKeys.F9 },
        { Key.F10,NormalKeys.F10 },
        { Key.F11,NormalKeys.F11 },
        { Key.F12,NormalKeys.F12 },

        };

        /// <summary>
        /// Key => ModelKeys 字典
        /// </summary>
        public static readonly Dictionary<Key, ModelKeys> KeyToModelKeys = new Dictionary<Key, ModelKeys>()
        {
        { Key.LeftCtrl, ModelKeys.CTRL },
        { Key.RightCtrl, ModelKeys.CTRL },
        { Key.LeftAlt, ModelKeys.ALT },
        { Key.RightAlt, ModelKeys.ALT },
        };

        /// <summary>
        /// 是否被保护（独立）
        /// </summary>
        internal bool Protected = false;

        /// <summary>
        /// 是否启用默认变色效果（独立）
        /// </summary>
        public bool IsDefaultColorChange = true;

        /// <summary>
        /// 该组件负责管理的事件之一
        /// </summary>
        internal KeyInvoke_Void? Event_void;
        /// <summary>
        /// 该组件负责管理的事件之一
        /// </summary>
        internal KeyInvoke_Return? Event_return;

        /// <summary>
        /// 获取焦点时的行为
        /// </summary>
        internal TextBoxFocusChange? Focused;
        /// <summary>
        /// 失去焦点时的行为
        /// </summary>
        internal TextBoxFocusChange? UnFocused;

        /// <summary>
        /// 应用父容器的尺寸，并自动调节字体大小
        /// </summary>
        /// <typeparam name="T">父容器类型</typeparam>
        public void UseFatherSize<T>() where T : UIElement
        {
            T? father = Parent as T;
            if (father == null) { return; }

            PropertyInfo? widthProperty = typeof(T).GetProperty("Width");
            PropertyInfo? heightProperty = typeof(T).GetProperty("Height");
            if (widthProperty == null) { return; }
            if (heightProperty == null) { return; }

            object? width = widthProperty.GetValue(father);
            object? height = heightProperty.GetValue(father);
            if (width == null) { return; }
            if (height == null) { return; }

            Width = (double)width;
            Height = (double)height;
            FontSize = (double)height * 0.8;
        }

        /// <summary>
        /// 应用资源样式中的全部属性
        /// </summary>
        /// <param name="styleName">资源样式的Key</param>
        public void UseStyleProperty(string styleName)
        {
            Style? style = (Style)TryFindResource(styleName);
            if (style == null) return;

            if (style.TargetType == typeof(TextBox))
            {
                foreach (SetterBase setterBase in style.Setters)
                {
                    if (setterBase is Setter setter)
                    {
                        SetValue(setter.Property, setter.Value);
                    }
                }
            }
        }

        /// <summary>
        /// 应用资源样式中，指定名称的属性
        /// </summary>
        /// <param name="styleName">资源样式的Key</param>
        /// <param name="targetProperties">属性名</param>
        public void UseStyleProperty(string styleName, string[] targetProperties)
        {
            Style? style = (Style)TryFindResource(styleName);
            if (style == null) return;

            if (style.TargetType == typeof(TextBox))
            {
                foreach (string target in targetProperties)
                {
                    Setter? targetSetter = style.Setters.FirstOrDefault(s => ((Setter)s).Property.Name == target) as Setter;
                    if (targetSetter != null)
                    {
                        SetValue(targetSetter.Property, targetSetter.Value);
                    }
                }
            }
        }

        /// <summary>
        /// 使用你自定义的焦点变色函数，这些函数必须带有一个TextBox参数定义
        /// </summary>
        /// <param name="enter">获取焦点时</param>
        /// <param name="leave">失去焦点时</param>
        public void UseFocusTrigger(TextBoxFocusChange enter, TextBoxFocusChange leave)
        {
            Focused = null;
            UnFocused = null;
            Focused = enter;
            UnFocused = leave;
            IsDefaultColorChange = false;
        }

        internal void WhileMouseEnter(object sender, MouseEventArgs e)
        {
            Protected = false;
            Focus();
            if (IsDefaultColorChange)
            {
                Background = Brushes.Black;
                Foreground = Brushes.Cyan;
            }
            else
            {
                if (Focused != null) Focused.Invoke(this);
            }
        }

        internal void WhileMouseLeave(object sender, MouseEventArgs e)
        {
            Protected = true;
            Keyboard.ClearFocus();
            if (IsDefaultColorChange)
            {
                Background = Brushes.Wheat;
                Foreground = Brushes.Black;
            }
            else
            {
                if (UnFocused != null) UnFocused.Invoke(this);
            }
        }

        /// <summary>
        /// 将此对象单独设为保护
        /// </summary>
        public void Protect()
        {
            Protected = true;
        }

        public void UnProtect()
        {
            Protected = false;
        }
    }
}