using CognitoUserManager.Contracts.Repositories;
using HPANetWeb6.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace HPANetWeb6.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserRepository _userService;
        public const string Session_TokenKey = "_Tokens";
        public HomeController(ILogger<HomeController> logger, IUserRepository userService)
        {
            _logger = logger;
            _userService = userService;
        }


        [Authorize]
        //[Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Index()
        {
            var id = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).First();
            var response = await _userService.GetUserAsync(id.Value);

            var ListGroup = new List<string>();
            foreach (var item in response.Group)
            {
                ListGroup.Add(item);
            }
            var groupItem = ListGroup.Where(x => x.Contains("SuperAdmin")).FirstOrDefault();
            ViewBag.IsSuperAd = groupItem;
            if (groupItem is not null)
            {
                return View();
            }
            return RedirectToAction("NotAuthorizedToViewResources", "Home");

        }
        [Authorize]
        public IActionResult NotAuthorizedToViewResources()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}