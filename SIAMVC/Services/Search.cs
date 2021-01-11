using SIAMVC.Models;
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
		private const string GetPhoto = "http://interactive.stockport.gov.uk/siarestapi/v1/GetPhotosByAccNo";
		private const string GetClassNo = "http://interactive.stockport.gov.uk/siarestapi/v1/GetPhotosByClassNo";

		private string urlParameters = "?term=";
		private string idParameters = "?id=";

		public async Task<IndexViewModel> SearchPhotographsByTitle(IndexViewModel indexViewModel)
		{
			if (indexViewModel.SearchString.Length < 3)
			{
				indexViewModel.Message = "Search term is too short : " + indexViewModel.SearchString;
				return indexViewModel;
			}

			client.BaseAddress = new Uri(SearchURL);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			HttpResponseMessage response = client.GetAsync(urlParameters + indexViewModel.SearchString).Result;
			string prevAccessionNo = "0";
			string nextAccessionNo = "0";
			if (response.IsSuccessStatusCode)
			{
				var listPhotographs = response.Content.ReadAsStringAsync().Result;
				var photographs = JsonConvert.DeserializeObject<List<Photograph>>(listPhotographs);
				if (photographs != null)
				{
					foreach (var photograph in photographs)
					{
						photograph.url = "https://interactive.stockport.gov.uk/stockportimagearchive/SIA/" + photograph.AccessionNo.Trim() + ".jpg";
						photograph.url = urlcheckImage(photograph);
						
						if (!string.IsNullOrEmpty(prevAccessionNo))
						{
							photograph.PrevAccessionNo = prevAccessionNo;
							photograph.NextAccessionNo = nextAccessionNo;
						}
						prevAccessionNo = photograph.AccessionNo;

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
						photograph.url = "https://interactive.stockport.gov.uk/stockportimagearchive/SIA/" + photograph.AccessionNo.Trim() + ".jpg";
						//photograph.url = urlcheckImage(photograph);
						//photograph.SearchResults = photographs;
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

		public async Task<Photograph> GetPhotographsByAccessionNo(string accessionNo)
		{
			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(GetPhoto);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			HttpResponseMessage response = client.GetAsync(idParameters + accessionNo).Result;

			if (response.IsSuccessStatusCode)
			{
				var listPhotographs = response.Content.ReadAsStringAsync().Result;
				var photograph = JsonConvert.DeserializeObject<List<Photograph>>(listPhotographs).FirstOrDefault();
				var photographArea = (Areas.areas)int.Parse(photograph.Area.Replace(".0", ""));
				photograph.Area = photographArea.ToString();

				photograph.url = "https://interactive.stockport.gov.uk/stockportimagearchive/SIA/" + photograph.AccessionNo.Trim() + ".jpg";
				photograph.url = urlcheckImage(photograph);
				//photograph.PrevAccessionNo = prevAccessionNo;
				//photograph.NextAccessionNo = nextAccessionNo;

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