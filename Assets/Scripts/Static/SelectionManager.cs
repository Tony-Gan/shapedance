using System;

public class SelectionManager
{
    private static DraggableItem currentSelection;
    public static event Action<DraggableItem> OnSelectionChanged;

    public static void SetSelected(DraggableItem item)
    {
        if (currentSelection == item) return;
        if (currentSelection != null) currentSelection.Dehighlight();

        currentSelection = item;
        currentSelection.Highlight();

        OnSelectionChanged?.Invoke(currentSelection);
    }

    public static void DeselectAll()
    {
        if (currentSelection != null)
        {
            currentSelection.Dehighlight();
            currentSelection = null;
        }

        OnSelectionChanged?.Invoke(null);
    }
}
