namespace BrandUp.Pages.Views
{
    public interface IViewLocator
    {
        ContentView FindView(Type contentType);
    }

    public class ContentView
    {
        public string Name { get; }
        public Type ContentType { get; }
        public IDictionary<string, object> DefaultModelData { get; }

        public ContentView(string name, Type contentType, IDictionary<string, object> defaultModelData)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            DefaultModelData = defaultModelData;
        }
    }
}