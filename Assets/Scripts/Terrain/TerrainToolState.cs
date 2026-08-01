public static class TerrainToolState
{
    public enum ActiveTool
    {
        Sculpt,
        Paint,
        Place
    }

    public static ActiveTool currentTool = ActiveTool.Sculpt;
}
