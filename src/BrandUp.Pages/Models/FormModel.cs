namespace BrandUp.Pages.Models
{
	public class FormModel<TValues>
		where TValues : class, new()
	{
		public TValues Values { get; set; } = new TValues();
	}

	public class ComboBoxItem
	{
		public string Value { get; set; }
		public string Title { get; set; }

		public ComboBoxItem(string value, string title)
		{
			Value = value;
			Title = title;
		}
	}
}