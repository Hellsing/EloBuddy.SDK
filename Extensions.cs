using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK.Spells;
using SharpDX;

// ReSharper disable SwitchStatementMissingSomeCases

// ReSharper disable MemberCanBePrivate.Global

namespace EloBuddy.SDK
{
    /// <summary>
    /// Misc
    /// </summary>
    public static partial class Extensions
    {
        #region GameObject Fast Checks

        /// <summary>
        /// Returns true if the source object is a minion.
        /// </summary>
        public static bool IsMinion(this Obj_AI_Base target)
        {
            return ObjectNames.Minions.Contains(target.BaseSkinName);
        }

        /// <summary>
        /// Returns true if the source object is a structure. Structures are: Turrets, Inhibs and Nexus's.
        /// </summary>
        public static bool IsStructure(this GameObject target)
        {
            var type = target.Type;
            return (type == GameObjectType.obj_AI_Turret || type == GameObjectType.obj_BarracksDampener ||
                    type == GameObjectType.obj_HQ);
        }

        /// <summary>
        /// Returns true if the source object is a ward.
        /// </summary>
        public static bool IsWard(this GameObject unit)
        {
            return unit.Type == GameObjectType.obj_Ward;
        }

        #endregion

        /// <summary>
        /// Returns the Auto Attack range of the unit given.
        /// </summary>
        public static float GetAutoAttackRange(this Obj_AI_Base source, AttackableUnit target = null)
        {
            var result = source.AttackRange + source.BoundingRadius +
                         (target != null ? (target.BoundingRadius - 30) : 35);
            var hero = source as AIHeroClient;
            if (hero != null && target != null)
            {
                switch (hero.Hero)
                {
                    case Champion.Caitlyn:
                        var targetBase = target as Obj_AI_Base;
                        if (targetBase != null && targetBase.HasBuff("caitlynyordletrapinternal"))
                        {
                            result += 650f;
                        }
                        break;
                }
            }
            else if (source is Obj_AI_Turret)
            {
                return 750f + source.BoundingRadius;
            }
            else if (source.BaseSkinName == "AzirSoldier")
            {
                result += Orbwalker.AzirSoldierAutoAttackRange - source.BoundingRadius;
            }
            return result;
        }

        /// <summary>
        /// Returns true if the source object and the target object have the same network id.
        /// </summary>
        public static bool IdEquals(this GameObject source, GameObject target)
        {
            if (source == null || target == null)
            {
                return false;
            }

            return source.NetworkId == target.NetworkId;
        }

        /// <summary>
        /// Returns a list where all elements of the original list are converted to the base class of Obj_AI_Base.
        /// </summary>
        public static List<Obj_AI_Base> ToObj_AI_BaseList<T>(this List<T> list) where T : Obj_AI_Base
        {
            return list.Cast<Obj_AI_Base>().ToList();
        }
        /// <summary>
        /// Returns a list where all elements of the original list are converted to the base class of Obj_AI_Base.
        /// </summary>
        public static List<Obj_AI_Base> ToObj_AI_BaseList<T>(this IEnumerable<T> list) where T : Obj_AI_Base
        {
            return list.Cast<Obj_AI_Base>().ToList();
        }
        /// <summary>
        /// Returns a IEnumerable where all elements of the original list are converted to the base class of Obj_AI_Base.
        /// </summary>
        public static IEnumerable<Obj_AI_Base> ToObj_AI_BaseEnumerable<T>(this IEnumerable<T> list) where T : Obj_AI_Base
        {
            return list.Cast<Obj_AI_Base>();
        }
    }

    /// <summary>
    /// Geometry
    /// </summary>
    public static partial class Extensions
    {
        #region SharpDX.Rectangle

        public static Rectangle Negate(this Rectangle rectangle)
        {
            return new Rectangle(-rectangle.X, -rectangle.Y, -rectangle.Width, -rectangle.Height);
        }

        public static Rectangle Add(this Rectangle rectangle1, Rectangle rectangle2)
        {
            return new Rectangle(rectangle1.X + rectangle2.X, rectangle1.Y + rectangle2.Y,
                rectangle1.Width + rectangle2.Width, rectangle1.Height + rectangle2.Height);
        }

        public static Rectangle Substract(this Rectangle rectangle1, Rectangle rectangle2)
        {
            return new Rectangle(rectangle1.X - rectangle2.X, rectangle1.Y - rectangle2.Y,
                rectangle1.Width - rectangle2.Width, rectangle1.Height - rectangle2.Height);
        }

        public static Rectangle Multiply(this Rectangle rectangle1, Rectangle rectangle2)
        {
            return new Rectangle(rectangle1.X * rectangle2.X, rectangle1.Y * rectangle2.Y,
                rectangle1.Width * rectangle2.Width, rectangle1.Height * rectangle2.Height);
        }

        public static Rectangle Divide(this Rectangle rectangle1, Rectangle rectangle2)
        {
            return new Rectangle(rectangle1.X / rectangle2.X, rectangle1.Y / rectangle2.Y,
                rectangle1.Width / rectangle2.Width, rectangle1.Height / rectangle2.Height);
        }

        public static bool IsInside(this Rectangle rectangle, Vector2 position)
        {
            return position.X >= rectangle.X && position.Y >= rectangle.Y &&
                   position.X < rectangle.BottomRight.X && position.Y < rectangle.BottomRight.Y;
        }

        public static bool IsCompletlyInside(this Rectangle rectangle1, Rectangle rectangle2)
        {
            return rectangle2.X >= rectangle1.X && rectangle2.Y >= rectangle1.Y &&
                   rectangle2.BottomRight.X <= rectangle1.BottomRight.X &&
                   rectangle2.BottomRight.Y <= rectangle1.BottomRight.Y;
        }

        public static bool IsPartialInside(this Rectangle rectangle1, Rectangle rectangle2)
        {
            return rectangle2.X >= rectangle1.X && rectangle2.X <= rectangle1.BottomRight.X ||
                   rectangle2.Y >= rectangle1.Y && rectangle2.Y <= rectangle1.BottomRight.Y;
        }

        public static bool IsNear(this Rectangle rectangle, Vector2 position, int distance)
        {
            return
                new Rectangle(rectangle.X - distance, rectangle.Y - distance, rectangle.Width + distance,
                    rectangle.Height + distance).IsInside(position);
        }

        #endregion

        #region Circle Intersection by UzumakiBoruto

        public static Vector2[] CirclesIntersection(this Vector2 center1, float radius1, Vector2 center2, float radius2)
        {
            var Distance = center1.Distance(center2);
            if (Distance > radius1 + radius2 || (Distance <= Math.Abs(radius1 - radius2)))
            {
                return new Vector2[] { };
            }

            var A = (radius1 * radius1 - radius2 * radius2 + Distance * Distance) / (2 * Distance);
            var H = (float)Math.Sqrt(radius1 * radius1 - A * A);
            var Direction = (center2 - center1).Normalized();
            var PA = center1 + A * Direction;
            var Loc1 = PA + H * Direction.Perpendicular();
            var Loc2 = PA - H * Direction.Perpendicular();
            return new[] { Loc1, Loc2 };
        }

        public static Vector2[] CirclesIntersection(this Vector3 center1, float radius1, Vector3 center2, float radius2)
        {
            var Pos1 = center1.To2D();
            var Pos2 = center2.To2D();
            return CirclesIntersection(Pos1, radius1, Pos2, radius2);
        }

        public static Vector2[] CirclesIntersection(this Geometry.Polygon.Circle circle1, Geometry.Polygon.Circle circle2)
        {
            return CirclesIntersection(circle1.Center, circle1.Radius, circle2.Center, circle2.Radius);
        }

        #endregion Circle Intersection by UzumakiBoruto
    }

    /// <summary>
    /// Numeric and Math
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Returns the given number raised to the power of two.
        /// </summary>
        public static int Pow(this int number)
        {
            return number * number;
        }

        /// <summary>
        /// Returns the given number raised to the power of two.
        /// </summary>
        public static uint Pow(this uint number)
        {
            return number * number;
        }

