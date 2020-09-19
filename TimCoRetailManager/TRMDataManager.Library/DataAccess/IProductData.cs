using System.Collections.Generic;
using TRMDataManager.Library.Models;

namespace TRMDataManager.Library.Internal.DataAccess
{
    public interface IProductData
    {
        ProductModel GetProductById(int ProductId);
        List<ProductModel> GetProducts();
    }
}