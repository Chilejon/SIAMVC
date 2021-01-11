using System.Collections.Generic;

namespace SIAMVC.Models
{
	public class Photograph
	{
		public string AccessionNo { get; set; }
		public string NextAccessionNo { get; set; }
		public string PrevAccessionNo { get; set; }
		public string Format { get; set; }
		public string Media { get; set; }
		public string Photographer { get; set; }
		public string StorageArea { get; set; }
		public string Area { get; set; }
		public string Availabletobuy { get; set; }
		public string Classno { get; set; }
		public string Dateentered { get; set; }
		public string Dateofimage { get; set; }
		public string Description { get; set; }
		public string Idno { get; set; }
		public string Incopyright { get; set; }
		public string Staffid { get; set; }
		public string Title { get; set; }
		public string Viewcount { get; set; }
		public string url { get; set; }
		public List<Photograph> SearchResults { get; set; }
	}
}
