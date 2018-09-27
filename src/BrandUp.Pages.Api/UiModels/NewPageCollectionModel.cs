using BrandUp.Pages.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace BrandUp.Pages.Api.UiModels
{
    public class NewPageCollectionModel
    {
        public Guid? PageId { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Поле обязательно для заполнения.")]
        [StringLength(256, ErrorMessage = "Максимальная длинна 256 символов.")]
        public string Title { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Поле обязательно для заполнения.")]
        public string PageTypeName { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Поле обязательно для заполнения.")]
        public PageSortMode PageSort { get; set; }
    }

    public class UpdatePageCollectionModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Поле обязательно для заполнения.")]
        [StringLength(256, ErrorMessage = "Максимальная длинна 256 символов.")]
        public string Title { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Поле обязательно для заполнения.")]
        public PageSortMode PageSort { get; set; }
    }
}