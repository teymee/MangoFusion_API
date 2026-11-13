using System.Net;
using MangoFusion_API.Data;
using MangoFusion_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace MangoFusion_API.Controllers
{
    [Route("api/MenuItem")]
    [ApiController]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ApiResponse _response;

        public MenuItemController( ApplicationDbContext db)
        {
            _db = db;
            _response = new ApiResponse();
        }

        [HttpGet]
        public IActionResult GetMenuItems()
        {
            _response.IsSuccess = true;
            _response.Result = _db.MenuItems.ToList();
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetMenuItem(int id)
        {
            if (id == 0)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            MenuItem? menuItem = _db.MenuItems.FirstOrDefault(u => u.Id == id);
            _response.Result = menuItem;
            _response.StatusCode = HttpStatusCode.OK;

            return Ok(_response);

        }
    }
}
