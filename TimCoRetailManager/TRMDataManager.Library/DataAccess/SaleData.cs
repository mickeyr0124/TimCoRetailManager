using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRMDataManager.Library.Models;

namespace TRMDataManager.Library.Internal.DataAccess
{
    public class SaleData
    {

        public void SaveSale(SaleModel saleInfo, string cashierId)
        {
            //TODO: MAke this SOLID/DRY/Better

            // Start filling in the models we will save to the database
            List<SaleDetailDBModel> details = new List<SaleDetailDBModel>();
            ProductData products = new ProductData();
            var taxRate = ConfigHelper.GetTaxRate()/100;

            foreach (var item in saleInfo.SaleDetails)
            {
                var detail = new SaleDetailDBModel                
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };

                // get the information about this product

                var productInfo = products.GetProductById(item.ProductId);

                if (productInfo == null)
                {
                    throw new Exception($"The product id of { detail.ProductId } could not be found in the database. ");
                }

                detail.PurchasePrice = (productInfo.RetailPrice * detail.Quantity);

                if (productInfo.IsTaxable)
                {
                    detail.Tax = (detail.PurchasePrice * taxRate);
                }


                details.Add(detail);
            }
            // Create sale model
            SaleDBModel sale = new SaleDBModel
            {
                SubTotal = details.Sum(x => x.PurchasePrice),
                Tax = details.Sum(x => x.Tax),
                CashierId = cashierId
            };

            sale.Total = sale.SubTotal + sale.Tax;


            

            using (SqlDataAccess sql = new SqlDataAccess())
            {
                sql.StartTransaction("TRMData");

                try
                {
                    // Save sale model
                    sql.SaveDataInTransaction("dbo.spSale_Insert", sale);

                    // Get ID from sale model
                    sale.Id = sql.LoadDataInTransaction<int, dynamic>("spSale_Lookup", new { CashierId = sale.CashierId, SaleDate = sale.SaleDate }).FirstOrDefault();

                    // Finish filling sale detail models
                    foreach (var item in details)
                    {
                        item.SaleId = sale.Id;
                        // Save sale detail models
                        sql.SaveDataInTransaction("dbo.spSaleDetail_Insert", item);
                    }

                    sql.CommitTransaction();
                }
                catch 
                {
                    sql.RollbackTransaction();
                    throw;
                }              
            }




        }
        //public List<ProductModel> GetProducts()
        //{
        //SqlDataAccess sql = new SqlDataAccess();

        //var output = sql.LoadData<ProductModel, dynamic>("dbo.spProduct_GetAll", new { }, "TRMData");

        //return output;

        //}
    }
}
