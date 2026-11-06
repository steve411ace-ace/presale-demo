# Handoff mini-CRM — 06/11/2026

Đã đọc `/home/hatch/workspace/survey-test/part1-results.md`, gồm F1–F4. Chỉ sửa source trong workspace. Không dùng git, không build dotnet, không deploy, không chạy setup-presales.ps1. F2/F3 giữ nguyên theo phạm vi.

## File thay đổi / thêm

- `Models.cs`, `DbContext.cs`: metadata pipeline và ba entity CRM, index/FK.
- `Program.cs`: route list mới, metadata GET detail, migration addCols; xóa hồ sơ bằng transaction, kèm khảo sát, lịch, tương tác, file.
- `CrmApi.cs` (mới): lọc hồ sơ, API trạng thái, tương tác, lịch, attachment và báo cáo; khởi tạo ba bảng.
- `SurveyApi.cs`: chỉ thêm trạng thái MoiKhaoSat cho hồ sơ khảo sát mới; không thay rule media, API khảo sát, seed hoặc câu hỏi.
- `sql/CreateDatabase.sql`: metadata Profiles + ba bảng CRM, idempotent.
- `wwwroot/index.html`: chỉ nạp CSS/JS mới; không thay các mục demo.
- `wwwroot/crm.js`, `wwwroot/crm.css` (mới): bốn view CRM, về danh sách, metadata hồ sơ, lịch/tương tác/file; CSV UTF-8 BOM và escape cell.
- `wwwroot/survey-admin.js`: kiểm tra media inline khi nhập, chặn submit, cả câu hỏi và media mở đầu.
- `wwwroot/builder-media.js` (mới): rule SafeMedia/MediaType tương ứng server, chỉ dùng trong Builder.
- `tools/check-crm.py`, `tools/check-builder-media.mjs`, `tools/check-crm-api.mjs` (mới): kiểm tra tĩnh, test rule media, bộ test API dành cho local instance.
- `CRM_HANDOFF.md` (mới): tài liệu này.

## Hành vi và nguồn schema

Home không có `?profile` là danh sách. Deep link và flow tạo hồ sơ cũ vẫn hoạt động. Nav 11 mục, scroll margin 132px, StoryEdited, auto score, Fit-Gap, AI/research/slides giữ nguyên. `khao-sat.html`, JS public, SurveySchema và seed không sửa. CRM dùng theme/font hiện có; không thêm teal cũ.

`sql/CreateDatabase.sql` là nguồn schema CRM chính. `CrmApi.Schema` là bản nhúng phục vụ migration runtime (không phụ thuộc copy file SQL vào publish); `tools/check-crm.py` kiểm tra hai khối giống nhau. `sql/SurveySchema.sql` vẫn chỉ thuộc schema khảo sát hiện hữu; không chứa bản CRM thứ hai. EnsureCreated cho DB mới; addCols và IF OBJECT_ID cho DB cũ, không drop dữ liệu. Sáu hồ sơ cũ nhận DangChuanBi khi thêm cột. Hồ sơ tạo tay dùng default model; khảo sát chỉ đổi default khi tạo mới, không ghi đè trạng thái hồ sơ trùng.

`GET /api/profiles` không query giữ sort Id và toàn bộ field cũ, chỉ thêm metadata/khảo sát/liên hệ. Sort mới: `updated`, `name`, `demoDate`. HasSurvey dựa SurveyResponses, liên hệ lấy bản mới nhất. Không query vẫn gồm mẫu; UI list ẩn mẫu mặc định, pipeline và báo cáo luôn loại mẫu.

Metadata PUT phân biệt field bị bỏ qua với null: dropdown list/board chỉ gửi status sẽ không xóa owner/demoDate/followUpNote. Chỉ khi đổi giá trị status mới sinh dòng GhiChu. Lưu metadata không đổi status không tạo lịch sử giả. Thêm/sửa lịch đồng bộ DemoDate, không tự chuyển pipeline. ReminderMinutes lưu được, chưa có dịch vụ gửi nhắc lịch (prompt không yêu cầu).

Thời gian mới gửi từ UI là UTC ISO; hiển thị theo múi giờ trình duyệt. Báo cáo phân kỳ theo tháng UTC; month rỗng lấy tháng UTC hiện tại. Chốt/mất tính hồ sơ duy nhất đã có tương tác đổi sang trạng thái đó trong kỳ; cùng hồ sơ có cả hai chuyển trạng thái trong kỳ sẽ xuất hiện ở cả hai số. Khảo sát đếm số câu trả lời; báo giá đếm tương tác BaoGia. Các số đều truy vấn DB khi gọi, không cache.

