using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SIAMVC.Models;
using SIAMVC.Services;

namespace SIAMVC.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		public Search search = new Search();

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		[HttpGet]
		public IActionResult Index()
		{
			IndexViewModel indexViewModel = new IndexViewModel();

			return View(indexViewModel);
		}

		[HttpPost]
		public async Task<IActionResult> IndexAsync(IndexViewModel indexViewModel)
		{
			if (!ModelState.IsValid)
			{
				return View(indexViewModel);
			}

			indexViewModel = await search.SearchPhotographsByTitle(indexViewModel);
			return View(indexViewModel);
		}

		[HttpGet]
		public async Task<IActionResult> PhotographAsync(string accessionno)
		{
			Photograph photograph = await search.GetPhotographsByAccessionNo(accessionno.ToString());

			return View(photograph);
		}

		[HttpGet]
		[Route("ClassSearch")]
		public async Task<IActionResult> ClassSearch(string classNo)
		{
			List<Photograph> photographs = await search.GetPhotographsByClassNo(classNo);

			return View(photographs);
		}



		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
