﻿using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace NHotkeysEditor.Controls;


public class HotKeySelector : TextBox
{
    private static readonly int WM_Hotkey = 786;

    public enum RequiredModifiersType
    {
        None = 0,
        CtrlShiftAlt,
        CtrlAlt,
        CtrlShift
    }

    public enum AllowedKeysType
    {
        AllKeys = 0,
        LettersDigitsFunctions
    }

    #region Fields

    /// <summary>
    /// The text to be displayed in the control when an invalid or unsupported hotkey is pressed.    
    /// </summary>
    private string UnsupportedKeyText { get; } = "Unsupported";
    private string NoneHotkeyText { get; } = "<None>";

    /// <summary>
    /// Holds the list of keys that, when pressed, clear the content of this control.
    /// </summary>
    private readonly List<Key> _clearKeys = new();
    /// <summary>
    /// Holds the list of keys that are whitelisted given the selected type 
    /// of allowed keys.
    /// </summary>
    private readonly List<Key> _allowedKeys = new();
    
    public static readonly DependencyProperty SelectedHotKeyProperty = DependencyProperty.Register(
        nameof(SelectedHotKey),
        typeof(HotKey),
        typeof(HotKeySelector),
            new FrameworkPropertyMetadata(
                default(HotKey),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHotKeyPropertyChanged));

    public static DependencyProperty ExcludedKeysProperty = DependencyProperty.Register(
        nameof(ExcludedKeys),
        typeof(List<Key>),
        typeof(HotKeySelector), new PropertyMetadata(new List<Key> { Key.None}));

    public List<Key> ExcludedKeys
    {
        get => (List<Key>)this.GetValue(ExcludedKeysProperty);
        set => this.SetValue(ExcludedKeysProperty, value);
    }

    public static readonly DependencyProperty MinRequiredModifiersProperty = DependencyProperty.Register(
        nameof(MinRequiredModifiers),
        typeof(RequiredModifiersType),
        typeof(HotKeySelector),
        new FrameworkPropertyMetadata(
            //TODO: review/decide what should be assigned as default value.
            RequiredModifiersType.CtrlShiftAlt,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
        ));

    public static readonly DependencyProperty RangeOfAllowedKeysProperty = DependencyProperty.Register(
        nameof(RangeOfAllowedKeys),
        typeof(AllowedKeysType),
        typeof(HotKeySelector),
        new FrameworkPropertyMetadata(
            AllowedKeysType.LettersDigitsFunctions,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
        ));

    #endregion

    #region Properties
    public HotKey SelectedHotKey
    {
        get => (HotKey)GetValue(SelectedHotKeyProperty);
        set => SetValue(SelectedHotKeyProperty, value);
    }
    public RequiredModifiersType MinRequiredModifiers
    {
        get => (RequiredModifiersType)GetValue(MinRequiredModifiersProperty);
        set => SetValue(MinRequiredModifiersProperty, value);
    }
    public AllowedKeysType RangeOfAllowedKeys
    {
        get => (AllowedKeysType)GetValue(RangeOfAllowedKeysProperty);
        set => SetValue(RangeOfAllowedKeysProperty, value);
    }

    #endregion

    public HotKeySelector()
    {
        if (ContextMenu is not null)
        {
            ContextMenu.Visibility = Visibility.Collapsed;
        }
        SelectedHotKey = HotKey.None;
        Focus();
        UpdateControlText();
        PopulateClearKeys();
        PopulateAllowedKeys();

    }

    private void PopulateClearKeys()
    {
        _clearKeys.Clear();
        _clearKeys.AddRange(new Key[] {
            Key.Escape, 
            Key.Space,
            Key.Back, 
            Key.Delete, 
            Key.Tab,
            Key.Insert, 
            Key.Scroll,
            Key.NumLock, 
            Key.Return,
            Key.CapsLock, 
            Key.Capital,
            Key.Pause, 
            Key.Enter, 
            Key.Clear
        });
    }

    /// <summary>
    /// Populates the list of allowed letters, digits, and function keys.
    /// </summary>
    private void PopulateAllowedKeys()
    {
        if (RangeOfAllowedKeys == AllowedKeysType.LettersDigitsFunctions)
        {
            PopulateLettersDigitsFun();
        }
    }

    private void PopulateLettersDigitsFun()
    {
        _allowedKeys.Clear();
        // Add 0 - 9, and a-z keys.
        for (Key k = Key.D0; k <= Key.Z; k++)
        {
            _allowedKeys.Add(k);
        }
        // Add the functions keys (F1-F12)
        for (Key k = Key.F1; k <= Key.F12; k++)
        {
            _allowedKeys.Add(k);
        }        
    }

