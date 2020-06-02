using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using nscreg.Data.Constants;
using nscreg.Data.Entities;
using nscreg.Data.Entities.ComplexTypes;
using nscreg.Data.Entities.History;
using nscreg.Server.Common.Models.ActivityCategories;
using nscreg.Server.Common.Models.Addresses;
using nscreg.Server.Common.Models.AnalysisQueue;
using nscreg.Server.Common.Models.DataAccess;
using nscreg.Server.Common.Models.Lookup;
using nscreg.Server.Common.Models.Regions;
using nscreg.Server.Common.Models.SampleFrames;
using nscreg.Server.Common.Models.StatUnits;
using nscreg.Server.Common.Models.StatUnits.Create;
using nscreg.Server.Common.Models.StatUnits.Edit;
using nscreg.Server.Common.Models.StatUnits.Search;
using nscreg.Server.Common.Services;
using nscreg.Utilities;
using nscreg.Utilities.Enums;

namespace nscreg.Server.Common
{
    /// <summary>
    /// Auto-match profile class
    /// </summary>
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            DataAccessCondition(CreateStatUnitFromModelMap<LegalUnitCreateM, LegalUnit>()
                .ForMember(x => x.LocalUnits, x => x.Ignore()));
            CreateMap<LegalUnit, LegalUnitCreateM>()
                .ForMember(x => x.LocalUnits, x => x.Ignore())
                .ForMember(x => x.ForeignParticipationCountriesUnits, opt => opt.MapFrom(src => src.ForeignParticipationCountriesUnits.Select(x => x.Id)));

            DataAccessCondition(CreateStatUnitFromModelMap<LocalUnitCreateM, LocalUnit>());
            CreateStatUnitFromModelReverseMap<LocalUnit, LocalUnitCreateM>();

            DataAccessCondition(CreateStatUnitFromModelMap<EnterpriseUnitCreateM, EnterpriseUnit>()
                .ForMember(x => x.LegalUnits, x => x.Ignore()));
            CreateStatUnitFromModelReverseMap<EnterpriseUnit, EnterpriseUnitCreateM>();

            DataAccessCondition(CreateMap<EnterpriseGroupCreateM, EnterpriseGroup>(MemberList.None)
                .ForMember(x => x.ChangeReason, x => x.UseValue(ChangeReasons.Create))
                .ForMember(x => x.StartPeriod, x => x.MapFrom(v => DateTime.Now))
                .ForMember(x => x.EndPeriod, x => x.UseValue(DateTime.MaxValue))
                .ForMember(x => x.RegIdDate, x => x.MapFrom(v => DateTime.Now))
                .ForMember(x => x.Address, x => x.Ignore())
                .ForMember(x => x.ActualAddress, x => x.Ignore())
                .ForMember(x => x.PostalAddress, x => x.Ignore())
                .ForMember(x => x.EnterpriseUnits, x => x.Ignore()));
            CreateMap<EnterpriseGroup, EnterpriseGroupCreateM>(MemberList.None)
                .ForMember(x => x.ChangeReason, x => x.UseValue(ChangeReasons.Create))
                .ForMember(x => x.Address, x => x.Ignore())
                .ForMember(x => x.ActualAddress, x => x.Ignore())
                .ForMember(x => x.PostalAddress, x => x.Ignore())
                .ForMember(x => x.EnterpriseUnits, x => x.Ignore());

