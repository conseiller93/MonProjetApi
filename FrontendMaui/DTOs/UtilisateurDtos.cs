using FrontendMaui.Models;

namespace FrontendMaui.DTOs;

public class ConnexionDto
{
    public string Identifiant { get; set; } = string.Empty;
    public string MotDePasse { get; set; } = string.Empty;
}

public class UtilisateurDto
{
    public int Id { get; set; }
    public string Identifiant { get; set; } = string.Empty;
    public RoleUtilisateur Role { get; set; }
    public decimal SalaireMensuelGNF { get; set; }
}

public class SessionResponseDto
{
    public UtilisateurDto Utilisateur { get; set; } = new();
    public string Token { get; set; } = string.Empty;
}

public class UtilisateurCreateDto
{
    public string Identifiant { get; set; } = string.Empty;
    public string MotDePasse { get; set; } = string.Empty;
    public RoleUtilisateur Role { get; set; }
}

public class UtilisateurLoginDto
{
    public string Identifiant { get; set; } = string.Empty;
    public string MotDePasse { get; set; } = string.Empty;
}
