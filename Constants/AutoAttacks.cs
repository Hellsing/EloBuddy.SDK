using System.Collections.Generic;
using System.Linq;

namespace EloBuddy.SDK.Constants
{
    /// <summary>
    /// Holds Auto Attack Information for special spells and auto reset spells.
    /// </summary>
    public static class AutoAttacks
    {
        internal static readonly HashSet<string> AutoAttackDatabase = new HashSet<string>
        {
            "caitlynheadshotmissile",
            "frostarrow",
            "garenslash2",
            "kennenmegaproc",
            "masteryidoublestrike",
            "quinnwenhanced",
            "renektonexecute",
            "renektonsuperexecute",
            "rengarnewpassivebuffdash",
            "trundleq",
            "viktorqbuff",
            "xenzhaothrust",
            "xenzhaothrust2",
            "xenzhaothrust3",
            "lucianpassiveshot"
        };
        /// <summary>
        /// List of special auto attacks. ie: Lucian Passive Shot, Ashe Q
        /// </summary>
        public static string[] AutoAttackSpells
        {
            get { return AutoAttackDatabase.ToArray(); }
        }

        internal static readonly HashSet<string> NoneAutoAttackDatabase = new HashSet<string>
        {
            "volleyattack",
            "volleyattackwithsound",
            "jarvanivcataclysmattack",
            "monkeykingdoubleattack",
            "shyvanadoubleattack",
            "shyvanadoubleattackdragon",
            "zyragraspingplantattack",
            "zyragraspingplantattack2",
            "zyragraspingplantattackfire",
            "zyragraspingplantattack2fire",
            "viktorpowertransfer",
            "sivirwattackbounce",
            "asheqattacknoonhit",
            "elisespiderlingbasicattack",
            "heimertyellowbasicattack",
            "heimertyellowbasicattack2",
            "heimertbluebasicattack",
            "annietibbersbasicattack",
            "annietibbersbasicattack2",
            "yorickdecayedghoulbasicattack",
            "yorickravenousghoulbasicattack",
            "yorickspectralghoulbasicattack", 
            "malzaharvoidlingbasicattack",
            "malzaharvoidlingbasicattack2",
            "malzaharvoidlingbasicattack3",
            "kindredwolfbasicattack",
            "gravesautoattackrecoil"
        };
        /// <summary>
        /// Auto Attacks that aren't directly linked to a champion.
        /// </summary>
        public static string[] NoneAutoAttackSpells
        {
            get { return NoneAutoAttackDatabase.ToArray(); }
        }
        internal static readonly HashSet<string> AutoAttackResetNamesDatabase = new HashSet<string>
        {
            "jaycehypercharge",
            "shyvanadoubleattack",
            "takedown",
            //"itemtiamatcleave",
            "itemtitanichydracleave"
        };

        internal static readonly Dictionary<Champion, HashSet<string>> SpecialAutoAttacksAnimationName = new Dictionary<Champion, HashSet<string>>
        {
            { Champion.Rengar, new HashSet<string> { "Spell5" } },
        };

        internal static readonly Dictionary<Champion, HashSet<string>> AutoAttackResetAnimationsName = new Dictionary<Champion, HashSet<string>>
        {
            { Champion.Graves, new HashSet<string> { "Spell3", "Spell3withReload" } },
        };
        internal static readonly Dictionary<Champion, SpellSlot> DashAutoAttackResetSlotsDatabase = new Dictionary<Champion, SpellSlot>
        {
            { Champion.Graves, SpellSlot.E },
            { Champion.Lucian, SpellSlot.E },
            { Champion.Riven, SpellSlot.Q },
            { Champion.Vayne, SpellSlot.Q }
        };
        private static readonly Dictionary<Champion, SpellSlot> AutoAttackResetSlotsDatabase = new Dictionary<Champion, SpellSlot>
        {
            { Champion.Ashe, SpellSlot.Q },
            { Champion.Blitzcrank, SpellSlot.E },
            { Champion.Darius, SpellSlot.W },
            { Champion.DrMundo, SpellSlot.E },
            { Champion.Fiora, SpellSlot.E },
            { Champion.Gangplank, SpellSlot.Q },
            { Champion.Garen, SpellSlot.Q },
            { Champion.Kassadin, SpellSlot.W },
            { Champion.Illaoi, SpellSlot.W },
            { Champion.Jax, SpellSlot.W },
            { Champion.Katarina, SpellSlot.E },
            { Champion.Leona, SpellSlot.Q },
            { Champion.MasterYi, SpellSlot.W },
            { Champion.Mordekaiser, SpellSlot.Q },
            { Champion.Nautilus, SpellSlot.W },
            { Champion.Nasus, SpellSlot.Q },
            { Champion.RekSai, SpellSlot.Q },
            { Champion.Renekton, SpellSlot.W },
            { Champion.Rengar, SpellSlot.Q },
            { Champion.Sejuani, SpellSlot.W },
            { Champion.Sivir, SpellSlot.W },
            { Champion.Talon, SpellSlot.Q },
            { Champion.Teemo, SpellSlot.Q },
            { Champion.Trundle, SpellSlot.Q },
            { Champion.Vi, SpellSlot.E },
            { Champion.Volibear, SpellSlot.Q },
            { Champion.MonkeyKing, SpellSlot.Q },
            { Champion.XinZhao, SpellSlot.Q },
            { Champion.Yorick, SpellSlot.Q }
        };

