using System.Collections.Generic;
using TRMDataManager.Library.Models;

namespace TRMDataManager.Library.Internal.DataAccess
{
    public interface ISaleData
    {
        List<SaleReportModel> GetSaleReport();
        void SaveSale(SaleModel saleInfo, string cashierId);
    }
}