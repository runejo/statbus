﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using nscreg.Data;
using nscreg.Data.Constants;
using nscreg.Data.Entities;
using nscreg.ReadStack;
using nscreg.Server.Core;
using nscreg.Server.Models.StatUnits;
using nscreg.Server.Models.StatUnits.Create;
using nscreg.Server.Models.StatUnits.Edit;
using System;
using System.Collections.Generic;
using System.Linq;
using nscreg.Resources.Languages;

namespace nscreg.Server.Services
{
    public class StatUnitService
    {
        private readonly Dictionary<StatUnitTypes, Action<int, bool>> _deleteUndeleteActions;
        private readonly NSCRegDbContext _dbContext;
        private readonly ReadContext _readCtx;

        public StatUnitService(NSCRegDbContext dbContext)
        {
            _dbContext = dbContext;
            _readCtx = new ReadContext(dbContext);
            _deleteUndeleteActions = new Dictionary<StatUnitTypes, Action<int, bool>>
            {
                {StatUnitTypes.EnterpriseGroup, DeleteUndeleteEnterpriseGroupUnit},
                {StatUnitTypes.EnterpriseUnit, DeleteUndeleteStatisticalUnit},
                {StatUnitTypes.LocalUnit, DeleteUndeleteStatisticalUnit},
                {StatUnitTypes.LegalUnit, DeleteUndeleteStatisticalUnit}
            };
        }

        #region SEARCH

        public SearchVm Search(SearchQueryM query, IEnumerable<string> propNames)
        {
            var unit =
                _readCtx.StatUnits.Where(x => (query.IncludeLiquidated || x.LiqReason == null))
                    .Select(
                        x =>
                            new
                            {
                                x.RegId,
                                x.Name,
                                x.Address,
                                x.Turnover,
                                UnitType =
                                x is LocalUnit
                                    ? StatUnitTypes.LocalUnit
                                    : x is LegalUnit ? StatUnitTypes.LegalUnit : StatUnitTypes.EnterpriseUnit
                            });
            var group =
                _readCtx.EnterpriseGroups.Where(x => (query.IncludeLiquidated || x.LiqReason == null))
                    .Select(x => new {x.RegId, x.Name, x.Address, x.Turnover, UnitType = StatUnitTypes.EnterpriseGroup});
            var filtered = unit.Union(group);

            if (!string.IsNullOrEmpty(query.Wildcard))
            {
                Predicate<string> checkWildcard =
                    superStr => !string.IsNullOrEmpty(superStr) && superStr.Contains(query.Wildcard);
                filtered = filtered.Where(x =>
                    x.Name.Contains(query.Wildcard)
                    || checkWildcard(x.Address.AddressPart1)
                    || checkWildcard(x.Address.AddressPart2)
                    || checkWildcard(x.Address.AddressPart3)
                    || checkWildcard(x.Address.AddressPart4)
                    || checkWildcard(x.Address.AddressPart5)
                    || checkWildcard(x.Address.GeographicalCodes));
            }

            if (query.Type.HasValue)
                filtered = filtered.Where(x => x.UnitType == query.Type.Value);

            if (query.TurnoverFrom.HasValue)
                filtered = filtered.Where(x => x.Turnover > query.TurnoverFrom);

            if (query.TurnoverTo.HasValue)
                filtered = filtered.Where(x => x.Turnover < query.TurnoverTo);

            var ids = filtered.Select(x => x.RegId);

            var result = ids
                .Skip(query.PageSize * query.Page)
                .Take(query.PageSize)
                .ToArray();

            var total = ids.Count();

            var unitList = new List<object>();

            foreach (var finalUnit in filtered)
            {
                unitList.Add(SearchItemVm.Create(finalUnit, finalUnit.UnitType, propNames));
            }

            return SearchVm.Create(
                result != null
                    ? unitList.ToArray()
                    : Array.Empty<object>(),
                total,
                (int) Math.Ceiling((double) total / query.PageSize));
        }

        #endregion

        #region VIEW

        internal object GetUnitById(int id, string[] propNames)
        {
            var item = GetNotDeletedStatisticalUnitById(id);
            return SearchItemVm.Create(item, item.UnitType, propNames);
        }

        private IStatisticalUnit GetNotDeletedStatisticalUnitById(int id)
            => _dbContext.StatisticalUnits.Where(x => !x.IsDeleted).First(x => x.RegId == id);

        #endregion

        #region DELETE

        public void DeleteUndelete(StatUnitTypes unitType, int id, bool toDelete)
        {
            _deleteUndeleteActions[unitType](id, toDelete);
        }


