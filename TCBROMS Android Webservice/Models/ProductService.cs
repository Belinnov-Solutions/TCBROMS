using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Entity;
using System.Transactions;
using Deznu.Products.Common.Utility;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.Entity;
using TCBROMS_Android_Webservice.Helpers;
using System.Configuration;
using Entity.Models;

namespace TCBROMS_Android_Webservice.Models
{
    public class ProductService
    { //added Roms Helper
        ROMSHelper ro = new ROMSHelper();
        public GetProductsResponse GetDineInProducts()
        {
            ChineseTillEntities1 _context = new ChineseTillEntities1();
            GetProductsResponse gpr = new GetProductsResponse();
            int currTime = Convert.ToInt32(DateTime.Now.ToString("HHMM"));
            int currDay = (int)DateTime.Today.DayOfWeek;
            if (currDay == 0)
                currDay = 7;
            //List<ProductGroupModel> FoodMenu = ro.GetBuffetMenu();
            gpr.BuffetMenu = ro.GetBuffetMenu();
            foreach (var item in gpr.BuffetMenu)
            {
                item.GroupProducts = item.GroupProducts.Where(x => x.bOnsite == true && x.ProductAvailable == true).ToList();
            }

            gpr.DrinksMenu = (from a in _context.tblProductGroups
                              where a.DelInd == false && a.bOnsite == true
                              select new ProductGroupModel
                              {
                                  Group = new ProductGroup
                                  {
                                      Groupname = a.Groupname,
                                      ProductGroupID = a.ProductGroupID,
                                      SortOrder = a.SortOrder
                                  },
                                  GroupProducts = (from p in _context.tblProducts
                                                   where p.DelInd == false && p.FoodRefil == false && p.ProductGroupID == a.ProductGroupID && p.Available == true && p.bOnsite == true
                                                   select new Entity.Product
                                                   {
                                                       ProductID = p.ProductID,
                                                       Description = p.Description,
                                                       ProductGroupID = p.ProductGroupID,
                                                       ImageName = p.ImageName != null ? p.ImageName : "",
                                                       Price = (float)Math.Round((double)p.Price, 2),
                                                       FoodRefil = false,
                                                       HasLinkedProducts = (_context.tblProductLinkers.Any(c => c.PrimaryProductID == p.ProductID)),
                                                       productsLinker = (from pl in _context.tblProductLinkers
                                                                         join pr in _context.tblProducts on pl.SecondaryProductID equals pr.ProductID
                                                                         where pl.PrimaryProductID == p.ProductID
                                                                         select new LinkedProduct
                                                                         {
                                                                             ProductID = pl.SecondaryProductID,
                                                                             Description = pr.Description,
                                                                             Price = (float)pr.Price

                                                                         }).ToList()
                                                   }).OrderBy(x => x.Description).ToList()


                              }).OrderBy(a => a.Group.SortOrder).ToList();

            gpr.DrinksMenu.RemoveAll(ro.noProducts);

            return gpr;
        }

        public tblProduct GetProductDetail(int id)
        {
            tblProduct pd = new tblProduct();
            ChineseTillEntities1 _context = new ChineseTillEntities1();
            pd = _context.tblProducts.Where(x => x.ProductID == id).FirstOrDefault();
            return pd;
        }


