﻿using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Core;
using WCell.Intercommunication;
using WCell.RealmServer.Auth;
using WCell.RealmServer.Auth.Accounts;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;
using WCell.RealmServer.Privileges;
using WCell.Util.Threading;

namespace WCell.RealmServer
{
  /// <summary>
  /// Represents the Account that a client used to login with on RealmServer side.
  /// </summary>
  public class RealmAccount : IAccount, IAccountInfo
  {
    protected static Logger log = LogManager.GetCurrentClassLogger();
    protected int m_accountId;
    protected string m_email;
    protected int m_HighestCharLevel;

    /// <summary>Default constructor.</summary>
    /// <param name="accountName">the name of the account</param>
    public RealmAccount(string accountName, IAccountInfo info)
    {
      Name = accountName;
      Characters = new List<CharacterRecord>();
      m_accountId = info.AccountId;
      ClientId = ClientId.Original;
      IsActive = true;
      Role = Singleton<PrivilegeMgr>.Instance.GetRoleOrDefault(info.RoleGroupName);
      m_email = info.EmailAddress;
      LastIP = info.LastIP;
      LastLogin = info.LastLogin;
      Locale = ClientLocale.Russian;
    }

    /// <summary>Still in Auth-Queue and waiting for a free slot</summary>
    public bool IsEnqueued { get; internal set; }

    /// <summary>The username of this account.</summary>
    public string Name { get; protected set; }

    public bool IsActive { get; protected set; }

    public DateTime? StatusUntil { get; protected set; }

    /// <summary>
    /// The database row ID for this account.
    /// Don't change it.
    /// </summary>
    public int AccountId
    {
      get { return m_accountId; }
    }

    /// <summary>The e-mail address of this account.</summary>
    /// <remarks>Use <c>SetEmail</c> instead to change the EmailAddress.</remarks>
    public string EmailAddress
    {
      get { return m_email; }
    }

    /// <summary>Setting this would not be saved to DB.</summary>
    public ClientId ClientId { get; protected set; }

    /// <summary>The last IP-Address that this Account connected with</summary>
    public byte[] LastIP { get; set; }

    /// <summary>
    /// The time of when this Account last logged in.
    /// Might be null.
    /// </summary>
    public DateTime? LastLogin { get; protected set; }

    public int HighestCharLevel
    {
      get { return m_HighestCharLevel; }
      set { m_HighestCharLevel = value; }
    }

    public ClientLocale Locale { get; private set; }

    /// <summary>The name of the RoleGroup.</summary>
    /// <remarks>
    /// Implements <see cref="P:WCell.Intercommunication.IAccountInfo.RoleGroupName" />.
    /// Use <c>SetRole</c> to change the Role.
    /// </remarks>
    public string RoleGroupName
    {
      get { return Role.Name; }
    }

    /// <summary>The RoleGroup of this Account.</summary>
    /// <remarks>Use <c>SetRole</c> to change the Role.</remarks>
    public RoleGroup Role { get; protected set; }

    /// <summary>All the character associated with this account.</summary>
    public List<CharacterRecord> Characters { get; protected set; }

    /// <summary>
    /// The Character that is currently being used by this Account (or null)
    /// </summary>
    public Character ActiveCharacter { get; internal set; }

    /// <summary>
    /// The client that is connected to this Account.
    /// If connected, the client is either still selecting a Character,
    /// seeing the Login-screen or already ingame (in which case ActiveCharacter is also set).
    /// </summary>
    public IRealmClient Client { get; internal set; }

    /// <summary>The account data cache, related to this account.</summary>
    public AccountDataRecord AccountData { get; internal set; }

    public CharacterRecord GetCharacterRecord(uint id)
    {
      foreach(CharacterRecord character in Characters)
      {
        if((int) character.EntityLowId == (int) id)
          return character;
      }

      return null;
    }

    public void RemoveCharacterRecord(uint id)
    {
      for(int index = 0; index < Characters.Count; ++index)
      {
        if((int) Characters[index].EntityLowId == (int) id)
        {
          Characters.RemoveAt(index);
          break;
        }
      }
    }

