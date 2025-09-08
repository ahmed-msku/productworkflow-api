using ProductWorkflow.API.Models;

namespace ProductWorkflow.API.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<Product> Add(Product product);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task Update(Product product);
        Task Delete(int id);
        Task AddBatch(List<Product> products);
    }
}
