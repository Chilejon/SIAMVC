using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static SIAMVC.Enums.Areas;

namespace SIAMVC.Models
{
	public class IndexViewModel
	{
		[Required(ErrorMessage="Give us something to search")]
		[MaxLength(20, ErrorMessage="Reduce search string")]
		public string SearchString { get; set; }
		public string Message { get; set; }
		public string SearchOption { get; set; }
		public string SearchArea { get; set; }
		public List<Photograph> Photographs { get; set; } = new List<Photograph>();
	}
}
