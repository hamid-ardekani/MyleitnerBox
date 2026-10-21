using System.Text.Json;
using System.Text.Json.Nodes;
using MyLeitner.Application.Contract.Services;
using MyLeitner.Domain.Flashcards;

namespace MyLeitner.Application.Flashcards;

public interface IFlashcardContentImageResolver
{
    /// <summary>
    /// Front/Back یک آرایهٔ JSON از بلوک‌هاست؛ بلوک‌های تصویری که هنوز base64 دارند در storage ذخیره
    /// و <c>content</c>شان با URL جایگزین می‌شود. بلوک‌هایی که از قبل URL دارند دست نمی‌خورند.
    /// </summary>
    Task<string> PersistEmbeddedImagesAsync(string blocksJson, CancellationToken cancellationToken);
}

public class FlashcardContentImageResolver(IImageStorageService imageStorage) : IFlashcardContentImageResolver
{
    public async Task<string> PersistEmbeddedImagesAsync(string blocksJson, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(blocksJson))
            return blocksJson;

        JsonNode? root;
        try
        {
            root = JsonNode.Parse(blocksJson);
        }
        catch (JsonException)
        {
            // JSON ویرایشگر بلوکی نیست؛ چیزی برای تبدیل نیست.
            return blocksJson;
        }

        if (root is not JsonArray blocks)
            return blocksJson;

        foreach (var blockNode in blocks)
        {
            if (blockNode is not JsonObject block)
                continue;

            var isImage = block.TryGetPropertyValue("blockType", out var blockTypeNode)
                          && blockTypeNode != null
                          && blockTypeNode.GetValue<int>() == (int)FlashcardBlockType.Image;

            if (!isImage)
                continue;

            if (!block.TryGetPropertyValue("content", out var contentNode) || contentNode == null)
                continue;

            var content = contentNode.GetValue<string>();
            if (string.IsNullOrEmpty(content) || !content.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                continue; // already a stored URL - leave as-is

            var url = await imageStorage.SaveImageAsync(content, cancellationToken);
            block["content"] = url;
        }

        return blocks.ToJsonString();
    }
}
