using System.Text;
using Microsoft.EntityFrameworkCore;
using PresalesDemo;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PresalesDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("PresalesDemo")));
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/health", () => new { ok = true, app = "PresalesDemo", time = DateTime.UtcNow });

app.MapGet("/api/industries", () =>
{
    var all = SeedData.Industries.Concat(SeedDataExtra.More()).Select(ind => new
    {
        ind.Key, ind.Name, ind.Summary, ind.Specifics, ind.Personas, ind.Audience, ind.Objections,
        presenter = new[] {
            new { stage = "00-05", title = "Mở đầu và xác nhận mục tiêu", note = "Giới thiệu ngắn, xác nhận lại 3 vấn đề cần giải quyết." },
            new { stage = "05-15", title = "Hiểu quy trình thực tế", note = "Làm rõ tình huống, độ ưu tiên và tiêu chí đánh giá." },
            new { stage = "15-45", title = "Demo 3 bài toán trọng tâm", note = "Mỗi bài 10 phút: vấn đề -> thao tác -> kết quả -> xác nhận. Sau mỗi bài hỏi: Với cách xử lý vừa trình bày, anh/chị thấy đã phù hợp với quy trình hiện tại chưa? Có điểm nào bắt buộc phải khác đi không?" },
            new { stage = "45-55", title = "Thảo luận khả năng đáp ứng", note = "Giải đáp điểm còn băn khoăn, tích hợp, triển khai, giới hạn giải pháp." },
            new { stage = "55-60", title = "Thống nhất bước tiếp theo", note = "Xác nhận mức độ phù hợp và bước đánh giá để tiến tới quyết định." },
        },
        checkpoint = "Với cách xử lý vừa trình bày, anh/chị thấy đã phù hợp với quy trình hiện tại chưa? Có điểm nào bắt buộc phải khác đi không?",
        closing = new[] {
            "Qua những nội dung vừa trao đổi, em muốn xác nhận lại 3 vấn đề ưu tiên của anh/chị là gì, và anh/chị đánh giá mức độ phù hợp của giải pháp như thế nào?",
            "Để đi đến quyết định lựa chọn nhà cung cấp, ngoài những nội dung đã trình bày, anh/chị còn cần kiểm chứng hoặc làm rõ những yếu tố nào?",
            "Nếu các nội dung đó được xác nhận đáp ứng, bước tiếp theo trong quy trình đánh giá của công ty mình sẽ là gì?",
        }
    });
    return Results.Ok(all);
});

app.MapGet("/api/profiles", async (PresalesDbContext db) =>
    Results.Ok(await db.Profiles.OrderBy(p => p.Id).ToListAsync()));

app.MapGet("/api/profiles/{id:int}", async (int id, PresalesDbContext db) =>
{
    var p = await db.Profiles.FindAsync(id);
    if (p is null) return Results.NotFound();
    return Results.Ok(new
    {
        p.Id, p.Name, p.Industry, p.Locations, p.EmployeeCount, p.IsDefault, p.IsDemoSample,
        details = new { systemInfo = p.SystemInfo, closingJson = p.ClosingJson, actionPlanJson = p.ActionPlanJson, prioConfirmedJson = p.PrioConfirmedJson, healthCsv = p.HealthCsv, prepCsv = p.PrepCsv, estimatorHead = p.EstimatorHead, estimatorDays = p.EstimatorDays, estimatorPeople = p.EstimatorPeople, payrollDeepAnswer = p.PayrollDeepAnswer },
        discovery = await db.DiscoveryItems.Where(x => x.ProfileId == id).OrderBy(x => x.SortOrder).ToListAsync(),
        priorities = await db.Priorities.Where(x => x.ProfileId == id).OrderBy(x => x.Rank).ToListAsync(),
        scenarios = await db.Scenarios.Where(x => x.ProfileId == id).OrderBy(x => x.SortOrder).ToListAsync(),
        fitGap = await db.FitGapRows.Where(x => x.ProfileId == id).OrderBy(x => x.SortOrder).ToListAsync(),
        scorecard = await db.Scorecards.FirstOrDefaultAsync(x => x.ProfileId == id),
        actions = await db.Actions.Where(x => x.ProfileId == id).OrderBy(x => x.Id).ToListAsync(),
        followUps = await db.FollowUps.Where(x => x.ProfileId == id).OrderBy(x => x.Id).ToListAsync(),
        checklist = await db.ChecklistItems.Where(x => x.ProfileId == id).OrderBy(x => x.SortOrder).ToListAsync(),
    });
});

