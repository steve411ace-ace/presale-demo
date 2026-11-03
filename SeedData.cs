using Microsoft.EntityFrameworkCore;

namespace PresalesDemo;

// Static industry content. This is demo-sample content only, not real customers.
// Pricing / timeline / contact are placeholders everywhere (the thuyet minh publishes no prices).
public record DiscQ(string Group, string Question, string Answer);
public record Persona(string Role, string Name, string Quote);
public record ScenarioDef(string Title, string Pain, string Solution, string Outcome, string Value, string ScreenFlow, string Story, string Before, string After);
public record FitDef(string Requirement, string Status, string Note);
public record Objection(string Q, string A);

public class IndustryDef
{
    public required string Key { get; init; }
    public required string Name { get; init; }
    public required string Summary { get; init; }
    public required string[] Specifics { get; init; }
    public required Persona[] Personas { get; init; }
    public required DiscQ[] Discovery { get; init; }
    public required (string Title, string Desc)[] Priorities { get; init; }
    public required ScenarioDef[] Scenarios { get; init; }
    public required (string Role, string Message)[] Audience { get; init; }
    public required Objection[] Objections { get; init; }
    public required FitDef[] FitGap { get; init; }
    public required (int U, int R, int V, int O, int N) Score { get; init; }
}

public static class SeedData
{
    public static readonly string[] ChecklistGroups = { "Hiểu khách hàng", "Người tham dự và người quyết định", "Khảo sát nhu cầu", "Xây dựng kịch bản", "Hệ thống và tài liệu", "Phối hợp Sales - Presales - BA" };
    public static string[] ChecklistTitles() => new[]
    {
        "Xác nhận lĩnh vực, quy mô, mô hình tổ chức và địa điểm làm việc",
        "Xác nhận hệ thống hiện tại và lý do muốn thay đổi",
        "Ghi nhận 3 vấn đề ưu tiên và mức độ ảnh hưởng (số người, số ngày xử lý)",
        "Liệt kê người tham dự, chức danh và vai trò từng người",
        "Phân biệt người sử dụng, người tư vấn và người phê duyệt ngân sách",
        "Xác nhận tiêu chí mua và quy trình phê duyệt nếu khách chia sẻ",
        "Thực hiện Discovery 15-30 phút đủ 8 nhóm câu hỏi",
        "Ghi quy trình hiện tại và kết quả khách mong muốn",
        "Xác định tình huống đặc thù cần chứng minh",
        "Chọn đúng 3 bài toán trọng tâm, không demo theo menu",
        "Chuẩn bị dữ liệu mẫu sát ngành và sát thực tế khách",
        "Định nghĩa kết quả cần chứng minh ở mỗi tình huống (PSOV)",
        "Kiểm tra tài khoản, phân quyền, kết nối và tốc độ demo",
        "Chuẩn bị dữ liệu, báo cáo, mobile app và phương án dự phòng",
        "Chuẩn bị phương án triển khai, tích hợp và tài liệu năng lực",
        "Thống nhất người dẫn, người demo và người ghi nhận",
        "Thống nhất thông điệp bán hàng và phạm vi cam kết",
        "Dự kiến phản đối, câu hỏi khó và bước tiếp theo",
        "[OneNow bắt buộc] Dùng công ty demo sạch, KHÔNG dùng ACE Company chứa dữ liệu test",
        "[OneNow bắt buộc] Dashboard không còn phiếu kẹt / quá hạn SLA trước giờ demo",
        "[OneNow bắt buộc] Tài khoản demo của CEO, HR, C&B và nhân viên ESS đều đăng nhập được",
    };