        private void DeleteUndeleteEnterpriseGroupUnit(int id, bool toDelete)
        {
            _dbContext.EnterpriseGroups.Find(id).IsDeleted = toDelete;
            _dbContext.SaveChanges();
        }

        private void DeleteUndeleteStatisticalUnit(int id, bool toDelete)
        {
            _dbContext.StatisticalUnits.Find(id).IsDeleted = toDelete;
            _dbContext.SaveChanges();
        }

        #endregion

        #region CREATE


        public void Create<TModel, TDomain>(TModel data)
            where TModel : IStatUnitM
            where TDomain : class, IStatisticalUnit

        {
            var errorMap = new Dictionary<Type, string>
            {
                {typeof(LegalUnit), nameof(Resource.CreateLegalUnitError)},
                {typeof(LocalUnit), nameof(Resource.CreateLocalUnitError)},
                {typeof(EnterpriseUnit), nameof(Resource.CreateEnterpriseUnitError)},
                {typeof(EnterpriseGroup), nameof(Resource.CreateEnterpriseGroupError)}
            };
            var unit = Mapper.Map<TModel, TDomain>(data);
            AddAddresses(unit, data);
            if (!NameAddressIsUnique<TDomain>(data.Name, data.Address, data.ActualAddress))
                throw new BadRequestException($"{nameof(Resource.AddressExcistsInDataBaseForError)} {data.Name}", null);
            _dbContext.Set<TDomain>().Add(unit);
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                throw new BadRequestException(errorMap[typeof(TDomain)], e);
            }
        } 


