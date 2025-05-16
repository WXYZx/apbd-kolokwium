using Kolokwium1.Exceptions;
using Kolokwium1.Models;
using Kolokwium1.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium1.Controlers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveriesController
    {
        private readonly IDeliveryServices _iAServices;
        
        public DeliveriesController(IDeliveryServices iAServices)
        {
            _iAServices = iAServices;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetA(string id)
        {
            var aDTO = await _iAServices.getDelivery(id);

            if (aDTO == null)
            {
                return NotFound("not found");
            }

            return Ok(aDTO);
        }
        
        [HttpPost]
        public async Task<IActionResult> AddNewDelivery([FromBody] postDeliveryDTO postDeliveryDto)
        {


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                await _iAServices.addNewDelivery(postDeliveryDto);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ConflictException ex)
            {
                return Conflict(ex.Message);
            }


            return Created("", "utworzone");
        }
    }
}