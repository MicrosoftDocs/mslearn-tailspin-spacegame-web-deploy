using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TailSpin.SpaceGame.Web.Models;

namespace TailSpin.SpaceGame.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDocumentDBRepository _dbRespository;
        public HomeController(IDocumentDBRepository dbRepository)
        {
            _dbRespository = dbRepository;
        }

        public async Task<IActionResult> Index(
            int page = 1, 
            int pageSize = 10, 
            string mode = "",
            string region = ""
            )
        {
            // Create the view model with initial values we already know.
            var vm = new LeaderboardViewModel
            {
                Page = page,
                PageSize = pageSize,
                SelectedMode = mode,
                SelectedRegion = region,

                GameModes = new List<string>()
                {
                    "Solo",
                    "Duo",
                    "Trio"
                },

                GameRegions = new List<string>()
                {
                    "Milky Way",
                    "Andromeda",
                    "Pinwheel",
                    "NGC 1300",
                    "Messier 82",
                }
            };

            try
            {
                // Fetch the total number of results in the background.
                var countItemsTask = _dbRespository.CountScoresAsync(mode, region);

                // Fetch the scores that match the current filter.
                IEnumerable<Score> scores = await _dbRespository.GetScoresAsync(mode, region, page, pageSize);

                // Wait for the total count.
                vm.TotalResults = await countItemsTask;

                // Fetch the user profile for each score.
                // This creates a list that's parallel with the scores collection.
                var profiles = new List<Task<Profile>>();
                foreach (var score in scores)
                {
                    profiles.Add(_dbRespository.GetProfileAsync(score.ProfileId));
                }
                Task<Profile>.WaitAll(profiles.ToArray());

                // Combine each score with its profile.
                vm.Scores = scores.Zip(profiles, (score, profile) => new ScoreProfile { Score = score, Profile = profile.Result });

                return View(vm);
            }
            catch (Exception)
            {
                return View(vm);
            }
        }

        [Route("/profile/{id}")]
        public async Task<IActionResult> Profile(string id, string rank="")
        {
            try
            {
                // Fetch the user profile with the given identifier.
                return View(new ProfileViewModel { Profile = await _dbRespository.GetProfileAsync(id), Rank = rank });
            }
            catch (Exception)
            {
                return RedirectToAction("/");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}