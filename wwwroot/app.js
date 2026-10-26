/**
 * Pokédex API - Frontend Integrado ao Backend ASP.NET Core & EF Core SQLite
 * Todas as operações (Listar, Buscar por ID, Criar, Atualizar, Excluir)
 * são enviadas diretamente aos endpoints reais da API.
 */

const API_BASE_URL = "/api/pokemon";

// Estado dos dados carregados da API
let pokemons = [];
let editandoId = null;
let timerFeedback = null;

// Elementos da interface
const form = document.getElementById("pokemon-form");
const formHeading = document.getElementById("form-heading");
const formSubheading = document.getElementById("form-subheading");
const btnSubmit = document.getElementById("btn-submit");
const btnCancel = document.getElementById("btn-cancel");
const listContainer = document.getElementById("pokemon-list");
const emptyState = document.getElementById("empty-state");
const totalCount = document.getElementById("total-count");
const filterInput = document.getElementById("filter-input");
const feedbackMessage = document.getElementById("feedback-message");

// Campos do formulário
const inputId = document.getElementById("pokemon-id");
const inputIdDisplay = document.getElementById("pokemon-id-display");
const groupIdDisplay = document.getElementById("group-id-display");
const inputNome = document.getElementById("pokemon-nome");
const selectTipoPrimario = document.getElementById("pokemon-tipo-primario");
const selectTipoSecundario = document.getElementById("pokemon-tipo-secundario");
const inputHp = document.getElementById("pokemon-hp");
const inputImagem = document.getElementById("pokemon-imagem");
const inputDescricao = document.getElementById("pokemon-descricao");

// Elementos de erro
const errorNome = document.getElementById("error-nome");
const errorTipoPrimario = document.getElementById("error-tipo-primario");
const errorHp = document.getElementById("error-hp");
const errorDescricao = document.getElementById("error-descricao");

/**
 * Função auxiliar para gerar URL da arte oficial da PokéAPI
 */
function gerarUrlArteOficial(id) {
    return `https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/${id}.png`;
}

/**
 * Exibe notificação de feedback para o usuário
 */
function exibirFeedback(texto, tipo = "info") {
    if (timerFeedback) {
        clearTimeout(timerFeedback);
    }
    feedbackMessage.className = `feedback-message ${tipo}`;
    feedbackMessage.innerHTML = `<span>${texto}</span>`;

    // Reseta para aviso padrão após 5 segundos
    timerFeedback = setTimeout(() => {
        feedbackMessage.className = "feedback-message info";
        feedbackMessage.innerHTML = "<span>Pokédex conectada à API com persistência em banco de dados SQLite.</span>";
    }, 5000);
}

/**
 * Limpa mensagens de erro de validação
 */
function limparErros() {
    errorNome.textContent = "";
    errorTipoPrimario.textContent = "";
    errorHp.textContent = "";
    errorDescricao.textContent = "";
}

/**
 * Retorna classe CSS para o badge do tipo
 */
function getBadgeClass(tipo) {
    if (!tipo) return "type-default";
    const normalizado = tipo.toLowerCase().normalize("NFD").replace(/[\u0300-\u036f]/g, "");
    return `type-${normalizado}`;
}

/**
 * Busca a lista de Pokémons da API (GET /api/pokemon)
 */
async function carregarPokemons() {
    try {
        const resposta = await fetch(API_BASE_URL, {
            headers: {
                "Accept": "application/json"
            }
        });

        if (!resposta.ok) {
            throw new Error(`Falha ao carregar dados da API (HTTP ${resposta.status})`);
        }

        pokemons = await resposta.json();
        renderizarLista();
    } catch (erro) {
        console.error("Erro ao carregar Pokémons:", erro);
        exibirFeedback(`Erro ao conectar à API: ${erro.message}`, "error");
    }
}

/**
 * Renderiza os cards de Pokémons na tela
 */