Đặt lịch tạo interaction Demo với marker `[appointment:id]` để phân biệt việc đặt lịch với demo thực sự. Báo cáo bỏ interaction đặt lịch và đếm lịch DaDienRa theo StartsAt; tương tác Demo nhập tay vẫn được đếm. Không tự suy đoán rằng tương tác Demo nhập tay trùng một lịch đã hoàn thành.

## Bằng chứng đã chạy trên VM

```
python3 tools/check-crm.py
```
PASS: cân bằng (), {}, [] trong 8 file C# sau khi bỏ chuỗi/comment; `node --check` 5 JS trong wwwroot và 2 inline script index; nav đúng 11 link, scroll-margin-top 132px; StoryEdited còn; index không có #0a7a5e; schema CRM SQL/embedded giống nhau; source có status khảo sát và cascade.

```
node tools/check-builder-media.mjs
```
PASS 14 ca rule media: ảnh không đuôi bị từ chối, jpg/jpeg query/uppercase, YouTube/Vimeo, video, host giả, HTTP, file local, path traversal, fragment và rỗng. PASS wiring input feedback/save guard trong source Builder. Đây là test hàm JS, không phải test trình duyệt hoặc test Lưu tới server.

```
node --check tools/check-crm-api.mjs
```
PASS cú pháp bộ test API. **Chưa chạy bộ test API.**

## Chưa kiểm chứng / điểm lệch và giới hạn

- VM không có dotnet: không chạy build, không thể khởi động phiên bản API mới tại đây. Chưa chạy curl/API hoặc kiểm chứng SQL/runtime, chưa đo số báo cáo trên DB thật. Cân bằng ngoặc không thay thế C# compiler.
- Chưa mở trình duyệt thật để kiểm tra render bốn view, đồng bộ list/board/hồ sơ, upload/download hay phát media. Kiểm tra nav/theme/public khảo sát chỉ bằng đọc source. Chưa xác nhận public runtime vẫn đúng 10 bước; không sửa public/seed.
- Không tạo branch vì yêu cầu chạy trên VM cấm mọi lệnh git. Không commit.
- File trên đĩa không thể enlist vào SQL transaction: dùng đổi tên tạm cùng volume trước SQL commit, tự khôi phục nếu lỗi trước commit, rồi xóa sau commit. Trong trường hợp crash đúng thời điểm hoặc filesystem từ chối xóa sau SQL commit, có thể còn đường dẫn `.deleted-{guid}` cần dọn; không tuyên bố có distributed transaction. Đây là giới hạn so với yêu cầu “trong cùng 1 transaction”.
- Báo cáo có diễn giải để tránh đếm việc đặt lịch như demo đã diễn ra, như mô tả ở trên. Không có trường giá trị giao dịch nên Kanban chỉ đếm hồ sơ. Không triển khai kéo thả (nice-to-have).

## Bước kiểm chứng tại PC trước deploy

Chạy build trên PC, khởi động một **local test instance với DB thử riêng**, rồi chạy (Node hỗ trợ fetch/FormData/Blob):

```
CRM_TEST_BASE=http://127.0.0.1:5000 CRM_TEST_WRITE=YES node tools/check-crm-api.mjs
```

Script chỉ nhận host loopback, tạo/xóa hồ sơ tạm, kiểm tra filter, trạng thái hợp lệ/sai và ghi chú tự động, giữ metadata khi chỉ đổi status, interaction CRUD/validation, appointment CRUD/validation, file pdf 1MB/exe/11MB, báo cáo delta báo giá/chốt, tạo khảo sát và xóa cascade. Cuối cùng dọn hồ sơ trong finally. Script **chưa được chạy ở VM**; không thay cho kiểm tra download/file trên đĩa và kiểm tra trình duyệt. Nếu POST khảo sát lỗi, kiểm tra bộ 10 câu ở DB test trước khi chạy lại.

Tiếp tục đối chiếu báo cáo theo seed + hồ sơ khảo sát, bộ lọc from/to/status của lịch, month/industry và mẫu không được tính. Kiểm tra SQL không còn SurveyResponses sau xóa, thư mục upload thực đã được xóa; thử trực tiếp liên kết tải file. Kiểm tra 4 view, đổi trạng thái hai chiều sau reload, deep link hợp lệ/404, tạo hồ sơ cũ, nav 11 mục/Trình bày/theme/AI không đổi, media lỗi inline ngay và jpg/YouTube lưu thật. Muse review source trước mọi deploy.
