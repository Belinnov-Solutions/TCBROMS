//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TCBROMS_Android_Webservice.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblUnclaimedProduct
    {
        public int UnclaimedProductId { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public System.Guid OrderGUID { get; set; }
        public System.DateTime DateCreated { get; set; }
        public int Qty { get; set; }
        public int TableId { get; set; }
        public Nullable<decimal> Price { get; set; }
        public bool WebUpload { get; set; }
    }
}
