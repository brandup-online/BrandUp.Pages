﻿namespace BrandUp.Pages.Content
{
	public static class ContentMetadataProviderExtensions
	{
		public static IEnumerable<ContentMetadataProvider> GetDerivedMetadataWithHierarhy(this ContentMetadataProvider contentMetadata, bool includeCurrent)
		{
			if (includeCurrent)
				yield return contentMetadata;

			foreach (var derivedContentMetadata in contentMetadata.DerivedContents)
			{
				yield return derivedContentMetadata;

				foreach (var childDerivedContentMetadata in GetDerivedMetadataWithHierarhy(derivedContentMetadata, false))
					yield return childDerivedContentMetadata;
			}
		}
		public static bool TryGetField<TField>(this ContentMetadataProvider contentMetadata, string fieldName, out TField field)
			where TField : class, Fields.IFieldProvider
		{
			if (!contentMetadata.TryGetField(fieldName, out Fields.IFieldProvider f))
			{
				field = null;
				return false;
			}

			if (!(f is TField))
				throw new ArgumentException();

			field = (TField)f;
			return true;
		}
	}
}