        public void CreateLegalUnit(LegalUnitCreateM data)
        {
            var unit = Mapper.Map<LegalUnitCreateM, LegalUnit>(data);
            AddAddresses(unit, data);
            if (!NameAddressIsUnique<LegalUnit>(data.Name, data.Address, data.ActualAddress))
                throw new BadRequestException($"{nameof(Resource.AddressExcistsInDataBaseForError)} {data.Name}", null);
            _dbContext.LegalUnits.Add(unit);
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                throw new BadRequestException(nameof(Resource.CreateLegalUnitError), e);
            }
        }

        public void CreateLocalUnit(LocalUnitCreateM data)
        {
            var unit = Mapper.Map<LocalUnitCreateM, LocalUnit>(data);
            AddAddresses(unit, data);
            if (!NameAddressIsUnique<LocalUnit>(data.Name, data.Address, data.ActualAddress))
                throw new BadRequestException($"{nameof(Resource.AddressExcistsInDataBaseForError)} {data.Name}", null);
            _dbContext.LocalUnits.Add(unit);
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                throw new BadRequestException(nameof(Resource.CreateLocalUnitError), e);
            }
        }

        public void CreateEnterpriseUnit(EnterpriseUnitCreateM data)
        {
            var unit = Mapper.Map<EnterpriseUnitCreateM, EnterpriseUnit>(data);
            AddAddresses(unit, data);
            if (!NameAddressIsUnique<EnterpriseUnit>(data.Name, data.Address, data.ActualAddress))
                throw new BadRequestException($"{nameof(Resource.AddressExcistsInDataBaseForError)} {data.Name}", null);
            _dbContext.EnterpriseUnits.Add(unit);
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                throw new BadRequestException(nameof(Resource.CreateEnterpriseUnitError), e);
            }
        }

        public void CreateEnterpriseGroupUnit(EnterpriseGroupCreateM data)
        {
            var unit = Mapper.Map<EnterpriseGroupCreateM, EnterpriseGroup>(data);
            AddAddresses(unit, data);
            if (!NameAddressIsUnique<EnterpriseGroup>(data.Name, data.Address, data.ActualAddress))
                throw new BadRequestException($"{nameof(Resource.AddressExcistsInDataBaseForError)} {data.Name}", null);
            _dbContext.EnterpriseGroups.Add(unit);
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                throw new BadRequestException(nameof(Resource.CreateEnterpriseGroupError), e);
            }
        }

        #endregion

        #region EDIT

        public void EditLegalUnit(LegalUnitEditM data)
        {
            var unit = (LegalUnit) ValidateChanges<LegalUnit>(data, data.RegId);
            if (unit == null) throw new ArgumentNullException(nameof(unit));
            Mapper.Map(data, unit);
            AddAddresses(unit, data);
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                throw new BadRequestException(nameof(Resource.UpdateLegalUnitError), e);
            }
        }

        public void EditLocalUnit(LocalUnitEditM data)
        {
            var unit = (LocalUnit) ValidateChanges<LocalUnit>(data, data.RegId);
            if (unit == null) throw new ArgumentNullException(nameof(unit));
            Mapper.Map(data, unit);
            AddAddresses(unit, data);
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                throw new BadRequestException(nameof(Resource.UpdateLocalUnitError), e);
            }
        }

        public void EditEnterpiseUnit(EnterpriseUnitEditM data)
        {
            var unit = (EnterpriseUnit) ValidateChanges<EnterpriseUnit>(data, data.RegId);
            if (unit == null) throw new ArgumentNullException(nameof(unit));
            Mapper.Map(data, unit);
            AddAddresses(unit, data);
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                throw new BadRequestException(nameof(Resource.UpdateEnterpriseUnitError), e);
            }
        }

        public void EditEnterpiseGroup(EnterpriseGroupEditM data)
        {
            var unit = (EnterpriseGroup) ValidateChanges<EnterpriseGroup>(data, data.RegId);
            if (unit == null) throw new ArgumentNullException(nameof(unit));
            Mapper.Map(data, unit);
            AddAddresses(unit, data);
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                throw new BadRequestException(nameof(Resource.UpdateEnterpriseGroupError), e);
            }
        }

        #endregion

        private void AddAddresses(IStatisticalUnit unit, IStatUnitM data)
        {
            if ((data.Address != null) && (!data.Address.IsEmpty()))
                unit.Address = GetAddress(data.Address);
            else unit.Address = null;
            if ((data.ActualAddress != null) && (!data.ActualAddress.IsEmpty()))
                unit.ActualAddress = data.ActualAddress.Equals(data.Address)
                    ? unit.Address
                    : GetAddress(data.ActualAddress);
            else unit.ActualAddress = null;
        }

        private Address GetAddress(AddressM data)
        {
            return _dbContext.Address.SingleOrDefault(a
                       => a.AddressPart1 == data.AddressPart1 &&
                          a.AddressPart2 == data.AddressPart2 &&
                          a.AddressPart3 == data.AddressPart3 &&
                          a.AddressPart4 == data.AddressPart4 &&
                          a.AddressPart5 == data.AddressPart5 &&
                          a.GpsCoordinates == data.GpsCoordinates)
                   ?? new Address()
                   {
                       AddressPart1 = data.AddressPart1,
                       AddressPart2 = data.AddressPart2,
                       AddressPart3 = data.AddressPart3,
                       AddressPart4 = data.AddressPart4,
                       AddressPart5 = data.AddressPart5,
                       GeographicalCodes = data.GeographicalCodes,
                       GpsCoordinates = data.GpsCoordinates
                   };
        }

        private bool NameAddressIsUnique<T>(string name, AddressM address, AddressM actualAddress)
            where T : class, IStatisticalUnit
        {
            if (address == null) address = new AddressM();
            if (actualAddress == null) actualAddress = new AddressM();
            var units =
                _dbContext.Set<T>()
                    .Include(a => a.Address)
                    .Include(aa => aa.ActualAddress)
                    .ToList()
                    .Where(u => u.Name == name);
            return
                units.All(
                    unit =>
                        (!address.Equals(unit.Address) && !actualAddress.Equals(unit.ActualAddress)));
        }

        private IStatisticalUnit ValidateChanges<T>(IStatUnitM data, int? regid)
            where T : class, IStatisticalUnit
        {
            var unit = _dbContext.Set<T>().Include(a => a.Address)
                .Include(aa => aa.ActualAddress)
                .Single(x => x.RegId == regid);

            if (!unit.Name.Equals(data.Name) &&
                !NameAddressIsUnique<T>(data.Name, data.Address, data.ActualAddress))
                throw new BadRequestException(
                    $"{typeof(T).Name} {nameof(Resource.AddressExcistsInDataBaseForError)} {data.Name}", null);
            else if (data.Address != null && data.ActualAddress != null && !data.Address.Equals(unit.Address) &&
                     !data.ActualAddress.Equals(unit.ActualAddress) &&
                     !NameAddressIsUnique<T>(data.Name, data.Address, data.ActualAddress))
                throw new BadRequestException(
                    $"{typeof(T).Name} {nameof(Resource.AddressExcistsInDataBaseForError)} {data.Name}", null);
            else if (data.Address != null && !data.Address.Equals(unit.Address) &&
                     !NameAddressIsUnique<T>(data.Name, data.Address, null))
                throw new BadRequestException(
                    $"{typeof(T).Name} {nameof(Resource.AddressExcistsInDataBaseForError)} {data.Name}", null);
            else if (data.ActualAddress != null && !data.ActualAddress.Equals(unit.ActualAddress) &&
                     !NameAddressIsUnique<T>(data.Name, null, data.ActualAddress))
                throw new BadRequestException(
                    $"{typeof(T).Name} {nameof(Resource.AddressExcistsInDataBaseForError)} {data.Name}", null);

            return unit;
        }
    }
}