using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Frostybee.Hotkeys.Controls;

public class HotKeyEditorControl : TextBox
{
    public static readonly DependencyProperty HotKeyProperty = DependencyProperty.Register(
        nameof(HotKey),
        typeof(HotKey),
        typeof(HotKeyEditorControl),
        new FrameworkPropertyMetadata(
            default(HotKey),
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            (sender, _) =>
            {
                var control = (HotKeyEditorControl)sender;
                control.Text = control.HotKey.ToString();
            }
        )
    );

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
            ContextMenu.Visibility = Visibility.Collapsed;
        HotKey = HotKey.None;
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
        e.Handled = true;

        var key = e.Key;
        HotKey = HotKey.None;
        var modifiers = Keyboard.Modifiers;

        // If nothing was pressed - return
        if (key == Key.None)
            return;

        // If the left or right Win key is pressed, reject the combination.
        if (Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin))
        {
            return;
        }
        /*// Pressing delete, backspace or escape without modifiers clears the current value
        if (modifiers == ModifierKeys.None &&
            (key == Key.Delete || key == Key.Back || key == Key.Escape))
        {
            Hotkey = null;
            return;
        }*/
        // If the only key pressed is one of the modifier keys - return
        if (key is Key.LeftCtrl or Key.RightCtrl
                or Key.LeftAlt or Key.RightAlt
                or Key.LeftShift or Key.RightShift
                or Key.Clear or Key.OemClear
                or Key.Apps)
        {
            return;
        }
        // Check of the following 3 modifiers are pressed
        // Ctrl, Alt, and Shift keys are required
        if (
             !(modifiers.HasFlag(ModifierKeys.Control) &&
             modifiers.HasFlag(ModifierKeys.Alt) &&
             modifiers.HasFlag(ModifierKeys.Shift))
           )
        {
            //TODO: put these modifiers in an array. 
            return;
        }

        // If Delete/Backspace/Escape is pressed without modifiers - clear current value and return
        if (key is Key.Delete or Key.Back or Key.Escape && modifiers == ModifierKeys.None)
        {
            return;
        }

        //KeyInterop.VirtualKeyFromKey(e.Key);
        //---- VALIDATION RULES: 
        //KeyInterop.VirtualKeyFromKey(e.Key);
        //TODO: use VirtualKeyFromKey instead.
        //TODO: Check if there is not modifier key pressed, disable. There must be at least two modifier keys.
        // Rule 1: The key combination  must start with a Ctrl, Alt, or Shift. and it can contains any combination of two or three modifiers.
        //----      

        // If Enter/Space/Tab is pressed - return
        //if (key is Key.Enter or Key.Space or Key.Tab && modifiers == ModifierKeys.None)
        if (key is Key.Enter or Key.Space or Key.Tab)
            return;

        // If key has a character and pressed without modifiers or only with Shift - return
        //if (HasKeyChar(key) && modifiers is ModifierKeys.None or ModifierKeys.Shift)
        //  return;
        //TODO: check if the key is blacklisted with or without modifiers pressed.

        // Set value
        HotKey = new HotKey(key, modifiers);

        //------------------------------
        /*// If Alt is used as modifier - the key needs to be extracted from SystemKey
        if (key == Key.System)
            key = e.SystemKey;
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
}