            DataAccessCondition(CreateMap<LegalUnitEditM, LegalUnit>()
                .ForMember(x => x.Address, x => x.Ignore())
                .ForMember(x => x.ActualAddress, x => x.Ignore())
                .ForMember(x => x.PostalAddress, x => x.Ignore())
                .ForMember(x => x.Activities, x => x.Ignore())
                .ForMember(x => x.LocalUnits, x => x.Ignore())
                .ForMember(x => x.Persons, x => x.Ignore())
                .ForMember(x => x.ForeignParticipationCountriesUnits, opt => opt.Ignore()));
            CreateMap<LegalUnit, LegalUnitEditM>()
                .ForMember(x => x.Address, x => x.Ignore())
                .ForMember(x => x.ActualAddress, x => x.Ignore())
                .ForMember(x => x.PostalAddress, x => x.Ignore())
                .ForMember(x => x.Activities, x => x.Ignore())
                .ForMember(x => x.LocalUnits, x => x.Ignore())
                .ForMember(x => x.Persons, x => x.Ignore())
                .ForMember(x => x.ForeignParticipationCountriesUnits, opt => opt.MapFrom(src => src.ForeignParticipationCountriesUnits.Select(x => x.Id)));

            DataAccessCondition(CreateMap<LocalUnitEditM, LocalUnit>()
                .ForMember(x => x.Address, x => x.Ignore())
                .ForMember(x => x.ActualAddress, x => x.Ignore())
                .ForMember(x => x.PostalAddress, x => x.Ignore())
                .ForMember(x => x.Activities, x => x.Ignore())
                .ForMember(x => x.Persons, x => x.Ignore())
                .ForMember(x => x.ForeignParticipationCountriesUnits, opt => opt.Ignore()));
            CreateMap<LocalUnit, LocalUnitEditM>()
                .ForMember(x => x.Address, x => x.Ignore())
                .ForMember(x => x.ActualAddress, x => x.Ignore())
                .ForMember(x => x.PostalAddress, x => x.Ignore())
                .ForMember(x => x.Activities, x => x.Ignore())
                .ForMember(x => x.Persons, x => x.Ignore())
                .ForMember(x=>x.ForeignParticipationCountriesUnits, opt=>opt.MapFrom(src=>src.ForeignParticipationCountriesUnits.Select(x=>x.Id)));

            DataAccessCondition(CreateMap<EnterpriseUnitEditM, EnterpriseUnit>()
                .ForMember(x => x.Address, x => x.Ignore())
                .ForMember(x => x.ActualAddress, x => x.Ignore())
                .ForMember(x => x.PostalAddress, x => x.Ignore())
                .ForMember(x => x.LegalUnits, x => x.Ignore())
                .ForMember(x => x.Activities, x => x.Ignore())
                .ForMember(x => x.Persons, x => x.Ignore())
                .ForMember(x => x.ForeignParticipationCountriesUnits, opt => opt.Ignore()));
            CreateMap<EnterpriseUnit, EnterpriseUnitEditM>()
                .ForMember(x => x.Address, x => x.Ignore())
                .ForMember(x => x.ActualAddress, x => x.Ignore())
                .ForMember(x => x.PostalAddress, x => x.Ignore())
                .ForMember(x => x.LegalUnits, x => x.Ignore())
                .ForMember(x => x.Activities, x => x.Ignore())
                .ForMember(x => x.Persons, x => x.Ignore())
                .ForMember(x => x.ForeignParticipationCountriesUnits, opt => opt.MapFrom(src => src.ForeignParticipationCountriesUnits.Select(x => x.Id)));

            DataAccessCondition(CreateMap<EnterpriseGroupEditM, EnterpriseGroup>()
                .ForMember(x => x.Address, x => x.Ignore())
                .ForMember(x => x.ActualAddress, x => x.Ignore())
                .ForMember(x => x.PostalAddress, x => x.Ignore())
                .ForMember(x => x.EnterpriseUnits, x => x.Ignore()));
            CreateMap<EnterpriseGroup, EnterpriseGroupEditM>()
                .ForMember(x => x.Address, x => x.Ignore())
                .ForMember(x => x.ActualAddress, x => x.Ignore())
                .ForMember(x => x.PostalAddress, x => x.Ignore())
                .ForMember(x => x.EnterpriseUnits, x => x.Ignore());

            CreateMap<Address, AddressM>().ReverseMap();