    private static void OnHotKeyPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        (sender as HotKeySelector)?.UpdateControlText();
    }

    public override void OnApplyTemplate()
    {
        this.GotFocus -= this.TextBoxOnGotFocus;
        this.LostFocus -= this.TextBoxOnLostFocus;
        this.TextChanged -= this.TextBoxOnTextChanged;

        base.OnApplyTemplate();

        this.GotFocus += this.TextBoxOnGotFocus;
        this.LostFocus += this.TextBoxOnLostFocus;
        this.TextChanged += this.TextBoxOnTextChanged;

        UpdateControlText();
    }
    private void TextBoxOnGotFocus(object sender, RoutedEventArgs routedEventArgs)
    {
        ComponentDispatcher.ThreadPreprocessMessage += this.ComponentDispatcherOnThreadPreprocessMessage;
    }
    private void TextBoxOnTextChanged(object sender, TextChangedEventArgs args)
    {
        //TODO: the following is not necessary.
        this!.SelectionStart = this.Text.Length;
        if (this.Text.Length == 0)
        {
            this.SelectedHotKey = HotKey.None;
        }            
    }
    private void TextBoxOnLostFocus(object sender, RoutedEventArgs routedEventArgs)
    {
        ComponentDispatcher.ThreadPreprocessMessage -= this.ComponentDispatcherOnThreadPreprocessMessage;
    }
    private void ComponentDispatcherOnThreadPreprocessMessage(ref MSG msg, ref bool handled)
    {
        if (handled || msg.message != WM_Hotkey)
            return;

        if (msg.message == WM_Hotkey)
        {
            // swallow all hotkeys, so our control can catch the pressedKey strokes
            handled = true;
        }
    }

    private void UpdateControlText()
    {
        if (SelectedHotKey is null || SelectedHotKey.PressedKey == Key.None)
        {
            Text = NoneHotkeyText;
            return;
        }
        Text = SelectedHotKey.ToString();
    }


    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        // Swallow all hotkeys, so our control can catch the pressedKey strokes
        e.Handled = true;

        SelectedHotKey = HotKey.None;
        var pressedModifiers = Keyboard.Modifiers;
        // Handle the case where F10 is pressed
        var pressedKey = e.Key == Key.System ? e.SystemKey : e.Key;
        var minRequiredModifiers = GetRequiredModifiers();

        // If nothing was pressed - return
        if (pressedKey == Key.None)
            return;

        // LWin/RWin are not allowed as hotkeys - reject the combination.
        if (Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin))
        {
            Text = UnsupportedKeyText;
            return;
        }
        // If a clear key (such as delete/space/tab/escape is pressed
        // without pressedModifiers - clear current value and return
        if (_clearKeys.Contains(pressedKey) && pressedModifiers == ModifierKeys.None)
        {
            //Hotkey = null;
            UpdateControlText();
            return;
        }
        // If the pressed key is any of the following with a modifier - return
        switch (pressedKey)
        {
            case Key.Tab:
            case Key.LeftShift:
            case Key.RightShift:
            case Key.LeftCtrl:
            case Key.RightCtrl:
            case Key.LeftAlt:
            case Key.RightAlt:
            case Key.Clear:
            case Key.Insert:
            case Key.OemClear:
            case Key.Apps:
                UpdateControlText();
                return;
        }
        // If the required modifier(s) are not all pressed - return.
        if (!Keyboard.Modifiers.HasFlag(minRequiredModifiers))
        {
            UpdateControlText();
            return;
        }
        if (ExcludedKeys.Contains(pressedKey))
        {
            UpdateControlText();
            return;
        }
        if (RangeOfAllowedKeys == AllowedKeysType.LettersDigitsFunctions)
        {
            // If the pressed key is not one of the whitelisted keys - return
            if (!_allowedKeys.Contains(pressedKey))
            {
                UpdateControlText();
                return;
            }
        }

        // We now have a valid hotkey.
        SelectedHotKey = new HotKey(pressedKey, pressedModifiers);
    }

    private ModifierKeys GetRequiredModifiers()
    {
        var expectedModifiers = ModifierKeys.None;
        switch (MinRequiredModifiers)
        {
            case RequiredModifiersType.CtrlShiftAlt:
                expectedModifiers |= ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt;
                break;
            case RequiredModifiersType.CtrlAlt:
                expectedModifiers |= ModifierKeys.Control | ModifierKeys.Alt;
                break;
            case RequiredModifiersType.CtrlShift:
                expectedModifiers |= ModifierKeys.Control | ModifierKeys.Shift;
                break;
            default:
                expectedModifiers = ModifierKeys.None;
                break;
        }
        return expectedModifiers;
    }
}