    /// <summary>
    /// Tells the AuthServer to change the role for this Account.
    /// </summary>
    /// <param name="role">the new role for this account</param>
    /// <returns>true if the role was set; false otherwise</returns>
    /// <remarks>Requires IO-Context</remarks>
    public bool SetRole(RoleGroup role)
    {
      bool wasStaff = Role.IsStaff;
      if(!Role.Equals(role))
      {
        Role = role;
        if(wasStaff != role.IsStaff)
        {
          Character chr = ActiveCharacter;
          if(chr != null)
          {
            Map map = chr.Map;
            IContextHandler context = chr.ContextHandler;
            if(context != null)
              context.AddMessage(() =>
              {
                if(!chr.IsInWorld || chr.Map != context)
                  return;
                if(wasStaff)
                {
                  --World.StaffMemberCount;
                  map.IncreasePlayerCount(chr);
                }
                else
                {
                  ++World.StaffMemberCount;
                  map.DecreasePlayerCount(chr);
                }
              });
          }
        }
      }

      return true;
    }

    public bool SetAccountActive(bool active, DateTime? statusUntil)
    {
      Account account = AccountMgr.GetAccount(Name);
      if(account == null)
        return false;
      account.StatusUntil = statusUntil;
      account.IsActive = active;
      account.SaveLater();
      IsActive = active;
      StatusUntil = statusUntil;
      if(ActiveCharacter != null && !active)
        ActiveCharacter.Kick("Banned");
      return true;
    }

    /// <summary>
    /// Sets the e-mail address for this account and persists it to the DB.
    /// Blocking call. Make sure to call this from outside the Map-Thread.
    /// </summary>
    /// <param name="email">the new e-mail address for this account</param>
    /// <returns>true if the e-mail address was set; false otherwise</returns>
    /// <remarks>Requires IO-Context</remarks>
    public bool SetEmail(string email)
    {
      if(EmailAddress != email)
        m_email = email;
      return true;
    }

    /// <summary>
    /// Sets the password for this account and sends it to the Authserver to be saved.
    /// Blocking call. Make sure to call this from outside the Map-Thread.
    /// </summary>
    /// <returns>true if the e-mail address was set; false otherwise</returns>
    public bool SetPass(string oldPassStr, string passStr)
    {
      return true;
    }

    /// <summary>
    /// Reloads all characters belonging to this account from the database.
    /// Blocking call. Make sure to call this from outside the Map-Thread.
    /// </summary>
    private void LoadCharacters()
    {
      foreach(CharacterRecord characterRecord in CharacterRecord.FindAllOfAccount(this))
        Characters.Add(characterRecord);
    }

    /// <summary>
    /// Loads account based data, creates base data if no data is found.
    /// </summary>
    private void LoadAccountData()
    {
      AccountData = AccountDataRecord.GetAccountData(AccountId) ??
                    AccountDataRecord.InitializeNewAccount(AccountId);
    }

    public override string ToString()
    {
      return Name + " (Id: " + AccountId + ")";
    }

    /// <summary>Called from within the IO-Context</summary>
    /// <param name="client"></param>
    /// <param name="accountName"></param>
    internal static void InitializeAccount(IRealmClient client, string accountName)
    {
      if(!client.IsConnected)
        return;
      if(ServerApp<RealmServer>.Instance.IsAccountLoggedIn(accountName))
      {
        log.Info("Client ({0}) tried to use online Account: {1}.", client,
          accountName);
        AuthenticationHandler.OnLoginError(client, AccountStatus.AccountInUse);
        client.Disconnect(false);
      }
      else
      {
        if(client.ClientAddress == null)
          return;
        AccountInfo accountInfo = new AccountInfo
        {
          AccountId = client.AuthAccount.AccountId,
          EmailAddress = "",
          LastIP = client.ClientAddress.GetAddressBytes(),
          LastLogin = DateTime.Now,
          RoleGroupName = client.AuthAccount.RoleGroupName
        };
        RealmAccount acc = new RealmAccount(accountName, accountInfo);
        ServerApp<RealmServer>.Instance.RegisterAccount(acc);
        acc.LoadCharacters();
        acc.LoadAccountData();
        acc.Client = client;
        client.Account = acc;
        log.Info("Account \"{0}\" logged in from {1}.", accountName,
          client.ClientAddress);
      }
    }

    internal void OnLogin()
    {
      AccountHandler loggedIn = LoggedIn;
      if(loggedIn == null)
        return;
      loggedIn(this);
    }

    internal void OnLogout()
    {
      if(AccountData != null)
        AccountData.Update();
      AccountHandler loggedOut = LoggedOut;
      if(loggedOut == null)
        return;
      loggedOut(this);
    }

    /// <summary>Is called when the Account logs in</summary>
    public static event AccountHandler LoggedIn;

    /// <summary>Is called when the Account logs out</summary>
    public static event AccountHandler LoggedOut;

    public delegate void AccountHandler(RealmAccount acc);
  }
}