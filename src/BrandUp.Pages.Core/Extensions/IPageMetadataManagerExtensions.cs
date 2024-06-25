﻿namespace BrandUp.Pages.Metadata
{
	public static class IPageMetadataManagerExtensions
	{
		public static PageMetadataProvider GetMetadata(this PageMetadataManager pageMetadataManager, string name)
		{
			var pageMetadataProvider = pageMetadataManager.FindPageMetadataByName(name);
			if (pageMetadataProvider == null)
				throw new ArgumentException($"Тип страницы {name} не найден.", nameof(pageMetadataProvider));
			return pageMetadataProvider;
		}

		public static PageMetadataProvider GetMetadata(this PageMetadataManager pageMetadataManager, Type modelType)
		{
			var pageMetadataProvider = pageMetadataManager.FindPageMetadataByContentType(modelType);
			if (pageMetadataProvider == null)
				throw new ArgumentException($"Тип страницы с типом {modelType.AssemblyQualifiedName} не найден.", nameof(pageMetadataProvider));
			return pageMetadataProvider;
		}

		public static PageMetadataProvider GetMetadata<TModel>(this PageMetadataManager pageMetadataManager)
		{
			return GetMetadata(pageMetadataManager, typeof(TModel));
		}
	}
}