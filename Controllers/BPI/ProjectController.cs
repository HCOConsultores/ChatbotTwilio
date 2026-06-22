using chatBotTwilio.Services.BPI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace chatBotTwilio.Controllers.BPI
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ILogger<ProjectController> _logger;


        public ProjectController(IProjectService projectService, ILogger<ProjectController> logger)
        {
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("find/{idc}")]
        public async Task<IActionResult> FindProject(int idc)
        {
            try
            {
                var projectExists = await _projectService.findProject(idc);
                if (projectExists == null)
                {
                    return NotFound($"Consecutive {idc} does not exist.");
                }
                return Ok(projectExists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if Project exists");
                return StatusCode(500, "An error occurred while checking if Project exists");
            }
        }

        [HttpGet()]
        public async Task<IActionResult> Projects(int priority)
        {
            try
            {
                var projectExists = await _projectService.GetProjects(priority);
                if (projectExists == null)
                {
                    return NotFound($"The Projects does not exist.");
                }

                return Ok(projectExists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if Project exists");
                return StatusCode(500, "An error occurred while checking if Project exists");
            }
        }
    }
}
    

