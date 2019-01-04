using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Resources;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using nscreg.Data;
using nscreg.Data.Entities;
using nscreg.Resources.Languages;
using nscreg.Server.Common.Models;
using nscreg.Server.Common.Models.Regions;
using nscreg.Utilities;
using nscreg.Utilities.Extensions;

namespace nscreg.Server.Common.Services
{
    /// <summary>
    /// Сервис региона
    /// </summary>
    public class RegionService
    {
        private readonly NSCRegDbContext _context;

        public RegionService(NSCRegDbContext dbContext)
        {
            _context = dbContext;
        }
        /// <summary>
        /// Метод получения списка регионов
        /// </summary>
        /// <param name="model">Модель</param>
        /// <param name="predicate">Предикат</param>
        /// <returns></returns>
        public async Task<SearchVm<Region>> ListAsync(PaginatedQueryM model,
            Expression<Func<Region, bool>> predicate = null)
        {
            IQueryable<Region> query = _context.Regions;
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            var total = await query.CountAsync();
            var regions = await query.OrderBy(v => v.Code)
                .Skip(Pagination.CalculateSkip(model.PageSize, model.Page, total))
                .Take(model.PageSize)
                .ToListAsync();
            return SearchVm<Region>.Create(regions, total);
        }

        /// <summary>
        /// Метод поиска региона
        /// </summary>
        /// <param name="predicate">Предикат</param>
        /// <param name="limit">Ограничение</param>
        /// <returns></returns>
        public Task<List<Region>> ListAsync(Expression<Func<Region, bool>> predicate = null, int? limit = null)
        {
            IQueryable<Region> query = _context.Regions;
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            return query.Where(v => !v.IsDeleted).OrderBy(v => v.Code).ToListAsync();
        }

        /// <summary>
        /// Метод получения региона
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns></returns>
        public async Task<Region> GetAsync(int id)
        {
            var region = await _context.Regions.SingleOrDefaultAsync(x => x.Id == id);
            if (region == null) throw new BadRequestException(nameof(Resource.RegionNotExistsError));
            return region;
        }

        /// <summary>
        /// Метод получения списка адресов
        /// </summary>
        /// <param name="code">Код</param>
        /// <returns></returns>
        public async Task<RegionM> GetAsync(string code)
        {
            var region = await _context.Regions.FirstOrDefaultAsync(x => x.Code.TrimEnd('0').Equals(code));
            return Mapper.Map<RegionM>(region);
        }

        public async Task<Region> GetRegionParents(int regionId)
        {
            async Task<Region> GetFourParents(int id)
            {
                return await _context.Regions
                    .Where(r => r.Id == id)
                    .Include(r => r.Parent)
                    .Include(r => r.Parent)
                    .Include(r => r.Parent)
                    .SingleAsync();
            }

            var region = await GetFourParents(regionId);

            Region currentRegion = region;
            while (currentRegion.ParentId.HasValue)
            {
                while (currentRegion.Parent != null)
                    currentRegion = currentRegion.Parent;

                if (currentRegion.ParentId.HasValue)
                    currentRegion.Parent = await GetFourParents(currentRegion.ParentId.Value);
            }

            return region;
        }

        /// <summary>
        /// Метод получения дерева регионов
        /// </summary>
        /// <returns></returns>
        public async Task<RegionNode> GetAllRegionTreeAsync(string rootNodeTitleResource)
        {
            var regions = await _context.Regions.OrderBy(r => r.Name).ToListAsync();
            var addedIds = new HashSet<int>();
            var regionsLookup = regions.ToLookup(r => r.ParentId);

            RegionNode ParseRegion(Region r)
            {
                bool added = addedIds.Add(r.Id);
                if (!added)
                    throw new Exception(Resource.RecursiveRegions);
                return new RegionNode
                {
                    Id = r.Id,
                    Name = r.Name,
                    NameLanguage1 = r.NameLanguage1,
                    NameLanguage2 = r.NameLanguage2,
                    Code = r.Code,
                    RegionNodes = regionsLookup[r.Id].Select(ParseRegion).ToList()
                };

            }

            addedIds.AddRange(regionsLookup[null].Select(r => r.Id));

            var resourceManager = new ResourceManager(typeof(Resource));

            return new RegionNode
            {
                Id = -1,
                Name = resourceManager.GetString(rootNodeTitleResource, new CultureInfo(Localization.LanguagePrimary)),
                NameLanguage1 = resourceManager.GetString(rootNodeTitleResource, new CultureInfo(Localization.Language1)),
                NameLanguage2 = resourceManager.GetString(rootNodeTitleResource, new CultureInfo(Localization.Language2)),
                RegionNodes = regionsLookup[null].Select(r => new RegionNode
                {
                    Id = r.Id,
                    Name = r.Name,
                    NameLanguage1 = r.NameLanguage1,
                    NameLanguage2 = r.NameLanguage2,
                    Code = r.Code,
                    RegionNodes = regionsLookup[r.Id].Select(ParseRegion).ToList()
                }).ToList()
            };
        }

        /// <summary>
        /// Метод создания региона
        /// </summary>
        /// <param name="data">Данные</param>
        /// <returns></returns>
        public async Task<Region> CreateAsync(RegionM data)
        {
            if (await _context.Regions.AnyAsync(x => x.Code == data.Code))
            {
                throw new BadRequestException(nameof(Resource.RegionAlreadyExistsError));
            }
            var region = new Region
            {
                Name = data.Name,
                Code = data.Code,
                AdminstrativeCenter = data.AdminstrativeCenter
            };
            _context.Add(region);
            await _context.SaveChangesAsync();
            return region;
        }

        /// <summary>
        /// Метод редактирования региона
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="data">Данные</param>
        /// <returns></returns>
        public async Task EditAsync(int id, RegionM data)
        {
            if (await _context.Regions.AnyAsync(x => x.Id != id && x.Code == data.Code))
            {
                throw new BadRequestException(nameof(Resource.RegionAlreadyExistsError));
            }
            var region = await GetAsync(id);
            region.Name = data.Name;
            region.Code = data.Code;
            region.AdminstrativeCenter = data.AdminstrativeCenter;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Метод переключения флага удалённости
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="delete">Флаг удалённости</param>
        /// <returns></returns>
        public async Task DeleteUndelete(int id, bool delete)
        {
            var region = await GetAsync(id);
            region.IsDeleted = delete;
            await _context.SaveChangesAsync();
        }
    }
}
