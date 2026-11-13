using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PresalesDemo;

public class CustomerProfile
{
    public int Id { get; set; }
    [MaxLength(200)] public string Name { get; set; } = "";
    [MaxLength(50)] public string Industry { get; set; } = "";
    [MaxLength(20)] public string Status { get; set; } = "DangChuanBi";
    public DateTime? DemoDate { get; set; }
    [MaxLength(100)] public string? Owner { get; set; }
    [MaxLength(1000)] public string? FollowUpNote { get; set; }
    public int Locations { get; set; }
    public int EmployeeCount { get; set; }
    // Per-profile details previously session-only in the frontend. Stored as
    // plain strings/JSON so one PUT can persist them without new tables.
    public string SystemInfo { get; set; } = "";
    public string ClosingJson { get; set; } = "";
    public string ActionPlanJson { get; set; } = "";
    public string PrioConfirmedJson { get; set; } = "";
    public string HealthCsv { get; set; } = "";
    public string PrepCsv { get; set; } = "";
    public string EstimatorHead { get; set; } = "";
    public string EstimatorDays { get; set; } = "";
    public string EstimatorPeople { get; set; } = "";
    public string PayrollDeepAnswer { get; set; } = "";
    public bool IsDefault { get; set; }
    public bool IsDemoSample { get; set; } = true;
    public bool AiSuggestionRequested { get; set; } = false;
    public DateTime? AiSuggestionRequestedAt { get; set; }
    public bool CompanyResearchRequested { get; set; } = false;
    public DateTime? CompanyResearchRequestedAt { get; set; }
    public string CompanyResearchNote { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class DiscoveryItem
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    [MaxLength(100)] public string Group { get; set; } = "";
    [MaxLength(500)] public string Question { get; set; } = "";
    public string Answer { get; set; } = "";
    public int SortOrder { get; set; }
}

public class PriorityItem
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    public int Rank { get; set; }
    [MaxLength(500)] public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    // "" = not confirmed, "1" = confirmed in Discovery, "0" = customer not confirmed
    [MaxLength(10)] public string ConfirmedStatus { get; set; } = "";
}

public class Scenario
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    public int SortOrder { get; set; }
    [MaxLength(500)] public string Title { get; set; } = "";
    public string Pain { get; set; } = "";
    public string Solution { get; set; } = "";
    public string Outcome { get; set; } = "";
    public string Value { get; set; } = "";
    public string ScreenFlow { get; set; } = "";
    public string StorySteps { get; set; } = "";
    // True only when the user actually edited this scenario in the UI, so the
    // saved (derived) story wins over the rich template story on reload.
    // Unedited profiles keep StoryEdited=false and keep the template story.
    public bool StoryEdited { get; set; }
    public string BeforeText { get; set; } = "";
    public string AfterText { get; set; } = "";
}

public class FitGapRow
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    [MaxLength(500)] public string Requirement { get; set; } = "";
    [MaxLength(50)] public string Status { get; set; } = "Đáp ứng sẵn";
    public string Note { get; set; } = "";
    [MaxLength(200)] public string FollowOwner { get; set; } = "";
    [MaxLength(50)] public string FollowDue { get; set; } = "";
    public int SortOrder { get; set; }
}

public class Scorecard
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    public int UnderstandCustomer { get; set; }
    public int RightProblem { get; set; }
    public int BusinessValue { get; set; }
    public int HandleObjections { get; set; }
    public int NextStep { get; set; }
    public int Total => UnderstandCustomer + RightProblem + BusinessValue + HandleObjections + NextStep;
}

public class ActionItem
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    [MaxLength(500)] public string Title { get; set; } = "";
    [MaxLength(200)] public string Owner { get; set; } = "";
    public string Deadline { get; set; } = "";
    [MaxLength(100)] public string Source { get; set; } = "";
    public bool IsDone { get; set; }
}

public class FollowUp
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    [MaxLength(500)] public string Title { get; set; } = "";
    [MaxLength(200)] public string Owner { get; set; } = "";
    public string Deadline { get; set; } = "";
    [MaxLength(500)] public string SourceFitGap { get; set; } = "";
}

public class ChecklistItem
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    [MaxLength(100)] public string Group { get; set; } = "";
    [MaxLength(500)] public string Title { get; set; } = "";
    public bool IsDone { get; set; }
    public bool IsOneNowMandatory { get; set; }
    public int SortOrder { get; set; }
}

