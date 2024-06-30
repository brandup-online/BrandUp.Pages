using System.Collections;
using System.ComponentModel.DataAnnotations;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Identity;
using BrandUp.Website;
using Microsoft.AspNetCore.Mvc;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/page/content"), Filters.Administration]
    public class EditContentController(ContentService contentService, IWebsiteContext websiteContext) : Controller
    {
        #region Actions

        [HttpPost("begin")]
        public async Task<IActionResult> BeginAsync([FromQuery] string key, [FromQuery] string type, [FromQuery] bool force, [FromServices] ContentMetadataManager contentMetadataManager, [FromServices] IAccessProvider accessProvider)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(type))
                return BadRequest();

            if (!contentMetadataManager.TryGetMetadata(type, out var contentMetadata))
                return BadRequest();

            var cancellationToken = HttpContext.RequestAborted;
            var websiteId = websiteContext.Website.Id;

            var result = new Models.Contents.BeginPageEditResult();

            var userId = await accessProvider.GetUserIdAsync(cancellationToken);
            var content = await contentService.FindContentAsync(websiteId, key, cancellationToken);

            IContentEdit currentEdit = null;
            if (content != null)
            {
                currentEdit = await contentService.FindEditByUserAsync(content, userId, cancellationToken);
                if (currentEdit != null)
                {
                    if (force)
                    {
                        await contentService.DiscardEditAsync(currentEdit, HttpContext.RequestAborted);
                        currentEdit = null;
                    }
                    else
                        result.CurrentDate = currentEdit.CreatedDate;
                }
            }

            if (currentEdit == null)
            {
                currentEdit = await contentService.BeginEditAsync(websiteId, key, userId, contentMetadata, cancellationToken);
                if (currentEdit == null)
                    return NotFound();
            }

            result.EditId = currentEdit.Id;

            var contentModel = await contentService.GetEditContentAsync(currentEdit, cancellationToken);
            var contentExplorer = ContentExplorer.Create(contentMetadataManager, contentModel);

            result.Content = [];
            await EnsureContentsAsync(contentExplorer, result.Content);

            return Ok(result);
        }

        [HttpGet("form")]
        public async Task<IActionResult> GetFormAsync([FromQuery] Guid editId, [FromQuery] string modelPath)
        {
            var editSession = await contentService.FindEditByIdAsync(editId, HttpContext.RequestAborted);
            if (editSession == null)
                return BadRequest();

            modelPath ??= string.Empty;

            var pageContent = await contentService.GetEditContentAsync(editSession);
            var pageContentContext = new ContentContext(editSession.ContentKey, pageContent, HttpContext.RequestServices, editSession);

            var contentContext = pageContentContext.Navigate(modelPath);
            if (contentContext == null)
                return BadRequest();

            var formModel = new Models.PageContentForm
            {
                Path = new Models.PageContentPath
                {
                    Name = contentContext.Explorer.Metadata.Name,
                    Title = contentContext.Explorer.Metadata.Title,
                    Index = contentContext.Explorer.Index,
                    ModelPath = contentContext.Explorer.ModelPath
                }
            };

            var path = formModel.Path;
            var explorer = contentContext.Explorer.Parent;
            while (explorer != null)
            {
                path.Parent = new Models.PageContentPath
                {
                    Name = explorer.Metadata.Name,
                    Title = explorer.Metadata.Title,
                    Index = explorer.Index,
                    ModelPath = explorer.ModelPath
                };

                explorer = explorer.Parent;
                path = path.Parent;
            }

            var fields = contentContext.Explorer.Metadata.Fields.ToList();
            foreach (var field in fields)
            {
                formModel.Fields.Add(new Models.ContentFieldModel
                {
                    Type = field.Type,
                    Name = field.Name,
                    Title = field.Title,
                    Options = field.GetFormOptions(contentContext.Services)
                });

                var modelValue = field.GetModelValue(contentContext.Content);
                var formValue = await field.GetFormValueAsync(modelValue, contentContext.Services);

                formModel.Values.Add(field.Name, formValue);
            }

            return Ok(formModel);
        }

        [HttpGet("changeType")]
        public async Task<IActionResult> ChangeModelTypeAsync([FromQuery] Guid editId, [FromQuery] string modelPath, [FromQuery] string modelType, [FromServices] ContentMetadataManager contentMetadataManager, [FromServices] Views.IViewLocator viewLocator)
        {
            if (modelType == null)
                return BadRequest();

            var editSession = await contentService.FindEditByIdAsync(editId, HttpContext.RequestAborted);
            if (editSession == null)
                return BadRequest();

            modelPath ??= string.Empty;

            var newModelType = contentMetadataManager.GetMetadata(modelType);

            var pageContent = await contentService.GetEditContentAsync(editSession, HttpContext.RequestAborted);
            var pageContentExplorer = ContentExplorer.Create(contentMetadataManager, pageContent);

            var contentExplorer = pageContentExplorer.Navigate(modelPath);
            if (contentExplorer == null)
                return BadRequest();

            contentExplorer.Field.ChangeType(contentExplorer.Model, modelType);

            return Ok();
        }

        [HttpPost("commit")]
        public async Task<IActionResult> CommitAsync([FromQuery] Guid editId, [FromServices] ContentMetadataManager contentMetadataManager)
        {
            var contentEdit = await contentService.FindEditByIdAsync(editId, HttpContext.RequestAborted);
            if (contentEdit == null)
                return BadRequest();

            var contentModel = await contentService.GetEditContentAsync(contentEdit, HttpContext.RequestAborted);
            var contentExplorer = ContentExplorer.Create(contentMetadataManager, contentModel);

            var result = new Models.Contents.CommitResult { IsSuccess = false, Validation = [] };

            await ValidateContentsAsync(contentExplorer, result.Validation);
            if (result.Validation.Count > 0)
                return Ok(result);

            await contentService.CommitEditAsync(contentEdit, HttpContext.RequestAborted);

            result.IsSuccess = true;
            result.Validation = null;

            return Ok(result);
        }

        [HttpPost("discard")]
        public async Task<IActionResult> DiscardAsync([FromQuery] Guid editId)
        {
            var editSession = await contentService.FindEditByIdAsync(editId, HttpContext.RequestAborted);
            if (editSession == null)
                return BadRequest();

            await contentService.DiscardEditAsync(editSession, HttpContext.RequestAborted);

            return Ok();
        }

        #endregion

        #region Helpers

        async Task EnsureContentsAsync(ContentExplorer contentExplorer, ICollection<Models.Contents.ContentModel> output)
        {
            ArgumentNullException.ThrowIfNull(contentExplorer);
            ArgumentNullException.ThrowIfNull(output);

            if (output.Count == 0 && !contentExplorer.IsRoot)
                throw new ArgumentException("Начать заполнение моделей контента можно только с рутового контента.");

            var serviceProvider = HttpContext.RequestServices;

            var content = new Models.Contents.ContentModel
            {
                Parent = contentExplorer.Parent?.ModelPath,
                Path = contentExplorer.ModelPath,
                Index = contentExplorer.Index,
                TypeName = contentExplorer.Metadata.Name,
                TypeTitle = contentExplorer.Metadata.Title,
                Fields = []
            };
            output.Add(content);

            var validationContext = new ValidationContext(contentExplorer.Model, HttpContext.RequestServices, null);

            foreach (var field in contentExplorer.Metadata.Fields)
            {
                var modelValue = field.GetModelValue(contentExplorer.Model);
                var fieldModel = new Models.Contents.ContentModel.Field
                {
                    Type = field.Type,
                    Name = field.Name,
                    Options = field.GetFormOptions(serviceProvider),
                    Title = field.Title,
                    IsRequired = field.IsRequired,
                    Value = await field.GetFormValueAsync(modelValue, serviceProvider),
                    Errors = field.GetErrors(contentExplorer.Model, validationContext)
                };
                content.Fields.Add(fieldModel);

                if (field is IModelField modelField && modelField.HasValue(modelValue))
                {
                    if (modelField.IsListValue)
                    {
                        var i = 0;
                        foreach (var item in (IList)modelValue)
                        {
                            var childContentExplorer = contentExplorer.Navigate($"{modelField.Name}[{i}]");
                            await EnsureContentsAsync(childContentExplorer, output);
                            i++;
                        }
                    }
                    else
                    {
                        var childContentExplorer = contentExplorer.Navigate(modelField.Name);
                        await EnsureContentsAsync(childContentExplorer, output);
                    }
                }
            }
        }

        async Task ValidateContentsAsync(ContentExplorer contentExplorer, ICollection<Models.Contents.ContentValidationResult> output)
        {
            ArgumentNullException.ThrowIfNull(contentExplorer);
            ArgumentNullException.ThrowIfNull(output);

            var validationContext = new ValidationContext(contentExplorer.Model, HttpContext.RequestServices, null);

            var content = new Models.Contents.ContentValidationResult
            {
                Path = contentExplorer.ModelPath,
                Index = contentExplorer.Index,
                TypeName = contentExplorer.Metadata.Name,
                TypeTitle = contentExplorer.Metadata.Title,
                Fields = []
            };

            if (contentExplorer.Metadata.IsValidatable)
                content.Errors = contentExplorer.Metadata.Validate(validationContext, contentExplorer.Model);
            else
                content.Errors = [];

            foreach (var field in contentExplorer.Metadata.Fields)
            {
                var errors = field.GetErrors(contentExplorer.Model, validationContext);
                if (errors.Count == 0)
                    continue;

                var fieldModel = new Models.Contents.FieldValidationResult
                {
                    Name = field.Name,
                    Title = field.Title,
                    Errors = errors
                };
                content.Fields.Add(fieldModel);
            }

            if (content.Errors.Count > 0 || content.Fields.Count > 0)
                output.Add(content);

            foreach (var field in contentExplorer.Metadata.Fields)
            {
                var modelValue = field.GetModelValue(contentExplorer.Model);
                if (field is IModelField modelField && modelField.HasValue(modelValue))
                {
                    if (modelField.IsListValue)
                    {
                        var i = 0;
                        foreach (var item in (IList)modelValue)
                        {
                            var childContentExplorer = contentExplorer.Navigate($"{modelField.Name}[{i}]");
                            await ValidateContentsAsync(childContentExplorer, output);
                            i++;
                        }
                    }
                    else
                    {
                        var childContentExplorer = contentExplorer.Navigate(modelField.Name);
                        await ValidateContentsAsync(childContentExplorer, output);
                    }
                }
            }
        }

        #endregion
    }
}