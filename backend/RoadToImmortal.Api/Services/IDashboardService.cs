using System.Threading.Tasks;
using RoadToImmortal.Api.Models;

namespace RoadToImmortal.Api.Services;

public interface IDashboardService
{
    Task<DashboardDto?> GetDashboardAsync(long steamId);
}
