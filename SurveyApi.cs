using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace PresalesDemo;

public static class SurveyApi
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    static readonly Dictionary<string, string[]> IndustryPainPoints = new()
    {
        ["Logistic"] = new[] { "Chấm công đa điểm, thiếu log (kho / tuyến)", "Tính lương ca đêm, tăng ca, phụ cấp theo tuyến", "Xin phép / nghỉ phép cho tài xế & nhân viên kho", "Tuyển dụng lái xe / nhân viên kho", "BHXH & hợp đồng cho lao động lưu động", "Đánh giá / KPI theo tuyến / kho" },
        ["FnB"] = new[] { "Chấm công & xếp ca theo cửa hàng", "Tính lương bán thời gian theo ca", "Đổi ca / thiếu log, duyệt trước giờ bán", "Xin phép / nghỉ phép trên ESS", "Tuyển dụng & mở cửa hàng mới nhanh", "Đánh giá / KPI theo cửa hàng" },
        ["Hotel"] = new[] { "Chấm công ca 24/7 theo bộ phận", "Tính lương tăng ca lễ, phụ cấp ca đêm", "Nghỉ phép mùa cao điểm", "Hợp đồng thời vụ", "Tuyển dụng lễ tân / buồng phòng mùa cao điểm", "Đánh giá / KPI theo bộ phận" },
        ["Sản xuất"] = new[] { "Chấm công nhà máy tập trung, công bất thường", "Tính lương theo công, tăng ca, phụ cấp ca", "Nghỉ phép theo ca sản xuất", "Hợp đồng & tái ký theo ca", "Tuyển dụng công nhân số lượng lớn", "BHXH & đánh giá năng suất" },
    };
    static readonly Dictionary<string, string[]> IndustryDemoSuggestions = new()
    {
        ["Logistic"] = new[] { "Chấm công đa điểm và xử lý thiếu log cho tài xế / nhân viên kho", "Tính lương ca đêm, tăng ca và phụ cấp theo tuyến, kiểm tra trước khi chốt", "Duyệt nghỉ phép và giải trình công trên điện thoại" },
        ["FnB"] = new[] { "Chấm công và xếp ca theo từng cửa hàng, xử lý đổi ca / thiếu log", "Nghỉ phép trên ESS, số dư chạy đúng khi duyệt / từ chối / hủy", "Tính lương bán thời gian theo ca và phụ cấp từng cửa hàng" },
        ["Hotel"] = new[] { "Chấm công ca 24/7 theo bộ phận và phát hiện thiếu log", "Nghỉ phép mùa cao điểm, duyệt theo bộ phận, số dư chính xác", "Tính lương tăng ca lễ, phụ cấp ca đêm và hợp đồng thời vụ" },
        ["Sản xuất"] = new[] { "Chấm công nhà máy tập trung, phát hiện công bất thường và giải trình", "Tính lương theo công, tăng ca và phụ cấp ca, công thức 23 thành phần", "Quản lý hợp đồng, tái ký và nghỉ phép theo ca sản xuất" },
        ["Khác"] = new[] { "Chấm công và tổng hợp công tự động", "Tính lương và kiểm tra sai lệch trước khi chốt", "Nghỉ phép và duyệt trên điện thoại" },
    };
    public static readonly string[] Types = "short_text long_text single_choice multi_choice dropdown yes_no number email phone date rating scale ranking file_upload picture_choice question_group".Split(' ');
    static string Encode(object value) => JsonSerializer.Serialize(value, Json);
    static List<T> Read<T>(string value) => JsonSerializer.Deserialize<List<T>>(value ?? "[]", Json) ?? new();
    static string Text(JsonElement value) => value.ValueKind == JsonValueKind.String ? value.GetString()!.Trim() : value.ValueKind is JsonValueKind.Number ? value.GetRawText() : "";
    static bool Empty(JsonElement value) => value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null || (value.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(value.GetString())) || (value.ValueKind == JsonValueKind.Array && value.GetArrayLength() == 0);
    static SurveyQuestion Q(int id, string title, string type, string[]? options = null, bool required = false, bool active = true) => new() { Id = id, SortOrder = id, Title = title, Type = type, Required = required, IsActive = active, OptionsJson = Encode((options ?? Array.Empty<string>()).Select(x => new SurveyOption(x))), ScaleMax = type == "rating" ? 5 : type == "scale" ? 10 : null };
    public static List<SurveyQuestion> Defaults()
    {
        var group = Q(1, "Thông tin liên hệ", "question_group", required: true);
        group.Hint = "Để chúng tôi chuẩn bị nội dung và trao đổi với anh/chị về buổi demo.";
        group.FieldsJson = Encode(new[] {
            new SurveyField("contactName", "Họ và tên của anh/chị", "short_text", "Type your answer here...", true),
            new SurveyField("contactEmail", "Email công việc của anh/chị", "email", "name@example.com", true),
            new SurveyField("companyName", "Tên công ty hoặc tổ chức của anh/chị", "short_text", "Type your answer here...", true),
            new SurveyField("role", "Chức danh của anh/chị", "short_text", "Type your answer here...", false) });
        var result = new List<SurveyQuestion> {
            group,
            Q(2,"Công ty thuộc ngành nào?","single_choice",new[]{"Logistic","FnB","Hotel","Sản xuất","Khác"},true),
            Q(3,"Quy mô nhân sự?","single_choice",new[]{"Dưới 50","50–200","200–500","500–1.000","Trên 1.000"}),
            Q(4,"Số địa điểm / chi nhánh?","single_choice",new[]{"1","2–5","6–20","Trên 20"}),
            Q(5,"Hiện tại công ty đang quản lý nhân sự bằng gì?","single_choice",new[]{"Excel","Giấy tờ / sổ sách thủ công","Phần mềm nhân sự khác","Chưa có hệ thống"}),
            Q(6,"Đau nhất hiện tại là gì?","multi_choice",new[]{"Chấm công","Tính lương","Xin phép - Nghỉ phép","Tuyển dụng","BHXH","Đánh giá - KPI"},true),
            Q(7,"Ca làm việc của công ty thế nào?","single_choice",new[]{"Hành chính","Ca xoay","Ca đêm","Hỗn hợp"}),
            Q(8,"Ai là người quyết định cuối cùng khi chọn phần mềm?","single_choice",new[]{"Tôi","Ban giám đốc","HR + IT cùng quyết","Chưa xác định"}),
            Q(9,"Anh/chị dự kiến khi nào sẽ áp dụng giải pháp mới?","single_choice",new[]{"Trong vòng 1 tháng","Trong 1–3 tháng","Trong 3–6 tháng","Sau 6 tháng","Đang tìm hiểu, chưa có thời gian cụ thể"}),
            Q(10,"3 điều anh/chị nhất định muốn thấy trong 60 phút demo?","long_text",required:true),
            Q(11,"Email / số điện thoại liên hệ","short_text",required:true,active:false),
            Q(12,"Mất bao nhiêu ngày để chốt lương mỗi tháng?","single_choice",new[]{"Dưới 1 ngày","1–2 ngày","3–4 ngày","Trên 4 ngày"},active:false),
            Q(13,"Nhân viên xin phép / xem công bằng cách nào?","single_choice",new[]{"Giấy/Excel","Zalo/chat","Phần mềm cũ","Chưa có"},active:false),
            Q(14,"Cần tích hợp gì không?","multi_choice",new[]{"BHXH","Ngân hàng trả lương","Kế toán/ERP","Máy chấm công","Không cần"},active:false),
            Q(15,"Ai sẽ tham dự demo cùng?","multi_choice",new[]{"HR","Kế toán/C&B","IT","Ban giám đốc"},active:false),
            Q(16,"Sau 3 tháng dùng, thế nào gọi là thành công?","long_text",active:false),
            Q(17,"Đã xem qua phần mềm nào khác chưa?","short_text",active:false),
            Q(18,"Ngân sách dự kiến khoảng nào?","single_choice",new[]{"Chưa xác định","Dưới 100 triệu/năm","100–300 triệu/năm","Trên 300 triệu/năm"},active:false),
            Q(19,"Anh/chị đánh giá mức độ hài lòng với hệ thống hiện tại mấy sao?","rating",active:false),
            Q(20,"Anh/chị có muốn nhận báo giá qua email không?","yes_no",new[]{"Có","Không"},active:false),
            Q(21,"Ngày dự kiến muốn demo?","date",active:false)
        };
        result[8].Hint = "Description (optional)";
        return result;
    }
    public static async Task Initialize(PresalesDbContext db)
    {
        // Keep this DDL identical to sql/SurveySchema.sql and CreateDatabase.sql.
        await db.Database.ExecuteSqlRawAsync(Schema);
        if (!await db.SurveyPresentations.AnyAsync()) { db.SurveyPresentations.Add(new SurveyPresentation()); await db.SaveChangesAsync(); }
        if (!await db.SurveyQuestions.AnyAsync())
        {
            // Stable IDs are mapping keys; identity insert is scoped to this connection.
            await db.Database.OpenConnectionAsync();
            try {
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.SurveyQuestions ON");
                db.SurveyQuestions.AddRange(Defaults());
                await db.SaveChangesAsync();
            } finally {
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.SurveyQuestions OFF");
                await db.Database.CloseConnectionAsync();
            }
        }
        // Only migrate the legacy question options; historical responses are untouched.
        var systemQuestion = await db.SurveyQuestions.FindAsync(5);
        if (systemQuestion != null && Read<SurveyOption>(systemQuestion.OptionsJson).Any(o => o.Text == "ezHR9"))
        {
            systemQuestion.OptionsJson = Defaults().First(q => q.Id == 5).OptionsJson;
            await db.SaveChangesAsync();
        }
    }
    // Add API metadata without changing the entity or the Builder's editable options.
    static object QuestionPayload(SurveyQuestion q)
    {
        var payload = JsonSerializer.Deserialize<Dictionary<string, object?>>(Encode(q), Json)!;
        if (q.Id == 6) payload["industryOptions"] = IndustryPainPoints;
        if (q.Id == 10) payload["industrySuggestions"] = IndustryDemoSuggestions;
        return payload;
    }
    static string? DefinitionError(SurveyQuestion q)
    {
        if (!Types.Contains(q.Type) || string.IsNullOrWhiteSpace(q.Title)) return "Anh/chị cần nhập tiêu đề và chọn loại câu hỏi hợp lệ.";
        if (q.Id == 1 && (q.Type != "question_group" || !q.Required || !q.IsActive)) return "Không được bỏ nhóm Thông tin liên hệ.";
        if ((q.Id == 2 || q.Id == 10) && (!q.Required || !q.IsActive)) return "Ngành và 3 điều muốn thấy phải được giữ và bắt buộc.";
        if (q.Id == 2 && q.Type != "single_choice" || q.Id == 10 && q.Type != "long_text") return "Không được đổi loại câu hỏi dùng để tạo hồ sơ.";
        if (q.MinValue > q.MaxValue || q.ScaleMax is < 1 or > 100) return "Giới hạn số không hợp lệ.";
        try {
            var options = Read<SurveyOption>(q.OptionsJson);
            if (options.Any(o => string.IsNullOrWhiteSpace(o.Text)) || options.Select(o=>o.Text).Distinct().Count()!=options.Count) return "Đáp án không được rỗng hoặc trùng.";
            if (new[]{"single_choice","multi_choice","dropdown","ranking","picture_choice","yes_no"}.Contains(q.Type) && options.Count < 2) return "Cần ít nhất hai đáp án.";
            if (q.Type == "yes_no" && !options.Select(o=>o.Text).SequenceEqual(new[]{"Có","Không"})) return "Có/Không phải có đúng hai đáp án Có, Không.";
            if (q.Id == 2 && !new[]{"Logistic","FnB","Hotel","Sản xuất","Khác"}.All(x=>options.Any(o=>o.Text==x))) return "Phải giữ các ngành demo gốc.";
            if (!new[]{"single_choice","multi_choice","dropdown","ranking","picture_choice","yes_no"}.Contains(q.Type)) q.OptionsJson="[]";
            var fields = Read<SurveyField>(q.FieldsJson);
            if (q.Type == "question_group" && (fields.Count == 0 || fields.Any(f=>string.IsNullOrWhiteSpace(f.Key)||string.IsNullOrWhiteSpace(f.Label)||!new[]{"short_text","email","phone","number","date"}.Contains(f.Type)) || fields.Select(f=>f.Key).Distinct().Count()!=fields.Count)) return "Các ô trong nhóm cần khóa riêng, nhãn và loại hợp lệ.";
            if (q.Id == 1 && new[]{("contactName","short_text"),("contactEmail","email"),("companyName","short_text")}.Any(k=>!fields.Any(f=>f.Key==k.Item1 && f.Type==k.Item2 && f.Required))) return "Phải giữ họ tên, email và tên công ty với loại và bắt buộc gốc.";
            if(q.Type!="question_group")q.FieldsJson="[]";
        } catch (Exception ex) when (ex is JsonException or NullReferenceException) { return "Cấu hình đáp án / nhóm không hợp lệ."; }
        if (!string.IsNullOrWhiteSpace(q.MediaUrl)) {
            if (!SafeMedia(q.MediaUrl)) return "Link media phải là HTTPS hoặc file đã tải lên.";
            q.MediaType = MediaType(q.MediaUrl);
            if (q.MediaType == null) return "Link ảnh/video không được hỗ trợ.";
        } else { q.MediaUrl = null; q.MediaType = null; }
        return null;
    }
    static bool SafeMedia(string url) => url.StartsWith("/uploads/survey-media/",StringComparison.Ordinal) && !url.Contains("..") || url == "/images/onenow-logo.png" || Uri.TryCreate(url,UriKind.Absolute,out var uri) && uri.Scheme=="https";
    static string? MediaType(string url) {
        if (Uri.TryCreate(url,UriKind.Absolute,out var u)) {
            if (u.Host is "youtube.com" or "www.youtube.com" or "youtu.be" or "www.youtube-nocookie.com") return "youtube";
            if (u.Host is "vimeo.com" or "www.vimeo.com" or "player.vimeo.com") return "vimeo";
        }
        var ext=Path.GetExtension(url.Split('?')[0]).ToLowerInvariant();
        return ext is ".png" or ".jpg" or ".jpeg" or ".webp" ? "image" : ext is ".mp4" or ".webm" ? "video" : null;
    }
    static string? Validate(SurveyQuestion q, JsonElement value, string uploadToken, IWebHostEnvironment env)
    {
        if (Empty(value)) return q.Required ? "Anh/chị vui lòng trả lời câu bắt buộc." : null;
        var text=Text(value);
        var options=Read<SurveyOption>(q.OptionsJson).Select(o=>o.Text).ToArray();
        if (q.Id == 6 && q.Type == "multi_choice")
            options = options.Concat(Read<SurveyOption>(Defaults().First(x => x.Id == 6).OptionsJson).Select(o => o.Text))
                .Concat(IndustryPainPoints.Values.SelectMany(x => x)).Distinct().ToArray();
        if (q.Type=="question_group") {
            if(value.ValueKind!=JsonValueKind.Object) return "Anh/chị vui lòng nhập thông tin nhóm.";
            foreach(var f in Read<SurveyField>(q.FieldsJson)) {
                value.TryGetProperty(f.Key,out var child);
                var error=Validate(new SurveyQuestion{Type=f.Type,Required=f.Required},child,uploadToken,env);
                if(error==null && q.Id==1 && (f.Key is "contactName" or "companyName" or "role")) {
                    var contactText=Text(child).Trim();
                    if(contactText.Length>0) {
                        if(contactText.Length<2) error="vui lòng nhập ít nhất 2 ký tự.";
                        else if(!Regex.IsMatch(contactText,@"\p{L}")) error="vui lòng nhập ít nhất 1 chữ cái.";
                    }
                }
                if(error!=null)return f.Label+": "+error;
            }
            return null;
        }
        if(q.Type is "multi_choice" or "ranking") {
            if(value.ValueKind!=JsonValueKind.Array) return "Danh sách lựa chọn không hợp lệ.";
            var chosen=value.EnumerateArray().Select(Text).ToArray();
            if(chosen.Distinct().Count()!=chosen.Length || chosen.Any(x=>!options.Contains(x)) || q.Type=="ranking" && chosen.Length!=options.Length) return "Anh/chị vui lòng chọn đúng các đáp án, không trùng.";
            return null;
        }
        if(q.Type=="file_upload") {
            if(value.ValueKind!=JsonValueKind.Object || !value.TryGetProperty("name",out var name) || !value.TryGetProperty("size",out var size) || !size.TryGetInt64(out var bytes)) return "File chưa được tải lên.";
            var filename=Text(name);
            if(!Regex.IsMatch(filename,@"\A[a-zA-Z0-9_.-]+\z") || filename.Contains("..") || !Guid.TryParseExact(uploadToken,"N",out _)) return "Tên file không hợp lệ.";
            var path=Path.Combine(env.WebRootPath,"uploads","surveys",uploadToken,filename);
            if(!File.Exists(path) || new FileInfo(path).Length!=bytes || bytes>10*1024*1024) return "File chưa được tải lên hoặc vượt giới hạn.";
            return null;
        }
        if(value.ValueKind is not (JsonValueKind.String or JsonValueKind.Number)) return "Giá trị câu trả lời không hợp lệ.";
        if(q.Type is "single_choice" or "dropdown" or "yes_no" or "picture_choice") return options.Contains(text) ? null : "Anh/chị vui lòng chọn đáp án có trong danh sách.";
        if(q.Type=="email" && (!System.Net.Mail.MailAddress.TryCreate(text,out var mail) || mail.Address!=text || !text.Contains('.'))) return "Anh/chị vui lòng nhập email đúng định dạng.";
        if(q.Type=="phone" && (!Regex.IsMatch(text,@"\A\+?[0-9 ()-]+\z") || Regex.Replace(text,@"\D","").Length is <9 or >12)) return "Số điện thoại cần 9–12 chữ số.";
        if(q.Type=="date" && !DateOnly.TryParseExact(text,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out _)) return "Ngày không hợp lệ.";
        if(q.Type is "number" or "rating" or "scale") {
            if(!decimal.TryParse(text,NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out var n))return "Anh/chị vui lòng nhập số.";
            var min=q.Type=="rating"?1:q.Type=="scale"?0:q.MinValue;
            var max=q.Type is "rating" or "scale" ? q.ScaleMax ?? (q.Type=="rating"?5:10) : q.MaxValue;
            if(min.HasValue && n<min || max.HasValue && n>max || q.Type!="number" && n!=decimal.Truncate(n))return "Số nằm ngoài giới hạn cho phép.";
        }
        return null;
    }
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/survey-presentation", async (PresalesDbContext db) => Results.Ok(await db.SurveyPresentations.FirstAsync()));
        app.MapPut("/api/survey-presentation", async (SurveyPresentation input, PresalesDbContext db) => {
            var q = new SurveyQuestion { Title="Mở đầu", MediaUrl=input.MediaUrl };
            var error=DefinitionError(q); if(error!=null)return Results.BadRequest(error);
            var settings=await db.SurveyPresentations.FirstAsync();settings.MediaUrl=q.MediaUrl;settings.MediaType=q.MediaType;
            await db.SaveChangesAsync();return Results.Ok(settings);
        });
        app.MapGet("/api/survey-questions", async (bool? includeInactive,PresalesDbContext db)=>Results.Ok((await db.SurveyQuestions.Where(q=>includeInactive==true || q.IsActive).OrderBy(q=>q.SortOrder).ThenBy(q=>q.Id).ToListAsync()).Select(QuestionPayload)));
        app.MapPost("/api/survey-questions", async (SurveyQuestion q,PresalesDbContext db)=>{
            q.Id=0; q.IsActive=true; q.SortOrder=(await db.SurveyQuestions.MaxAsync(x=>(int?)x.SortOrder)??0)+1;
            var error=DefinitionError(q); if(error!=null)return Results.BadRequest(error);
            db.SurveyQuestions.Add(q);await db.SaveChangesAsync();return Results.Created($"/api/survey-questions/{q.Id}",q);
        });
        app.MapPut("/api/survey-questions/{id:int}",async(int id,SurveyQuestion q,PresalesDbContext db)=>{
            var old=await db.SurveyQuestions.FindAsync(id);if(old==null)return Results.NotFound();
            q.Id=id;var error=DefinitionError(q);if(error!=null)return Results.BadRequest(error);
            db.Entry(old).CurrentValues.SetValues(q);await db.SaveChangesAsync();return Results.Ok(old);
        });
        app.MapDelete("/api/survey-questions/{id:int}",async(int id,PresalesDbContext db)=>{
            if(id is 1 or 2 or 10)return Results.BadRequest("Không được xóa thông tin dùng để tạo hồ sơ.");
            var q=await db.SurveyQuestions.FindAsync(id);if(q==null)return Results.NotFound();q.IsActive=false;await db.SaveChangesAsync();return Results.NoContent();
        });
        app.MapPost("/api/survey-questions/reorder",async(int[] ids,PresalesDbContext db)=>{
            var questions=await db.SurveyQuestions.Where(q=>q.IsActive).ToListAsync();
            if(ids.Distinct().Count()!=ids.Length || !ids.Order().SequenceEqual(questions.Select(q=>q.Id).Order()))return Results.BadRequest("Thứ tự phải chứa đủ các câu đang bật, không trùng.");
            for(var i=0;i<ids.Length;i++)questions.First(q=>q.Id==ids[i]).SortOrder=i+1;
            await db.SaveChangesAsync();return Results.Ok();
        });
        app.MapPost("/api/survey-questions/reset",async(PresalesDbContext db)=>{
            var all=await db.SurveyQuestions.ToListAsync();foreach(var q in all)q.IsActive=false;
            foreach(var q in Defaults()){var old=all.First(x=>x.Id==q.Id);db.Entry(old).CurrentValues.SetValues(q);}
            await db.SaveChangesAsync();return Results.Ok();
        });
        app.MapGet("/api/surveys",async(PresalesDbContext db)=>Results.Ok(await db.SurveyResponses.OrderByDescending(s=>s.SubmittedAt).Select(s=>new{s.Id,s.CompanyName,s.Industry,s.SubmittedAt,s.ProfileId}).ToListAsync()));
        app.MapGet("/api/profiles/{id:int}/survey",async(int id,PresalesDbContext db)=>{
            var s=await db.SurveyResponses.Where(s=>s.ProfileId==id).OrderByDescending(s=>s.SubmittedAt).ThenByDescending(s=>s.Id).FirstOrDefaultAsync();return s==null?Results.NotFound():Results.Ok(s);
        });
        // A random upload session ID exists before submission; committed files are moved under the numeric survey ID.
        app.MapPost("/api/surveys/{id}/files",async(string id,HttpRequest request,IWebHostEnvironment env)=>await Upload(id,false,request,env));
        app.MapPost("/api/survey-media",async(HttpRequest request,IWebHostEnvironment env)=>await Upload("",true,request,env));
        app.MapPost("/api/surveys",Submit);
    }
    static async Task<IResult> Upload(string id,bool media,HttpRequest request,IWebHostEnvironment env)
    {
        if(!media && !Guid.TryParseExact(id,"N",out _))return Results.BadRequest("Phiên tải file không hợp lệ.");
        var max=media?50L*1024*1024:10L*1024*1024;
        var feature=request.HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>();
        if(feature is {IsReadOnly:false})feature.MaxRequestBodySize=max+1024*1024;
        if(!request.HasFormContentType || request.ContentLength>max+1024*1024)return Results.BadRequest("File vượt giới hạn.");
        IFormCollection form;
        try { form=await request.ReadFormAsync(); }
        catch (Exception ex) when (ex is InvalidDataException or BadHttpRequestException) { return Results.BadRequest("File hoặc dữ liệu tải lên không hợp lệ / vượt giới hạn."); }
        if(form.Files.Count!=1)return Results.BadRequest("Anh/chị vui lòng tải mỗi lần một file.");
        var file=form.Files.GetFile("file");
        if(file==null || file.Length==0)return Results.BadRequest("Anh/chị vui lòng chọn file.");
        var ext=Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed=media?new[]{".jpg",".png",".webp",".mp4",".webm"}:new[]{".pdf",".doc",".docx",".xls",".xlsx",".png",".jpg",".jpeg"};
        var limit=media && (ext is ".mp4" or ".webm")?50L*1024*1024:10L*1024*1024;
        if(!allowed.Contains(ext) || file.Length>limit)return Results.BadRequest("Định dạng hoặc kích thước file không được hỗ trợ.");
        var clean=Regex.Replace(Path.GetFileNameWithoutExtension(file.FileName.Replace('\\','/')),"[^A-Za-z0-9_-]","_");
        if(clean.Length>100)clean=clean[..100];
        var name=Guid.NewGuid().ToString("N")+"-"+clean+ext;
        var relative=media?"uploads/survey-media":"uploads/surveys/"+id;
        var directory=Path.Combine(env.WebRootPath,relative);Directory.CreateDirectory(directory);
        await using(var output=File.Create(Path.Combine(directory,name)))await file.CopyToAsync(output);
        return Results.Ok(new{name,size=file.Length,url="/"+relative+"/"+name,mediaType=media?MediaType(name):null});
    }
    static async Task<IResult> Submit(SurveySubmitDto dto,PresalesDbContext db,IWebHostEnvironment env)
    {
        var answers=dto.Answers;if(answers==null)return Results.BadRequest("Anh/chị vui lòng điền khảo sát.");
        var questions=await db.SurveyQuestions.Where(q=>q.IsActive).OrderBy(q=>q.SortOrder).ToListAsync();
        if(answers.Keys.Any(k=>!questions.Any(q=>q.Id.ToString()==k)))return Results.BadRequest("Bộ câu hỏi đã thay đổi. Anh/chị vui lòng tải lại trang.");
        foreach(var q in questions){answers.TryGetValue(q.Id.ToString(),out var value);var error=Validate(q,value,dto.UploadToken??"",env);if(error!=null)return Results.BadRequest(new{message=q.Title+": "+error,questionId=q.Id});}
        foreach(var q in questions.Where(q=>q.Type=="file_upload")) {
            if(answers.TryGetValue(q.Id.ToString(),out var file) && !Empty(file))
                answers[q.Id.ToString()]=JsonSerializer.SerializeToElement(new { name=Text(file.GetProperty("name")),size=file.GetProperty("size").GetInt64() },Json);
        }
        string Get(int id)=>answers.TryGetValue(id.ToString(),out var v)?(v.ValueKind==JsonValueKind.Array?string.Join(", ",v.EnumerateArray().Select(Text)):Text(v)):"";
        string Contact(string key)=>answers.TryGetValue("1",out var g) && g.ValueKind==JsonValueKind.Object && g.TryGetProperty(key,out var v)?Text(v):"";
        var company=Contact("companyName");var industry=Get(2);
        if(string.IsNullOrWhiteSpace(company) || company.Length>200 || Contact("contactName")=="" || Contact("contactEmail")=="" || Get(10)=="")return Results.BadRequest("Anh/chị vui lòng nhập đủ liên hệ, tên công ty (tối đa 200 ký tự) và 3 điều muốn thấy.");
        if(industry is not ("Logistic" or "FnB" or "Hotel" or "Sản xuất"))return Results.BadRequest("Anh/chị vui lòng chọn 1 trong 4 ngành Logistic, FnB, Hotel, Sản xuất để chuẩn bị demo; ngành Khác chưa được hỗ trợ.");
        // Serializable transaction prevents two concurrent submissions creating duplicate company profiles.
        await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var profile=await db.Profiles.FirstOrDefaultAsync(p=>p.Name.ToLower()==company.ToLower());var duplicate=profile!=null;
        if(profile==null){
            // Bucket representatives are estimates only. Original buckets remain in SurveyResponse.
            var employees=new Dictionary<string,int>{{"Dưới 50",25},{"50–200",100},{"200–500",300},{"500–1.000",700},{"Trên 1.000",1500}};
            var locations=new Dictionary<string,int>{{"1",1},{"2–5",3},{"6–20",10},{"Trên 20",25}};
            profile=await ProfileFactory.Create(db,new ProfileCreateDto(company,industry,locations.GetValueOrDefault(Get(4)),employees.GetValueOrDefault(Get(3))));
            profile.Status="MoiKhaoSat";
            profile.SystemInfo=Get(5);
            var pain=Get(6);
            foreach(var item in await db.DiscoveryItems.Where(x=>x.ProfileId==profile.Id).ToListAsync()){
                var key=(item.Group+" "+item.Question).ToLowerInvariant();
                if(key.Contains("hệ thống hiện tại") || key.Contains("đang quản lý") || key.Contains("hiện quản lý")){if(!string.IsNullOrWhiteSpace(Get(5)))item.Answer=Get(5);}
                else if(key.Contains("pain point") || key.Contains("đau") || key.Contains("khó khăn") || key.Contains("vướng")){if(!string.IsNullOrWhiteSpace(pain))item.Answer=pain;}
                else if(key.Contains("ca làm") || key.Contains("ca kíp")){if(!string.IsNullOrWhiteSpace(Get(7)))item.Answer=Get(7);}
                else if(key.Contains("khi nào") || key.Contains("tiến độ")){if(!string.IsNullOrWhiteSpace(Get(9)))item.Answer=Get(9);}
                else if(key.Contains("ai") && key.Contains("quyết định")){if(!string.IsNullOrWhiteSpace(Get(8)))item.Answer=Get(8);}
                else if(key.Contains("muốn thấy") || key.Contains("mong muốn"))item.Answer=Get(10);
                else if(item.Group=="Ưu tiên")item.Answer="Đau nhất: "+pain+"\n3 điều muốn thấy: "+Get(10);
            }
        }
        var response=new SurveyResponse{ContactName=Contact("contactName"),ContactEmail=Contact("contactEmail"),Role=Contact("role"),CompanyName=company,Industry=industry,EmployeeBucket=Get(3),LocationsBucket=Get(4),CurrentSystem=Get(5),PainPointsJson=Encode(answers.TryGetValue("6",out var pains) && pains.ValueKind==JsonValueKind.Array?pains.EnumerateArray().Select(Text).ToArray():Get(6)==""?Array.Empty<string>():new[]{Get(6)}),ShiftType=Get(7),DecisionMaker=Get(8),ApplyTimeline=Get(9),DemoMustSee=Get(10),AnswersJson=Encode(answers),QuestionsJson=Encode(questions),ProfileId=profile.Id};
        db.SurveyResponses.Add(response);await db.SaveChangesAsync();
        // Files are served as static documents only, never scripts. JSON holds name + size, no client path.
        if(Guid.TryParseExact(dto.UploadToken,"N",out _)){
            var source=Path.Combine(env.WebRootPath,"uploads","surveys",dto.UploadToken!);
            var target=Path.Combine(env.WebRootPath,"uploads","surveys",response.Id.ToString());
            var files=questions.Where(q=>q.Type=="file_upload").Select(q=>answers.GetValueOrDefault(q.Id.ToString())).Where(v=>!Empty(v)).ToList();
            if(files.Count>0){Directory.CreateDirectory(target);foreach(var name in files.Select(f=>Text(f.GetProperty("name"))).Distinct()){File.Copy(Path.Combine(source,name),Path.Combine(target,name),true);}}
        }
        await tx.CommitAsync();
        if(Guid.TryParseExact(dto.UploadToken,"N",out _)) {
            // Keep temporary originals until the DB commit succeeds so a failed submission can be retried.
            foreach(var q in questions.Where(q=>q.Type=="file_upload")) {
                if(answers.TryGetValue(q.Id.ToString(),out var file) && !Empty(file)) {
                    try { File.Delete(Path.Combine(env.WebRootPath,"uploads","surveys",dto.UploadToken!,Text(file.GetProperty("name")))); }
                    catch(IOException) { /* A leftover temporary file does not invalidate a committed response. */ }
                }
            }
        }
        var result=new{surveyId=response.Id,profileId=profile.Id,profileUrl=$"/?profile={profile.Id}",message=duplicate?"Công ty đã có hồ sơ. Chúng tôi đã ghi nhận câu trả lời của anh/chị.":"Đã gửi khảo sát."};
        return duplicate?Results.Conflict(result):Results.Created($"/api/surveys/{response.Id}",result);
    }
    const string Schema = """
IF OBJECT_ID(N'dbo.SurveyQuestions', N'U') IS NULL
CREATE TABLE [dbo].[SurveyQuestions] (
[Id] int IDENTITY(1,1) PRIMARY KEY,
[SortOrder] int NOT NULL,
[Title] nvarchar(max) NOT NULL,
[Hint] nvarchar(max) NOT NULL,
[Type] nvarchar(max) NOT NULL,
[OptionsJson] nvarchar(max) NOT NULL,
[FieldsJson] nvarchar(max) NOT NULL,
[Placeholder] nvarchar(max) NOT NULL,
[MinValue] int NULL,
[MaxValue] int NULL,
[ScaleMax] int NULL,
[Required] bit NOT NULL,
[IsActive] bit NOT NULL,
[MediaUrl] nvarchar(max) NULL,
[MediaType] nvarchar(max) NULL
);
IF OBJECT_ID(N'dbo.SurveyResponses', N'U') IS NULL
CREATE TABLE [dbo].[SurveyResponses] (
[Id] int IDENTITY(1,1) PRIMARY KEY,
[SubmittedAt] datetime2 NOT NULL,
[ContactName] nvarchar(max) NOT NULL,
[ContactEmail] nvarchar(max) NOT NULL,
[Role] nvarchar(max) NOT NULL,
[CompanyName] nvarchar(max) NOT NULL,
[Industry] nvarchar(max) NOT NULL,
[EmployeeBucket] nvarchar(max) NOT NULL,
[LocationsBucket] nvarchar(max) NOT NULL,
[CurrentSystem] nvarchar(max) NOT NULL,
[PainPointsJson] nvarchar(max) NOT NULL,
[ShiftType] nvarchar(max) NOT NULL,
[DecisionMaker] nvarchar(max) NOT NULL,
[ApplyTimeline] nvarchar(max) NOT NULL,
[DemoMustSee] nvarchar(max) NOT NULL,
[AnswersJson] nvarchar(max) NOT NULL,
[QuestionsJson] nvarchar(max) NOT NULL,
[ProfileId] int NULL
);

IF OBJECT_ID(N'dbo.SurveyPresentations', N'U') IS NULL
CREATE TABLE [dbo].[SurveyPresentations] (
[Id] int IDENTITY(1,1) PRIMARY KEY,
[MediaUrl] nvarchar(max) NULL,
[MediaType] nvarchar(max) NULL
);

""";
}