function renderizarLista() {
    const termo = filterInput.value.trim().toLowerCase();
    const filtrados = pokemons.filter(p => {
        const nomeMatch = p.nome.toLowerCase().includes(termo);
        const tipo1Match = p.tipoPrimario.toLowerCase().includes(termo);
        const tipo2Match = p.tipoSecundario ? p.tipoSecundario.toLowerCase().includes(termo) : false;
        return nomeMatch || tipo1Match || tipo2Match;
    });

    totalCount.textContent = filtrados.length;
    listContainer.innerHTML = "";

    if (filtrados.length === 0) {
        emptyState.style.display = "block";
        return;
    }

    emptyState.style.display = "none";

    filtrados.forEach(pokemon => {
        const card = document.createElement("article");
        card.className = "pokemon-card";
        card.setAttribute("aria-label", `Pokémon ${pokemon.nome}`);

        const tipoSecBadge = pokemon.tipoSecundario 
            ? `<span class="type-badge ${getBadgeClass(pokemon.tipoSecundario)}">${pokemon.tipoSecundario}</span>` 
            : "";

        const imgUrl = pokemon.imagemUrl || gerarUrlArteOficial(pokemon.id);

        card.innerHTML = `
            <div>
                <div class="card-top">
                    <span class="card-id">#${String(pokemon.id).padStart(3, "0")}</span>
                    <span class="card-hp">HP: ${pokemon.pontosDeVida}</span>
                </div>
                <div class="card-image-container">
                    <img src="${imgUrl}" 
                         alt="Foto oficial de ${pokemon.nome}" 
                         class="pokemon-image" 
                         loading="lazy"
                         onerror="this.onerror=null; this.src='https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/${pokemon.id}.png';">
                </div>
                <h3 class="card-name">${pokemon.nome}</h3>
                <div class="card-types">
                    <span class="type-badge ${getBadgeClass(pokemon.tipoPrimario)}">${pokemon.tipoPrimario}</span>
                    ${tipoSecBadge}
                </div>
                <p class="card-desc">${pokemon.descricao}</p>
            </div>
            <div class="card-actions">
                <button type="button" class="btn btn-edit" data-id="${pokemon.id}" aria-label="Visualizar e editar ${pokemon.nome}">
                    Visualizar / Editar
                </button>
                <button type="button" class="btn btn-delete" data-id="${pokemon.id}" aria-label="Excluir ${pokemon.nome}">
                    Excluir
                </button>
            </div>
        `;

        // Eventos dos botões do card
        const btnEdit = card.querySelector(".btn-edit");
        btnEdit.addEventListener("click", () => carregarParaEdicao(pokemon.id));

        const btnDel = card.querySelector(".btn-delete");
        btnDel.addEventListener("click", () => excluirPokemon(pokemon.id));

        listContainer.appendChild(card);
    });
}

/**
 * Busca os dados de um Pokémon por ID na API (GET /api/pokemon/{id}) e preenche o formulário
 */
async function carregarParaEdicao(id) {
    try {
        const resposta = await fetch(`${API_BASE_URL}/${id}`, {
            headers: {
                "Accept": "application/json"
            }
        });

        if (!resposta.ok) {
            throw new Error(`Pokémon não encontrado no banco (HTTP ${resposta.status})`);
        }

        const pokemon = await resposta.json();

        limparErros();
        editandoId = pokemon.id;

        inputId.value = pokemon.id;
        inputIdDisplay.value = `#${pokemon.id}`;
        groupIdDisplay.style.display = "flex";

        inputNome.value = pokemon.nome;
        selectTipoPrimario.value = pokemon.tipoPrimario;
        selectTipoSecundario.value = pokemon.tipoSecundario || "";
        inputHp.value = pokemon.pontosDeVida;
        inputImagem.value = pokemon.imagemUrl || "";
        inputDescricao.value = pokemon.descricao;

        formHeading.textContent = `Visualizar / Editar Pokémon #${pokemon.id}`;
        formSubheading.textContent = "Altere os campos e clique em Salvar Alterações para persistir no banco de dados.";
        btnSubmit.textContent = "Salvar Alterações";
        btnSubmit.className = "btn btn-primary";

        inputNome.focus();
        exibirFeedback(`Pokémon "${pokemon.nome}" (#${pokemon.id}) carregado da API para edição.`, "info");
    } catch (erro) {
        console.error("Erro ao carregar Pokémon para edição:", erro);
        exibirFeedback(erro.message, "error");
    }
}

/**
 * Reseta o formulário para o modo de adição
 */
function resetarFormulario() {
    limparErros();
    editandoId = null;
    form.reset();
    inputId.value = "";
    inputIdDisplay.value = "";
    groupIdDisplay.style.display = "none";
    inputImagem.value = "";

    formHeading.textContent = "Adicionar Pokémon";
    formSubheading.textContent = "Preencha os campos para cadastrar um novo Pokémon no banco de dados.";
    btnSubmit.textContent = "Adicionar Pokémon";
    btnSubmit.className = "btn btn-primary";
}

/**
 * Valida os campos obrigatórios
 */
