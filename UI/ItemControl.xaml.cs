using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace IsaacEntityScannerRE.UI;

public partial class ItemControl : UserControl
{
    // 🔥 dane kontekstowe (co ten card reprezentuje)
    public int Variant { get; private set; }
    public int Id { get; private set; }

    // 🔥 eventy do zewnętrznej logiki (UIManager)
    public event Action<ItemControl>? OnAddClicked;
    public event Action<ItemControl>? OnRemoveClicked;
    public event Action<ItemControl>? OnMoveClicked;

    public ItemControl()
    {
        InitializeComponent();

        AddButton.Click += (_, __) => OnAddClicked?.Invoke(this);
        RemoveButton.Click += (_, __) => OnRemoveClicked?.Invoke(this);
        MoveButton.Click += (_, __) => OnMoveClicked?.Invoke(this);
    }


    private T? FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is T t)
                return t;

            var result = FindChild<T>(child);
            if (result != null)
                return result;
        }

        return null;
    }

    public void SetData(string text, int variant, int id)
    {
        ItemText.Text = text;

        Variant = variant;
        Id = id;
    }
}
