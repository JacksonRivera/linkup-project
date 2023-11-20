using System;
using System.Collections.Generic;
using System.Text;

namespace LinkUp.Shopify.Entities
{
    public class InventoryWrapper
    {
        public string title { get; set; }
        public string body_html { get; set; }
        public string vendor { get; set; }
        public string product_type { get; set; }
        public string tags { get; set; }
        public List<Variant> variants { get; set; }

        public class Variant
        {
            public string title { get; set; }
            public string sku { get; set; }
            public string barcode { get; set; }
            public string weight_unit { get; set; }
            public bool taxable { get; set; }
            public bool requires_shipping { get; set; }
            public int inventory_quantity { get; set; }
            public double? weight { get; set; }
            public double? compare_at_price { get; set; }
            public double? price { get; set; }
        }
    }
}