app.MapPost("/api/profiles", async (ProfileCreateDto dto, PresalesDbContext db) =>
{
    if (dto.Industry is not ("Logistic" or "FnB" or "Hotel" or "Sản xuất")) return Results.BadRequest("Industry must be Logistic, FnB, Hotel or Sản xuất.");
    if (string.IsNullOrWhiteSpace(dto.Name)) return Results.BadRequest("Profile name is required.");
    if (await db.Profiles.AnyAsync(x => x.Name.ToLower() == dto.Name.Trim().ToLower())) return Results.Conflict("A profile with this name already exists. Choose a different name or open the existing profile.");
    if (dto.Locations < 0 || dto.EmployeeCount < 0) return Results.BadRequest("Locations and EmployeeCount must be non-negative integers.");
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
        var sc = await db.Scorecards.FirstOrDefaultAsync(x => x.ProfileId == tpl.Id);
        if (sc is not null) db.Scorecards.Add(new Scorecard { ProfileId = p.Id, UnderstandCustomer = sc.UnderstandCustomer, RightProblem = sc.RightProblem, BusinessValue = sc.BusinessValue, HandleObjections = sc.HandleObjections, NextStep = sc.NextStep });
        foreach (var x in await db.Actions.Where(x => x.ProfileId == tpl.Id).ToListAsync()) db.Actions.Add(new ActionItem { ProfileId = p.Id, Title = x.Title, Owner = x.Owner, Deadline = x.Deadline, Source = x.Source });
        foreach (var x in await db.FollowUps.Where(x => x.ProfileId == tpl.Id).ToListAsync()) db.FollowUps.Add(new FollowUp { ProfileId = p.Id, Title = x.Title, Owner = x.Owner, Deadline = x.Deadline, SourceFitGap = x.SourceFitGap });
        foreach (var x in await db.ChecklistItems.Where(x => x.ProfileId == tpl.Id).ToListAsync()) db.ChecklistItems.Add(new ChecklistItem { ProfileId = p.Id, Group = x.Group, Title = x.Title, IsDone = false, IsOneNowMandatory = x.IsOneNowMandatory, SortOrder = x.SortOrder });
        await db.SaveChangesAsync();
    }
    return Results.Created($"/api/profiles/{p.Id}", p);
});

