namespace FrontendMaui.Models;

public class Utilisateur
{
    public int Id { get; set; }
    public string Identifiant { get; set; } = string.Empty;
    public RoleUtilisateur Role { get; set; }
    public decimal SalaireMensuelGNF { get; set; }
}
