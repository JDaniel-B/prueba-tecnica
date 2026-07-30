using MesaSitec.Dominio.Entidades;

namespace MesaSitec.Aplicacion.Autenticacion.Abstracciones;

public interface IPasswordVerifier
{
    bool Verificar(Usuario usuario, string password);
}
