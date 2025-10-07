using UnityEngine;

public static class InputBlocker
{
    public static bool IsCameraDragging { get; set; }
    public static bool IsModelDragging { get; set; }

    public static bool AnyActive => IsCameraDragging || IsModelDragging;
}
