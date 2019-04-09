using System;

namespace BrandUp.Pages
{
    public readonly struct PageCollectionReference<TPageModel> : IPageCollectionReference
        where TPageModel : class
    {
        public Guid CollectionId { get; }

        public PageCollectionReference(Guid collectionId)
        {
            CollectionId = collectionId;
        }

        Guid IPageCollectionReference.CollectionId => CollectionId;
        Type IPageCollectionReference.PageModelType => typeof(TPageModel);
    }

    internal interface IPageCollectionReference
    {
        Guid CollectionId { get; }
        Type PageModelType { get; }
    }
}