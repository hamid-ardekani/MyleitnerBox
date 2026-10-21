using MyLeitner.Common;

namespace MyLeitner.Application.Services;
 

public static class ChangeOrderManager
{
    public static void ChangeOrderInPlace(List<OrderItem> items, long itemIdToMove, short newPosition)
    {
        if (!items.Any())
            return;

        // بررسی جایگاه جدید
        if (newPosition < 1 || newPosition > items.Count)
            throw new ArgumentException($"New position must be between 1 and {items.Count}");

        // پیدا کردن آیتم مورد نظر
        var itemToMove = items.FirstOrDefault(b => b.Id == itemIdToMove);
        if (itemToMove == null)
            throw new ArgumentException($"Box with ID {itemIdToMove} not found");

        var currentPosition = itemToMove.Order;

        if (currentPosition == newPosition)
            return;

        // حذف آیتم از لیست
        var itemList = items.OrderBy(b => b.Order).ToList();
        items.Clear();

        // مرتب‌سازی دوباره
        if (currentPosition < newPosition)
        {
            var order = 1;
            foreach (var orderItem in itemList)
            {
                if (orderItem.Id == itemToMove.Id)
                {
                    if (order == newPosition)
                    {   
                        itemToMove.UpdateOrder((short)order);
                        AddToBoxes(items, itemToMove); 
                    }
                }
                else 
                {
                    orderItem.UpdateOrder((short)order);
                    AddToBoxes(items, orderItem); 
                    order++;
                }
                
                if (order == newPosition)
                {
                    itemToMove.UpdateOrder((short)order);
                    AddToBoxes(items, itemToMove);
                    order++;
                }
            }
        }
        else
        {
            // جابه‌جایی به عقب
            var order = 1;
            foreach (var orderItem in itemList)
            {
                if (order == newPosition)
                {
                    itemToMove.UpdateOrder((short)order);
                    AddToBoxes(items, itemToMove);
                    order++;
                }
                
                if(orderItem.Id == itemToMove.Id)
                    continue;

                orderItem.UpdateOrder((short)order);
                AddToBoxes(items, orderItem); 
                order++;
            }
        }
    }
    
    private static void AddToBoxes(List<OrderItem> itemes, OrderItem orderItem)
    {
        if (itemes.All(c => c.Id != orderItem.Id))
            itemes.Add(orderItem);
    }
}