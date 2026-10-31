using System.Globalization;
using System.Text;
using Tasqana.Models;
using Tasqana.Models.http;

namespace Tasqana.Services;

public record CategoryMenuAction(string Command, string Context, bool Select, long? CategoryId);
public record CategoryMenuPage(string Text, Telegram.ReplyMarkup Markup);

public class TelegramCategoryMenu
{
    public CategoryMenuPage Build(IReadOnlyCollection<Category> categories, long? parentId,
        string command, string context, string heading)
    {
        var parent = parentId.HasValue ? categories.SingleOrDefault(c => c.Id == parentId)
            ?? throw new KeyNotFoundException("Category not found") : null;
        var children = categories.Where(c => c.ParentId == parentId).OrderBy(c => c.Order).ThenBy(c => c.Id);
        var markup = new Telegram.ReplyMarkup();
        foreach (var category in children)
        {
            var row = new List<Telegram.InlineKeyboardButton>
            {
                Telegram.InlineKeyboardButton.Callback(category.Title, Encode(command, context, true, category.Id))
            };
            if (categories.Any(c => c.ParentId == category.Id))
                row.Add(Telegram.InlineKeyboardButton.Callback("Открыть", Encode(command, context, false, category.Id)));
            markup.inline_keyboard.Add(row);
        }
        if (parent != null)
            markup.inline_keyboard.Add([Telegram.InlineKeyboardButton.Callback("← Назад",
                Encode(command, context, false, parent.ParentId))]);
        var text = heading + "\n" + (parent == null ? "Корневые категории" : "Категория: " + parent.Title);
        if (!children.Any()) text += "\nКатегорий пока нет.";
        return new(text, markup);
    }

    public string Encode(string command, string context, bool select, long? categoryId)
    {
        if (string.IsNullOrWhiteSpace(command) || command.Contains('|') ||
            string.IsNullOrWhiteSpace(context) || context.Contains('|') || categoryId is <= 0 ||
            (select && !categoryId.HasValue)) throw new ArgumentException("Invalid category callback");
        var data = $"cat|{command}|{context}|{(select ? "s" : "o")}|{categoryId?.ToString(CultureInfo.InvariantCulture) ?? "0"}";
        if (Encoding.UTF8.GetByteCount(data) > 64) throw new ArgumentException("Callback exceeds 64 bytes");
        return data;
    }

    public bool TryParse(string? data, out CategoryMenuAction? action)
    {
        action = null;
        if (data == null || Encoding.UTF8.GetByteCount(data) > 64) return false;
        var parts = data.Split('|');
        if (parts.Length != 5 || parts[0] != "cat" || string.IsNullOrWhiteSpace(parts[1]) ||
            string.IsNullOrWhiteSpace(parts[2]) || (parts[3] != "s" && parts[3] != "o") ||
            !long.TryParse(parts[4], NumberStyles.None, CultureInfo.InvariantCulture, out var id) ||
            (id == 0 && parts[3] == "s")) return false;
        action = new(parts[1], parts[2], parts[3] == "s", id == 0 ? null : id);
        return true;
    }
}
