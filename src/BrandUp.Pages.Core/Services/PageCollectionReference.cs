using System;

namespace BrandUp.Pages
{
    public struct PageCollectionReference<TPageModel> : IPageCollectionReference
        where TPageModel : class
    {
        public Guid CollectionId { get; set; }

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