using System;
using System.Collections.Generic;
using AutoMapper;
using nscreg.Data;
using nscreg.Data.Constants;
using nscreg.Data.Entities;
using nscreg.Utilities.Enums;

namespace nscreg.Server.Common.Services.StatUnit
{
    /// <summary>
    /// Класс сервис удаления
    /// </summary>
    public class DeleteService
    {
        private readonly Dictionary<StatUnitTypes, Action<int, bool, string>> _deleteUndeleteActions;
        private readonly NSCRegDbContext _dbContext;

        public DeleteService(NSCRegDbContext dbContext)
        {
            _dbContext = dbContext;
            _deleteUndeleteActions = new Dictionary<StatUnitTypes, Action<int, bool, string>>
            {
                [StatUnitTypes.EnterpriseGroup] = DeleteUndeleteEnterpriseGroupUnit,
                [StatUnitTypes.EnterpriseUnit] = DeleteUndeleteEnterpriseUnit,
                [StatUnitTypes.LocalUnit] = DeleteUndeleteLocalUnit,
                [StatUnitTypes.LegalUnit] = DeleteUndeleteLegalUnit
            };
        }

        /// <summary>
        /// Удаление/Восстановление  стат. единицы
        /// </summary>
        /// <param name="unitType">Тип стат. единицы</param>
        /// <param name="id">Id стат. единицы</param>
        /// <param name="toDelete">Флаг удалённости</param>
        /// <param name="userId">Id пользователя</param>
        public void DeleteUndelete(StatUnitTypes unitType, int id, bool toDelete, string userId)
        {
            _deleteUndeleteActions[unitType](id, toDelete, userId);
        }

        /// <summary>
        /// Удаление/Восстановление группы предприятия
        /// </summary>
        /// <param name="id">Id стат. единицы</param>
        /// <param name="toDelete">Флаг удалённости</param>
        /// <param name="userId">Id пользователя</param>
        private void DeleteUndeleteEnterpriseGroupUnit(int id, bool toDelete, string userId)
        {
            var unit = _dbContext.EnterpriseGroups.Find(id);
            if (unit.IsDeleted == toDelete) return;
            var hUnit = new EnterpriseGroup();
            Mapper.Map(unit, hUnit);
            unit.IsDeleted = toDelete;
            unit.UserId = userId;
            unit.EditComment = null;
            unit.ChangeReason = toDelete ? ChangeReasons.Delete : ChangeReasons.Undelete;
            _dbContext.EnterpriseGroups.Add((EnterpriseGroup) Common.TrackHistory(unit, hUnit));
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// Удаление/Восстановление  правовой единицы
        /// </summary>
        /// <param name="id">Id стат. единицы</param>
        /// <param name="toDelete">Флаг удалённости</param>
        /// <param name="userId">Id пользователя</param>
        private void DeleteUndeleteLegalUnit(int id, bool toDelete, string userId)
        {
            var unit = _dbContext.StatisticalUnits.Find(id);
            if (unit.IsDeleted == toDelete) return;
            var hUnit = new LegalUnit();
            Mapper.Map(unit, hUnit);
            unit.IsDeleted = toDelete;
            unit.UserId = userId;
            unit.EditComment = null;
            unit.ChangeReason = toDelete ? ChangeReasons.Delete : ChangeReasons.Undelete;
            _dbContext.LegalUnits.Add((LegalUnit) Common.TrackHistory(unit, hUnit));
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// Удаление/Восстановление  местной единицы
        /// </summary>
        /// <param name="id">Id стат. единицы</param>
        /// <param name="toDelete">Флаг удалённости</param>
        /// <param name="userId">Id пользователя</param>
        private void DeleteUndeleteLocalUnit(int id, bool toDelete, string userId)
        {
            var unit = _dbContext.StatisticalUnits.Find(id);
            if (unit.IsDeleted == toDelete) return;
            var hUnit = new LocalUnit();
            Mapper.Map(unit, hUnit);
            unit.IsDeleted = toDelete;
            unit.UserId = userId;
            unit.EditComment = null;
            unit.ChangeReason = toDelete ? ChangeReasons.Delete : ChangeReasons.Undelete;
            _dbContext.LocalUnits.Add((LocalUnit) Common.TrackHistory(unit, hUnit));
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// Удаление/Восстановление  предприятия
        /// </summary>
        /// <param name="id">Id стат. единицы</param>
        /// <param name="toDelete">Флаг удалённости</param>
        /// <param name="userId">Id пользователя</param>
        private void DeleteUndeleteEnterpriseUnit(int id, bool toDelete, string userId)
        {
            var unit = _dbContext.StatisticalUnits.Find(id);
            if (unit.IsDeleted == toDelete) return;
            var hUnit = new EnterpriseUnit();
            Mapper.Map(unit, hUnit);
            unit.IsDeleted = toDelete;
            unit.UserId = userId;
            unit.EditComment = null;
            unit.ChangeReason = toDelete ? ChangeReasons.Delete : ChangeReasons.Undelete;
            _dbContext.EnterpriseUnits.Add((EnterpriseUnit) Common.TrackHistory(unit, hUnit));
            _dbContext.SaveChanges();
        }
    }
}
