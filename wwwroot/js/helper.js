
// función para validar el correo
const validarMail = (email) => {
  const formato = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
  //retorna true o false
  return formato.test(email);
};

// función para validarque permita sólo el ingreso de números
const soloNumeros = (evt) => {
  if (evt.keyCode >= 48 && evt.keyCode <= 57) return true;
  return false;
};

const limpiar = () => {
  document
    .querySelectorAll("form .form-control,.form-select")
    .forEach((item) => {
      item.value = "";
      item.classList.remove("is-invalid");
      item.classList.remove("is-valid");
      item.classList.remove("border-red-500");
      item.classList.remove("border-green-500");
      if (document.getElementById("e-" + item.id)) {
        document.getElementById("e-" + item.id).innerHTML = "";
      }
    });
};

const validaRun = (run) => {
  const Fn = {
    // función  para validar el run con su cadena completa "XXXXXXXX-X"
    validaRut: function (rutCompleto) {
      rutCompleto = rutCompleto.replace("‐", "-");
      if (!/^[0-9]+[-|‐]{1}[0-9kK]{1}$/.test(rutCompleto)) return false;
      let tmp = rutCompleto.split("-");
      let digv = tmp[1];
      let rut = tmp[0];

      // validar que tenga al menos 7 digitos (sin contar el digito verificador)
      if (rut.length < 7) return false;

      if (digv == "K") digv = "k";

      return Fn.dv(rut) == digv;
    },
    dv: function (T) {
      let M = 0,
        S = 1;
      for (; T; T = Math.floor(T / 10))
        S = (S + (T % 10) * (9 - (M++ % 6))) % 11;
      return S ? S - 1 : "k";
    },
  };
  return Fn.validaRut(run);
};

// validar longitud minima del RUN (al menos 10 caracteres con guion)
const validarLongitudRun = (run) => {
  if (!run) return false;
  // ej: 12345678-9
  return run.length >= 10;
};

// validar que no haya solo espacios
const validarSoloEspacios = (valor) => {
  return valor.trim().length > 0;
};

// validar que solo contenga letras y espacios (para nombres y apellidos)
const validarSoloLetras = (valor) => {
  const formato = /^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$/;
  return formato.test(valor);
};

// validar que no contenga numeros
const validarSinNumeros = (valor) => {
  return !/\d/.test(valor);
};

// validar fecha (no puede ser pasada)
const validarFechaFutura = (fecha) => {
  const fechaIngresada = new Date(fecha);
  const fechaActual = new Date();
  fechaActual.setHours(0, 0, 0, 0);
  return fechaIngresada >= fechaActual;
};

// validar que fecha fin > fecha inicio
const validarRangoFechas = (fechaInicio, fechaFin) => {
  const inicio = new Date(fechaInicio);
  const fin = new Date(fechaFin);
  return fin > inicio;
};

// validar telefono (solo numeros, 9-15 digitos)
const validarTelefono = (telefono) => {
  const formato = /^\d{9,15}$/;
  return formato.test(telefono);
};

// funcion para prevenir espacios al inicio
const prevenirEspaciosInicio = (evt) => {
  if (evt.target.value.length === 0 && evt.key === " ") {
    evt.preventDefault();
    return false;
  }
  return true;
};
