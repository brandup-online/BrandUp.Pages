using System.Collections;
using System.ComponentModel.DataAnnotations;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Views;
using Microsoft.AspNetCore.Mvc;

namespace BrandUp.Pages.Controllers
{
    public class ModelController : FieldController<IModelField>
    {
        [HttpGet("types")]
        public IActionResult GetContentTypes()
        {
            var result = new List<Models.ContentTypeModel>();

            var contentTypes = Field.ContentMetadata.GetDerivedMetadataWithHierarhy(true);
            foreach (var contentType in contentTypes)
            {
                if (!contentType.IsAbstract)
                    continue;

                result.Add(new Models.ContentTypeModel
                {
                    Name = contentType.Name,
                    Title = contentType.Title
                });
            }

            return Ok(result);
        }

        [HttpGet("data")]
        public IActionResult GetData([FromQuery] int itemIndex = -1)
        {
            var contentPath = Field.Name;
            if (Field.IsListValue)
            {
                if (itemIndex < 0)
                    return BadRequest();

                contentPath += $"[{itemIndex}]";
            }

            var contentContext = ContentContext.Navigate(contentPath);
            if (contentContext == null)
                return NotFound();

            var contentExplorer = contentContext.Explorer;

            var result = new ContentItem
            {
                Title = contentExplorer.Title,
                Type = new ContentItemType
                {
                    Name = contentExplorer.Metadata.Name,
                    Title = contentExplorer.Metadata.Title
                }
            };

            return Ok(result);
        }

        [HttpGet("view")]
        public async Task<IActionResult> ViewAsync([FromServices] IViewRenderService viewRenderService, [FromQuery] int itemIndex = -1)
        {
            var contentPath = Field.Name;
            if (Field.IsListValue)
            {
                if (itemIndex < 0)
                    return BadRequest();

                contentPath += $"[{itemIndex}]";
            }

            var contentContext = ContentContext.Navigate(contentPath);
            if (contentContext == null)
                return NotFound();

            var html = await viewRenderService.RenderToStringAsync(contentContext);

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = 200,
                Content = html
            };
        }

        [HttpPut]
        public async Task<IActionResult> AddAsync([FromServices] ContentService contentService, [FromQuery] string itemType, [FromQuery] int itemIndex = -1)
        {
            if (itemType == null)
                return BadRequest();

            if (!Field.ValueContentMetadata.Manager.TryGetMetadata(itemType, out ContentMetadata contentMetadataProvider))
                return BadRequest();

            if (!contentMetadataProvider.IsInheritedOrEqual(Field.ValueContentMetadata))
                return BadRequest();

            var newItem = await contentService.CreateDefaultAsync(contentMetadataProvider, HttpContext.RequestAborted);
            newItem ??= contentMetadataProvider.CreateModelInstance();

            ContentExplorer newItemExplorer;
            if (Field.IsListValue)
            {
                if (Field.GetModelValue(ContentContext.Content) is not IList list)
                    list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(Field.ValueContentMetadata.ModelType));

                if (itemIndex == -1)
                    itemIndex = list.Count;

                list.Insert(itemIndex, newItem);

                Field.SetModelValue(ContentContext.Content, list);

                newItemExplorer = ContentContext.Explorer.Navigate($"{Field.Name}[{itemIndex}]");
            }
            else
            {
                if (Field.HasValue(ContentContext.Content))
                    throw new InvalidOperationException();

                Field.SetModelValue(ContentContext.Content, newItem);

                newItemExplorer = ContentContext.Explorer.Navigate($"{Field.Name}");
            }

            await SaveChangesAsync();

            var result = new Models.Contents.AddContentResult
            {
                FieldValue = await CreateFormValueAsync(),
                Content = []
            };

            await EnsureContentsAsync(newItemExplorer, result.Content);

            return Ok(result);
        }

        [HttpPost("up")]
        public async Task<IActionResult> UpAsync([FromQuery] int itemIndex)
        {
            if (!Field.IsListValue)
                return BadRequest();
            if (itemIndex <= 0)
                return BadRequest();

            if (Field.GetModelValue(ContentContext.Content) is IList list)
            {
                var item = list[itemIndex];

                list.RemoveAt(itemIndex);
                list.Insert(itemIndex - 1, item);

                Field.SetModelValue(ContentContext.Content, list);

                await SaveChangesAsync();
            }

            return await FormValueResultAsync();
        }

        [HttpPost("down")]
        public async Task<IActionResult> DownAsync([FromQuery] int itemIndex)
        {
            if (!Field.IsListValue)
                return BadRequest();

            if (Field.GetModelValue(ContentContext.Content) is IList list)
            {
                if (itemIndex >= list.Count - 1)
                    return BadRequest();

                var item = list[itemIndex];

                list.RemoveAt(itemIndex);
                list.Insert(itemIndex + 1, item);

                Field.SetModelValue(ContentContext.Content, list);

                await SaveChangesAsync();
            }

            return await FormValueResultAsync();
        }

        [HttpPost("move")]
        public async Task<IActionResult> MoveAsync([FromQuery] int itemIndex, [FromQuery] int newIndex)
        {
            if (itemIndex == newIndex)
                return BadRequest();
            if (!Field.IsListValue)
                return BadRequest();

            if (Field.GetModelValue(ContentContext.Content) is IList list)
            {
                if (itemIndex > list.Count - 1)
                    return BadRequest();
                if (newIndex > list.Count - 1)
                    return BadRequest();

                var item = list[itemIndex];

                list.RemoveAt(itemIndex);
                list.Insert(newIndex, item);

                Field.SetModelValue(ContentContext.Content, list);

                await SaveChangesAsync();
            }

            return await FormValueResultAsync();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromQuery] int itemIndex)
        {
            if (Field.IsListValue)
            {
                if (Field.GetModelValue(ContentContext.Content) is IList list)
                {
                    list.RemoveAt(itemIndex);

                    Field.SetModelValue(ContentContext.Content, list);

                    await SaveChangesAsync();
                }
            }

            return await FormValueResultAsync();
        }

        async Task EnsureContentsAsync(ContentExplorer contentExplorer, ICollection<Models.Contents.ContentModel> output)
        {
            ArgumentNullException.ThrowIfNull(contentExplorer);
            ArgumentNullException.ThrowIfNull(output);

            var serviceProvider = HttpContext.RequestServices;

            var content = new Models.Contents.ContentModel
            {
                ParentPath = contentExplorer.Parent?.ModelPath,
                ParentField = contentExplorer.Field?.Name,
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
    }
}