using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NFL.Models;

namespace NFL.Controllers
{
  public class HomeController : Controller
  {
    private TeamContext context;

    public HomeController(TeamContext ctx)
    {
      context = ctx;
    }

    public IActionResult Index(string activeConf = "all", string activeDiv = "all")
    {
      var session = new NFLSession(HttpContext.Session);
      session.SetActiveConf(activeConf);
      session.SetActiveDiv(activeDiv);

      int? count = session.GetMyTeamCount();

      if (count == null)
      {
        var cookies = new NFLCookies(Request.Cookies);
        string[] ids = cookies.GetMyTeamsIds();

        List<Team> myteams = new List<Team>();
        if (ids.Length > 0)
        {
          myteams = context.Teams
            .Include(c => c.Conference)
            .Include(d => d.Division)
            .Where(t => ids.Contains(t.TeamID))
            .ToList();
        }

        session.SetMyTeams(myteams);
      }

      var data = new TeamListViewModel
      {
        ActiveConf = activeConf,
        ActiveDiv = activeDiv,
        Conferences = context.Conferences.ToList(),
        Divisions = context.Divisions.ToList()
      };

      IQueryable<Team> query = context.Teams;

      if (activeConf != "all")
      {
        query = query.Where(t => t.Conference.ConferenceID.ToLower() == activeConf.ToLower());
      }

      if (activeDiv != "all")
      {
        query = query.Where(d => d.Division.DivisionID.ToLower() == activeDiv.ToLower());
      }

      data.Teams = query.ToList();

      return View(data);
    }

    public IActionResult Details (string id)
    {
      var session = new NFLSession(HttpContext.Session);

      var model = new TeamViewModel
      {
        Team = context.Teams
                      .Include(c => c.Conference)
                      .Include(d => d.Division)
                      .FirstOrDefault(t => t.TeamID == id),
        ActiveConf = session.GetActiveConf(),
        ActiveDiv = session.GetActiveDiv()
      };

      return View(model);
    }

    [HttpPost]
    public RedirectToActionResult Add(TeamViewModel model)
    {
      model.Team = context.Teams
                          .Include(c => c.Conference)
                          .Include(d => d.Division)
                          .Where(t => t.TeamID == model.Team.TeamID)
                          .FirstOrDefault();

      var session = new NFLSession(HttpContext.Session);
      var teams = session.GetMyTeams();
      teams.Add(model.Team);
      session.SetMyTeams(teams);

      var cookies = new NFLCookies(Response.Cookies);
      cookies.SetMyTeamIds(teams);

      TempData["message"] = $"{model.Team.Name} was added to your favorites";

      return RedirectToAction("Index", new { ActiveConf = session.GetActiveConf(), ActiveDiv = session.GetActiveDiv() });
    }
  }
}
