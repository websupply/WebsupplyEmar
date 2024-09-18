// Url do Ambiente
const baseUrl = `${window.location.protocol}//${window.location.host}/WSEmaR`;

// Metodos da API
const apiUrl = {
    logs_emar: `${baseUrl}${ambiente}/api/Cockpit/logs_emar`,
    logs_emar_processamentos: `${baseUrl}${ambiente}/api/Cockpit/logs_emar_processamentos`,
    logs_websocket: `${baseUrl}${ambiente}/api/Cockpit/logs_websocket`,
    cards: `${baseUrl}${ambiente}/api/Cockpit/cards`,
    dados_servidor_websocket: `${baseUrl}${ambiente}/api/Cockpit/dados_servidor_websocket`,
    dados_ambiente: `${baseUrl}${ambiente}/api/Cockpit/dados_ambiente`
}