        /// <summary>
        /// Returns the given number raised to the power of two.
        /// </summary>
        public static double Pow(this double number)
        {
            return number * number;
        }

        /// <summary>
        /// Returns the given number raised to the power of two.
        /// </summary>
        public static float Pow(this float number)
        {
            return number * number;
        }

        /// <summary>
        /// Returns the square root of the given number.
        /// </summary>
        public static double Sqrt(this int number)
        {
            return Math.Sqrt(number);
        }

        /// <summary>
        /// Returns the square root of the given number.
        /// </summary>
        public static double Sqrt(this uint number)
        {
            return Math.Sqrt(number);
        }

        /// <summary>
        /// Returns the square root of the given number.
        /// </summary>
        public static double Sqrt(this double number)
        {
            return Math.Sqrt(number);
        }

        /// <summary>
        /// Returns the square root of the given number.
        /// </summary>
        public static double Sqrt(this float number)
        {
            return Math.Sqrt(number);
        }
    }

    /// <summary>
    /// Buffs
    /// </summary>
    public static partial class Extensions
    {
        // http://leagueoflegends.wikia.com/wiki/Crowd_control
        private static readonly HashSet<BuffType> BlockedMovementBuffTypes = new HashSet<BuffType>
        {
            BuffType.Knockup,
            BuffType.Knockback,
            BuffType.Charm,
            BuffType.Fear,
            BuffType.Flee,
            BuffType.Taunt,
            BuffType.Snare,
            BuffType.Stun,
            BuffType.Suppression,
        };

        [Obsolete("GetMovementDebuffDuration is deprecated, please use GetMovementBlockedDebuffDuration instead.")]
        public static float GetMovementDebuffDuration(this Obj_AI_Base target)
        {
            return
                target.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && BlockedMovementBuffTypes.Contains(b.Type))
                    .Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) -
                Game.Time;
        }

        public static float GetMovementBlockedDebuffDuration(this Obj_AI_Base target)
        {
            return
                target.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && BlockedMovementBuffTypes.Contains(b.Type))
                    .Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) -
                Game.Time;
        }

        private static readonly HashSet<BuffType> ReducedMovementBuffTypes = new HashSet<BuffType>
        {
            BuffType.Slow,
            BuffType.Polymorph
        };

        public static float GetMovementReducedDebuffDuration(this Obj_AI_Base target)
        {
            return
                target.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && ReducedMovementBuffTypes.Contains(b.Type))
                    .Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) -
                Game.Time;
        }
    }

    /// <summary>
    /// Booleans, like IsXXX
    /// </summary>
    public static partial class Extensions
    {
        #region Validation

        /// <summary>
        /// Returns true if the buff is not null.
        /// </summary>
        public static bool IsValid(this BuffInstance buffInstance)
        {
            return buffInstance != null &&
                   buffInstance.IsValid &&
                   buffInstance.EndTime - Game.Time > 0;
        }

        /// <summary>
        /// Returns true if the vector is not zero.
        /// </summary>
        public static bool IsValid(this Vector3 vector, bool checkWorldCoords = false)
        {
            return IsValid(vector.To2D(), checkWorldCoords);
        }

        /// <summary>
        /// Returns true if the vector is not zero.
        /// </summary>
        public static bool IsValid(this Vector2 vector, bool checkWorldCoords = false)
        {
            if (vector.IsZero)
            {
                return false;
            }

            if (checkWorldCoords)
            {
                var navMeshCoords = vector.WorldToGrid();
                return navMeshCoords.X >= 0 && navMeshCoords.X <= NavMesh.Width &&
                       navMeshCoords.Y >= 0 && navMeshCoords.Y <= NavMesh.Height;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the unit is effected by Aatrox passive, Fiora W, Tryndamere R, Vladimir W, Kayle R, Kindred R, Zilean R.
        /// </summary>
        public static bool HasUndyingBuff(this AIHeroClient target, bool addHealthCheck = false)
        {
            switch (target.Hero)
            {
                case Champion.Aatrox:
                    if (target.HasBuff("aatroxpassivedeath"))
                    {
                        return true;
                    }
                    break;
                case Champion.Fiora:
                    if (target.HasBuff("FioraW"))
                    {
                        return true;
                    }
                    break;
                case Champion.Tryndamere:
                    if (target.HasBuff("UndyingRage") && (!addHealthCheck || target.Health <= 30))
                    {
                        return true;
                    }
                    break;

                case Champion.Vladimir:
                    if (target.HasBuff("VladimirSanguinePool"))
                    {
                        return true;
                    }
                    break;
            }

            if (EntityManager.Heroes.ContainsKayle && target.HasBuff("JudicatorIntervention"))
            {
                return true;
            }

            if (EntityManager.Heroes.ContainsKindred &&
                target.HasBuff("kindredrnodeathbuff") && (!addHealthCheck || target.HealthPercent <= 10))
            {
                return true;
            }

            if (EntityManager.Heroes.ContainsZilean && (target.HasBuff("ChronoShift") ||
                                                        target.HasBuff("chronorevive")) &&
                (!addHealthCheck || target.HealthPercent <= 10))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the target is not null, alive, visible, targetable, and not invulnerable.
        /// </summary>
        public static bool IsValidTarget(
            this AttackableUnit target,
            float? range = null,
            bool onlyEnemyTeam = false,
            Vector3? rangeCheckFrom = null)
        {
            if (target == null || !target.IsValid || target.IsDead || !target.IsVisible
                /* TODO: Check if IsVisible is correct */|| !target.IsTargetable || target.IsInvulnerable)
            {
                return false;
            }

            if (onlyEnemyTeam && Player.Instance.Team == target.Team)
            {
                return false;
            }

            var baseObject = target as Obj_AI_Base;
            if (baseObject != null && !baseObject.IsHPBarRendered)
            {
                return false;
            }
            if (range.HasValue)
            {
                range = range.Value.Pow();
                var unitPosition = baseObject != null ? baseObject.ServerPosition : target.Position;
                return rangeCheckFrom.HasValue
                    ? rangeCheckFrom.Value.Distance(unitPosition, true) < range
                    : Player.Instance.ServerPosition.DistanceSquared(unitPosition) < range;
            }
            return true;
        }

        /// <summary>
        /// Returns true if the missile is not null and if the missiles target is not null. If check target is true: check if the missiles position is in range of the targets bounding box.
        /// </summary>
        public static bool IsValidMissile(this MissileClient source, bool checkTarget = true)
        {
            return ObjectManager.Get<MissileClient>().Count(w => w.MemoryAddress == source.MemoryAddress) == 1 &&
                   source.IsValid &&
                   (source.Target == null ||
                    (source.Target.IsValid &&
                     (!checkTarget || !source.IsInRange(source.Target, source.Target.BoundingRadius))));
        }

        #endregion

        /// <summary>
        /// Returns true if the unit is in range of the fountain of the specified team.
        /// </summary>
        public static bool IsInFountainRange(this Obj_AI_Base hero, bool enemyFountain = false)
        {
            return hero.IsHPBarRendered && ObjectManager.Get<Obj_SpawnPoint>().Any(s => (enemyFountain ? s.Team != hero.Team : s.Team == hero.Team) && s.Team != hero.Team && hero.IsInRange(s, 1250));
        }

        /// <summary>
        /// Returns true if the unit is in shop range.
        /// </summary>
        public static bool IsInShopRange(this AIHeroClient hero)
        {
            return hero.IsVisible && ObjectManager.Get<Obj_Shop>().Any(s => hero.IsInRange(s, 1250));
        }

        /// <summary>
        /// Returns true if the unit is recalling.
        /// </summary>
        public static bool IsRecalling(this AIHeroClient unit)
        {
            return unit.Buffs.Any(buff => buff.Type == BuffType.Aura && buff.Name.ToLower().Contains("recall"));
        }

        /// <summary>
        /// Returns the direction the unit is facing.
        /// </summary>
        public static Vector2 Direction(this Obj_AI_Base source)
        {
            const bool finnPlease = true;
            Vector2 position = source.Direction.To2D();
			return (finnPlease ? position : position.Perpendicular()).Normalized();
        }

        /// <summary>
        /// Returns true if the original unit is facing within 90 degrees of the specified position.
        /// </summary>
        public static bool IsFacing(this Obj_AI_Base source, Vector3 position)
        {
            return (source != null && source.IsValid &&
                    source.Direction().AngleBetween((position - source.Position).To2D()) < 90);
        }

        /// <summary>
        /// Returns true if the original unit is facing within 90 degrees of the second unit.
        /// </summary>
        public static bool IsFacing(this Obj_AI_Base source, Obj_AI_Base target)
        {
            return source.IsFacing(target.Position);
        }

        /// <summary>
        /// Returns true if the original position perpendicular angle is within 90 degrees.
        /// </summary>
        public static bool IsFacing(this Vector3 position1, Vector3 position2)
        {
            return position1.To2D().Perpendicular().AngleBetween((position2 - position1).To2D()) < 90;
        }

        /// <summary>
        /// Returns true if both units are facing the same direction.
        /// </summary>
        public static bool IsBothFacing(this Obj_AI_Base source, Obj_AI_Base target)
        {
            return source.IsFacing(target) && target.IsFacing(source);
        }

        /// <summary>
        /// Checks if the target is not null and if the unit is valid.
        /// </summary>
        public static bool IsValid(this Obj_AI_Base unit)
        {
            return unit != null && unit.IsValid;
        }

        /// <summary>
        /// Returns true if target is in the Auto Attack range of the original object.
        /// </summary>
        public static bool IsInAutoAttackRange(this Obj_AI_Base source, AttackableUnit target)
        {
            var hero = source as AIHeroClient;
            if (hero != null)
            {
                switch (hero.Hero)
                {
                    case Champion.Azir:
                        if (hero.IsMe && Orbwalker.ValidAzirSoldiers.Any(i => i.IsInAutoAttackRange(target)))
                        {
                            return true;
                        }
                        break;
                }
            }
            return source.IsInRange(target, GetAutoAttackRange(source, target));
        }

        /// <summary>
        /// Returns true if the team is the same as the Player's team.
        /// </summary>
        public static bool IsAlly(this GameObjectTeam team)
        {
            return team == Player.Instance.Team;
        }

        /// <summary>
        /// Returns true if the team is opposite of the Player's team.
        /// </summary>
        public static bool IsEnemy(this GameObjectTeam team)
        {
            return team != Player.Instance.Team;
        }

        /// <summary>
        /// Returns true if the team is Neutral.
        /// </summary>
        public static bool IsNeutral(this GameObjectTeam team)
        {
            return team == GameObjectTeam.Neutral;
        }
    }

    /// <summary>
    /// Spells related
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Returns the range of the spell with the largest range.
        /// </summary>
        public static int GetHighestSpellRange(this AIHeroClient target)
        {
            var qSpell = target.Spellbook.GetSpell(SpellSlot.Q);
            var wSpell = target.Spellbook.GetSpell(SpellSlot.W);
            var eSpell = target.Spellbook.GetSpell(SpellSlot.E);
            var rSpell = target.Spellbook.GetSpell(SpellSlot.R);

            if (qSpell == null || wSpell == null || eSpell == null || rSpell == null)
            {
                return 0;
            }

            var spellList = new List<SpellDataInst>
            {
                qSpell,
                wSpell,
                eSpell,
                rSpell
            };

            var highestSpell = spellList.OrderByDescending(spell => spell.SData.CastRangeDisplayOverride > 0 ? spell.SData.CastRangeDisplayOverride : spell.SData.CastRange).FirstOrDefault();
            if (highestSpell != null)
            {
                return (int) (highestSpell.SData.CastRangeDisplayOverride > 0 ? highestSpell.SData.CastRangeDisplayOverride : highestSpell.SData.CastRange);
            }
            return 0;
        }

        /// <summary>
        /// Returns the range of the spell with the lowest range.
        /// </summary>
        public static int GetLowestSpellRange(this AIHeroClient target)
        {
            var qSpell = target.Spellbook.GetSpell(SpellSlot.Q);
            var wSpell = target.Spellbook.GetSpell(SpellSlot.W);
            var eSpell = target.Spellbook.GetSpell(SpellSlot.E);
            var rSpell = target.Spellbook.GetSpell(SpellSlot.R);

            if (qSpell == null || wSpell == null || eSpell == null || rSpell == null)
            {
                return 0;
            }

            var spellList = new List<SpellDataInst>
            {
                qSpell,
                wSpell,
                eSpell,
                rSpell
            };

            var highestSpell = spellList.OrderBy(spell => spell.SData.CastRangeDisplayOverride > 0 ? spell.SData.CastRangeDisplayOverride : spell.SData.CastRange).FirstOrDefault();
            if (highestSpell != null)
            {
                return (int)(highestSpell.SData.CastRangeDisplayOverride > 0 ? highestSpell.SData.CastRangeDisplayOverride : highestSpell.SData.CastRange);
            }
            return 0;
        }

        /// <summary>
        /// Returns the SpellDataInst of a spell if the target has the spell given.
        /// </summary>
        public static SpellDataInst GetSpellDataFromName(this AIHeroClient target, string spellName)
        {
            return target.Spellbook.Spells.FirstOrDefault(spell => string.Equals(spell.Name, spellName, StringComparison.CurrentCultureIgnoreCase));
        }
    }

    /// <summary>
    /// SpellSlot
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Returns the SpellSlot of the Spell given if the target has the spell. Else returns SpellSlot.Unknown
        /// </summary>
        public static SpellSlot GetSpellSlotFromName(this AIHeroClient target, string spellName)
        {
            foreach (
                var spell in
                target.Spellbook.Spells.Where(
                    spell => string.Equals(spell.Name, spellName, StringComparison.CurrentCultureIgnoreCase)))
            {
                return spell.Slot;
            }
            return SpellSlot.Unknown;
        }

        /// <summary>
        /// Returns the SpellSlot of the SummonerSpell given if the target has the spell. Else returns SpellSlot.Unknown. 
        /// </summary>
        /// <param name="target">The unit to get the SpellSlot from</param>
        /// <param name="spellName">Summoner Spell name. Casing does not matter.</param>
        /// <returns></returns>
        public static SpellSlot FindSummonerSpellSlotFromName(this AIHeroClient target, string spellName)
        {
            foreach (
                var spell in
                target.Spellbook.Spells.Where(
                    spell =>
                        (spell.Slot == SpellSlot.Summoner1 || spell.Slot == SpellSlot.Summoner2) &&
                        spell.Name.ToLower().Contains(spellName.ToLower())))
            {
                return spell.Slot;
            }
            return SpellSlot.Unknown;
        }
    }

    /// <summary>
    /// Item Related
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Returns true if the inventory has any of the items given.
        /// </summary>
        public static bool HasItem(this IEnumerable<InventorySlot> inventory, params int[] itemIds)
        {
            return inventory.Any(itemId => itemIds.Contains((int) itemId.Id));
        }

        /// <summary>
        /// Returns true if the inventory has any of the items given.
        /// </summary>
        public static bool HasItem(this IEnumerable<InventorySlot> inventory, params ItemId[] items)
        {
            return inventory.Any(itemId => items.Contains(itemId.Id));
        }

        /// <summary>
        /// Returns true if the target has any of the items given.
        /// </summary>
        public static bool HasItem(this Obj_AI_Base target, params int[] itemIds)
        {
            return target.InventoryItems.HasItem(itemIds);
        }

        /// <summary>
        /// Returns true if the target has any of the items given.
        /// </summary>
        public static bool HasItem(this Obj_AI_Base target, params ItemId[] itemIds)
        {
            return target.InventoryItems.HasItem(itemIds);
        }

        /// <summary>
        /// Returns the first InventorySlot of the Item if the Inventory has any of the items given. Returns null if the item does not exist.
        /// </summary>
        public static InventorySlot GetItemSlot(this IEnumerable<InventorySlot> inventory, params int[] itemIds)
        {
            return inventory.FirstOrDefault(itemId => itemIds.Contains((int)itemId.Id));
        }

        /// <summary>
        /// Returns the first InventorySlot of the Item if the Inventory has any of the items given. Returns null if the item does not exist.
        /// </summary>
        public static InventorySlot GetItemSlot(this IEnumerable<InventorySlot> inventory, params ItemId[] items)
        {
            return inventory.FirstOrDefault(itemId => items.Contains(itemId.Id));
        }

        /// <summary>
        /// Returns the first InventorySlot of the Item if the target has any of the items given. Returns null if the item does not exist.
        /// </summary>
        public static InventorySlot GetItemSlot(this Obj_AI_Base target, params int[] itemIds)
        {
            return target.InventoryItems.GetItemSlot(itemIds);
        }

        /// <summary>
        /// Returns the first InventorySlot of the Item if the target has any of the items given. Returns null if the item does not exist.
        /// </summary>
        public static InventorySlot GetItemSlot(this Obj_AI_Base target, params ItemId[] itemIds)
        {
            return target.InventoryItems.GetItemSlot(itemIds);
        }
    }

    /// <summary>
    /// Turret Related
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Returns the last target a turret has attacked.
        /// </summary>
        public static Obj_AI_Base LastTarget(this Obj_AI_Turret turret)
        {
            if (!Orbwalker.LastTargetTurrets.ContainsKey(turret.NetworkId))
            {
                Orbwalker.LastTargetTurrets[turret.NetworkId] = null;
            }
            else if (Orbwalker.LastTargetTurrets[turret.NetworkId] != null &&
                     !Orbwalker.LastTargetTurrets[turret.NetworkId].IsValidTarget())
            {
                Orbwalker.LastTargetTurrets[turret.NetworkId] = null;
            }
            return Orbwalker.LastTargetTurrets[turret.NetworkId];
        }
    }

        /// <summary>
        /// Health Related
        /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Returns the amount of health the unit has. Includes Skaarl's HP.
        /// </summary>
        public static float TotalHealth(this Obj_AI_Base target)
        {
            var result = target.Health;
            var hero = target as AIHeroClient;
            if (hero != null)
            {
                switch (hero.Hero)
                {
                    case Champion.Kled:
                        result += target.KledSkaarlHP;
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the amount of maximum amount of health the unit can have. Includes Skaarl's HP.
        /// </summary>
        public static float TotalMaxHealth(this Obj_AI_Base target)
        {
            var result = target.MaxHealth;
            var hero = target as AIHeroClient;
            if (hero != null)
            {
                switch (hero.Hero)
                {
                    case Champion.Kled:
                        result += target.MaxKledSkaarlHP;
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the amount of missing health the unit has.
        /// </summary>
        public static float TotalMissingHealth(this Obj_AI_Base target)
        {
            return target.TotalMaxHealth() - target.TotalHealth();
        }

        /// <summary>
        /// Returns the total amount of shields the unit has. 
        /// </summary>
        public static float TotalShield(this Obj_AI_Base target)
        {
            var result = target.AllShield + target.AttackShield + target.MagicShield;
            var hero = target as AIHeroClient;
            if (hero != null)
            {
                switch (hero.Hero)
                {
                    case Champion.Blitzcrank:
                        if (!target.HasBuff("BlitzcrankManaBarrierCD") && !target.HasBuff("ManaBarrier"))
                        {
                            result += target.Mana / 2;
                        }
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the amount of health the unit has and total amount of shields it has.
        /// </summary>
        public static float TotalShieldHealth(this Obj_AI_Base target)
        {
            return target.TotalHealth() + target.TotalShield();
        }

        /// <summary>
        /// Returns the amount of maximum health the unit has and total amount of shields it has.
        /// </summary>
        public static float TotalShieldMaxHealth(this Obj_AI_Base target)
        {
            return target.TotalMaxHealth() + target.TotalShield();
        }

        /// <summary>
        /// Returns the percent of HP the unit is missing as a decimal.
        /// </summary>
        public static float MissingHealthPercent(this Obj_AI_Base target)
        {
            return target.TotalHealth() / target.TotalMaxHealth();
        }

        /// <summary>
        /// Returns true if the unit is not dead.
        /// </summary>
        public static bool IsAlive(this GameObject unit)
        {
            return !unit.IsDead;
        }
    }

    /// <summary>
    /// Vectors
    /// </summary>
    public static partial class Extensions
    {
        #region Distance

        public static float Distance(this Obj_AI_Base target1, GameObject target2, bool squared = false)
        {
            return Distance(target1.ServerPosition.To2D(), target2.Position.To2D(), squared);
        }

        public static float Distance(this Obj_AI_Base target, Vector3 pos, bool squared = false)
        {
            return Distance(target.ServerPosition.To2D(), pos.To2D(), squared);
        }

        public static float Distance(this Obj_AI_Base target, Vector2 pos, bool squared = false)
        {
            return Distance(target.ServerPosition.To2D(), pos, squared);
        }

        public static float Distance(this Obj_AI_Base target1, Obj_AI_Base target2, bool squared = false)
        {
            return Distance(target1.ServerPosition.To2D(), target2.ServerPosition.To2D(), squared);
        }

        public static float Distance(this GameObject target1, Obj_AI_Base target2, bool squared = false)
        {
            return Distance(target1.Position.To2D(), target2.ServerPosition.To2D(), squared);
        }

        public static float Distance(this GameObject target, Vector3 pos, bool squared = false)
        {
            return Distance(target.Position.To2D(), pos.To2D(), squared);
        }

        public static float Distance(this GameObject target, Vector2 pos, bool squared = false)
        {
            return Distance(target.Position.To2D(), pos, squared);
        }

        public static float Distance(this GameObject target1, GameObject target2, bool squared = false)
        {
            return Distance(target1.Position.To2D(), target2.Position.To2D(), squared);
        }

        public static float Distance(this Vector3 pos, Obj_AI_Base target, bool squared = false)
        {
            return Distance(pos.To2D(), target.ServerPosition.To2D(), squared);
        }

        public static float Distance(this Vector3 pos, GameObject target, bool squared = false)
        {
            return Distance(pos.To2D(), target.Position.To2D(), squared);
        }

        public static float Distance(this Vector3 pos1, Vector2 pos2, bool squared = false)
        {
            return Distance(pos1.To2D(), pos2, squared);
        }

        public static float Distance(this Vector3 pos1, Vector3 pos2, bool squared = false)
        {
            return Distance(pos1.To2D(), pos2.To2D(), squared);
        }

        public static float Distance(this Vector2 pos, Obj_AI_Base target, bool squared = false)
        {
            return Distance(pos, target.ServerPosition.To2D(), squared);
        }

        public static float Distance(this Vector2 pos, GameObject target, bool squared = false)
        {
            return Distance(pos, target.Position.To2D(), squared);
        }

        public static float Distance(this Vector2 pos1, Vector3 pos2, bool squared = false)
        {
            return Distance(pos1, pos2.To2D(), squared);
        }

        public static float Distance(this Vector2 pos1, Vector2 pos2, bool squared = false)
        {
            if (squared)
            {
                return Vector2.DistanceSquared(pos1, pos2);
            }
            else
            {
                return Vector2.Distance(pos1, pos2);
            }
        }

        public static float Distance(this Vector2 point, Vector2 segmentStart, Vector2 segmentEnd, bool squared = false)
        {
            var a =
                Math.Abs((segmentEnd.Y - segmentStart.Y) * point.X - (segmentEnd.X - segmentStart.X) * point.Y +
                         segmentEnd.X * segmentStart.Y - segmentEnd.Y * segmentStart.X);
            return (squared ? a.Pow() : a) / segmentStart.Distance(segmentEnd, squared);
        }

        public static float Distance(this Vector3 point, Vector2 segmentStart, Vector2 segmentEnd, bool squared = false)
        {
            return point.To2D().Distance(segmentStart, segmentEnd, squared);
        }

        public static float DistanceSquared(this Vector2 pos1, Vector2 pos2)
        {
            return Vector2.DistanceSquared(pos1, pos2);
        }

        public static float DistanceSquared(this Vector3 pos1, Vector3 pos2)
        {
            return DistanceSquared(pos1.To2D(), pos2.To2D());
        }

        public static float DistanceSquared(this Vector2 pos1, Vector3 pos2)
        {
            return DistanceSquared(pos1, pos2.To2D());
        }

        public static float DistanceSquared(this Vector3 pos1, Vector2 pos2)
        {
            return DistanceSquared(pos1.To2D(), pos2);
        }

        public static bool IsInRange(this Vector2 source, Vector2 target, float range)
        {
            return source.Distance(target, true) < range.Pow();
        }

        public static bool IsInRange(this Vector2 source, Vector3 target, float range)
        {
            return IsInRange(source, target.To2D(), range);
        }

        public static bool IsInRange(this Vector2 source, GameObject target, float range)
        {
            return IsInRange(source, target.Position.To2D(), range);
        }

        public static bool IsInRange(this Vector2 source, Obj_AI_Base target, float range)
        {
            return IsInRange(source, target.ServerPosition.To2D(), range);
        }

        public static bool IsInRange(this Vector3 source, Vector2 target, float range)
        {
            return IsInRange(source.To2D(), target, range);
        }

        public static bool IsInRange(this Vector3 source, Vector3 target, float range)
        {
            return IsInRange(source.To2D(), target, range);
        }

        public static bool IsInRange(this Vector3 source, GameObject target, float range)
        {
            return IsInRange(source.To2D(), target, range);
        }

        public static bool IsInRange(this Vector3 source, Obj_AI_Base target, float range)
        {
            return IsInRange(source.To2D(), target, range);
        }

        public static bool IsInRange(this GameObject source, Vector2 target, float range)
        {
            return IsInRange(source.Position.To2D(), target, range);
        }

        public static bool IsInRange(this GameObject source, Vector3 target, float range)
        {
            return IsInRange(source.Position.To2D(), target, range);
        }

        public static bool IsInRange(this GameObject source, GameObject target, float range)
        {
            return IsInRange(source.Position.To2D(), target, range);
        }

        public static bool IsInRange(this GameObject source, Obj_AI_Base target, float range)
        {
            return IsInRange(source.Position.To2D(), target, range);
        }

        public static bool IsInRange(this Obj_AI_Base source, Vector2 target, float range)
        {
            return IsInRange(source.ServerPosition.To2D(), target, range);
        }

        public static bool IsInRange(this Obj_AI_Base source, Vector3 target, float range)
        {
            return IsInRange(source.ServerPosition.To2D(), target, range);
        }

        public static bool IsInRange(this Obj_AI_Base source, GameObject target, float range)
        {
            return IsInRange(source.ServerPosition.To2D(), target, range);
        }

        public static bool IsInRange(this Obj_AI_Base source, Obj_AI_Base target, float range)
        {
            return IsInRange(source.ServerPosition.To2D(), target, range);
        }

        #endregion

        #region Vector Conversions

        /// <summary>
        /// Changes List of Vector3 to List of Vector2 by removing the Z axis.
        /// </summary>
        public static List<Vector2> To2D(this List<Vector3> points)
        {
            var l = new List<Vector2>();
            l.AddRange(points.Select(v => v.To2D()));
            return l;
        }

        /// <summary>
        /// Changes Vector3 to Vector2 by removing the Z axis.
        /// </summary>
        public static Vector2 To2D(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        /// <summary>
        /// Changes Vector2 to Vector3 by adding the height parameter as it's Z axis.
        /// </summary>
        public static Vector3 To3D(this Vector2 vector, int height = 0)
        {
            return new Vector3(vector.X, vector.Y, height);
        }

        /// <summary>
        /// Changes Vector2 to Vector3 by setting Z equal to the height of the land at the specified coordinates.
        /// </summary>
        public static Vector3 To3DWorld(this Vector2 vector)
        {
            return new Vector3(vector.X, vector.Y, NavMesh.GetHeightForPosition(vector.X, vector.Y));
        }

        /// <summary> 
        /// Normalizes a Vector2. This results in an angle with a length of 1.
        /// </summary>
        public static Vector2 Normalized(this Vector2 vector)
        {
            return Vector2.Normalize(vector);
        }

        /// <summary> 
        /// Normalizes a Vector3. This results in an angle with a length of 1.
        /// </summary>
        public static Vector3 Normalized(this Vector3 vector)
        {
            return Vector3.Normalize(vector);
        }

        /// <summary> 
        /// Converts a World point to it's Screen equivalent.
        /// </summary>
        public static Vector2 WorldToScreen(this Vector3 vector)
        {
            return Drawing.WorldToScreen(vector);
        }

        /// <summary> 
        /// Converts a Screen point to it's World equivalent.
        /// </summary>
        public static Vector3 ScreenToWorld(this Vector2 vector)
        {
            return Drawing.ScreenToWorld(vector.X, vector.Y);
        }

        /// <summary> 
        /// Converts a World point to it's Grid equivalent.
        /// </summary>
        public static Vector2 WorldToGrid(this Vector3 vector)
        {
            return WorldToGrid(vector.To2D());
        }

        /// <summary> 
        /// Converts a World point to it's Grid equivalent.
        /// </summary>
        public static Vector2 WorldToGrid(this Vector2 vector)
        {
            return NavMesh.WorldToGrid(vector.X, vector.Y);
        }

        /// <summary> 
        /// Converts a Grid point to it's World equivalent.
        /// </summary>
        public static Vector3 GridToWorld(this Vector3 vector)
        {
            return GridToWorld(vector.To2D());
        }

        /// <summary> 
        /// Converts a Grid point to it's World equivalent.
        /// </summary>
        public static Vector3 GridToWorld(this Vector2 vector)
        {
            return NavMesh.GridToWorld((short) vector.X, (short) vector.Y);
        }

        /// <summary> 
        /// Converts a World point to it's Minimap equivalent.
        /// </summary>
        public static Vector2 WorldToMinimap(this Vector3 vector)
        {
            return TacticalMap.WorldToMinimap(vector);
        }

        /// <summary> 
        /// Converts a Minimap point to it's World equivalent.
        /// </summary>
        public static Vector3 MinimapToWorld(this Vector2 vector)
        {
            return TacticalMap.MinimapToWorld(vector.X, vector.Y);
        }

        /// <summary> 
        /// Converts a point to it's NavMeshCell equivalent.
        /// </summary>
        public static NavMeshCell ToNavMeshCell(this Vector3 vector)
        {
            return ToNavMeshCell(vector.To2D());
        }

        /// <summary> 
        /// Converts a point to it's NavMeshCell equivalent.
        /// </summary>
        public static NavMeshCell ToNavMeshCell(this Vector2 vector)
        {
            var gridCoords = vector.WorldToGrid();
            return NavMesh.GetCell((short) gridCoords.X, (short) gridCoords.Y);
        }

        /// <summary> 
        /// Finds the angle between to points and retuns it as a radian.
        /// </summary>
        public static float AngleBetween(this Vector3 vector3, Vector3 toVector3)
        {
            var magnitudeA = Math.Sqrt((vector3.X * vector3.X) + (vector3.Y * vector3.Y) + (vector3.Z * vector3.Z));
            var magnitudeB =
                Math.Sqrt((toVector3.X * toVector3.X) + (toVector3.Y * toVector3.Y) + (toVector3.Z * toVector3.Z));

            var dotProduct = (vector3.X * toVector3.X) + (vector3.Y * toVector3.Y) + (vector3.Z + toVector3.Z);
            return (float) Math.Acos(dotProduct / magnitudeA * magnitudeB);
        }

        #endregion

        #region Vector Extending

        /// <summary> 
        /// Returns a position that runs from the source position to the targets position with the length of the range specified.
        /// </summary>
        public static Vector2 Extend(this Vector3 source, GameObject target, float range)
        {
            return source.To2D().Extend(target.Position.To2D(), range);
        }

        /// <summary> 
        /// Returns a position that runs from the source position to the targets position with the length of the range specified.
        /// </summary>
        public static Vector2 Extend(this Vector3 source, Obj_AI_Base target, float range)
        {
            return source.To2D().Extend(target.ServerPosition.To2D(), range);
        }

        /// <summary> 
        /// Returns a position that runs from the source position to the target position with the length of the range specified.
        /// </summary>
        public static Vector2 Extend(this Vector3 source, Vector3 target, float range)
        {
            return source.To2D().Extend(target.To2D(), range);
        }

        /// <summary> 
        /// Returns a position that runs from the source position to the target position with the length of the range specified.
        /// </summary>
        public static Vector2 Extend(this Vector3 source, Vector2 target, float range)
        {
            return source.To2D().Extend(target, range);
        }

        /// <summary> 
        /// Returns a position that runs from the source position to the targets position with the length of the range specified.
        /// </summary>
        public static Vector2 Extend(this Vector2 source, GameObject target, float range)
        {
            return source.Extend(target.Position.To2D(), range);
        }

        /// <summary> 
        /// Returns a position that runs from the source position to the targets position with the length of the range specified.
        /// </summary>
        public static Vector2 Extend(this Vector2 source, Obj_AI_Base target, float range)
        {
            return source.Extend(target.ServerPosition.To2D(), range);
        }

        /// <summary> 
        /// Returns a position that runs from the source position to the target position with the length of the range specified.
        /// </summary>
        public static Vector2 Extend(this Vector2 source, Vector3 target, float range)
        {
            return source.Extend(target.To2D(), range);
        }

        /// <summary> 
        /// Returns a position that runs from the source position to the target position with the length of the range specified.
        /// </summary>
        public static Vector2 Extend(this Vector2 source, Vector2 target, float range)
        {
            return source + range * (target - source).Normalized();
        }

        #endregion

        #region Collision

        /// <summary> 
        /// Returns what type of objects are at the specified vector.
        /// </summary>
        public static CollisionFlags GetCollisionFlags(this Vector2 vector)
        {
            return NavMesh.GetCollisionFlags(vector.X, vector.Y);
        }

        /// <summary> 
        /// Returns what type of objects are at the specified vector.
        /// </summary>
        public static CollisionFlags GetCollisionFlags(this Vector3 vector)
        {
            return GetCollisionFlags(vector.To2D());
        }

        #region Bools

        /// <summary> 
        /// Returns true if there is a wall object at the specified vector.
        /// </summary>
        public static bool IsWall(this Vector2 vector)
        {
            return NavMesh.GetCollisionFlags(vector.X, vector.Y).HasFlag(CollisionFlags.Wall);
        }

        /// <summary> 
        /// Returns true if there is a wall object at the specified vector.
        /// </summary>
        public static bool IsWall(this Vector3 vector)
        {
            return IsWall(vector.To2D());
        }

        /// <summary> 
        /// Returns true if there is a building object at the specified vector.
        /// </summary>
        public static bool IsBuilding(this Vector2 vector)
        {
            return NavMesh.GetCollisionFlags(vector.X, vector.Y).HasFlag(CollisionFlags.Building);
        }

        /// <summary> 
        /// Returns true if there is a building object at the specified vector.
        /// </summary>
        public static bool IsBuilding(this Vector3 vector)
        {
            return IsBuilding(vector.To2D());
        }

        /// <summary> 
        /// Returns true if there is a grass object at the specified vector.
        /// </summary>
        public static bool IsGrass(this Vector2 vector)
        {
            return NavMesh.GetCollisionFlags(vector.X, vector.Y).HasFlag(CollisionFlags.Grass);
        }

        /// <summary> 
        /// Returns true if there is a grass object at the specified vector.
        /// </summary>
        public static bool IsGrass(this Vector3 vector)
        {
            return IsGrass(vector.To2D());
        }

        /// <summary> 
        /// Returns true if the specified position is under any alive turrets.
        /// </summary>
        public static bool IsUnderTurret(this Vector2 position)
        {
            return
                EntityManager.Turrets.AllTurrets.Any(
                    turret => turret.IsInRange(position, turret.GetAutoAttackRange()));
        }

        /// <summary> 
        /// Returns true if the specified position is under any alive turrets.
        /// </summary>
        public static bool IsUnderTurret(this Vector3 position)
        {
            return IsUnderTurret(position.To2D());
        }

        /// <summary> 
        /// Returns true if the specified position is under any alive turrets.
        /// </summary>
        public static bool IsUnderTurret(this Obj_AI_Base target)
        {
            return IsUnderTurret(target.ServerPosition);
        }

        /// <summary> 
        /// Returns true if the specified object is under a turret of the opposite team.
        /// </summary>
        public static bool IsUnderEnemyturret(this Obj_AI_Base target)
        {
            return
                EntityManager.Turrets.AllTurrets.Any(
                    turret => target.Team != turret.Team && turret.IsInAutoAttackRange(target));
        }

        /// <summary> 
        /// Returns true if the specified object is under a turret of the same team.
        /// </summary>
        public static bool IsUnderHisturret(this Obj_AI_Base target)
        {
            return
                EntityManager.Turrets.AllTurrets.Any(
                    turret => target.Team == turret.Team && turret.IsInAutoAttackRange(target));
        }

        /// <summary> 
        /// Returns the path the unit is taking.
        /// </summary>
        public static Vector3[] RealPath(this Obj_AI_Base unit)
        {
            return Prediction.Position.GetRealPath(unit);
        }

        /// <summary> 
        /// Checks if a Vector2 is visible on the screen. Use WorldToScreen before using this method.
        /// </summary>
        public static bool IsOnScreen(this Vector2 p)
        {
            return p.X <= Drawing.Width && p.X >= 0 && p.Y <= Drawing.Height && p.Y >= 0;
        }

        /// <summary> 
        /// Checks if a Vector3 is visible on the screen. 
        /// </summary>
        public static bool IsOnScreen(this Vector3 p)
        {
            return p.WorldToScreen().IsOnScreen();
        }

        #endregion Bools

        #endregion Collision

        #region PathRelated

        internal static List<Vector2> GetWaypoints(this Obj_AI_Base unit)
        {
            var result = new List<Vector2>();

            if (unit.IsVisible)
            {
                result.Add(unit.ServerPosition.To2D());
                var path = unit.Path;
                if (path.Length > 0)
                {
                    var first = path[0].To2D();
                    if (first.Distance(result[0], true) > 40)
                    {
                        result.Add(first);
                    }

                    for (int i = 1; i < path.Length; i++)
                    {
                        result.Add(path[i].To2D());
                    }
                }
            }
            //else if (WaypointTracker.StoredPaths.ContainsKey(unit.NetworkId))
            //{
            //    var path = WaypointTracker.StoredPaths[unit.NetworkId];
            //    var timePassed = (Utils.TickCount - WaypointTracker.StoredTick[unit.NetworkId]) / 1000f;
            //    if (path.PathLength() >= unit.MoveSpeed * timePassed)
            //    {
            //        result = CutPath(path, (int)(unit.MoveSpeed * timePassed));
            //    }
            //}

            return result;
        }

        public static List<Vector2> CutPath(this List<Vector2> path, float distance)
        {
            var result = new List<Vector2>();
            var Distance = distance;
            if (distance < 0)
            {
                path[0] = path[0] + distance * (path[1] - path[0]).Normalized();
                return path;
            }

            for (var i = 0; i < path.Count - 1; i++)
            {
                var dist = path[i].Distance(path[i + 1]);
                if (dist > Distance)
                {
                    result.Add(path[i] + Distance * (path[i + 1] - path[i]).Normalized());
                    for (var j = i + 1; j < path.Count; j++)
                    {
                        result.Add(path[j]);
                    }

                    break;
                }
                Distance -= dist;
            }
            return result.Count > 0 ? result : new List<Vector2> { path.Last() };
        }

        #endregion PathRelated

        public static Vector3 GetMissileFixedYPosition(this MissileClient target)
        {
            var pos = target.Position;
            return new Vector3(pos.X, pos.Y, pos.Z - 100);
        }
    }

    /// <summary>
    /// Count Extensions
    /// </summary>
    public static partial class Extensions
    {
        #region NoPred

        [Obsolete("Use CountEnemyChampionsInRange")]
        public static int CountEnemiesInRange(this Vector3 position, float range)
        {
            return CountEnemiesInRange(position.To2D(), range);
        }

        [Obsolete("Use CountEnemyChampionsInRange")]
        public static int CountEnemiesInRange(this Vector2 position, float range)
        {
            var rangeSqr = range.Pow();
            return EntityManager.Heroes.Enemies.Count(o => o.IsValidTarget() && o.Distance(position, true) < rangeSqr);
        }

        [Obsolete("Use CountEnemyChampionsInRange")]
        public static int CountEnemiesInRange(this GameObject target, float range)
        {
            var baseObject = target as Obj_AI_Base;
            return CountEnemiesInRange(baseObject != null ? baseObject.ServerPosition : target.Position, range);
        }

        [Obsolete("Use CountAllyChampionsInRange")]
        public static int CountAlliesInRange(this Vector3 position, float range)
        {
            return CountAlliesInRange(position.To2D(), range);
        }

        [Obsolete("Use CountAllyChampionsInRange")]
        public static int CountAlliesInRange(this Vector2 position, float range)
        {
            var rangeSqr = range.Pow();
            return EntityManager.Heroes.Allies.Count(o => o.IsValidTarget() && o.Distance(position, true) < rangeSqr);
        }

        [Obsolete("Use CountAllyChampionsInRange")]
        public static int CountAlliesInRange(this GameObject target, float range)
        {
            var baseObject = target as Obj_AI_Base;
            return CountAlliesInRange(baseObject != null ? baseObject.ServerPosition : target.Position, range);
        }

        ///<summary>
        ///Returns the amount of Enemy Champions in range.
        ///</summary>
        public static int CountEnemyChampionsInRange(this Vector3 position, float range)
        {
            return CountEnemyChampionsInRange(position.To2D(), range);
        }

        ///<summary>
        ///Returns the amount of Enemy Champions in range.
        ///</summary>
        public static int CountEnemyChampionsInRange(this Vector2 position, float range)
        {
            return EntityManager.Heroes.Enemies.Count(o => o.IsValidTarget() && o.IsInRange(position, range));
        }

        ///<summary>
        ///Returns the amount of Enemy Champions in range.
        ///</summary>
        public static int CountEnemyChampionsInRange(this GameObject target, float range)
        {
            var baseObject = target as Obj_AI_Base;
            return CountEnemyChampionsInRange(baseObject != null ? baseObject.ServerPosition : target.Position, range);
        }

        ///<summary>
        ///Returns the amount of Ally Champions in range.
        ///</summary>
        public static int CountAllyChampionsInRange(this Vector3 position, float range)
        {
            return CountAllyChampionsInRange(position.To2D(), range);
        }

        ///<summary>
        ///Returns the amount of Ally Champions in range.
        ///</summary>
        public static int CountAllyChampionsInRange(this Vector2 position, float range)
        {
            return EntityManager.Heroes.Allies.Count(o => o.IsValidTarget() && o.IsInRange(position, range));
        }

        ///<summary>
        ///Returns the amount of Ally Champions in range.
        ///</summary>
        public static int CountAllyChampionsInRange(this GameObject target, float range)
        {
            var baseObject = target as Obj_AI_Base;
            return CountAllyChampionsInRange(baseObject != null ? baseObject.ServerPosition : target.Position, range);
        }

        ///<summary>
        ///Returns the amount of Ally Minions in range.
        ///</summary>
        public static int CountAllyMinionsInRange(this Vector3 position, float range)
        {
            return CountAllyMinionsInRange(position.To2D(), range);
        }

        ///<summary>
        ///Returns the amount of Ally Minions in range.
        ///</summary>
        public static int CountAllyMinionsInRange(this Vector2 position, float range)
        {
            return
                EntityManager.MinionsAndMonsters.AlliedMinions.Count(
                    o => o.IsValidTarget() && o.IsInRange(position, range));
        }

        ///<summary>
        ///Returns the amount of Ally Minions in range. Uses Server Position if possible.
        ///</summary>
        public static int CountAllyMinionsInRange(this GameObject target, float range)
        {
            var baseObject = target as Obj_AI_Base;
            return CountAllyMinionsInRange(baseObject != null ? baseObject.ServerPosition : target.Position, range);
        }

        ///<summary>
        ///Returns the amount of Enemy Minions in range.
        ///</summary>
        public static int CountEnemyMinionsInRange(this Vector3 position, float range)
        {
            return CountEnemyMinionsInRange(position.To2D(), range);
        }

        ///<summary>
        ///Returns the amount of Enemy Minions in range.
        ///</summary>
        public static int CountEnemyMinionsInRange(this Vector2 position, float range)
        {
            return
                EntityManager.MinionsAndMonsters.EnemyMinions.Count(
                    o => o.IsValidTarget() && o.IsInRange(position, range));
        }

        ///<summary>
        ///Returns the amount of Enemy Minions in range. Uses Server Position if possible.
        ///</summary>
        public static int CountEnemyMinionsInRange(this GameObject target, float range)
        {
            var baseObject = target as Obj_AI_Base;
            return CountEnemyMinionsInRange(baseObject != null ? baseObject.ServerPosition : target.Position, range);
        }

        #endregion NoPred

        #region With Pred

        #region Heroes

        //Enemies
        ///<summary>
        ///Returns the amount of Enemy Champions in range with Prediction.
        ///</summary>
        public static int CountEnemyHeroesInRangeWithPrediction(this Vector2 position, int range, int delay = 250)
        {
            return
                EntityManager.Heroes.Enemies.Count(
                    e => e.IsValidTarget() && Prediction.Position.PredictUnitPosition(e, delay).IsInRange(position, range));
        }

        ///<summary>
        ///Returns the amount of Enemy Champions in range with Prediction.
        ///</summary>
        public static int CountEnemyHeroesInRangeWithPrediction(this Vector3 position, int range, int delay = 250)
        {
            return
                EntityManager.Heroes.Enemies.Count(
                    e => e.IsValidTarget() && Prediction.Position.PredictUnitPosition(e, delay).IsInRange(position, range));
        }

        ///<summary>
        ///Returns the amount of Enemy Champions in range with Prediction.
        ///</summary>
        public static int CountEnemyHeroesInRangeWithPrediction(this GameObject target, int range, int delay = 250)
        {
            return
                EntityManager.Heroes.Enemies.Count(
                    e => e.IsValidTarget() && Prediction.Position.PredictUnitPosition(e, delay).IsInRange(target, range));
        }

        //Allies
        ///<summary>
        ///Returns the amount of Ally Champions in range with Prediction.
        ///</summary>
        [Obsolete("Use CountAllyChampionsInRangeWithPrediction")]
        public static int CountEnemyAlliesInRangeWithPrediction(this Vector2 position, int range, int delay = 250)
        {
            return
                EntityManager.Heroes.Allies.Count(
                    e => e.IsValidTarget() && Prediction.Position.PredictUnitPosition(e, delay).IsInRange(position, range));
        }

        ///<summary>
        ///Returns the amount of Ally Champions in range with Prediction.
        ///</summary>
        [Obsolete("Use CountAllyChampionsInRangeWithPrediction")]
        public static int CountEnemyAlliesInRangeWithPrediction(this Vector3 position, int range, int delay = 250)
        {
            return
                EntityManager.Heroes.Allies.Count(
                    e => e.IsValidTarget() && Prediction.Position.PredictUnitPosition(e, delay).IsInRange(position, range));
        }

        ///<summary>
        ///Returns the amount of Ally Champions in range with Prediction.
        ///</summary>
        [Obsolete("Use CountAllyChampionsInRangeWithPrediction")]
        public static int CountEnemyAlliesInRangeWithPrediction(this GameObject target, int range, int delay = 250)
        {
            return
                EntityManager.Heroes.Allies.Count(
                    e => e.IsValidTarget() && Prediction.Position.PredictUnitPosition(e, delay).IsInRange(target, range));
        }

        ///<summary>
        ///Returns the amount of Ally Champions in range with Prediction.
        ///</summary>
        public static int CountAllyChampionsInRangeWithPrediction(this Vector2 position, int range, int delay = 250)
        {
            return
                EntityManager.Heroes.Allies.Count(
                    e => e.IsValidTarget() && Prediction.Position.PredictUnitPosition(e, delay).IsInRange(position, range));
        }

        ///<summary>
        ///Returns the amount of Ally Champions in range with Prediction.
        ///</summary>
        public static int CountAllyChampionsInRangeWithPrediction(this Vector3 position, int range, int delay = 250)
        {
            return
                EntityManager.Heroes.Allies.Count(
                    e => e.IsValidTarget() && Prediction.Position.PredictUnitPosition(e, delay).IsInRange(position, range));
        }

        ///<summary>
        ///Returns the amount of Ally Champions in range with Prediction.
        ///</summary>
        public static int CountAllyChampionsInRangeWithPrediction(this GameObject target, int range, int delay = 250)
        {
            return
                EntityManager.Heroes.Allies.Count(
                    e => e.IsValidTarget() && Prediction.Position.PredictUnitPosition(e, delay).IsInRange(target, range));
        }

        #endregion Heroes

        #region Minions

        //Enemies

        ///<summary>
        ///Returns the amount of Enemy Minions in range with Prediction.
        ///</summary>
        public static int CountEnemyMinionsInRangeWithPrediction(this Vector2 position, int range, int delay = 250)
        {
            return
                EntityManager.MinionsAndMonsters.EnemyMinions.Count(
                    e => e.IsValidTarget() && Prediction.Position.PredictUnitPosition(e, delay).IsInRange(position, range));
        }

        ///<summary>
        ///Returns the amount of Enemy Minions in range with Prediction.
        ///</summary>
        public static int CountEnemyMinionsInRangeWithPrediction(this Vector3 position, int range, int delay = 250)
        {
            return
                EntityManager.MinionsAndMonsters.EnemyMinions.Count(
                    e => e.IsValidTarget() && Prediction.Position.PredictUnitPosition(e, delay).IsInRange(position, range));
        }

        ///<summary>
        ///Returns the amount of Enemy Minions in range with Prediction.
        ///</summary>
        public static int CountEnemyMinionsInRangeWithPrediction(this GameObject target, int range, int delay = 250)
        {
            return
                EntityManager.MinionsAndMonsters.EnemyMinions.Count(
                    e => e.IsValidTarget() && Prediction.Position.PredictUnitPosition(e, delay).IsInRange(target.Position, range));
        }

        //Allies

        ///<summary>
        ///Returns the amount of Ally Minions in range with Prediction.
        ///</summary>
        public static int CountAllyMinionsInRangeWithPrediction(this Vector2 position, int range, int delay = 250)
        {
            return
                EntityManager.MinionsAndMonsters.AlliedMinions.Count(
                    e => e.IsValidTarget() && Prediction.Position.PredictUnitPosition(e, delay).IsInRange(position, range));
        }

        ///<summary>
        ///Returns the amount of Ally Minions in range with Prediction.
        ///</summary>
        public static int CountAllyMinionsInRangeWithPrediction(this Vector3 position, int range, int delay = 250)
        {
            return
                EntityManager.MinionsAndMonsters.AlliedMinions.Count(
                    e => e.IsValidTarget() && Prediction.Position.PredictUnitPosition(e, delay).IsInRange(position, range));
        }

        ///<summary>
        ///Returns the amount of Ally Minions in range with Prediction.
        ///</summary>
        public static int CountAllyMinionsInRangeWithPrediction(this GameObject target, int range, int delay = 250)
        {
            return
                EntityManager.MinionsAndMonsters.AlliedMinions.Count(
                    e => e.IsValidTarget() && Prediction.Position.PredictUnitPosition(e, delay).IsInRange(target.Position, range));
        }

        #endregion Minions

        #endregion With Pred
    }

    /// <summary>
    /// Color Extensions
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Converts a System.Drawing color to it's SharpDX equivalent.
        /// </summary>
        public static Color ToSharpDX(this System.Drawing.Color color)
        {
            return new Color(color.R, color.G, color.B, color.A);
        }

        /// <summary>
        /// Converts a SharpDX color to it's System.Drawing equivalent.
        /// </summary>
        public static System.Drawing.Color ToSystem(this Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }

    /// <summary>
    /// Drawings
    /// </summary>
    public static partial class Extensions
    {
        #region DrawCircle
        //int
        public static void DrawCircle(this GameObject target, int radius, System.Drawing.Color color, float lineWidth = 3f)
        {
            Circle.Draw(color.ToSharpDX(), radius, lineWidth, target);
        }

        public static void DrawCircle(this GameObject target, int radius, Color color, float lineWidth = 3f)
        {
            Circle.Draw(color, radius, lineWidth, target);
        }

        public static void DrawCircle(this Vector2 position, int radius, Color color, float lineWidth = 3f)
        {
            Circle.Draw(color, radius, lineWidth, position.To3D());
        }

        public static void DrawCircle(this Vector3 position, int radius, Color color, float lineWidth = 3f)
        {
            Circle.Draw(color, radius, lineWidth, position);
        }
        #endregion DrawCircle

        #region DrawArrow
        public static void DrawArrow(this Vector2 start, Vector2 end, System.Drawing.Color color, int width = 3)
        {
            var pos1 = end + (start - end).Rotated(45 * (float)Math.PI / 180) / 5f;
            var pos2 = end + (start - end).Rotated(-45 * (float)Math.PI / 180) / 5f;

            new Geometry.Polygon.Line(start.To3D(), end.To3D()).Draw(color, width);
            new Geometry.Polygon.Line(end.To3D(), pos1.To3D()).Draw(color, width);
            new Geometry.Polygon.Line(end.To3D(), pos2.To3D()).Draw(color, width);
        }

        public static void DrawArrow(this Vector3 start, Vector3 end, System.Drawing.Color color, int width = 3)
        {
            var pos1 = end.To2D() + (start - end).To2D().Rotated(45 * (float)Math.PI / 180) / 5f;
            var pos2 = end.To2D() + (start - end).To2D().Rotated(-45 * (float)Math.PI / 180) / 5f;

            new Geometry.Polygon.Line(start, end).Draw(color, width);
            new Geometry.Polygon.Line(end, pos1.To3D()).Draw(color, width);
            new Geometry.Polygon.Line(end, pos2.To3D()).Draw(color, width);
        }

        public static void DrawArrow(this Geometry.Polygon.Line line, System.Drawing.Color color, int width = 3)
        {
            var start = line.LineStart;
            var end = line.LineEnd;
            DrawArrow(start, end, color, width);
        }
        #endregion DrawArrow

    }

    /// <summary>
    /// Minion related
    /// </summary>
    public static partial class Extensions
    {
        private static readonly List<string> PetList = new List<string>
        {
            "annietibbers",
            "elisespiderling",
            "heimertyellow",
            "heimertblue",
            "malzaharvoidling",
            "shacobox",
            "yorickspectralghoul",
            "yorickdecayedghoul",
            "yorickravenousghoul",
            "zyrathornplant",
            "zyragraspingplant"
        };

        private static readonly List<string> CloneList = new List<string> { "leblanc", "shaco", "monkeyking" };


        /// <summary>
        /// Returns true if the object is a pet.
        /// </summary>
        public static bool IsPet(this Obj_AI_Minion minion)
        {
            var name = minion.CharData.BaseSkinName.ToLower();
            return PetList.Contains(name);
        }

        /// <summary>
        /// Returns true if the object is a clone.
        /// </summary>
        public static bool IsClone(this Obj_AI_Minion minion)
        {
            var name = minion.CharData.BaseSkinName.ToLower();
            return CloneList.Contains(name);
        }
    }
}
