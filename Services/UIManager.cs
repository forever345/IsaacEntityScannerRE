using IsaacEntityScannerRE.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static IsaacEntityScannerRE.Services.PickupTracker;

namespace IsaacEntityScannerRE.Services;

public enum ViewMode
{
    All,
    Current
}

public class UIManager
{
    private readonly ItemDatabase _db;
    public Action<string> Output { get; set; }
    private readonly StackPanel _rootPanel;
    private readonly ScrollViewer _scroll;
    private ViewMode _mode = ViewMode.Current;

    public UIManager(ItemDatabase db, StackPanel rootPanel, ScrollViewer scroll)
    {
        _db = db;
        _rootPanel = rootPanel;
        _scroll = scroll;
    }

    public void OnPickupUpdated(PickupUpdate update)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"=== PICKUP UPDATE ({_mode}) ===");

        IEnumerable<PickupKey> source = _mode == ViewMode.Current
            ? update.Recent
            : update.Seen;

        _rootPanel.Children.Clear();

        foreach (var key in source)
        {
            var items = _db.GetItems(key.Id, key.Variant);

            if (items == null || items.Count == 0)
                continue;

            foreach (var item in items)
            {
                var control = new ItemControl();

                control.SetData(
                    FormatItem(item),
                    item.Id,
                    key.Variant
                );

                //control.OnRemoveClicked += OnRemove;
                //control.OnAddClicked += OnAdd;
                //control.OnMoveClicked += OnMove;

                _rootPanel.Children.Add(control);
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _scroll.ScrollToEnd();
                });
            }
        }

        Output?.Invoke(sb.ToString());
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

    public void SetMode(ViewMode mode)
    {
        _mode = mode;
    }
}
