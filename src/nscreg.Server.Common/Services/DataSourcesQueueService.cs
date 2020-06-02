using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using nscreg.Data;
using nscreg.Data.Constants;
using nscreg.Data.Entities;
using nscreg.Resources.Languages;
using nscreg.Server.Common.Models;
using nscreg.Server.Common.Models.DataSourcesQueue;
using nscreg.Server.Common.Services.StatUnit;
using nscreg.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using nscreg.Server.Common.Helpers;
using nscreg.Server.Common.Models.StatUnits;
using nscreg.Server.Common.Models.StatUnits.Create;
using nscreg.Server.Common.Models.StatUnits.Edit;
using nscreg.Utilities;
using nscreg.Utilities.Configuration;
using nscreg.Utilities.Configuration.DBMandatoryFields;
using nscreg.Utilities.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Activity = nscreg.Data.Entities.Activity;
using LegalUnit = nscreg.Data.Entities.LegalUnit;
using LocalUnit = nscreg.Data.Entities.LocalUnit;
using SearchQueryM = nscreg.Server.Common.Models.DataSourcesQueue.SearchQueryM;

namespace nscreg.Server.Common.Services
{
    public class DataSourcesQueueService
    {
        private readonly NSCRegDbContext _dbContext;
        private readonly CreateService _createSvc;
        private readonly EditService _editSvc;
        private readonly string _rootPath;
        private readonly string _uploadDir;
        private readonly DbMandatoryFields _dbMandatoryFields;
        private DeleteService _statUnitDeleteService;
        private readonly ElasticService _elasticService;

        public DataSourcesQueueService(NSCRegDbContext ctx,
            CreateService createSvc,
            EditService editSvc,
            ServicesSettings config,
            DbMandatoryFields dbMandatoryFields)
        {
            _dbContext = ctx;
            _createSvc = createSvc;
            _editSvc = editSvc;
            _rootPath = config.RootPath;
            _uploadDir = config.UploadDir;
            _dbMandatoryFields = dbMandatoryFields;
            _statUnitDeleteService = new DeleteService(ctx);
            _elasticService = new ElasticService(ctx);
        }

        public async Task<SearchVm<QueueVm>> GetAllDataSourceQueues(SearchQueryM query)
        {
            var sortBy = string.IsNullOrEmpty(query.SortBy)
                ? "Id"
                : query.SortBy;

            var orderRule = query.OrderByValue == OrderRule.Asc && !string.IsNullOrEmpty(query.SortBy)
                ? "ASC"
                : "DESC";

            var filtered = _dbContext.DataSourceQueues
                .Include(x => x.DataSource)
                .Include(x => x.User)
                .AsNoTracking();

            if (query.Status.HasValue)
                filtered = filtered.Where(x => x.Status == query.Status.Value);

            if (query.DateFrom.HasValue && query.DateTo.HasValue)
            {
                filtered = filtered.Where(x => x.StartImportDate >= query.DateFrom.Value &&
                                               x.StartImportDate <= query.DateTo.Value);
            }
            else
            {
                if (query.DateFrom.HasValue)
                    filtered = filtered.Where(x => x.StartImportDate >= query.DateFrom.Value);

                if (query.DateTo.HasValue)
                    filtered = filtered.Where(x => x.StartImportDate <= query.DateTo.Value);
            }

            filtered = filtered.OrderBy($"{sortBy} {orderRule}");

            var total = await filtered.CountAsync();

            var result = await filtered
                .Skip(Pagination.CalculateSkip(query.PageSize, query.Page, total))
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync();

            return SearchVm<QueueVm>.Create(result.Select(QueueVm.Create), total);
        }

