using Microsoft.AspNetCore.Mvc;

namespace Hospital_Managment_System.Controllers
{
    public class AppointmentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
