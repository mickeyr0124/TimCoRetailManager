﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TRMDataManager.Library.Internal.DataAccess;
using TRMDataManager.Library.Models;

namespace TRMApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SaleController : ControllerBase
    {

            [Authorize(Roles = "Cashier")]
            public void Post(SaleModel sale)
            {
                SaleData data = new SaleData();
            string userID = User.FindFirstValue(ClaimTypes.NameIdentifier); // old way - RequestContext.Principal.Identity.GetUserId();

                data.SaveSale(sale, userID);
            }

            [Authorize(Roles = "Admin,Manager")]
            [Route("GetSalesReport")]
            public List<SaleReportModel> GetSalesReport()
            {
                SaleData data = new SaleData();
                return data.GetSaleReport();
            }

        }

    }
