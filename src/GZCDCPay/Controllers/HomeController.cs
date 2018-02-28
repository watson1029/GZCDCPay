using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GZCDCPay.Models;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GZCDCPay.Controllers
{
    [Route("[controller]")]
    public class HomeController: Controller
    {
        public IActionResult Index()
        {
            return Content("Not implemented");
        }

        public IActionResult Error(PaymentError model)
        {
            return View(model);
        }
    }
}