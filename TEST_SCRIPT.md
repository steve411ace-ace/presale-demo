# Kịch bản test web OneNow Presales Demo — bản server port 99

## Mục tiêu luồng chính (đơn giản, không rối)

1. Chọn ngành trước.
2. Web tự gen ra toàn bộ thông tin của ngành đó, và cho phép điều chỉnh.
3. Nhập thêm thông tin về khách hàng.
4. Ra được kịch bản và chi tiết demo.
5. Trình bày ở cuối cùng, tổng hợp hết thông tin đã nhập phía trên.
6. Sau demo: Fit-Gap, chấm điểm, bước tiếp theo, xuất file.

Nguyên tắc kiểm tra xuyên suốt: **một dữ liệu chỉ có một chỗ nhập chính**.
Nếu cùng một thông tin xuất hiện ở nhiều chỗ (tên khách, số nhân viên, tên bài toán,
ghi chú Fit-Gap, người phụ trách, thời hạn), sửa ở chỗ chính phải đổi ở mọi chỗ hiển
thị, hoặc phải ghi rõ đó là hai thông tin khác nhau. Sửa ở chỗ hiển thị mà không đổi
ở chỗ chính, hoặc tải lại trang là mất, đều tính là lỗi.

## Dữ liệu test — TÁCH RIÊNG HOÀN TOÀN KHỎI TEST ONENOW

- Đây là test riêng cho web Presales Demo, KHÔNG chung với test hệ thống OneNow
  (Round 1, Round 2, Round 3, TEST-3010, issue list OneNow ISS-001-ISS-074).
  Không dùng, không sửa, không ghi đè dữ liệu test OneNow, không mở công ty
  ACE Company, không dùng tài khoản test-3010 hay hợp đồng TEST-3010-HD01.
- Dùng hồ sơ test riêng cho Presales Demo, tiền tố `PRESALES-TEST-`
  (ví dụ `PRESALES-TEST-LOG-01`, `PRESALES-TEST-FNB-01`), để không lẫn với
  tiền tố `TEST-3010` của test OneNow.
- Sau mỗi luồng test, xóa hồ sơ test Presales đã tạo qua API, chỉ giữ 4 hồ sơ
  default: Logistic 3/850, FnB 12/620, Hotel 1/340, Sản xuất 2/1400.
- Kết quả test Presales ghi riêng trong file này và báo cáo test Presales,
  không ghi vào issue list OneNow, không lẫn với các issue ISS-xxx của OneNow.
- Không demo trên dữ liệu test nội bộ ACE, không chốt lương toàn công ty,
  không gửi BHXH.

## A. Khởi động, logo, hồ sơ default

- T01: `GET /api/health` = 200.
- T02: Trang gốc tải được, tiêu đề đúng, logo header là bản sạch (puzzle + OneNow,
  không tagline), favicon là biểu tượng puzzle, không còn logo wordmark có tagline.
- T03: `GET /api/profiles` trả đúng 4 hồ sơ default, không có hồ sơ rác.
- T04: Tạo rồi xóa một hồ sơ test qua API, sau đó danh sách trở lại đúng 4 hồ sơ.

## B. Chọn ngành và tự gen thông tin ngành (có thể điều chỉnh)

Chạy đủ 4 ngành: Logistic, FnB, Hotel, Sản xuất. Mỗi ngành kiểm tra:

- T05: Đổi ngành thì đổi đồng thời: banner ngành, chi tiết ngành, câu hỏi Discovery
  8 nhóm, 3 vấn đề ưu tiên gợi ý, 3 bài toán, ma trận vai trò, checklist riêng theo
  ngành, Fit-Gap gợi ý, thư viện phản đối, gợi ý Scorecard, lời thoại trình bày.
- T06: Không còn nội dung ngành cũ lẫn sang ngành mới (đặc biệt Fit-Gap, ưu tiên,
  bài toán, lời thoại).
- T07: Sửa một thông tin ngành gợi ý (ví dụ sửa tên một bài toán, sửa một ưu tiên),
  rồi bấm "Khôi phục gợi ý gốc" thì trở lại đúng gợi ý ban đầu của ngành đó.
- T08: Sửa thông tin ngành trên hồ sơ riêng không làm thay đổi mẫu ngành gốc.
  Quay lại "Mẫu ngành gốc" vẫn thấy mẫu nguyên vẹn.

## C. Hồ sơ khách hàng — chỗ nhập chính, tránh trùng

