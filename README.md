# NHotkeys Editor

NHotkeys Editor is a customizable shortcut editor control for **WPF**. It allows the user to enter a combination of keys that can be later associated with an action to be performed within a WPF application.

This control does not support the `Win` modifier key.

## Modifier Keys

To specify the modifier key(s) that are required to be included in a shortcut, you can assign one of the following values to the `MinRequiredModifiers` property:

- `CtrlShiftAlt` : Ctrl, Shift, and Alt keys,
- `CtrlAlt` : Ctrl and Alt,
- `CtrlShift` : Ctrl and Shift.

## Controlling Allowed Keys

- `RangeOfAllowedKeys` : AllKeys, LettersDigitsFunctions

## Usage Example

Add the following to your .xaml file.

```xml
<window
    xmlns:fb="clr-namespace:NHotkeysEditor.Controls;assembly=NHotkeysEditor">
    <fb:HotKeySelector
        Width="250"
        Height="35"
        HorizontalAlignment="Left"
        HorizontalContentAlignment="Center"
        VerticalContentAlignment="Center"    
        BorderThickness="1"
        CaretBrush="Red"
        ExcludedKeys="{Binding ExcludedKeys}"
        FontSize="15"    
        IsReadOnly="True"
        IsReadOnlyCaretVisible="True"
        IsUndoEnabled="False"
        MinRequiredModifiers="CtrlShiftAlt"
        RangeOfAllowedKeys="All" />
</window>    
```

## Behaviors

- Pressing escape resets the control.
  