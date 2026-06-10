using Microsoft.EntityFrameworkCore;
using ProjetApiNet.Data;        // CORRIGÉ : namespace unifié (plus de TCA.API.Data)
using ProjetApiNet.Models;      // CORRIGÉ : namespace unifié (plus de TCA.API.Models)

namespace ProjetApiNet.Repositories  // CORRIGÉ : namespace unifié (plus de TCA.API.Repositories)
{
    public interface IGroupeTransportRepository
    {
        Task<IEnumerable<GroupeTransport>> GetAllAsync();
        Task<GroupeTransport?> GetByIdAsync(int id);
        Task AddAsync(GroupeTransport groupeTransport);
        void Update(GroupeTransport groupeTransport);
        void Delete(GroupeTransport groupeTransport);
        Task<bool> SaveChangesAsync();
    }

    public class GroupeTransportRepository : IGroupeTransportRepository
    {
        private readonly ApplicationDbContext _context;

        public GroupeTransportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GroupeTransport>> GetAllAsync()
        {
            return await _context.Set<GroupeTransport>()
                .Include(g => g.SuperviseurGroupe)
                .Include(g => g.ZoneMiniere)
                .Include(g => g.Camions)
                .ToListAsync();
        }

        public async Task<GroupeTransport?> GetByIdAsync(int id)
        {
            return await _context.Set<GroupeTransport>()
                .Include(g => g.SuperviseurGroupe)
                .Include(g => g.ZoneMiniere)
                .Include(g => g.Camions)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task AddAsync(GroupeTransport groupeTransport)
        {
            await _context.Set<GroupeTransport>().AddAsync(groupeTransport);
        }

        public void Update(GroupeTransport groupeTransport)
        {
            _context.Set<GroupeTransport>().Update(groupeTransport);
        }

        public void Delete(GroupeTransport groupeTransport)
        {
            _context.Set<GroupeTransport>().Remove(groupeTransport);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}