﻿using LanAdept.Views.Tournament.ModelController;
using LanAdeptCore.Attribute.Authorization;
using LanAdeptData.DAL;
using LanAdeptData.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace LanAdept.Views.Team.ModelController
{
	public class TeamController : Controller
	{
		UnitOfWork uow = UnitOfWork.Current;


		[Authorize]
		public ActionResult Index()
		{
			int TeamLeaderID = LanAdeptCore.Service.UserService.GetLoggedInUser().UserID;
			IEnumerable<LanAdeptData.Model.Team> teams = uow.TeamRepository.GetByTeamLeaderID(TeamLeaderID);

			IndexTeamModel model = new IndexTeamModel();
			model.Teams = new List<TeamDemandeModel>();

			foreach (LanAdeptData.Model.Team team in teams)
			{
				TeamDemandeModel tdm = new TeamDemandeModel();

				tdm.Team = team;

				tdm.Demandes = new List<Demande>();

				foreach (Demande demande in uow.DemandeRepository.Get())
				{
					if (demande.Team.TeamID == team.TeamID)
					{
						tdm.Demandes.Add(demande);
					}
				}

				model.Teams.Add(tdm);
			}

			return View(model);
		}

		[AuthorizePermission("user.tournament.team.details")]
		public ActionResult DetailsTeam(int? teamId)
		{
			if (teamId == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			DetailsTeamModel team = new DetailsTeamModel();

			LanAdeptData.Model.Team teamToAdd = uow.TeamRepository.GetByID(teamId);

			team.GamerTags = teamToAdd.GamerTags;
			team.TeamID = teamToAdd.TeamID;
			team.TournamentID = teamToAdd.TournamentID;
			team.MaxPlayerPerTeam = teamToAdd.Tournament.MaxPlayerPerTeam;
			team.Name = teamToAdd.Name;
			team.Tag = teamToAdd.Tag;
			team.TeamLeaderTag = teamToAdd.TeamLeaderTag;

			List<Demande> demandes = new List<Demande>();

			foreach (Demande demande in uow.DemandeRepository.Get())
			{
				if (demande.Team.TeamID == teamToAdd.TeamID)
				{
					demandes.Add(demande);
				}
			}

			team.Demandes = demandes;


			if (team == null)
			{
				return HttpNotFound();
			}

			return View(team);
		}

		[AuthorizePermission("user.tournament.team.kick")]
		public ActionResult KickPlayer(int? gamerTagId, int? teamId)
		{
			GamerTag gamerTag = uow.GamerTagRepository.GetByID(gamerTagId);
			LanAdeptData.Model.Team team = uow.TeamRepository.GetByID(teamId);

			if (team.TeamLeaderTag == gamerTag || team.GamerTags.Count == 1)
			{
				TempData["ErrorMessage"] = "Vous ne pouvez pas kicker le team leader.";
				return RedirectToAction("DetailsTeam", new { teamId = teamId });
			}
			else
			{
				team.GamerTags.Remove(gamerTag);

				uow.TeamRepository.Update(team);
				uow.GamerTagRepository.Update(gamerTag);
				uow.Save();
			}

			return RedirectToAction("DetailsTeam", new { teamId = teamId });
		}

		[Authorize]
		public ActionResult AcceptTeamMember(int gamerTagId, int teamId)
		{
			LanAdeptData.Model.Team team = uow.TeamRepository.GetByID(teamId);
			if (team.GamerTags.Count < team.Tournament.MaxPlayerPerTeam)
			{
				GamerTag gamer = uow.GamerTagRepository.GetByID(gamerTagId);

				team.GamerTags.Add(gamer);

				uow.TeamRepository.Update(team);

				List<Demande> demandes = uow.DemandeRepository.GetByGamerTagId(gamerTagId);

				foreach (Demande demande in demandes)
				{
					if (demande.Team.Tournament.TournamentID == team.Tournament.TournamentID)
					{
						uow.DemandeRepository.Delete(demande);
					}
				}

				uow.Save();
			}
			return RedirectToAction("DetailsTeam", new { teamId = teamId });
		}

		[Authorize]
		public ActionResult RefuseTeamMember(int gamerTagId, int teamId)
		{
			GamerTag gamer = uow.GamerTagRepository.GetByID(gamerTagId);
			LanAdeptData.Model.Team team = uow.TeamRepository.GetByID(teamId);

			List<Demande> demandes = uow.DemandeRepository.GetByGamerTagId(gamerTagId);

			foreach (Demande demande in demandes)
			{
				if (demande.Team.TeamID == teamId)
				{
					uow.DemandeRepository.Delete(demande);
				}
			}

			uow.Save();

			return RedirectToAction("DetailsTeam", new { teamId = teamId });
		}
	}
}