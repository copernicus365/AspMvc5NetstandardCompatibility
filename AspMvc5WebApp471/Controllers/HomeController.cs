using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AspMvc5WebApp471.Models;
using FooProj_NetStandard20;

namespace AspMvc5WebApp471.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			var vm = new AboutViewModel() {
				Title = "How's netstandard looking in Razor?!",
				AnimalTyp = AnimalType.Alligator
			};
			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}
	}
}