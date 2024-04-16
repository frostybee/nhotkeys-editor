# NHotkeys Editor

NHotkeys Editor is a customizable shortcut editor control for WPF.

**NOTE:** The `Win` key is not supported.

## Configurable Properties

- `MinRequiredModifiers` : CtrlShiftAlt, CtrlAlt, CtrlShift
- `RangeOfAllowedKeys` : AllKeys, LettersDigitsFunctions

## How Do I Use the Control?

Add a reference to NHotkeysEditor.Controls

```xml
xmlns:fb="clr-namespace:NHotkeysEditor.Controls;assembly=NHotkeysEditor"
```

Add the following to your .xaml file.

```xml
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
```

## Behaviors

- Pressing escape resets the control.
  