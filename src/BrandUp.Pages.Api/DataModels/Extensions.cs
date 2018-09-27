using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Metadata;

namespace BrandUp.Pages.Api.DataModels
{
    public static class DataModelExtensions
    {
        public static DataModels.PageCollectionModel CreateDataModel(this IPageCollection current)
        {
            return new DataModels.PageCollectionModel
            {
                Id = current.Id,
                Title = current.Title,
                PageTypeName = current.PageTypeName,
                PageSort = current.SortMode,
                PageId = current.PageId
            };
        }

        public static DataModels.PageTypeModel CreateDataModel(this PageMetadataProvider current)
        {
            return new DataModels.PageTypeModel
            {
                Name = current.Name,
                Title = current.Title
            };
        }

        public static DataModels.ContentViewModel CreateDataModel(this IContentView current)
        {
            return new DataModels.ContentViewModel
            {
                Name = current.Name,
                Title = current.Title,
                Description = current.Description
            };
        }

        public static DataModels.ContentMetadataModel CreateDataModel(this ContentMetadataProvider current)
        {
            return new DataModels.ContentMetadataModel
            {
                Name = current.Name,
                Title = current.Title
            };
        }
    }
}