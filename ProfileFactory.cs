using Microsoft.EntityFrameworkCore;
namespace PresalesDemo;
public static class ProfileFactory
{
 public static async Task<CustomerProfile> Create(PresalesDbContext db, ProfileCreateDto dto)
 {
    var p = new CustomerProfile { Name = dto.Name, Industry = dto.Industry, Locations = dto.Locations, EmployeeCount = dto.EmployeeCount, IsDefault = false, IsDemoSample = false };
    db.Profiles.Add(p); await db.SaveChangesAsync();
    // Copy template content from the default profile of this industry.
    var tpl = await db.Profiles.FirstOrDefaultAsync(x => x.Industry == dto.Industry && x.IsDefault);
    if (tpl is not null)
    {
        foreach (var d in await db.DiscoveryItems.Where(x => x.ProfileId == tpl.Id).ToListAsync()) db.DiscoveryItems.Add(new DiscoveryItem { ProfileId = p.Id, Group = d.Group, Question = d.Question, Answer = d.Answer, SortOrder = d.SortOrder });
        foreach (var x in await db.Priorities.Where(x => x.ProfileId == tpl.Id).ToListAsync()) db.Priorities.Add(new PriorityItem { ProfileId = p.Id, Rank = x.Rank, Title = x.Title, Description = x.Description });
        foreach (var x in await db.Scenarios.Where(x => x.ProfileId == tpl.Id).ToListAsync()) db.Scenarios.Add(new Scenario { ProfileId = p.Id, SortOrder = x.SortOrder, Title = x.Title, Pain = x.Pain, Solution = x.Solution, Outcome = x.Outcome, Value = x.Value, ScreenFlow = x.ScreenFlow, StorySteps = x.StorySteps, StoryEdited = x.StoryEdited, BeforeText = x.BeforeText, AfterText = x.AfterText });
        foreach (var x in await db.FitGapRows.Where(x => x.ProfileId == tpl.Id).ToListAsync()) db.FitGapRows.Add(new FitGapRow { ProfileId = p.Id, Requirement = x.Requirement, Status = x.Status, Note = x.Note, SortOrder = x.SortOrder });
        // Flow fix: a NEW customer profile must start un-scored (0/100, "Chưa chấm") and without the template's
        // sample action deadlines — copying the template scorecard made a fresh profile look already scored (e.g. 73/100)
        // before any demo happened. Template content (Discovery, priorities, scenarios, Fit-Gap) is still copied;
        // only the score and the template's dated actions/follow-ups are NOT carried over. Duplicate keeps copying
        // (that route is intentionally a copy of an existing profile).
        foreach (var x in await db.ChecklistItems.Where(x => x.ProfileId == tpl.Id).ToListAsync()) db.ChecklistItems.Add(new ChecklistItem { ProfileId = p.Id, Group = x.Group, Title = x.Title, IsDone = false, IsOneNowMandatory = x.IsOneNowMandatory, SortOrder = x.SortOrder });
    }
    else
    {
        // No industry template: start with the shared preparation checklist only.
        var titles = SeedData.ChecklistTitles();
        for (var i = 0; i < titles.Length; i++)
            db.ChecklistItems.Add(new ChecklistItem { ProfileId = p.Id, Group = SeedData.ChecklistGroups[Math.Min(i / 3, SeedData.ChecklistGroups.Length - 1)], Title = titles[i], SortOrder = i, IsOneNowMandatory = titles[i].StartsWith("[OneNow bắt buộc]") });
    }
    db.Scorecards.Add(new Scorecard { ProfileId = p.Id });
    await db.SaveChangesAsync();
    return p;
 }
}
