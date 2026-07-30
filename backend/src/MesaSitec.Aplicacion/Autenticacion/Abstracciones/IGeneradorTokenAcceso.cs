using MesaSitec.Aplicacion.Autenticacion.Modelos;

namespace MesaSitec.Aplicacion.Autenticacion.Abstracciones;

public interface IGeneradorTokenAcceso
{
    TokenAcceso Generar(UsuarioAutenticacion usuario);
}