- T09: Tạo hồ sơ từ ngành đang chọn, nhập tên khách, số địa điểm, số nhân viên.
- T10: Nhân bản hồ sơ, đổi tên, kiểm tra hồ sơ gốc không bị đổi theo.
- T11: Xóa hồ sơ đang mở, kiểm tra hồ sơ đó biến mất khỏi danh sách.
- T12: Tải lại trang, hồ sơ đã tạo qua API vẫn còn (không mất).
- T13 (kiểm tra trùng): Tên khách, số địa điểm, số nhân viên chỉ nhập một lần
  ở hồ sơ, nhưng nếu Discovery và máy tính ước lượng cũng hỏi lại cùng số đó thì
  phải tự điền từ hồ sơ, hoặc ghi rõ là số khác. Không được để 3 chỗ nhập cùng
  một số mà không đồng bộ với nhau.

## D. Discovery và 3 vấn đề ưu tiên

- T14: Đủ 8 nhóm câu hỏi Discovery theo ngành, mỗi nhóm có ô ghi câu trả lời.
- T15: Ghi câu trả lời Discovery, tải lại trang, câu trả lời vẫn còn (qua API).
- T16: Chốt đủ 3 vấn đề ưu tiên thì hiện thông báo đủ điều kiện sang kịch bản;
  chưa đủ 3 thì hiện cảnh báo, không cho coi như đã chốt.
- T17: Đổi ngành rồi quay lại, ghi chú Discovery đã nhập của khách không bị
  ghi đè bởi gợi ý ngành.
- T18: Máy tính ước lượng chỉ dùng số do khách cung cấp, hiện rõ 2 giả định,
  không tự kết luận kiểu "giảm từ 4 ngày xuống 1 ngày".

## E. Kịch bản và chi tiết demo — một nguồn duy nhất

- T19: 3 bài toán đều đủ Pain - Solution - Outcome - Value, đúng thứ tự ưu tiên,
  mỗi bài 10 phút (2-5-2-1).
- T20: Mỗi bài có bước kể chuyện (câu mở đầu, màn hình OneNow mở, thao tác chính,
  kết quả chỉ vào, câu hỏi chốt) và đồng hồ riêng 10 phút.
- T21: Mỗi bài có so sánh trước/sau, không tự kết luận ROI chưa kiểm chứng.
- T22 (kiểm tra trùng, quan trọng): Sửa tên hoặc nội dung một bài toán ở Builder
  thì cùng lúc phải đổi ở: bước kể chuyện, so sánh trước/sau, lời thoại trình bày,
  bản tổng hợp ở mục trình bày cuối trang, và file xuất Scenario Matrix.
  Nếu chỗ nào vẫn hiện tên/nội dung cũ thì tính là lỗi trùng lắp.
- T23: Sửa một ưu tiên ở Discovery thì thứ tự/tên bài toán liên quan phải
  phản ánh đúng, hoặc phải ghi rõ bài toán và ưu tiên là hai lớp khác nhau.

## F. Checklist và sức khỏe demo

- T24: Checklist 21 mục (18 Playbook + 3 OneNow + chuẩn bị riêng theo ngành),
  tick từng mục thì tiến độ và số đếm tăng đúng, tải lại trang vẫn giữ (qua API).
- T25: Sức khỏe demo 5 điều kiện, chưa đủ thì cảnh báo, đủ 5 thì báo sẵn sàng.
- T26 (kiểm tra trùng): Checklist và sức khỏe demo không hỏi trùng nhau một
  điều kiện mà tick ở chỗ này không đổi ở chỗ kia. Nếu trùng thì phải đồng bộ.

## G. Trình bày ở cuối cùng, tổng hợp hết thông tin phía trên

- T27: Thứ tự mục trên trang: ngành, Discovery, vai trò, kịch bản, checklist,
  sức khỏe, Fit-Gap, phản đối/theo dõi, chấm điểm, bước tiếp, trình bày cuối cùng.
  Điều hướng trên cùng cũng kết thúc ở trình bày.
- T28: Bảng tổng hợp đầu mục trình bày phải khớp với dữ liệu đã nhập: tên khách,
  ngành, 3 ưu tiên, 3 bài toán, Fit-Gap theo trạng thái, điểm Scorecard,
  tiến độ checklist, bước tiếp theo. Sai một trường là lỗi.
- T29: Agenda 5 chặng, bấm được cả 5 chặng, đặc biệt chặng 45-55 phải nhảy đúng
  chặng 45-55 (không nhảy sang 55-60), đồng hồ và tiêu đề khớp chặng đã chọn.
- T30: Đồng hồ 60 phút: bắt đầu, tạm dừng, đặt lại đều đúng. Đồng hồ từng bài
  10 phút chạy độc lập, không làm lệch đồng hồ chính.
- T31: Lời thoại theo từng chặng đổi đúng theo ngành và chặng.
- T32: Chế độ trình chiếu ẩn các mục nội bộ, không ẩn mục trình bày và tổng hợp.

## H. Sau demo: Fit-Gap, phản đối, theo dõi

- T33: Fit-Gap chỉ có đúng 4 trạng thái: Đáp ứng sẵn, Đáp ứng qua cấu hình,
  Cần phát triển thêm, Chưa đáp ứng. Không có trạng thái thứ 5.
