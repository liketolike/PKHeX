﻿namespace PKHeX.Core
{
    /// <summary>
    /// Contains logic for the Generation 8b (BD/SP) roaming spawns.
    /// </summary>
    /// <remarks>
    /// Roaming encounters use the pokemon's 32-bit <see cref="PKM.EncryptionConstant"/> as RNG seed.
    /// </remarks>
    public static class Roaming8bRNG
    {
        private const int NoMatchIVs = -1;
        private const int UNSET = -1;

        public static void ApplyDetails(PKM pk, EncounterCriteria criteria, Shiny shiny = Shiny.FixedValue, int flawless = -1)
        {
            if (shiny == Shiny.FixedValue)
                shiny = criteria.Shiny is Shiny.Random or Shiny.Never ? Shiny.Never : Shiny.Always;
            if (flawless == -1)
                flawless = 0;

            int ctr = 0;
            const int maxAttempts = 50_000;
            var rnd = Util.Rand;
            do
            {
                var seed = Util.Rand32(rnd);
                if (TryApplyFromSeed(pk, criteria, shiny, flawless, seed))
                    return;
            } while (++ctr != maxAttempts);
            TryApplyFromSeed(pk, EncounterCriteria.Unrestricted, shiny, flawless, Util.Rand32(rnd));
        }

        private static bool TryApplyFromSeed(PKM pk, EncounterCriteria criteria, Shiny shiny, int flawless, uint seed)
        {
            var xoro = new Xoroshiro128Plus8b(seed);

            // Encryption Constant
            pk.EncryptionConstant = seed;
            var _ = xoro.NextUInt(); // fakeTID

            // PID
            var pid = xoro.NextUInt();
            if (shiny == Shiny.Never)
            {
                if (GetIsShiny(pk.TID, pk.SID, pid))
                    return false;
            }
            else if (shiny != Shiny.Random)
            {
                if (!GetIsShiny(pk.TID, pk.SID, pid))
                    return false;

                if (shiny == Shiny.AlwaysSquare && pk.ShinyXor != 0)
                    return false;
                if (shiny == Shiny.AlwaysStar && pk.ShinyXor == 0)
                    return false;
            }
            pk.PID = pid;

            // Check IVs: Create flawless IVs at random indexes, then the random IVs for not flawless.
            int[] ivs = { UNSET, UNSET, UNSET, UNSET, UNSET, UNSET };
            const int MAX = 31;
            var determined = 0;
            while (determined < flawless)
            {
                var idx = xoro.NextUInt(6);
                if (ivs[idx] != UNSET)
                    continue;
                ivs[idx] = 31;
                determined++;
            }

            for (var i = 0; i < ivs.Length; i++)
            {
                if (ivs[i] == UNSET)
                    ivs[i] = (int)xoro.NextUInt(MAX + 1);
            }

            if (!criteria.IsIVsCompatible(ivs, 8))
                return false;

            pk.IV_HP = ivs[0];
            pk.IV_ATK = ivs[1];
            pk.IV_DEF = ivs[2];
            pk.IV_SPA = ivs[3];
            pk.IV_SPD = ivs[4];
            pk.IV_SPE = ivs[5];

            // Ability
            pk.SetAbilityIndex((int)xoro.NextUInt(2));

            // Remainder
            var scale = (IScaledSize)pk;
            scale.HeightScalar = (int)xoro.NextUInt(0x81) + (int)xoro.NextUInt(0x80);
            scale.WeightScalar = (int)xoro.NextUInt(0x81) + (int)xoro.NextUInt(0x80);

            return true;
        }

        public static bool ValidateRoamingEncounter(PKM pk, Shiny shiny = Shiny.Random, int flawless = 0)
        {
            var seed = pk.EncryptionConstant;
            var xoro = new Xoroshiro128Plus8b(seed);

            // Check PID
            var _ = xoro.NextUInt(); // fakeTID
            var pid = xoro.NextUInt();
            if (pk.PID != pid)
                return false;

            // Check IVs: Create flawless IVs at random indexes, then the random IVs for not flawless.
            int[] ivs = { UNSET, UNSET, UNSET, UNSET, UNSET, UNSET };

            var determined = 0;
            while (determined < flawless)
            {
                var idx = xoro.NextUInt(6);
                if (ivs[idx] != UNSET)
                    continue;
                ivs[idx] = 31;
                determined++;
            }

            for (var i = 0; i < ivs.Length; i++)
            {
                if (ivs[i] == UNSET)
                    ivs[i] = (int)xoro.NextUInt(31 + 1);
            }

            if (ivs[0] != pk.GetIV(0)) return false;
            if (ivs[1] != pk.GetIV(1)) return false;
            if (ivs[2] != pk.GetIV(2)) return false;
            if (ivs[3] != pk.GetIV(4)) return false;
            if (ivs[4] != pk.GetIV(5)) return false;
            if (ivs[5] != pk.GetIV(3)) return false;

            // Don't check Hidden ability, as roaming encounters are 1/2 only.
            if (pk.AbilityNumber != (1 << (int)xoro.NextUInt(2)))
                return false;

            return GetIsMatchEnd(pk, xoro) || GetIsMatchEndWithCuteCharm(pk, xoro) || GetIsMatchEndWithSynchronize(pk, xoro);
        }

        private static bool GetIsMatchEnd(PKM pk, Xoroshiro128Plus8b xoro)
        {
            // Check that gender matches
            var genderRatio = PersonalTable.BDSP.GetFormEntry(pk.Species, pk.Form).Gender;
            if (genderRatio == PersonalInfo.RatioMagicGenderless)
            {
                if (pk.Gender != (int)Gender.Genderless)
                    return false;
            }
            else if (genderRatio == PersonalInfo.RatioMagicMale)
            {
                if (pk.Gender != (int)Gender.Male)
                    return false;
            }
            else if (genderRatio == PersonalInfo.RatioMagicFemale)
            {
                if (pk.Gender != (int)Gender.Female)
                    return false;
            }
            else
            {
                if (pk.Gender != (((int)xoro.NextUInt(253) + 1 < genderRatio) ? 1 : 0))
                    return false;
            }

            // Check that the nature matches
            if (pk.Nature != (int)xoro.NextUInt(25))
                return false;

            return GetIsHeightWeightMatch(pk, xoro);
        }

        private static bool GetIsMatchEndWithCuteCharm(PKM pk, Xoroshiro128Plus8b xoro)
        {
            // Check that gender matches
            // Assume that the gender is a match due to cute charm.

            // Check that the nature matches
            if (pk.Nature != (int)xoro.NextUInt(25))
                return false;

            return GetIsHeightWeightMatch(pk, xoro);
        }

        private static bool GetIsMatchEndWithSynchronize(PKM pk, Xoroshiro128Plus8b xoro)
        {
            // Check that gender matches
            var genderRatio = PersonalTable.BDSP.GetFormEntry(pk.Species, pk.Form).Gender;
            if (genderRatio == PersonalInfo.RatioMagicGenderless)
            {
                if (pk.Gender != (int)Gender.Genderless)
                    return false;
            }
            else if (genderRatio == PersonalInfo.RatioMagicMale)
            {
                if (pk.Gender != (int)Gender.Male)
                    return false;
            }
            else if (genderRatio == PersonalInfo.RatioMagicFemale)
            {
                if (pk.Gender != (int)Gender.Female)
                    return false;
            }
            else
            {
                if (pk.Gender != (((int)xoro.NextUInt(253) + 1 < genderRatio) ? 1 : 0))
                    return false;
            }

            // Assume that the nature is a match due to synchronize.

            return GetIsHeightWeightMatch(pk, xoro);
        }

        private static bool GetIsHeightWeightMatch(PKM pk, Xoroshiro128Plus8b xoro)
        {
            // Check height/weight
            if (pk is not IScaledSize s)
                return false;

            var height = xoro.NextUInt(0x81) + xoro.NextUInt(0x80);
            var weight = xoro.NextUInt(0x81) + xoro.NextUInt(0x80);
            return s.HeightScalar == height && s.WeightScalar == weight;
        }

        private static bool GetIsShiny(int tid, int sid, uint pid)
        {
            return GetShinyXor(pid, (uint)((sid << 16) | tid)) < 16;
        }

        private static uint GetShinyXor(uint pid, uint oid)
        {
            var xor = pid ^ oid;
            return (xor ^ (xor >> 16)) & 0xFFFF;
        }
    }
}
