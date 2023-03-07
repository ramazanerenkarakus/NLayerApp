﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NLayer.Core.DTOs;
using NLayer.Core.Models;
using NLayer.Core.Repositories;
using NLayer.Core.Service;
using NLayer.Core.UnitOfWorks;
using NLayer.Service.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace NLayer.Caching
{
    public class ProductServiceWithCaching : IProductService
    {

        private const string CacheProductKey = "productsCache";

        private readonly IMapper _mapper;

        private readonly IMemoryCache _memoryCache;

        private readonly IProductRepository _repository;

        private readonly IUnitOfWork _unitOfWork;

        public ProductServiceWithCaching(IUnitOfWork unitOfWork, IProductRepository repository, IMemoryCache memoryCache, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _memoryCache = memoryCache;
            _mapper = mapper;


            if (!_memoryCache.TryGetValue(CacheProductKey, out _))
            {
                _memoryCache.Set(CacheProductKey, _repository.GetProductsWithCategory().Result);
            }

        }

        public async Task<Product> AddAsync(Product entity)
        {
            await _repository.AddAsync(entity);
            await _unitOfWork.CommitAsync();
            await CahceAllProductsAsync();
            return entity;
        }

        public async Task<IEnumerable<Product>> AddRangeAsync(IEnumerable<Product> entities)
        {
            await _repository.AddRangeAsync(entities);
            await _unitOfWork.CommitAsync();
            await CahceAllProductsAsync();
            return entities;
        }

        public Task<bool> AnyAsync(Expression<Func<Product, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Product>> GetAllAsync()
        {
            return Task.FromResult(_memoryCache.Get<IEnumerable<Product>>(CacheProductKey));
        }

        public Task<Product> GetByIdAsync(int id)
        {
            var product = _memoryCache.Get<List<Product>>(CacheProductKey).FirstOrDefault(x => x.Id == id);

            if (product == null)
            {
                throw new NotFoundException($"{typeof(Product).Name}({id}) not found.");
            }

            return Task.FromResult(product);
        }

        public Task<List<ProductWithCategoryDto>> GetProductsWithCategory()
        {
            var products = _memoryCache.Get<IEnumerable<Product>>(CacheProductKey);

            var productCategoryDto = _mapper.Map<List<ProductWithCategoryDto>>(products);

            return Task.FromResult(productCategoryDto);
        }

        public async Task RemoveAsync(Product entity)
        {
            _repository.Remove(entity);
            await _unitOfWork.CommitAsync();
            await CahceAllProductsAsync();
        }

        public async Task RemoveRangeAsync(IEnumerable<Product> entities)
        {
            _repository.RemoveRange(entities);
            await _unitOfWork.CommitAsync();
            await CahceAllProductsAsync();
        }

        public async Task UpdateAsync(Product entity)
        {
            _repository.Update(entity);
            await _unitOfWork.CommitAsync();
            await CahceAllProductsAsync();
        }

        public IQueryable<Product> Where(Expression<Func<Product, bool>> expression)
        {
            return _memoryCache.Get<List<Product>>(CacheProductKey).Where(expression.Compile()).AsQueryable();
        }

        public async Task CahceAllProductsAsync() //Caching method.
        {
            _memoryCache.Set(CacheProductKey, await _repository.GetAll().ToListAsync());
        }

    }
}
