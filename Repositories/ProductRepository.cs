using Microsoft.EntityFrameworkCore;
using ProductWorkflow.API.Models;
using ProductWorkflow.Data;
using ProductWorkflow.API.Repositories.Interfaces;

namespace ProductWorkflow.API.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public ProductRepository(IDbContextFactory<AppDbContext> contextFactory)
            => _dbContextFactory = contextFactory;

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            return await dbContext.Products.Take(20).OrderByDescending(x => x.Id).ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            return await dbContext.Products.FindAsync(id);
        }

        public async Task<Product> Add(Product product)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            return product;
        }

        public async Task Update(Product product)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

            dbContext.Products.Update(product);
            await dbContext.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

            var product = await dbContext.Products.FindAsync(id);

            if (product != null)
            {
                dbContext.Products.Remove(product);
                await dbContext.SaveChangesAsync();
            }
        }

        // This method can be optimized further by using StoredProcedure or BulkInsert libraries
        // For smaller batches the code will work fine 
        public async Task AddBatch(List<Product> products)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

            dbContext.Products.AddRange(products);
            await dbContext.SaveChangesAsync();
        }
    }
}
