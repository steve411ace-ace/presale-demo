namespace PresalesDemo;

public static partial class SeedDataExtra
{
    public static List<IndustryDef> More() => new()
    {
        new IndustryDef
        {
            Key = "Hotel", Name = "Hotel",
            Summary = "[MẪU] Khách sạn 180 phòng tại Đà Nẵng, 340 nhân viên gồm lễ tân, buồng phòng, nhà hàng, kỹ thuật và văn phòng. Vận hành 24/7, lễ tết và mùa cao điểm cần nhiều nhân viên thời vụ, ca đêm và ca gãy giờ phổ biến.",
            Specifics = new[] { "180 phòng, vận hành 24/7, cao điểm lễ tết và mùa hè", "Buồng phòng và lễ tân làm ca xoay, ca đêm, ca gãy giờ", "Nhân viên thời vụ tăng mạnh mùa cao điểm, hợp đồng ngắn hạn nhiều", "Tăng ca lễ và phụ cấp ca đêm là phần khiếu nại nhiều nhất nếu tính sai" },
            Personas = new[] {
                new Persona("CEO", "Tổng giám đốc khách sạn", "“Mùa cao điểm mà thiếu buồng phòng là mất doanh thu ngay trong ngày.”"),
                new Persona("HR Director", "Giám đốc nhân sự", "“Tôi cần biết ai làm ca đêm nhiều, ai sắp hết hợp đồng thời vụ, trước khi tới lễ.”"),
                new Persona("Trưởng bộ phận buồng phòng", "Trưởng buồng phòng", "“Ca sáng thiếu người dọn phòng là khách check-in trễ, tôi phải biết sớm.”"),
                new Persona("C&B Manager", "Trưởng C&B", "“Tăng ca lễ tính sai là cả bộ phận phản ứng.”"),
            },
            Discovery = new[] {
                new DiscQ("Bối cảnh", "Hiện quản lý nhân sự và chấm công khách sạn bằng gì?", "Mẫu: bảng ca theo bộ phận, chấm công riêng, chưa tập trung."),
                new DiscQ("Động lực mua", "Vì sao khách sạn cân nhắc thay đổi lúc này?", "Mẫu: chuẩn bị mùa cao điểm, số thời vụ tăng, cách làm cũ không kiểm soát được ca đêm."),
                new DiscQ("Pain point", "Vấn đề ảnh hưởng vận hành nhất là gì?", "Mẫu: ca 24/7 thiếu người, tăng ca lễ tính tay, hợp đồng thời vụ theo dõi tay."),
                new DiscQ("Mức độ ảnh hưởng", "Ảnh hưởng bao nhiêu nhân viên và bộ phận?", "Mẫu: 340 nhân viên, 5 bộ phận chính, C&B mất nhiều ngày đối chiếu kỳ lễ."),
                new DiscQ("Mong muốn", "Sau triển khai kỳ vọng gì?", "Mẫu: ca rõ theo bộ phận, nghỉ mùa cao điểm có duyệt, lương lễ đúng."),
                new DiscQ("Ưu tiên", "Nếu chỉ giải quyết 3 vấn đề trước mắt thì đó là gì?", "Mẫu: (1) ca 24/7 và thiếu log, (2) nghỉ mùa cao điểm, (3) lương tăng ca lễ và hợp đồng thời vụ."),
                new DiscQ("Tiêu chí quyết định", "Điều kiện bắt buộc khi chọn nhà cung cấp là gì?", "Mẫu: phân quyền theo bộ phận, dữ liệu công có dấu vết, triển khai không gián đoạn vận hành 24/7."),
                new DiscQ("Tiến độ và ngân sách", "Dự kiến triển khai khi nào?", "Mẫu: triển khai trước mùa cao điểm, ngân sách để placeholder."),
            },
            Priorities = new[] { ("Ca 24/7 & thiếu log được phát hiện, giải trình rõ ràng", "Công và ca rõ theo bộ phận, thiếu log xử lý có duyệt."), ("Nghỉ phép mùa cao điểm — duyệt theo bộ phận, số dư chính xác", "Nghỉ trên ESS, duyệt theo bộ phận, số dư đúng."), ("Lương, tăng ca lễ & hợp đồng thời vụ", "Lương lễ và ca đêm kiểm tra trước chốt, hợp đồng thời vụ và tái ký rõ ràng.") },
            Scenarios = new[] {
                new ScenarioDef("Ca 24/7 và thiếu log theo bộ phận",
                    "Ca xoay 24/7, nhân viên buồng phòng và lễ tân thiếu log ca đêm, phát hiện muộn.",
                    "Mở Dashboard xem phiếu chờ duyệt theo bộ phận, xử lý thiếu log bằng phiếu giải trình có duyệt, xem lại kết quả chấm công.",
                    "Thiếu log xử lý trong ngày, ca theo bộ phận rõ ràng.",
                    "Không để thiếu người dọn phòng làm khách check-in trễ.",
                    "Dashboard -> Approvals -> Kết quả chấm công",
                    "Mở đầu: Ca đêm thiếu người thì bộ phận biết bằng cách nào?|Mở Dashboard theo bộ phận|Mở phiếu giải trình thiếu log ca đêm|Duyệt và chỉ log đã bổ sung|Hỏi chốt: Quy trình giải trình ca đêm của anh chị có bước nào bắt buộc khác không?",
                    "Trước: ca 24/7 theo bộ phận rời rạc, thiếu log biết muộn.",
                    "Sau: ca tập trung theo bộ phận, thiếu log có duyệt."),
                new ScenarioDef("Nghỉ phép mùa cao điểm",
                    "Mùa cao điểm ai cũng xin nghỉ, duyệt không theo bộ phận sẽ thiếu người phục vụ.",
                    "Nhân viên nộp nghỉ trên ESS, trưởng bộ phận duyệt trên Approvals, kiểm tra số dư giảm đúng khi duyệt và tăng lại khi từ chối/hủy.",
                    "Nghỉ mùa cao điểm có duyệt theo bộ phận, số dư khớp.",
                    "Giữ đủ người phục vụ mùa cao điểm mà vẫn đảm bảo quyền nghỉ.",
                    "ESS -> Approvals -> Số dư phép",
                    "Mở đầu: Mùa cao điểm anh chị duyệt nghỉ theo nguyên tắc nào?|Nhân viên nộp nghỉ trên ESS|Trưởng bộ phận duyệt|Chỉ số dư trước và sau duyệt|Hỏi chốt: Nguyên tắc duyệt nghỉ mùa cao điểm của anh chị có gì bắt buộc khác không?",
                    "Trước: nghỉ mùa cao điểm duyệt tay, dễ thiếu người.",
                    "Sau: nghỉ có duyệt theo bộ phận, số dư đúng."),
                new ScenarioDef("Lương tăng ca lễ, phụ cấp ca đêm và hợp đồng thời vụ",
                    "Tăng ca lễ và phụ cấp ca đêm tính tay dễ sai, hợp đồng thời vụ theo dõi tay dễ sót tái ký.",
                    "Lấy công ca đêm và tăng ca lễ vào công thức lương, kiểm tra trước chốt, xem hợp đồng thời vụ và quy trình tái ký.",
                    "Lương lễ và ca đêm kiểm tra trước chốt, hợp đồng thời vụ không sót tái ký.",
                    "Giảm khiếu nại lương lễ và giảm rủi ro hợp đồng quá hạn.",
                    "Chấm công -> Công thức lương -> Hợp đồng",
                    "Mở đầu: Kỳ lễ vừa rồi có trường hợp tăng ca nào phải sửa sau trả không?|Mở bảng kiểm tra lương tăng ca lễ|Chỉ phụ cấp ca đêm|Mở hợp đồng thời vụ sắp hết hạn|Hỏi chốt: Anh chị cần thêm bước kiểm tra nào trước khi chốt lương lễ?",
                    "Trước: tăng ca lễ tính tay, hợp đồng thời vụ theo dõi tay.",
                    "Sau: lương kiểm tra trước chốt, hợp đồng có nhắc tái ký."),
            },
            Audience = new[] {
                ("CEO", "Dashboard theo bộ phận, nhân sự mùa cao điểm và chi phí theo bộ phận."),
                ("HR Director", "Ca 24/7, hợp đồng thời vụ, tái ký và phê duyệt xuyên suốt."),
                ("C&B", "Luồng công ca đêm -> tăng ca lễ -> phụ cấp -> kiểm tra -> phiếu lương."),
                ("Recruitment", "Tuyển thời vụ mùa cao điểm: yêu cầu -> ứng viên -> phỏng vấn -> tiếp nhận."),
                ("IT", "Phân quyền theo bộ phận, SSO, kiến trúc, API, backup, triển khai không gián đoạn 24/7."),
                ("Finance / Procurement", "Phạm vi license, triển khai, bảo trì — giá để placeholder."),
            },
            Objections = new[] {
                new Objection("Vận hành 24/7 thì triển khai có phải dừng hệ thống không?", "Trình bày phương án triển khai theo bộ phận và chạy song song kiểm tra, không hứa dừng vận hành."),
                new Objection("Nhân viên thời vụ có dùng ESS được không?", "Trình bày ESS và tài khoản demo từng vai trò, kiểm tra trước giờ demo."),
                new Objection("Tăng ca lễ có tính đúng chính sách khách sạn không?", "Trình bày cấu hình chính sách tăng ca lễ và kiểm tra trước chốt trên phạm vi demo nhỏ."),
            },
            FitGap = new[] {
                new FitDef("Chấm công ca 24/7 theo bộ phận", "Đáp ứng sẵn", "Luồng Dashboard và kết quả chấm công đã kiểm chứng."),
                new FitDef("Giải trình thiếu log ca đêm có duyệt", "Đáp ứng sẵn", "Phiếu giải trình, trạng thái Applied đã kiểm chứng."),
                new FitDef("Nghỉ phép mùa cao điểm và số dư phép", "Đáp ứng sẵn", "Số dư chạy đúng theo duyệt/từ chối/hủy."),
                new FitDef("Lương tăng ca lễ và phụ cấp ca đêm", "Đáp ứng qua cấu hình", "Cần cấu hình chính sách tăng ca lễ và ca đêm."),
                new FitDef("Hợp đồng thời vụ và tái ký", "Đáp ứng qua cấu hình", "Cần cấu hình loại hợp đồng và nhắc tái ký."),
                new FitDef("Tích hợp thiết bị chấm công riêng của khách sạn", "Chưa đáp ứng", "Chưa có tích hợp sẵn, cần đánh giá riêng — không cam kết trong demo."),
            },
            Score = (17, 20, 15, 12, 9),
        },
        new IndustryDef
        {
            Key = "Sản xuất", Name = "Sản xuất",
            Summary = "[MẪU] Doanh nghiệp sản xuất: 2 nhà máy tại Bình Dương và Đồng Nai, 1.400 nhân viên gồm 1.050 công nhân sản xuất, 200 kỹ thuật và bảo trì, 150 văn phòng. Sản xuất 3 ca, tăng ca theo đơn hàng, nhiều loại hợp đồng từ thử việc tới không xác định thời hạn.",
            Specifics = new[] { "2 nhà máy, 3 ca sản xuất, tăng ca theo đơn hàng", "Công nhân làm ca, tăng ca và phụ cấp ca, phụ cấp độc hại theo vị trí (cấu hình theo chính sách)", "Hợp đồng thử việc, xác định thời hạn và không xác định thời hạn, tái ký nhiều", "C&B đối chiếu công nhà máy với lương mất nhiều ngày nếu làm tay" },
            Personas = new[] {
                new Persona("CEO", "Tổng giám đốc sản xuất", "“Tôi cần biết chi phí nhân công theo nhà máy và ca, không phải số tổng chung.”"),
                new Persona("HR Director", "Giám đốc nhân sự", "“Tôi cần kiểm soát hợp đồng sắp hết hạn và công bất thường trước kỳ lương.”"),
                new Persona("Quản đốc nhà máy", "Quản đốc nhà máy Bình Dương", "“Tăng ca theo đơn hàng mà không biết ai đã làm bao nhiêu giờ là rất rủi ro.”"),
                new Persona("C&B Manager", "Trưởng C&B", "“Lương công nhân tính sai một ca là cả chuyền phản ứng.”"),
            },
            Discovery = new[] {
                new DiscQ("Bối cảnh", "Hiện quản lý nhân sự và chấm công 2 nhà máy bằng gì?", "Mẫu: bảng công theo chuyền, chấm công riêng từng nhà máy, chưa tập trung."),
                new DiscQ("Động lực mua", "Vì sao doanh nghiệp cân nhắc thay đổi lúc này?", "Mẫu: tăng ca theo đơn hàng nhiều, số công nhân tăng, đối chiếu tay không theo kịp."),
                new DiscQ("Pain point", "Vấn đề ảnh hưởng sản xuất nhất là gì?", "Mẫu: công nhà máy tổng hợp chậm, tăng ca và phụ cấp tính tay, hợp đồng sắp hết hạn theo dõi tay."),
                new DiscQ("Mức độ ảnh hưởng", "Ảnh hưởng bao nhiêu nhân viên?", "Mẫu: 1.400 nhân viên, 2 nhà máy, C&B mất 4 ngày mỗi kỳ."),
                new DiscQ("Mong muốn", "Sau triển khai kỳ vọng gì?", "Mẫu: công tập trung theo nhà máy, tăng ca rõ, lương kiểm tra trước chốt."),
                new DiscQ("Ưu tiên", "Nếu chỉ giải quyết 3 vấn đề trước mắt thì đó là gì?", "Mẫu: (1) công nhà máy và giải trình, (2) hợp đồng và nghỉ theo ca, (3) lương tăng ca và phụ cấp."),
                new DiscQ("Tiêu chí quyết định", "Điều kiện bắt buộc khi chọn nhà cung cấp là gì?", "Mẫu: phân quyền theo nhà máy, dữ liệu công có dấu vết, triển khai không gián đoạn sản xuất."),
                new DiscQ("Tiến độ và ngân sách", "Dự kiến triển khai khi nào?", "Mẫu: triển khai theo từng nhà máy, ngân sách để placeholder."),
            },
            Priorities = new[] { ("Công nhà máy tập trung — phát hiện công bất thường & giải trình", "Công theo nhà máy, phát hiện và giải trình công bất thường có duyệt."), ("Hợp đồng, tái ký & nghỉ phép theo ca sản xuất", "Hợp đồng rõ loại và hạn, nghỉ theo ca có duyệt, số dư đúng."), ("Lương theo công, tăng ca & phụ cấp — công thức 23 thành phần", "Lương theo công, tăng ca và phụ cấp, công thức 23 thành phần, kiểm tra trước chốt.") },
            Scenarios = new[] {
                new ScenarioDef("Công nhà máy tập trung và giải trình",
                    "Công 2 nhà máy về chậm, công bất thường theo chuyền phát hiện muộn.",
                    "Mở Dashboard xem phiếu chờ duyệt theo nhà máy, xử lý công bất thường bằng phiếu giải trình có duyệt, xem lại kết quả chấm công.",
                    "Công bất thường xử lý trong ngày, có duyệt và dấu vết.",
                    "Không để sai công kéo tới kỳ lương mới phát hiện.",
                    "Dashboard -> Approvals -> Kết quả chấm công",
                    "Mở đầu: Công bất thường theo chuyền hiện phát hiện bằng cách nào?|Mở Dashboard theo nhà máy|Mở phiếu giải trình công bất thường|Duyệt và chỉ log đã bổ sung|Hỏi chốt: Quy trình xử lý công bất thường của anh chị có bước nào bắt buộc khác không?",
                    "Trước: công theo nhà máy rời rạc, bất thường biết muộn.",
                    "Sau: công tập trung, bất thường có duyệt."),
                new ScenarioDef("Hợp đồng, tái ký và nghỉ phép theo ca",
                    "Hợp đồng nhiều loại, sắp hết hạn theo dõi tay, nghỉ theo ca duyệt không đều.",
                    "Xem hợp đồng và trường hợp sắp hết hạn, nhân viên nộp nghỉ trên ESS, quản lý duyệt, kiểm tra số dư đúng.",
                    "Hợp đồng không sót tái ký, nghỉ theo ca có duyệt và số dư khớp.",
                    "Giảm rủi ro hợp đồng quá hạn và tranh cãi số dư phép.",
                    "Hợp đồng -> ESS -> Approvals -> Số dư phép",
                    "Mở đầu: Hợp đồng sắp hết hạn hiện theo dõi bằng gì?|Mở danh sách hợp đồng sắp hết hạn|Nhân viên nộp nghỉ trên ESS và quản lý duyệt|Chỉ số dư trước và sau duyệt|Hỏi chốt: Quy trình tái ký và duyệt nghỉ theo ca của anh chị có gì bắt buộc khác không?",
                    "Trước: hợp đồng theo dõi tay, nghỉ theo ca duyệt không đều.",
                    "Sau: hợp đồng có nhắc tái ký, nghỉ có duyệt và số dư đúng."),
                new ScenarioDef("Lương tăng ca, phụ cấp và kiểm tra trước chốt",
                    "Tăng ca theo đơn hàng và phụ cấp theo ca tính tay, sai một ca là cả chuyền phản ứng.",
                    "Lấy công, tăng ca và phụ cấp vào công thức lương 23 thành phần, kiểm tra chi tiết cho phạm vi demo nhỏ, rồi phát hành phiếu lương.",
                    "Lương theo công thực tế, tăng ca và phụ cấp đúng, kiểm tra trước chốt.",
                    "Giảm khiếu nại lương công nhân và giảm sửa sau chốt.",
                    "Chấm công -> Công thức lương -> Phiếu lương",
                    "Mở đầu: Tăng ca theo đơn hàng hiện tính và đối chiếu như thế nào?|Mở kết quả chấm công tăng ca|Mở bảng kiểm tra lương phạm vi demo|Phát hành phiếu lương mẫu|Hỏi chốt: Anh chị cần thêm bước kiểm tra nào trước khi chốt lương thật?",
                    "Trước: tăng ca và phụ cấp tính tay, sai ca là khiếu nại.",
                    "Sau: lương theo công thực tế, kiểm tra trước chốt."),
            },
            Audience = new[] {
                ("CEO", "Dashboard theo nhà máy và ca, chi phí nhân công tập trung."),
                ("HR Director", "Hợp đồng, tái ký, quy trình phê duyệt và báo cáo đa nhà máy."),
                ("C&B", "Luồng chấm công -> tăng ca -> phụ cấp -> kiểm tra -> phiếu lương."),
                ("Recruitment", "Tuyển công nhân theo chuyền: yêu cầu -> ứng viên -> phỏng vấn -> tiếp nhận."),
                ("IT", "Phân quyền theo nhà máy, SSO, kiến trúc, API, backup, triển khai theo nhà máy."),
                ("Finance / Procurement", "Phạm vi license, triển khai, bảo trì — giá để placeholder."),
            },
            Objections = new[] {
                new Objection("Tăng ca theo đơn hàng đột xuất thì sao?", "Trình bày đối chiếu tăng ca trước lương và kiểm tra trên phạm vi demo nhỏ, không hứa tính tự động cho mọi trường hợp đặc thù chưa cấu hình."),
                new Objection("Triển khai có làm gián đoạn sản xuất không?", "Trình bày triển khai theo từng nhà máy và chạy song song kiểm tra."),
                new Objection("Dữ liệu công cũ chuyển sang có mất không?", "Trình bày phương án chuyển đổi và kiểm tra trước kỳ lương đầu tiên."),
            },
            FitGap = new[] {
                new FitDef("Chấm công và tổng hợp công 2 nhà máy", "Đáp ứng sẵn", "Luồng Dashboard và kết quả chấm công đã kiểm chứng."),
                new FitDef("Giải trình công bất thường có duyệt", "Đáp ứng sẵn", "Phiếu giải trình, trạng thái Applied đã kiểm chứng."),
                new FitDef("Hợp đồng và tái ký theo loại hợp đồng", "Đáp ứng qua cấu hình", "Cần cấu hình loại hợp đồng và nhắc tái ký."),
                new FitDef("Nghỉ phép theo ca và số dư phép", "Đáp ứng sẵn", "Số dư chạy đúng theo duyệt/từ chối/hủy."),
                new FitDef("Lương tăng ca, phụ cấp và công thức 23 thành phần", "Đáp ứng qua cấu hình", "Chỉ demo phạm vi nhỏ, không chốt toàn công ty live."),
                new FitDef("Tích hợp máy chấm công riêng của nhà máy", "Cần phát triển thêm", "Cần đánh giá tích hợp, không cam kết ngay."),
            },
            Score = (18, 21, 14, 11, 10),
        },
    };

