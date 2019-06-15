using System;
using System.ComponentModel.DataAnnotations;

namespace BrandUp.Pages.Models
{
    public class EditorListModel
    {
    }

    public class PageEditorModel
    {
        public string Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Email { get; set; }
    }

    public class PageEditorAssignForm : FormModel<PageEditorAssignValues>
    {
    }

    public class PageEditorAssignValues
    {
        [Required(AllowEmptyStrings = false), EmailAddress]
        public string Email { get; set; }
    }
}