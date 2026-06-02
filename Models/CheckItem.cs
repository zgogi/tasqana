using System.Text.Json.Serialization;
using Tasqana.Models;

namespace Tasqana.Models
{
    public class CheckItem : AbstractModel<CheckItem>, IOrderable
    {
        public long TodoId { get; set; }
        public string Title { get; set; } = null!;
        public bool IsCompleted { get; set; }
        public int Order { get; set; }
        public Todo Todo { get; set; } = null!;
    }
}