            CreateMap<ActivityM, Activity>()
                .ForMember(x => x.Id, x => x.Ignore())
                .ForMember(x => x.UpdatedDate, x => x.MapFrom(v => DateTime.Now))
                .ForMember(x => x.ActivityCategory, x => x.Ignore());

            CreateMap<Activity, ActivityM>()
                .ForMember(x => x.Id, x => x.Ignore())
                .ForMember(x => x.IdDate, x => x.MapFrom(v => DateTime.Now));

            CreateMap<PersonM, Person>()
                .ForMember(x => x.Id, x => x.Ignore())
                .ForMember(x => x.IdDate, x => x.MapFrom(v => DateTime.Now));

            CreateMap<Person, PersonM>()
                .ForMember(x => x.Id, x => x.Ignore());

            CreateMap<AddressModel, Address>().ReverseMap();
            CreateMap<RegionM, Region>().ReverseMap();

            CreateMap<CodeLookupVm, UnitLookupVm>();
            CreateMap<DataAccessAttributeM, DataAccessAttributeVm>();
            CreateMap<ActivityCategory, ActivityCategoryVm>();
            CreateMap<SampleFrameM, SampleFrame>().ForMember(x => x.User, x => x.Ignore());

            CreateMap<DataAccessAttributeVm, Permission>()
                .ForMember(x => x.PropertyName, opt => opt.MapFrom(x => x.Name))
                .ForMember(x => x.CanRead, opt => opt.MapFrom(x => x.CanRead))
                .ForMember(x => x.CanWrite, opt => opt.MapFrom(x => x.CanWrite))
                .ForAllOtherMembers(opt=>opt.Ignore());

            CreateMap<AnalysisQueue, AnalysisQueueModel>()
                .ForMember(x => x.UserName, opt => opt.MapFrom(x => x.User.Name));

            CreateMap<ElasticStatUnit, SearchViewAdapterModel>()
                .ForMember(x => x.Address, opt => opt.MapFrom(x => new AddressAdapterModel(x)))
                .ForMember(x => x.Persons, opt => opt.Ignore())
                .ForMember(x => x.Activities, opt => opt.Ignore());

            CreateMap<StatUnitSearchView, ElasticStatUnit>()
                .ForMember(d => d.RegionIds, opt => opt.ResolveUsing(s => s.RegionId != null ? new List<int> { (int)s.RegionId } : new List<int>()));

            CreateMap<IStatisticalUnit, ElasticStatUnit>()
                .ForMember(d => d.LiqDate, opt => opt.MapFrom(s => (s is EnterpriseGroup) ? (s as EnterpriseGroup).LiqDateEnd : (s as StatisticalUnit).LiqDate))
                .ForMember(d => d.RegionId, opt => opt.ResolveUsing(s => s.Address?.RegionId))
                .ForMember(d => d.SectorCodeId, opt => opt.MapFrom(s => s.InstSectorCodeId))
                .ForMember(d => d.AddressPart1, opt => opt.ResolveUsing(s => s.Address?.AddressPart1))
                .ForMember(d => d.AddressPart2, opt => opt.ResolveUsing(s => s.Address?.AddressPart2))
                .ForMember(d => d.AddressPart3, opt => opt.ResolveUsing(s => s.Address?.AddressPart3))
                .ForMember(d => d.ActivityCategoryIds,
                    opt => opt.ResolveUsing(s =>
                        s.ActivitiesUnits?.Select(a => a.Activity.ActivityCategoryId).ToList() ?? new List<int>()
                    )
                )
                .ForMember(d => d.RegionIds,
                    opt => opt.ResolveUsing(s =>
                        new List<int?> { s.Address?.RegionId, s.PostalAddress?.RegionId, s.ActualAddress?.RegionId }
                            .Where(x => x != null).Select(x => (int)x).ToList()
                    )
                );

