using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PresalesDemo;

public static class CrmApi
{
    public static readonly string[] Statuses = ["MoiKhaoSat", "DangChuanBi", "DaDemo", "DaBaoGia", "Chot", "Mat"];
    public static readonly string[] StatusNames = ["Mới khảo sát", "Đang chuẩn bị", "Đã demo", "Đã báo giá", "Chốt", "Mất"];
    static readonly string[] Types = ["GhiChu", "CuocGoi", "Email", "GapMat", "Demo", "BaoGia"];
    static readonly string[] AppointmentStatuses = ["SapDienRa", "DaDienRa", "DaHuy"];
    static readonly string[] Industries = ["Logistic", "FnB", "Hotel", "Sản xuất"];
    public static async Task<IResult> ListProfiles(HttpRequest request, PresalesDbContext db)
    {
        var q = db.Profiles.AsNoTracking().AsQueryable();
        string Get(string key) => request.Query[key].ToString();
        var search = Get("search").Trim(); var industry = Get("industry"); var status = Get("status");
        if (search.Length > 0) q = q.Where(p => p.Name.Contains(search) || db.SurveyResponses.Any(s => s.ProfileId == p.Id && (s.ContactName.Contains(search) || s.ContactEmail.Contains(search))));
        if (industry != "") q = q.Where(p => p.Industry == industry);
        if (status != "") q = q.Where(p => p.Status == status);
        if (Get("hasSurvey") != "") {
            if (!bool.TryParse(Get("hasSurvey"), out var has)) return Results.BadRequest("hasSurvey phải là true/false.");
            q = q.Where(p => db.SurveyResponses.Any(s => s.ProfileId == p.Id) == has);
        }
        q = Get("sort") switch {
            "updated" => q.OrderByDescending(p => p.UpdatedAt),
            "name" => q.OrderBy(p => p.Name),
            "demoDate" => q.OrderBy(p => p.DemoDate == null).ThenBy(p => p.DemoDate),
            _ => q.OrderBy(p => p.Id)
        };
        // Preserve every legacy profile field in the default response; metadata is additive.
        var profiles = await q.ToListAsync();
        var ids = profiles.Select(p => p.Id).ToArray();
        var surveys = await db.SurveyResponses.Where(s => s.ProfileId != null && ids.Contains(s.ProfileId.Value)).OrderByDescending(s => s.SubmittedAt).ThenByDescending(s => s.Id).ToListAsync();
        return Results.Ok(profiles.Select(p => {
            var row = JsonSerializer.SerializeToElement(p, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var result = row.EnumerateObject().ToDictionary(x => x.Name, x => (object?)x.Value);
            var survey = surveys.FirstOrDefault(s => s.ProfileId == p.Id);
            result["hasSurvey"] = survey != null; result["surveySubmittedAt"] = survey?.SubmittedAt;
            result["contactName"] = survey?.ContactName; result["contactEmail"] = survey?.ContactEmail;
            return result;
        }));
    }
    static void Touch(CustomerProfile p) => p.UpdatedAt = DateTime.UtcNow;
    static string? OptionalText(JsonElement value) => value.ValueKind == JsonValueKind.String ? value.GetString()?.Trim() : null;
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/profiles/{id:int}/status", async (int id, StatusDto dto, PresalesDbContext db) => {
            if (!Statuses.Contains(dto.Status)) return Results.BadRequest("Trạng thái không hợp lệ.");
            DateTime? date = null;
            if (dto.DemoDate.ValueKind is not (JsonValueKind.Undefined or JsonValueKind.Null) && !(dto.DemoDate.ValueKind == JsonValueKind.String && dto.DemoDate.GetString() == "")) {
                if (dto.DemoDate.ValueKind != JsonValueKind.String || !DateTime.TryParse(dto.DemoDate.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)) return Results.BadRequest("Ngày demo không hợp lệ.");
                date = parsed.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(parsed, DateTimeKind.Utc) : parsed.ToUniversalTime();
            }
            if (dto.Owner.ValueKind is not (JsonValueKind.Undefined or JsonValueKind.Null or JsonValueKind.String) || dto.FollowUpNote.ValueKind is not (JsonValueKind.Undefined or JsonValueKind.Null or JsonValueKind.String) || OptionalText(dto.Owner)?.Length > 100 || OptionalText(dto.FollowUpNote)?.Length > 1000) return Results.BadRequest("Chủ sở hữu tối đa 100, ghi chú tối đa 1000 ký tự.");
            var p = await db.Profiles.FindAsync(id); if (p == null) return Results.NotFound();
            if (p.Status != dto.Status) db.Interactions.Add(new Interaction { ProfileId = id, Note = "Đổi trạng thái → " + StatusNames[Array.IndexOf(Statuses, dto.Status)] });
            p.Status = dto.Status;
            if (dto.DemoDate.ValueKind != JsonValueKind.Undefined) p.DemoDate = date;
            if (dto.Owner.ValueKind != JsonValueKind.Undefined) p.Owner = OptionalText(dto.Owner);
            if (dto.FollowUpNote.ValueKind != JsonValueKind.Undefined) p.FollowUpNote = OptionalText(dto.FollowUpNote);
            Touch(p); await db.SaveChangesAsync(); return Results.Ok(p);
        });
        app.MapGet("/api/profiles/{id:int}/interactions", async (int id, PresalesDbContext db) => !await db.Profiles.AnyAsync(p => p.Id == id) ? (IResult)Results.NotFound() : Results.Ok(await db.Interactions.Where(x => x.ProfileId == id).OrderByDescending(x => x.OccurredAt).ThenByDescending(x => x.Id).ToListAsync()));
        app.MapPost("/api/profiles/{id:int}/interactions", async (int id, Interaction input, PresalesDbContext db) => {
            if (!Types.Contains(input.Type) || string.IsNullOrWhiteSpace(input.Note) || input.Note.Length > 2000 || input.CreatedBy?.Length > 100 || input.OccurredAt == default) return Results.BadRequest("Loại, ngày giờ hoặc ghi chú không hợp lệ (1–2000 ký tự).");
            var p = await db.Profiles.FindAsync(id); if (p == null) return Results.NotFound();
            input.Id = 0; input.ProfileId = id; input.Note = input.Note.Trim(); input.CreatedAt = DateTime.UtcNow;
            db.Interactions.Add(input); Touch(p); await db.SaveChangesAsync(); return Results.Ok(input);
        });
        app.MapDelete("/api/profiles/{id:int}/interactions/{iid:int}", async (int id, int iid, PresalesDbContext db) => {
            var x = await db.Interactions.FirstOrDefaultAsync(x => x.ProfileId == id && x.Id == iid); if (x == null) return Results.NotFound();
            db.Interactions.Remove(x); var p = await db.Profiles.FindAsync(id); if (p != null) Touch(p); await db.SaveChangesAsync(); return Results.NoContent();
        });
        app.MapGet("/api/appointments", async (DateTime? from, DateTime? to, string? status, PresalesDbContext db) => {
            if (status != null && !AppointmentStatuses.Contains(status) || from > to) return Results.BadRequest("Bộ lọc lịch không hợp lệ.");
            var q = db.DemoAppointments.AsNoTracking().Where(x => (!from.HasValue || x.StartsAt >= from) && (!to.HasValue || x.StartsAt <= to) && (status == null || x.Status == status));
            return Results.Ok(await q.Join(db.Profiles, a => a.ProfileId, p => p.Id, (a,p) => new { a.Id, a.ProfileId, a.Title, a.StartsAt, a.EndsAt, a.Location, a.Notes, a.ReminderMinutes, a.Status, a.CreatedAt, companyName = p.Name, p.Owner, p.IsDefault }).OrderBy(x => x.StartsAt).ToListAsync());
        });
        app.MapGet("/api/profiles/{id:int}/appointments", async (int id, PresalesDbContext db) => !await db.Profiles.AnyAsync(p => p.Id == id) ? (IResult)Results.NotFound() : Results.Ok(await db.DemoAppointments.Where(x => x.ProfileId == id).OrderBy(x => x.StartsAt).ToListAsync()));
        app.MapPost("/api/profiles/{id:int}/appointments", async (int id, DemoAppointment input, PresalesDbContext db) => {
            var error = AppointmentError(input); if (error != null) return Results.BadRequest(error);
            var p = await db.Profiles.FindAsync(id); if (p == null) return Results.NotFound();
            input.Id = 0; input.ProfileId = id; input.CreatedAt = DateTime.UtcNow; input.Title = input.Title.Trim();
            await using var tx = await db.Database.BeginTransactionAsync();
            db.DemoAppointments.Add(input); await db.SaveChangesAsync();
            // Linked interaction records the booking now, so scheduled demos are not counted as completed demos.
            db.Interactions.Add(new Interaction { ProfileId = id, Type = "Demo", OccurredAt = DateTime.UtcNow, Note = "Đặt lịch demo: " + input.Title + " — " + input.StartsAt.ToString("O") + " [appointment:" + input.Id + "]" });
            p.DemoDate = input.StartsAt; Touch(p); await db.SaveChangesAsync(); await tx.CommitAsync(); return Results.Ok(input);
        });
        app.MapPut("/api/appointments/{id:int}", async (int id, DemoAppointment input, PresalesDbContext db) => {
            var error = AppointmentError(input); if (error != null) return Results.BadRequest(error);
            var x = await db.DemoAppointments.FindAsync(id); if (x == null) return Results.NotFound();
            x.Title = input.Title.Trim(); x.StartsAt = input.StartsAt; x.EndsAt = input.EndsAt; x.Location = input.Location; x.Notes = input.Notes; x.ReminderMinutes = input.ReminderMinutes; x.Status = input.Status;
            var p = await db.Profiles.FindAsync(x.ProfileId); if (p != null) { p.DemoDate = x.StartsAt; Touch(p); }
            await db.SaveChangesAsync(); return Results.Ok(x);
        });
        app.MapDelete("/api/appointments/{id:int}", async (int id, PresalesDbContext db) => {
            var x = await db.DemoAppointments.FindAsync(id); if (x == null) return Results.NotFound();
            db.DemoAppointments.Remove(x); var p = await db.Profiles.FindAsync(x.ProfileId); if (p != null) { if (p.DemoDate == x.StartsAt) p.DemoDate = null; Touch(p); }
            await db.SaveChangesAsync(); return Results.NoContent();
        });
        app.MapGet("/api/profiles/{id:int}/attachments", async (int id, PresalesDbContext db) => !await db.Profiles.AnyAsync(p => p.Id == id) ? (IResult)Results.NotFound() : Results.Ok(await db.Attachments.Where(x => x.ProfileId == id).OrderByDescending(x => x.UploadedAt).ToListAsync()));
        app.MapPost("/api/profiles/{id:int}/attachments", Upload);
        app.MapDelete("/api/profiles/{id:int}/attachments/{aid:int}", async (int id, int aid, PresalesDbContext db, IWebHostEnvironment env) => {
            var x = await db.Attachments.FirstOrDefaultAsync(x => x.Id == aid && x.ProfileId == id); if (x == null) return Results.NotFound();
            using var files = new StagedFiles(app.Logger); files.Stage(Path.Combine(env.WebRootPath, "uploads", "attachments", id.ToString(), x.StoredName));
            db.Attachments.Remove(x); var p = await db.Profiles.FindAsync(id); if (p != null) Touch(p); await db.SaveChangesAsync(); files.Commit(); return Results.NoContent();
        });
        app.MapGet("/api/reports/summary", Report);
    }
    static string? AppointmentError(DemoAppointment x) => string.IsNullOrWhiteSpace(x.Title) || x.Title.Length > 200 || x.StartsAt == default || x.EndsAt <= x.StartsAt || !AppointmentStatuses.Contains(x.Status) || x.Location?.Length > 1000 || x.Notes?.Length > 2000 || x.ReminderMinutes < 0 ? "Tiêu đề, thời gian hoặc trạng thái lịch không hợp lệ; kết thúc phải sau bắt đầu." : null;
    static async Task<IResult> Upload(int id, HttpRequest request, PresalesDbContext db, IWebHostEnvironment env)
    {
        var p = await db.Profiles.FindAsync(id); if (p == null) return Results.NotFound();
        if (!request.HasFormContentType) return Results.BadRequest("Cần multipart/form-data.");
        if (request.ContentLength > 12 * 1024 * 1024) return Results.BadRequest("File tối đa 10 MB.");
        var form = await request.ReadFormAsync(); var file = form.Files.GetFile("file");
        if (file == null || file.Length == 0 || file.Length > 10 * 1024 * 1024) return Results.BadRequest("File phải có dữ liệu và tối đa 10 MB.");
        var name = Path.GetFileName(file.FileName.Replace('\\', '/'));
        var ext = Path.GetExtension(name).ToLowerInvariant();
        var mime = new Dictionary<string,string> { [".pdf"]="application/pdf", [".doc"]="application/msword", [".docx"]="application/vnd.openxmlformats-officedocument.wordprocessingml.document", [".xls"]="application/vnd.ms-excel", [".xlsx"]="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", [".png"]="image/png", [".jpg"]="image/jpeg", [".jpeg"]="image/jpeg" };
        if (!mime.ContainsKey(ext)) return Results.BadRequest("Đuôi file không được hỗ trợ.");
        name = Regex.Replace(name, "[^A-Za-z0-9_.-]", "_"); if (name.Length > 180) name = name[..160] + ext;
        var stored = Guid.NewGuid().ToString("N") + "-" + name;
        var directory = Path.Combine(env.WebRootPath, "uploads", "attachments", id.ToString()); Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, stored);
        try {
            await using (var stream = File.Create(path)) await file.CopyToAsync(stream);
            var x = new Attachment { ProfileId = id, FileName = name, StoredName = stored, Size = file.Length, ContentType = mime[ext] };
            db.Attachments.Add(x); Touch(p); await db.SaveChangesAsync(); return Results.Ok(x);
        } catch { if (File.Exists(path)) File.Delete(path); throw; }
    }
    static async Task<IResult> Report(string? month, string? industry, PresalesDbContext db)
    {
        industry = string.IsNullOrWhiteSpace(industry) ? null : industry;
        month = string.IsNullOrWhiteSpace(month) ? DateTime.UtcNow.ToString("yyyy-MM") : month;
        if (!DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start) || start.Year < 2 || start.Year > 9998 || industry != null && !Industries.Contains(industry)) return Results.BadRequest("Tháng hoặc ngành không hợp lệ.");
        var first = start.AddMonths(-5); var end = start.AddMonths(1);
        var profiles = await db.Profiles.AsNoTracking().Where(p => !p.IsDefault && (industry == null || p.Industry == industry)).ToListAsync();
        var ids = profiles.Select(p => p.Id).ToArray();
        var surveys = await db.SurveyResponses.AsNoTracking().Where(s => s.ProfileId.HasValue && ids.Contains(s.ProfileId.Value) && s.SubmittedAt >= first && s.SubmittedAt < end).ToListAsync();
        var interactions = await db.Interactions.AsNoTracking().Where(i => ids.Contains(i.ProfileId) && i.OccurredAt >= first && i.OccurredAt < end).ToListAsync();
        var appointments = await db.DemoAppointments.AsNoTracking().Where(a => ids.Contains(a.ProfileId) && a.Status == "DaDienRa" && a.StartsAt >= first && a.StartsAt < end).ToListAsync();
        object Metrics(DateTime from, string? sector) {
            var allowed = profiles.Where(p => sector == null || p.Industry == sector).Select(p => p.Id).ToHashSet();
            var until = from.AddMonths(1);
            var events = interactions.Where(i => allowed.Contains(i.ProfileId) && i.OccurredAt >= from && i.OccurredAt < until).ToList();
            var won = events.Where(i => i.Type == "GhiChu" && i.Note == "Đổi trạng thái → Chốt").Select(i => i.ProfileId).Distinct().Count();
            var lost = events.Where(i => i.Type == "GhiChu" && i.Note == "Đổi trạng thái → Mất").Select(i => i.ProfileId).Distinct().Count();
            return new {
                surveys = surveys.Count(s => allowed.Contains(s.ProfileId!.Value) && s.SubmittedAt >= from && s.SubmittedAt < until),
                // Booking interactions are linked, counted once through the completed appointment instead.
                demos = events.Count(i => i.Type == "Demo" && !i.Note.Contains(" [appointment:")) + appointments.Count(a => allowed.Contains(a.ProfileId) && a.StartsAt >= from && a.StartsAt < until),
                quotations = events.Count(i => i.Type == "BaoGia"), won, lost,
                winRate = won + lost == 0 ? 0d : (double)won / (won + lost)
            };
        }
        return Results.Ok(new { month, industry, summary = Metrics(start, null), byIndustry = Industries.Select(i => new { industry = i, metrics = Metrics(start, i) }), byMonth = Enumerable.Range(0,6).Select(i => new { month = first.AddMonths(i).ToString("yyyy-MM"), metrics = Metrics(first.AddMonths(i), null) }) });
    }
    // Stage on the same volume; restore on failure before commit. Cleanup failure keeps a recoverable tombstone.
    public sealed class StagedFiles : IDisposable
    {
        readonly List<(string Original, string Staged, bool Directory)> paths = new();
        bool committed;
        readonly ILogger logger;
        public StagedFiles(ILogger logger) => this.logger = logger;
        public void Stage(string path) {
            var directory = Directory.Exists(path); if (!directory && !File.Exists(path)) return;
            var staged = path + ".deleted-" + Guid.NewGuid().ToString("N");
            if (directory) Directory.Move(path, staged); else File.Move(path, staged);
            paths.Add((path, staged, directory));
        }
        public void Commit() {
            committed = true;
            foreach (var p in paths) { try { if (p.Directory) Directory.Delete(p.Staged, true); else File.Delete(p.Staged); } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { logger.LogWarning(ex, "Committed delete needs filesystem cleanup: {Path}", p.Staged); } }
        }
        public void Dispose() {
            if (!committed) foreach (var p in paths.AsEnumerable().Reverse()) { if (p.Directory) Directory.Move(p.Staged, p.Original); else File.Move(p.Staged, p.Original); }
        }
    }
    public static Task Initialize(PresalesDbContext db) => db.Database.ExecuteSqlRawAsync(Schema);
    // CRM schema mirrored from sql/CreateDatabase.sql; tools/check-crm.py verifies parity.
    public const string Schema = """
IF OBJECT_ID(N'Interactions', N'U') IS NULL
BEGIN
CREATE TABLE Interactions (
 Id int IDENTITY PRIMARY KEY, ProfileId int NOT NULL REFERENCES Profiles(Id) ON DELETE CASCADE,
 Type nvarchar(20) NOT NULL, OccurredAt datetime2 NOT NULL, Note nvarchar(2000) NOT NULL,
 CreatedBy nvarchar(100) NULL, CreatedAt datetime2 NOT NULL
);
CREATE INDEX IX_Interactions_ProfileId ON Interactions(ProfileId);
END;
IF OBJECT_ID(N'DemoAppointments', N'U') IS NULL
BEGIN
CREATE TABLE DemoAppointments (
 Id int IDENTITY PRIMARY KEY, ProfileId int NOT NULL REFERENCES Profiles(Id) ON DELETE CASCADE,
 Title nvarchar(200) NOT NULL, StartsAt datetime2 NOT NULL, EndsAt datetime2 NULL,
 Location nvarchar(1000) NULL, Notes nvarchar(2000) NULL, ReminderMinutes int NULL DEFAULT 60,
 Status nvarchar(20) NOT NULL, CreatedAt datetime2 NOT NULL
);
CREATE INDEX IX_DemoAppointments_ProfileId ON DemoAppointments(ProfileId);
END;
IF OBJECT_ID(N'Attachments', N'U') IS NULL
BEGIN
CREATE TABLE Attachments (
 Id int IDENTITY PRIMARY KEY, ProfileId int NOT NULL REFERENCES Profiles(Id) ON DELETE CASCADE,
 FileName nvarchar(255) NOT NULL, StoredName nvarchar(255) NOT NULL, Size bigint NOT NULL,
 ContentType nvarchar(200) NOT NULL, UploadedAt datetime2 NOT NULL
);
CREATE INDEX IX_Attachments_ProfileId ON Attachments(ProfileId);
END;
""";
}
