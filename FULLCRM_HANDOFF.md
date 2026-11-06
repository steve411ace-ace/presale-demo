# Handoff — Presales Demo mini-CRM FULL (Muse kiểm chứng) — 06/11/2026

Codex đã chạy xong trên bản copy VM `~/workspace/presales-demo-server/`
(model gpt-6.1-sol, 84.018 token, exit 0, ~11 phút).
File chi tiết của Codex: `CRM_HANDOFF.md` (cùng thư mục).
Backup trước khi sửa: `/tmp/fullcrm-backup/presales-demo-server-before-fullcrm-20261106.tar.gz`
(+ log Codex `/tmp/fullcrm-backup/codex-fullcrm-20261106.log`).
**Không deploy, không chạy `setup-presales.ps1`, không commit, không chạy git.**

## File đổi / thêm (đối chiếu bằng `diff -r` với backup — khớp danh sách Codex)

- Mới: `CrmApi.cs` (216 dòng), `wwwroot/crm.js`, `wwwroot/crm.css`,
  `wwwroot/builder-media.js`, `tools/check-crm.py`,
  `tools/check-builder-media.mjs`, `tools/check-crm-api.mjs`,
  `CRM_HANDOFF.md` (của Codex), `FULLCRM_HANDOFF.md` (file này).
- Sửa: `Models.cs`, `DbContext.cs`, `Program.cs`, `SurveyApi.cs`,
  `sql/CreateDatabase.sql`, `wwwroot/index.html` (**chỉ +3 dòng**:
  link `crm.css`, script `builder-media.js`, script `crm.js`),
  `wwwroot/survey-admin.js` (chỉ phần F1).
- **Không đụng:** `khao-sat.html`, `khao-sat.js`, `survey-common.js`,
  `survey.css`, `sql/SurveySchema.sql`, `SeedData*.cs`, `ProfileFactory.cs`,
  `PresalesDemo.csproj`, `appsettings*.json`, `web.config`,
  `deploy-to-iis.ps1`, `TEST_SCRIPT.md`, `scripts/`. Không có `bin/`/`obj/`
  (không ai chạy `dotnet build` trên VM). Không có dữ liệu TEST-3010/ACE/ISS.
  Không còn teal cũ `#0a7a5e` trong file mới.

## Đối chiếu prompt (Muse đã đọc lại từng file, không chỉ tin Codex)

- [x] 4 view mới ngoài luồng 11 mục: Danh sách / Pipeline / Lịch demo / Báo cáo,
  thanh tab dưới header; mở site không `?profile` → Danh sách;
  `/?profile={id}` → thẳng hồ sơ; nút `← Về danh sách hồ sơ` trong view hồ sơ.
- [x] Danh sách: đủ 8 cột (công ty, ngành, liên hệ, trạng thái, ngày demo,
  chủ sở hữu, khảo sát, cập nhật); tìm kiếm công ty/liên hệ/email; lọc ngành /
  trạng thái / có khảo sát; sort cập nhật mới nhất / tên A–Z / ngày demo;
  toggle `Hiện hồ sơ mẫu` (mặc định ẩn); nút `Hồ sơ mới` dùng flow tạo cũ,
  nút `Sao chép link khảo sát`.
- [x] Pipeline 6 trạng thái `MoiKhaoSat → DangChuanBi → DaDemo → DaBaoGia →
  Chot / Mat`; Kanban 6 cột, đếm hồ sơ/cột (không có trường giá trị nên chỉ đếm);
  đổi trạng thái bằng dropdown trên thẻ (không kéo-thả — nice-to-have).
  Hồ sơ khảo sát mới `MoiKhaoSat`; tạo tay `DangChuanBi` (default model);
  6 hồ sơ cũ nhận `DangChuanBi` khi migrate (cột `NOT NULL DEFAULT`).
- [x] 1 API chung `PUT /api/profiles/{id}/status` (`{status, demoDate?, owner?,
  followUpNote?}`): sai status → 400, demoDate sai → 400; chỉ đổi giá trị
  status mới ghi interaction GhiChu `Đổi trạng thái → …`; field bỏ qua
  (Undefined) không bị xóa — dropdown list/board chỉ gửi `status` nên giữ
  nguyên owner/demoDate. Badge + form trạng thái/ngày demo/chủ sở hữu ngay
  dưới thẻ hồ sơ, đồng bộ 2 chiều sau reload.
