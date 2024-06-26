﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BrandUp.Pages.Interfaces;

namespace BrandUp.Pages.Models
{
	public class PageCollectionCreateForm : FormModel<PageCollectionCreateValues>
	{
		public PageModel Page { get; set; }
		public List<ComboBoxItem> Sorts { get; set; }
		public List<ComboBoxItem> PageTypes { get; set; }
	}

	public class PageCollectionCreateValues
	{
		[Required(AllowEmptyStrings = false), MaxLength(150)]
		public string Title { get; set; }
		[Required]
		public string PageType { get; set; }
		[Required]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public PageSortMode Sort { get; set; }
	}
}