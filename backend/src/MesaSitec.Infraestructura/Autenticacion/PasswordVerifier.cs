using MesaSitec.Aplicacion.Autenticacion.Abstracciones;
using MesaSitec.Dominio.Entidades;
using Microsoft.AspNetCore.Identity;

namespace MesaSitec.Infraestructura.Autenticacion;

public sealed class PasswordVerifier(
    IPasswordHasher<Usuario> passwordHasher) : IPasswordVerifier
{
    public bool Verificar(Usuario usuario, string password)
    {
        var resultado = passwordHasher.VerifyHashedPassword(
            usuario,
            usuario.PasswordHash,
            password);

        return resultado != PasswordVerificationResult.Failed;
    }
}
