using Microsoft.EntityFrameworkCore;

namespace chatBotTwilio.Services.BPI
{
    /// <summary>
    /// Servicio para gestionar proyectos en el sistema BPI.
    /// Proporciona métodos para obtener listas de proyectos y buscar proyectos específicos.
    /// </summary>
    public class ProjectService : IProjectService
    {

        private readonly DbSmpContext _context;
        private readonly ILogger<ProjectService> _logger;

        /// <summary>
        /// Constructor del servicio ProjectService.
        /// Inicializa el contexto de base de datos y el logger.
        /// </summary>
        /// <param name="dbContext">Contexto de base de datos DbSmpContext.</param>
        /// <param name="logger">Instancia de ILogger para logging.</param>
        public ProjectService(DbSmpContext dbContext, ILogger<ProjectService> logger)
        {
            _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        /// <summary>
        /// Obtiene una lista de proyectos ordenados por prioridad.
        /// Incluye información relacionada como campo petrolero, contrato y compañía.
        /// Actualmente filtra por Id > 0 (comentado filtro por prioridad).
        /// </summary>
        /// <param name="priority">Parámetro de prioridad (actualmente no usado).</param>
        /// <returns>Lista de objetos con detalles de proyectos.</returns>
        public async Task<List<object>> GetProjects(int priority)
        {
            try
            {
                return await _context.Projects
                //.Where(p => (p.Priority == priority)) // Filtro comentado
                .Where(p => (p.Id > 0)) // Filtrar proyectos válidos
               .Select(p => new
               {
                   p.Id,
                   p.IdConsecutivo,
                   p.Number,
                   p.Name,
                   p.Description,
                   p.Priority,
                   campo = p.NavOilfield.Name, // Nombre del campo petrolero
                   contrato = p.NavContract.NumberContract, // Número de contrato
                   company = p.NavContract.NavProvider.NameShort // Nombre corto de la compañía
               })
               .OrderBy(p => p.Priority) // Ordenar por prioridad
               .AsNoTracking() // No rastrear para rendimiento
               .ToListAsync<object>();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Project ,  idCompany");
                throw;
            }
        }

        /// <summary>
        /// Busca un proyecto específico por su ID consecutivo.
        /// Devuelve detalles incluyendo campo, contrato, supervisor y compañía.
        /// </summary>
        /// <param name="idConsec">ID consecutivo del proyecto.</param>
        /// <returns>Objeto con detalles del proyecto o null si no se encuentra.</returns>
        public async Task<object> findProject(int idConsec)
        {
            try
            {
                return await _context.Projects
                    .Where(c => c.IdConsecutivo == idConsec) // Filtrar por ID consecutivo
                    .Select(p => new
                    {
                        p.IdConsecutivo,
                        Number = p.Number ?? "", // Número con valor por defecto
                        ProjectName = p.Name ?? "", // Nombre con valor por defecto
                        //Description = p.Description ?? "", // Comentado

                        //oilfield
                        campo = p.NavOilfield.Name, // Nombre del campo

                        //contract
                        contrato = p.NavContract.NumberContract, // Número de contrato
                        supervisor = p.NavContract.Supervisor, // Supervisor del contrato

                        //company with contract
                        company = p.NavContract.NavProvider.NameShort, // Nombre corto de la compañía


                    })
                    .FirstOrDefaultAsync(); // Obtener el primero o null

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contract details");
                throw;
            }
        }


    }

    /// <summary>
    /// Interfaz para el servicio de proyectos.
    /// Define los métodos disponibles para operaciones con proyectos.
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// Obtiene una lista de proyectos por prioridad.
        /// </summary>
        /// <param name="priority">Prioridad de los proyectos.</param>
        /// <returns>Lista de proyectos.</returns>
        Task<List<object>> GetProjects(int priority);

        /// <summary>
        /// Busca un proyecto por ID consecutivo.
        /// </summary>
        /// <param name="idConsec">ID consecutivo del proyecto.</param>
        /// <returns>Detalles del proyecto.</returns>
        Task<object> findProject(int idConsec);
    }
  
}