app.MapPost("/api/profiles/{id:int}/duplicate", async (int id, PresalesDbContext db) =>
{
    var src = await db.Profiles.FindAsync(id);
    if (src is null) return Results.NotFound();
    var dto = new ProfileCreateDto(src.Name + " (bản sao)", src.Industry, src.Locations, src.EmployeeCount);
    // Reuse the create logic by copying directly from src.
    var p = new CustomerProfile { Name = dto.Name, Industry = dto.Industry, Locations = dto.Locations, EmployeeCount = dto.EmployeeCount, IsDefault = false, IsDemoSample = src.IsDemoSample, SystemInfo = src.SystemInfo, ClosingJson = src.ClosingJson, ActionPlanJson = src.ActionPlanJson, PrioConfirmedJson = src.PrioConfirmedJson, HealthCsv = src.HealthCsv, PrepCsv = src.PrepCsv, EstimatorHead = src.EstimatorHead, EstimatorDays = src.EstimatorDays, EstimatorPeople = src.EstimatorPeople, PayrollDeepAnswer = src.PayrollDeepAnswer };
    db.Profiles.Add(p); await db.SaveChangesAsync();
    foreach (var d in await db.DiscoveryItems.Where(x => x.ProfileId == src.Id).ToListAsync()) db.DiscoveryItems.Add(new DiscoveryItem { ProfileId = p.Id, Group = d.Group, Question = d.Question, Answer = d.Answer, SortOrder = d.SortOrder });
    foreach (var x in await db.Priorities.Where(x => x.ProfileId == src.Id).ToListAsync()) db.Priorities.Add(new PriorityItem { ProfileId = p.Id, Rank = x.Rank, Title = x.Title, Description = x.Description, ConfirmedStatus = x.ConfirmedStatus });
    foreach (var x in await db.Scenarios.Where(x => x.ProfileId == src.Id).ToListAsync()) db.Scenarios.Add(new Scenario { ProfileId = p.Id, SortOrder = x.SortOrder, Title = x.Title, Pain = x.Pain, Solution = x.Solution, Outcome = x.Outcome, Value = x.Value, ScreenFlow = x.ScreenFlow, StorySteps = x.StorySteps, StoryEdited = x.StoryEdited, BeforeText = x.BeforeText, AfterText = x.AfterText });
    foreach (var x in await db.FitGapRows.Where(x => x.ProfileId == src.Id).ToListAsync()) db.FitGapRows.Add(new FitGapRow { ProfileId = p.Id, Requirement = x.Requirement, Status = x.Status, Note = x.Note, FollowOwner = x.FollowOwner, FollowDue = x.FollowDue, SortOrder = x.SortOrder });
    var sc = await db.Scorecards.FirstOrDefaultAsync(x => x.ProfileId == src.Id);
    if (sc is not null) db.Scorecards.Add(new Scorecard { ProfileId = p.Id, UnderstandCustomer = sc.UnderstandCustomer, RightProblem = sc.RightProblem, BusinessValue = sc.BusinessValue, HandleObjections = sc.HandleObjections, NextStep = sc.NextStep });
    foreach (var x in await db.Actions.Where(x => x.ProfileId == src.Id).ToListAsync()) db.Actions.Add(new ActionItem { ProfileId = p.Id, Title = x.Title, Owner = x.Owner, Deadline = x.Deadline, Source = x.Source });
    foreach (var x in await db.FollowUps.Where(x => x.ProfileId == src.Id).ToListAsync()) db.FollowUps.Add(new FollowUp { ProfileId = p.Id, Title = x.Title, Owner = x.Owner, Deadline = x.Deadline, SourceFitGap = x.SourceFitGap });
    foreach (var x in await db.ChecklistItems.Where(x => x.ProfileId == src.Id).ToListAsync()) db.ChecklistItems.Add(new ChecklistItem { ProfileId = p.Id, Group = x.Group, Title = x.Title, IsDone = x.IsDone, IsOneNowMandatory = x.IsOneNowMandatory, SortOrder = x.SortOrder });
    await db.SaveChangesAsync();
    return Results.Created($"/api/profiles/{p.Id}", p);
});

app.MapPut("/api/profiles/{id:int}", async (int id, ProfileUpdateDto dto, PresalesDbContext db) =>
{
    var p = await db.Profiles.FindAsync(id);
    if (p is null) return Results.NotFound();
    p.Name = dto.Name; p.Locations = dto.Locations; p.EmployeeCount = dto.EmployeeCount; p.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync(); return Results.Ok(p);
});

app.MapDelete("/api/profiles/{id:int}", async (int id, PresalesDbContext db) =>
{
    var p = await db.Profiles.FindAsync(id);
    if (p is null) return Results.NotFound();
    if (p.IsDefault) return Results.BadRequest("Không xóa hồ sơ mặc định theo ngành. Hãy nhân bản trước rồi làm việc trên bản sao.");
    db.DiscoveryItems.RemoveRange(db.DiscoveryItems.Where(x => x.ProfileId == id));
    db.Priorities.RemoveRange(db.Priorities.Where(x => x.ProfileId == id));
    db.Scenarios.RemoveRange(db.Scenarios.Where(x => x.ProfileId == id));
    db.FitGapRows.RemoveRange(db.FitGapRows.Where(x => x.ProfileId == id));
    db.Scorecards.RemoveRange(db.Scorecards.Where(x => x.ProfileId == id));
    db.Actions.RemoveRange(db.Actions.Where(x => x.ProfileId == id));
    db.FollowUps.RemoveRange(db.FollowUps.Where(x => x.ProfileId == id));
    db.ChecklistItems.RemoveRange(db.ChecklistItems.Where(x => x.ProfileId == id));
    db.Profiles.Remove(p); await db.SaveChangesAsync(); return Results.NoContent();
});

