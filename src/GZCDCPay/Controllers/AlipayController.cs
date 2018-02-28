using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GZCDCPay.Data;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GZCDCPay.Controllers
{
    [Route("[controller]")]
    public class AlipayController: Controller
    {
        [AcceptVerbs("Get", "Post")]
        public IActionResult Index()
        {
            return Content("Not implemented");
        }
    }
}