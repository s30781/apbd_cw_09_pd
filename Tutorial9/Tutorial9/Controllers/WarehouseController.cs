using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model;
using Tutorial9.Services;

namespace Tutorial9.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseController(IWarehouseService _warehouseService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> AddProductToWarehouse([FromBody] AddProductWarehouse product)
        {
            if (product == null){return BadRequest("Product data cannot be null"); }

            try
            {
                var id = await _warehouseService.AddProductToWarehouse(product);
                return Ok(id);
            }
            catch (Exception e)
            {return BadRequest(e.Message);}
        }

        [HttpPost("Procedure")]
        public async Task<IActionResult> AddProductToWarehouseProcedure([FromBody] AddProductWarehouse product)
        {
            if (product == null) {return BadRequest("Product data cannot be null"); }

            try
            {
                var result = await _warehouseService.AddProductToWarehouseProcedure(product);
                return Ok(result);
            }
            catch (Exception e)
            {return BadRequest(e.Message);}
        }
    }
}