- T34: Đổi trạng thái một dòng Fit-Gap thì màu và nhãn đổi ngay, tải lại trang
  vẫn giữ trạng thái đó (qua API).
- T35: Thêm một yêu cầu phát sinh, sửa ghi chú, xóa dòng, khôi phục gợi ý gốc
  đều hoạt động, không làm mất các dòng khác.
- T36: Dòng Fit-Gap ở trạng thái Cần phát triển thêm hoặc Chưa đáp ứng tự
  thành đầu việc theo dõi, đổi trạng thái về Đáp ứng thì đầu việc biến mất.
- T37 (kiểm tra trùng): Ghi chú Fit-Gap và ghi chú đầu việc theo dõi là một
  dữ liệu hay hai dữ liệu phải ghi rõ. Sửa ở Fit-Gap mà đầu việc vẫn hiện
  ghi chú cũ thì tính là lỗi.
- T38: Thư viện phản đối đúng theo ngành, câu trả lời không hứa vượt khả năng.

## I. Sau demo: Scorecard tự chấm (có thể điều chỉnh)

- T39: Bấm "Chấm tự động" thì 5 slider tự điền từ dữ liệu thực đã nhập
  (Discovery, bài toán, Fit-Gap, Action Plan), kèm giải thích cách tính
  từng tiêu chí, tổng đúng 100 tối đa, ngưỡng dưới 60 là "Cần chuẩn bị thêm".
- T40: Sau khi tự chấm, kéo tay một slider thì chỉ slider đó đổi, các slider
  khác giữ nguyên, và ghi rõ đã điều chỉnh tay.
- T41: Bấm lại "Chấm tự động" thì tính lại từ dữ liệu mới nhất.
- T42: Điểm Scorecard tải lại trang vẫn giữ (qua API), và khớp với điểm
  hiện trong bảng tổng hợp ở mục trình bày.

## J. Sau demo: Action Plan và xuất file

- T43: 3 câu hỏi chốt có ô ghi câu trả lời của khách.
- T44: Mỗi bước tiếp theo có người phụ trách và thời hạn, thiếu một trong hai
  thì phải nhìn thấy rõ là chưa đủ.
- T45 (kiểm tra trùng): Người phụ trách/thời hạn ở Action Plan và ở đầu việc
  theo dõi Fit-Gap nếu cùng một việc thì phải đồng bộ, nếu khác việc thì
  phải ghi rõ khác nhau.
- T46: Tải 4 file: tóm tắt buổi demo, phiếu Discovery, Scenario Matrix,
  dàn ý Action Plan. Nội dung file phải khớp với dữ liệu đang hiển thị
  trên trang, không phải wording mẫu cũ của server.
- T47: File tóm tắt phải chứa đủ: khách, ngành, 3 ưu tiên, 3 bài toán,
  Fit-Gap, điểm, bước tiếp.

## K. Hồi quy và các trường hợp biên

- T48: Đổi ngành giữa chừng khi đã nhập dữ liệu khách: dữ liệu khách không
  bị xóa, dữ liệu ngành mới không lẫn ngành cũ.
- T49: Tạo hồ sơ với tên trùng, số địa điểm/số nhân viên không phải số:
  web phải báo rõ, không lưu sai và không vỡ trang.
- T50: Mở trang trên màn hình hẹp (mobile): các bảng không vỡ, Fit-Gap
  vẫn chọn được trạng thái, đồng hồ vẫn đọc được.

## L. Đối chiếu với file Presales Playbook (bắt buộc, không được lệch)

Nguồn chuẩn duy nhất là `workspace/user/files/PRESALES_PLAYBOOK.docx`.
Mọi nội dung, số liệu, khung giờ, trạng thái trong web phải khớp file này.
Nếu web khác Playbook thì tính là lỗi, sửa web theo Playbook, không sửa Playbook.

- L01: Mục tiêu demo là giúp khách tự trả lời 4 câu hỏi: Có hiểu tôi không?
  Có giải quyết được không? Có triển khai được không? Có đáng mua không?
  Không phải khoe đủ tính năng.
- L02: Đúng luồng 5 bước: DISCOVER -> DESIGN -> DEMONSTRATE -> VALIDATE -> ADVANCE.
- L03: Discovery trước demo 15-30 phút, đủ 8 nhóm câu hỏi: bối cảnh, động lực mua,
  pain point, mức độ ảnh hưởng, mong muốn, ưu tiên 3 vấn đề, tiêu chí quyết định,
  tiến độ và ngân sách. Phần lương phải hỏi sâu thêm: quy mô/nhóm chính sách,
  khó khăn về công/phụ cấp/tăng ca/thuế, số ngày chạy và kiểm tra lương,
  nhiều công ty/địa điểm/kỳ lương, truy lĩnh/truy thu/điều chỉnh kỳ trước,
  ưu tiên tự động hóa/chính xác/bảo mật/kiểm soát.
