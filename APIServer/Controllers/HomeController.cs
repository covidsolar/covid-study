using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using APIServer.Models;

namespace APIServer.Controllers;

public class HomeController : Controller
{

    public IActionResult Index()
    {
        return View();
    }

}