- [x] `DemoAppointments` + `Interactions` + `Attachments` đủ CRUD theo prompt
  (13 route trong `CrmApi.cs` + `GET /api/profiles` mới). Validate: note rỗng/
  type sai → 400; endsAt ≤ startsAt → 400; title bắt buộc; upload chỉ
  `.pdf/.doc/.docx/.xls/.xlsx/.png/.jpg/.jpeg`, ≤10 MB, tên làm sạch,
  `.exe/.bat/.ps1/.js/.html` bị chặn bởi allowlist; xóa attachment xóa cả file.
  Tải về qua `/uploads/attachments/{id}/{storedName}` (middleware đã mở
  `Content-Disposition: attachment` cho nhánh này). Đặt lịch từ hồ sơ tạo cả
  appointment + interaction Demo; lịch quá giờ vẫn `SapDienRa` hiện `Quá giờ`.
- [x] Báo cáo `GET /api/reports/summary?month=&industry=`: 5 KPI (Khảo sát,
  Demo đã diễn ra, Báo giá, Chốt, Tỉ lệ chốt), bảng theo ngành (4) + theo tháng
  (6 tháng), loại `isDefault`, winRate = 0 khi mẫu = 0, tính trực tiếp từ DB,
  nút Xuất CSV `bao-cao-presales-yyyy-MM.csv` (BOM UTF-8, escape cell).
- [x] DB: `Profiles` + `Status/DemoDate/Owner/FollowUpNote` qua đúng pattern
  `addCols` + `IF COL_LENGTH`; 3 bảng mới trong `sql/CreateDatabase.sql`
  (**nguồn schema chính**) và mirror trong `CrmApi.Schema`
  (`tools/check-crm.py` kiểm tra 2 khối giống nhau);
  `sql/SurveySchema.sql` giữ nguyên, chỉ thuộc khảo sát.
  `GET /api/profiles/{id}` thêm `status, demoDate, owner, followUpNote,
  hasSurvey` (+ `updatedAt`); `GET /api/profiles` mặc định vẫn sort theo Id
  và trả đủ field cũ + field mới.
