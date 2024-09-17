// Função para Carregar os Dados da Tabela de Logs Emar
function CarregaLogsEmar(){
	// Trata o Campo de Periodo
	var periodo = ($('#periodoCockpit').val()).split(" - ");

	var periodoInicio = periodo[0].split("/");
	periodoInicio = periodoInicio[2] + "-" + periodoInicio[1] + "-" + periodoInicio[0] + "T00:00:00";

	var periodoFim = periodo[1].split("/");
	periodoFim = periodoFim[2] + "-" + periodoFim[1] + "-" + periodoFim[0] + "T23:59:59";

	// Verifica se já existe a datatable e caso sim, destroi para construir de novo
	if($.fn.DataTable.isDataTable("#tabelaLogsEmar")){
		$("#tabelaLogsEmar").DataTable().destroy();
	}

	// Cria a DataTable
	var tabela = $("#tabelaLogsEmar").DataTable({
		language: {
			lengthMenu: '<span class="nomecampos">Visualizando</span> _MENU_ <span class="nomecampos">registros por página</span> <span style="padding-right: 10px;"></span>',
			zeroRecords: '<span class="nomecampos">Desculpe - Não há Registros</span>',
			info: '<span class="nomecampos">Exibindo Página _PAGE_ de _PAGES_ </span>',
			infoEmpty: '<span class="nomecampos">Não há Registros</span>',
			infoFiltered: '<span class="nomecampos">(filtrando de _MAX_ total de registros)</span>',
			search: '<span class="nomecampos">Buscar</span>',
			paginate: {
				first: 'Primeiro',
				last: 'Último',
				next: 'Próximo',
				previous: 'Anterior'
			},
		},
		ajax: {
			url: apiUrl.logs_emar,
			type: "POST",
			contentType: "application/json",
			data: (d) => {
				return JSON.stringify(
					{
						...d,
						"periodoInicio": periodoInicio,
						"periodoFim": periodoFim
					}
				)
			},
			dataSrc: function(retorno) {
				return retorno.data;
			}
		},
		ordering: false,
		responsive: true,
		lengthChange: true,
		lengthMenu: [[5, 10, 15, 20], [5, 10, 15, 20]],
		autoWidth: false,
		paging: true,
		serverSide: true,
		buttons: [
			{
				extend: 'csv',
				text: '<i class="fas fa-file-csv"></i> CSV',
				className: 'btn btn-secondary rounded-0',
				footer: false
			}, {
				extend: 'excel',
				text: '<i class="fas fa-file-excel"></i> Excel',
				className: 'btn btn-success rounded-0',
				footer: false
			}, {
				extend: 'pdf',
				text: '<i class="fas fa-file-pdf"></i> PDF',
				className: 'btn btn-danger rounded-0',
				footer: false,
				exportOptions: {
					pageSize: 'LEGAL'
				},
				customize: function (doc) {
					// Define o Tamanho da Fonte da Página
					doc.defaultStyle.fontSize = 8;
					
					// Define a Margem da Página
					doc.pageMargins = [0, 0, 0, 0]; // [left, top, right, bottom]
					
					// Define a Orientação da Página
					doc.pageOrientation = 'landscape';
					
					// Centralize o conteúdo horizontal e verticalmente
					doc.content[1].alignment = 'center';
					doc.content[1].layout = {
						hLineWidth: function(i, node) { return 0; },
						vLineWidth: function(i, node) { return 0; },
						paddingLeft: function(i, node) { return 0; },
						paddingRight: function(i, node) { return 0; },
						paddingTop: function(i, node) { return 0; },
						paddingBottom: function(i, node) { return 0; },
						defaultBorder: false
					};
					
					// Determina a não quebra de pagina na linha
					doc.content[1].table.dontBreakRows = true;
				}
			}
		],
		columns: [
			{
				data: 'Log',
				title: 'Log'
			}, {
				data: 'DataHorario',
				title: 'Data',
				width: '150px',
				type: 'date'
			}
		],
		rowCallback: (row, data)=>{
			$(row).css("cursor", "pointer");
			$(row).find("td").addClass("text-center").css("font-size", "12px");;
		},
		initComplete: ()=>{
			// Adiciona a Linha dos botôes
			$("#tabelaLogsEmar_wrapper").prepend($('<div>', {
				class: "row mb-2"
			}).append($('<div>', {
				class: "col-12"
			})));

			// Adiciona os botões dentro da linha
			tabela.buttons().container().appendTo('#tabelaLogsEmar_wrapper .row:eq(0) .col-12:eq(0)');
		}
	});				
}

