using Microsoft.AspNetCore.Mvc;

namespace Hospital_Managment_System.Controllers
{
    public class PrescriptionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