    public static List<IndustryDef> Industries => new()
    {
        new IndustryDef
        {
            Key = "Logistic", Name = "Logistic",
            Summary = "[MẪU] Công ty vận tải và kho vận: 3 kho tại TP.HCM, Bình Dương và Đà Nẵng, 850 nhân viên gồm 320 tài xế và phụ xe, 380 nhân viên kho và giao nhận, 150 văn phòng. Ca đêm và ca gãy giờ phổ biến, công phải tổng hợp từ nhiều điểm trong ngày.",
            Specifics = new[] { "3 kho, 2 tuyến vận chuyển liên tỉnh, ca đêm từ 22:00 và ca gãy giờ", "Tài xế thường xuyên thiếu log ra khi giao hàng muộn hoặc đổi tuyến đột xuất", "Phụ cấp đi đường, phụ cấp ca đêm và tăng ca tính theo tuyến và ca, không tính chung một mức", "C&B hiện mất 4 ngày đối chiếu công nhiều điểm với bảng lương" },
            Personas = new[] {
                new Persona("CEO", "Tổng giám đốc vận tải", "“Tôi cần biết chi phí nhân sự theo kho và tuyến, không phải một con số chung.”"),
                new Persona("HR Director", "Giám đốc nhân sự", "“Tôi cần kiểm soát được ai thiếu công, ai làm ca đêm nhiều, trước khi tới kỳ lương.”"),
                new Persona("C&B Manager", "Trưởng C&B", "“Tôi cần đối chiếu công, tăng ca và phụ cấp trước khi chốt, không sửa tay sau khi trả lương.”"),
                new Persona("Quản lý kho", "Quản lý kho Bình Dương", "“Ca đêm thiếu người là tôi phải biết ngay trong ngày, không chờ cuối tháng.”"),
            },
            Discovery = new[] {
                new DiscQ("Bối cảnh", "Hiện quản lý nhân sự, chấm công kho và tài xế bằng phần mềm gì, log công lấy từ đâu?", "Mẫu: Excel theo kho + log chấm công riêng từng kho, chưa tập trung."),
                new DiscQ("Động lực mua", "Vì sao công ty cân nhắc thay đổi hệ thống lúc này?", "Mẫu: mở thêm kho mới, số lượng tài xế tăng nhanh, cách làm cũ không theo kịp."),
                new DiscQ("Pain point", "Vấn đề mất nhiều thời gian hoặc ảnh hưởng vận hành nhất là gì?", "Mẫu: tổng hợp công 3 kho thủ công, thiếu log tài xế giao hàng muộn."),
                new DiscQ("Mức độ ảnh hưởng", "Vấn đề ảnh hưởng bao nhiêu nhân viên, mất bao nhiêu ngày công xử lý?", "Mẫu: 700 nhân viên kho và tài xế, C&B mất 4 ngày mỗi kỳ lương."),
                new DiscQ("Mong muốn", "Sau triển khai, doanh nghiệp kỳ vọng cải thiện điều gì?", "Mẫu: công tập trung trong ngày, sai lệch thấy trước khi chốt lương."),
                new DiscQ("Ưu tiên", "Nếu chỉ giải quyết 3 vấn đề trước mắt thì đó là gì?", "Mẫu: (1) công đa điểm, (2) ca đêm và tăng ca, (3) kiểm tra lương trước chốt."),
                new DiscQ("Tiêu chí quyết định", "Điều kiện bắt buộc để lựa chọn nhà cung cấp là gì?", "Mẫu: phân quyền theo kho, dữ liệu công có dấu vết duyệt, triển khai không làm gián đoạn ca vận hành."),
                new DiscQ("Tiến độ và ngân sách", "Dự kiến triển khai khi nào và đã có ngân sách chưa?", "Mẫu: triển khai sau mùa cao điểm, ngân sách đang trình, chưa chốt số — để placeholder."),
            },
            Priorities = new[] { ("Công từ nhiều điểm tập trung & xử lý thiếu log", "Tổng hợp công 3 kho, phát hiện và giải trình thiếu log tài xế trong ngày."), ("Ca đêm & tăng ca được tổng hợp, đối chiếu trước lương", "Tổng hợp ca đêm, tăng ca và phụ cấp đi đường đúng tuyến và ca."), ("C&B kiểm tra lương, phụ cấp & sai lệch trước khi chốt", "C&B thấy sai lệch công, tăng ca và phụ cấp trước khi phát hành phiếu lương.") },
            Scenarios = new[] {
                new ScenarioDef("Công 3 kho tập trung và giải trình thiếu log",
                    "Công từ 3 kho về chậm, tài xế giao muộn thường thiếu log ra, HR cuối tháng mới phát hiện.",
                    "Mở Dashboard OneNow xem phiếu chờ duyệt và phiếu quá hạn SLA, mở phiếu giải trình công trong Approvals, duyệt, hệ thống ghi Applied và bổ sung lượt chấm công, xem lại kết quả chấm công.",
                    "Thiếu log được xử lý trong ngày, có người duyệt và dấu vết, không sửa tay trong bảng công.",
                    "Giảm tranh cãi cuối tháng về công tài xế và kho.",
                    "Dashboard -> Approvals -> Kết quả chấm công",
                    "Mở đầu: Anh chị đang mất bao nhiêu ngày để gom công 3 kho?|Mở Dashboard, chỉ phiếu chờ duyệt và quá hạn SLA|Mở phiếu giải trình công của tài xế giao muộn|Duyệt và chỉ trạng thái Applied, log đã bổ sung|Hỏi chốt: Với cách này, quy trình giải trình của anh chị có điểm nào bắt buộc khác đi không?",
                    "Trước: mỗi kho gửi file riêng, cuối tháng mới đối chiếu, thiếu log phát hiện muộn.",
                    "Sau: công tập trung, thiếu log hiện ngay, giải trình có duyệt và dấu vết."),
                new ScenarioDef("Ca đêm, tăng ca và phụ cấp theo tuyến",
                    "Ca đêm và tăng ca tính tay theo tuyến, dễ nhầm mức phụ cấp và giờ tăng ca.",
                    "Xem kết quả chấm công theo ca đêm và tăng ca, đối chiếu phụ cấp theo tuyến, chuyển sang công thức lương 23 thành phần để kiểm tra.",
                    "Ca đêm, tăng ca và phụ cấp được đối chiếu trước lương, sai lệch thấy rõ theo từng người.",
                    "C&B bớt thời gian đối chiếu tay và bớt khiếu nại về phụ cấp.",
                    "Kết quả chấm công -> Công thức lương -> Kiểm tra lương",
                    "Mở đầu: Phụ cấp ca đêm hiện tính theo tuyến hay tính chung một mức?|Mở kết quả chấm công ca đêm của một tài xế|Chỉ giờ tăng ca và phụ cấp theo tuyến|Chạy kiểm tra lương cho phạm vi demo nhỏ|Hỏi chốt: Cách đối chiếu này đã khớp với cách tính phụ cấp của anh chị chưa?",
                    "Trước: tính phụ cấp tay theo tuyến, nhầm mức là khiếu nại kéo dài.",
                    "Sau: phụ cấp gắn với ca và tuyến, kiểm tra được trước khi chốt."),
                new ScenarioDef("Kiểm tra lương trước khi chốt và phát hành phiếu lương",
                    "Lương chốt rồi mới phát hiện sai, sửa sau khi trả rất tốn công và mất niềm tin.",
                    "Lấy công và ngày nghỉ đã duyệt đưa vào công thức lương, kiểm tra chi tiết lương và phụ cấp cho phạm vi demo nhỏ, rồi mới phát hành phiếu lương.",
                    "Sai lệch thấy trước khi chốt, phiếu lương phát hành đúng dữ liệu đã duyệt.",
                    "Giảm rủi ro trả lương sai và giảm số lần sửa sau chốt.",
                    "Chấm công -> Nghỉ phép -> Công thức lương -> Phiếu lương",
                    "Mở đầu: Kỳ lương vừa rồi có trường hợp nào phải sửa sau khi trả không?|Mở bảng kiểm tra lương phạm vi demo|Chỉ một trường hợp sai lệch và cách sửa trước chốt|Phát hành phiếu lương mẫu|Hỏi chốt: Anh chị cần thêm bước kiểm tra nào nữa trước khi chốt thật?",
                    "Trước: chốt rồi mới phát hiện sai, sửa sau trả lương.",
                    "Sau: kiểm tra trước chốt, phát hành khi dữ liệu đã sạch."),
            },
            Audience = new[] {
                ("CEO", "Dashboard theo kho và tuyến, chi phí nhân sự tập trung, không cần xem từng bảng công."),
                ("HR Director", "Quy trình công, giải trình và phê duyệt xuyên suốt, kiểm soát ca đêm và công bất thường."),
                ("C&B", "Luồng chấm công -> tăng ca -> phụ cấp -> kiểm tra -> phiếu lương, thấy sai trước chốt."),
                ("Recruitment", "Tuyển tài xế và nhân viên kho nhanh, yêu cầu -> ứng viên -> phỏng vấn -> tiếp nhận."),
                ("IT", "Phân quyền theo kho, SSO, kiến trúc, API, backup, triển khai Windows/IIS."),
                ("Finance / Procurement", "Phạm vi license, triển khai, bảo trì và chi phí phát sinh — giá để placeholder, không bịa số."),
            },
            Objections = new[] {
                new Objection("Tài xế không chấm công tại kho thì sao?", "Dùng phiếu giải trình công có duyệt và dấu vết trên OneNow, không sửa tay bảng công. Trường hợp thiết bị riêng của chuỗi cần đánh giá tích hợp riêng — ghi Fit-Gap, không cam kết ngay."),
                new Objection("Mở thêm kho thì hệ thống có theo kịp không?", "Trình bày phân quyền và báo cáo theo đơn vị, và phương án triển khai theo từng kho, không hứa triển khai ngay trong ngày."),
                new Objection("Dữ liệu công cũ chuyển sang có mất không?", "Trình bày phương án chuyển đổi và chạy song song kiểm tra trước khi chốt kỳ đầu tiên."),
            },
            FitGap = new[] {
                new FitDef("Chấm công và tổng hợp công 3 kho", "Đáp ứng sẵn", "Luồng Dashboard và kết quả chấm công đã kiểm chứng."),
                new FitDef("Giải trình thiếu log có duyệt và dấu vết", "Đáp ứng sẵn", "Phiếu giải trình, trạng thái Applied đã kiểm chứng."),
                new FitDef("Phụ cấp ca đêm và tăng ca theo tuyến", "Đáp ứng qua cấu hình", "Cần cấu hình chính sách phụ cấp theo tuyến trước khi demo với dữ liệu thật."),
                new FitDef("Số dư phép theo duyệt / từ chối / hủy", "Đáp ứng sẵn", "Số dư chạy đúng theo duyệt, từ chối, hủy đã kiểm chứng."),
                new FitDef("Lương theo công thức 23 thành phần và kiểm tra trước chốt", "Đáp ứng qua cấu hình", "Chỉ demo trên phạm vi nhỏ, không chốt lương toàn công ty live."),
                new FitDef("Chấm công bằng thiết bị riêng của chuỗi kho", "Cần phát triển thêm", "Cần đánh giá tích hợp thiết bị, không cam kết trong buổi demo."),
            },
            Score = (18, 20, 14, 10, 8),
        },
        new IndustryDef
        {
            Key = "FnB", Name = "FnB",
            Summary = "[MẪU] Chuỗi FnB: 12 cửa hàng tại TP.HCM và Hà Nội, 620 nhân viên gồm 380 bán thời gian, 180 toàn thời gian và 60 quản lý cửa hàng. Xoay ca, đổi ca và nghỉ đột xuất xảy ra hàng ngày, tuyển dụng cửa hàng mới liên tục.",
            Specifics = new[] { "12 cửa hàng, ca sáng, ca chiều, ca gãy giờ, nhân viên bán thời gian chiếm đa số", "Đổi ca giữa hai cửa hàng khác nhau cần quản lý cửa hàng duyệt, không tự đổi", "Nghỉ đột xuất và nghỉ phép ảnh hưởng trực tiếp tới ca bán hàng trong ngày", "Lương bán thời gian tính theo ca và phụ cấp cửa hàng, không tính như toàn thời gian" },
            Personas = new[] {
                new Persona("CEO", "Tổng giám đốc chuỗi", "“Tôi cần biết cửa hàng nào thiếu người, cửa hàng nào tăng ca nhiều, trong ngày.”"),
                new Persona("HR Director", "Giám đốc nhân sự", "“Bán thời gian ra vào liên tục, tôi cần hồ sơ và hợp đồng gọn, không giấy tờ rải rác.”"),
                new Persona("Quản lý cửa hàng", "Quản lý cửa hàng quận 1", "“Ca chiều thiếu người là tôi phải xoay ca ngay, không chờ HR duyệt cuối tuần.”"),
                new Persona("C&B Manager", "Trưởng C&B", "“Lương bán thời gian tính sai một ca là nhân viên nghỉ việc ngay.”"),
            },
            Discovery = new[] {
                new DiscQ("Bối cảnh", "Hiện quản lý nhân sự và chấm công các cửa hàng bằng gì?", "Mẫu: bảng ca theo cửa hàng, chấm công riêng từng cửa hàng, chưa tập trung."),
                new DiscQ("Động lực mua", "Vì sao chuỗi cân nhắc thay đổi lúc này?", "Mẫu: mở thêm cửa hàng, số bán thời gian tăng, quản lý ca bằng chat và Excel không kiểm soát được."),
                new DiscQ("Pain point", "Vấn đề ảnh hưởng vận hành cửa hàng nhất là gì?", "Mẫu: xoay ca và đổi ca lộn xộn, thiếu người ca bán hàng mà không ai báo sớm."),
                new DiscQ("Mức độ ảnh hưởng", "Ảnh hưởng bao nhiêu nhân viên và cửa hàng?", "Mẫu: 620 nhân viên, 12 cửa hàng, quản lý ca mất nhiều giờ mỗi tuần."),
                new DiscQ("Mong muốn", "Sau triển khai kỳ vọng cải thiện điều gì?", "Mẫu: ca rõ ràng theo cửa hàng, nghỉ và đổi ca có duyệt, lương theo ca đúng."),
                new DiscQ("Ưu tiên", "Nếu chỉ giải quyết 3 vấn đề trước mắt thì đó là gì?", "Mẫu: (1) chấm công và xếp ca theo cửa hàng, (2) nghỉ và đổi ca, (3) lương bán thời gian theo ca."),
                new DiscQ("Tiêu chí quyết định", "Điều kiện bắt buộc khi chọn nhà cung cấp là gì?", "Mẫu: phân quyền theo cửa hàng, nhân viên xem được ca và phiếu lương của mình trên ESS."),
                new DiscQ("Tiến độ và ngân sách", "Dự kiến triển khai khi nào, ngân sách ra sao?", "Mẫu: triển khai theo từng đợt cửa hàng, ngân sách để placeholder."),
            },
            Priorities = new[] { ("Chấm công theo cửa hàng & ca — xử lý đổi ca, thiếu log", "Công và ca rõ theo cửa hàng, thiếu log xử lý có duyệt."), ("Nghỉ phép trên ESS — số dư chạy đúng theo duyệt / từ chối / hủy", "Nghỉ trên ESS, duyệt, số dư giảm đúng và tăng lại khi từ chối/hủy."), ("Lương bán thời gian theo ca & tiếp nhận nhanh cửa hàng mới", "Lương tính theo ca thực tế và phụ cấp cửa hàng, kiểm tra trước chốt.") },
            Scenarios = new[] {
                new ScenarioDef("Chấm công và xếp ca theo từng cửa hàng",
                    "Ca theo cửa hàng rời rạc, thiếu người ca bán hàng không ai thấy sớm.",
                    "Mở Dashboard xem phiếu chờ duyệt theo cửa hàng, xem kết quả chấm công theo ca, xử lý thiếu log bằng phiếu giải trình có duyệt.",
                    "Ca và công rõ theo từng cửa hàng, thiếu log xử lý trong ngày.",
                    "Quản lý cửa hàng xoay ca kịp thời, không chờ cuối tuần.",
                    "Dashboard -> Kết quả chấm công -> Approvals",
                    "Mở đầu: Hôm nay cửa hàng nào thiếu người thì anh chị biết bằng cách nào?|Mở Dashboard theo cửa hàng|Mở kết quả chấm công ca chiều| Xử lý một phiếu thiếu log bằng giải trình có duyệt|Hỏi chốt: Cách xem theo cửa hàng này đã đủ để quản lý ca trong ngày chưa?",
                    "Trước: ca nằm rải rác theo cửa hàng, thiếu người biết muộn.",
                    "Sau: ca và công tập trung theo cửa hàng, thiếu log có duyệt."),
                new ScenarioDef("Nghỉ phép và đổi ca: số dư chạy đúng",
                    "Nhân viên nghỉ đột xuất, đổi ca tự do, số dư phép không khớp gây tranh cãi.",
                    "Nhân viên nộp nghỉ trên ESS, quản lý cửa hàng duyệt trên Approvals, kiểm tra số dư giảm đúng khi duyệt và tăng lại khi từ chối hoặc hủy.",
                    "Nghỉ và đổi ca có duyệt, số dư phép khớp với quyết định duyệt.",
                    "Nhân viên tin vào số dư, quản lý không phải giải thích tay từng trường hợp.",
                    "ESS -> Approvals -> Số dư phép",
                    "Mở đầu: Nhân viên có hay hỏi vì sao số dư phép không khớp không?|Nhân viên nộp nghỉ trên ESS|Quản lý duyệt trên Approvals|Chỉ số dư trước và sau duyệt, rồi trường hợp hủy|Hỏi chốt: Quy trình duyệt nghỉ và đổi ca của anh chị có bước nào bắt buộc khác không?",
                    "Trước: nghỉ và đổi ca tự do, số dư không khớp.",
                    "Sau: nghỉ có duyệt, số dư chạy đúng theo duyệt/từ chối/hủy."),
                new ScenarioDef("Lương bán thời gian theo ca và phụ cấp cửa hàng",
                    "Lương bán thời gian tính sai một ca là nhân viên phản ứng ngay và có thể nghỉ việc.",
                    "Lấy công theo ca và phụ cấp cửa hàng đưa vào công thức lương, kiểm tra chi tiết cho phạm vi demo nhỏ, rồi phát hành phiếu lương.",
                    "Lương theo ca thực tế, phụ cấp đúng cửa hàng, kiểm tra trước chốt.",
                    "Giảm khiếu nại lương của nhân viên bán thời gian.",
                    "Chấm công -> Công thức lương -> Phiếu lương",
                    "Mở đầu: Lương bán thời gian hiện tính theo ca hay tính chung theo tháng?|Mở công theo ca của một nhân viên bán thời gian|Chỉ phụ cấp cửa hàng trong bảng kiểm tra|Phát hành phiếu lương mẫu|Hỏi chốt: Cách tính theo ca này đã khớp với chính sách cửa hàng của anh chị chưa?",
                    "Trước: lương bán thời gian tính chung, sai ca là khiếu nại.",
                    "Sau: lương theo ca thực tế, kiểm tra trước chốt."),
            },
            Audience = new[] {
                ("CEO", "Dashboard theo cửa hàng, cửa hàng nào thiếu người và tăng ca nhiều, trong ngày."),
                ("HR Director", "Hồ sơ, hợp đồng và quy trình phê duyệt cho nhân viên bán thời gian ra vào liên tục."),
                ("C&B", "Luồng công theo ca -> phụ cấp cửa hàng -> kiểm tra -> phiếu lương."),
                ("Recruitment", "Tuyển nhanh cho cửa hàng mới: yêu cầu -> ứng viên -> phỏng vấn -> tiếp nhận."),
                ("IT", "Phân quyền theo cửa hàng, SSO, kiến trúc, API, backup, triển khai theo đợt cửa hàng."),
                ("Finance / Procurement", "Phạm vi license theo cửa hàng, triển khai, bảo trì — giá để placeholder."),
            },
            Objections = new[] {
                new Objection("Đổi ca giữa hai cửa hàng khác nhau có được không?", "Trình bày quy trình đổi ca có duyệt của quản lý, ghi rõ điều kiện cấu hình, không hứa tự đổi không cần duyệt."),
                new Objection("Nhân viên bán thời gian có dùng được ESS không?", "Trình bày ESS cho nhân viên xem ca, nộp nghỉ và xem phiếu lương, kiểm tra tài khoản demo từng vai trò trước giờ demo."),
                new Objection("Mở thêm cửa hàng thì sao?", "Trình bày triển khai theo đợt cửa hàng và phân quyền theo cửa hàng."),
            },
            FitGap = new[] {
                new FitDef("Chấm công và xếp ca theo từng cửa hàng", "Đáp ứng sẵn", "Luồng Dashboard và kết quả chấm công đã kiểm chứng."),
                new FitDef("Số dư phép biến động đúng khi duyệt / từ chối / hủy", "Đáp ứng sẵn", "Số dư chạy đúng đã kiểm chứng."),
                new FitDef("Đổi ca giữa nhân viên hai cửa hàng khác nhau", "Đáp ứng qua cấu hình", "Cần cấu hình quy trình duyệt đổi ca liên cửa hàng."),
                new FitDef("Lương bán thời gian theo ca và phụ cấp cửa hàng", "Đáp ứng qua cấu hình", "Cần cấu hình chính sách theo ca và phụ cấp cửa hàng."),
                new FitDef("Tuyển và tiếp nhận nhanh cho cửa hàng mới", "Đáp ứng sẵn", "Luồng yêu cầu -> ứng viên -> phỏng vấn -> tiếp nhận."),
                new FitDef("Chấm công bằng thiết bị riêng của chuỗi", "Cần phát triển thêm", "Cần đánh giá tích hợp thiết bị, không cam kết ngay."),
            },
            Score = (19, 19, 15, 11, 9),
        },
    };
}