            ConfigureLookups();
            HistoryMaping();
            CreateStatUnitByRules();
        }

        /// <summary>
        /// Метод конфигурации поиска
        /// </summary>
        private void ConfigureLookups()
        {
            CreateMap<EnterpriseUnit, CodeLookupVm>()
                .ForMember(x => x.Id, opt => opt.MapFrom(x => x.RegId))
                .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Name))
                .ForMember(x => x.Code, opt => opt.MapFrom(x => x.StatId))
                .ForMember(x=>x.NameLanguage1, opt=>opt.Ignore())
                .ForMember(x=>x.NameLanguage2, opt=>opt.Ignore());

            CreateMap<EnterpriseGroup, CodeLookupVm>()
                .ForMember(x => x.Id, opt => opt.MapFrom(x => x.RegId))
                .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Name))
                .ForMember(x => x.Code, opt => opt.MapFrom(x => x.StatId))
                .ForMember(x => x.NameLanguage1, opt => opt.Ignore())
                .ForMember(x => x.NameLanguage2, opt => opt.Ignore());

            CreateMap<LocalUnit, CodeLookupVm>()
                .ForMember(x => x.Id, opt => opt.MapFrom(x => x.RegId))
                .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Name))
                .ForMember(x => x.Code, opt => opt.MapFrom(x => x.StatId))
                .ForMember(x => x.NameLanguage1, opt => opt.Ignore())
                .ForMember(x => x.NameLanguage2, opt => opt.Ignore());

            CreateMap<LegalUnit, CodeLookupVm>()
                .ForMember(x => x.Id, opt => opt.MapFrom(x => x.RegId))
                .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Name))
                .ForMember(x => x.Code, opt => opt.MapFrom(x => x.StatId))
                .ForMember(x => x.NameLanguage1, opt => opt.Ignore())
                .ForMember(x => x.NameLanguage2, opt => opt.Ignore());

            CreateMap<Country, CodeLookupVm>();
            CreateMap<SectorCode, CodeLookupVm>();
            CreateMap<LegalForm, CodeLookupVm>();
            CreateMap<DataSourceClassification, CodeLookupVm>();
            CreateMap<ReorgType, CodeLookupVm>();
            CreateMap<UnitSize, CodeLookupVm>();
            CreateMap<ForeignParticipation, CodeLookupVm>();
            CreateMap<UnitStatus, CodeLookupVm>();
            CreateMap<RegistrationReason, CodeLookupVm>();
            CreateMap<PersonType, CodeLookupVm>();
        }

        /// <summary>
        /// Метод сопоставления истории
        /// </summary>
        private void HistoryMaping()
        {
            MapStatisticalUnit<LocalUnit>();

            MapStatisticalUnit<LegalUnit>()
                .ForMember(m => m.LocalUnits, m => m.Ignore());

            MapStatisticalUnit<EnterpriseUnit>()
                .ForMember(m => m.LegalUnits, m => m.Ignore());

            CreateMap<StatisticalUnit, StatisticalUnitHistory>()
                .ForMember(dst=>dst.RegId, opt=>opt.MapFrom(src=>src.RegId));

            CreateMap<StatisticalUnitHistory, StatisticalUnit>()
                .ForMember(dst => dst.RegId, opt => opt.MapFrom(src => src.RegId));

            CreateMap<EnterpriseGroup, EnterpriseGroup>()
                .ForMember(m => m.EnterpriseUnits, m => m.Ignore());

            CreateMap<LocalUnit, LocalUnitHistory>()
                .ForMember(dst => dst.Activities, opt => opt.Ignore())
                .ForMember(dst => dst.Persons, opt => opt.Ignore())
                .ForMember(dst => dst.Countries, opt => opt.Ignore())
                .ForMember(dst => dst.PersonsUnits, opt => opt.MapFrom(src => src.PersonsUnits))
                .ForMember(dst => dst.ActivitiesUnits, opt => opt.MapFrom(src => src.ActivitiesUnits))
                .ForMember(dst => dst.ForeignParticipationCountriesUnits, opt => opt.MapFrom(src => src.ForeignParticipationCountriesUnits));

            CreateMap<LocalUnitHistory, LocalUnit>()
                .ForMember(dst => dst.Activities, opt => opt.Ignore())
                .ForMember(dst => dst.Persons, opt => opt.Ignore())
                .ForMember(dst => dst.PersonsUnits, opt => opt.Ignore())
                .ForMember(dst => dst.ActivitiesUnits, opt => opt.Ignore())
                .ForMember(dst => dst.ForeignParticipationCountriesUnits, opt => opt.Ignore());

            CreateMap<LegalUnit, LegalUnitHistory>()
                .ForMember(dst=>dst.HistoryLocalUnitIds, opt=>opt.MapFrom(src=>src.HistoryLocalUnitIds))
                .ForMember(dst => dst.Activities, opt => opt.Ignore())
                .ForMember(dst => dst.Persons, opt => opt.Ignore())
                .ForMember(dst => dst.Countries, opt => opt.Ignore())
                .ForMember(dst => dst.PersonsUnits, opt => opt.MapFrom(src => src.PersonsUnits))
                .ForMember(dst => dst.ActivitiesUnits, opt => opt.MapFrom(src => src.ActivitiesUnits))
                .ForMember(dst => dst.ForeignParticipationCountriesUnits, opt => opt.MapFrom(src => src.ForeignParticipationCountriesUnits));
            CreateMap<LegalUnitHistory, LegalUnit>()
                .ForMember(dst => dst.HistoryLocalUnitIds, opt => opt.MapFrom(src => src.HistoryLocalUnitIds))
                .ForMember(dst => dst.Activities, opt => opt.Ignore())
                .ForMember(dst => dst.Persons, opt => opt.Ignore())
                .ForMember(dst => dst.PersonsUnits, opt => opt.Ignore())
                .ForMember(dst => dst.ActivitiesUnits, opt => opt.Ignore())
                .ForMember(dst => dst.ForeignParticipationCountriesUnits, opt => opt.Ignore());

            CreateMap<EnterpriseUnit, EnterpriseUnitHistory>()
                .ForMember(dst => dst.Activities, opt => opt.Ignore())
                .ForMember(dst => dst.Persons, opt => opt.Ignore())
                .ForMember(dst => dst.Countries, opt => opt.Ignore())
                .ForMember(dst => dst.PersonsUnits, opt => opt.MapFrom(src => src.PersonsUnits))
                .ForMember(dst => dst.ActivitiesUnits, opt => opt.MapFrom(src => src.ActivitiesUnits))
                .ForMember(dst => dst.ForeignParticipationCountriesUnits, opt => opt.MapFrom(src => src.ForeignParticipationCountriesUnits));
            CreateMap<EnterpriseUnitHistory, EnterpriseUnit>()
                .ForMember(dst => dst.Activities, opt => opt.Ignore())
                .ForMember(dst => dst.Persons, opt => opt.Ignore())
                .ForMember(dst => dst.PersonsUnits, opt => opt.Ignore())
                .ForMember(dst => dst.ActivitiesUnits, opt => opt.Ignore())
                .ForMember(dst => dst.ForeignParticipationCountriesUnits, opt => opt.Ignore());

            CreateMap<EnterpriseGroup, EnterpriseGroupHistory>().ReverseMap();

            CreateMap<ActivityStatisticalUnit, ActivityStatisticalUnitHistory>()
                .ForMember(dst=>dst.Unit, opt=>opt.Ignore());
            CreateMap<ActivityStatisticalUnitHistory, ActivityStatisticalUnit>()
                .ForMember(dst => dst.Unit, opt => opt.Ignore());
            CreateMap<PersonStatisticalUnit, PersonStatisticalUnitHistory>()
                .ForMember(dst => dst.Unit, opt => opt.Ignore());
            CreateMap<PersonStatisticalUnitHistory, PersonStatisticalUnit>()
                .ForMember(dst => dst.Unit, opt => opt.Ignore());
            CreateMap<CountryStatisticalUnit, CountryStatisticalUnitHistory>()
                .ForMember(dst => dst.Unit, opt => opt.Ignore());
            CreateMap<CountryStatisticalUnitHistory, CountryStatisticalUnit>()
                .ForMember(dst => dst.Unit, opt => opt.Ignore());

        }

        private void CreateStatUnitByRules()
        {
            CreateMap<LocalUnit, LegalUnit>()
                .ForMember(x => x.MunCapitalShare, x => x.UseValue(string.Empty))
                .ForMember(x => x.PrivCapitalShare, x => x.UseValue(string.Empty))
                .ForMember(x => x.StateCapitalShare, x => x.UseValue(string.Empty))
                .ForMember(x => x.TotalCapital, x => x.UseValue(string.Empty))
                .ForMember(x => x.ForeignCapitalShare, x => x.UseValue(string.Empty))
                .ForMember(x => x.ForeignCapitalCurrency, x => x.UseValue(string.Empty))
                .ForMember(x => x.HistoryLocalUnitIds, x => x.UseValue(string.Empty))
                .ForMember(x => x.EntRegIdDate, x => x.UseValue(DateTime.Now))
                .ForMember(x => x.Market, x => x.UseValue(false))
                .ForMember(x => x.EnterpriseUnitRegId, x => x.UseValue((int?) null))
                .ForMember(x => x.AddressId, x => x.MapFrom(y => y.AddressId == 0 ? null : y.AddressId))
                .ForMember(x => x.ChangeReason, x => x.UseValue(ChangeReasons.Create))

                .ForMember(x => x.EnterpriseUnit, x => x.Ignore())

                .ForMember(x => x.RegId, x => x.Ignore())
                .ForMember(x => x.Address, x => x.Ignore())
                .ForMember(x => x.ActualAddress, x => x.Ignore())
                .ForMember(x => x.PostalAddress, x => x.Ignore())
               
                .ForMember(x => x.ActivitiesUnits, x => x.Ignore())
                .ForMember(x => x.Activities, x => x.Ignore())
                .ForMember(x => x.PersonsUnits, x => x.Ignore())
                .ForMember(x => x.Persons, x => x.Ignore())
                .ForMember(x => x.ForeignParticipationCountriesUnits, x => x.Ignore())

                .ForMember(x => x.LocalUnits, x => x.Ignore())
                .ForMember(x => x.LegalForm, x => x.Ignore())
                .ForMember(x => x.InstSectorCode, x => x.Ignore());

            CreateMap<LegalUnit, LocalUnit>()
                .ForMember(x => x.AddressId, x => x.MapFrom(y => y.AddressId == 0 ? null : y.AddressId))
                .ForMember(x => x.ChangeReason, x => x.UseValue(ChangeReasons.Create))
                .ForMember(x => x.LegalUnitIdDate, x => x.UseValue(DateTime.Now))

                .ForMember(x => x.RegId, x => x.Ignore())

                .ForMember(x => x.Address, x => x.Ignore())
                .ForMember(x => x.ActualAddress, x => x.Ignore())
                .ForMember(x => x.PostalAddress, x => x.Ignore())

                .ForMember(x => x.ActivitiesUnits, x => x.Ignore())
                .ForMember(x => x.Activities, x => x.Ignore())
                .ForMember(x => x.PersonsUnits, x => x.Ignore())
                .ForMember(x => x.Persons, x => x.Ignore())
                .ForMember(x => x.ForeignParticipationCountriesUnits, x => x.Ignore())

                .ForMember(x => x.LegalForm, x => x.Ignore())
                .ForMember(x => x.LegalUnit, x => x.Ignore())
                .ForMember(x => x.InstSectorCode, x => x.Ignore());

            CreateMap<LegalUnit, EnterpriseUnit>()
                .ForMember(x => x.AddressId, x => x.MapFrom(y => y.AddressId == 0 ? null : y.AddressId))
                .ForMember(x => x.ChangeReason, x => x.UseValue(ChangeReasons.Create))
                .ForMember(x => x.Commercial, x => x.UseValue(false))
                .ForMember(x => x.EntGroupId, x => x.UseValue((int?) null))
                .ForMember(x => x.EntGroupIdDate, x => x.Ignore())
                .ForMember(x => x.EntGroupRole, x => x.UseValue(string.Empty))
                .ForMember(x => x.HistoryLegalUnitIds, x => x.UseValue(string.Empty))
                .ForMember(x => x.EnterpriseGroup, x => x.Ignore())
                .ForMember(x => x.LegalUnits, x => x.Ignore())
                .ForMember(x => x.RegId, x => x.Ignore())
                .ForMember(x => x.ActivitiesUnits, x => x.Ignore())
                .ForMember(x => x.Activities, x => x.Ignore())
                .ForMember(x => x.Address, x => x.Ignore())
                .ForMember(x => x.PersonsUnits, x => x.Ignore())
                .ForMember(x => x.Persons, x => x.Ignore())
                .ForMember(x => x.ForeignParticipationCountriesUnits, x => x.Ignore())
                .ForMember(x => x.LegalForm, x => x.Ignore())
                .ForMember(x => x.ActualAddress, x => x.Ignore())
                .ForMember(x => x.PostalAddress, x => x.Ignore())
                .ForMember(x => x.InstSectorCode, x => x.Ignore());

            CreateMap<EnterpriseUnit, EnterpriseGroup>()
                .ForMember(x => x.AddressId, x => x.MapFrom(y => y.AddressId == 0 ? null : y.AddressId))
                .ForMember(x => x.ChangeReason, x => x.UseValue(ChangeReasons.Create))
                .ForMember(x => x.StatusDate, x => x.MapFrom(y => y.StatusDate ?? DateTime.Now))
                .ForMember(x => x.LiqDateStart, x => x.UseValue((DateTime?) null))
                .ForMember(x => x.LiqDateEnd, x => x.UseValue((DateTime?) null))
                .ForMember(x => x.HistoryEnterpriseUnitIds, x => x.UseValue(string.Empty))
                .ForMember(x => x.EntGroupType, x => x.Ignore())
                .ForMember(x => x.RegId, x => x.Ignore())
                .ForMember(x => x.Address, x => x.Ignore())
                .ForMember(x => x.ActualAddress, x => x.Ignore())
                .ForMember(x => x.PostalAddress, x => x.Ignore());

            CreateMap<AnalisysQueueCreateModel, AnalysisQueue>()
                .ForMember(x=>x.UserStartPeriod, opt=>opt.MapFrom(x=>x.DateFrom))
                .ForMember(x=> x.UserEndPeriod, opt=>opt.MapFrom(x=>x.DateTo))
                .ForMember(x=>x.Comment, opt=>opt.MapFrom(x=>x.Comment))
                .ForAllOtherMembers(x=>x.Ignore());

            CreateMap<Activity, Activity>()
                .ForMember(x => x.ActivityType, opt => opt.PreCondition(x => x.ActivityType != default(ActivityTypes)))
                .ForMember(x => x.ActivityType, opt => opt.MapFrom(x => x.ActivityType))
                .ForMember(x => x.ActivityYear, opt => opt.PreCondition(x => x.ActivityYear != default(int)))
                .ForMember(x => x.ActivityYear, opt => opt.MapFrom(x => x.ActivityYear))
                .ForMember(x => x.Employees, opt => opt.PreCondition(x => x.Employees != default(int)))
                .ForMember(x => x.Employees, opt => opt.MapFrom(x => x.Employees))
                .ForMember(x => x.Turnover, opt => opt.PreCondition(x => x.Turnover != default(decimal?)))
                .ForMember(x => x.Turnover, opt => opt.MapFrom(x => x.Turnover))
                .ForAllOtherMembers(x => x.Ignore());
        }

        /// <summary>
        /// Метод сопоставления стат. единицы
        /// </summary>
        /// <returns></returns>
        private IMappingExpression<T, T> MapStatisticalUnit<T>() where T : StatisticalUnit
            => CreateMap<T, T>()
                .ForMember(v => v.Activities, v => v.Ignore())
                .ForMember(v => v.ActivitiesUnits, v =>
                    v.MapFrom(x => x.ActivitiesUnits.Select(z => new ActivityStatisticalUnit {ActivityId = z.ActivityId})))
                .ForMember(v => v.Persons, v => v.Ignore())
                .ForMember(v => v.PersonsUnits, v =>
                    v.MapFrom(x => x.PersonsUnits.Select(z => new PersonStatisticalUnit {PersonId = z.PersonId, PersonTypeId = z.PersonTypeId})))
                .ForMember(v => v.ForeignParticipationCountriesUnits, v =>
                    v.MapFrom(x => x.ForeignParticipationCountriesUnits.Select(z => new CountryStatisticalUnit {CountryId = z.CountryId})));

        /// <summary>
        /// Метод создания стат. единицы из модели сопоставления
        /// </summary>
        /// <returns></returns>
        private IMappingExpression<TSource, TDestination> CreateStatUnitFromModelMap<TSource, TDestination>()
            where TSource : StatUnitModelBase
            where TDestination : StatisticalUnit
            => CreateMap<TSource, TDestination>()
                .ForMember(x => x.ChangeReason, x => x.UseValue(ChangeReasons.Create))
                .ForMember(x => x.StartPeriod, x => x.MapFrom(v => DateTime.Now))
                .ForMember(x => x.EndPeriod, x => x.UseValue(DateTime.MaxValue))
                .ForMember(x => x.RegIdDate, x => x.MapFrom(v => DateTime.Now))
                .ForMember(x => x.Address, x => x.Ignore())
                .ForMember(x => x.ActualAddress, x => x.Ignore())
                .ForMember(x => x.PostalAddress, x => x.Ignore())
                .ForMember(x => x.ActivitiesUnits, x => x.Ignore())
                .ForMember(x => x.Activities, x => x.Ignore())
                .ForMember(x => x.Persons, x => x.Ignore())
                .ForMember(x => x.ForeignParticipationCountriesUnits, x => x.Ignore());

        /// <summary>
        ///  Метод создания стат. единицы из обратного сопоставления
        /// </summary>
        private void CreateStatUnitFromModelReverseMap<TSource, TDestination>()
            where TSource : StatisticalUnit
            where TDestination : StatUnitModelBase
            => CreateMap<TSource, TDestination>()
                .ForMember(x => x.ChangeReason, x => x.UseValue(ChangeReasons.Create))
                .ForMember(x => x.Address, x => x.Ignore())
                .ForMember(x => x.ActualAddress, x => x.Ignore())
                .ForMember(x => x.PostalAddress, x => x.Ignore())
                .ForMember(x => x.Activities, x => x.Ignore())
                .ForMember(x => x.Persons, x => x.Ignore());
        /// <summary>
        /// Метод  обработки условии к доступу данных
        /// </summary>
        private static void DataAccessCondition<TSource, TDestionation>(
            IMappingExpression<TSource, TDestionation> mapping)
            where TSource : IStatUnitM
            where TDestionation : IStatisticalUnit
            =>
                mapping.ForAllMembers(v => v.Condition((src, dst) =>
                {
                    var name = DataAccessAttributesHelper.GetName(dst.GetType(), v.DestinationMember.Name);
                    return DataAccessAttributesProvider.Find(name) == null
                           || (src.DataAccess?.HasWritePermission(name) ?? false);
                }));
    }
}
