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
    
    public partial class tblSCAmountUpdate
    {
        public int Id { get; set; }
        public System.Guid OrderId { get; set; }
        public int UserId { get; set; }
        public decimal OldAmount { get; set; }
        public decimal NewAmount { get; set; }
        public System.DateTime DateCreated { get; set; }
    }
}