        /// <summary>
        /// Spells that can reset the auto attack timer.
        /// </summary>
        public static string[] AutoAttackResetSpells
        {
            get { return AutoAttackResetNamesDatabase.ToArray(); }
        }

        internal static readonly HashSet<Champion> UnabortableAutoDatabase = new HashSet<Champion>
        {
            Champion.Kalista
        };

        /// <summary>
        /// Champions that can't stop their auto attacks once they have started them.
        /// </summary>
        public static Champion[] UnabortableAutoChamps
        {
            get { return UnabortableAutoDatabase.ToArray(); }
        }

        /// <summary>
        /// Returns true if the spell is an Auto Attack.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool IsAutoAttack(this GameObjectProcessSpellCastEventArgs args)
        {
            return args.Target != null && args.SData.IsAutoAttack();
        }

        /// <summary>
        /// Returns true if the missile is an Auto Attack.
        /// </summary>
        /// <returns></returns>
        public static bool IsAutoAttack(this MissileClient missile)
        {
            return missile.Target != null && missile.SData.IsAutoAttack();
        }

        /// <summary>
        /// Returns true if the spell is an Auto Attack.
        /// </summary>
        /// <returns></returns>
        public static bool IsAutoAttack(this SpellData spellData)
        {
            return IsAutoAttack(spellData.Name);
        }

        /// <summary>
        /// Returns true if the spell is an Auto Attack.
        /// </summary>
        /// <returns></returns>
        public static bool IsAutoAttack(this SpellDataInst spellDataInst)
        {
            return IsAutoAttack(spellDataInst.Name);
        }

        /// <summary>
        /// Returns true if the spell is an Auto Attack.
        /// </summary>
        /// <returns></returns>
        public static bool IsAutoAttack(string spellName)
        {
            var spell = spellName.ToLower();
            return AutoAttackDatabase.Contains(spell) ||
                   (!NoneAutoAttackDatabase.Contains(spell) && spell.Contains("attack"));
        }

        /// <summary>
        /// Returns true if the spell is an Auto Attack Reset
        /// </summary>
        public static bool IsAutoAttackReset(AIHeroClient hero, GameObjectProcessSpellCastEventArgs args)
        {
            if (AutoAttackResetSlotsDatabase.ContainsKey(hero.Hero))
            {
                return AutoAttackResetSlotsDatabase[hero.Hero] == args.Slot;
            }
            return IsAutoAttackReset(args.SData.Name);
        }

        /// <summary>
        /// Returns true if the spell is a Dash Auto Attack Reset
        /// </summary>
        public static bool IsDashAutoAttackReset(AIHeroClient hero, GameObjectProcessSpellCastEventArgs args)
        {
            if (DashAutoAttackResetSlotsDatabase.ContainsKey(hero.Hero))
            {
                return DashAutoAttackResetSlotsDatabase[hero.Hero] == args.Slot;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the spell is a Dash Auto Attack Reset
        /// </summary>
        public static bool IsDashAutoAttackReset(AIHeroClient hero, GameObjectPlayAnimationEventArgs args)
        {
            return AutoAttackResetAnimationsName.ContainsKey(hero.Hero) && AutoAttackResetAnimationsName[hero.Hero].Contains(args.Animation);
        }

        /// <summary>
        /// Returns true if the spell is an Auto Attack Reset
        /// </summary>
        public static bool IsAutoAttackReset(string spellName)
        {
            return AutoAttackResetNamesDatabase.Contains(spellName.ToLower());
        }
    }
}
