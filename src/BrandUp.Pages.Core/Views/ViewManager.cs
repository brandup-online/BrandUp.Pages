using System;
using System.Collections.Generic;

namespace BrandUp.Pages.Views
{
    public class ViewManager
    {
        private readonly List<ContentViewProvider> providers = new List<ContentViewProvider>();
        private readonly Dictionary<Type, int> providersByContentTypes = new Dictionary<Type, int>();

        public ViewManager(IViewLocator viewLocator)
        {
            if (viewLocator == null)
                throw new ArgumentNullException(nameof(viewLocator));
        }
    }

    public interface IViewLocator
    {

    }

    public class ContentViewProvider
    {

    }
}