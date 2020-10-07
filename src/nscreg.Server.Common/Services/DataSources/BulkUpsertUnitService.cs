using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using nscreg.Data;
using nscreg.Data.Entities;
using nscreg.Resources.Languages;
using nscreg.Server.Common.Helpers;
using nscreg.Utilities.Extensions;
using Activity = nscreg.Data.Entities.Activity;

namespace nscreg.Server.Common.Services.DataSources
{
    public class BulkUpsertUnitService
    {
        private readonly NSCRegDbContext _dbContext;
        private readonly ElasticBulkService _elasticService;
        private readonly UpsertUnitBulkBuffer _bufferService;

        public BulkUpsertUnitService(NSCRegDbContext context, ElasticBulkService service, UpsertUnitBulkBuffer buffer)
        {
            _bufferService = buffer;
            _elasticService = service;
            _dbContext = context;
        }

        /// <summary>
        /// Creation of a local unit together with a legal unit, if there is none
        /// </summary>
        /// <param name="localUnit"></param>
        /// <returns></returns>
        public async Task CreateLocalUnit(LocalUnit localUnit)
        {
            try
            {
                //await _dbContext.LocalUnits.AddAsync(localUnit);
                //await _bufferService.AddToBufferAsync(localUnit);
            }
            catch (Exception e)
            {
                throw new BadRequestException(nameof(Resource.SaveError), e);
            }

            await _elasticService.AddDocument(Mapper.Map<IStatisticalUnit, ElasticStatUnit>(localUnit));
        }

        /// <summary>
        /// Creating a legal unit with a local unit and an enterprise
        /// </summary>
        /// <param name="legal"></param>
        /// <returns></returns>
        public async Task CreateLegalWithEnterpriseAndLocal(LegalUnit legal)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    Tracer.createStat.Start();
                    Tracer.createStat.Stop();
                    Debug.WriteLine($"Create legal {Tracer.createStat.ElapsedMilliseconds / ++Tracer.countcreateStat}");
                    if (legal.EnterpriseUnitRegId == null || legal.EnterpriseUnitRegId == 0)
                    {
                        Tracer.enterprise1.Start();
                        var sameStatIdEnterprise =
                            await _dbContext.EnterpriseUnits.FirstOrDefaultAsync(eu => eu.StatId == legal.StatId);
                        Tracer.enterprise1.Stop();
                        Debug.WriteLine(
                            $"Enterprise first or default {Tracer.enterprise1.ElapsedMilliseconds / ++Tracer.countenterprise1}");

                        if (sameStatIdEnterprise != null)
                        {
                            Tracer.enterprise2.Start();
                            legal.EnterpriseUnit = sameStatIdEnterprise;
                            Tracer.enterprise2.Stop();
                            Debug.WriteLine(
                                $"Enterprise link {Tracer.enterprise2.ElapsedMilliseconds / ++Tracer.countenterprise2}");
                        }
                        else
                        {
                            Tracer.enterprise3.Start();
                            CreateEnterpriseForLegalAsync(legal);
                            Tracer.enterprise3.Stop();
                            Debug.WriteLine(
                                $"Enterprise create {Tracer.enterprise3.ElapsedMilliseconds / ++Tracer.countenterprise3}");
                        }

                    }

                    Tracer.address.Start();
                    const double tolerance = 0.000000001;
                    var addressIds = legal.LocalUnits.Where(x => x.AddressId != null).Select(x => x.AddressId).ToList();
                    var addresses = await _dbContext.Address.Where(x => addressIds.Contains(x.Id) && x.RegionId == legal.Address.RegionId &&
                                                                        x.AddressPart1 == legal.Address.AddressPart1 &&
                                                                        x.AddressPart2 == legal.Address.AddressPart2 &&
                                                                        x.AddressPart3 == legal.Address.AddressPart3 && legal.Address.Latitude != null && Math.Abs((double)x.Latitude - (double)legal.Address.Latitude) < tolerance && legal.Address.Longitude != null && Math.Abs((double)x.Longitude - (double)legal.Address.Longitude) < tolerance).ToListAsync();
                    Tracer.address.Stop();
                    Debug.WriteLine($"Address {Tracer.address.ElapsedMilliseconds / ++Tracer.countaddress}");
                    if (!addresses.Any())
                    {
                        Tracer.localForLegal.Start();
                        CreateLocalForLegalAsync(legal);
                        Tracer.localForLegal.Stop();
                        Debug.WriteLine(
                            $"Local for legal create {Tracer.localForLegal.ElapsedMilliseconds / ++Tracer.countlocalForLegal}");
                    }
                    await _bufferService.AddLegalToBufferAsync(legal);
                    transaction.Commit();
                    //TODO: History for bulk
                    //var legalsOfEnterprise = await _dbContext.LegalUnits.Where(leu => leu.RegId == legalUnit.EnterpriseUnitRegId)
                    //    .Select(x => x.RegId).ToListAsync();
                    //legalUnit.EnterpriseUnit.HistoryLegalUnitIds += string.Join(",", legalsOfEnterprise);
                    //Tracer.commit2.Start();

                    //_dbContext.EnterpriseUnits.Update(legalUnit.EnterpriseUnit);

                    //legalUnit.HistoryLocalUnitIds = createdLocal?.RegId.ToString();
                    //_dbContext.LegalUnits.Update(legalUnit);

