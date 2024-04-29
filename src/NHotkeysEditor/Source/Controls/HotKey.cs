using NHotkeysEditor.NativeApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NHotkeysEditor.Controls;


[Serializable]
public class HotKey : IEquatable<HotKey>
{
    private const int VK_MENU = 18;
    private const int VK_CONTROL = 17;
    private const int VK_SHIFT = 16;
    public readonly static HotKey None = new(Key.None, ModifierKeys.None);
    public Key PressedKey { get; } = Key.None;
    public ModifierKeys PressedModifiers { get; } = ModifierKeys.None;

    public HotKey(Key key, ModifierKeys modifierKeys = ModifierKeys.None)
    {
        this.PressedKey = key;
        this.PressedModifiers = modifierKeys;
    }

    public override bool Equals(object obj)
    {
        return obj is HotKey key && this.Equals(key);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((int)this.PressedKey * 397) ^ (int)this.PressedModifiers;
        }
    }

    public bool Equals(HotKey other)
    {
        if (other is null)
            return false;
        return this.PressedKey == other.PressedKey && this.PressedModifiers == other.PressedModifiers;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        if ((this.PressedModifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            sb.Append(GetLocalizedKeyStringUnsafe(VK_CONTROL));
            sb.Append('+');
        }
        if ((this.PressedModifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
        {
            sb.Append(GetLocalizedKeyStringUnsafe(VK_MENU));
            sb.Append('+');
        }
        if ((this.PressedModifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
        {
            sb.Append(GetLocalizedKeyStringUnsafe(VK_SHIFT));
            sb.Append('+');
        }


        sb.Append(GetLocalizedKeyString(this.PressedKey));
        return sb.ToString();
    }

    private static string GetLocalizedKeyString(Key key)
    {
        if (key >= Key.BrowserBack && key <= Key.LaunchApplication2)
        {
            return key.ToString();
        }

        var vkey = KeyInterop.VirtualKeyFromKey(key);
        return GetLocalizedKeyStringUnsafe(vkey) ?? key.ToString();
    }

    private static string GetLocalizedKeyStringUnsafe(int key)
    {
        // strip any modifier keys
        long keyCode = key & 0xffff;

        var sb = new StringBuilder(256);

        long scanCode = NativeMethods.MapVirtualKey((uint)keyCode, NativeMethods.MapType.MAPVK_VK_TO_VSC);

        // shift the scancode to the high word
        scanCode = (scanCode << 16);
        if (keyCode == 45 ||
            keyCode == 46 ||
            keyCode == 144 ||
            (33 <= keyCode && keyCode <= 40))
        {
            // add the extended key flag
            scanCode |= 0x1000000;
        }

        var resultLength = NativeMethods.GetKeyNameText((int)scanCode, sb, 256);
        return resultLength > 0 ? sb.ToString() : null;
    }
}
