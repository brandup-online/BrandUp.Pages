using BrandUp.Pages.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace BrandUp.Pages.Api.UiModels
{
    public class PublishPageModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Поле обязательно для заполнения.")]
        [StringLength(256, ErrorMessage = "Максимальная длинна 256 символов.")]
        public string Title { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Поле обязательно для заполнения.")]
        [StringLength(256, ErrorMessage = "Максимальная длинна 256 символов.")]
        [RegularExpression(@"^(\w|[-_])+$", ErrorMessage = "Можно использовать только буквенные символы, тире или подчёркивание.")]
        [PageUrlExsists]
        public string UrlPathName { get; set; }
    }

    public class PageUrlExsists : ValidationAttribute
    {
        public PageUrlExsists() : base("Страница с таким url уже существует.") { }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var urlPath = (string)value;
            if (string.IsNullOrEmpty(urlPath))
                return null;

            var pageRepository = (IPageRepositiry)validationContext.GetService(typeof(IPageRepositiry));

            var page = pageRepository.FindPageByPathAsync(urlPath).Result;
            if (page != null)
                return new ValidationResult(FormatErrorMessage(validationContext.MemberName));

            return ValidationResult.Success;
        }
    }
}