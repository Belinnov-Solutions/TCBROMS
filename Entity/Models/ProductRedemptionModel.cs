using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class ProductRedemptionModel
    {
		public Guid OrderId
		{
			get;
			set;
		}

		public int CustomerId
		{
			get;
			set;
		}

		public int CustomerPoints
		{
			get;
			set;
		}

		public List<Product> RedemptionProducts
		{
			get;
			set;
		}

		public ProductRedemptionModel()
		{
			RedemptionProducts = new List<Product>();
		}
	}
}