- L04: Checklist chuẩn bị đủ 18 mục, 6 nhóm: hiểu khách hàng; người tham dự/
  người ra quyết định; khảo sát nhu cầu; xây dựng kịch bản; hệ thống/tài liệu;
  phối hợp Sales-Presales-BA.
- L05: Ma trận đối tượng đủ 6 vai trò: CEO, HR Director, C&B/Payroll,
  Recruitment, IT, Finance/Procurement, mỗi vai trò có mối quan tâm và điểm
  chứng minh khác nhau. Thứ tự trình bày theo mức quan tâm của người quyết định,
  không theo thứ tự menu.
- L06: Demo theo bài toán kinh doanh, không theo menu. Mỗi kịch bản đủ
  Pain -> Solution -> Outcome -> Value. Sau mỗi bài toán phải có câu hỏi chốt
  của Playbook về mức độ phù hợp với quy trình hiện tại và điểm bắt buộc
  phải khác đi.
- L07: Agenda đúng 60 phút: 00-05 mở đầu/xác nhận 3 vấn đề; 05-15 quy trình
  thực tế/ưu tiên/tiêu chí; 15-45 ba bài toán trọng tâm, mỗi bài 10 phút;
  45-55 fit/tích hợp/triển khai/giới hạn; 55-60 bước quyết định tiếp theo.
- L08: Fit-Gap đúng và chỉ đúng 4 trạng thái: Đáp ứng sẵn, Đáp ứng qua cấu hình,
  Cần phát triển thêm, Chưa đáp ứng. Không có lựa chọn "làm được hết", không
  cam kết tùy chỉnh trước khi có đánh giá kỹ thuật.
- L09: 5 biểu mẫu chuẩn của Playbook phải đủ: 01 Customer Discovery Form,
  02 Demo Preparation Checklist, 03 Demo Scenario Matrix, 04 Demo Feedback
  & Fit-Gap, 05 Post-demo Action Plan. Kiểm tra 4 file xuất có bao phủ đúng
  5 biểu mẫu này hay không, thiếu biểu mẫu nào thì tính là lỗi.
- L10: Scorecard đúng tổng 100: hiểu khách/pain point 25, demo đúng bài toán 25,
  giá trị kinh doanh 20, phản đối/rủi ro 15, chốt bước tiếp theo 15.
  Ví dụ Playbook: 55/100 = "Cần chuẩn bị thêm", dưới 60 là cần chuẩn bị thêm.
- L11: Kết thúc phải trả lời 3 câu hỏi chốt của Playbook và chốt được bước tiếp
  theo cụ thể có người phụ trách và thời hạn (phạm vi, workshop, PoC, đề xuất
  thương mại, trình Ban Giám đốc), không kết thúc bằng "Anh/chị có câu hỏi gì không?".
- L12: Không phạm các lỗi Playbook cấm: demo hết các module, thao tác màn hình
  mà không giải thích giá trị, dùng dữ liệu demo chung chung không theo ngành
  của khách, đi sâu cấu hình kỹ thuật cho lãnh đạo, hứa mọi thứ, kết thúc
  không có bước tiếp theo.
- L13: Không hứa kết quả chưa kiểm chứng, ví dụ không tự kết luận lương giảm
  từ 4 ngày xuống 1 ngày khi chưa có dữ liệu, chính sách và phạm vi của khách.
- L14: Dữ liệu mẫu của Playbook (doanh nghiệp 3.000 nhân viên, tổng hợp công
  thủ công nhiều chi nhánh, C&B mất 4 ngày xử lý và đối chiếu lương, HR không
  kiểm soát được bất thường công, lãnh đạo thiếu dữ liệu HR kịp thời) nếu
  được dùng làm ví dụ thì phải ghi rõ đó là ví dụ của Playbook, không phải
  khách hàng thật.

## Tiêu chí hoàn thiện

- P0 (bắt buộc): T01, T05, T12, T15, T22, T28, T29, T34, T39, T46 và L01-L14 đều đạt.
- P1: Không còn trường hợp cùng một thông tin có 2 giá trị khác nhau trên
  cùng một trang tại cùng một thời điểm.
- P2: Mọi trường đã lưu qua API đều còn sau khi tải lại trang. Các trường
  chỉ lưu trong phiên trình duyệt phải được liệt kê rõ trong báo cáo cuối,
  không được giấu, và nếu là trường quan trọng (tên bài toán, ưu tiên,
  người phụ trách, thời hạn) thì phải bổ sung API để lưu, không để mất
  khi tải lại.
- Khi sửa xong một lỗi, chạy lại đúng test case đó và các test case liên
  quan (đặc biệt T22, T28, T42) trước khi coi là xong.
