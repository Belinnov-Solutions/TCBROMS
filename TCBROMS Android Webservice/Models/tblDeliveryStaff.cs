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
    
    public partial class tblDeliveryStaff
    {
        public int Id { get; set; }
        public int DeliveryStaffId { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string ImageName { get; set; }
        public System.DateTime DateCreated { get; set; }
        public bool DelInd { get; set; }
        public Nullable<bool> OutForDelivery { get; set; }
        public Nullable<bool> Available { get; set; }
    }
}
