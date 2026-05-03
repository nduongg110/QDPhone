using Microsoft.EntityFrameworkCore;
using QDPhone.Web.Data;

namespace QDPhone.Web.Services;

public interface IReviewService
{
    Task<double> GetAverageRatingAsync(int productId);
}

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _db;
    public ReviewService(ApplicationDbContext db) => _db = db;

    public async Task<double> GetAverageRatingAsync(int productId)
        => await _db.Reviews.Where(x => x.ProductId == productId).AverageAsync(x => (double?)x.Rating) ?? 0d;
}

