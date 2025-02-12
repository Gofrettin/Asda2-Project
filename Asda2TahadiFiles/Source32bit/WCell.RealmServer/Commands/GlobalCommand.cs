﻿using System;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class GlobalCommand : RealmServerCommand
  {
    public override RoleStatus RequiredStatusDefault
    {
      get { return RoleStatus.Admin; }
    }

    protected override void Initialize()
    {
      Init("Global");
      EnglishParamInfo = "[-pi] <command + command args>";
      EnglishDescription =
        "Executes the given command on everyone ingame. Use carefully! -p Only on Players (exclude staff members). -i Include self";
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      string str = trigger.Text.NextModifiers();
      BaseCommand<RealmServerCmdArgs> cmd = RealmCommandHandler.Instance.GetCommand(trigger);
      if(cmd == null || !cmd.Enabled || trigger.Args.User != null && !trigger.Args.User.Role.MayUse(cmd.RootCmd))
      {
        trigger.Reply("Invalid Command.");
      }
      else
      {
        SilentCmdTrigger<RealmServerCmdArgs> chrTrigger = trigger.Silent(new RealmServerCmdArgs(trigger.Args));
        bool playersOnly = str.Contains("p");
        bool inclSelf = str.Contains("i");
        int chrCount = 0;
        World.CallOnAllChars(chr =>
        {
          if(!(chr.Role <= trigger.Args.Role) || playersOnly && chr.Role.Status != RoleStatus.Player ||
             !inclSelf && ReferenceEquals(chr, trigger.Args.User))
            return;
          chrTrigger.Args.Target = chr;
          RealmCommandHandler.Instance.Execute(chrTrigger, cmd, true);
          ++chrCount;
        }, () => trigger.Reply("Done. - Called Command on {0} Characters.", (object) chrCount));
      }
    }
  }
}