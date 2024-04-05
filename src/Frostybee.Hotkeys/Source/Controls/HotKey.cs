using FrostyBee.Hotkeys.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Frostybee.Hotkeys.Controls;

[Serializable]
public class HotKey : IEquatable<HotKey>
{
    private const int VK_MENU = 18;
    private const int VK_CONTROL = 17;
    private const int VK_SHIFT = 16;
    public static HotKey None = new HotKey(Key.None, ModifierKeys.None);
    public Key Key { get; } 
    public ModifierKeys ModifierKeys { get; } 

    public HotKey(Key key, ModifierKeys modifierKeys = ModifierKeys.None)
    {
        this.Key = key;
        this.ModifierKeys = modifierKeys;
    }

    public override bool Equals(object obj)
    {
        return obj is HotKey key && this.Equals(key);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((int)this.Key * 397) ^ (int)this.ModifierKeys;
        }
    }

    public bool Equals(HotKey other)
    {
        if (other is null)
            return false;
        return this.Key == other.Key && this.ModifierKeys == other.ModifierKeys;
    }

    public override string ToString()
    {
        KeyConverter converter = new KeyConverter();
        //TODO: use the converter instead?
        var sb = new StringBuilder();
        if ((this.ModifierKeys & ModifierKeys.Alt) == ModifierKeys.Alt)
        {
            sb.Append(GetLocalizedKeyStringUnsafe(VK_MENU));
            sb.Append("+");
        }

        if ((this.ModifierKeys & ModifierKeys.Control) == ModifierKeys.Control)
        {
            sb.Append(GetLocalizedKeyStringUnsafe(VK_CONTROL));
            sb.Append("+");
        }

        if ((this.ModifierKeys & ModifierKeys.Shift) == ModifierKeys.Shift)
        {
            sb.Append(GetLocalizedKeyStringUnsafe(VK_SHIFT));
            sb.Append("+");
        }

        if ((this.ModifierKeys & ModifierKeys.Windows) == ModifierKeys.Windows)
        {
            sb.Append("Windows+");
        }

        //sb.Append(GetLocalizedKeyString(this.Key));
        sb.Append(converter.ConvertToString(this.Key));
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
