/*************************************************************************
 *
 *   file		: MovementCommands.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-02-01 14:22:25 +0100 (ma, 01 feb 2010) $

 *   revision		: $Rev: 1240 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Linq;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Lang;
using WCell.Util;
using WCell.Util.Commands;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Commands
{
    #region Fly
    public class FlyCommand : RealmServerCommand
    {
        protected FlyCommand() { }

        protected override void Initialize()
        {
            base.Init("Fly");
            EnglishParamInfo = "[0/1]";
            EnglishDescription = "Toggles flying mode";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            bool flying = (trigger.Text.HasNext && trigger.Text.NextBool()) || (trigger.Args.Target.Flying == 0);
            if (flying)
            {
                trigger.Args.Target.Flying++;
            }
            else
            {
                trigger.Args.Target.Flying = 0;
            }
            trigger.Reply("Flying " + (flying ? "on" : "off"));
        }


        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.All; }
        }
    }
    #endregion

    #region Knockback
    public class KnockbackCommand : RealmServerCommand
    {
        protected KnockbackCommand() { }

        protected override void Initialize()
        {
            base.Init("Knockback");
            EnglishParamInfo = "<verticalSpeed> [<horizontalSpeed>]";
            EnglishDescription = "Knocks the target back";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            var vertSpeed = trigger.Text.NextFloat();
            var horSpeed = trigger.Text.NextFloat(vertSpeed);
            MovementHandler.SendKnockBack(trigger.Args.Character, trigger.Args.Target, vertSpeed, horSpeed);
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
    #endregion

    #region Speed
    public class MultiplySpeedCommand : RealmServerCommand
    {
        public override bool RequiresEqualOrHigherRank
        {
            get
            {
                return true;
            }
        }
        public override Intercommunication.DataTypes.RoleStatus RequiredStatusDefault
        {
            get
            {
                return RoleStatus.EventManager;
            }
        }
        protected MultiplySpeedCommand() { }

        protected override void Initialize()
        {
            Init("MultiplySpeed", "SpeedFactor", "MultSpeed", "Speed");
            EnglishParamInfo = "<speedFactor>";
            EnglishDescription = "Sets the overall speed-factor of a Unit";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            var factor = trigger.Text.NextFloat(1);
            var mod = trigger.Text.NextModifiers();
            if (mod == "i")
            {
                trigger.Reply("speed : {0}",trigger.Args.Target.SpeedFactor);
                return;
            }
            if (factor > 0.01)
            {
                if (mod.Contains("w"))
                {
                    foreach (var chr in World.GetAllCharacters())
                    {
                        chr.SpeedFactor = factor;
                    }
                }
                else if (mod.Contains("n"))
                {
                    foreach (var chr in trigger.Args.Character.GetNearbyCharacters())
                    {
                        chr.SpeedFactor = factor;
                    }
                }
                else if (mod.Contains("m"))
                {
                    foreach (var chr in trigger.Args.Character.Map.Characters.ToArray())
                    {
                        chr.SpeedFactor = factor;
                    }
                }
                else
                {
                    trigger.Args.Target.SpeedFactor = factor;
                    trigger.Reply("SpeedFactor set to: " + factor);
                }
            }
            else
            {
                trigger.Reply("The argument must be a positive number");
            }
        }


        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.All; }
        }
    }
    #endregion

    #region Waterwalk
    public class WaterWalkCommand : RealmServerCommand
    {
        protected WaterWalkCommand() { }

        protected override void Initialize()
        {
            base.Init("WaterWalk", "WalkWater");
            EnglishParamInfo = "[0/1]";
            EnglishDescription = "Toggles the ability to walk on water";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            bool waterWalk = (trigger.Text.HasNext && trigger.Text.NextBool()) || (trigger.Args.Target.WaterWalk == 0);
            if (waterWalk)
            {
                trigger.Args.Target.WaterWalk++;
            }
            else
            {
                trigger.Args.Target.WaterWalk = 0;
            }
            trigger.Reply("WaterWalking " + (waterWalk ? "on" : "off"));
        }



        public override ObjectTypeCustom TargetTypes
        {
            get
            {
                return ObjectTypeCustom.All;
            }
        }
    }
    #endregion

    #region Rooted
    public class RootedCommand : RealmServerCommand
    {
        protected RootedCommand() { }

        protected override void Initialize()
        {
            Init("Rooted", "Root");
            EnglishParamInfo = "[0/1]";
            EnglishDescription = "Toggles whether the Unit can move or not";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            var newState = (trigger.Text.HasNext && trigger.Text.NextBool()) ||
                (!trigger.Text.HasNext && trigger.Args.Target.CanMove);

            if (newState)
            {
                trigger.Args.Target.IncMechanicCount(SpellMechanic.Rooted);
            }
            else
            {
                trigger.Args.Target.DecMechanicCount(SpellMechanic.Rooted);
            }
            trigger.Reply((newState ? "R" : "Unr") + "ooted ");
        }

        public override ObjectTypeCustom TargetTypes
        {
            get
            {
                return ObjectTypeCustom.All;
            }
        }
    }
    #endregion

    #region Stunned
    public class StunnededCommand : RealmServerCommand
    {
        protected StunnededCommand() { }

        protected override void Initialize()
        {
            Init("Stunned", "Stun");
            EnglishParamInfo = "[0/1]";
            EnglishDescription = "Toggles whether the Unit is stunned or not";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            var newState = (trigger.Text.HasNext && trigger.Text.NextBool()) ||
                (!trigger.Text.HasNext && trigger.Args.Target.CanMove);

            if (newState)
            {
                trigger.Args.Target.IncMechanicCount(SpellMechanic.Stunned);
            }
            else
            {
                trigger.Args.Target.DecMechanicCount(SpellMechanic.Stunned);
            }
            trigger.Reply((newState ? "S" : "Uns") + "tunned ");
        }

        public override ObjectTypeCustom TargetTypes
        {
            get
            {
                return ObjectTypeCustom.All;
            }
        }
    }

    public class StunAllCommand : RealmServerCommand
    {
        protected StunAllCommand() { }

        protected override void Initialize()
        {
            Init("Stunall");
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            foreach (var chr in World.GetAllCharacters())
            {
                if (chr == trigger.Args.Character || chr.Role.Status>=RoleStatus.EventManager || chr.IsStunned)
                    continue;
                chr.IncMechanicCount(SpellMechanic.Stunned);
            }
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.EventManager; }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get
            {
                return ObjectTypeCustom.All;
            }
        }
    }
    public class UnStunAllCommand : RealmServerCommand
    {
        protected UnStunAllCommand() { }

        protected override void Initialize()
        {
            Init("UnStunall");
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            foreach (var chr in World.GetAllCharacters())
            {
                if (chr == trigger.Args.Character || chr.Role.Status >= RoleStatus.EventManager || !chr.IsStunned)
                    continue;
                chr.DecMechanicCount(SpellMechanic.Stunned);
            }
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.EventManager; }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get
            {
                return ObjectTypeCustom.All;
            }
        }
    }
    #endregion

    #region Teleport
    public class TeleportCommand : RealmServerCommand
    {
        protected TeleportCommand() { }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.EventManager; }
        }

        protected override void Initialize()
        {
            Init("Tele", "Teleport");
            EnglishParamInfo = "[-c [<x> <y> [<MapName or Id>]]] | [<LocationName>]";
            EnglishDescription = "Teleports to the given location or shows a list of all places that match the given name. " +
                "-c teleports to the given coordinates instead. ";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (!trigger.Text.HasNext)
            {
                trigger.Reply("Invalid position. Usage: " + EnglishParamInfo);
                return;
            }

            var target = trigger.Args.Target;
            var mod = trigger.Text.NextModifiers();
            if (mod == "c")
            {
                float? o = null;
                Map map = null;

                var x = trigger.Text.NextFloat(-50001);
                var y = trigger.Text.NextFloat(-50001);

                if (trigger.Text.HasNext)
                {
                    var mapId = trigger.Text.NextEnum(MapId.End);
                    map = World.GetNonInstancedMap(mapId);
                    if (map == null)
                    {
                        trigger.Reply("Invalid map: " + mapId);
                        return;
                    }
                }

                if (x < -50000 || y < -50000)
                {
                    trigger.Reply("Invalid position. Usage: " + EnglishParamInfo);
                    return;
                }
                if (map == null)
                {
                    map = trigger.Args.Character.Map;
                }


                var pos = new Vector3(x + map.Offset, y + map.Offset, 0);
                trigger.Args.Target.TeleportTo(map, ref pos, o);
            }
            else
            {
                // Named Teleport Location
                var targetName = trigger.Text.Remainder;

                if (trigger.Args.Character != null)
                {
                    // TODO: Use localization
                    var locs = WorldLocationMgr.GetMatches(targetName);

                    if (locs.Count == 0)
                    {
                        trigger.Reply("No matches found for: " + targetName);
                        return;
                    }
                    else
                    {
                        if (locs.Count == 1)
                        {
                            target.TeleportTo(locs[0]);
                        }
                        else
                        {
                            var perfectMatch = locs.FirstOrDefault(loc => loc.DefaultName.Equals(targetName, StringComparison.InvariantCultureIgnoreCase));
                            if (perfectMatch != null)
                            {
                                target.TeleportTo(perfectMatch);
                            }
                            else
                            {
                                trigger.Args.Character.StartGossip(WorldLocationMgr.CreateTeleMenu(locs));
                            }
                        }
                    }
                }
                else
                {
                    var loc = WorldLocationMgr.GetFirstMatch(targetName);
                    if (loc != null)
                    {
                        target.TeleportTo(loc);
                    }
                    else
                    {
                        trigger.Reply("No matches found for: " + targetName);
                    }
                }

                // var loc = WorldLocationMgr.GetFirstMatch(targetName);
                //if (loc != null)
                //{
                //    var map = World.GetMap(loc.MapId);
                //    trigger.Args.Target.TeleportTo(map, loc.Position);
                //}
                //else
                //{
                //    trigger.Reply("Teleport failed - Invalid location: " + targetName);
                //}
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Unit; }
        }
    }
    #endregion

    #region GoTo
    public class GoToCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            Init("GoTo");
            EnglishParamInfo = "<targetname>";
            EnglishDescription =
                "Teleports the Target to Character/Unit/GameObject. If Unit or GO are specified, target will be teleported to the nearest one [NYI].";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            var targetName = trigger.Text.NextWord();
            WorldObject target = null;
            if (targetName.Length > 0)
            {
                //if (mod == "c")
                //{
                target = World.GetCharacter(targetName, false);
                //}
                //else
                //{
                //    trigger.Reply("Modifier \"{0}\" is currently not supported.", mod);
                //    // TODO: Units and GOs
                //    //if (mod == "u")
                //    //{

                //    //}
                //}
            }

            if (target == null)
            {
                trigger.Reply("Invalid Target: " + targetName);
            }
            else
            {
                trigger.Args.Target.TeleportTo(target);
            }
        }
    }
    #endregion

    #region Summon All
    public class SummonAllCommand : RealmServerCommand
    {
        protected SummonAllCommand() { }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.EventManager; }
        }

        protected override void Initialize()
        {
            Init("SummonAll");
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            var minLevel = trigger.Text.NextInt(1);
            var maxLevel = trigger.Text.NextInt(250);
            var diffMod = trigger.Text.NextFloat(0.7f);
            var mod = trigger.Text.NextModifiers();

            var i = 0;
            if (mod == "n")
            {
                foreach (var chr in trigger.Args.Character.GetNearbyCharacters(200,false))
                {
                    var pos = trigger.Args.Character.Position;
                    var diff = Utility.GetPosDiff(i++);
                    chr.TeleportTo(trigger.Args.Character.Map.Id, new Vector3(pos.X + diff.X * diffMod, pos.Y + diff.Y * diffMod));
                }
            }
            else
            {
                foreach (var chr in World.GetAllCharacters())
                {
                    if (chr == trigger.Args.Target || chr.Level < minLevel || maxLevel < chr.Level)
                    {
                        continue;
                    }
                    if (mod == "f")
                    {
                        var pos = trigger.Args.Character.Position;
                        var diff = Utility.GetPosDiff(i++);
                        chr.TeleportTo(trigger.Args.Character.Map.Id, new Vector3(pos.X + diff.X * diffMod, pos.Y + diff.Y * diffMod));
                    }
                    else
                    {
                        Asda2SoulmateHandler.SendSoulmateSummoningYouResponse(trigger.Args.Character, chr);
                    }
                }
            }
        }


        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.All; }
        }
    }
    public class AcceptSummonCommand : RealmServerCommand
    {
        protected AcceptSummonCommand() { }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Player; }
        }

        protected override void Initialize()
        {
            Init("acpts","acceptsummon");
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (trigger.Args.Character.CanTeleportToFriend)
            {
                trigger.Args.Character.CanTeleportToFriend = false;
                if (trigger.Args.Character.SoulmateCharacter != null)
                    trigger.Args.Character.TeleportTo(trigger.Args.Character.Map, trigger.Args.Character.Position);
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.All; }
        }
    }
    #endregion

    #region Summon Single Player
    public class SummonPlayerCommand : RealmServerCommand
    {
        protected SummonPlayerCommand() { }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.GA; }
        }

        protected override void Initialize()
        {
            Init("Summon");
            EnglishParamInfo = "[-aq] <name>";
            EnglishDescription = "Summons the Player with the given name. " +
                "-q will queries Player before teleporting (can be denied). " +
                "-a switch will use the account name instead of the Char-name.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (!trigger.Text.HasNext)
            {
                trigger.Reply("You need to specify the name of the Player to be summoned.");
            }
            else
            {
                var mod = trigger.Text.NextModifiers();
                var query = mod.Contains("q");
                var name = trigger.Text.NextWord();

                Character chr;
                if (mod.Contains("a"))
                {
                    var acc = RealmServer.Instance.GetLoggedInAccount(name);
                    if (acc != null)
                    {
                        chr = acc.ActiveCharacter;
                    }
                    else
                    {
                        chr = null;
                    }
                }
                else
                {
                    chr = World.GetCharacter(name, false);
                }

                if (chr == null)
                {
                    trigger.Reply(RealmLangKey.CmdSummonPlayerNotOnline, name);
                }
                else
                {
                    // staff of higher ranks cannot be insta-summoned
                    if (query || chr.Role > trigger.Args.Character.Role)
                    {
                        chr.StartSummon(trigger.Args.Character);
                    }
                    else
                    {
                        chr.TeleportTo(trigger.Args.Target.Map, trigger.Args.Target.Position);
                    }
                }
            }
        }

        public override bool RequiresCharacter
        {
            get
            {
                return true;
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get
            {
                return ObjectTypeCustom.All;
            }
        }
    }
    #endregion
}