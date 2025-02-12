﻿using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
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
      string name = trigger.Text.NextWord();
      WorldObject worldObject = null;
      if(name.Length > 0)
        worldObject = World.GetCharacter(name, false);
      if(worldObject == null)
        trigger.Reply("Invalid Target: " + name);
      else
        trigger.Args.Target.TeleportTo(worldObject);
    }
  }
}