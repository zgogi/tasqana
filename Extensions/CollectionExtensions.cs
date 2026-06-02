using WebApi.Models;

namespace WebApi.Extensions
{
    public static class CollectionExtensions
    {
        public static void UpdateOrder<T>(this ICollection<T> items) where T : IOrderable
        {
            if (items == null) return;

            int index = 0;
            foreach (var item in items)
            {
                item.Order = index++;
            }
        }

        public static void MoveBefore<T>(this List<T> items, long moveId, long? beforeId) where T : IOrderable
        {
            if (items == null || items.Count <= 1) return;

             var itemToMove = items.FirstOrDefault(x => x.Id == moveId);
            if (itemToMove == null) return; 
            items.Remove(itemToMove);
            int targetIndex = items.Count; 

            if (beforeId.HasValue)
            {
                int foundIndex = items.FindIndex(x => x.Id == beforeId);
                if (foundIndex != -1)
                {
                    targetIndex = foundIndex;
                }
            }

            items.Insert(targetIndex, itemToMove);
            items.UpdateOrder();
        }

    }
}