app.MapPut("/api/profiles/{id:int}/discovery/{itemId:int}", async (int id, int itemId, DiscoveryUpdateDto dto, PresalesDbContext db) =>
{
    var x = await db.DiscoveryItems.FirstOrDefaultAsync(v => v.Id == itemId && v.ProfileId == id);
    if (x is null) return Results.NotFound();
    x.Answer = dto.Answer; await db.SaveChangesAsync(); return Results.Ok(x);
});

app.MapPut("/api/profiles/{id:int}/fitgap/{itemId:int}", async (int id, int itemId, FitGapUpdateDto dto, PresalesDbContext db) =>
{
    var allowed = new[] { "Đáp ứng sẵn", "Đáp ứng qua cấu hình", "Cần phát triển thêm", "Chưa đáp ứng" };
    if (!allowed.Contains(dto.Status)) return Results.BadRequest("Fit-Gap status must be one of the 4 states.");
    var x = await db.FitGapRows.FirstOrDefaultAsync(v => v.Id == itemId && v.ProfileId == id);
    if (x is null) return Results.NotFound();
    x.Status = dto.Status; x.Note = dto.Note;
    if (dto.Requirement is not null) x.Requirement = dto.Requirement;
    if (dto.FollowOwner is not null) x.FollowOwner = dto.FollowOwner;
    if (dto.FollowDue is not null) x.FollowDue = dto.FollowDue;
    await db.SaveChangesAsync();
    await Helper.RegenerateFollowUps(db, id);
    return Results.Ok(x);
});

app.MapPost("/api/profiles/{id:int}/fitgap", async (int id, FitGapRow row, PresalesDbContext db) =>
{
    row.ProfileId = id; row.Id = 0; db.FitGapRows.Add(row); await db.SaveChangesAsync();
    await Helper.RegenerateFollowUps(db, id); return Results.Created($"/api/profiles/{id}", row);
});

app.MapDelete("/api/profiles/{id:int}/fitgap/{itemId:int}", async (int id, int itemId, PresalesDbContext db) =>
{
    var x = await db.FitGapRows.FirstOrDefaultAsync(v => v.Id == itemId && v.ProfileId == id);
    if (x is null) return Results.NotFound();
    db.FitGapRows.Remove(x); await db.SaveChangesAsync(); await Helper.RegenerateFollowUps(db, id); return Results.NoContent();
});



app.MapPut("/api/profiles/{id:int}/details", async (int id, ProfileDetailsDto dto, PresalesDbContext db) =>
{
    var p = await db.Profiles.FindAsync(id);
    if (p is null) return Results.NotFound();
    p.SystemInfo = dto.SystemInfo; p.ClosingJson = dto.ClosingJson; p.ActionPlanJson = dto.ActionPlanJson;
    p.PrioConfirmedJson = dto.PrioConfirmedJson; p.HealthCsv = dto.HealthCsv; p.PrepCsv = dto.PrepCsv;
    p.EstimatorHead = dto.EstimatorHead; p.EstimatorDays = dto.EstimatorDays; p.EstimatorPeople = dto.EstimatorPeople;
    p.PayrollDeepAnswer = dto.PayrollDeepAnswer; p.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync(); return Results.Ok(p);
});

app.MapPut("/api/profiles/{id:int}/scenarios/{scenarioId:int}", async (int id, int scenarioId, ScenarioUpdateDto dto, PresalesDbContext db) =>
{
    var x = await db.Scenarios.FirstOrDefaultAsync(v => v.Id == scenarioId && v.ProfileId == id);
    if (x is null) return Results.NotFound();
    x.Title = dto.Title; x.Pain = dto.Pain; x.Solution = dto.Solution; x.Outcome = dto.Outcome; x.Value = dto.Value;
    x.ScreenFlow = dto.ScreenFlow; x.StorySteps = dto.StorySteps; x.StoryEdited = dto.StoryEdited; x.BeforeText = dto.BeforeText; x.AfterText = dto.AfterText;
    await db.SaveChangesAsync(); return Results.Ok(x);
});

