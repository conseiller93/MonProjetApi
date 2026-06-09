using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjetApiNet.Data; 
using ProjetApiNet.Models;

namespace ProjetApiNet.Repositories
{
    public interface IUtilisateurRepository
    {
        Task<IEnumerable<Utilisateur>> GetAllAsync();
        Task<Utilisateur?> GetByIdAsync(int id);
        Task AddAsync(Utilisateur utilisateur);
        void Update(Utilisateur utilisateur);
        void Delete(Utilisateur utilisateur);
        Task<bool> SaveChangesAsync();
    }

    public class UtilisateurRepository : IUtilisateurRepository
    {
        private readonly ApplicationDbContext _context;

        public UtilisateurRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Utilisateur>> GetAllAsync()
        {
            return await _context.Set<Utilisateur>()
                .Include(u => u.GroupesSupervises)
                .Include(u => u.CamionsConduits)
                .ToListAsync();
        }

        public async Task<Utilisateur?> GetByIdAsync(int id)
        {
            return await _context.Set<Utilisateur>()
                .Include(u => u.GroupesSupervises)
                .Include(u => u.CamionsConduits)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task AddAsync(Utilisateur utilisateur)
        {
            await _context.Set<Utilisateur>().AddAsync(utilisateur);
        }

        public void Update(Utilisateur utilisateur)
        {
            _context.Set<Utilisateur>().Update(utilisateur);
        }

        public void Delete(Utilisateur utilisateur)
        {
            _context.Set<Utilisateur>().Remove(utilisateur);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}