using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GZCDCPay.Areas.Admin.Controllers
{
    [AreaAttribute("Admin")]
    public class HomeController: Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}