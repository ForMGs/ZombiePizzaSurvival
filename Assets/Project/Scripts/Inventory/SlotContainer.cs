//드래그 중 무엇을 옮기는지 공통으로 전달
public enum SlotContainerType
{
    Inventory,
    QuickSlot
}

public struct DraggedSlotData
{
    public SlotContainerType containerType;
    public int slotIndex;
    public ItemData itemData;
    public int amount;
}