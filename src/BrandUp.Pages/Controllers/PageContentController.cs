using BrandUp.Pages.Content;
using BrandUp.Pages.Identity;
using BrandUp.Pages.Models;
using BrandUp.Website;
using Microsoft.AspNetCore.Mvc;

namespace BrandUp.Pages.Controllers
{
    [Route("brandup.pages/page/content"), Filters.Administration]
    public class PageContentController(ContentEditService contentEditService, IWebsiteContext websiteContext) : Controller
    {
        [HttpPost("begin")]
        public async Task<IActionResult> BeginEditAsync([FromQuery] string key, [FromQuery] string type, [FromQuery] bool force, [FromServices] ContentMetadataManager contentMetadataManager, [FromServices] IAccessProvider accessProvider)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(type))
                return BadRequest();

            if (!contentMetadataManager.TryGetMetadata(type, out var contentMetadata))
                return BadRequest();

            var websiteId = websiteContext.Website.Id;

            var result = new Models.BeginPageEditResult();

            var userId = await accessProvider.GetUserIdAsync(HttpContext.RequestAborted);

            var currentEdit = await contentEditService.FindEditByUserAsync(websiteId, key, userId, HttpContext.RequestAborted);
            if (currentEdit != null)
            {
                if (force)
                {
                    await contentEditService.DiscardEditAsync(currentEdit, HttpContext.RequestAborted);
                    currentEdit = null;
                }
                else
                    result.CurrentDate = currentEdit.CreatedDate;
            }

            if (currentEdit == null)
            {
                currentEdit = await contentEditService.BeginEditAsync(websiteId, key, userId, contentMetadata, HttpContext.RequestAborted);
                if (currentEdit == null)
                    return NotFound();
            }

            result.EditId = currentEdit.Id;

            var contentModel = await contentEditService.GetContentAsync(currentEdit, HttpContext.RequestAborted);

            var contentExplorer = ContentExplorer.Create(contentMetadataManager, contentModel);

            result.Content = [
                    new ContentModel {
                        Path = string.Empty,
                        Index = -1,
                        TypeName = "PageNav",
                        TypeTitle = "PageNav",
                        Fields = [
                                new ContentModel.TextField { Name = "LogoText", Title = "Logo", Type = "text", Value = "Logo text" },
                                new ContentModel.ModelField { Name = "Items", Title = "Menu", Type = "model", IsList = true, Value = new List<string> { "Items[0]", "Items[1]" } }
                            ]
                    },
                    new ContentModel {
                        Path = "Items[0]",
                        Index = -1,
                        TypeName = "PageNav.MenuItem",
                        TypeTitle = "PageNav.MenuItem",
                        Fields = []
                    },
                    new ContentModel {
                        Path = "Items[1]",
                        Index = -1,
                        TypeName = "PageNav.MenuItem",
                        TypeTitle = "PageNav.MenuItem",
                        Fields = []
                    }
                ];

            return Ok(result);
        }

        [HttpGet("form")]
        public async Task<IActionResult> GetFormAsync([FromQuery] Guid editId, [FromQuery] string modelPath)
        {
            var editSession = await contentEditService.FindEditByIdAsync(editId, HttpContext.RequestAborted);
            if (editSession == null)
                return BadRequest();

            modelPath ??= string.Empty;

            var pageContent = await contentEditService.GetContentAsync(editSession);
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

            var editSession = await contentEditService.FindEditByIdAsync(editId);
            if (editSession == null)
                return BadRequest();

            modelPath ??= string.Empty;

            var newModelType = contentMetadataManager.GetMetadata(modelType);

            var pageContent = await contentEditService.GetContentAsync(editSession);
            var pageContentExplorer = ContentExplorer.Create(contentMetadataManager, pageContent);

            var contentExplorer = pageContentExplorer.Navigate(modelPath);
            if (contentExplorer == null)
                return BadRequest();

            contentExplorer.Field.ChangeType(contentExplorer.Model, modelType);

            return Ok();
        }

        [HttpPost("commit")]
        public async Task<IActionResult> CommitEditAsync([FromQuery] Guid editId)
        {
            var editSession = await contentEditService.FindEditByIdAsync(editId);
            if (editSession == null)
                return BadRequest();

            await contentEditService.CommitEditAsync(editSession);

            return Ok();
        }

        [HttpPost("discard")]
        public async Task<IActionResult> DiscardEditAsync([FromQuery] Guid editId)
        {
            var editSession = await contentEditService.FindEditByIdAsync(editId);
            if (editSession == null)
                return BadRequest();

            await contentEditService.DiscardEditAsync(editSession);

            return Ok();
        }
    }
}