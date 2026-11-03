using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PresalesDemo;

public class CustomerProfile
{
    public int Id { get; set; }
    [MaxLength(200)] public string Name { get; set; } = "";
    [MaxLength(50)] public string Industry { get; set; } = "";
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
