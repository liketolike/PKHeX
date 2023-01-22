using System;

namespace PKHeX.Core;

/// <summary>
/// Details about moves in <see cref="EntityContext.Gen1"/>
/// </summary>
internal static class MoveInfo1
{
    public static ReadOnlySpan<byte> MovePP_RBY => new byte[]
    {
        00, 35, 25, 10, 15, 20, 20, 15, 15, 15, 35, 30, 05, 10, 30, 30, 35, 35, 20, 15,
        20, 20, 10, 20, 30, 05, 25, 15, 15, 15, 25, 20, 05, 35, 15, 20, 20, 20, 15, 30,
        35, 20, 20, 30, 25, 40, 20, 15, 20, 20, 20, 30, 25, 15, 30, 25, 05, 15, 10, 05,
        20, 20, 20, 05, 35, 20, 25, 20, 20, 20, 15, 20, 10, 10, 40, 25, 10, 35, 30, 15,
        20, 40, 10, 15, 30, 15, 20, 10, 15, 10, 05, 10, 10, 25, 10, 20, 40, 30, 30, 20,
        20, 15, 10, 40, 15, 20, 30, 20, 20, 10, 40, 40, 30, 30, 30, 20, 30, 10, 10, 20,
        05, 10, 30, 20, 20, 20, 05, 15, 10, 20, 15, 15, 35, 20, 15, 10, 20, 30, 15, 40,
        20, 15, 10, 05, 10, 30, 10, 15, 20, 15, 40, 40, 10, 05, 15, 10, 10, 10, 15, 30,
        30, 10, 10, 20, 10, 10,
    };
}
