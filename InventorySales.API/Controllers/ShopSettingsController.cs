using InventorySales.API.Data;
using InventorySales.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySales.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopSettingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ShopSettingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var settings = await _context.ShopSettings
                .FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new ShopSetting
                {
                    ShopName = "Inventory & Sales"
                };

                _context.ShopSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return Ok(settings);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSettings(
            ShopSetting updatedSettings)
        {
            var settings = await _context.ShopSettings
                .FirstOrDefaultAsync();

            if (settings == null)
            {
                updatedSettings.Id = 0;

                _context.ShopSettings.Add(updatedSettings);
            }
            else
            {
                settings.ShopName = updatedSettings.ShopName;
                settings.Address = updatedSettings.Address;
                settings.Phone = updatedSettings.Phone;
                settings.Email = updatedSettings.Email;
                settings.Logo = updatedSettings.Logo;
                settings.InvoiceFooter = updatedSettings.InvoiceFooter;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Shop settings updated successfully."
            });
        }
    }
}