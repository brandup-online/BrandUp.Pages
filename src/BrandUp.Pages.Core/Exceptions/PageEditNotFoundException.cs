namespace BrandUp.Pages
{
	/// <summary>
	/// Thrown when an operation targets a page edit session that no longer exists —
	/// e.g. the session was committed or discarded (often from another tab) while the
	/// client still holds its <c>editId</c>. Treated as a client conflict, not a server error.
	/// </summary>
	public class PageEditNotFoundException : InvalidOperationException
	{
		/// <summary>Identifier of the edit session that was not found.</summary>
		public Guid EditId { get; }

		public PageEditNotFoundException(Guid editId)
			: base($"Edit session \"{editId}\" was not found. It may have already been committed or discarded.")
		{
			EditId = editId;
		}
	}
}
