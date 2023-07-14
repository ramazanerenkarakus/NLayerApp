using NLayer.Core.DTOs;
using NLayer.Core.Models;

namespace NLayer.Core.Service
{
    public interface ICategoryService : IService<Category>
    {
        public Task<CustomResponseDto<CategoryWithProductsDto>> GetSingleCategoryByIdWithProductsAsync(int categoryId);
    }
}
