using chatBotTwilio.Models.BPI;
using chatBotTwilio.Models.WhatsApp;
using chatBotTwilio.Services.BPI;
using System.Text.Json;
using System.Globalization;

namespace chatBotTwilio.Services.WhatsApp
{
    public class ConversationManagerService : IConversationManagerService
    {
        private const int DAILY_PHOTO_LIMIT = 30;
        private readonly IConversationStateService _stateRepository;
        private readonly IContractService _contractService;
        private readonly IProjectService _projectService;
        private readonly ISendWhatsappService _sendWhatsAppService;
        private readonly ILogbookService _logbookService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConversationManagerService> _logger;

        public ConversationManagerService(
            IConversationStateService conversationStateService,
            IContractService contractService,
            IProjectService projectService,
            ILogbookService logbookService,
            IConfiguration configuration,
            ILogger<ConversationManagerService> logger)
        {
            _stateRepository = conversationStateService;
            _contractService = contractService;
            _projectService = projectService;
            _logbookService = logbookService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> ProcessMessage(string from, string body, string numMedia, string mediaUrl, string mediaContentType, string imageDescription = null)
        {
            _logger.LogInformation($"Procesando mensaje para {from}: {body}");

            var state = await _stateRepository.GetStateAsync(from);
            if (state == null)
            {
                state = new ConversationState(from) { CurrentState = "ESPERANDO_PROYECTO" };
                await _stateRepository.SaveStateAsync(state);
            }

            string response;
            bool isImage = !string.IsNullOrEmpty(numMedia) && int.Parse(numMedia) > 0;

            try
            {
                // Manejador central (State Machine)
                response = await HandleState(state, body, isImage, mediaUrl, from);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error procesando mensaje para {from}");
                response = "Lo siento, ha ocurrido un error no esperado. Volvamos al menú principal.\n\n" + GetMainMenuOptions();
                state.CurrentState = "MENU_PRINCIPAL";
            }

            state.LastInteraction = DateTime.UtcNow;
            await _stateRepository.SaveStateAsync(state);
            return response;
        }

        private async Task<string> HandleState(ConversationState state, string msg, bool isImage, string mediaUrl, string phone)
        {
            msg = msg?.Trim() ?? "";

            switch (state.CurrentState)
            {
                case "ESPERANDO_PROYECTO":
                case "INICIO":
                    return await Auth_ProcessId(state, msg);
                    
                case "MENU_PRINCIPAL":
                    return await MainMenu_Process(state, msg);
                    
                case "MENU_PMO":
                    return await PmoMenu_Process(state, msg);
                    
                case "DIARIO_INIT_FECHA":
                    return await DiarioInit_ProcessFecha(state, msg);
                case "DIARIO_INIT_INICIO":
                    return await DiarioInit_ProcessInicio(state, msg);
                case "DIARIO_INIT_FIN":
                    return await DiarioInit_ProcessFin(state, msg);
                    
                case "MENU_DIARIO":
                    return await DiarioMenu_Process(state, msg);

                // ================= FLUJOS PMO =================
                // Actividades PMO
                case "PMO_ACT_1_ID":
                    return await Step_SaveTextAndGo(state, msg, "task_reference", "PMO_ACT_2_AVANCE", "Por favor ingresa el *Avance Real* (ej. 50.5):");
                case "PMO_ACT_2_AVANCE":
                    return await Step_SaveDecimalAndGo(state, msg, "quantity", "PMO_ACT_3_COMENTARIO", "Por favor ingresa los *Comentarios* de la tarea:");
                case "PMO_ACT_3_COMENTARIO":
                    return await Step_SaveTextAndGo(state, msg, "description", "PMO_ACT_4_FOTO", "Por favor envía una *Evidencia Fotográfica*:");
                case "PMO_ACT_4_FOTO":
                    return await PmoAct_SavePhotoAndDb(state, isImage, mediaUrl, phone);
                case "PMO_ACT_5_MAS_FOTOS":
                    return await PmoAct_ProcessMasFotos(state, msg);
                case "PMO_ACT_6_FOTO_EXTRA":
                    return await PmoAct_SaveExtraPhoto(state, isImage, mediaUrl, phone);

                // Equipos PMO
                case "PMO_EQ_1_TIPO":
                    return await Step_SaveTextAndGo(state, msg, "typenote", "PMO_EQ_2_DESC", "Por favor ingresa una *Descripción Breve* del equipo:");
                case "PMO_EQ_2_DESC":
                    return await Step_SaveTextAndGo(state, msg, "description", "PMO_EQ_3_ID", "Por favor ingresa el *ID de Cronograma asociado*:");
                case "PMO_EQ_3_ID":
                    return await Step_SaveTextAndGo(state, msg, "task_reference", "PMO_EQ_4_CANTIDAD", "Por favor ingresa la *Recepción Total de Equipo o Paquete* (cantidad numérica):");
                case "PMO_EQ_4_CANTIDAD":
                    return await Step_SaveDecimalAndGo(state, msg, "quantity", "PMO_EQ_5_FOTO", "Por favor envía una *Evidencia Fotográfica*:");
                case "PMO_EQ_5_FOTO":
                    return await PmoEq_SavePhotoAndDb(state, isImage, mediaUrl, phone);
                case "PMO_EQ_6_MAS_FOTOS":
                    return await PmoEq_ProcessMasFotos(state, msg);
                case "PMO_EQ_7_FOTO_EXTRA":
                    return await PmoEq_SaveExtraPhoto(state, isImage, mediaUrl, phone);

                // Afectaciones PMO
                case "PMO_AF_1_TIPO":
                    return await Step_SaveTextAndGo(state, msg, "typenote", "PMO_AF_2_DESC", "Por favor ingresa la *Descripción* de la afectación:");
                case "PMO_AF_2_DESC":
                    return await Step_SaveTextAndGo(state, msg, "description", "PMO_AF_3_INICIO", "Por favor ingresa la *Hora de Inicio* (formato HH:MM, ej 14:30):");
                case "PMO_AF_3_INICIO":
                    return await Step_SaveTimeAndGo(state, msg, "start", "PMO_AF_4_FIN", "Por favor ingresa la *Hora de Fin* (formato HH:MM):");
                case "PMO_AF_4_FIN":
                    return await Step_SaveTimeAndGo(state, msg, "end", "PMO_AF_5_ID", "Por favor ingresa el *ID de la Tarea Afectada*:");
                case "PMO_AF_5_ID":
                    return await PmoAf_SaveDb(state, msg, phone);

                // Cambios PMO
                case "PMO_CAM_1_TIPO":
                    return await Step_SaveTextAndGo(state, msg, "typenote", "PMO_CAM_2_DESC", "Por favor ingresa la *Descripción del Cambio*:");
                case "PMO_CAM_2_DESC":
                    return await PmoCam_SaveDb(state, msg, phone);

                // ================= FLUJOS DIARIO =================
                // Actividades Diario
                case "DIA_ACT_1_ID":
                    return await Step_SaveTextAndGo(state, msg, "task_reference", "DIA_ACT_2_AVANCE", "Por favor ingresa el *Avance Real* (numérico):");
                case "DIA_ACT_2_AVANCE":
                    return await Step_SaveDecimalAndGo(state, msg, "quantity", "DIA_ACT_3_TAREAS", "Por favor describe las *Tareas por Actividad* (Durante Jornada):");
                case "DIA_ACT_3_TAREAS":
                    return await Step_SaveTextAndGo(state, msg, "description", "DIA_ACT_4_OBS", "Por favor ingresa las *Observaciones* (Comentarios generales del día):");
                case "DIA_ACT_4_OBS":
                    return await DiaAct_SaveDb(state, msg, phone);

                // Volúmenes Diario
                case "DIA_VOL_1_PARTIDA":
                    return await Step_SaveTextAndGo(state, msg, "task_reference", "DIA_VOL_2_CANTIDAD", "Por favor ingresa el *Volumen Ejecutado del día* (numérico):");
                case "DIA_VOL_2_CANTIDAD":
                    return await DiaVol_SaveDb(state, msg, phone);

                // Afectaciones Diario
                case "DIA_AF_1_TIPO":
                    return await Step_SaveTextAndGo(state, msg, "typenote", "DIA_AF_2_DESC", "Por favor ingresa la *Descripción*:");
                case "DIA_AF_2_DESC":
                    return await Step_SaveTextAndGo(state, msg, "description", "DIA_AF_3_TAREAS", "Por favor ingresa las *Tareas Asociadas* (ID de tarea afectada):");
                case "DIA_AF_3_TAREAS":
                    return await Step_SaveTextAndGo(state, msg, "task_reference", "DIA_AF_4_DURACION", "Por favor ingresa la *Duración/Observaciones*:");
                case "DIA_AF_4_DURACION":
                    return await DiaAf_SaveDb(state, msg, phone);

                // Recursos Diario
                case "DIA_REC_1_TIPO":
                    return await DiaRec_ProcessTipo(state, msg);
                case "DIA_REC_2_NOMBRE":
                    return await Step_SaveTextAndGo(state, msg, "task_reference", "DIA_REC_3_CANTIDAD", "Por favor ingresa la *Cantidad* (numérica):");
                case "DIA_REC_3_CANTIDAD":
                    return await DiaRec_ProcessCantidad(state, msg, phone);
                case "DIA_REC_4_COMBUSTIBLE":
                    return await DiaRec_ProcessCombustible(state, msg, phone);

                // Foto Diario
                case "DIA_FOT_1_FOTO":
                    return await DiaFot_ProcessFoto(state, isImage, mediaUrl, msg);
                case "DIA_FOT_2_DESC":
                    return await DiaFot_SaveDb(state, msg, phone);

                default:
                    state.CurrentState = "ESPERANDO_PROYECTO";
                    return "Lo siento, hubo un error de flujo. Comencemos de nuevo. Por favor, ingresa el número Proyecto 👇";
            }
        }

        // ================= AUTH & MENUS =================

        private async Task<string> Auth_ProcessId(ConversationState state, string message)
        {
            if (int.TryParse(message, out int projectId))
            {
                var project = await _projectService.findProject(projectId);
                if (project != null)
                {
                    state.CurrentProjectId = projectId;
                    var projectType = project.GetType();
                    state.Supervisor = (projectType.GetProperty("supervisor")?.GetValue(project) ?? "").ToString();
                    state.Ot = (projectType.GetProperty("Number")?.GetValue(project) ?? "").ToString();
                    state.Descripcorta = (projectType.GetProperty("ProjectName")?.GetValue(project) ?? "").ToString();
                    state.CurrentProject = projectId.ToString();
                    state.CurrentState = "MENU_PRINCIPAL";

                    return $"*Proyecto válido.*\n" +
                           $"Supervisor: {state.Supervisor}\n" +
                           $"OT/Proyecto: {state.Ot}\n" +
                           $"Desc. Corta: {state.Descripcorta}\n\n" +
                           GetMainMenuOptions();
                }
                return "Proyecto Inválido. *🔴 NO HAY UN PROYECTO SELECCIONADO. *Soy BPI*, tu asistente virtual 🤖 *HCO*. Por favor, teclee ID Proyecto 👇VÁLIDO";
            }
            return "Por favor, ingresa solo números para el ID de proyecto.";
        }

        private string GetMainMenuOptions() =>
            "¿Qué reporte desea capturar?\n" +
            "1. Reporte de campo de PMO\n" +
            "2. Reporte diario de control de obra\n" +
            "3. Cambiar de Proyecto";

        private async Task<string> MainMenu_Process(ConversationState state, string message)
        {
            if (message == "1") {
                state.CurrentState = "MENU_PMO";
                return GetMenuPmoOptions();
            }
            if (message == "2") {
                state.CurrentState = "DIARIO_INIT_FECHA";
                state.TempData.Clear(); // Limpiar temp data
                return "Iniciando Reporte Diario.\nPor favor captura la *Fecha del Reporte* (ej. 25/12/2026 o YYYY-MM-DD):";
            }
            if (message == "3") {
                state.CurrentState = "ESPERANDO_PROYECTO";
                return "Hola, ¡un gusto saludarte! *Soy BPI*, tu asistente virtual 🤖 *HCO*. Estoy para Apoyarlo, teclee ID Proyecto 👇";
            }
            return "Opción inválida, por favor inténtalo nuevamente.\n\n" + GetMainMenuOptions();
        }

        // ================= MENÚ PMO =================

        private string GetMenuPmoOptions() =>
            "Captura de Reporte de Campo PMO\n¿Qué desea capturar?\n" +
            "1. Captura de Actividades\n" +
            "2. Registro de Equipos Críticos IP\n" +
            "3. Registro de Afectaciones\n" +
            "4. Registro de Cambios\n" +
            "5. Regresar";

        private async Task<string> PmoMenu_Process(ConversationState state, string message)
        {
            state.TempData.Clear(); // Limpiar variables en cada flujo nuevo
            switch (message)
            {
                case "1": state.CurrentState = "PMO_ACT_1_ID"; return "Captura de actividades.\nPor favor ingresa el *Id de Tarea del Cronograma*:";
                case "2": state.CurrentState = "PMO_EQ_1_TIPO"; return "Registro de Equipos Críticos.\nPor favor ingresa el *Tipo de Equipo* (Proceso, Eléctrico, Mecánico, etc.):";
                case "3": state.CurrentState = "PMO_AF_1_TIPO"; return "Captura de Registro de Afectaciones.\nPor favor ingresa el *Tipo de Afectación*:";
                case "4": state.CurrentState = "PMO_CAM_1_TIPO"; return "Captura de Registro de Cambios.\nPor favor ingresa el *Tipo de Cambio* (Adicional o Extraordinario):";
                case "5": state.CurrentState = "MENU_PRINCIPAL"; return GetMainMenuOptions();
                default: return "Opción inválida, por favor inténtalo nuevamente.\n\n" + GetMenuPmoOptions();
            }
        }

        // ================= INIT DIARIO =================
        private async Task<string> DiarioInit_ProcessFecha(ConversationState state, string msg)
        {
            state.TempData["reportdate"] = msg;
            state.CurrentState = "DIARIO_INIT_INICIO";
            return "Por favor captura el *Horario de Inicio* (formato HH:MM):";
        }
        private async Task<string> DiarioInit_ProcessInicio(ConversationState state, string msg)
        {
            if (!TimeSpan.TryParse(msg, out _)) return "Formato de hora inválido. Intenta con HH:MM (ej. 08:30).";
            state.TempData["start"] = msg;
            state.CurrentState = "DIARIO_INIT_FIN";
            return "Por favor captura el *Horario de Fin* (formato HH:MM):";
        }
        private async Task<string> DiarioInit_ProcessFin(ConversationState state, string msg)
        {
            if (!TimeSpan.TryParse(msg, out _)) return "Formato de hora inválido. Intenta con HH:MM (ej. 17:30).";
            state.TempData["end"] = msg;
            state.CurrentState = "MENU_DIARIO";
            return GetMenuDiarioOptions();
        }

        // ================= MENÚ DIARIO =================

        private string GetMenuDiarioOptions() =>
            "Captura de Reporte Diario de Control de Obra\n¿Qué desea capturar?\n" +
            "1. Captura de Avances del Cronograma\n" +
            "2. Captura de Volumen de Obra\n" +
            "3. Captura de Afectaciones\n" +
            "4. Registro de Recursos (equipos y personal)\n" +
            "5. Registro Fotográfico\n" +
            "6. Regresar";

        private async Task<string> DiarioMenu_Process(ConversationState state, string message)
        {
            // Conservar temporales de INIT: reportdate, start, end. Borrar el resto
            var reportDate = state.TempData.GetValueOrDefault("reportdate");
            var start = state.TempData.GetValueOrDefault("start");
            var end = state.TempData.GetValueOrDefault("end");
            state.TempData.Clear();
            if (reportDate != null) state.TempData["reportdate"] = reportDate;
            if (start != null) state.TempData["start"] = start;
            if (end != null) state.TempData["end"] = end;

            switch (message)
            {
                case "1": state.CurrentState = "DIA_ACT_1_ID"; return "Captura de Actividades.\nPor favor ingresa el *Id de Tarea del Cronograma*:";
                case "2": state.CurrentState = "DIA_VOL_1_PARTIDA"; return "Captura de Volúmenes de Obra.\nPor favor ingresa el *Número de Partida Catálogo de Conceptos*:";
                case "3": state.CurrentState = "DIA_AF_1_TIPO"; return "Registro de Afectaciones.\nPor favor ingresa el *Tipo de Afectación* (catálogo):";
                case "4": state.CurrentState = "DIA_REC_1_TIPO"; return "Registro de Recursos.\n¿Qué tipo de recurso es?\n1. Categoría de Personal\n2. Equipo";
                case "5": state.CurrentState = "DIA_FOT_1_FOTO"; return "Registro Fotográfico.\nPor favor, envía la *Fotografía*:";
                case "6": state.CurrentState = "MENU_PRINCIPAL"; return GetMainMenuOptions();
                default: return "Opción inválida, por favor inténtalo nuevamente.\n\n" + GetMenuDiarioOptions();
            }
        }

        // ================= HELPERS FLUJOS =================

        private async Task<string> Step_SaveTextAndGo(ConversationState state, string msg, string key, string nextState, string nextPrompt)
        {
            state.TempData[key] = msg;
            state.CurrentState = nextState;
            return nextPrompt;
        }

        private async Task<string> Step_SaveDecimalAndGo(ConversationState state, string msg, string key, string nextState, string nextPrompt)
        {
            if (!decimal.TryParse(msg, out _)) return "Por favor ingresa un número válido.";
            state.TempData[key] = msg;
            state.CurrentState = nextState;
            return nextPrompt;
        }

        private async Task<string> Step_SaveTimeAndGo(ConversationState state, string msg, string key, string nextState, string nextPrompt)
        {
            if (!TimeSpan.TryParse(msg, out _)) return "Formato de hora inválido. Intenta con HH:MM.";
            state.TempData[key] = msg;
            state.CurrentState = nextState;
            return nextPrompt;
        }

        // ================= MANEJADORES FINALES PMO =================

        private async Task<string> PmoAct_SavePhotoAndDb(ConversationState state, bool isImage, string mediaUrl, string phone)
        {
            if (!isImage || string.IsNullOrEmpty(mediaUrl)) return "Por favor envía una FOTO válida.";
            state.TempData["imageazure"] = mediaUrl;
            state.TempData["orden"] = "1"; // Primera foto

            await SaveLogbookEntry(state, 1, 1, phone);
            state.CurrentState = "PMO_ACT_5_MAS_FOTOS";
            return "Registro de actividad y fotografía guardados. ¿Deseas subir otra fotografía para esta misma tarea? (responde 'si' o 'no')";
        }

        private async Task<string> PmoAct_ProcessMasFotos(ConversationState state, string msg)
        {
            msg = msg.ToLower();
            if (msg == "si" || msg == "sí") {
                state.CurrentState = "PMO_ACT_6_FOTO_EXTRA";
                return "Por favor envía la *siguiente Evidencia Fotográfica*:";
            }
            if (msg == "no") {
                state.CurrentState = "MENU_PMO";
                return GetMenuPmoOptions();
            }
            return "Opción inválida. Responde 'si' o 'no'.";
        }

        private async Task<string> PmoAct_SaveExtraPhoto(ConversationState state, bool isImage, string mediaUrl, string phone)
        {
            if (!isImage || string.IsNullOrEmpty(mediaUrl)) return "Por favor envía una FOTO válida.";
            state.TempData["imageazure"] = mediaUrl;
            int ordenActual = int.Parse(state.TempData.GetValueOrDefault("orden", "1"));
            state.TempData["orden"] = (ordenActual + 1).ToString();

            await SaveLogbookEntry(state, 1, 1, phone);
            state.CurrentState = "PMO_ACT_5_MAS_FOTOS";
            return "Fotografía adicional guardada. ¿Deseas subir otra fotografía? ('si' o 'no')";
        }

        private async Task<string> PmoEq_SavePhotoAndDb(ConversationState state, bool isImage, string mediaUrl, string phone)
        {
            if (!isImage || string.IsNullOrEmpty(mediaUrl)) return "Por favor envía una FOTO válida.";
            state.TempData["imageazure"] = mediaUrl;
            state.TempData["orden"] = "1";

            await SaveLogbookEntry(state, 1, 2, phone);
            state.CurrentState = "PMO_EQ_6_MAS_FOTOS";
            return "Registro de equipo y fotografía guardados. ¿Deseas subir otra fotografía? (responde 'si' o 'no')";
        }

        private async Task<string> PmoEq_ProcessMasFotos(ConversationState state, string msg)
        {
            msg = msg.ToLower();
            if (msg == "si" || msg == "sí") {
                state.CurrentState = "PMO_EQ_7_FOTO_EXTRA";
                return "Por favor envía la *siguiente Evidencia Fotográfica*:";
            }
            if (msg == "no") {
                state.CurrentState = "MENU_PMO";
                return GetMenuPmoOptions();
            }
            return "Opción inválida. Responde 'si' o 'no'.";
        }

        private async Task<string> PmoEq_SaveExtraPhoto(ConversationState state, bool isImage, string mediaUrl, string phone)
        {
            if (!isImage || string.IsNullOrEmpty(mediaUrl)) return "Por favor envía una FOTO válida.";
            state.TempData["imageazure"] = mediaUrl;
            int ordenActual = int.Parse(state.TempData.GetValueOrDefault("orden", "1"));
            state.TempData["orden"] = (ordenActual + 1).ToString();

            await SaveLogbookEntry(state, 1, 2, phone);
            state.CurrentState = "PMO_EQ_6_MAS_FOTOS";
            return "Fotografía adicional guardada. ¿Deseas subir otra fotografía? ('si' o 'no')";
        }

        private async Task<string> PmoAf_SaveDb(ConversationState state, string msg, string phone)
        {
            state.TempData["task_reference"] = msg;
            await SaveLogbookEntry(state, 1, 3, phone);
            state.CurrentState = "MENU_PMO";
            return "Registro de Afectación PMO guardado exitosamente.\n\n" + GetMenuPmoOptions();
        }

        private async Task<string> PmoCam_SaveDb(ConversationState state, string msg, string phone)
        {
            state.TempData["description"] = msg;
            await SaveLogbookEntry(state, 1, 4, phone);
            state.CurrentState = "MENU_PMO";
            return "Registro de Cambio PMO guardado exitosamente.\n\n" + GetMenuPmoOptions();
        }

        // ================= MANEJADORES FINALES DIARIO =================

        private async Task<string> DiaAct_SaveDb(ConversationState state, string msg, string phone)
        {
            state.TempData["notes"] = msg;
            await SaveLogbookEntry(state, 2, 1, phone);
            state.CurrentState = "MENU_DIARIO";
            return "Registro de Actividad Diario guardado exitosamente.\n\n" + GetMenuDiarioOptions();
        }

        private async Task<string> DiaVol_SaveDb(ConversationState state, string msg, string phone)
        {
            if (!decimal.TryParse(msg, out _)) return "Por favor ingresa un volumen válido numérico.";
            state.TempData["quantity"] = msg;
            await SaveLogbookEntry(state, 2, 2, phone);
            state.CurrentState = "MENU_DIARIO";
            return "Registro de Volumen Diario guardado exitosamente.\n\n" + GetMenuDiarioOptions();
        }

        private async Task<string> DiaAf_SaveDb(ConversationState state, string msg, string phone)
        {
            state.TempData["notes"] = msg; // Observaciones/Duracion
            await SaveLogbookEntry(state, 2, 3, phone);
            state.CurrentState = "MENU_DIARIO";
            return "Registro de Afectación Diario guardado exitosamente.\n\n" + GetMenuDiarioOptions();
        }

        private async Task<string> DiaRec_ProcessTipo(ConversationState state, string msg)
        {
            if (msg == "1" || msg == "2") {
                state.TempData["typenote"] = msg; // 1=Personal, 2=Equipo
                state.CurrentState = "DIA_REC_2_NOMBRE";
                return "Por favor ingresa el *Nombre* del recurso:";
            }
            return "Opción inválida. Responde 1 (Personal) o 2 (Equipo).";
        }

        private async Task<string> DiaRec_ProcessCantidad(ConversationState state, string msg, string phone)
        {
            if (!decimal.TryParse(msg, out _)) return "Por favor ingresa una cantidad numérica válida.";
            state.TempData["quantity"] = msg;
            
            if (state.TempData.GetValueOrDefault("typenote") == "2") {
                state.CurrentState = "DIA_REC_4_COMBUSTIBLE";
                return "Has registrado un Equipo. Por favor ingresa la *Cantidad de Combustible* (numérica):";
            }
            
            // Si es Personal, guarda y termina
            await SaveLogbookEntry(state, 2, 5, phone);
            state.CurrentState = "MENU_DIARIO";
            return "Registro de Personal guardado exitosamente.\n\n" + GetMenuDiarioOptions();
        }

        private async Task<string> DiaRec_ProcessCombustible(ConversationState state, string msg, string phone)
        {
            if (!int.TryParse(msg, out _)) return "Por favor ingresa un número entero válido para combustible.";
            state.TempData["fuel_quantity"] = msg;
            await SaveLogbookEntry(state, 2, 5, phone);
            state.CurrentState = "MENU_DIARIO";
            return "Registro de Equipo guardado exitosamente.\n\n" + GetMenuDiarioOptions();
        }

        private async Task<string> DiaFot_ProcessFoto(ConversationState state, bool isImage, string mediaUrl, string msg)
        {
            if (!isImage || string.IsNullOrEmpty(mediaUrl)) return "Por favor envía una FOTO válida.";
            state.TempData["imageazure"] = mediaUrl;
            state.CurrentState = "DIA_FOT_2_DESC";
            return "Por favor, ingresa una *Descripción* o comentario para esta foto:";
        }

        private async Task<string> DiaFot_SaveDb(ConversationState state, string msg, string phone)
        {
            state.TempData["description"] = msg;
            await SaveLogbookEntry(state, 2, 6, phone);
            state.CurrentState = "MENU_DIARIO";
            return "Registro Fotográfico Diario guardado exitosamente.\n\n" + GetMenuDiarioOptions();
        }


        // ================= DB SAVER =================

        private async Task SaveLogbookEntry(ConversationState state, int idReporte, int reportSectionId, string phone)
        {
            try
            {
                var logbook = new Logbook
                {
                    IdProject = state.CurrentProjectId,
                    IdReporte = idReporte,
                    ReportSectionId = reportSectionId,
                    Date = DateTimeHelper.GetCurrentMexicoDateTime(),
                    Timexnote = DateTime.Now.TimeOfDay,
                    Supervisor = state.Supervisor,
                    Phone = phone.Length >= 10 ? phone.Substring(phone.Length - 10) : phone,
                    Active = 1
                };

                var d = state.TempData;

                if (d.TryGetValue("reportdate", out var rd)) logbook.ReportDate = rd;
                if (d.TryGetValue("start", out var st) && TimeSpan.TryParse(st, out var tSt)) logbook.Start = tSt;
                if (d.TryGetValue("end", out var en) && TimeSpan.TryParse(en, out var tEn)) logbook.End = tEn;
                if (d.TryGetValue("task_reference", out var tr)) logbook.TaskReference = tr;
                if (d.TryGetValue("quantity", out var qt) && decimal.TryParse(qt, out var dQt)) logbook.Quantity = dQt;
                if (d.TryGetValue("description", out var ds)) logbook.Description = ds;
                if (d.TryGetValue("typenote", out var tn)) logbook.TypeNote = tn;
                if (d.TryGetValue("notes", out var no)) logbook.Notes = no;
                if (d.TryGetValue("fuel_quantity", out var fq) && int.TryParse(fq, out var iFq)) logbook.FuelQuantity = iFq;
                if (d.TryGetValue("imageazure", out var img)) logbook.ImageAzure = img;
                if (d.TryGetValue("orden", out var ord) && int.TryParse(ord, out var iOrd)) logbook.Orden = iOrd;

                await _logbookService.Save(logbook);
                _logger.LogInformation($"Logbook guardado: Reporte {idReporte}, Seccion {reportSectionId}, Proy {state.CurrentProjectId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar Logbook");
                throw;
            }
        }

        public static class DateTimeHelper
        {
            private static readonly TimeZoneInfo _zonaHorariaMexico =
                TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)") ?? TimeZoneInfo.Local;

            public static DateTime GetCurrentMexicoDateTime() =>
                TimeZoneInfo.ConvertTime(DateTime.Now, _zonaHorariaMexico);
        }
    }

    public interface IConversationManagerService
    {
        Task<string> ProcessMessage(string from, string body, string numMedia, string mediaUrl,
            string mediaContentType, string imageDescription = null);
    }
}
