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
    
    public partial class tblZCardSale
    {
        public int ZID { get; set; }
        public string PCName { get; set; }
        public System.DateTime ZDate { get; set; }
        public int UserID { get; set; }
        public decimal CardSale { get; set; }
        public decimal CardTip { get; set; }
        public decimal AmexCardSale { get; set; }
        public decimal AmexCardTip { get; set; }
        public decimal TillCardSale { get; set; }
        public decimal Difference { get; set; }
        public bool Matched { get; set; }
    }
}