// Função para Carregar os Dados da Tabela de Logs Emar Processamento
function CarregaLogsEmarProcessamento(){
	// Trata o Campo de Periodo
	var periodo = ($('#periodoCockpit').val()).split(" - ");

	var periodoInicio = periodo[0].split("/");
	periodoInicio = periodoInicio[2] + "-" + periodoInicio[1] + "-" + periodoInicio[0] + "T00:00:00";

	var periodoFim = periodo[1].split("/");
	periodoFim = periodoFim[2] + "-" + periodoFim[1] + "-" + periodoFim[0] + "T23:59:59";

	// Verifica se já existe a datatable e caso sim, destroi para construir de novo
	if($.fn.DataTable.isDataTable("#tabelaLogsEmarProcessamento")){
		$("#tabelaLogsEmarProcessamento").DataTable().destroy();
	}

	// Cria a DataTable
	var tabela = $("#tabelaLogsEmarProcessamento").DataTable({
		language: {
			lengthMenu: '<span class="nomecampos">Visualizando</span> _MENU_ <span class="nomecampos">registros por página</span> <span style="padding-right: 10px;"></span>',
			zeroRecords: '<span class="nomecampos">Desculpe - Não há Registros</span>',
			info: '<span class="nomecampos">Exibindo Página _PAGE_ de _PAGES_ </span>',
			infoEmpty: '<span class="nomecampos">Não há Registros</span>',
			infoFiltered: '<span class="nomecampos">(filtrando de _MAX_ total de registros)</span>',
			search: '<span class="nomecampos">Buscar</span>',
			paginate: {
				first: 'Primeiro',
				last: 'Último',
				next: 'Próximo',
				previous: 'Anterior'
			},
		},
		ajax: {
			url: apiUrl.logs_emar_processamentos,
			type: "POST",
			contentType: "application/json",
			data: (d) => {
				return JSON.stringify(
					{
						...d,
						"periodoInicio": periodoInicio,
						"periodoFim": periodoFim
					}
				)
			},
			dataSrc: function(retorno) {
				return retorno.data;
			}
		},
		ordering: false,
		responsive: true,
		lengthChange: true,
		lengthMenu: [[5, 10, 15, 20], [5, 10, 15, 20]],
		autoWidth: false,
		paging: true,
		serverSide: true,
		buttons: [
			{
				extend: 'csv',
				text: '<i class="fas fa-file-csv"></i> CSV',
				className: 'btn btn-secondary rounded-0',
				footer: false
			}, {
				extend: 'excel',
				text: '<i class="fas fa-file-excel"></i> Excel',
				className: 'btn btn-success rounded-0',
				footer: false
			}, {
				extend: 'pdf',
				text: '<i class="fas fa-file-pdf"></i> PDF',
				className: 'btn btn-danger rounded-0',
				footer: false,
				exportOptions: {
					pageSize: 'LEGAL'
				},
				customize: function (doc) {
					// Define o Tamanho da Fonte da Página
					doc.defaultStyle.fontSize = 8;
					
					// Define a Margem da Página
					doc.pageMargins = [0, 0, 0, 0]; // [left, top, right, bottom]
					
					// Define a Orientação da Página
					doc.pageOrientation = 'landscape';
					
					// Centralize o conteúdo horizontal e verticalmente
					doc.content[1].alignment = 'center';
					doc.content[1].layout = {
						hLineWidth: function(i, node) { return 0; },
						vLineWidth: function(i, node) { return 0; },
						paddingLeft: function(i, node) { return 0; },
						paddingRight: function(i, node) { return 0; },
						paddingTop: function(i, node) { return 0; },
						paddingBottom: function(i, node) { return 0; },
						defaultBorder: false
					};
					
					// Determina a não quebra de pagina na linha
					doc.content[1].table.dontBreakRows = true;
				}
			}
		],
		columns: [
			{
				data: 'Email',
				title: 'Email'
			}, {
				data: 'Anexo',
				title: 'Anexo',
				render: (data, type, row, meta)=>{
					// Pega os Anexos e separa por virgula
					var anexos = data.split(",");

					// Verifica se tem anexos e caso tenha
					// realiza a renderização dos anexos
					if(anexos.length > 0){
						// Instância o html que vai gerar a timeline
						// de anexos
						var html = $("<div>", { class: "timeline m-0" });

						// Realiza um laço de repetição para montar a
						// estrutura da timeline
						for(var i = 0; i < anexos.length; i++){
							var registro = anexos[i];
	
							if(registro.length > 0){
								// Verifica a Extensão
								var extensao = (registro.split(".")[1]).toLowerCase();

								// Seleciona o Icone e a cor do anexo
								switch(extensao){
									// Excel
									case "xls": case "xlsm": case "xlsb": case "xltx": case "xlsx":
										var icone = 'far fa-file-excel'
										var cor = 'bg-success';
										break;
									// PDF
									case "pdf":
										var icone = 'far fa-file-pdf'
										var cor = 'bg-danger';
										break;
									// Arquivo Texto
									case "txt":
										var icone = 'far fa-file-alt'
										var cor = 'bg-light';
										break;
									// Word
									case "docx": case "docm": case "dotx": case "dotm":
										var icone = 'far fa-file-word'
										var cor = 'bg-primary';
										break;
									// Imagem
									case "jpg": case "jpeg": case "bmp": case "png":
										var icone = 'far fa-file-image'
										var cor = 'bg-indigo';
										break;
									// default
									default:
										var icone = 'fas fa-question'
										var cor = 'bg-warning';
										break;
								}

								// Alimenta o Html Principal
								html.append(
									$("<div>", { class: "m-0 mb-2" }).append(
										$("<i>", { class: icone + " " + cor })
									).append(
										$("<div>", { class: "timeline-item" }).append(
											$("<div>", { class: "timeline-header p-2 border-0 rounded" + " " + cor }).html(
												registro.trim()
											)
										)
									)
								)
							}
						}

						// Retorna o html na coluna
						return html.prop('outerHTML');
					}
				},
				width: '30%'
			}, {
				data: 'Status',
				title: 'Status',
				render: (data, type, row, meta)=>{
					switch(data){
						case "PR":
							var html = '<span style="font-size: 12px;" class="badge badge-primary"><i class="fas fa-check-square ml-1 mr-2"></i>Processado</span>';
							break;
						case "SP":
							var html = '<span style="font-size: 12px;" class="badge badge-warning"><i class="fas fa-exclamation-triangle ml-1 mr-2"></i>Spam</span>';
							break;
						case "NP":
							var html = '<span style="font-size: 12px;" class="badge badge-danger"><i class="fas fa-times-circle ml-1 mr-2"></i>Não Processado</span>';
							break;
					}
					return html;
				}
			}, {
				data: 'Log',
				title: 'Log',
				width: '30%'
			}, {
				data: 'DataHorario',
				title: 'Data',
				width: '150px',
				type: 'date'
			}
		],
		rowCallback: (row, data)=>{
			$(row).css("cursor", "pointer");
			$(row).find("td").addClass("text-center").addClass("align-middle").css("font-size", "12px");;
		},
		initComplete: ()=>{
			// Adiciona a Linha dos botôes
			$("#tabelaLogsEmarProcessamento_wrapper").prepend($('<div>', {
				class: "row mb-2"
			}).append($('<div>', {
				class: "col-12"
			})));

			// Adiciona os botões dentro da linha
			tabela.buttons().container().appendTo('#tabelaLogsEmarProcessamento_wrapper .row:eq(0) .col-12:eq(0)');
		}
	});				
}

