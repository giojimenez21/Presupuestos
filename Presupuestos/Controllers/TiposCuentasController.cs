using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Presupuestos.Models;
using Presupuestos.Services;

namespace Presupuestos.Controllers
{
    public class TiposCuentasController: Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuarios servicioUsuarios;

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, IServicioUsuarios servicioUsuarios)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuarios = servicioUsuarios;
        }
        public IActionResult Crear()
        {
            
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Editar(Tipocuenta tipocuenta)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(tipocuenta.Id, usuarioId);
            if(tipocuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            return View(tiposCuentas);
        }

        [HttpGet]
        public async Task<ActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if(tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Tipocuenta tipocuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipocuenta);
            }
            tipocuenta.UsuarioId = servicioUsuarios.ObtenerUsuarioId();
            var yaExiste = await repositorioTiposCuentas.Existe(tipocuenta.Nombre, tipocuenta.UsuarioId);
            if (yaExiste)
            {
                ModelState.AddModelError(nameof(tipocuenta.Nombre), $"El nombre {tipocuenta.Nombre} ya existe");
                return View(tipocuenta);
            }

            await repositorioTiposCuentas.Crear(tipocuenta);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre, int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var yaExisteTipoCuenta = await repositorioTiposCuentas.Existe(nombre, usuarioId, id);
            if(yaExisteTipoCuenta)
            {
                return Json($"El nombre {nombre} ya existe");
            }

            return Json(true);
        }

        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipocuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if(tipocuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipocuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarTipoCuenta(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipocuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if (tipocuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas.Borrar(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            var idsTipoSCuentas = tiposCuentas.Select(x => x.Id);

            var idsTiposCuentasNoPertenecenAlUsuario = ids.Except(idsTipoSCuentas).ToList();
            if(idsTiposCuentasNoPertenecenAlUsuario.Count > 0)
            {
                return Forbid();
            }
            var tiposCuentasOrdenados = ids.Select((valor, indice) => new Tipocuenta() { Id = valor, Orden = indice + 1 }).AsEnumerable();
            await repositorioTiposCuentas.Ordenar(tiposCuentasOrdenados);
            return Ok();
        }
    }
}
