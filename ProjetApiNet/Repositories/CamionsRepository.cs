using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjetApiNet.Data;
using ProjetApiNet.Models;

namespace ProjetApiNet.Repositories
{
    public interface ICamionRepository
    {
        Task<IEnumerable<Camion>> GetAllAsync();
        Task<Camion?> GetByIdAsync(int id);
        Task AddAsync(Camion camion);
        void Update(Camion camion);
        void Delete(Camion camion);
        Task<bool> SaveChangesAsync();
    }

    public class CamionRepository : ICamionRepository
    {
        private readonly ApplicationDbContext _context;

        public CamionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Camion>> GetAllAsync()
        {
            return await _context.Set<Camion>()
                .AsNoTracking()
                .Include(c => c.Chauffeur)
                .Include(c => c.GroupeTransport)
                .ToListAsync();
        }

        public async Task<Camion?> GetByIdAsync(int id)
        {
            return await _context.Set<Camion>()
                .AsNoTracking()
                .Include(c => c.Chauffeur)
                .Include(c => c.GroupeTransport)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Camion camion)
        {
            await _context.Set<Camion>().AddAsync(camion);
        }

        public void Update(Camion camion)
        {
            _context.Set<Camion>().Update(camion);
        }

        public void Delete(Camion camion)
        {
            _context.Set<Camion>().Remove(camion);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}