﻿using WCell.Util.Strings;

namespace WCell.Util.Commands
{
  /// <summary>
  /// CmdTriggers trigger Commands. There are different kinds of triggers which are handled differently,
  /// according to where they came from.
  /// 
  /// </summary>
  /// 
  ///             TODO: Have a reply-stream.
  public abstract class CmdTrigger<C> : ITriggerer where C : ICmdArgs
  {
    protected StringStream m_text;

    /// <summary>The alias that has been used to trigger this command.</summary>
    public string Alias;

    protected internal BaseCommand<C> cmd;
    protected internal BaseCommand<C> selectedCmd;
    public C Args;

    protected CmdTrigger()
    {
    }

    protected CmdTrigger(StringStream text, C args)
    {
      m_text = text;
      Args = args;
    }

    protected CmdTrigger(C args)
    {
      Args = args;
    }

    protected CmdTrigger(StringStream text, BaseCommand<C> selectedCmd, C args)
    {
      m_text = text;
      this.selectedCmd = selectedCmd;
      Args = args;
    }

    /// <summary>
    /// That command that has been triggered or null if the command for this <code>Alias</code> could
    /// not be found.
    /// </summary>
    public BaseCommand<C> Command
    {
      get { return cmd; }
    }

    /// <summary>
    /// That command that was selected when triggering this Trigger.
    /// </summary>
    public BaseCommand<C> SelectedCommand
    {
      get { return selectedCmd; }
      set { selectedCmd = value; }
    }

    /// <summary>
    /// A <code>StringStream</code> which contains the supplied arguments.
    /// </summary>
    public StringStream Text
    {
      get { return m_text; }
      set { m_text = value; }
    }

    /// <summary>Replies accordingly with the given text.</summary>
    public abstract void Reply(string text);

    /// <summary>Replies accordingly with the given formatted text.</summary>
    public abstract void ReplyFormat(string text);

    public void Reply(string format, params object[] args)
    {
      Reply(string.Format(format, args));
    }

    public void ReplyFormat(string format, params object[] args)
    {
      ReplyFormat(string.Format(format, args));
    }

    public T EvalNext<T>(T deflt)
    {
      object obj = cmd.mgr.EvalNext(this, deflt);
      if(obj is T)
        return (T) obj;
      return default(T);
    }

    public NestedCmdTrigger<C> Nest(C args)
    {
      return new NestedCmdTrigger<C>(this, args);
    }

    public NestedCmdTrigger<C> Nest(string text)
    {
      return Nest(new StringStream(text));
    }

    public NestedCmdTrigger<C> Nest(StringStream text)
    {
      return new NestedCmdTrigger<C>(this, Args, text);
    }

    public SilentCmdTrigger<C> Silent(C args)
    {
      return new SilentCmdTrigger<C>(m_text, args);
    }
  }
}