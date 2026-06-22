using chatBotTwilio.Services.BPI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace chatBotTwilio.Controllers.BPI
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly ILogger<ContractController> _logger;


        public ContractController(IContractService contractService, ILogger<ContractController> logger)
        {
            _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet()]
        public async Task<ActionResult<List<object>>> GetWarByComp(int idCompany)
        {
            try
            {
                var contract = await _contractService.GetContracts(idCompany);
                if (contract == null || contract.Count == 0)
                {
                    return NotFound();
                }
                return Ok(contract);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Contracts for company");
                return StatusCode(500, "An error occurred while retrieving Contracts");
            }
        }

        [HttpGet("{contract}")]
        public async Task<IActionResult> FindContract(string contract)
        {
            try
            {
                var contractExists = await _contractService.findContract(contract);
                if (contractExists == null)
                {
                    return NotFound($"Contract {contract} does not exist.");
                }

                return Ok(contractExists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if contract exists");
                return StatusCode(500, "An error occurred while checking if contract exists");
            }
        }


    }
}