function validarCampos() {
    limparErros();
    let valido = true;

    if (!inputNome.value.trim()) {
        errorNome.textContent = "O nome do Pokémon é obrigatório.";
        valido = false;
    }

    if (!selectTipoPrimario.value) {
        errorTipoPrimario.textContent = "Selecione o tipo primário.";
        valido = false;
    }

    const hpNum = parseInt(inputHp.value, 10);
    if (!inputHp.value || isNaN(hpNum) || hpNum <= 0) {
        errorHp.textContent = "Informe um valor de HP válido maior que 0.";
        valido = false;
    }

    if (!inputDescricao.value.trim()) {
        errorDescricao.textContent = "A descrição é obrigatória.";
        valido = false;
    }

    return valido;
}

/**
 * Processa a submissão do formulário:
 * - Modo Adição: POST /api/pokemon (HTTP 201 Created)
 * - Modo Edição: PUT /api/pokemon/{id} (HTTP 200 OK)
 */
form.addEventListener("submit", async (e) => {
    e.preventDefault();

    if (!validarCampos()) {
        exibirFeedback("Por favor, corrija os erros indicados no formulário.", "warning");
        return;
    }

    const dados = {
        nome: inputNome.value.trim(),
        tipoPrimario: selectTipoPrimario.value,
        tipoSecundario: selectTipoSecundario.value || null,
        pontosDeVida: parseInt(inputHp.value, 10),
        descricao: inputDescricao.value.trim(),
        imagemUrl: inputImagem.value.trim() || null
    };

    try {
        if (editandoId !== null) {
            // Atualização via PUT
            const resposta = await fetch(`${API_BASE_URL}/${editandoId}`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                },
                body: JSON.stringify(dados)
            });

            if (!resposta.ok) {
                const erroJson = await resposta.json().catch(() => ({}));
                throw new Error(erroJson.mensagem || `Falha na atualização (HTTP ${resposta.status})`);
            }

            const pokemonAtualizado = await resposta.json();
            exibirFeedback(`Pokémon "${pokemonAtualizado.nome}" (#${pokemonAtualizado.id}) atualizado com sucesso no banco de dados!`, "success");
            resetarFormulario();
            await carregarPokemons();
        } else {
            // Criação via POST
            const resposta = await fetch(API_BASE_URL, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                },
                body: JSON.stringify(dados)
            });

            if (!resposta.ok) {
                const erroJson = await resposta.json().catch(() => ({}));
                throw new Error(erroJson.mensagem || `Falha no cadastro (HTTP ${resposta.status})`);
            }

            const pokemonCriado = await resposta.json();
            exibirFeedback(`Pokémon "${pokemonCriado.nome}" cadastrado com sucesso no banco com ID #${pokemonCriado.id}!`, "success");
            resetarFormulario();
            await carregarPokemons();
        }
    } catch (erro) {
        console.error("Erro ao enviar dados do formulário:", erro);
        exibirFeedback(`Erro: ${erro.message}`, "error");
    }
});

/**
 * Exclusão de Pokémon via DELETE /api/pokemon/{id} (HTTP 204 No Content)
 */
async function excluirPokemon(id) {
    const pokemon = pokemons.find(p => p.id === id);
    const nomePokemon = pokemon ? pokemon.nome : `#${id}`;

    const confirmacao = window.confirm(`Deseja realmente remover o Pokémon "${nomePokemon}" (#${id}) do banco de dados?`);
    if (!confirmacao) return;

    try {
        const resposta = await fetch(`${API_BASE_URL}/${id}`, {
            method: "DELETE"
        });

        if (!resposta.ok && resposta.status !== 204) {
            const erroJson = await resposta.json().catch(() => ({}));
            throw new Error(erroJson.mensagem || `Falha ao excluir (HTTP ${resposta.status})`);
        }

        // Se estava editando o item excluído, reseta o formulário
        if (editandoId === id) {
            resetarFormulario();
        }

        exibirFeedback(`Pokémon "${nomePokemon}" (#${id}) excluído com sucesso do banco de dados.`, "warning");
        await carregarPokemons();
    } catch (erro) {
        console.error("Erro ao excluir Pokémon:", erro);
        exibirFeedback(`Erro ao excluir: ${erro.message}`, "error");
    }
}

// Botão de cancelar / limpar
btnCancel.addEventListener("click", () => {
    resetarFormulario();
    exibirFeedback("Formulário limpo.", "info");
});

// Filtro de busca em tempo real
filterInput.addEventListener("input", renderizarLista);

// Inicialização da interface: carrega os Pokémons da API
document.addEventListener("DOMContentLoaded", () => {
    carregarPokemons();
});
