﻿using System;

namespace PKHeX.Core
{
    public sealed class BoxLayout8b : SaveBlock, IBoxDetailName
    {
        public const int BoxCount = 40;

        private const int StringMaxLength = SAV6.LongStringLength / 2;
        public readonly int[] TeamSlots = new int[TeamCount * TeamSlotCount];
        private const int TeamNameLength = 0x16;

        public BoxLayout8b(SAV8BS sav, int offset) : base(sav) => Offset = offset;

        private static int GetBoxNameOffset(int box) => SAV6.LongStringLength * box;
        private static int GetTeamNameOffset(int box) => GetBoxNameOffset(BoxCount) + (TeamNameLength * box);

        public string GetBoxName(int box)
        {
            var boxName = SAV.GetString(Data, Offset + GetBoxNameOffset(box), SAV6.LongStringLength);
            if (string.IsNullOrEmpty(boxName))
                boxName = $"Box {box + 1}";
            return boxName;
        }

        public void SetBoxName(int box, string value)
        {
            var data = SAV.SetString(value, StringMaxLength, StringMaxLength, 0);
            var offset = Offset + GetBoxNameOffset(box);
            SAV.SetData(Data, data, offset);
        }

        public string GetTeamName(int team)
        {
            var boxName = SAV.GetString(Data, Offset + GetTeamNameOffset(team), TeamNameLength);
            if (string.IsNullOrEmpty(boxName))
                boxName = $"Team {team + 1}";
            return boxName;
        }

        public void SetTeamName(int team, string value)
        {
            var data = SAV.SetString(value, StringMaxLength, TeamNameLength / 2, 0);
            var offset = Offset + GetTeamNameOffset(team);
            SAV.SetData(Data, data, offset);
        }

        public string this[int i]
        {
            get => GetBoxName(i);
            set => SetBoxName(i, value);
        }

        public const int TeamPositionOffset = 0x5D4;
        public const int TeamCount = 6;
        public const int TeamSlotCount = 6;
        private const short NONE_SELECTED = -1;

        public void LoadBattleTeams()
        {
            for (int i = 0; i < TeamCount * TeamSlotCount; i++)
            {
                short val = BitConverter.ToInt16(Data, Offset + TeamPositionOffset + (i * 2));
                if (val < 0)
                {
                    TeamSlots[i] = NONE_SELECTED;
                    continue;
                }

                int box = val >> 8;
                int slot = val & 0xFF;
                int index = (SAV.BoxSlotCount * box) + slot;
                TeamSlots[i] = index & 0xFFFF;
            }
        }

        public void ClearBattleTeams()
        {
            for (int i = 0; i < TeamSlots.Length; i++)
                TeamSlots[i] = NONE_SELECTED;
            LockedTeam = 0;
        }

        public void SaveBattleTeams()
        {
            for (int i = 0; i < TeamCount * 6; i++)
            {
                int index = TeamSlots[i];
                if (index < 0)
                {
                    BitConverter.GetBytes((short)index).CopyTo(Data, Offset + TeamPositionOffset + (i * 2));
                    continue;
                }

                SAV.GetBoxSlotFromIndex(index, out var box, out var slot);
                int val = (box << 8) | slot;
                BitConverter.GetBytes((short)val).CopyTo(Data, Offset + TeamPositionOffset + (i * 2));
            }
        }

        public byte LockedTeam
        {
            get => Data[Offset + 0x61C];
            set
            {
                if (value > BoxCount)
                    value = BoxCount;
                Data[Offset + 0x61C] = value;
            }
        }

        public byte BoxesUnlocked
        {
            get => Data[Offset + 0x61D];
            set
            {
                if (value > BoxCount)
                    value = BoxCount;
                Data[Offset + 0x61D] = value;
            }
        }

        public byte CurrentBox
        {
            get => Data[Offset + 0x61E];
            set => Data[Offset + 0x61E] = value;
        }

        public int GetBoxWallpaperOffset(int box) => Offset + 0x620 + box;

        public int GetBoxWallpaper(int box)
        {
            if ((uint)box > BoxCount)
                return 0;
            return Data[GetBoxWallpaperOffset(box)];
        }

        public void SetBoxWallpaper(int box, int value)
        {
            if ((uint)box > BoxCount)
                return;
            Data[GetBoxWallpaperOffset(box)] = (byte)value;
        }

        public ushort StatusPut
        {
            get => BitConverter.ToUInt16(Data, Offset + 0x648);
            set => BitConverter.GetBytes(value).CopyTo(Data, Offset + 0x648);
        }
    }
}
