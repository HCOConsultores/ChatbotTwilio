using chatBotTwilio.Models.BPI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using chatBotTwilio.Services.BPI;

namespace chatBotTwilio.Controllers.BPI
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogbookController : ControllerBase
    {
        private readonly ILogbookService _lbService;
        private readonly ILogger<LogbookController> _logger;

        public LogbookController(ILogbookService logbookService, ILogger<LogbookController> logger)
        {
            _lbService = logbookService ?? throw new ArgumentNullException(nameof(logbookService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet()]
        public async Task<IActionResult> Showlb(DateTime lb, int id)
        {
            try
            {
                var projectExists = await _lbService.showlogbookt(lb, id);
                if (projectExists == null)
                {
                    return NotFound($"Loogbook does not exist.");
                }

                return Ok(projectExists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if Logbook exists");
                return StatusCode(500, "An error occurred while checking if logbook exists");
            }
        }

        [HttpGet("showphotos")]
        public async Task<IActionResult> Showph(DateTime lb, int id)
        {
            try
            {
                var projectExists = await _lbService.showlogPhotos(lb, id);
                if (projectExists == null)
                {
                    return NotFound($"Loogbook Photos does not exist.");
                }

                return Ok(projectExists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if Logbook exists");
                return StatusCode(500, "An error occurred while checking if logbook exists");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Logbook lb)
        {
            try
            {
                await _lbService.Save(lb);
                return CreatedAtAction(nameof(Showlb),
                    new { description = lb.Description, type = lb.TypeNote }, lb);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Logbook");
                return StatusCode(500, "An error occurred while saving the warehouse.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Logbook lb)
        {
            try
            {
                var success = await _lbService.Update(id, lb);
                if (success)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Logbook with ID {Id}", id);
                return StatusCode(500, "An error occurred while updating the Logbook");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _lbService.Delete(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting LogBook with ID {Id}", id);
                return StatusCode(500, "An error occurred while deleting the Logbook.");
            }
        }

    }
}