app.MapPut("/api/profiles/{id:int}/priorities/{priorityId:int}", async (int id, int priorityId, PriorityUpdateDto dto, PresalesDbContext db) =>
{
    var x = await db.Priorities.FirstOrDefaultAsync(v => v.Id == priorityId && v.ProfileId == id);
    if (x is null) return Results.NotFound();
    x.Title = dto.Title; x.Description = dto.Description; x.ConfirmedStatus = dto.ConfirmedStatus;
    await db.SaveChangesAsync(); return Results.Ok(x);
});

app.MapPut("/api/profiles/{id:int}/actions/{actionId:int}", async (int id, int actionId, ActionUpdateDto dto, PresalesDbContext db) =>
{
    var x = await db.Actions.FirstOrDefaultAsync(v => v.Id == actionId && v.ProfileId == id);
    if (x is null) return Results.NotFound();
    x.Title = dto.Title; x.Owner = dto.Owner; x.Deadline = dto.Deadline; x.Source = dto.Source;
    await db.SaveChangesAsync(); return Results.Ok(x);
});

app.MapPut("/api/profiles/{id:int}/followups/{followUpId:int}", async (int id, int followUpId, FollowUpUpdateDto dto, PresalesDbContext db) =>
{
    var x = await db.FollowUps.FirstOrDefaultAsync(v => v.Id == followUpId && v.ProfileId == id);
    if (x is null) return Results.NotFound();
    x.Owner = dto.Owner; x.Deadline = dto.Deadline;
    await db.SaveChangesAsync(); return Results.Ok(x);
});

app.MapPut("/api/profiles/{id:int}/scorecard", async (int id, ScorecardUpdateDto dto, PresalesDbContext db) =>
{
    if (dto.UnderstandCustomer is < 0 or > 25 || dto.RightProblem is < 0 or > 25 || dto.BusinessValue is < 0 or > 20 || dto.HandleObjections is < 0 or > 15 || dto.NextStep is < 0 or > 15)
        return Results.BadRequest("Scorecard limits: 25/25/20/15/15, total 100.");
    var sc = await db.Scorecards.FirstOrDefaultAsync(x => x.ProfileId == id);
    if (sc is null) { sc = new Scorecard { ProfileId = id }; db.Scorecards.Add(sc); }
    sc.UnderstandCustomer = dto.UnderstandCustomer; sc.RightProblem = dto.RightProblem; sc.BusinessValue = dto.BusinessValue; sc.HandleObjections = dto.HandleObjections; sc.NextStep = dto.NextStep;
    await db.SaveChangesAsync(); return Results.Ok(sc);
});

app.MapPost("/api/profiles/{id:int}/actions", async (int id, ActionCreateDto dto, PresalesDbContext db) =>
{
    var a = new ActionItem { ProfileId = id, Title = dto.Title, Owner = dto.Owner, Deadline = dto.Deadline, Source = dto.Source };
    db.Actions.Add(a); await db.SaveChangesAsync(); return Results.Created($"/api/profiles/{id}", a);
});

app.MapPut("/api/profiles/{id:int}/checklist/{itemId:int}", async (int id, int itemId, ChecklistToggleDto dto, PresalesDbContext db) =>
{
    var x = await db.ChecklistItems.FirstOrDefaultAsync(v => v.Id == itemId && v.ProfileId == id);
    if (x is null) return Results.NotFound();
    x.IsDone = dto.IsDone; await db.SaveChangesAsync(); return Results.Ok(x);
});