- [x] **F4:** `DELETE /api/profiles/{id}` xóa `SurveyResponses` + survey files +
  attachments + interactions/appointments trong 1 SQL transaction
  (xem giới hạn #3). **F1:** Builder kiểm tra `mediaUrl` ngay khi nhập bằng
  đúng rule server (`SafeMedia` + `MediaType`), lỗi inline dưới ô nhập +
  `setCustomValidity` + chặn Lưu — cả editor câu hỏi lẫn media màn mở đầu;
  server giữ nguyên rule + 400. F2/F3 không sửa (ngoài phạm vi).
- [x] Không vỡ tính năng cũ (kiểm tra tĩnh + đọc diff): nav đúng 11 link,
  `scroll-margin-top:132px`, `StoryEdited` còn, khối 12 trường khảo sát giữ
  nguyên vị trí/nội dung, `?profile` còn, tone xanh OneNow, font Be Vietnam Pro.

## Bằng chứng test (Muse chạy lại trên VM, 06/11 ~20:40)

```
python3 tools/check-crm.py          # PASS: cân bằng (),{},[] 8 file C#;
                                    # node --check 5 JS wwwroot + 2 inline script;
                                    # nav 11, margin 132px, StoryEdited, không teal cũ;
                                    # schema SQL == CrmApi.Schema; status khảo sát + cascade có mặt
node tools/check-builder-media.mjs  # PASS 14 case: ảnh không đuôi bị từ chối,
                                    # .jpg/.jpeg (query/uppercase), YouTube/Vimeo, video,
                                    # host giả, HTTP, file local, path traversal, fragment, rỗng
node --check tools/check-crm-api.mjs # PASS cú pháp (CHƯA CHẠY bộ test API)
```

## Điểm lệch / giới hạn — đọc trước khi deploy

1. **VM không có dotnet: chưa hề build, chưa chạy API/SQL/trình duyệt thật.**
   `tools/check-crm-api.mjs` chưa được chạy. Bắt buộc chạy trên PC trước deploy
   (xem mục dưới). Cân bằng ngoặc không thay thế compiler.
2. **Không có branch `feature/full-crm`:** bản VM không phải git repo
   (không có `.git`), Codex không chạy lệnh git nào — đúng yêu cầu.
   Lúc đồng bộ sang PC: tạo branch `feature/full-crm` trên PC trước, rồi mới
   copy file vào (`D:\Source\PresalesDemo`), không copy đè lên `main`.
3. **F4 “cùng 1 transaction” chỉ đúng một nửa:** phần DB nằm trong 1 SQL
   transaction; phần file dùng cơ chế đổi tên tạm (`StagedFiles`, cùng volume,
   khôi phục nếu lỗi trước commit, xóa sau commit). Crash đúng thời điểm hoặc
   filesystem từ chối xóa sau commit có thể để lại thư mục `.deleted-{guid}`
   cần dọn tay (lỗi dọn đã log warning, không im lặng).
4. **Báo cáo — diễn giải của Codex (đã ghi trong `CRM_HANDOFF.md`):**
   interaction đặt lịch (ghi chú có marker `[appointment:id]`) KHÔNG tính vào
   demo đã diễn ra; demo đã diễn ra = interaction Demo nhập tay + appointment
   `DaDienRa` (theo `StartsAt`) — một demo vừa có interaction tay vừa có lịch
   hoàn thành sẽ bị đếm 2 lần; chốt/mất đếm theo hồ sơ duy nhất có interaction
   đổi trạng thái trong kỳ — hồ sơ Chốt→Mất trong cùng tháng tính cả 2 số.
5. **Tháng mặc định của báo cáo phía server là tháng UTC** (frontend gửi tháng
   local rõ ràng nên không ảnh hưởng khi dùng UI).
6. `PUT /api/appointments/{id}` vẫn gán `DemoDate = StartsAt` cả khi trạng
   thái lịch là `DaHuy` (hủy). Nhỏ, chưa sửa.
7. Nav 11 mục bị ẩn ở 4 view CRM (`.crm-home .nav{display:none}`), chỉ hiện
   trong view hồ sơ. Trong DOM vẫn đủ 11 link.
8. Không kéo-thả Kanban, không có dịch vụ gửi nhắc lịch (chỉ lưu
   `ReminderMinutes`) — đều ngoài yêu cầu bắt buộc.
9. `refreshProfile()` gọi 4 API mỗi lần `renderProfiles()` chạy trong view hồ
   sơ (kể cả các lần render lại không liên quan) — chỉ là tốn request, không sai.
10. Rủi ro biên dịch chưa kiểm chứng được trên VM (cần `dotnet build` trên PC):
    chuỗi fluent EF `HasOne(...).WithMany().HasForeignKey("ProfileId")
    .OnDelete(Cascade)`, binding `StatusDto` dùng `JsonElement`,
    `ListProfiles` serialize entity + dictionary.

## Bước kiểm chứng trên PC trước deploy (DB test riêng, không đụng DB thật)

```powershell
dotnet build
# Khởi động local test instance (vd. http://127.0.0.1:5000) với DB thử riêng, rồi:
CRM_TEST_BASE=http://127.0.0.1:5000 CRM_TEST_WRITE=YES node tools/check-crm-api.mjs
```

Script chỉ nhận host loopback, tự tạo/xóa hồ sơ tạm, kiểm tra: filter
`status/industry/hasSurvey/search`, status đúng/sai + ghi interaction tự động +
giữ metadata khi chỉ đổi status, interactions CRUD/validation, appointments
CRUD/validation, upload pdf 1 MB / `.exe` / 11 MB, delta báo cáo (báo giá, chốt),
tạo hồ sơ bằng `POST /api/surveys` → `MoiKhaoSat` → xóa → `GET /api/surveys`
không còn câu trả lời của hồ sơ đó. Sau đó kiểm tra tay: SQL không còn
`SurveyResponses` sau xóa + thư mục upload đã xóa thật; tải file đính kèm;
4 view + đổi trạng thái 2 chiều sau reload; deep link hợp lệ/`?profile=999999`;
tạo hồ sơ cũ; nav 11 mục / Trình bày / theme / AI không đổi; Builder dán link
không đuôi → lỗi inline ngay, link `.jpg` + YouTube → lưu được; khảo sát public
vẫn 10 bước.
