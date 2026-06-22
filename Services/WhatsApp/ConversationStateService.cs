using chatBotTwilio.Models.WhatsApp;
using Microsoft.EntityFrameworkCore;

namespace chatBotTwilio.Services.WhatsApp
{
    public class ConversationStateService : IConversationStateService
    {
        private readonly DbSmpContext _context;
        private readonly ILogger<ConversationStateService> _logger;

        public ConversationStateService(DbSmpContext context, ILogger<ConversationStateService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ConversationState> GetStateAsync(string phoneNumber)
        {
            {
                _logger.LogInformation($"Buscando estado para el número {phoneNumber}");

                try
                {
                    var state = await _context.ConversationStates
                        .AsNoTracking()
                        .Where(s => s.PhoneNumber == phoneNumber)
                        .Select(s => new ConversationState(s.PhoneNumber ?? string.Empty)
                        {
                            Id = s.Id,
                            CurrentState     = s.CurrentState,
                            PreviousState    = s.PreviousState,
                            CurrentContract  = s.CurrentContract,
                            CurrentProjectId = s.CurrentProjectId,
                            CurrentProject   = s.CurrentProject,
                            CurrentNoteType  = s.CurrentNoteType,
                            Supervisor       = s.Supervisor,
                            Ot               = s.Ot,
                            Descripcorta     = s.Descripcorta,
                            CreatedAt        = s.CreatedAt,
                            UpdatedAt        = s.UpdatedAt,
                            LastInteraction  = s.LastInteraction,
                            
                        })
                        .FirstOrDefaultAsync();

                    if (state == null)
                    {
                        _logger.LogInformation($"No se encontró estado para {phoneNumber}. Creando uno nuevo.");
                        state = new ConversationState(phoneNumber)
                        {
                            CurrentState = "Start",
                            CurrentProjectId = 0,
                            CurrentProject = "",
                            CurrentNoteType = "",
                            Supervisor = "",
                            Ot = "",
                            Descripcorta = "",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            LastInteraction = DateTime.UtcNow,
                            
                        };
                        _context.ConversationStates.Add(state);
                        await _context.SaveChangesAsync();
                    }                    

                    return state;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error al obtener el estado para {phoneNumber}");
                    throw;
                }
            }
        }


        public async Task SaveStateAsync(ConversationState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            try
            {
                var existingState = await _context.ConversationStates
                    .FirstOrDefaultAsync(s => s.PhoneNumber == state.PhoneNumber);

                if (existingState == null)
                {
                    state.CreatedAt = DateTime.UtcNow;
                    state.UpdatedAt = DateTime.UtcNow;                    
                    await _context.ConversationStates.AddAsync(state);
                }
                else
                {
                    existingState.CurrentState     = state.CurrentState ?? existingState.CurrentState;
                    existingState.PreviousState    = state.PreviousState ?? existingState.PreviousState;
                    existingState.CurrentContract  = state.CurrentContract ?? existingState.CurrentContract;
                    existingState.CurrentProject   = state.CurrentProject ?? existingState.CurrentProject;
                    existingState.CurrentProjectId = state.CurrentProjectId ?? existingState.CurrentProjectId;
                    existingState.Supervisor       = state.Supervisor ?? existingState.Supervisor;
                    existingState.Ot               = state.Ot ?? existingState.Ot;
                    existingState.CurrentNoteType  = state.CurrentNoteType ?? existingState.CurrentNoteType;
                    existingState.Descripcorta     = state.Descripcorta ?? existingState.Descripcorta;

                    existingState.UpdatedAt = DateTime.UtcNow;
                    
                    _context.ConversationStates.Update(existingState);
                    _context.ConversationStates.Update(existingState);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Estado guardado exitosamente para {state.PhoneNumber}");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"Error de concurrencia al guardar el estado para {state.PhoneNumber}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al guardar el estado para {state.PhoneNumber}");
                throw;
            }
        }
                              

        private void UpdateExistingState(ConversationState existingState, ConversationState newState)
        {
            existingState.CurrentProjectId = newState.CurrentProjectId;
            existingState.CurrentState     = newState.CurrentState ?? existingState.CurrentState;
            existingState.PreviousState    = newState.PreviousState ?? existingState.PreviousState;
            existingState.CurrentContract  = newState.CurrentContract ?? existingState.CurrentContract;
            existingState.CurrentProject   = newState.CurrentProject ?? existingState.CurrentProject;
            existingState.Supervisor       = newState.Supervisor ?? existingState.Supervisor;
            existingState.Ot               = newState.Ot ?? existingState.Ot;
            existingState.CurrentNoteType  = newState.CurrentNoteType ?? existingState.CurrentNoteType;
            existingState.LastInteraction  = newState.LastInteraction;
            existingState.UpdatedAt        = DateTime.UtcNow;            
        }
    }

    public interface IConversationStateService
    {
        Task<ConversationState> GetStateAsync(string phoneNumber);
        Task SaveStateAsync(ConversationState state);
    }
}

