using Microsoft.EntityFrameworkCore;
using ProjetApiNet.Data; 
using ProjetApiNet.Models;

namespace ProjetApiNet.Repositories
{
    public interface IZoneMiniereRepository
    {
        Task<IEnumerable<ZoneMiniere>> GetAllAsync();
        Task<ZoneMiniere?> GetByIdAsync(int id);
        Task AddAsync(ZoneMiniere zoneMiniere);
        void Update(ZoneMiniere zoneMiniere);
        void Delete(ZoneMiniere zoneMiniere);
        Task<bool> SaveChangesAsync();
    }

    public class ZoneMiniereRepository : IZoneMiniereRepository
    {
        private readonly ApplicationDbContext _context;

        public ZoneMiniereRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ZoneMiniere>> GetAllAsync()
        {
            return await _context.Set<ZoneMiniere>().AsNoTracking().ToListAsync();
        }

        public async Task<ZoneMiniere?> GetByIdAsync(int id)
        {
            return await _context.Set<ZoneMiniere>().AsNoTracking().FirstOrDefaultAsync(z => z.Id == id);
        }

        public async Task AddAsync(ZoneMiniere zoneMiniere)
        {
            await _context.Set<ZoneMiniere>().AddAsync(zoneMiniere);
        }

        public void Update(ZoneMiniere zoneMiniere)
        {
            _context.Set<ZoneMiniere>().Update(zoneMiniere);
        }

        public void Delete(ZoneMiniere zoneMiniere)
        {
            _context.Set<ZoneMiniere>().Remove(zoneMiniere);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync()) > 0;
        }
    }
}