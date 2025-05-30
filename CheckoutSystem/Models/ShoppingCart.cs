using System.Collections.Generic;

namespace CheckoutSystem.Models
{
    public class ShoppingCart
    {
        public required string UserName { get; set; }
        public List<string> Items { get; set; } = new List<string>();
    }
}
