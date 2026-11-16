using System.Net;
using MangoFusion_API.Data;
using MangoFusion_API.Models;
using MangoFusion_API.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MangoFusion_API.Controllers
{
    [Route("api/MenuItem")]
    [ApiController]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ApiResponse _response;
        private readonly IWebHostEnvironment _env;

        public MenuItemController(ApplicationDbContext db, IWebHostEnvironment env)
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

        [HttpGet("{id:int}", Name = "GetMenuItem")]
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
        public async Task<ActionResult<ApiResponse>> CreateMenuItem([FromForm] MenuItemCreateDTO menuItem)
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
                        Image = "images/" + menuItem.File.FileName
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
                _response.IsSuccess = false;
                _response.ErrorMessages = [ex.ToString()];

            }

            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.BadRequest;
            return BadRequest(_response);

        }

        [HttpPut]
        public async Task<ActionResult<ApiResponse>> UpdateMenuItem(int id, [FromForm] MenuItemUpdateDTO menuItem)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    if (menuItem == null || menuItem.Id != id)
                    {
                        _response.IsSuccess = false;
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        return BadRequest(_response);
                    }

                    MenuItem? menuItemDB = await _db.MenuItems.FirstOrDefaultAsync(x => x.Id == id);

                    if (menuItemDB == null)
                    {
                        _response.IsSuccess = false;
                        _response.StatusCode = HttpStatusCode.NotFound;
                        return NotFound(_response);
                    }

                    menuItemDB.Name = menuItem.Name;
                    menuItemDB.Category = menuItem.Category;
                    menuItemDB.Description = menuItem.Description;
                    menuItemDB.Price = menuItem.Price;
                    menuItemDB.SpecialTag = menuItem.SpecialTag;

                    if (menuItem.File != null && menuItem.File.Length > 0)
                    {
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

                        var filePath_OldFile = Path.Combine(_env.WebRootPath, menuItemDB.Image);
                        if (System.IO.File.Exists(filePath_OldFile))
                        {
                            System.IO.File.Delete(filePath_OldFile);
                        }

                        //UPLOADING IMAGE
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await menuItem.File.CopyToAsync(stream);
                        }

                        menuItemDB.Image = "images/" + menuItem.File.FileName;
                    }

                    _db.MenuItems.Update(menuItemDB);
                    await _db.SaveChangesAsync();

                    _response.Result = menuItem;
                    _response.StatusCode = HttpStatusCode.NoContent;
                    return Ok(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = [ex.ToString()];

            }

            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.BadRequest;
            return BadRequest(_response);

        }



        [HttpDelete]
        public async Task<ActionResult<ApiResponse>> DeleteMenuItem(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);

                }

                MenuItem? menuItemDB = await _db.MenuItems.FirstOrDefaultAsync(menuItem => menuItem.Id == id);

                if (menuItemDB == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }



                var filePath_OldFile = Path.Combine(_env.WebRootPath, menuItemDB.Image);
                if (System.IO.File.Exists(filePath_OldFile))
                {
                    System.IO.File.Delete(filePath_OldFile);
                }

                _db.MenuItems.Remove(menuItemDB);
                await _db.SaveChangesAsync();

                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = [ex.ToString()];
            }

            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.BadRequest;
            return BadRequest(_response);
        }

    }
}
