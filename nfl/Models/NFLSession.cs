using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NFL.Models
{
  public class NFLSession
  {
    private const string TeamsKey = "myteams";
    private const string CountKey = "teamcount";
    private const string ConfKey = "conf";
    private const string DivKey = "div";

    private ISession Session { get; set; }

    public NFLSession(ISession session)
    {
      Session = session;
    }

    public void SetMyTeams(List<Team> teams)
    {
      Session.SetObject(TeamsKey, teams);
      Session.SetInt32(CountKey, teams.Count);
    }

    public List<Team> GetMyTeams() => Session.GetObject<List<Team>>(TeamsKey) ?? new List<Team>();
    public int? GetMyTeamCount() => Session.GetInt32(CountKey);

    public void SetActiveConf(string conference) => Session.SetString(ConfKey, conference);
    public string GetActiveConf() => Session.GetString(ConfKey);

    public void SetActiveDiv(string division) => Session.SetString(DivKey, division);
    public string GetActiveDiv() => Session.GetString(DivKey);

    public void RemoveMyTeams()
    {
      Session.Remove(TeamsKey);
      Session.Remove(CountKey);
    }
  }
}
