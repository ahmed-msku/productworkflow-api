using ProductWorkflow.API.Models;
using ProductWorkflow.API.Repositories.Interfaces;

namespace ProductWorkflow.API.Services
{
    public class ProductService
    {
        private readonly IProductRepository _repository;
        private readonly ILogger _logger;

        public ProductService(IProductRepository repository, ILogger<ProductService> logger) 
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Product> Create(Product product)
        {
            return await _repository.Add(product);
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task Update(Product product)
        {
            await _repository.Update(product);
        }

        public async Task Delete(int id)
        {
            await _repository.Delete(id);
        }
    }
}