// DTOs for create/update (keeps API surface small and explicit).
public record ProfileCreateDto(string Name, string Industry, int Locations, int EmployeeCount);
public record ResearchNoteDto(string Note);
public record ProfileUpdateDto(string Name, int Locations, int EmployeeCount);
public record DiscoveryUpdateDto(string Answer);
public record FitGapUpdateDto(string Status, string Note, string? Requirement = null, string? FollowOwner = null, string? FollowDue = null);
public record ProfileDetailsDto(string SystemInfo, string ClosingJson, string ActionPlanJson, string PrioConfirmedJson, string HealthCsv, string PrepCsv, string EstimatorHead, string EstimatorDays, string EstimatorPeople, string PayrollDeepAnswer);
public record ScenarioUpdateDto(string Title, string Pain, string Solution, string Outcome, string Value, string ScreenFlow, string StorySteps, string BeforeText, string AfterText, bool StoryEdited = false);
public record PriorityUpdateDto(string Title, string Description, string ConfirmedStatus);
public record ActionUpdateDto(string Title, string Owner, string Deadline, string Source);
public record FollowUpUpdateDto(string Owner, string Deadline);
public record ScorecardUpdateDto(int UnderstandCustomer, int RightProblem, int BusinessValue, int HandleObjections, int NextStep);
public record ActionCreateDto(string Title, string Owner, string Deadline, string Source);
public record ChecklistToggleDto(bool IsDone);

public class SurveyQuestion
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
    public string Title { get; set; } = "";
    public string Hint { get; set; } = "";
    public string Type { get; set; } = "short_text";
    public string OptionsJson { get; set; } = "[]";
    public string FieldsJson { get; set; } = "[]";
    public string Placeholder { get; set; } = "";
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
    public int? ScaleMax { get; set; }
    public bool Required { get; set; }
    public bool IsActive { get; set; } = true;
    public string? MediaUrl { get; set; }
    public string? MediaType { get; set; }
}
public class SurveyResponse
{
    public int Id { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public string ContactName { get; set; } = "";
    public string ContactEmail { get; set; } = "";
    public string Role { get; set; } = "";
    public string CompanyName { get; set; } = "";
    public string Industry { get; set; } = "";
    public string EmployeeBucket { get; set; } = "";
    public string LocationsBucket { get; set; } = "";
    public string CurrentSystem { get; set; } = "";
    public string PainPointsJson { get; set; } = "[]";
    public string ShiftType { get; set; } = "";
    public string DecisionMaker { get; set; } = "";
    public string ApplyTimeline { get; set; } = "";
    public string DemoMustSee { get; set; } = "";
    public string AnswersJson { get; set; } = "{}";
    // Snapshot preserves labels/types even after editing or restoring definitions.
    public string QuestionsJson { get; set; } = "[]";
    public int? ProfileId { get; set; }
}
public record SurveySubmitDto(Dictionary<string, System.Text.Json.JsonElement>? Answers, string? UploadToken, string? IndustryOther = null);
public record SurveyField(string Key, string Label, string Type, string Placeholder, bool Required);
public record SurveyOption(string Text, string? Icon = null);

public class SurveyPresentation
{
    public int Id { get; set; }
    public string? MediaUrl { get; set; } = "/images/onenow-logo.png";
    public string? MediaType { get; set; } = "image";
}

public class Interaction
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    [MaxLength(20)] public string Type { get; set; } = "GhiChu";
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    [MaxLength(2000)] public string Note { get; set; } = "";
    [MaxLength(100)] public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public class DemoAppointment
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    [MaxLength(200)] public string Title { get; set; } = "";
    public DateTime StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    [MaxLength(1000)] public string? Location { get; set; }
    [MaxLength(2000)] public string? Notes { get; set; }
    public int? ReminderMinutes { get; set; } = 60;
    [MaxLength(20)] public string Status { get; set; } = "SapDienRa";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public class Attachment
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    [MaxLength(255)] public string FileName { get; set; } = "";
    [MaxLength(255)] public string StoredName { get; set; } = "";
    public long Size { get; set; }
    [MaxLength(200)] public string ContentType { get; set; } = "application/octet-stream";
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
// JsonElement preserves omitted fields versus explicit null when patching CRM metadata.
public record StatusDto(string Status, System.Text.Json.JsonElement DemoDate, System.Text.Json.JsonElement Owner, System.Text.Json.JsonElement FollowUpNote);