        public async Task<SearchVm<QueueLogVm>> GetQueueLog(int queueId, PaginatedQueryM query)
        {
            var queue = await _dbContext.DataSourceQueues.Include(x => x.DataSource).FirstAsync(x => x.Id == queueId);
            switch (queue.DataSource.DataSourceUploadType)
            {
                case DataSourceUploadTypes.StatUnits:
                    return await GetQueueLogForStatUnitUpload(queueId, query);
                case DataSourceUploadTypes.Activities:
                    return await GetQueueLogForActivityUpload(queueId, query);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task<SearchVm<QueueLogVm>> GetQueueLogForActivityUpload(int queueId, PaginatedQueryM query)
        {
            var orderBy = string.IsNullOrEmpty(query.SortBy) ? nameof(DataUploadingLog.Id) : query.SortBy;
            var orderRule = query.SortAscending ? "ASC" : "DESC";
            var filtered = _dbContext.DataUploadingLogs
                .Where(x => x.DataSourceQueueId == queueId)
                .OrderBy($"{orderBy} {orderRule}")
                .GroupBy(x => x.TargetStatId);
            var total = await filtered.CountAsync();

            var result = (await filtered
                    .Skip(Pagination.CalculateSkip(query.PageSize, query.Page, total))
                    .Take(query.PageSize)
                    .AsNoTracking()
                    .ToListAsync())
                .Select(x => new DataUploadingLog
                {
                    DataSourceQueue = x.FirstOrDefault()?.DataSourceQueue,
                    EndImportDate = x.Select(y => y.EndImportDate).Max(),
                    StartImportDate = x.Select(y => y.StartImportDate).Max(),
                    TargetStatId = x.FirstOrDefault()?.TargetStatId,
                    StatUnitName = x.FirstOrDefault()?.StatUnitName,
                    Status = x.Any(y => y.Status == DataUploadingLogStatuses.Error)
                        ? DataUploadingLogStatuses.Error
                        : x.Any(y => y.Status == DataUploadingLogStatuses.Warning)
                            ? DataUploadingLogStatuses.Warning
                            : DataUploadingLogStatuses.Done,
                    Note = x.FirstOrDefault()?.Note,
                });

            return SearchVm<QueueLogVm>.Create(result.Select(QueueLogVm.Create), total);
        }

        private async Task<SearchVm<QueueLogVm>> GetQueueLogForStatUnitUpload(int queueId, PaginatedQueryM query)
        {
            var orderBy = string.IsNullOrEmpty(query.SortBy) ? nameof(DataUploadingLog.Id) : query.SortBy;
            var orderRule = query.SortAscending ? "ASC" : "DESC";
            var filtered = _dbContext.DataUploadingLogs
                .Where(x => x.DataSourceQueueId == queueId)
                .OrderBy($"{orderBy} {orderRule}");

            var total = await filtered.CountAsync();

            var result = await filtered
                .Skip(Pagination.CalculateSkip(query.PageSize, query.Page, total))
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync();

            return SearchVm<QueueLogVm>.Create(result.Select(QueueLogVm.Create), total);
        }

        public async Task<QueueLogDetailsVm> GetLogDetails(int logId)
        {
            var logEntry = await _dbContext.DataUploadingLogs
                .Include(x => x.DataSourceQueue)
                .ThenInclude(x => x.DataSource)
                .FirstOrDefaultAsync(x => x.Id == logId);

            if (logEntry == null)
            {
                throw new NotFoundException(nameof(Resource.NotFoundMessage));
            }

            var metadata = await new ViewService(_dbContext, _dbMandatoryFields).GetViewModel(
                null,
                logEntry.DataSourceQueue.DataSource.StatUnitType,
                logEntry.DataSourceQueue.UserId,
                ActionsEnum.Edit);

            return QueueLogDetailsVm.Create(
                logEntry,
                metadata.StatUnitType,
                metadata.Properties,
                metadata.Permissions);
        }

        public async Task CreateAsync(IFormFileCollection files, UploadQueueItemVm data, string userId)
        {
            var today = DateTime.Now;
            var path = Path.Combine(
                Path.GetFullPath(_rootPath),
                _uploadDir,
                today.Year.ToString(),
                today.Month.ToString(),
                today.Day.ToString());
            try
            {
                Directory.CreateDirectory(path);
                foreach (var file in files)
                {
                    var filePath = Path.Combine(path, Guid.NewGuid().ToString());
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                        _dbContext.DataSourceQueues.Add(new DataSourceQueue
                        {
                            UserId = userId,
                            DataSourcePath = filePath,
                            DataSourceFileName = file.FileName,
                            DataSourceId = data.DataSourceId,
                            Description = data.Description,
                            StartImportDate = today,
                            EndImportDate = DateTime.Now,
                            Status = DataSourceQueueStatuses.InQueue,
                        });
                    }
                }
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new BadRequestException(nameof(Resource.CantStoreFile), e);
            }
        }

        public async Task<Dictionary<string, string[]>> UpdateLog(int logId, string data, string userId)
        {
            var logEntry = await _dbContext.DataUploadingLogs
                .Include(l => l.DataSourceQueue)
                .ThenInclude(q => q.DataSource)
                .FirstOrDefaultAsync(l => l.Id == logId);
            if (logEntry == null) throw new BadRequestException(nameof(Resource.UploadLogNotFound));

            var type = logEntry.DataSourceQueue.DataSource.StatUnitType;
            var definitionWithRegId = new {RegId = 0};
            var hasId = JsonConvert.DeserializeAnonymousType(data, definitionWithRegId).RegId > 0;

            Task<Dictionary<string, string[]>> task;
            switch (type)
            {
                case StatUnitTypes.LocalUnit:
                    task = hasId
                        ? _editSvc.EditLocalUnit(ParseModel<LocalUnitEditM>(), userId)
                        : _createSvc.CreateLocalUnit(ParseModel<LocalUnitCreateM>(), userId);
                    break;
                case StatUnitTypes.LegalUnit:
                    task = hasId
                        ? _editSvc.EditLegalUnit(ParseModel<LegalUnitEditM>(), userId)
                        : _createSvc.CreateLegalUnit(ParseModel<LegalUnitCreateM>(), userId);
                    break;
                case StatUnitTypes.EnterpriseUnit:
                    task = hasId
                        ? _editSvc.EditEnterpriseUnit(ParseModel<EnterpriseUnitEditM>(), userId)
                        : _createSvc.CreateEnterpriseUnit(ParseModel<EnterpriseUnitCreateM>(), userId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        $"Parameter `{nameof(data)}`: value of type `{type.ToString()}` is not supported.");
            }

            var errors = await task;
            if (errors != null && errors.Any()) return errors;

            logEntry.Status = DataUploadingLogStatuses.Done;
            await _dbContext.SaveChangesAsync();

            return null;

            T ParseModel<T>() where T : StatUnitModelBase
            {
                var result = JsonConvert.DeserializeObject<T>(data);
                result.DataSource = logEntry.DataSourceQueue.DataSource.Name;
                return result;
            }
        }

        public async Task<IEnumerable<Activity>> GetActivityLogDetailsByStatId(int queueId, string statId)
        {
            var logEntries = await _dbContext.DataUploadingLogs
                .Where(x => x.DataSourceQueueId == queueId)
                .Where(x => x.TargetStatId == statId)
                .ToListAsync();
            var queue = await _dbContext.DataSourceQueues
                .Include(x => x.DataSource)
                .FirstAsync(x => x.Id == queueId);

            var deserializeMap = new Dictionary<StatUnitTypes, Func<string, StatisticalUnit>>()
            {
                [StatUnitTypes.LegalUnit] = JsonConvert.DeserializeObject<LegalUnit>,
                [StatUnitTypes.LocalUnit] = JsonConvert.DeserializeObject<LocalUnit>,
                [StatUnitTypes.EnterpriseUnit] = JsonConvert.DeserializeObject<EnterpriseUnit>

            };
            var activities = logEntries
                .Select(x => x.SerializedUnit)
                .Select(deserializeMap[queue.DataSource.StatUnitType])
                .Select(x => x.ActivitiesUnits.First().Activity);

           return activities;
        }

        /// <summary>
        /// Checks other logs of data source queue
        /// </summary>
        /// <param name="queueId">Id of data source queue</param>
        private bool QueueLogsExist(int queueId)
        {
            var existing = _dbContext.DataUploadingLogs.FirstOrDefault(log => log.DataSourceQueueId == queueId);
            return existing != null;
        }

        /// <summary>
        /// Data source queue delete method from db
        /// </summary>
        /// <param name="queueId">Id of data source queue</param>
        public async Task DeleteQueueById(int queueId)
        {
            var existing = await _dbContext.DataSourceQueues.FindAsync(queueId);
            if (existing == null) throw new NotFoundException(nameof(Resource.DataSourceQueueNotFound));
            _dbContext.DataSourceQueues.Remove(existing);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Log delete method with clearing statistical units
        /// </summary>
        /// <param name="logId">Id of log</param>
        /// <param name="userId">Id of user</param>
        public async Task DeleteLogById(int logId, string userId)
        {
            var existing =  await _dbContext.DataUploadingLogs.FindAsync(logId);
            if (existing == null) throw new NotFoundException(nameof(Resource.QueueLogNotFound));

            if (existing.SerializedUnit != null)
            {
                dynamic jsonParsed = JsonConvert.DeserializeObject(existing.SerializedUnit);
                int unitType = int.Parse(jsonParsed["unitType"].ToString());

                if (existing.Status == DataUploadingLogStatuses.Done &&
                    existing.StartImportDate != null)
                {
                    var unitTypes = GetUnitTypes(existing.TargetStatId, (StatUnitTypes)unitType);
                    switch (unitType)
                    {
                        case (int)StatUnitTypes.LocalUnit:
                            await _statUnitDeleteService.DeleteLocalUnitFromDb(existing.TargetStatId, userId, existing.StartImportDate);
                            break;
                        case (int)StatUnitTypes.LegalUnit:
                            await _statUnitDeleteService.DeleteLegalUnitFromDb(existing.TargetStatId, userId, existing.StartImportDate);
                            break;
                        case (int)StatUnitTypes.EnterpriseUnit:
                            await _statUnitDeleteService.DeleteEnterpriseUnitFromDb(existing.TargetStatId, userId, existing.StartImportDate);
                            break;
                        default:
                            throw new NotFoundException(nameof(Resource.StatUnitTypeNotFound));
                    }
                }
            }

            _dbContext.DataUploadingLogs.Remove(existing);
            await _dbContext.SaveChangesAsync();
        }

        private List<StatUnitTypes> GetUnitTypes(string statId, StatUnitTypes statUnitType)
        {
            var statUnitTypes = new List<StatUnitTypes>();
            var isContainDataSource = _dbContext.StatisticalUnits.AsNoTracking().First(x => x.StatId == statId && x.UnitType == statUnitType).DataSource != null;
            if (!isContainDataSource)
            {
                statUnitTypes.Add(statUnitType);
            }
            else
            {
                statUnitTypes.AddRange(_dbContext.StatisticalUnits.AsNoTracking().Where(x=>x.StatId == statId && x.DataSource != null).Select(x=>x.UnitType).ToList());
            }

            return statUnitTypes;
        }

        /// <summary>
        /// Log delete method, if data queue hasn`t other logs, this method will delete data queue too
        /// </summary>
        /// <param name="logId">Id of log</param>
        /// <param name="userId">Id of user</param>
        public async Task DeleteLog(int logId, string userId)
        {
            var existing = await _dbContext.DataUploadingLogs.FindAsync(logId);
            if (existing == null) throw new NotFoundException(nameof(Resource.QueueLogNotFound));
            var queueId = existing.DataSourceQueueId;
            await DeleteLogById(existing.Id, userId);
            if (!QueueLogsExist(queueId))
                await DeleteQueueById(queueId);
        }

        /// <summary>
        /// Data source queue delete method
        /// </summary>
        /// <param name="queueId">Id of data source queue</param>
        /// <param name="userId">Id of user</param>
        public async Task DeleteQueue(int queueId, string userId)
        {
            var existing = await _dbContext.DataSourceQueues.FindAsync(queueId);
            if (existing == null) throw new NotFoundException(nameof(Resource.DataSourceQueueNotFound));
            var logs = _dbContext.DataUploadingLogs.Where(log => log.DataSourceQueueId == existing.Id).ToArray();
            if (logs.Length > 0)
                await logs.ForEachAsync(log => DeleteLogById(log.Id, userId));
            await DeleteQueueById(existing.Id);
        }
    }
}
