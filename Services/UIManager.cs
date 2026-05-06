using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsaacEntityScannerRE.Services;

public class UIManager
{
    private readonly Action<string> _output;
    private readonly ItemDatabase _db;

    public UIManager(Action<string> output, ItemDatabase db)
    {
        _output = output;
        _db = db;
    }

    public void OnPickupUpdated(PickupUpdate update)
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== PICKUP UPDATE ===");

        foreach (var key in update.Added)
        {
            var items = _db.GetItems(key.Id, key.Variant);

            if (items == null || items.Count == 0)
                continue;

            foreach (var item in items)
            {
                sb.AppendLine(FormatItem(item));
            }
        }

        _output(sb.ToString());
    }

    private string FormatItem(Item item)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"ID: {item.Id}");
        sb.AppendLine($"Name: {item.Name}");

        if (!string.IsNullOrWhiteSpace(item.Pickup))
            sb.AppendLine($"Pickup: {item.Pickup}");

        sb.AppendLine($"Quality: {item.Quality}");
        sb.AppendLine();

        if (item.Description != null && item.Description.Count > 0)
        {
            sb.AppendLine("Description:");
            foreach (var line in item.Description)
                sb.AppendLine($"- {line}");
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(item.Type))
            sb.AppendLine($"Type: {item.Type}");

        if (!string.IsNullOrWhiteSpace(item.Pools))
            sb.AppendLine($"Pools: {item.Pools}");

        if (!string.IsNullOrWhiteSpace(item.Tags))
            sb.AppendLine($"Tags: {item.Tags}");

        sb.AppendLine("================================");

        return sb.ToString();
    }
}
