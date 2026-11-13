using System.Net;
using MangoFusion_API.Data;
using MangoFusion_API.Models;
using MangoFusion_API.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace MangoFusion_API.Controllers
{
    [Route("api/MenuItem")]
    [ApiController]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ApiResponse _response;
        private readonly IWebHostEnvironment _env;

        public MenuItemController( ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _response = new ApiResponse();
            _env = env;
        }

        [HttpGet]
        public IActionResult GetMenuItems()
        {
            _response.IsSuccess = true;
            _response.Result = _db.MenuItems.ToList();
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        [HttpGet("{id:int}", Name ="GetMenuItem")]
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

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateMenuItem(MenuItemCreateDTO menuItem)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    if (menuItem.File == null || menuItem.File.Length == 0)
                    {
                        _response.IsSuccess = false;
                        _response.ErrorMessages = ["File is required"];
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        return BadRequest(_response);
                    }

                    var imagesPath = Path.Combine(_env.WebRootPath, "images");
                    if (!Directory.Exists(imagesPath))
                    {
                        Directory.CreateDirectory(imagesPath);
                    }
                    var filePath = Path.Combine(imagesPath, menuItem.File.FileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    //UPLOADING IMAGE
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await menuItem.File.CopyToAsync(stream);
                    }

                    MenuItem menuItemStore = new()
                    {
                        Name = menuItem.Name,
                        Description = menuItem.Description,
                        Price = menuItem.Price,
                        Category = menuItem.Category,
                        SpecialTag = menuItem.SpecialTag,
                        Image = "images/"+ menuItem.File.FileName
                    };

                    _db.MenuItems.Add(menuItemStore);
                    await _db.SaveChangesAsync();

                    _response.Result = menuItem;
                    _response.StatusCode = HttpStatusCode.Created;
                    return CreatedAtRoute("GetMenuItem", new { id = menuItemStore.Id }, _response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess=false;
                _response.ErrorMessages = [ex.ToString()];

            }

            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.BadRequest;
            return BadRequest(_response);

        }

    }
}
