﻿using WCell.Constants.Updates;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class YellCommand : RealmServerCommand
  {
    protected YellCommand()
    {
    }

    protected override void Initialize()
    {
      Init("Yell");
      EnglishParamInfo = "<text>";
      EnglishDescription = "Yell something";
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      trigger.Args.Target.Yell(trigger.Text.Remainder.Trim());
    }

    public override ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.All; }
    }
  }
}