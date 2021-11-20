using System;
using System.Collections.Generic;

namespace PKHeX.Core
{
    /// <summary>
    /// Encounter Slot found in <see cref="GameVersion.BDSP"/>.
    /// </summary>
    /// <inheritdoc cref="EncounterSlot"/>
    public sealed record EncounterSlot8b : EncounterSlot
    {
        public override int Generation => 8;
        public bool IsUnderground => Area.Location is (>= 508 and <= 617);
        public bool IsMarsh => Area.Location is (>= 219 and <= 224);

        public EncounterSlot8b(EncounterArea area, int species, int form, int min, int max) : base(area, species, form, min, max)
        {
        }
        protected override void SetFormatSpecificData(PKM pk)
        {
            if (IsUnderground)
            {
                if (GetBaseEggMove(out int move1))
                    pk.RelearnMove1 = move1;
            }
            else if (IsMarsh)
            {
                pk.Ball = (int)Ball.Safari;
            }
            pk.SetRandomEC();
        }

        public bool CanBeUndergroundMove(int move)
        {
            var et = EvolutionTree.Evolves8b;
            var sf = et.GetBaseSpeciesForm(Species, Form);
            var species = sf & 0x7FF;
            var form = sf >> 11;
            if (IgnoreEggMoves.TryGetValue(species, out var exclude) && Array.IndexOf(exclude, move) != -1)
                return false;

            var baseEgg = MoveEgg.GetEggMoves(8, species, form, Version);
            return baseEgg.Length == 0 || Array.IndexOf(baseEgg, move) >= 0;
        }

        public bool GetBaseEggMove(out int move)
        {
            var et = EvolutionTree.Evolves8b;
            var sf = et.GetBaseSpeciesForm(Species, Form);
            var species = sf & 0x7FF;
            var form = sf >> 11;

            int[] Exclude = IgnoreEggMoves.TryGetValue(species, out var exclude) ? exclude : Array.Empty<int>();
            var baseEgg = MoveEgg.GetEggMoves(8, species, form, Version);
            if (baseEgg.Length == 0)
            {
                move = 0;
                return false;
            }

            var rnd = Util.Rand;
            while (true)
            {
                var index = rnd.Next(baseEgg.Length);
                move = baseEgg[index];
                if (Array.IndexOf(Exclude, move) == -1)
                    return true;
            }
        }