                    ////await _dbContext.SaveChangesAsync();
                    //Tracer.commit2.Stop();
                    //Debug.WriteLine($"History {Tracer.commit2.ElapsedMilliseconds / ++Tracer.countcommit2}");
                }
                catch (Exception e)
                {
                    throw new BadRequestException(nameof(Resource.SaveError), e);
                }

                //TODO: Вынести во Flush потому что некоторые поля до Flush null и не присвоены
                //Tracer.elastic.Start();
                //await _elasticService.AddDocument(Mapper.Map<IStatisticalUnit, ElasticStatUnit>(legal));
                //if (createdLocal != null)
                //    await _elasticService.AddDocument(Mapper.Map<IStatisticalUnit, ElasticStatUnit>(createdLocal));
                //if (createdEnterprise != null)
                //    await _elasticService.AddDocument(Mapper.Map<IStatisticalUnit, ElasticStatUnit>(createdEnterprise));
                //Tracer.elastic.Stop();
                //Debug.WriteLine($"Elastic {Tracer.elastic.ElapsedMilliseconds / ++Tracer.countelastic}\n\n");

            }

        }

        /// <summary>
        /// Creating an enterprise with a group of enterprises
        /// </summary>
        /// <param name = "enterpriseUnit" ></param >
        /// < returns ></returns >
        public async Task CreateEnterpriseWithGroup(EnterpriseUnit enterpriseUnit)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    _dbContext.EnterpriseUnits.Add(enterpriseUnit);
                    if (enterpriseUnit.EntGroupId == null || enterpriseUnit.EntGroupId <= 0)
                    {

                        CreateGroupForEnterpriseAsync(enterpriseUnit);
                        var sameStatIdLegalUnits = await _dbContext.LegalUnits.Where(leu => leu.StatId == enterpriseUnit.StatId).ToListAsync();
                        foreach (var legalUnit in sameStatIdLegalUnits)
                        {
                            legalUnit.EnterpriseUnit = enterpriseUnit;
                        }
                        enterpriseUnit.HistoryLegalUnitIds = string.Join(",", sameStatIdLegalUnits.Select(x => x.RegId));

                    }
                    //await _bufferService.AddLegalToBufferAsync(enterpriseUnit);
                    //await _dbContext.SaveChangesAsync();
                    //TODO Расследовать и поменять условия Where
                    //var legalsOfEnterprise = await _dbContext.LegalUnits.Where(leu => leu.RegId == createdUnit.RegId)
                    //    .Select(x => x.RegId).ToListAsync();
                    //createdUnit.HistoryLegalUnitIds = string.Join(",", legalsOfEnterprise);

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    throw new BadRequestException(nameof(Resource.SaveError), e);
                }
            }

            await _elasticService.AddDocument(Mapper.Map<IStatisticalUnit, ElasticStatUnit>(enterpriseUnit));
            if (enterpriseUnit.EnterpriseGroup != null)
                await _elasticService.AddDocument(Mapper.Map<IStatisticalUnit, ElasticStatUnit>(enterpriseUnit.EnterpriseGroup));
        }


        private void CreateEnterpriseForLegalAsync(LegalUnit legalUnit)
        {
            var enterpriseUnit = new EnterpriseUnit();
            Mapper.Map(legalUnit, enterpriseUnit);
            legalUnit.EnterpriseUnit = enterpriseUnit;
            CreateActivitiesAndPersonsAndForeignParticipations(legalUnit.Activities, legalUnit.PersonsUnits, legalUnit.ForeignParticipationCountriesUnits, enterpriseUnit);
        }

        //TODO вынести в Bulk
        private void CreateActivitiesAndPersonsAndForeignParticipations(IEnumerable<Activity> activities, IEnumerable<PersonStatisticalUnit> persons, IEnumerable<CountryStatisticalUnit> foreignPartCountries, StatisticalUnit unit)
        {
            activities.ForEach(x =>
            {
                _dbContext.ActivityStatisticalUnits.Add(new ActivityStatisticalUnit
                {
                    ActivityId = x.Id,
                    Unit = unit
                });
            });
            persons.ForEach(x =>
            {
                _dbContext.PersonStatisticalUnits.Add(new PersonStatisticalUnit
                {
                    PersonId = x.PersonId,
                    Unit = unit,
                    PersonTypeId = x.PersonTypeId,
                    EnterpriseGroupId = x.EnterpriseGroupId
                });
            });

            foreignPartCountries.ForEach(x =>
            {
                _dbContext.CountryStatisticalUnits.Add(new CountryStatisticalUnit
                {
                    Unit = unit,
                    CountryId = x.CountryId
                });

            });

        }
        private void CreateLocalForLegalAsync(LegalUnit legalUnit)
        {
            var localUnit = new LocalUnit();
            Mapper.Map(legalUnit, localUnit);
            legalUnit.LocalUnits.Add(localUnit);
            CreateActivitiesAndPersonsAndForeignParticipations(legalUnit.Activities, legalUnit.PersonsUnits, legalUnit.ForeignParticipationCountriesUnits, localUnit);
        }
        private void CreateGroupForEnterpriseAsync(EnterpriseUnit enterpriseUnit)
        {
            var enterpriseGroup = new EnterpriseGroup();
            Mapper.Map(enterpriseUnit, enterpriseGroup);
            enterpriseUnit.EnterpriseGroup = enterpriseGroup;
        }
    }
}
