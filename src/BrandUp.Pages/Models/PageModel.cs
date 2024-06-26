using System.Text.Json.Serialization;

namespace BrandUp.Pages.Models
{
    public class PageModel
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Title { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PageStatus Status { get; set; }
        public string Url { get; set; }
    }

    public enum PageStatus
    {
        Draft,
        Published
    }

    public class BeginPageEditResult
    {
        public Guid EditId { get; set; }
        public DateTime? CurrentDate { get; set; }
        public List<ContentModel> Content { get; set; }
    }

    public class ContentModel
    {
        public string Parent { get; set; }
        public string Path { get; set; }
        public int Index { get; set; }
        public string TypeName { get; set; }
        public string TypeTitle { get; set; }
        public List<Field> Fields { get; set; }

        public abstract class Field
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public string Title { get; set; }
            public bool IsRequired { get; set; }
            public object Value { get; set; }
        }

        public class TextField : Field
        {
        }

        public class HtmlField : Field
        {
        }

        public class HyperLinkField : Field
        {
        }

        public class ImageField : Field
        {
        }

        public class PagesField : Field
        {
        }

        public class ModelField : Field
        {
            public bool IsList { get; set; }
            public List<ItemType> ItemTypes { get; set; }
            public string AddText { get; set; }

            public class ItemType
            {
                public string Name { get; set; }
                public string Title { get; set; }
            }
        }
    }
}