using Microsoft.EntityFrameworkCore;

namespace PresalesDemo;

public class PresalesDbContext : DbContext
{
    public PresalesDbContext(DbContextOptions<PresalesDbContext> options) : base(options) { }

    public DbSet<CustomerProfile> Profiles => Set<CustomerProfile>();
    public DbSet<DiscoveryItem> DiscoveryItems => Set<DiscoveryItem>();
    public DbSet<PriorityItem> Priorities => Set<PriorityItem>();
    public DbSet<Scenario> Scenarios => Set<Scenario>();
    public DbSet<FitGapRow> FitGapRows => Set<FitGapRow>();
    public DbSet<Scorecard> Scorecards => Set<Scorecard>();
    public DbSet<ActionItem> Actions => Set<ActionItem>();
    public DbSet<FollowUp> FollowUps => Set<FollowUp>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();

    public DbSet<SurveyPresentation> SurveyPresentations => Set<SurveyPresentation>();
    public DbSet<SurveyQuestion> SurveyQuestions => Set<SurveyQuestion>();
    public DbSet<SurveyResponse> SurveyResponses => Set<SurveyResponse>();

    public DbSet<Interaction> Interactions => Set<Interaction>();
    public DbSet<DemoAppointment> DemoAppointments => Set<DemoAppointment>();
    public DbSet<Attachment> Attachments => Set<Attachment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<DemoAppointment>().Property(a => a.ReminderMinutes).HasDefaultValue(60);
        b.Entity<CustomerProfile>().Property(p => p.Status).HasDefaultValue("DangChuanBi");
        foreach (var type in new[] { typeof(Interaction), typeof(DemoAppointment), typeof(Attachment) })
        {
            b.Entity(type).HasIndex("ProfileId");
            b.Entity(type).HasOne(typeof(CustomerProfile)).WithMany().HasForeignKey("ProfileId").OnDelete(DeleteBehavior.Cascade);
        }
        b.Entity<CustomerProfile>().HasIndex(p => p.Industry);
        b.Entity<Scorecard>().HasIndex(s => s.ProfileId).IsUnique();
        foreach (var e in new[] { typeof(DiscoveryItem), typeof(PriorityItem), typeof(Scenario), typeof(FitGapRow), typeof(ActionItem), typeof(FollowUp), typeof(ChecklistItem) })
        {
            b.Entity(e).HasIndex("ProfileId");
        }
    }
}
