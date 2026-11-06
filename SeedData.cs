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
            Priorities = new[] { ("Công từ nhiều điểm tập trung & xử lý thiếu log", "Công từ 3 kho và các điểm giao nhận tập trung về một nơi, thiếu log và ca bất thường được phát hiện trong ngày; giải trình có người duyệt và dấu vết, không sửa tay trong bảng công."), ("Ca đêm & tăng ca được tổng hợp, đối chiếu trước lương", "Ca đêm, tăng ca và phụ cấp theo tuyến được tổng hợp và đối chiếu trước kỳ lương trên công thức 23 thành phần; sai lệch thấy rõ theo từng người trước khi chốt."), ("C&B kiểm tra lương, phụ cấp & sai lệch trước khi chốt", "C&B kiểm tra chi tiết lương, phụ cấp và sai lệch trên dữ liệu đã duyệt trước khi chốt; phiếu lương chỉ phát hành khi dữ liệu đã sạch, không sửa sau khi trả.") },
            Scenarios = new[] {
                new ScenarioDef("Công 3 kho tập trung và giải trình thiếu log",
                    "Công từ 3 kho và các điểm giao nhận gom về trung tâm bằng file Excel, thường chậm 1–2 ngày. Tài xế giao muộn hay quên log ra, ca bất thường không ai thấy sớm; đến cuối tháng HR mới phát hiện thiếu log, phải gọi từng kho xác minh rồi sửa tay trong bảng công.",
                    "Mở Dashboard OneNow để xem toàn bộ phiếu chờ duyệt và phiếu quá hạn SLA theo từng kho, thay vì chờ file gửi về. Mở phiếu giải trình công của tài xế giao muộn trong Approvals, kiểm tra log ra/vào rồi duyệt — hệ thống ghi trạng thái Applied và bổ sung lượt chấm công có dấu vết, không sửa tay. Xem lại kết quả chấm công để chắc ca bất thường đã xử lý xong trong ngày.",
                    "Thiếu log được phát hiện và xử lý trong ngày, mỗi trường hợp đều có người duyệt và dấu vết rõ ràng; bảng công cuối tháng lấy từ dữ liệu đã duyệt, không còn sửa tay.",
                    "HR không còn mất cuối tháng để gom công 3 kho và đối chiếu tay; tranh cãi về công tài xế và kho giảm vì mọi ca bất thường đều có người duyệt và log đầy đủ.",
                    "Dashboard -> Approvals -> Kết quả chấm công",
                    "Mở đầu: Anh chị đang mất bao nhiêu ngày để gom công 3 kho về trung tâm, và thiếu log thường phát hiện lúc nào?|Mở Dashboard, chỉ phiếu chờ duyệt và phiếu quá hạn SLA theo từng kho|Mở phiếu giải trình công của một tài xế giao muộn, kiểm tra log ra/vào|Duyệt và chỉ trạng thái Applied cùng lượt chấm công đã bổ sung có dấu vết|Hỏi chốt: Với cách này, quy trình giải trình công của anh chị có điểm nào bắt buộc khác đi không?",
                    "Trước: mỗi kho gửi file riêng, công về chậm 1–2 ngày; thiếu log chỉ phát hiện cuối tháng, HR phải gọi từng kho xác minh rồi sửa tay trong bảng công, không có dấu vết ai sửa.",
                    "Sau: công tập trung trên Dashboard, thiếu log hiện ngay trong ngày; giải trình có người duyệt, trạng thái Applied rõ ràng, lượt chấm công bổ sung có dấu vết đầy đủ."),
                new ScenarioDef("Ca đêm, tăng ca và phụ cấp theo tuyến",
                    "Ca đêm và tăng ca của tài xế, nhân viên kho đang tính tay theo từng tuyến, mỗi tuyến một mức phụ cấp khác nhau. Nhầm một mức phụ cấp hay một giờ tăng ca là cả tháng phải đối chiếu lại, nhân viên khiếu nại kéo dài.",
                    "Mở kết quả chấm công theo ca đêm và tăng ca của một tài xế, đối chiếu giờ tăng ca với phụ cấp theo tuyến ngay trên màn hình. Đưa công và phụ cấp vào công thức lương 23 thành phần để kiểm tra trước khi chốt, sai lệch thấy rõ theo từng người chứ không phải mò trong bảng tính.",
                    "Ca đêm, tăng ca và phụ cấp được đối chiếu trước kỳ lương; sai lệch hiện rõ theo từng người, C&B sửa trước khi chốt thay vì sửa sau khi trả.",
                    "C&B bớt hẳn thời gian đối chiếu tay cuối tháng; khiếu nại về phụ cấp ca đêm và tăng ca giảm vì mọi mức phụ cấp đều gắn với ca và tuyến, kiểm tra được.",
                    "Kết quả chấm công -> Công thức lương -> Kiểm tra lương",
                    "Mở đầu: Phụ cấp ca đêm hiện tính theo tuyến hay tính chung một mức cho mọi tuyến?|Mở kết quả chấm công ca đêm của một tài xế, chỉ giờ tăng ca|Chỉ phụ cấp theo tuyến và cách đối chiếu với giờ tăng ca|Chạy kiểm tra lương cho phạm vi demo nhỏ, chỉ một trường hợp sai lệch|Hỏi chốt: Cách đối chiếu này đã khớp với cách tính phụ cấp của anh chị chưa?",
                    "Trước: tính phụ cấp tay theo tuyến trên bảng tính, nhầm một mức là khiếu nại kéo dài cả tháng, C&B phải đối chiếu lại từng người.",
                    "Sau: phụ cấp gắn với ca và tuyến, đối chiếu ngay trên kết quả chấm công; kiểm tra được trước khi chốt lương."),
                new ScenarioDef("Kiểm tra lương trước khi chốt và phát hành phiếu lương",
                    "Lương chốt rồi mới phát hiện sai — sai một phụ cấp hay một ngày nghỉ chưa duyệt là phải sửa sau khi trả, vừa tốn công vừa mất niềm tin của nhân viên, nhất là với đội tài xế và kho đông người.",
                    "Lấy công và ngày nghỉ đã duyệt đưa vào công thức lương 23 thành phần, chạy kiểm tra chi tiết lương và phụ cấp cho phạm vi demo nhỏ trước khi chốt. Chỉ rõ từng trường hợp sai lệch và cách sửa ngay trên màn hình, rồi mới phát hành phiếu lương khi dữ liệu đã sạch.",
                    "Sai lệch thấy trước khi chốt, không còn sửa sau khi trả; phiếu lương phát hành đúng dữ liệu đã duyệt, nhân viên tra cứu thấy khớp với công và nghỉ của mình.",
                    "Giảm rủi ro trả lương sai và số lần sửa sau chốt; C&B chốt lương tự tin hơn vì mọi con số đều đối chiếu được trước khi phát hành.",
                    "Chấm công -> Nghỉ phép -> Công thức lương -> Phiếu lương",
                    "Mở đầu: Kỳ lương vừa rồi có trường hợp nào phải sửa sau khi trả không, và sửa mất bao lâu?|Mở bảng kiểm tra lương cho phạm vi demo nhỏ|Chỉ một trường hợp sai lệch và cách sửa trước khi chốt|Phát hành phiếu lương mẫu, chỉ phiếu lương khớp với công đã duyệt|Hỏi chốt: Anh chị cần thêm bước kiểm tra nào nữa trước khi chốt thật?",
                    "Trước: chốt rồi mới phát hiện sai, sửa sau khi trả lương vừa tốn công vừa mất niềm tin, nhân viên tra phiếu thấy không khớp công.",
                    "Sau: kiểm tra trước chốt trên dữ liệu đã duyệt, phát hành phiếu lương khi mọi sai lệch đã sửa xong."),
            },
            Audience = new[] {
                ("CEO", "Dashboard theo kho và tuyến, chi phí nhân sự tập trung, không cần xem từng bảng công."),
                ("HR Director", "Quy trình công, giải trình và phê duyệt xuyên suốt, kiểm soát ca đêm và công bất thường."),
                ("C&B", "Luồng chấm công -> tăng ca -> phụ cấp -> kiểm tra -> phiếu lương, thấy sai trước chốt."),
                ("Recruitment", "Tuyển tài xế và nhân viên kho nhanh, yêu cầu -> ứng viên -> phỏng vấn -> tiếp nhận."),
                ("IT", "Phân quyền theo kho, SSO, kiến trúc, API, backup, triển khai Windows/IIS."),
                ("Finance / Procurement", "Phạm vi giấy phép, triển khai, bảo trì và chi phí phát sinh — giá để placeholder, không bịa số."),
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
            Priorities = new[] { ("Chấm công theo cửa hàng & ca — xử lý đổi ca, thiếu log", "Chấm công và xếp ca rõ theo từng cửa hàng; đổi ca và thiếu log được xử lý trong ngày có người duyệt, quản lý cửa hàng xoay ca kịp thời trước giờ bán hàng, không chờ cuối tuần."), ("Nghỉ phép trên ESS — số dư chạy đúng theo duyệt / từ chối / hủy", "Nhân viên nộp nghỉ trên ESS, quản lý cửa hàng duyệt trên Approvals; số dư phép giảm đúng khi duyệt và tăng lại đúng khi từ chối hoặc hủy, tra cứu thấy khớp."), ("Lương bán thời gian theo ca & tiếp nhận nhanh cửa hàng mới", "Lương bán thời gian tính theo ca thực tế và phụ cấp đúng cửa hàng, kiểm tra trước chốt; mở cửa hàng mới tiếp nhận nhanh vì ca, lương và phụ cấp đã có mẫu sẵn.") },
            Scenarios = new[] {
                new ScenarioDef("Chấm công và xếp ca theo từng cửa hàng",
                    "Mỗi cửa hàng chấm công và xếp ca một kiểu, ca bán hàng thiếu người không ai thấy sớm; đổi ca, thiếu log dồn về HR cuối tháng mới phát hiện, lúc đó đã không còn đối chiếu được với thực tế ca làm.",
                    "Mở Dashboard xem phiếu chờ duyệt theo từng cửa hàng, xem kết quả chấm công theo ca để thấy ngay cửa hàng nào thiếu người, ca nào thiếu log. Xử lý thiếu log bằng phiếu giải trình có duyệt trên Approvals, rồi xem lại kết quả chấm công để chắc ca đã đủ người trước giờ bán hàng.",
                    "Ca và công rõ theo từng cửa hàng, thiếu log xử lý trong ngày; quản lý cửa hàng xoay ca kịp thời, không chờ đến cuối tuần mới biết thiếu người.",
                    "Cửa hàng không còn cảnh đến giờ bán mới biết thiếu người; HR không phải gom ca tay cuối tháng, đổi ca và thiếu log đều có người duyệt.",
                    "Dashboard -> Kết quả chấm công -> Approvals",
                    "Mở đầu: Ca bán hàng thiếu người thì cửa hàng biết bằng cách nào, và thường biết lúc nào?|Mở Dashboard theo cửa hàng, chỉ cửa hàng đang thiếu người|Mở phiếu giải trình thiếu log của ca bán hàng|Duyệt và chỉ log đã bổ sung, ca đã đủ người|Hỏi chốt: Quy trình xoay ca khi thiếu người của anh chị có bước nào bắt buộc khác không?",
                    "Trước: mỗi cửa hàng chấm công một kiểu, ca thiếu người chỉ biết khi đã đến giờ bán; đổi ca, thiếu log dồn về HR cuối tháng.",
                    "Sau: ca và công rõ theo từng cửa hàng, thiếu log xử lý trong ngày, quản lý xoay ca kịp trước giờ bán hàng."),
                new ScenarioDef("Nghỉ phép và đổi ca: số dư chạy đúng",
                    "Nhân viên nghỉ đột xuất và đổi ca tự do giữa các cửa hàng, quản lý duyệt miệng hoặc nhắn tin; số dư phép tính tay không khớp với quyết định duyệt, nhân viên tranh cãi vì thấy số dư của mình sai.",
                    "Nhân viên nộp nghỉ trên ESS, quản lý cửa hàng duyệt trên Approvals; kiểm tra số dư phép giảm đúng khi duyệt và tăng lại đúng khi từ chối hoặc hủy, ngay trên màn hình. Mọi quyết định duyệt đều lưu dấu vết, không còn duyệt miệng.",
                    "Nghỉ và đổi ca có duyệt theo từng cửa hàng, số dư phép khớp với quyết định duyệt; nhân viên tra cứu thấy đúng số dư của mình.",
                    "Nhân viên tin vào số dư phép, quản lý không phải giải thích tay từng trường hợp; tranh cãi về phép giảm vì mọi thứ đều đối chiếu được.",
                    "ESS -> Approvals -> Số dư phép",
                    "Mở đầu: Số dư phép của nhân viên hiện khớp với quyết định duyệt hay vẫn tính tay riêng?|Nhân viên nộp nghỉ trên ESS|Quản lý cửa hàng duyệt trên Approvals|Chỉ số dư trước và sau duyệt: giảm khi duyệt, tăng lại khi từ chối|Hỏi chốt: Quy tắc duyệt nghỉ và đổi ca của anh chị có gì bắt buộc khác không?",
                    "Trước: nghỉ đột xuất và đổi ca duyệt miệng, số dư tính tay không khớp, nhân viên tranh cãi vì thấy số dư sai.",
                    "Sau: nghỉ và đổi ca có duyệt trên hệ thống, số dư chạy đúng theo duyệt, từ chối và hủy."),
                new ScenarioDef("Lương bán thời gian theo ca và phụ cấp cửa hàng",
                    "Nhân viên bán thời gian làm theo ca ở nhiều cửa hàng, lương tính theo giờ làm và phụ cấp từng cửa hàng; sai một ca là nhân viên phản ứng ngay, nhóm vào ra liên tục nên chỉ cần sai vài lần là mất người.",
                    "Lấy công theo ca và phụ cấp cửa hàng đưa vào công thức lương, kiểm tra chi tiết cho phạm vi demo nhỏ trước khi chốt: giờ làm theo ca, phụ cấp đúng cửa hàng, số giờ khớp với ca đã duyệt. Sai lệch thấy trước khi phát hành phiếu lương.",
                    "Lương theo ca thực tế, phụ cấp đúng cửa hàng, kiểm tra trước chốt; nhân viên bán thời gian tra phiếu thấy khớp với ca mình đã làm.",
                    "Giảm khiếu nại lương của nhân viên bán thời gian; cửa hàng giữ được người vì lương trả đúng ca, đúng giờ, đúng cửa hàng.",
                    "Chấm công -> Công thức lương -> Phiếu lương",
                    "Mở đầu: Lương bán thời gian hiện tính theo ca hay tính chung, và sai một ca thì xử lý thế nào?|Mở kết quả chấm công theo ca của một nhân viên bán thời gian|Chỉ phụ cấp theo cửa hàng và giờ làm theo ca|Chạy kiểm tra lương cho phạm vi demo, chỉ một trường hợp sai lệch|Hỏi chốt: Cách tính lương theo ca của anh chị có điểm nào bắt buộc khác không?",
                    "Trước: lương bán thời gian tính tay theo ca, sai một ca là nhân viên phản ứng ngay, nhóm vào ra liên tục nên dễ mất người.",
                    "Sau: lương theo ca thực tế và phụ cấp đúng cửa hàng, kiểm tra trước chốt, nhân viên tra phiếu thấy khớp ca đã làm."),
            },
            Audience = new[] {
                ("CEO", "Dashboard theo cửa hàng, cửa hàng nào thiếu người và tăng ca nhiều, trong ngày."),
                ("HR Director", "Hồ sơ, hợp đồng và quy trình phê duyệt cho nhân viên bán thời gian ra vào liên tục."),
                ("C&B", "Luồng công theo ca -> phụ cấp cửa hàng -> kiểm tra -> phiếu lương."),
                ("Recruitment", "Tuyển nhanh cho cửa hàng mới: yêu cầu -> ứng viên -> phỏng vấn -> tiếp nhận."),
                ("IT", "Phân quyền theo cửa hàng, SSO, kiến trúc, API, backup, triển khai theo đợt cửa hàng."),
                ("Finance / Procurement", "Phạm vi giấy phép theo cửa hàng, triển khai, bảo trì — giá để placeholder."),
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