// Função para Carregar os Dados da Tabela de Logs WebSocket
function CarregaLogsWebSocket(){
	// Trata o Campo de Periodo
	var periodo = ($('#periodoCockpit').val()).split(" - ");

	var periodoInicio = periodo[0].split("/");
	periodoInicio = periodoInicio[2] + "-" + periodoInicio[1] + "-" + periodoInicio[0] + "T00:00:00";

	var periodoFim = periodo[1].split("/");
	periodoFim = periodoFim[2] + "-" + periodoFim[1] + "-" + periodoFim[0] + "T23:59:59";

	// Verifica se já existe a datatable e caso sim, destroi para construir de novo
	if($.fn.DataTable.isDataTable("#tabelaLogsWebSocket")){
		$("#tabelaLogsWebSocket").DataTable().destroy();
	}

	// Cria a DataTable
	var tabela = $("#tabelaLogsWebSocket").DataTable({
		language: {
			lengthMenu: '<span class="nomecampos">Visualizando</span> _MENU_ <span class="nomecampos">registros por página</span> <span style="padding-right: 10px;"></span>',
			zeroRecords: '<span class="nomecampos">Desculpe - Não há Registros</span>',
			info: '<span class="nomecampos">Exibindo Página _PAGE_ de _PAGES_ </span>',
			infoEmpty: '<span class="nomecampos">Não há Registros</span>',
			infoFiltered: '<span class="nomecampos">(filtrando de _MAX_ total de registros)</span>',
			search: '<span class="nomecampos">Buscar</span>',
			paginate: {
				first: 'Primeiro',
				last: 'Último',
				next: 'Próximo',
				previous: 'Anterior'
			},
		},
		ajax: {
			url: apiUrl.logs_websocket,
			type: "POST",
			contentType: "application/json",
			data: (d) => {
				return JSON.stringify(
					{
						...d,
						"periodoInicio": periodoInicio,
						"periodoFim": periodoFim
					}
				)
			},
			dataSrc: function(retorno) {
				return retorno.data;
			}
		},
		ordering: false,
		responsive: true,
		lengthChange: true,
		lengthMenu: [[5, 10, 15, 20], [5, 10, 15, 20]],
		autoWidth: false,
		paging: true,
		serverSide: true,
		buttons: [
			{
				extend: 'csv',
				text: '<i class="fas fa-file-csv"></i> CSV',
				className: 'btn btn-secondary rounded-0',
				footer: false
			}, {
				extend: 'excel',
				text: '<i class="fas fa-file-excel"></i> Excel',
				className: 'btn btn-success rounded-0',
				footer: false
			}, {
				extend: 'pdf',
				text: '<i class="fas fa-file-pdf"></i> PDF',
				className: 'btn btn-danger rounded-0',
				footer: false,
				exportOptions: {
					pageSize: 'LEGAL'
				},
				customize: function (doc) {
					// Define o Tamanho da Fonte da Página
					doc.defaultStyle.fontSize = 8;
					
					// Define a Margem da Página
					doc.pageMargins = [0, 0, 0, 0]; // [left, top, right, bottom]
					
					// Define a Orientação da Página
					doc.pageOrientation = 'landscape';
					
					// Centralize o conteúdo horizontal e verticalmente
					doc.content[1].alignment = 'center';
					doc.content[1].layout = {
						hLineWidth: function(i, node) { return 0; },
						vLineWidth: function(i, node) { return 0; },
						paddingLeft: function(i, node) { return 0; },
						paddingRight: function(i, node) { return 0; },
						paddingTop: function(i, node) { return 0; },
						paddingBottom: function(i, node) { return 0; },
						defaultBorder: false
					};
					
					// Determina a não quebra de pagina na linha
					doc.content[1].table.dontBreakRows = true;
				}
			}
		],
		columns: [
			{
				data: 'Log',
				title: 'Log'
			}, {
				data: 'DataHorario',
				title: 'Data',
				width: '150px',
				type: 'date'
			}
		],
		rowCallback: (row, data)=>{
			$(row).css("cursor", "pointer");
			$(row).find("td").addClass("text-center").css("font-size", "12px");;
		},
		initComplete: ()=>{
			// Adiciona a Linha dos botôes
			$("#tabelaLogsWebSocket_wrapper").prepend($('<div>', {
				class: "row mb-2"
			}).append($('<div>', {
				class: "col-12"
			})));

			// Adiciona os botões dentro da linha
			tabela.buttons().container().appendTo('#tabelaLogsWebSocket_wrapper .row:eq(0) .col-12:eq(0)');
		}
	});				
}

