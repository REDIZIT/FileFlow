using System.Collections.Generic;

namespace FileFlow;

public enum HistoryPointerType
{
    /// <summary>
    /// { 0, 1, 2, [3], 4 }, Undo -> 2, Redo -> 4
    /// </summary>
    TargetFrame,
    /// <summary>
    /// { 0, 1, 2, [3], 4 }, Undo -> 3, Redo -> 3
    /// </summary>
    CurrentFrame
}
public class History<T>
{
    private List<T> list = new List<T>();
    private int index = -1;
    private HistoryPointerType pointer;

    public History(HistoryPointerType pointer)
    {
        this.pointer = pointer;
    }

    public void Add(T item)
    {
        int min = pointer == HistoryPointerType.TargetFrame ? 1 : 0;
        if (list.Count > min)
        {
            list.RemoveRange(index + 1, list.Count - index - 1);
        }

        list.Add(item);
        index++;
    }

    public bool TryUndo(out T item)
    {
        int min = pointer == HistoryPointerType.TargetFrame ? 1 : 0;
        if (index >= min)
        {
            if (pointer == HistoryPointerType.TargetFrame)
            {
                index--;
                item = list[index];
            }
            else
            {
                item = list[index];
                index--;
            }

            return true;
        }
        else
        {
            item = default;
            return false;
        }
    }
    public bool TryRedo(out T item)
    {
        if (index < list.Count - 1)
        {
            index++;
            item = list[index];
            return true;
        }
        item = default;
        return false;
    }
}