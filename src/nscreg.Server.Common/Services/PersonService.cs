using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using nscreg.Data;
using nscreg.Data.Entities;
using nscreg.Server.Common.Models.StatUnits;

namespace nscreg.Server.Common.Services
{
    /// <summary>
    /// Сервис персон
    /// </summary>
    public class PersonService
    {
        private readonly NSCRegDbContext _context;

        public PersonService(NSCRegDbContext context)
        {
            _context = context;
        }

        /// <summary>
        ///  Метод поиска персоны
        /// </summary>
        /// <param name="wildcard">Шаблон поиска</param>
        /// <param name="limit">Ограничение</param>
        /// <returns></returns>
        public async Task<List<PersonM>> Search(string wildcard, int limit = 10)
        {
            var loweredwc = wildcard.ToLower();
            return await ToViewModel(_context.Persons.Where(v =>
                    v.GivenName.ToLower().StartsWith(loweredwc) ||
                    v.Surname.ToLower().StartsWith(loweredwc))
                .Select(g => new Person
                {
                    Id = g.Id,
                    GivenName = g.GivenName,
                    MiddleName = g.MiddleName,
                    Surname = g.Surname,
                    Address = g.Address,
                    BirthDate = g.BirthDate,
                    CountryId = g.CountryId,
                    PersonalId = g.PersonalId,
                    PhoneNumber = g.PhoneNumber,
                    PhoneNumber1 = g.PhoneNumber1,
                    Sex = g.Sex
                })
                .Distinct()
                .OrderBy(v => v.GivenName)
                .Take(limit));
        }

        /// <summary>
        /// Метод преобразования данных ко вью модели
        /// </summary>
        /// <param name="query">Запрос</param>
        /// <returns></returns>
        private static async Task<List<PersonM>> ToViewModel(IQueryable<Person> query)
            => await query.Select(v => new PersonM
            {
                Id = v.Id,
                Address = v.Address,
                Surname = v.Surname,
                MiddleName = v.MiddleName,
                GivenName = v.GivenName,
                BirthDate = v.BirthDate,
                CountryId = v.CountryId,
                PersonalId = v.PersonalId,
                PhoneNumber = v.PhoneNumber,
                PhoneNumber1 = v.PhoneNumber1,
                Sex = v.Sex
            }).ToListAsync();
    }
}