        private static readonly Dictionary<int, int[]> IgnoreEggMoves = new()
        {
            {004, new[] {394}}, // Charmander
            {016, new[] {403}}, // Pidgey
            {019, new[] {044}}, // Rattata
            {027, new[] {229}}, // Sandshrew
            {037, new[] {180,050,326}}, // Vulpix
            {050, new[] {310}}, // Diglett
            {056, new[] {370}}, // Mankey
            {058, new[] {242,336,394}}, // Growlithe
            {060, new[] {061,341}}, // Poliwag
            {066, new[] {282}}, // Machop
            {077, new[] {172}}, // Ponyta
            {079, new[] {428}}, // Slowpoke
            {083, new[] {348}}, // Farfetch�d
            {084, new[] {098,283}}, // Doduo
            {086, new[] {227}}, // Seel
            {098, new[] {175,021}}, // Krabby
            {102, new[] {235}}, // Exeggcute
            {108, new[] {187}}, // Lickitung
            {109, new[] {194}}, // Koffing
            {113, new[] {270}}, // Chansey
            {114, new[] {072}}, // Tangela
            {115, new[] {023,116}}, // Kangaskhan
            {116, new[] {225}}, // Horsea
            {122, new[] {102,298}}, // Mr. Mime
            {127, new[] {450,276}}, // Pinsir
            {133, new[] {204,343}}, // Eevee
            {140, new[] {341}}, // Kabuto
            {143, new[] {122,562}}, // Snorlax
            {147, new[] {349,407}}, // Dratini
            {152, new[] {267,312,034}}, // Chikorita
            {155, new[] {098,038}}, // Cyndaquil
            {158, new[] {242,037,056}}, // Totodile
            {161, new[] {179}}, // Sentret
            {170, new[] {175}}, // Chinchou
            {173, new[] {150}}, // Cleffa
            {179, new[] {036,268}}, // Mareep
            {183, new[] {276}}, // Marill
            {187, new[] {388}}, // Hoppip
            {190, new[] {103,097}}, // Aipom
            {191, new[] {073,275}}, // Sunkern
            {198, new[] {017,372}}, // Murkrow
            {200, new[] {180}}, // Misdreavus
            {204, new[] {038}}, // Pineco
            {206, new[] {246}}, // Dunsparce
            {209, new[] {242,423,424,422}}, // Snubbull
            {214, new[] {224}}, // Heracross
            {216, new[] {313}}, // Teddiursa
            {218, new[] {414}}, // Slugma
            {220, new[] {036}}, // Swinub
            {222, new[] {392}}, // Corsola
            {223, new[] {062}}, // Remoraid
            {226, new[] {056,469}}, // Mantine
            {227, new[] {065,413}}, // Skarmory
            {228, new[] {251,424}}, // Houndour
            {234, new[] {428}}, // Stantler
            {236, new[] {270}}, // Tyrogue
            {238, new[] {008}}, // Smoochum
            {252, new[] {283,437}}, // Treecko
            {255, new[] {179,297}}, // Torchic
            {261, new[] {281,389,583}}, // Poochyena
            {270, new[] {175,055}}, // Lotad
            {276, new[] {413}}, // Taillow
            {278, new[] {054,097}}, // Wingull
            {283, new[] {453}}, // Surskit
            {285, new[] {388,402}}, // Shroomish
            {296, new[] {197}}, // Makuhita
            {298, new[] {021}}, // Azurill
            {299, new[] {335}}, // Nosepass
            {300, new[] {252}}, // Skitty
            {302, new[] {212}}, // Sableye
            {303, new[] {389}}, // Mawile
            {304, new[] {442}}, // Aron
            {309, new[] {435,422}}, // Electrike
            {311, new[] {435,204}}, // Plusle
            {312, new[] {435,313}}, // Minun
            {313, new[] {227}}, // Volbeat
            {314, new[] {227}}, // Illumise
            {315, new[] {235}}, // Roselia
            {316, new[] {220,441}}, // Gulpin
            {320, new[] {034}}, // Wailmer
            {322, new[] {281}}, // Numel
            {324, new[] {284,499}}, // Torkoal
            {325, new[] {428}}, // Spoink
            {328, new[] {414}}, // Trapinch
            {336, new[] {400}}, // Seviper
            {339, new[] {330}}, // Barboach
            {341, new[] {283,282}}, // Corphish
            {345, new[] {072}}, // Lileep
            {352, new[] {050,492}}, // Kecleon
            {353, new[] {425,566}}, // Shuppet
            {357, new[] {437,235,349,692}}, // Tropius
            {359, new[] {389,195}}, // Absol
            {363, new[] {205}}, // Spheal
            {369, new[] {401}}, // Relicanth
            {370, new[] {392}}, // Luvdisc
            {387, new[] {074}}, // Turtwig
            {390, new[] {612}}, // Chimchar
            {393, new[] {056}}, // Piplup
            {399, new[] {111,205}}, // Bidoof
            {408, new[] {043}}, // Cranidos
            {417, new[] {608}}, // Pachirisu
            {418, new[] {401}}, // Buizel
            {422, new[] {262}}, // Shellos
            {425, new[] {194,366}}, // Drifloon
            {439, new[] {102,298}}, // Mime Jr.
            {442, new[] {425}}, // Spiritomb
            {443, new[] {225,328}}, // Gible
            {446, new[] {122}}, // Munchlax
            {449, new[] {303,328}}, // Hippopotas
            {451, new[] {400}}, // Skorupi
            {456, new[] {175}}, // Finneon
            {458, new[] {056,469}}, // Mantyke
            {459, new[] {054}}, // Snover
        };
    }
}
