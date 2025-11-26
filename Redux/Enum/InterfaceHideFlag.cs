using System;

namespace Redux.Enum
{
    [Flags]
    public enum InterfaceHideFlag : uint
    {
        Mentor = 1 << 4,
        ItemLock = 1 << 5
    }
}
