using Microsoft.EntityFrameworkCore;

namespace chatBotTwilio.Services.BPI
{
    /// <summary>
    /// Servicio para gestionar contratos en el sistema BPI.
    /// Implementa la interfaz IContractService para proporcionar operaciones relacionadas con contratos.
    /// </summary>
    public class ContractService : IContractService
    {
        private readonly DbSmpContext _context;
        private readonly DbSmpContext ss; // Campo no utilizado, duplicado de _context
        private readonly ILogger<ContractService> _logger;

        /// <summary>
        /// Constructor del servicio ContractService.
        /// Inicializa el contexto de base de datos y el logger.
        /// </summary>
        /// <param name="dbContext">Contexto de base de datos DbSmpContext.</param>
        /// <param name="logger">Instancia de ILogger para logging.</param>
        public ContractService(DbSmpContext dbContext, ILogger<ContractService> logger)
        {
            _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        /// <summary>
        /// Obtiene una lista de contratos asociados a una compañía específica.
        /// Filtra contratos por IdProvider y selecciona campos relevantes incluyendo el nombre del proveedor.
        /// </summary>
        /// <param name="idCompany">ID de la compañía (proveedor) para filtrar contratos.</param>
        /// <returns>Lista de objetos anónimos con detalles de contratos.</returns>
        public async Task<List<object>> GetContracts(int idCompany)
        {
            try
            {
                return await _context.Contracts
                    .Where(c => (c.IdProvider == idCompany)) // Filtrar por proveedor
                    .Select(c => new
                    {
                        c.Id,
                        c.NumberContract,
                        c.StateContract,
                        c.Description,
                        c.DescripSmall,
                        c.AmountMx,
                        c.Term,
                        c.AmountDll,
                        c.Resident,
                        c.Supervisor,
                        c.NavProvider.Name // Incluir nombre del proveedor
                    })
                    .AsNoTracking() // No rastrear entidades para mejor rendimiento
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Contract for company and project ,  idCompany");
                throw;
            }
        }

        /// <summary>
        /// Busca un contrato específico por su número de contrato.
        /// Devuelve detalles básicos del contrato si se encuentra.
        /// </summary>
        /// <param name="contractNumber">Número del contrato a buscar.</param>
        /// <returns>Objeto anónimo con detalles del contrato o null si no se encuentra.</returns>
        public async Task<object> findContract(string contractNumber)
        {
            try
            {
                return await _context.Contracts
                    .Where(c => c.NumberContract == contractNumber) // Filtrar por número de contrato
                    .Select(c => new
                    {
                        c.NumberContract,
                        c.Description,
                        c.DescripSmall,
                        c.DateStar,
                        c.DateEnd,
                        c.AmountMx,
                        c.AmountDll
                    })
                    .FirstOrDefaultAsync(); // Obtener el primero o null
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contract details for {ContractNumber}", contractNumber);
                throw;
            }
        }
    }


    /// <summary>
    /// Interfaz para el servicio de contratos.
    /// Define los métodos disponibles para operaciones con contratos.
    /// </summary>
    public interface IContractService
    {
        /// <summary>
        /// Obtiene contratos por ID de compañía.
        /// </summary>
        /// <param name="idCompany">ID de la compañía.</param>
        /// <returns>Lista de contratos.</returns>
        Task<List<object>> GetContracts(int idCompany);

        /// <summary>
        /// Busca un contrato por número.
        /// </summary>
        /// <param name="contractNumber">Número del contrato.</param>
        /// <returns>Detalles del contrato.</returns>
        Task<object> findContract(string contractNumber);
    }
}
