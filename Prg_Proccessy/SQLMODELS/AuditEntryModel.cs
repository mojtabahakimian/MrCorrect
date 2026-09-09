using System;

namespace Prg_Proccessy.SQLMODELS
{
    public class AuditEntryModel
    {
        public long LogId { get; set; }
        public long PersianDate { get; set; }
        public string LogTime { get; set; } = string.Empty;
        public DateTime ServerDateTime { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? ClientIP { get; set; }
        public string? MachineName { get; set; }
        public Guid SessionId { get; set; }
        public string ModuleTitle { get; set; } = string.Empty;
        public string FormClass { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string ActionCaption { get; set; } = string.Empty;
        public string? EntityName { get; set; }
        public double? DocNumber { get; set; }
        public double? DocTag { get; set; }
        public string DocTrackingKey => (DocNumber.HasValue && DocTag.HasValue) ? $"{DocNumber}/{DocTag}" : (DocNumber.HasValue ? DocNumber.Value.ToString() : string.Empty);
        public string? DetailsJson { get; set; }
        public byte Severity { get; set; } = 1;
    }
}
