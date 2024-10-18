using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class TableOrder
    {
        public Guid OrderGUID { get; set; }
        public Customer tableCustomer { get; set; }
        public List<Product> tableProducts { get; set; }
        public User tableUser { get; set; }
        public Table tableDetails { get; set; }

        public string Type { get; set; }

        public int LastOrderNo { get; set; }
        //public int LastOrderTime { get; set; }
        public bool ReservedCustomer { get; set; }
       public List<Table> joinTables { get; set; }
       
        public List<OrderBuffetItem> BuffetItems { get; set; }

        public List<ProductOrderNo> pOrderNo { get; set; }
        public int CustCount { get; set; }
        public string UserType { get; set; }
        public long MobileNumber { get; set; }
        public string UserName { get; set; }
        public int CustomerId { get; set; }
        public bool ServiceChargeApplicable { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public bool PayAsYouGo { get; set; }
        public decimal SCRate { get; set; }
        public string RestaurantName { get; set; }
        public TableOrder()
        {
            this.tableCustomer = new Customer();
            this.tableProducts = new List<Product>();
            this.tableUser = new User();
            this.tableDetails = new Table();
            this.pOrderNo = new List<ProductOrderNo>();
            this.joinTables = new List<Table>();
            this.BuffetItems = new List<OrderBuffetItem>();

        }
    }
}