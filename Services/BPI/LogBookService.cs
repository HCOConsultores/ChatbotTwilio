using chatBotTwilio.Hub;
using chatBotTwilio.Models.BPI;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace chatBotTwilio.Services.BPI
{
    /// <summary>
    /// Servicio para gestionar bitácoras (logbooks) en el sistema BPI.
    /// Maneja operaciones CRUD para logbooks, incluyendo notificaciones en tiempo real vía SignalR.
    /// </summary>
    public class LogbookService : ILogbookService
    {
        private readonly DbSmpContext _context;
        private readonly ILogger<LogbookService> _logger;
        private readonly IHubContext<StorageHub> _hubContext;

        /// <summary>
        /// Constructor del servicio LogbookService.
        /// Inicializa el contexto de base de datos, logger y contexto de SignalR Hub.
        /// </summary>
        /// <param name="dbContext">Contexto de base de datos DbSmpContext.</param>
        /// <param name="logger">Instancia de ILogger para logging.</param>
        /// <param name="hubContext">Contexto del Hub de SignalR para notificaciones.</param>
        public LogbookService(DbSmpContext dbContext, ILogger<LogbookService> logger, IHubContext<StorageHub> hubContext)
        {
            _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hubContext = hubContext;
        }

        /// <summary>
        /// Muestra las entradas de bitácora de texto (excluyendo fotos) para una fecha y proyecto específicos.
        /// Ordena los resultados por el campo Orden.
        /// </summary>
        /// <param name="dateLog">Fecha de las entradas de bitácora.</param>
        /// <param name="id">ID del proyecto.</param>
        /// <returns>Lista de objetos con detalles de las entradas de texto.</returns>
        public async Task<List<object>> showlogbookt(DateTime dateLog, int id)
        {
            try
            {
                return await _context.Logbooks
                    .Where(l => l.Date == dateLog.Date && l.IdProject == id && l.TypeNote != "Photo") // Filtrar por fecha, proyecto y tipo no foto
                    .Select(lb => new
                    {
                        lb.Date,
                        lb.Description,
                        lb.TypeNote,
                        lb.Timexnote,
                        lb.Supervisor,
                        lb.Orden
                    })
                     .OrderBy(lb => lb.Orden) // Ordenar por orden
                     .AsNoTracking() // No rastrear para rendimiento
                     .ToListAsync<object>();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting logbooks details");
                throw;
            }
        }

        /// <summary>
        /// Muestra las entradas de bitácora de fotos para una fecha y proyecto específicos.
        /// Ordena los resultados por el campo Orden.
        /// </summary>
        /// <param name="dateLog">Fecha de las entradas de bitácora.</param>
        /// <param name="id">ID del proyecto.</param>
        /// <returns>Lista de objetos con detalles de las entradas de fotos.</returns>
        public async Task<List<object>> showlogPhotos(DateTime dateLog, int id)
        {
            try
            {
                return await _context.Logbooks
                    .Where(l => l.Date == dateLog.Date && l.IdProject == id && l.TypeNote == "Photo") // Filtrar por fecha, proyecto y tipo foto
                    .Select(lb => new
                    {
                        lb.Date,
                        lb.Description,
                        lb.TypeNote,
                        lb.Timexnote,
                        lb.ImageAzure,
                        lb.Supervisor,
                        lb.Orden
                    })
                     .OrderBy(lb => lb.Orden) // Ordenar por orden
                     .AsNoTracking() // No rastrear para rendimiento
                     .ToListAsync<object>();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting logbooks details");
                throw;
            }
        }

        /// <summary>
        /// Guarda una nueva entrada de bitácora en la base de datos.
        /// Después de guardar, notifica a todos los clientes conectados vía SignalR según el tipo de nota.
        /// </summary>
        /// <param name="lbh">Objeto Logbook a guardar.</param>
        public async Task Save(Logbook lbh)
        {
            try
            {
                _context.Logbooks.Add(lbh);
                await _context.SaveChangesAsync(); // Guardar en BD

                // Crear objeto para notificación
                // Crear objeto para notificación, serializamos directamente el objeto lbh para enviar toda la información nueva
                string jsonString = JsonConvert.SerializeObject(lbh, new JsonSerializerSettings 
                { 
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore 
                });

                // Notificar vía SignalR basado en tipo y orden
                if (lbh.Orden == 4 && lbh.TypeNote == "Photo")
                {
                    await _hubContext.Clients.All.SendAsync("ReceivePhotoUpdate", jsonString);
                }
                else
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveTextUpdate", jsonString);
                }

            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error while saving Logbooks");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving logbooks");
                throw;
            }
        }

        /// <summary>
        /// Actualiza una entrada de bitácora existente por ID.
        /// Maneja excepciones de concurrencia.
        /// </summary>
        /// <param name="id">ID de la entrada a actualizar.</param>
        /// <param name="lgs">Objeto Logbook con los nuevos valores.</param>
        /// <returns>True si la actualización fue exitosa, false si no se encontró la entrada.</returns>
        public async Task<bool> Update(int id, Logbook lgs)
        {
            var existingWarehouse = await _context.Logbooks.FindAsync(id);
            if (existingWarehouse == null)
            {
                return false;
            }

            try
            {
                _context.Entry(existingWarehouse).CurrentValues.SetValues(lgs);
                await _context.SaveChangesAsync();
                //await NotifyWarehouseUpdate(existingWarehouse.IdCompany, existingWarehouse.IdProject); // Notificación comentada
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error while updating Logbooks with ID {Id}", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Logbooks with ID {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Elimina una entrada de bitácora por ID si existe.
        /// </summary>
        /// <param name="id">ID de la entrada a eliminar.</param>
        public async Task Delete(int id)
        {
            var warehouse = await _context.Logbooks.FindAsync(id);
            if (warehouse != null)
            {
                try
                {
                    _context.Logbooks.Remove(warehouse);
                    await _context.SaveChangesAsync();
                    // await NotifyWarehouseUpdate(warehouse.IdCompany, warehouse.IdProject); // Notificación comentada
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting warehouse with ID {Id}", id);
                    throw;
                }
            }
        }

        /// <summary>
        /// Obtiene el conteo de fotos para un proyecto en la fecha actual de México.
        /// </summary>
        /// <param name="projectId">ID del proyecto.</param>
        /// <returns>Número de fotos para el proyecto hoy.</returns>
        public async Task<int> GetPhotoCountForProject(int projectId)
        {
            return await _context.Logbooks
                .CountAsync(l => l.IdProject == projectId &&
                                 l.TypeNote == "Photo" &&
                                 l.Date == DateTimeHelper.GetCurrentMexicoDateTime());
        }


        /// <summary>
        /// Clase helper para manejar fechas y zonas horarias, específicamente para México.
        /// </summary>
        public static class DateTimeHelper
        {
            private static readonly TimeZoneInfo _zonaHorariaMexico =
                TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");

            /// <summary>
            /// Obtiene la fecha y hora actual en la zona horaria de México.
            /// </summary>
            /// <returns>Fecha y hora actual en México.</returns>
            public static DateTime GetCurrentMexicoDateTime() =>
                TimeZoneInfo.ConvertTime(DateTime.Now, _zonaHorariaMexico);
        }

    }

    /// <summary>
    /// Interfaz para el servicio de bitácoras.
    /// Define los métodos disponibles para operaciones con logbooks.
    /// </summary>
    public interface ILogbookService
    {
        /// <summary>
        /// Guarda una nueva entrada de bitácora.
        /// </summary>
        /// <param name="lbh">Entrada de bitácora a guardar.</param>
        Task Save(Logbook lbh);

        /// <summary>
        /// Actualiza una entrada de bitácora por ID.
        /// </summary>
        /// <param name="id">ID de la entrada.</param>
        /// <param name="lgs">Nuevos valores.</param>
        /// <returns>True si exitoso.</returns>
        Task<bool> Update(int id, Logbook lgs);

        /// <summary>
        /// Muestra entradas de texto de bitácora.
        /// </summary>
        /// <param name="dateLog">Fecha.</param>
        /// <param name="id">ID del proyecto.</param>
        /// <returns>Lista de entradas.</returns>
        Task<List<object>> showlogbookt(DateTime dateLog, int id);

        /// <summary>
        /// Muestra entradas de fotos de bitácora.
        /// </summary>
        /// <param name="dateLog">Fecha.</param>
        /// <param name="id">ID del proyecto.</param>
        /// <returns>Lista de entradas.</returns>
        Task<List<object>> showlogPhotos(DateTime dateLog, int id);

        /// <summary>
        /// Elimina una entrada de bitácora por ID.
        /// </summary>
        /// <param name="id">ID de la entrada.</param>
        Task Delete(int id);

        /// <summary>
        /// Obtiene el conteo de fotos para un proyecto.
        /// </summary>
        /// <param name="projectId">ID del proyecto.</param>
        /// <returns>Conteo de fotos.</returns>
        Task<int> GetPhotoCountForProject(int projectId);

    }
}
