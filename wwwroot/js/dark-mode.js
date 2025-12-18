/**
 * Sistema de Modo Oscuro - JavaScript
 * ====================================
 * Maneja la lógica de cambio entre modo claro y oscuro.
 * Guarda la preferencia del usuario en localStorage.
 * Detecta la preferencia del sistema operativo.
 *
 * Autor: Sistema de Gestión de Eventos
 * Fecha: Noviembre 2025
 */

(function () {
  "use strict";

  // ========================================
  // CONSTANTES Y CONFIGURACIÓN
  // ========================================

  const DARK_MODE_CLASS = "dark-mode";
  const STORAGE_KEY = "theme-preference";
  const THEME_LIGHT = "light";
  const THEME_DARK = "dark";
  const THEME_AUTO = "auto";

  // ========================================
  // ESTADO Y ELEMENTOS DOM
  // ========================================

  let darkModeToggle = null;
  let currentTheme = THEME_LIGHT;

  // ========================================
  // FUNCIONES DE UTILIDAD
  // ========================================

  /**
   * Obtiene la preferencia del sistema operativo
   * @returns {string} 'dark' o 'light'
   */
  function getSystemPreference() {
    if (
      window.matchMedia &&
      window.matchMedia("(prefers-color-scheme: dark)").matches
    ) {
      return THEME_DARK;
    }
    return THEME_LIGHT;
  }

  /**
   * Obtiene la preferencia guardada del usuario
   * @returns {string} 'dark', 'light' o 'auto'
   */
  function getSavedTheme() {
    try {
      return localStorage.getItem(STORAGE_KEY) || THEME_AUTO;
    } catch (e) {
      console.warn("No se pudo acceder a localStorage:", e);
      return THEME_AUTO;
    }
  }

  /**
   * Guarda la preferencia del usuario
   * @param {string} theme - 'dark', 'light' o 'auto'
   */
  function saveTheme(theme) {
    try {
      localStorage.setItem(STORAGE_KEY, theme);
    } catch (e) {
      console.warn("No se pudo guardar en localStorage:", e);
    }
  }

  /**
   * Determina el tema efectivo a aplicar
   * @param {string} preference - La preferencia del usuario
   * @returns {string} 'dark' o 'light'
   */
  function resolveTheme(preference) {
    if (preference === THEME_AUTO) {
      return getSystemPreference();
    }
    return preference;
  }

  // ========================================
  // APLICACIÓN DEL TEMA
  // ========================================

  /**
   * Aplica el modo oscuro al documento
   */
  function enableDarkMode() {
    // Verificar que el body exista antes de intentar modificarlo
    if (!document.body) {
      // Si el body no existe, añadir clase al HTML temporalmente
      document.documentElement.classList.add(DARK_MODE_CLASS);
      return;
    }

    document.body.classList.add(DARK_MODE_CLASS);
    updateToggleIcon(true);
    currentTheme = THEME_DARK;

    // Dispatch evento personalizado para que otros scripts puedan reaccionar
    window.dispatchEvent(
      new CustomEvent("themeChanged", {
        detail: { theme: THEME_DARK },
      })
    );
  }

  /**
   * Aplica el modo claro al documento
   */
  function enableLightMode() {
    // Verificar que el body exista antes de intentar modificarlo
    if (!document.body) {
      // Si el body no existe, remover clase del HTML temporalmente
      document.documentElement.classList.remove(DARK_MODE_CLASS);
      return;
    }

    document.body.classList.remove(DARK_MODE_CLASS);
    updateToggleIcon(false);
    currentTheme = THEME_LIGHT;

    // Dispatch evento personalizado
    window.dispatchEvent(
      new CustomEvent("themeChanged", {
        detail: { theme: THEME_LIGHT },
      })
    );
  }

  /**
   * Aplica un tema específico
   * @param {string} theme - 'dark' o 'light'
   */
  function applyTheme(theme) {
    if (theme === THEME_DARK) {
      enableDarkMode();
    } else {
      enableLightMode();
    }
  }

  /**
   * Actualiza el icono del botón toggle
   * @param {boolean} isDark - Si el modo oscuro está activo
   */
  function updateToggleIcon(isDark) {
    if (!darkModeToggle) return;

    const icon = darkModeToggle.querySelector("i");
    if (!icon) return;

    if (isDark) {
      icon.classList.remove("fa-moon");
      icon.classList.add("fa-sun");
      darkModeToggle.setAttribute("title", "Cambiar a modo claro");
      darkModeToggle.setAttribute("aria-label", "Cambiar a modo claro");
    } else {
      icon.classList.remove("fa-sun");
      icon.classList.add("fa-moon");
      darkModeToggle.setAttribute("title", "Cambiar a modo oscuro");
      darkModeToggle.setAttribute("aria-label", "Cambiar a modo oscuro");
    }
  }

  // ========================================
  // MANEJO DE EVENTOS
  // ========================================

  /**
   * Alterna entre modo claro y oscuro
   */
  function toggleDarkMode() {
    const newTheme = currentTheme === THEME_DARK ? THEME_LIGHT : THEME_DARK;
    saveTheme(newTheme);
    applyTheme(newTheme);

    // Añadir pequeña animación al botón
    if (darkModeToggle) {
      darkModeToggle.style.transform = "scale(0.9)";
      setTimeout(() => {
        darkModeToggle.style.transform = "scale(1)";
      }, 150);
    }
  }

  /**
   * Maneja cambios en la preferencia del sistema
   * @param {MediaQueryListEvent} e - Evento de cambio
   */
  function handleSystemThemeChange(e) {
    const savedTheme = getSavedTheme();

    // Solo reaccionar si el usuario tiene configurado 'auto'
    if (savedTheme === THEME_AUTO) {
      const systemTheme = e.matches ? THEME_DARK : THEME_LIGHT;
      applyTheme(systemTheme);
    }
  }

  // ========================================
  // INICIALIZACIÓN
  // ========================================

  /**
   * Inicializa el sistema de modo oscuro
   * Se ejecuta lo antes posible para evitar parpadeos
   */
  function initializeTheme() {
    const savedTheme = getSavedTheme();
    const effectiveTheme = resolveTheme(savedTheme);
    applyTheme(effectiveTheme);
  }

  /**
   * Configura los event listeners y elementos DOM
   */
  function setupEventListeners() {
    // Transferir clase del html al body si es necesario
    if (
      document.documentElement.classList.contains(DARK_MODE_CLASS) &&
      document.body
    ) {
      document.body.classList.add(DARK_MODE_CLASS);
      document.documentElement.classList.remove(DARK_MODE_CLASS);
    } else if (
      !document.documentElement.classList.contains(DARK_MODE_CLASS) &&
      document.body
    ) {
      document.body.classList.remove(DARK_MODE_CLASS);
    }

    // Buscar el botón toggle
    darkModeToggle = document.getElementById("darkModeToggle");

    if (darkModeToggle) {
      darkModeToggle.addEventListener("click", toggleDarkMode);

      // Actualizar el icono inicial
      updateToggleIcon(currentTheme === THEME_DARK);
    } else {
      console.warn(
        "No se encontró el botón de toggle de modo oscuro (#darkModeToggle)"
      );
    }

    // Escuchar cambios en la preferencia del sistema
    if (window.matchMedia) {
      const darkModeMediaQuery = window.matchMedia(
        "(prefers-color-scheme: dark)"
      );

      // Usar el método correcto según el navegador
      if (darkModeMediaQuery.addEventListener) {
        darkModeMediaQuery.addEventListener("change", handleSystemThemeChange);
      } else if (darkModeMediaQuery.addListener) {
        // Fallback para navegadores antiguos
        darkModeMediaQuery.addListener(handleSystemThemeChange);
      }
    }
  }

  /**
   * Inicialización completa del sistema
   */
  function init() {
    // Aplicar tema inmediatamente
    initializeTheme();

    // Configurar event listeners cuando el DOM esté listo
    if (document.readyState === "loading") {
      document.addEventListener("DOMContentLoaded", setupEventListeners);
    } else {
      setupEventListeners();
    }
  }

  // ========================================
  // API PÚBLICA
  // ========================================

  /**
   * Expone API pública para uso externo
   */
  window.DarkMode = {
    /**
     * Obtiene el tema actual
     * @returns {string} 'dark' o 'light'
     */
    getCurrentTheme: function () {
      return currentTheme;
    },

    /**
     * Obtiene la preferencia guardada
     * @returns {string} 'dark', 'light' o 'auto'
     */
    getSavedTheme: getSavedTheme,

    /**
     * Establece un tema específico
     * @param {string} theme - 'dark', 'light' o 'auto'
     */
    setTheme: function (theme) {
      if ([THEME_DARK, THEME_LIGHT, THEME_AUTO].includes(theme)) {
        saveTheme(theme);
        const effectiveTheme = resolveTheme(theme);
        applyTheme(effectiveTheme);
      } else {
        console.error("Tema inválido:", theme);
      }
    },

    /**
     * Alterna el tema
     */
    toggle: toggleDarkMode,

    /**
     * Verifica si el modo oscuro está activo
     * @returns {boolean}
     */
    isDarkMode: function () {
      return currentTheme === THEME_DARK;
    },

    /**
     * Constantes disponibles
     */
    THEME_LIGHT: THEME_LIGHT,
    THEME_DARK: THEME_DARK,
    THEME_AUTO: THEME_AUTO,
  };

  // ========================================
  // INICIO
  // ========================================

  // Ejecutar inicialización inmediatamente
  init();

  // Log de inicialización
  console.log("Sistema de Modo Oscuro inicializado correctamente");
})();