// Função para Carregar os Cards no Dashboard
function CarregaCards(){
	// Trata o Campo de Periodo
	var periodo = ($('#periodoCockpit').val()).split(" - ");

	var periodoInicio = periodo[0].split("/");
	periodoInicio = periodoInicio[2] + "-" + periodoInicio[1] + "-" + periodoInicio[0] + "T00:00:00";

	var periodoFim = periodo[1].split("/");
	periodoFim = periodoFim[2] + "-" + periodoFim[1] + "-" + periodoFim[0] + "T23:59:59";

	// Realiza a chamada do ajax
	$.ajax({
		url: apiUrl.cards,
		type: "POST",
		contentType: "application/json",
		data: JSON.stringify(
			{
				"periodoInicio": periodoInicio,
				"periodoFim": periodoFim
			}
		),
		success: function(retorno) {
			// Realiza a renderização dos cards
			for(var i = 0; i < retorno.Requisicao.Retorno._card.length; i++){
				// Define a o registro
				var registro = retorno.Requisicao.Retorno._card[i];

				// Atualiza o Card
				$("span[id='total-" + registro.Tipo + "']").html(registro.Valor);
			}
		}
	});
}

// Função para Carregar os Dados do Servidor WebSocket
function CarregaDadosWebSocket(){
	// Realiza a chamada do ajax
	$.ajax({
		url: apiUrl.dados_servidor_websocket,
		type: "POST",
		contentType: "application/json",
		success: function(retorno) {
			// Dados do WebSocket
			var _infoWebSocket = retorno.Requisicao.Retorno._infoWebSocket;

			// Caso o Host for diferente de Null faz as tratativas
			if(_infoWebSocket.Host != null){
				// Valida se o Certificado está valido
				var certificadoValido = moment(_infoWebSocket.SSL.FimValidade).diff(moment(), 'days') > 0;
	
				// Atualiza os dados do Servidor WebSocket
				$("#websocket-host").html(
					$("<span>", { class: "badge badge-light" }).html(
						_infoWebSocket.Host
					)
				);
	
				$("#websocket-status").html(
					$("<span>", { class: "badge badge-" + (_infoWebSocket.ServidorOnline ? "success" : "danger") }).html(
						(_infoWebSocket.ServidorOnline ? "Online" : "Offline")
					)
				);
	
				$("#websocket-data-inicio").html(
					$("<span>", { class: "badge badge-primary" }).html(
						moment(_infoWebSocket.DataHorarioInicio).format("DD/MM/YYYY - HH:mm:ss")
					)
				);
	
				$("#websocket-status-ssl").html(
					$("<span>", { class: "badge badge-" + (certificadoValido ? "success" : "danger") }).html(
						(certificadoValido ? "Ok" : "Expirado")
					)
				);

				$("#websocket-hash-ssl").html(
					$("<span>", { class: "badge badge-light" }).html(
						_infoWebSocket.SSL.Hash
					)
				);

				$("#websocket-validade-ssl").html(
					$("<span>", { class: "badge badge-primary" }).html(
						moment(_infoWebSocket.SSL.InicioValidade).format("DD/MM/YYYY - HH:mm:ss") +
						" a " +
						moment(_infoWebSocket.SSL.FimValidade).format("DD/MM/YYYY - HH:mm:ss")
					)
				);
			} else {
				// Atualiza os dados do Servidor WebSocket
				$("#websocket-host").html(
					$("<span>", { class: "badge badge-warning" }).html(
						$("<i>", { class: "fas fa-exclamation-triangle" })
					)
				);
	
				$("#websocket-status").html(
					$("<span>", { class: "badge badge-warning" }).html(
						$("<i>", { class: "fas fa-exclamation-triangle" })
					)
				);
	
				$("#websocket-data-inicio").html(
					$("<span>", { class: "badge badge-warning" }).html(
						$("<i>", { class: "fas fa-exclamation-triangle" })
					)
				);
	
				$("#websocket-status-ssl").html(
					$("<span>", { class: "badge badge-warning" }).html(
						$("<i>", { class: "fas fa-exclamation-triangle" })
					)
				);

				$("#websocket-hash-ssl").html(
					$("<span>", { class: "badge badge-warning" }).html(
						$("<i>", { class: "fas fa-exclamation-triangle" })
					)
				);

				$("#websocket-validade-ssl").html(
					$("<span>", { class: "badge badge-warning" }).html(
						$("<i>", { class: "fas fa-exclamation-triangle" })
					)
				);
			}
		}
	});
}

