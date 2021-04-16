using System.Collections.Generic;
using System.ComponentModel;

namespace SIAMVC.Models
{
	public class Photograph
	{
		[DisplayName("Image ID")]
		public string AccessionNo { get; set; }
		public string NextAccessionNo { get; set; }
		public string PrevAccessionNo { get; set; }
		public string Format { get; set; }
		public string Media { get; set; }
		public string Photographer { get; set; }
		[DisplayName("Storage area")]
		public string StorageArea { get; set; }
		public string Area { get; set; }
		[DisplayName("Available to buy?")]
		public string Availabletobuy { get; set; }
		[DisplayName("Class number")]
		public string Classno { get; set; }

		public string Dateentered { get; set; }
		[DisplayName("Date of image")]
		public string Dateofimage { get; set; }
		public string Description { get; set; }
		public string Idno { get; set; }
		[DisplayName("In copyright?")]
		public string Incopyright { get; set; }
		public string Staffid { get; set; }
		public string Title { get; set; }
		public string Viewcount { get; set; }
		public string url { get; set; }
		public string SearchString { get; set; }
		public string SearchOption { get; set; }
		public string SearchArea { get; set; }
		public List<Photograph> SearchResults { get; set; }
	}
}
