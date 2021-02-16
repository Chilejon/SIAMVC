﻿using SIAMVC.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using SIAMVC.Enums;
using System.Net;

namespace SIAMVC.Services
{
	public class Search
	{
		HttpClient client = new HttpClient();
		private const string SearchURL = "https://interactive.stockport.gov.uk/siarestapi/v1/GetPhotosByTitle";
		private const string GetPhoto = "https://interactive.stockport.gov.uk/siarestapi/v1/GetPhotosByAccNo";
		private const string GetClassNo = "https://interactive.stockport.gov.uk/siarestapi/v1/GetPhotosByClassNo";
		private const string GetAllPhoto = "https://interactive.stockport.gov.uk/siarestapi/v1/GetPhotosByTerm";
		private const string GetPhotoByTermArea = "https://interactive.stockport.gov.uk/siarestapi/v1/GetPhotosByTermArea";
		private const string GetPhotoByTitleArea = "https://interactive.stockport.gov.uk/siarestapi/v1/GetPhotosByTitleArea";
		private const string GetPhotosByID = "https://interactive.stockport.gov.uk/siarestapi/v1/GetPhotosByID";

		private string urlParameters = "?term=";
		private string idParameters = "?id=";
		private string areaParameters = "&area=";

		public async Task<IndexViewModel> SearchPhotographsByTitle(IndexViewModel indexViewModel)
		{
			if (indexViewModel.SearchString.Length < 3)
			{
				indexViewModel.Message = "Search term is too short : " + indexViewModel.SearchString;
				return indexViewModel;
			}

			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			HttpResponseMessage response = new HttpResponseMessage();

			if (indexViewModel.SearchArea == "All")
			{
				if (indexViewModel.SearchOption == "Title")
				{
					client.BaseAddress = new Uri(SearchURL);
					response = client.GetAsync(urlParameters + indexViewModel.SearchString).Result;
				}
				else if (indexViewModel.SearchOption == "All")
				{
					client.BaseAddress = new Uri(GetAllPhoto);
					response = client.GetAsync(urlParameters + indexViewModel.SearchString).Result;
				}
				else if (indexViewModel.SearchOption == "IDNumber")
				{
					client.BaseAddress = new Uri(GetPhotosByID);
					response = client.GetAsync(idParameters + indexViewModel.SearchString.Trim()).Result;
				}
			}
			else
			{
				if (indexViewModel.SearchOption == "Title")
				{
					client.BaseAddress = new Uri(GetPhotoByTitleArea);
					response = client.GetAsync(urlParameters + indexViewModel.SearchString + areaParameters + indexViewModel.SearchArea).Result;
				}
				else if (indexViewModel.SearchOption == "All")
				{
					client.BaseAddress = new Uri(GetPhotoByTermArea);
					response = client.GetAsync(urlParameters + indexViewModel.SearchString + areaParameters + indexViewModel.SearchArea).Result;
				}
				else if (indexViewModel.SearchOption == "IDNumber")
				{
					client.BaseAddress = new Uri(GetPhotosByID);
					response = client.GetAsync(idParameters + indexViewModel.SearchString.Trim()).Result;
				}
			}

			if (response.IsSuccessStatusCode && indexViewModel.SearchOption != "IDNumber")
			{
				var listPhotographs = response.Content.ReadAsStringAsync().Result;
				var photographs = JsonConvert.DeserializeObject<List<Photograph>>(listPhotographs);
				if (photographs != null)
				{
					foreach (var photograph in photographs)
					{
						photograph.url = "http://interactive.stockport.gov.uk/stockportimagearchive/SIA/thumbnails/" + photograph.AccessionNo.Trim() + ".jpg";
						photograph.SearchResults = photographs;
						photograph.SearchString = indexViewModel.SearchString.Trim();
						photograph.AccessionNo = photograph.AccessionNo.Trim();
						photograph.SearchOption = indexViewModel.SearchOption;
						photograph.SearchArea = indexViewModel.SearchArea;
					}

					indexViewModel.Photographs = photographs;
					indexViewModel.Message = string.Empty;
				}
				else
				{
					indexViewModel.Message = "No search results for : " + indexViewModel.SearchString;
				}
				return indexViewModel;
			}
			if (response.IsSuccessStatusCode && indexViewModel.SearchOption == "IDNumber")
			{
				var listPhotographs = response.Content.ReadAsStringAsync().Result;
				var photograph = JsonConvert.DeserializeObject<Photograph>(listPhotographs);
				List<Photograph> photographs = new List<Photograph>();
				photographs.Add(photograph);
				photographs[0].url = "http://interactive.stockport.gov.uk/stockportimagearchive/SIA/thumbnails/" + photograph.AccessionNo.Trim() + ".jpg";
				photographs[0].SearchResults = photographs;
				photographs[0].SearchString = indexViewModel.SearchString.Trim();
				photographs[0].AccessionNo = photograph.AccessionNo.Trim();
				photographs[0].SearchOption = indexViewModel.SearchOption;
				photographs[0].SearchArea = indexViewModel.SearchArea;
				indexViewModel.Photographs = photographs;
				indexViewModel.Message = string.Empty;
			}

			if (!response.IsSuccessStatusCode)
			{
				indexViewModel.Message = "No search results for : " + indexViewModel.SearchString;
				return indexViewModel;
			}
			else
			{
				Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
			}
			client.Dispose();
			return indexViewModel;
		}

