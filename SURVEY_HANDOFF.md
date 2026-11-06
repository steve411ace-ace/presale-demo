# Bàn giao khảo sát trước demo

Đã triển khai form public 10 bước, Builder nội bộ 16 loại + 11 gợi ý, media, upload, lưu snapshot câu hỏi/câu trả lời, tạo hồ sơ qua factory dùng chung với POST /api/profiles, xử lý trùng công ty, khối khảo sát 12 trường và deep-link Discovery. Không sửa seed bốn hồ sơ mẫu, nav, StoryEdited, chấm điểm hay Fit–Gap follow-up.

File sửa: `Models.cs`, `DbContext.cs`, `Program.cs`, `sql/CreateDatabase.sql`, `wwwroot/index.html`.

File mới: `SurveyApi.cs`, `ProfileFactory.cs`, `sql/SurveySchema.sql`, `web.config`, `wwwroot/khao-sat.html`, `wwwroot/khao-sat.js`, `wwwroot/survey-common.js`, `wwwroot/survey.css`, `wwwroot/survey-admin.js`, `wwwroot/survey-admin.css`, `scripts/check-survey.mjs`, `SURVEY_HANDOFF.md`.

## Kiểm chứng đã chạy trên VM

- `node scripts/check-survey.mjs` → **29 checks passed**, exit 0. Script chạy `node --check` cho ba file JS mới, kiểm tra token upload hoạt động cả HTTP, parse JS inline của index, kiểm tra hành vi escape/summary/media bằng Node VM và các ràng buộc tĩnh 10 bước, 4 ô liên hệ, 11 gợi ý, 16 loại, 12 trường, nav 11 mục, scroll-margin 132px, không teal cũ, không Builder/link hồ sơ trên public, schema đồng nhất, factory không copy action/follow-up.
- `python3` với `xml.etree.ElementTree.parse('web.config')` → XML hợp lệ; `maxAllowedContentLength=54525952` (50 MiB + 2 MiB multipart). Chỉ thêm giới hạn upload, Web SDK bổ sung cấu hình handler khi publish.
- Kiểm tra cân bằng dấu ngoặc C# bằng Python sau khi bỏ chuỗi/comment → pass cả 5 file C#. Đây chỉ là kiểm tra từ vựng, không thay thế compiler.

**`dotnet build` chưa chạy: VM không có dotnet; sẽ chạy trên PC lúc deploy.** Chưa chạy curl/API, SQL Server hay kiểm thử trình duyệt thực tế. Muse cần chạy build và các ca API/UI trong prompt trên PC trước khi deploy, đặc biệt 201/409/400, dữ liệu Discovery/Scorecard, media và upload 50 MiB qua IIS. Không chạy git, setup-presales.ps1, deploy hay commit trên VM.

## Điểm triển khai cần biết

- Chưa gắn ảnh minh họa chính thức vào câu “Đau nhất”: source chỉ có logo OneNow, không có ảnh phù hợp. Màn mở đầu dùng đúng logo hiện có; Builder đã hỗ trợ bổ sung ảnh/video thật cho mọi bước và màn mở đầu. Không tự tạo ảnh thay thế.
- File bắt buộc phải tải được trước khi gửi khảo sát: `{id}` của `/api/surveys/{id}/files` là token phiên GUID ngẫu nhiên trước khi gửi, được truyền qua `UploadToken` trong POST /api/surveys. Sau khi có SurveyResponse, file được chép vào `uploads/surveys/{surveyId}/`, commit DB rồi xóa bản tạm. AnswersJson chỉ giữ `{name,size}`. Đây là phần bổ sung để giải quyết vòng phụ thuộc giữa file bắt buộc và surveyId chỉ được tạo sau submit.
- Thêm `QuestionsJson` trong SurveyResponses để giữ nguyên định nghĩa lúc gửi; thêm bảng `SurveyPresentations` và GET/PUT `/api/survey-presentation` cho media màn mở đầu. Không thay đổi schema bảng cũ. API media riêng: POST `/api/survey-media` multipart field `file`.
- Giữ bắt buộc, chặn xóa/đổi loại câu ngành và “3 điều muốn thấy”, ngoài bảo vệ nhóm liên hệ. Đây là các dữ liệu bắt buộc để tạo hồ sơ.
- Quy đổi quy mô: Dưới 50→25; 50–200→100; 200–500→300; 500–1.000→700; Trên 1.000→1500. Địa điểm: 1→1; 2–5→3; 6–20→10; Trên 20→25. Giá trị gốc giữ nguyên trong SurveyResponse.
- Không thêm cơ chế đăng nhập mới; Builder dùng cùng mô hình truy cập nội bộ với các API sửa hồ sơ hiện có. Public không tải code Builder. Cần giữ phạm vi truy cập nội bộ hiện tại của Presales Demo khi triển khai.
