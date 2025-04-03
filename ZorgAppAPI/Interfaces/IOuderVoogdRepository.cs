using ZorgAppAPI.Models;

namespace ZorgAppAPI.Interfaces
{
    public interface IOuderVoogdRepository
    {
        Task<IEnumerable<OuderVoogd>> GetOuderVoogden();
        Task<OuderVoogd> GetOuderVoogd(int id);
        Task<OuderVoogd> GetCurrentOuderVoogd(string userId);
    }
}
