using System.ComponentModel.DataAnnotations;

namespace BrandUp.Pages.Models
{
    public class EditorListModel
    {
    }

    public class ContentEditorModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
    }

    public class ContentEditorAssignForm : FormModel<ContentEditorAssignValues>
    {
    }

    public class ContentEditorAssignValues
    {
        [Required(AllowEmptyStrings = false), EmailAddress]
        public string Email { get; set; }
    }
}