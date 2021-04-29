using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SIAMVC.Models;
using SIAMVC.Services;

namespace SIAMVC.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IMemoryCache _memoryCache;
		public Search search = new Search();

		public HomeController(ILogger<HomeController> logger, IMemoryCache memoryCache)
		{
			_logger = logger;
			_memoryCache = memoryCache;
		}

		//first load
		[HttpGet]
		public IActionResult Index()
		{
			IndexViewModel indexViewModel = new IndexViewModel();
			indexViewModel.SearchOption = "title";

			return View(indexViewModel);
		}

		//submit pressed
		[HttpPost]
		public async Task<IActionResult> Index(string searchString, string searchOption, string searchArea, string classNo)
		{
			if (string.IsNullOrEmpty(searchOption))
			{
				searchOption = "title";
			}

			IndexViewModel indexViewModel = new IndexViewModel();
			indexViewModel.SearchString = searchString;
			indexViewModel.SearchOption = searchOption;
			indexViewModel.SearchArea = searchArea;

			if (string.IsNullOrEmpty(searchString))
			{
				indexViewModel.Message = "No search term entered";
				return View(indexViewModel);
			}

			var cacheKey = indexViewModel.SearchString.Trim() + indexViewModel.SearchOption.Trim() + indexViewModel.SearchArea.Trim();

			if (!_memoryCache.TryGetValue(cacheKey, out IndexViewModel searchResults))
			{
				searchResults = await search.SearchPhotographsByTitle(indexViewModel);

				var cachExpirationOptions = new MemoryCacheEntryOptions
				{
					AbsoluteExpiration = DateTime.Now.AddHours(6),
					Priority = CacheItemPriority.Normal,
					SlidingExpiration = TimeSpan.FromMinutes(5)
				};

				_memoryCache.Set(cacheKey, searchResults, cachExpirationOptions);
			}

			searchResults.SearchString = indexViewModel.SearchString;
			//searchResults.SearchArea = "Bredbury";
			searchResults.SearchOption = indexViewModel.SearchOption;

			if (searchResults.Photographs.Count != 1)
			{
				return View(searchResults);
			}
			return RedirectToAction("Photograph", new { accessionno = searchResults.Photographs[0].AccessionNo, searchString = searchResults.SearchString, searchOption = searchResults.SearchOption, searchArea = searchResults.SearchArea, singleImage = true });
		}

		[HttpGet]
		public async Task<IActionResult> ShowResults(string searchString, string searchOption, string searchArea, string classno)
		{
			if (!ModelState.IsValid)
			{
				IndexViewModel indexViewModel = new IndexViewModel();
				return View(indexViewModel);
			}

			var cacheKey = searchString.Trim() + searchOption + searchArea + classno.Trim();

			if (!_memoryCache.TryGetValue(cacheKey, out IndexViewModel searchResults))
			{
				if (searchOption != "class")
				{
					searchResults = await search.SearchPhotographsByTitle(searchString, searchOption, searchArea);
				}
				else
				{
					searchResults = await search.GetPhotographsByClassNo2(classno);
				}

				var cachExpirationOptions = new MemoryCacheEntryOptions
				{
					AbsoluteExpiration = DateTime.Now.AddHours(6),
					Priority = CacheItemPriority.Normal,
					SlidingExpiration = TimeSpan.FromMinutes(5)
				};

				_memoryCache.Set(cacheKey, searchResults, cachExpirationOptions);
			}

			return View("Index", searchResults);

		}

		[HttpGet]
		[Route("Next")]
		public async Task<IActionResult> Next(string searchString, string accessionNo, string direction, string searchOption, string searchArea, string classNo, bool classSearch)
		{
			var cacheKey = string.Empty;
			if (classSearch)
			{
				cacheKey = classNo.Trim();
			}
			else
			{
				cacheKey = searchString.Trim() + searchOption + searchArea; // + classNo.Trim();
			}

			if (!_memoryCache.TryGetValue(cacheKey, out IndexViewModel searchResults))
			{
				searchResults = await search.SearchPhotographsByTitle(searchString, searchOption, searchArea);

				var cachExpirationOptions = new MemoryCacheEntryOptions
				{
					AbsoluteExpiration = DateTime.Now.AddHours(6),
					Priority = CacheItemPriority.Normal,
					SlidingExpiration = TimeSpan.FromMinutes(5)
				};

				_memoryCache.Set(cacheKey, searchResults, cachExpirationOptions);
			}

			Photograph nextPhotograph = searchResults.Photographs.FirstOrDefault();
			try
			{
				Photograph currentPhotograph = searchResults.Photographs.Where(x => x.AccessionNo.Trim() == accessionNo.Trim()).FirstOrDefault();
				int indexOfCurrentImage = searchResults.Photographs.IndexOf(currentPhotograph);

				if (direction == "next")
				{
					nextPhotograph = searchResults.Photographs[indexOfCurrentImage + 1];
				}
				else
				{
					nextPhotograph = searchResults.Photographs[indexOfCurrentImage - 1];

				}
			}
			catch (Exception)
			{
				if (direction == "next")
				{
					nextPhotograph = searchResults.Photographs.Last();
				}
			}
			return RedirectToAction("Photograph", new { accessionno = nextPhotograph.AccessionNo.Trim(), searchString = searchString, searchOption = searchOption, searchArea = searchArea, classSearch = classSearch });
		}

		[HttpGet]
		public async Task<IActionResult> PhotographAsync(string accessionno, string searchString, string searchOption, string searchArea, bool singleImage, bool classSearch)
		{
			var cacheKey = accessionno.Trim();

			try
			{


				if (!_memoryCache.TryGetValue(cacheKey, out Photograph photograph))
				{
					photograph = await search.GetPhotographsByAccessionNo(accessionno.ToString(), searchString, searchOption, searchArea);

					var cachExpirationOptions = new MemoryCacheEntryOptions
					{
						AbsoluteExpiration = DateTime.Now.AddHours(6),
						Priority = CacheItemPriority.Normal,
						SlidingExpiration = TimeSpan.FromMinutes(5)
					};

					_memoryCache.Set(cacheKey, photograph, cachExpirationOptions);

					photograph.singleImage = singleImage;
					if (string.IsNullOrEmpty(searchString) && !classSearch && string.IsNullOrEmpty(searchOption)) photograph.singleImage = true;
					photograph.ClassSearch = classSearch;
				}

				if (photograph.url != "./assets/images/NotFound.jpg")
				{
					return View(photograph);
				}
				else
				{
					return RedirectToAction("Next", new { accessionno = accessionno, direction = "next", searchString = searchString, searchOption = searchOption, searchArea = searchArea, classSearch = classSearch });
				}

			}
			catch (Exception)
			{
				return RedirectToAction("Index");

				throw;
			}
		}

		[HttpGet]
		[Route("ClassSearch")]
		public async Task<IActionResult> ClassSearch(string classNo, string searchString, string searchOption, string searchArea, bool classSearch)
		{
			//var cacheKey = searchString.Trim() + searchOption + searchArea + classNo.Trim();
			var cacheKey = classNo.Trim();

			if (!_memoryCache.TryGetValue(cacheKey, out IndexViewModel searchResults)) //out List<Photograph> photographs
			{
				//photographs = await search.GetPhotographsByClassNo(classNo.ToString());
				searchResults = await search.GetPhotographsByClassNo2(classNo);

				var cachExpirationOptions = new MemoryCacheEntryOptions
				{
					AbsoluteExpiration = DateTime.Now.AddHours(6),
					Priority = CacheItemPriority.Normal,
					SlidingExpiration = TimeSpan.FromMinutes(5)
				};

				_memoryCache.Set(cacheKey, searchResults, cachExpirationOptions);
			}

			//IndexViewModel searchResults = new IndexViewModel();
			//searchResults.Photographs = photographs;
			searchResults.SearchString = searchString;
			searchResults.SearchArea = searchArea;
			searchResults.SearchOption = searchOption;
			searchResults.ClassSearch = classSearch;
			return View(searchResults);

			//return View(photographs);
		}

		[HttpGet]
		[Route("OrderAPrint")]
		public async Task<IActionResult> OrderAPrint(string classNo, string accessionNo)
		{
			OrderAPrintViewModel viewModel = new OrderAPrintViewModel();
			viewModel.accessionNo = accessionNo;
			viewModel.classNo = classNo;

			return View(viewModel);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