        #region V1 APIs
        public GetProductsResponse GetDineInProducts(Guid orderId)
        {
            ChineseTillEntities1 _context = new ChineseTillEntities1();
            GetProductsResponse gpr = new GetProductsResponse();
            int currTime = Convert.ToInt32(DateTime.Now.ToString("HHMM"));
            int currDay = (int)DateTime.Today.DayOfWeek;
            if (currDay == 0)
                currDay = 7;
            //List<ProductGroupModel> FoodMenu = ro.GetBuffetMenu();
            gpr.BuffetMenu = ro.GetBuffetMenu();
            foreach (var item in gpr.BuffetMenu)
            {
                item.GroupProducts = item.GroupProducts.Where(x => x.bOnsite == true && x.ProductAvailable == true).ToList();
            }

            gpr.DrinksMenu = (from a in _context.tblProductGroups
                              where a.DelInd == false && a.bOnsite == true
                              select new ProductGroupModel
                              {
                                  Group = new ProductGroup
                                  {
                                      Groupname = a.Groupname,
                                      ProductGroupID = a.ProductGroupID,
                                      SortOrder = a.SortOrder
                                  },
                                  GroupProducts = (from p in _context.tblProducts
                                                   where p.DelInd == false && p.FoodRefil == false && p.ProductGroupID == a.ProductGroupID && p.Available == true && p.bOnsite == true
                                                   select new Entity.Product
                                                   {
                                                       ProductID = p.ProductID,
                                                       Description = p.Description,
                                                       ProductGroupID = p.ProductGroupID,
                                                       ImageName = p.ImageName != null ? p.ImageName : "",
                                                       Price = (float)Math.Round((double)p.Price, 2),
                                                       FoodRefil = false,
                                                       ProductTypeId = p.ProductTypeID,
                                                       HasLinkedProducts = (_context.tblProductLinkers.Any(c => c.PrimaryProductID == p.ProductID)),
                                                       productsLinker = (from pl in _context.tblProductLinkers
                                                                         join pr in _context.tblProducts on pl.SecondaryProductID equals pr.ProductID
                                                                         where pl.PrimaryProductID == p.ProductID
                                                                         select new LinkedProduct
                                                                         {
                                                                             ProductID = pl.SecondaryProductID,
                                                                             Description = pr.Description,
                                                                             Price = (float)pr.Price

                                                                         }).ToList()
                                                   }).OrderBy(x => x.Description).ToList()


                              }).OrderBy(a => a.Group.SortOrder).ToList();


            if (_context.tblOrders.Any(x => x.OrderGUID == orderId && x.ServiceRequired == true))
            {
                foreach (var item in gpr.DrinksMenu)
                {
                    item.GroupProducts.RemoveAll(ro.alcoholProducts);
                }
            }
            gpr.DrinksMenu.RemoveAll(ro.noProducts);
            return gpr;
        }
        #endregion
        #region V2 APIs
        public GetProductsResponse GetDineInProductsV2(Guid orderId)
        {
            ChineseTillEntities1 _context = new ChineseTillEntities1();
            GetProductsResponse gpr = new GetProductsResponse();
            int currTime = Convert.ToInt32(DateTime.Now.ToString("HHMM"));
            int currDay = (int)DateTime.Today.DayOfWeek;
            if (currDay == 0)
                currDay = 7;
            //List<ProductGroupModel> FoodMenu = ro.GetBuffetMenu();



            gpr.BuffetMenu = ro.GetBuffetMenu();
            foreach (var item in gpr.BuffetMenu)
            {
                item.GroupProducts = item.GroupProducts.Where(x => x.ProductAvailable == true).ToList();
            }

            gpr.DrinksMenu = (from a in _context.tblProductGroups
                              where a.DelInd == false && a.bOnsite == true
                              select new ProductGroupModel
                              {
                                  Group = new ProductGroup
                                  {
                                      Groupname = a.Groupname,
                                      ProductGroupID = a.ProductGroupID,
                                      SortOrder = a.SortOrder,
                                      ParentGroupID = a.ParentGroupID,
                                      GroupImage = a.ImageName == null ? "" : a.ImageName,
                                  },
                                  GroupProducts = (from p in _context.tblProducts
                                                   where p.DelInd == false && p.FoodRefil == false && p.ProductGroupID == a.ProductGroupID && p.Available == true && p.bOnsite == true
                                                   select new Entity.Product
                                                   {
                                                       ProductID = p.ProductID,
                                                       Description = p.Description,
                                                       ProductGroupID = p.ProductGroupID,
                                                       ImageName = p.ImageName != null ? p.ImageName : "",
                                                       Price = (float)Math.Round((double)p.Price, 2),
                                                       FoodRefil = false,
                                                       ProductTypeId = p.ProductTypeID,
                                                       Type = "Product",
                                                       //RewardPoints = (int)(p.RewardPoints == null ? 0 : p.RewardPoints),
                                                       //RedemptionPoints = (int)(p.RedemptionPoints == null ? 0 : p.RedemptionPoints),
                                                       RewardPoints = (int)(p.RewardPoints == null ? 0 : (p.RewardStartDate !=null && p.RewardStartDate <= DateTime.Now && p.RewardEndDate != null && p.RewardEndDate >= DateTime.Now ? p.RewardPoints : 0)),
                                                       RedemptionPoints = (int)(p.RedemptionPoints == null ? 0 : (((p.RedeemStartDate != null && p.RedeemStartDate <= DateTime.Now && p.RedeemEndDate != null && p.RedeemEndDate >= DateTime.Now) || p.RedeemValidDays > 0) ? p.RedemptionPoints : 0)),

                                                       HasLinkedProducts = (_context.tblProductLinkers.Any(c => c.PrimaryProductID == p.ProductID)),
                                                       productsLinker = (from pl in _context.tblProductLinkers
                                                                         join pr in _context.tblProducts on pl.SecondaryProductID equals pr.ProductID
                                                                         where pl.PrimaryProductID == p.ProductID
                                                                         select new LinkedProduct
                                                                         {
                                                                             ProductID = pl.SecondaryProductID,
                                                                             Description = pr.Description,
                                                                             Price = (float)pr.Price

                                                                         }).ToList()
                                                   }).OrderBy(x => x.Description).ToList(),

                              }).OrderBy(a => a.Group.SortOrder).ToList();

            foreach (var item in gpr.DrinksMenu)
            {
                if (_context.tblProductGroups.Any(x => x.ParentGroupID == item.Group.ProductGroupID))
                {
                    var childGroups = (from a in _context.tblProductGroups
                                       where a.ParentGroupID == item.Group.ProductGroupID && a.DelInd == false
                                       && (_context.tblProducts.Any(x => x.ProductGroupID == a.ProductGroupID && x.DelInd == false))
                                       select new Entity.Product
                                       {
                                           ProductID = a.ProductGroupID,
                                           Description = a.Groupname,
                                           ProductGroupID = a.ProductGroupID,
                                           ParentGroupID = item.Group.ProductGroupID,
                                           ImageName = a.ImageName == null ? "" : a.ImageName,
                                           Price = 0,
                                           FoodRefil = false,
                                           ProductTypeId = 0,
                                           Type = "Group",

                                       }).ToList();
                    List<Product> singleProducts = item.GroupProducts;
                    item.GroupProducts = new List<Product>();
                    //foreach (var cg in childGroups)
                    //{
                    //    if (_context.tblProducts.Any(x => x.ProductGroupID == cg.ProductID && x.DelInd == false))
                    //        item.GroupProducts.Add(cg);
                    //}
                    item.GroupProducts = childGroups;
                    item.GroupProducts.AddRange(singleProducts);
                    //item.GroupProducts.AddRange(childGroups);
                }
            }
            var order = _context.tblOrders.Where(x => x.OrderGUID == orderId).FirstOrDefault();
            if(order != null)
            {
                if (order.ServiceRequired == true)
                {
                    foreach (var item in gpr.DrinksMenu)
                    {
                        item.GroupProducts.RemoveAll(ro.alcoholProducts);
                    }
                }
                gpr.DrinksMenu.RemoveAll(ro.noProducts);
            }
            

            //remove all food products with 0 price if Pay As you Go
            if (order.PayAsYouGo)
            {
                var buffetMenu = gpr.BuffetMenu;
                foreach (var item in buffetMenu)
                {
                    item.GroupProducts.RemoveAll(x => x.Price == 0);
                    item.GroupProducts.ForEach(x => x.RedemptionPoints = 0);
                }
                buffetMenu.RemoveAll(ro.noProducts);
                gpr.BuffetMenu = buffetMenu;
            }
            return gpr;
        }

