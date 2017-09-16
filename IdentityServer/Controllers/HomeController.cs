using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace IdentityServer.Controllers
{
	[AllowAnonymous]
    public class HomeController : Controller
    {
		/// <summary>
		/// We'll need to add a MessageId design to the home controller. See ManageController
		/// </summary>
		/// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
