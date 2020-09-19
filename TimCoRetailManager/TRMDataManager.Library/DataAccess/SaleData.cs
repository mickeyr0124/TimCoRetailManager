using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRMDataManager.Library.Models;

namespace TRMDataManager.Library.Internal.DataAccess
{
    public class SaleData : ISaleData
    {
        private readonly IProductData _productData;
        private readonly ISqlDataAccess _sql;

        public SaleData(IProductData productData, ISqlDataAccess sql)
        {
            _productData = productData;
            _sql = sql;
        }
        public void SaveSale(SaleModel saleInfo, string cashierId)
        {
            //TODO: MAke this SOLID/DRY/Better

            // Start filling in the models we will save to the database
            List<SaleDetailDBModel> details = new List<SaleDetailDBModel>();
            var taxRate = ConfigHelper.GetTaxRate() / 100;

            foreach (var item in saleInfo.SaleDetails)
            {
                var detail = new SaleDetailDBModel
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };

                // get the information about this product

                var productInfo = _productData.GetProductById(item.ProductId);

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

            _sql.StartTransaction("TRMData");

            try
            {
                // Save sale model
                _sql.SaveDataInTransaction("dbo.spSale_Insert", sale);

                // Get ID from sale model
                sale.Id = _sql.LoadDataInTransaction<int, dynamic>("spSale_Lookup", new { CashierId = sale.CashierId, SaleDate = sale.SaleDate }).FirstOrDefault();

                // Finish filling sale detail models
                foreach (var item in details)
                {
                    item.SaleId = sale.Id;
                    // Save sale detail models
                    _sql.SaveDataInTransaction("dbo.spSaleDetail_Insert", item);
                }

                _sql.CommitTransaction();
            }
            catch
            {
                _sql.RollbackTransaction();
                throw;
            }
        }

        public List<SaleReportModel> GetSaleReport()
        {
            var output = _sql.LoadData<SaleReportModel, dynamic>("dbo.spSale_SaleReport", new { }, "TRMData");

            return output;
        }

    }
}