// Função para Carregar os Dados do Ambiente
function CarregaDadosAmbiente(){
	// Realiza a chamada do ajax
	$.ajax({
		url: apiUrl.dados_ambiente,
		type: "POST",
		contentType: "application/json",
		success: function(retorno) {
			// Dados do WebSocket
			var _infoAmbiente = retorno.Requisicao.Retorno._infoAmbiente;

			// Caso o Host for diferente de Null faz as tratativas
			if(_infoAmbiente.Host != null){
				// Valida se o Certificado está valido
				var certificadoValido = moment(_infoAmbiente.SSL.FimValidade).diff(moment(), 'days') > 0;
	
				// Atualiza os dados do Servidor WebSocket
				$("#ambiente-host").html(
					$("<span>", { class: "badge badge-light" }).html(
						_infoAmbiente.Host
					)
				);
	
				$("#ambiente-status-ssl").html(
					$("<span>", { class: "badge badge-" + (certificadoValido ? "success" : "danger") }).html(
						(certificadoValido ? "Ok" : "Expirado")
					)
				);

				$("#ambiente-hash-ssl").html(
					$("<span>", { class: "badge badge-light" }).html(
						_infoAmbiente.SSL.Hash
					)
				);

				$("#ambiente-validade-ssl").html(
					$("<span>", { class: "badge badge-primary" }).html(
						moment(_infoAmbiente.SSL.InicioValidade).format("DD/MM/YYYY - HH:mm:ss") +
						" a " +
						moment(_infoAmbiente.SSL.FimValidade).format("DD/MM/YYYY - HH:mm:ss")
					)
				);
			} else {
				// Atualiza os dados do Servidor WebSocket
				$("#ambiente-host").html(
					$("<span>", { class: "badge badge-warning" }).html(
						$("<i>", { class: "fas fa-exclamation-triangle" })
					)
				);
	
				$("#ambiente-status-ssl").html(
					$("<span>", { class: "badge badge-warning" }).html(
						$("<i>", { class: "fas fa-exclamation-triangle" })
					)
				);

				$("#ambiente-hash-ssl").html(
					$("<span>", { class: "badge badge-warning" }).html(
						$("<i>", { class: "fas fa-exclamation-triangle" })
					)
				);

				$("#ambiente-validade-ssl").html(
					$("<span>", { class: "badge badge-warning" }).html(
						$("<i>", { class: "fas fa-exclamation-triangle" })
					)
				);
			}
		}
	});
}

