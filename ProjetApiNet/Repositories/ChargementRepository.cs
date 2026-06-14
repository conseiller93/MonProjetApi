using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjetApiNet.Data;
using ProjetApiNet.Models;

namespace ProjetApiNet.Repositories;

public interface IChargementRepository
{
    Task<IEnumerable<Chargement>> GetAllAsync();
    Task<Chargement?> GetByIdAsync(int id);
    Task AddAsync(Chargement chargement);
    void Update(Chargement chargement);
    void Delete(Chargement chargement);
    Task<bool> SaveChangesAsync();
}

public class ChargementRepository : IChargementRepository
{
    private readonly ApplicationDbContext _context;

    public ChargementRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Chargement>> GetAllAsync()
    {
        return await _context.Set<Chargement>()
            .AsNoTracking()
            .Include(c => c.Camion)
                // CORRIGÉ : la propriété de navigation s'appelle GroupeTransport (pas Groupe)
                .ThenInclude(cam => cam.GroupeTransport)
            .Include(c => c.ZoneMiniere)  // CORRIGÉ : inclure directement ZoneMiniere depuis Chargement
            .ToListAsync();
    }

    public async Task<Chargement?> GetByIdAsync(int id)
    {
        return await _context.Set<Chargement>()
            .AsNoTracking()
            .Include(c => c.Camion)
                .ThenInclude(cam => cam.GroupeTransport)
            .Include(c => c.ZoneMiniere)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Chargement chargement)
    {
        await _context.Set<Chargement>().AddAsync(chargement);
    }

    public void Update(Chargement chargement)
    {
        _context.Set<Chargement>().Update(chargement);
    }

    public void Delete(Chargement chargement)
    {
        _context.Set<Chargement>().Remove(chargement);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return (await _context.SaveChangesAsync()) > 0;
    }
}