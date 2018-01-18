using System;
using System.Collections.Generic;
using nscreg.Data.Constants;
using Newtonsoft.Json;

namespace nscreg.Data.Entities
{
    /// <summary>
    ///  Класс сущность деятельности
    /// </summary>
    public class Activity
    {
        public int Id { get; set; }
        public DateTime IdDate { get; set; }
        [JsonIgnore]
        public virtual ICollection<ActivityStatisticalUnit> ActivitiesUnits { get; set; }
        public int ActivityCategoryId { get; set; }
        public virtual ActivityCategory ActivityCategory { get; set; }
        public int ActivityYear { get; set; }
        public ActivityTypes ActivityType { get; set; }
        public int Employees { get; set; }
        public decimal? Turnover { get; set; }
        [JsonIgnore]
        public string UpdatedBy { get; set; }
        [JsonIgnore]
        public virtual User UpdatedByUser { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
