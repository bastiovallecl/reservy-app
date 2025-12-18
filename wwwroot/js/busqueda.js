// Búsqueda Global en Tiempo Real
(function () {
  const searchInput = document.getElementById("searchInput");
  const searchResults = document.getElementById("searchResults");
  const btnNavbarSearch = document.getElementById("btnNavbarSearch");
  let searchTimeout;

  if (!searchInput || !searchResults) return;

  // Búsqueda al escribir (debounce)
  searchInput.addEventListener("input", function () {
    clearTimeout(searchTimeout);
    const termino = this.value.trim();

    if (termino.length < 2) {
      searchResults.style.display = "none";
      return;
    }

    searchTimeout = setTimeout(() => {
      realizarBusqueda(termino);
    }, 300);
  });

  // Búsqueda al hacer clic en el botón
  btnNavbarSearch.addEventListener("click", function () {
    const termino = searchInput.value.trim();
    if (termino.length >= 2) {
      realizarBusqueda(termino);
    }
  });

  // Búsqueda al presionar Enter
  searchInput.addEventListener("keypress", function (e) {
    if (e.key === "Enter") {
      e.preventDefault();
      const termino = this.value.trim();
      if (termino.length >= 2) {
        realizarBusqueda(termino);
      }
    }
  });

  // Cerrar resultados al hacer clic fuera
  document.addEventListener("click", function (e) {
    if (
      !searchInput.contains(e.target) &&
      !searchResults.contains(e.target) &&
      !btnNavbarSearch.contains(e.target)
    ) {
      searchResults.style.display = "none";
    }
  });

  function realizarBusqueda(termino) {
    // Mostrar indicador de carga
    searchResults.innerHTML =
      '<div class="p-3 text-center"><i class="fas fa-spinner fa-spin"></i> Buscando...</div>';
    searchResults.style.display = "block";

    fetch(`/Home/BusquedaGlobal?termino=${encodeURIComponent(termino)}`)
      .then((response) => response.json())
      .then((data) => {
        mostrarResultados(data);
      })
      .catch((error) => {
        console.error("Error en la búsqueda:", error);
        searchResults.innerHTML =
          '<div class="p-3 text-danger"><i class="fas fa-exclamation-triangle"></i> Error al buscar</div>';
      });
  }

  function mostrarResultados(resultados) {
    if (resultados.length === 0) {
      searchResults.innerHTML =
        '<div class="p-3 text-muted text-center"><i class="fas fa-search"></i> No se encontraron resultados</div>';
      return;
    }

    let html = '<div class="list-group list-group-flush">';

    resultados.forEach((resultado) => {
      const iconoColor = getIconColor(resultado.colorBadge);
      html += `
                <a href="${
                  resultado.urlDetalle
                }" class="list-group-item list-group-item-action border-0">
                    <div class="d-flex align-items-start">
                        <div class="me-3 pt-1">
                            <i class="fas ${
                              resultado.icono
                            } fa-lg text-${iconoColor}"></i>
                        </div>
                        <div class="flex-grow-1">
                            <div class="d-flex justify-content-between align-items-center">
                                <h6 class="mb-1 fw-bold">${
                                  resultado.titulo
                                }</h6>
                                <span class="badge bg-${
                                  resultado.colorBadge
                                }">${resultado.tipo}</span>
                            </div>
                            ${
                              resultado.subtitulo
                                ? `<p class="mb-1 text-muted small">${resultado.subtitulo}</p>`
                                : ""
                            }
                            ${
                              resultado.descripcion
                                ? `<p class="mb-0 small text-truncate" style="max-width: 400px;">${resultado.descripcion}</p>`
                                : ""
                            }
                        </div>
                    </div>
                </a>
            `;
    });

    html += "</div>";
    searchResults.innerHTML = html;
  }

  function getIconColor(colorBadge) {
    const colorMap = {
      success: "success",
      warning: "warning",
      danger: "danger",
      primary: "primary",
      info: "info",
      secondary: "secondary",
    };
    return colorMap[colorBadge] || "secondary";
  }
})();
