// Url do Ambiente
const baseUrl = `${window.location.protocol}//${window.location.host}`;

// Metodos da API
const apiUrl = {
    logs_emar: `${baseUrl}/api/Cockpit/logs_emar`,
    logs_emar_processamentos: `${baseUrl}/api/Cockpit/logs_emar_processamentos`,
    logs_websocket: `${baseUrl}/api/Cockpit/logs_websocket`,
    cards: `${baseUrl}/api/Cockpit/cards`,
    dados_servidor_websocket: `${baseUrl}/api/Cockpit/dados_servidor_websocket`,
    dados_ambiente: `${baseUrl}/api/Cockpit/dados_ambiente`
}