    public static void Seed(PresalesDbContext db)
    {
        if (db.Profiles.Any()) return;
        var all = SeedData.Industries.Concat(More()).ToList();
        int order = 0;
        foreach (var ind in all)
        {
            var p = new CustomerProfile { Name = "[MẪU] " + ind.Name + " - Hồ sơ demo mặc định", Industry = ind.Key, Locations = ind.Key == "FnB" ? 12 : ind.Key == "Logistic" ? 3 : ind.Key == "Sản xuất" ? 2 : 1, EmployeeCount = ind.Key == "Logistic" ? 850 : ind.Key == "FnB" ? 620 : ind.Key == "Hotel" ? 340 : 1400, IsDefault = true, IsDemoSample = true };
            db.Profiles.Add(p); db.SaveChanges();
            int i = 0;
            foreach (var d in ind.Discovery) db.DiscoveryItems.Add(new DiscoveryItem { ProfileId = p.Id, Group = d.Group, Question = d.Question, Answer = d.Answer, SortOrder = i++ });
            i = 0;
            foreach (var pr in ind.Priorities) db.Priorities.Add(new PriorityItem { ProfileId = p.Id, Rank = ++i, Title = pr.Title, Description = pr.Desc });
            i = 0;
            foreach (var s in ind.Scenarios) db.Scenarios.Add(new Scenario { ProfileId = p.Id, SortOrder = i++, Title = s.Title, Pain = s.Pain, Solution = s.Solution, Outcome = s.Outcome, Value = s.Value, ScreenFlow = s.ScreenFlow, StorySteps = s.Story, BeforeText = s.Before, AfterText = s.After });
            i = 0;
            foreach (var f in ind.FitGap) db.FitGapRows.Add(new FitGapRow { ProfileId = p.Id, Requirement = f.Requirement, Status = f.Status, Note = f.Note, SortOrder = i++ });
            db.Scorecards.Add(new Scorecard { ProfileId = p.Id, UnderstandCustomer = ind.Score.U, RightProblem = ind.Score.R, BusinessValue = ind.Score.V, HandleObjections = ind.Score.O, NextStep = ind.Score.N });
            db.Actions.Add(new ActionItem { ProfileId = p.Id, Title = "Tổ chức workshop chuyên sâu theo 3 bài toán đã demo", Owner = "Presales", Deadline = "2026-11-10", Source = "Post-demo" });
            db.Actions.Add(new ActionItem { ProfileId = p.Id, Title = "Chạy PoC với dữ liệu mẫu của khách (không dùng dữ liệu thật khi chưa có thỏa thuận)", Owner = "BA", Deadline = "2026-11-17", Source = "Post-demo" });
            db.Actions.Add(new ActionItem { ProfileId = p.Id, Title = "Gửi đề xuất thương mại (giá và timeline để placeholder cho tới khi có báo giá chính thức)", Owner = "Sales", Deadline = "2026-11-24", Source = "Post-demo" });
            foreach (var f in ind.FitGap.Where(x => x.Status == "Cần phát triển thêm" || x.Status == "Chưa đáp ứng"))
                db.FollowUps.Add(new FollowUp { ProfileId = p.Id, Title = "Làm rõ: " + f.Requirement, Owner = "Presales", Deadline = "2026-11-17", SourceFitGap = f.Requirement });
            i = 0;
            var titles = SeedData.ChecklistTitles();
            foreach (var t in titles)
            {
                var g = SeedData.ChecklistGroups[Math.Min(i / 3, 5)];
                if (i >= 18) g = "OneNow bắt buộc";
                db.ChecklistItems.Add(new ChecklistItem { ProfileId = p.Id, Group = g, Title = t, IsDone = false, IsOneNowMandatory = i >= 18, SortOrder = i });
                i++;
            }
            db.SaveChanges();
            order++;
        }
    }
}
