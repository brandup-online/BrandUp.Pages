using BrandUp.Pages.Content;
using BrandUp.Pages.Models;
using Microsoft.AspNetCore.Mvc;

namespace BrandUp.Pages.Controllers
{
	[Route("brandup.pages/content-type/list", Name = "BrandUp.Pages.ContentType.List"), Filters.Administration]
	public class ContentTypeListController : ListController<ContentTypeListModel, ContentTypeItemModel, ContentMetadataProvider, string>
	{
		readonly ContentMetadataManager contentMetadataManager;
		private ContentMetadataProvider contentMetadataProvider;

		public ContentTypeListController(ContentMetadataManager contentMetadataManager)
		{
			this.contentMetadataManager = contentMetadataManager ?? throw new ArgumentNullException(nameof(contentMetadataManager));
		}

		#region ListController members

		protected override Task OnInitializeAsync()
		{
			if (Request.Query.TryGetValue("baseType", out string baseTypeName))
			{
				if (!contentMetadataManager.TryGetMetadata(baseTypeName, out contentMetadataProvider))
				{
					AddErrors("Not found content type.");
					return Task.CompletedTask;
				}
			}

			return Task.CompletedTask;
		}

		protected override Task OnBuildListAsync(ContentTypeListModel listModel)
		{
			listModel.Parents = new List<string>();

			if (contentMetadataProvider != null)
			{
				var currentPage = contentMetadataProvider;
				while (currentPage != null)
				{
					listModel.Parents.Add(currentPage.Name);

					currentPage = currentPage.BaseMetadata;
				}

				listModel.Parents.Reverse();
			}

			return Task.CompletedTask;
		}

		protected override string ParseId(string value)
		{
			return value;
		}

		protected override Task<IEnumerable<ContentMetadataProvider>> OnGetItemsAsync(int offset, int limit)
		{
			if (contentMetadataProvider == null)
				return Task.FromResult(contentMetadataManager.MetadataProviders.Where(it => it.BaseMetadata == null));
			else
				return Task.FromResult(contentMetadataProvider.DerivedContents);
		}

		protected override Task<ContentMetadataProvider> OnGetItemAsync(string id)
		{
			contentMetadataManager.TryGetMetadata(id, out ContentMetadataProvider contentMetadataProvider);

			return Task.FromResult(contentMetadataProvider);
		}

		protected override Task<ContentTypeItemModel> OnGetItemModelAsync(ContentMetadataProvider item)
		{
			return Task.FromResult(new ContentTypeItemModel
			{
				Name = item.Name,
				Title = item.Title,
				IsAbstract = item.IsAbstract

			});
		}

		#endregion
	}
}