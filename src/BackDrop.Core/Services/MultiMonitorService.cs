using Windows.Graphics;
using Windows.Graphics.Display;

namespace BackDrop.Core.Services;

/// <summary>Physical-pixel bounds of one display.</summary>
public sealed record DisplayAreaInfo(string Id, RectInt32 Bounds, RectInt32 WorkArea, bool IsPrimary);

/// <summary>Enumerates physical displays via WinRT DisplayArea (no WinForms dependency).</summary>
public sealed class MultiMonitorService
{
    public IReadOnlyList<DisplayAreaInfo> GetDisplays()
    {
        var areas = DisplayArea.FindAll();
        var result = new List<DisplayAreaInfo>(areas.Count);
        foreach (var area in areas)
        {
            result.Add(new DisplayAreaInfo(
                area.DisplayId.Value.ToString(),
                area.Bounds,
                area.WorkArea,
                area.IsPrimary));
        }
        return result;
    }
}
