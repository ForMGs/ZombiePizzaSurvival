//드래그 중인 데이터를 관리하는 클래스
public static class DragContext
{
    public static DraggedSlotData? Current {get; private set;}
    public static void Begin(DraggedSlotData data)
    {
        Current = data;
    }

    public static void End()
    {
        Current = null;
    }
}