        public List<Entity.Product> GetProductsForRedemptionV2()
        {
            List<Entity.Product> prds = new List<Entity.Product>();
            ChineseTillEntities1 _context = new ChineseTillEntities1();
            prds = (from p in _context.tblProducts
                    where p.DelInd == false && p.FoodRefil == false && p.Available == true && p.bOnsite == true
                    && p.RedemptionPoints != null && p.RedemptionPoints > 0
                    select new Entity.Product
                    {
                        ProductID = p.ProductID,
                        Description = p.Description,
                        ProductGroupID = p.ProductGroupID,
                        ImageName = p.ImageName != null ? p.ImageName : "",
                        Price = (float)Math.Round((double)p.Price, 2),
                        FoodRefil = false,
                        ProductTypeId = p.ProductTypeID,
                        Type = "Product",
                        RedemptionPoints = (int)(p.RedemptionPoints == null ? 0 : p.RedemptionPoints),
                        RewardPoints = (int)(p.RewardPoints == null ? 0 : p.RewardPoints),
                    }).OrderBy(x => x.Description).ToList();
            return prds;
        }
        #endregion

        #region V3 APIs
        public GetProductsResponse GetDineInProductsV3(Guid orderId)
        {
            ChineseTillEntities1 _context = new ChineseTillEntities1();
            GetProductsResponse gpr = new GetProductsResponse();
            int currTime = Convert.ToInt32(DateTime.Now.ToString("HHMM"));
            int currDay = (int)DateTime.Today.DayOfWeek;
            if (currDay == 0)
                currDay = 7;
            //List<ProductGroupModel> FoodMenu = ro.GetBuffetMenu();
            try
            {
                gpr.BuffetMenu = ro.GetBuffetMenuV3();
                foreach (var item in gpr.BuffetMenu)
                {
                    item.GroupProducts = item.GroupProducts.Where(x => x.bOnsite == true && x.ProductAvailable == true).ToList();
                }

                gpr.DrinksMenu = (from a in _context.tblProductGroups
                                  where a.DelInd == false && a.bOnsite == true
                                  select new ProductGroupModel
                                  {
                                      Group = new ProductGroup
                                      {
                                          Groupname = a.Groupname,
                                          ProductGroupID = a.ProductGroupID,
                                          SortOrder = a.SortOrder,
                                          ParentGroupID = a.ParentGroupID,
                                          GroupImage = a.ImageName == null ? "" : a.ImageName,
                                      },
                                      GroupProducts = (from p in _context.tblProducts
                                                       where p.DelInd == false && p.FoodRefil == false && p.ProductGroupID == a.ProductGroupID && p.Available == true && p.bOnsite == true
                                                       select new Entity.Product
                                                       {
                                                           ProductID = p.ProductID,
                                                           Description = p.Description,
                                                           ProductGroupID = p.ProductGroupID,
                                                           ImageName = p.ImageName != null ? p.ImageName : "",
                                                           Price = (float)Math.Round((double)p.Price, 2),
                                                           FoodRefil = false,
                                                           ProductTypeId = p.ProductTypeID,
                                                           Type = "Product",
                                                           RewardPoints = (int)(p.RewardPoints == null ? 0 : p.RewardPoints),
                                                           RedemptionPoints = (int)(p.RedemptionPoints == null ? 0 : p.RedemptionPoints),

                                                           HasLinkedProducts = (_context.tblProductLinkers.Any(c => c.PrimaryProductID == p.ProductID)),
                                                           productsLinker = (from pl in _context.tblProductLinkers
                                                                             join pr in _context.tblProducts on pl.SecondaryProductID equals pr.ProductID
                                                                             where pl.PrimaryProductID == p.ProductID
                                                                             select new LinkedProduct
                                                                             {
                                                                                 ProductID = pl.SecondaryProductID,
                                                                                 Description = pr.Description,
                                                                                 Price = (float)pr.Price

                                                                             }).ToList()
                                                       }).OrderBy(x => x.Description).ToList(),

                                  }).OrderBy(a => a.Group.SortOrder).ToList();

                foreach (var item in gpr.DrinksMenu)
                {
                    if (_context.tblProductGroups.Any(x => x.ParentGroupID == item.Group.ProductGroupID))
                    {
                        var childGroups = (from a in _context.tblProductGroups
                                           where a.ParentGroupID == item.Group.ProductGroupID && a.DelInd == false
                                           select new Entity.Product
                                           {
                                               ProductID = a.ProductGroupID,
                                               Description = a.Groupname,
                                               ProductGroupID = a.ProductGroupID,
                                               ParentGroupID = item.Group.ProductGroupID,
                                               ImageName = a.ImageName == null ? "" : a.ImageName,
                                               Price = 0,
                                               FoodRefil = false,
                                               ProductTypeId = 0,
                                               Type = "Group",
                                           }).ToList();
                        item.GroupProducts.AddRange(childGroups);
                    }
                }
                if (_context.tblOrders.Any(x => x.OrderGUID == orderId && x.ServiceRequired == true))
                {
                    foreach (var item in gpr.DrinksMenu)
                    {
                        item.GroupProducts.RemoveAll(ro.alcoholProducts);
                    }
                }
                gpr.DrinksMenu.RemoveAll(ro.noProducts);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return gpr;
        }
        #endregion

    }
}