using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Hushi
{
    public class PurchaseProduct
    {
        public PurchaseProduct()
        {
            Location = new StorageLocation();
            Restaurant = new Restaurant();
            OrderList = new List<ProductOrder>();
        }
        public int ProductId { get; set; }
        public int OrderPartId { get; set; }
        public string Description { get; set; }
        public int ProductQty { get; set; }
        public string SubLocationName { get; set; }
        public Guid OrderGUID { get; set; }
        public string TableName { get; set; }
        public StorageLocation Location { get; set; }
        public int PickedBy { get; set; }
        public int ReceivedQty { get; set; }
        public Restaurant Restaurant { get; set; }
        public List<ProductOrder> OrderList { get; set; }
    }
}