// Exports as downloadable text files. The header in each export uses the official OneNow logo path in print,
// and the frontend print view shows the logo image with the wordmark fallback.
app.MapGet("/api/profiles/{id:int}/exports/{kind}", async (int id, string kind, PresalesDbContext db) =>
{
    var p = await db.Profiles.FindAsync(id);
    if (p is null) return Results.NotFound();
    var sb = new StringBuilder();
    sb.AppendLine("OneNow - Presales Demo");
    sb.AppendLine("Logo: /images/onenow-logo.png (logo chinh thuc OneNow; neu file chua co, dung wordmark OneNow mau navy/royal-blue)");
    sb.AppendLine($"Ho so: {p.Name} | Nganh: {p.Industry} | Dia diem: {p.Locations} | Nhan vien: {p.EmployeeCount}");
    sb.AppendLine("Day la du lieu DEMO MAU, khong phai khach hang that. Gia, timeline, lien he la placeholder.");
    sb.AppendLine();
    if (kind == "discovery")
    {
        sb.AppendLine("== PHIEU DISCOVERY (8 nhom Playbook) ==");
        foreach (var d in await db.DiscoveryItems.Where(x => x.ProfileId == id).OrderBy(x => x.SortOrder).ToListAsync())
            sb.AppendLine($"- [{d.Group}] {d.Question}\n  Tra loi: {d.Answer}");
        sb.AppendLine("\n== 3 VAN DE UU TIEN ==");
        foreach (var x in await db.Priorities.Where(x => x.ProfileId == id).OrderBy(x => x.Rank).ToListAsync())
            sb.AppendLine($"- {x.Rank}. {x.Title}: {x.Description}");
    }
    else if (kind == "matrix")
    {
        sb.AppendLine("== SCENARIO MATRIX (Pain - Solution - Outcome - Value) ==");
        foreach (var s in await db.Scenarios.Where(x => x.ProfileId == id).OrderBy(x => x.SortOrder).ToListAsync())
            sb.AppendLine($"\n* {s.Title}\n  Pain: {s.Pain}\n  Solution: {s.Solution}\n  Outcome: {s.Outcome}\n  Value: {s.Value}\n  Luong man hinh: {s.ScreenFlow}\n  Truoc: {s.BeforeText}\n  Sau: {s.AfterText}");
        sb.AppendLine("\n== FIT-GAP ==");
        foreach (var f in await db.FitGapRows.Where(x => x.ProfileId == id).OrderBy(x => x.SortOrder).ToListAsync())
            sb.AppendLine($"- {f.Requirement} | {f.Status} | {f.Note}");
    }
    else if (kind == "action")
    {
        sb.AppendLine("== ACTION PLAN - 3 CAU HOI CHOT ==");
        sb.AppendLine("1. Xac nhan lai 3 van de uu tien va muc do phu hop cua giai phap?");
        sb.AppendLine("2. Con can kiem chung hoac lam ro yeu to nao?");
        sb.AppendLine("3. Neu dap ung, buoc tiep theo trong quy trinh danh gia cua cong ty la gi?");
        sb.AppendLine("\n== DAU VIEC ==");
        foreach (var a in await db.Actions.Where(x => x.ProfileId == id).ToListAsync())
            sb.AppendLine($"- {a.Title} | Phu trach: {a.Owner} | Han: {a.Deadline}");
        sb.AppendLine("\n== DAU VIEC THEO DOI TU FIT-GAP ==");
        foreach (var f in await db.FollowUps.Where(x => x.ProfileId == id).ToListAsync())
            sb.AppendLine($"- {f.Title} | Phu trach: {f.Owner} | Han: {f.Deadline}");
        var sc = await db.Scorecards.FirstOrDefaultAsync(x => x.ProfileId == id);
        if (sc is not null) sb.AppendLine($"\nScorecard: {sc.Total}/100 (25/25/20/15/15)");
    }
    else if (kind == "checklist")
    {
        sb.AppendLine("== 02 DEMO PREPARATION CHECKLIST (Playbook 18 muc, 6 nhom + 3 muc bat buoc OneNow) ==");
        foreach (var c in await db.ChecklistItems.Where(x => x.ProfileId == id).OrderBy(x => x.SortOrder).ToListAsync())
            sb.AppendLine($"- [{(c.IsDone ? "x" : " ")}] {c.Group}: {c.Title}");
    }
    else if (kind == "feedback")
    {
        sb.AppendLine("== 04 DEMO FEEDBACK & FIT-GAP (Playbook: chi 4 trang thai, khong cam ket tuy chinh truoc danh gia ky thuat) ==");
        foreach (var f in await db.FitGapRows.Where(x => x.ProfileId == id).OrderBy(x => x.SortOrder).ToListAsync())
            sb.AppendLine($"- {f.Requirement} | {f.Status} | Ghi chu: {f.Note} | Phu trach theo doi: {f.FollowOwner} | Han: {f.FollowDue}");
        sb.AppendLine("\n== DAU VIEC THEO DOI ==");
        foreach (var f in await db.FollowUps.Where(x => x.ProfileId == id).ToListAsync())
            sb.AppendLine($"- {f.Title} | Phu trach: {f.Owner} | Han: {f.Deadline}");
        var sc2 = await db.Scorecards.FirstOrDefaultAsync(x => x.ProfileId == id);
        if (sc2 is not null) sb.AppendLine($"\nScorecard: {sc2.Total}/100");
    }
    else if (kind == "summary")
    {
        sb.AppendLine("== TOM TAT BUOI DEMO ==");
        sb.AppendLine($"Ho so: {p.Name} ({p.Industry})");
        foreach (var s in await db.Scenarios.Where(x => x.ProfileId == id).OrderBy(x => x.SortOrder).ToListAsync())
            sb.AppendLine($"- {s.Title}: {s.Outcome}");
    }
    else return Results.BadRequest("kind must be discovery, matrix, action, summary, checklist or feedback.");
    var bytes = Encoding.UTF8.GetBytes(sb.ToString());
    return Results.File(bytes, "text/plain; charset=utf-8", $"{kind}-{p.Industry}-{p.Id}.txt");
});

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PresalesDbContext>();
    db.Database.EnsureCreated();
    // Add columns added after the DB was first created, without dropping data.
    var addCols = new (string Table, string Col, string Type)[]
    {
        ("Profiles", "SystemInfo", "nvarchar(max)"), ("Profiles", "ClosingJson", "nvarchar(max)"),
        ("Profiles", "ActionPlanJson", "nvarchar(max)"), ("Profiles", "PrioConfirmedJson", "nvarchar(max)"),
        ("Profiles", "HealthCsv", "nvarchar(max)"), ("Profiles", "PrepCsv", "nvarchar(max)"),
        ("Profiles", "EstimatorHead", "nvarchar(50)"), ("Profiles", "EstimatorDays", "nvarchar(50)"),
        ("Profiles", "EstimatorPeople", "nvarchar(50)"), ("Profiles", "PayrollDeepAnswer", "nvarchar(max)"),
        ("Priorities", "ConfirmedStatus", "nvarchar(10)"),
        ("Scenarios", "StoryEdited", "bit"),
        ("FitGapRows", "FollowOwner", "nvarchar(200)"), ("FitGapRows", "FollowDue", "nvarchar(50)"),
    };
    foreach (var (table, col, type) in addCols)
    {
        try { db.Database.ExecuteSqlRaw($"IF COL_LENGTH('{table}', '{col}') IS NULL ALTER TABLE [{table}] ADD [{col}] {type} NOT NULL DEFAULT ''"); } catch { }
    }
    SeedDataExtra.Seed(db);
    // Sync ONLY untouched default-profile priorities to the frontend template
    // titles (INDUSTRIES scenarios in wwwroot/index.html). A default whose
    // priority was already edited (title != old seed title) is left alone.
    try
    {
        var oldSeed = new Dictionary<string, string[]>
        {
            ["Logistic"] = new[] { "Công đa điểm và thiếu log", "Ca đêm, tăng ca và phụ cấp theo tuyến", "Kiểm tra lương trước khi chốt" },
            ["FnB"] = new[] { "Chấm công và xếp ca theo từng cửa hàng", "Nghỉ phép và đổi ca có duyệt, số dư đúng", "Lương bán thời gian theo ca và phụ cấp cửa hàng" },
            ["Hotel"] = new[] { "Ca 24/7 và thiếu log theo bộ phận", "Nghỉ phép mùa cao điểm có duyệt theo bộ phận", "Lương tăng ca lễ, phụ cấp ca đêm và hợp đồng thời vụ" },
            ["Sản xuất"] = new[] { "Công nhà máy tập trung và giải trình công bất thường", "Hợp đồng, tái ký và nghỉ phép theo ca sản xuất", "Lương tăng ca, phụ cấp theo ca và kiểm tra trước chốt" },
        };
        var tpl = new Dictionary<string, string[]>
        {
            ["Logistic"] = new[] { "Công từ nhiều điểm tập trung & xử lý thiếu log", "Ca đêm & tăng ca được tổng hợp, đối chiếu trước lương", "C&B kiểm tra lương, phụ cấp & sai lệch trước khi chốt" },
            ["FnB"] = new[] { "Chấm công theo cửa hàng & ca — xử lý đổi ca, thiếu log", "Nghỉ phép trên ESS — số dư chạy đúng theo duyệt / từ chối / hủy", "Lương bán thời gian theo ca & tiếp nhận nhanh cửa hàng mới" },
            ["Hotel"] = new[] { "Ca 24/7 & thiếu log được phát hiện, giải trình rõ ràng", "Nghỉ phép mùa cao điểm — duyệt theo bộ phận, số dư chính xác", "Lương, tăng ca lễ & hợp đồng thời vụ" },
            ["Sản xuất"] = new[] { "Công nhà máy tập trung — phát hiện công bất thường & giải trình", "Hợp đồng, tái ký & nghỉ phép theo ca sản xuất", "Lương theo công, tăng ca & phụ cấp — công thức 23 thành phần" },
        };
        var changed = false;
        foreach (var prof in await db.Profiles.Where(p => p.IsDefault).ToListAsync())
        {
            if (!oldSeed.TryGetValue(prof.Industry, out var oldT) || !tpl.TryGetValue(prof.Industry, out var newT)) continue;
            foreach (var pr in await db.Priorities.Where(x => x.ProfileId == prof.Id).OrderBy(x => x.Rank).ToListAsync())
            {
                var idx = pr.Rank - 1;
                if (idx >= 0 && idx < 3 && pr.Title == oldT[idx] && pr.Title != newT[idx]) { pr.Title = newT[idx]; changed = true; }
            }
        }
        if (changed) await db.SaveChangesAsync();
    }
    catch { }
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Database init/seed failed. Check ConnectionStrings:PresalesDemo in appsettings.json (SQL auth, no Trusted_Connection). The frontend will still load, but API calls needing the DB will fail until the connection string is fixed on the PC.");
}