		public async Task<IndexViewModel> SearchPhotographsByTitle(string searchString, string searchOptions, string searchArea)
		{
			IndexViewModel indexViewModel = new IndexViewModel();
			if (searchString.Length < 3)
			{
				indexViewModel.Message = "Search term is too short : " + indexViewModel.SearchString;
				return indexViewModel;
			}

			client.BaseAddress = new Uri(SearchURL);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			HttpResponseMessage response = client.GetAsync(urlParameters + indexViewModel.SearchString).Result;
			if (response.IsSuccessStatusCode)
			{
				var listPhotographs = response.Content.ReadAsStringAsync().Result;
				var photographs = JsonConvert.DeserializeObject<List<Photograph>>(listPhotographs);
				if (photographs != null)
				{
					foreach (var photograph in photographs)
					{
						photograph.url = "http://interactive.stockport.gov.uk/stockportimagearchive/SIA/thumbnails/" + photograph.AccessionNo.Trim() + ".jpg";
						photograph.SearchResults = photographs;
					}

					indexViewModel.Photographs = photographs;
					indexViewModel.Message = string.Empty;
				}
				else
				{
					indexViewModel.Message = "No search results for : " + indexViewModel.SearchString;
				}
				return indexViewModel;
			}
			else
			{
				Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
			}
			client.Dispose();
			return indexViewModel;
		}

		public async Task<List<Photograph>> GetPhotographsByClassNo(string classNo)
		{
			client.BaseAddress = new Uri(GetClassNo);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			HttpResponseMessage response = client.GetAsync(idParameters + classNo).Result;
			List<Photograph> allPhotographs = new List<Photograph>();

			if (response.IsSuccessStatusCode)
			{
				var listPhotographs = response.Content.ReadAsStringAsync().Result;
				var photographs = JsonConvert.DeserializeObject<List<Photograph>>(listPhotographs);
				if (photographs != null)
				{
					foreach (var photograph in photographs)
					{
						photograph.url = "http://interactive.stockport.gov.uk/stockportimagearchive/SIA/thumbnails/" + photograph.AccessionNo.Trim() + ".jpg";
					}
				}
				else
				{
				}
				allPhotographs = photographs;
			}
			else
			{
				Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
			}
			client.Dispose();
			return allPhotographs;
		}

		private string urlcheckImage(Photograph photograph)
		{
			HttpWebResponse httpResponse = null;
			WebRequest webRequest = WebRequest.Create(photograph.url);
			webRequest.Timeout = 1200;
			webRequest.Method = "Head";

			try
			{
				var resp = (HttpWebResponse)webRequest.GetResponse();
				return photograph.url;
			}
			catch (Exception ex)
			{
				return "../assets/images/NotFound.jpg";
			}
		}

		public async Task<Photograph> GetPhotographsByAccessionNo(string accessionNo, string searchString, string searchOption, string searchArea)
		{
			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(GetPhoto);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			HttpResponseMessage response = client.GetAsync(idParameters + accessionNo).Result;

			if (response.IsSuccessStatusCode)
			{
				var listPhotographs = response.Content.ReadAsStringAsync().Result;
				var photograph = JsonConvert.DeserializeObject<List<Photograph>>(listPhotographs).FirstOrDefault();
				var area = photograph.Area.Replace(".0", "");
				int areaInt = int.Parse(area);
				var photographArea = (Areas.areas)(areaInt);
				photograph.Area = photographArea.ToString();
				photograph.url = "http://interactive.stockport.gov.uk/stockportimagearchive/SIA/" + photograph.AccessionNo.Trim() + ".jpg";
				photograph.url = urlcheckImage(photograph);
				photograph.SearchString = searchString;
				photograph.SearchOption = searchOption;
				photograph.SearchArea = searchArea;

				return photograph;
			}
			else
			{
				Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
			}
			client.Dispose();
			return null;
		}
	}
}