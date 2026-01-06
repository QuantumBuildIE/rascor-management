using Rascor.Core.Application.Models;
using Rascor.Modules.StockManagement.Application.Features.BayLocations.DTOs;

namespace Rascor.Modules.StockManagement.Application.Features.BayLocations;

public interface IBayLocationService
{
    Task<Result<List<BayLocationDto>>> GetAllAsync(Guid? stockLocationId = null);
    Task<Result<BayLocationDto>> GetByIdAsync(Guid id);
    Task<Result<List<BayLocationDto>>> GetByLocationAsync(Guid stockLocationId);
    Task<Result<BayLocationDto>> CreateAsync(CreateBayLocationDto dto);
    Task<Result<BayLocationDto>> UpdateAsync(Guid id, UpdateBayLocationDto dto);
    Task<Result> DeleteAsync(Guid id);
}