app.Run();

public static class Helper
{
    public static async Task RegenerateFollowUps(PresalesDbContext db, int profileId)
    {
        // Preserve owner/deadline already entered for the same requirement, and
        // prefer the owner/deadline entered on the FitGap row itself (single source).
        var old = await db.FollowUps.Where(x => x.ProfileId == profileId).ToListAsync();
        var oldByReq = old.ToDictionary(x => x.SourceFitGap, x => x, StringComparer.OrdinalIgnoreCase);
        db.FollowUps.RemoveRange(old);
        foreach (var f in await db.FitGapRows.Where(x => x.ProfileId == profileId && (x.Status == "Cần phát triển thêm" || x.Status == "Chưa đáp ứng")).ToListAsync())
        {
            var owner = string.IsNullOrWhiteSpace(f.FollowOwner) ? (oldByReq.TryGetValue(f.Requirement, out var o) ? o.Owner : "Presales") : f.FollowOwner;
            var due = string.IsNullOrWhiteSpace(f.FollowDue) ? (oldByReq.TryGetValue(f.Requirement, out var o2) ? o2.Deadline : "2026-11-17") : f.FollowDue;
            db.FollowUps.Add(new FollowUp { ProfileId = profileId, Title = "Làm rõ: " + f.Requirement, Owner = owner, Deadline = due, SourceFitGap = f.Requirement });
        }
        await db.SaveChangesAsync();
    }
}