jQuery(document).ready(function($) {
	// Executa os scripts ao finalizar o carregamento da pagina
	$(document).ready(()=>{
		// Instancia o DateRangePicker
		$('#periodoCockpit').daterangepicker({
			locale: moment.locale("pt-br"),
			startDate: moment().startOf('month').endOf('day')
		});

		// Carrega os Dados do Ambiente
		CarregaDadosAmbiente();

		// Carrega os Dados do WebSocket
		CarregaDadosWebSocket();
	})

    // Executa os scripts ao fechar ou ir para outra pagina
	$(window).on('beforeunload', ()=>{
		localStorage.clear();
	});

	// Atualiza o Cockpit conforme realiza alteração no range de datas
	$(document).on("change", "#periodoCockpit", ()=>{
		// Mensagem de Carregando
		swal.fire({
			icon: 'info',
			text: 'Carregando Dados Cockpit',
			didOpen: () => {
				swal.showLoading()
			},
			timer: 0,
			allowOutsideClick: false
		})

		// Trata o Campo de Periodo
		var periodo = ($('#periodoCockpit').val()).split(" - ");

		var periodoInicio = periodo[0].split("/");
		periodoInicio = periodoInicio[2] + "-" + periodoInicio[1] + "-" + periodoInicio[0] + "T00:00:00";

		var periodoFim = periodo[1].split("/");
		periodoFim = periodoFim[2] + "-" + periodoFim[1] + "-" + periodoFim[0] + "T23:59:59";
		
		// Carrega os Cards
		CarregaCards();

		// Carrega os Logs de Monitoramento do Robô
		CarregaLogsEmar();

		// Carrega os Logs de Processamento do Robô
		CarregaLogsEmarProcessamento();

		// Carrega os Logs do WebSocket
		CarregaLogsWebSocket();

		// Fecha a Mensagem
		swal.close();
	})
})