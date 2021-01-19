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

			var cacheKey = indexViewModel.SearchString.Trim() + indexViewModel.SearchOption.Trim() + indexViewModel.SearchArea.Trim();

			//if (indexViewModel.SearchOption == "Title")
			//{
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
				return View(searchResults);
			//}
			//else
			//{
			//	if (!_memoryCache.TryGetValue(cacheKey, out IndexViewModel searchResults))
			//	{
			//		searchResults = await search.SearchPhotographsByTitle(indexViewModel);
			//
			//		var cachExpirationOptions = new MemoryCacheEntryOptions
			//		{
			//			AbsoluteExpiration = DateTime.Now.AddHours(6),
			//			Priority = CacheItemPriority.Normal,
			//			SlidingExpiration = TimeSpan.FromMinutes(5)
			//		};
			//
			//		_memoryCache.Set(cacheKey, searchResults, cachExpirationOptions);
			//	}
			//	return View(searchResults);
			//}
		}

		[HttpGet]
		[Route("ShowResults")]
		public async Task<IActionResult> ShowResults(string searchString, string searchOption, string searchArea)
		{
			if (!ModelState.IsValid)
			{
				IndexViewModel indexViewModel = new IndexViewModel();
				return View(indexViewModel);
			}

			var cacheKey = searchString.Trim();

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

			//return RedirectToAction("Index", searchResults);
			return View(searchResults);
		}

		[HttpGet]
		[Route("Next")]
		public async Task<IActionResult> Next(string searchString, string accessionNo, string direction, string searchOption, string searchArea)
		{
			var cacheKey = searchString.Trim() + searchOption + searchArea;

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
				Photograph currentPhotograph = searchResults.Photographs.Where(x => x.AccessionNo == accessionNo.Trim()).FirstOrDefault();
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
			return RedirectToAction("Photograph", new { accessionno = nextPhotograph.AccessionNo, searchString = searchString, searchOption = searchOption, searchArea = searchArea });
		}

		[HttpGet]
		public async Task<IActionResult> PhotographAsync(string accessionno, string searchString, string searchOption, string searchArea)
		{
			var cacheKey = accessionno.Trim();

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
			}

			return View(photograph);
		}

		[HttpGet]
		[Route("ClassSearch")]
		public async Task<IActionResult> ClassSearch(string classNo)
		{
			var cacheKey = classNo.Trim();

			if (!_memoryCache.TryGetValue(cacheKey, out List<Photograph> photographs))
			{
				photographs = await search.GetPhotographsByClassNo(classNo.ToString());

				var cachExpirationOptions = new MemoryCacheEntryOptions
				{
					AbsoluteExpiration = DateTime.Now.AddHours(6),
					Priority = CacheItemPriority.Normal,
					SlidingExpiration = TimeSpan.FromMinutes(5)
				};

				_memoryCache.Set(cacheKey, photographs, cachExpirationOptions);
			}

			return View(photographs);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
