using System.Diagnostics;
using System.Globalization;
using System.IO.Packaging;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace Frostybee.Hotkeys.Controls;

public class HotKeyEditorControl : TextBox
{
    private static int WM_Hotkey = 786;
    public static readonly DependencyProperty HotKeyProperty = DependencyProperty.Register(
        nameof(HotKey),
        typeof(HotKey),
        typeof(HotKeyEditorControl),
        new FrameworkPropertyMetadata(
            default(HotKey),
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHotKeyPropertyChanged));

    /// <summary>
    /// The text to be displayed in a control when an invalid or unsupported hotkey is pressed.
    /// (Preferred default text is "(Unsupported)")
    /// </summary>
    private string InvalidKeyText { get; } = "Unsupported";
    private string NoneHotkeyText { get; } = "<None>";
    private string _reasonText = string.Empty;

    /// <summary>
    /// Holds the list of required modifiers. 
    /// </summary>
    List<ModifierKeys> _minRequiredModifiers = new List<ModifierKeys>
            //{ModifierKeys.Shift, ModifierKeys.Control, ModifierKeys.Alt};
            //{ModifierKeys.Control, ModifierKeys.Alt};
            {ModifierKeys.Control};

    public HotKey HotKey
    {
        get => (HotKey)GetValue(HotKeyProperty);
        set => SetValue(HotKeyProperty, value);
    }

    public HotKeyEditorControl()
    {
        /*IsReadOnly = true;
        IsReadOnlyCaretVisible = false;
        IsUndoEnabled = false;*/

        if (ContextMenu is not null)
        {
            ContextMenu.Visibility = Visibility.Collapsed;
        }
        HotKey = HotKey.None;
        Focus();
        UpdateControlText();
    }
    private static void OnHotKeyPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        (sender as HotKeyEditorControl)?.UpdateControlText();
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
            this.HotKey = null;
    }
    private void TextBoxOnLostFocus(object sender, RoutedEventArgs routedEventArgs)
    {
        ComponentDispatcher.ThreadPreprocessMessage -= this.ComponentDispatcherOnThreadPreprocessMessage;
    }
    private void ComponentDispatcherOnThreadPreprocessMessage(ref MSG msg, ref bool handled)
    {
        if (msg.message == WM_Hotkey)
        {
            // swallow all hotkeys, so our control can catch the pressedKey strokes
            handled = true;
        }
    }
    
    private void UpdateControlText()
    {
        if (HotKey is null || HotKey.Key == Key.None)
        {
            Text = NoneHotkeyText;
            return;
        }
        Text = HotKey.ToString();
    }

    private static bool HasKeyChar(Key key) =>
        key
            is
                // A - Z
                >= Key.A
                and <= Key.Z
                or
                // 0 - 9
                >= Key.D0
                and <= Key.D9
                or
                // Numpad 0 - 9
                >= Key.NumPad0
                and <= Key.NumPad9
                or
                // The rest
                Key.OemQuestion
                or Key.OemQuotes
                or Key.OemPlus
                or Key.OemOpenBrackets
                or Key.OemCloseBrackets
                or Key.OemMinus
                or Key.DeadCharProcessed
                or Key.Oem1
                or Key.Oem5
                or Key.Oem7
                or Key.OemPeriod
                or Key.OemComma
                or Key.Add
                or Key.Divide
                or Key.Multiply
                or Key.Subtract
                or Key.Oem102
                or Key.Decimal;

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        // Swallow all hotkeys, so our control can catch the pressedKey strokes
        e.Handled = true;

        var pressedKey = e.Key;
        HotKey = HotKey.None;
        var pressedModifiers = Keyboard.Modifiers;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        switch (key)
        {
            case Key.Tab:
            case Key.LeftShift:
            case Key.RightShift:
            case Key.LeftCtrl:
            case Key.RightCtrl:
            case Key.LeftAlt:
            case Key.RightAlt:
            case Key.Enter:
            case Key.RWin:
            case Key.LWin:
                return;
        }

        // If nothing was pressed - return
        if (pressedKey == Key.None)
            return;

        // LWin/RWin are not allowed as hotkeys - reject the combination.
        if (Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin))
        {
            UpdateControlText();
            return;
        }
        // If Delete/Backspace/Escape is pressed without pressedModifiers - clear current value and return
        if (pressedKey is Key.Delete or Key.Enter or Key.Space or Key.Back or Key.Tab or Key.Escape && pressedModifiers == ModifierKeys.None)
        {
            //Hotkey = null;
            UpdateControlText();
            return;
        }

        // Check whether the required modifier(s) are all pressed.
        if (!AreRequiredModifiersPressed())
        {
            UpdateControlText();
            return;
        }
        // Now check if the pressed key is an acceptable one.

        /*// If the only pressedKey pressed is one of the modifier keys - return
        if (pressedKey is Key.LeftCtrl or Key.RightCtrl
                or Key.LeftAlt or Key.RightAlt
                or Key.LeftShift or Key.RightShift
                or Key.Clear or Key.OemClear
                or Key.Apps)
        {
            return;
        }*/

        // If pressedKey has a character and pressed without pressedModifiers or only with Shift - return
        //if (HasKeyChar(pressedKey) && pressedModifiers is ModifierKeys.None or ModifierKeys.Shift)
        //  return;
        //TODO: check if the pressedKey is blacklisted with or without pressedModifiers pressed.

        // Set value
        HotKey = new HotKey(pressedKey, pressedModifiers);

        //------------------------------
        /*// If Alt is used as modifier - the pressedKey needs to be extracted from SystemKey
        if (pressedKey == Key.System)
            pressedKey = e.SystemKey;
        if (
            !((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
            (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) &&
            (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            )
        {
            return;
        }
         */
    }
    private bool AreRequiredModifiersPressed()
    {
        var expectedModifiers = ModifierKeys.None;
        // Combined the required expectedModifiers 
        _minRequiredModifiers.ForEach(m => expectedModifiers |= m);
        return (Keyboard.Modifiers.HasFlag(expectedModifiers));
    }
}
