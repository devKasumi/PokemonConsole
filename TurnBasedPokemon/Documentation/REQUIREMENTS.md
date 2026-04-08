# Pokemon: Eclipse of Legends - Software Requirements Specification

## 1. Tổng quan dự án (Project Overview)

**Pokemon: Eclipse of Legends** là một trò chơi nhập vai Pokemon dạng console, được phát triển trên nền tảng C# .NET 9.0, mô phỏng cơ chế chiến đấu theo lượt (turn-based) kết hợp khám phá câu chuyện và đối kháng trực tuyến PvP. Game sử dụng kiến trúc Clean Architecture với 4 tầng: Presentation, Application (Core Business Logic), Domain (Shared Entities) và Infrastructure.

---

## 2. Phạm vi hệ thống (System Scope)

### 2.1 Các chức năng chính

| Nhóm chức năng | Mô tả |
|----------------|--------|
| Xác thực & Tài khoản | Đăng ký, đăng nhập, quản lý phiên người chơi |
| Khám phá câu chuyện | Di chuyển giữa các địa điểm, tương tác NPC, tiến trình story |
| Chiến đấu PvE | Chiến đấu Pokemon hoang dã, Gym Leader, Elite Four, Champion |
| Thu thập & Quản lý | Bắt Pokemon, sử dụng vật phẩm, quản lý đội hình |
| Tiến trình & Phát triển | Lên cấp, tiến hóa, học chiêu thức mới |
| PvP Trực tuyến | Matchmaking, chiến đấu real-time qua mạng |
| Lưu trữ dữ liệu | Lưu/Tải game, theo dõi thành tích PvP |

### 2.2 Tác nhân (Actors)

- **Player**: Người chơi chính tương tác với toàn bộ hệ thống
- **PvP Server**: Máy chủ xử lý matchmaking và chiến đấu trực tuyến

---

## 3. Yêu cầu chức năng (Functional Requirements)

### 3.1 Xác thực & Quản lý tài khoản

**FR-01: Đăng nhập (UC1)**
- Người chơi đăng nhập bằng username và password
- Hệ thống xác thực thông tin với MySQL database sử dụng BCrypt hash
- Đăng nhập thành công: tải dữ liệu người chơi vào GameSession, chuyển đến Main Menu
- Đăng nhập thất bại: hiển thị lỗi, giữ nguyên tại Auth menu

**FR-02: Đăng ký (UC18)**
- Người chơi tạo tài khoản mới với username và password
- Hệ thống kiểm tra tính duy nhất của username
- Password được mã hóa bằng BCrypt trước khi lưu vào MySQL
- Đăng ký thành công: chuyển đến màn hình Login

**FR-03: Chọn Pokemon khởi đầu (UC17)**
- Người chơi mới (chưa có Pokemon) chọn 1 trong 3 starter: Bulbasaur (Grass), Charmander (Fire), Squirtle (Water)
- Hệ thống tạo Pokemon qua WorldGenService và thêm vào đội hình
- Người chơi nhận thêm 5 Poke Ball ban đầu

### 3.2 Khám phá câu chuyện (Story Exploration)

**FR-04: Khám phá địa điểm (UC6)**
- Hệ thống hiển thị địa điểm hiện tại với tên, mô tả, danh sách NPC
- Người chơi có thể: di chuyển (tiến/lùi), nói chuyện NPC, vào Pokemon Center, khám phá vùng hoang dã, thách đấu Gym
- Story data được tải từ story.json, tiến trình tự động lưu khi chuyển chapter

**FR-05: Tương tác NPC (UC7)**
- Hệ thống hiển thị dialogue NPC với hiệu ứng typewriter (từng ký tự)
- NPC có thể: đưa dialogue, tặng vật phẩm, khởi động trận đấu trainer
- Mỗi NPC có Name, Role và danh sách Dialogue

**FR-06: Hồi máu đội hình (UC13)**
- Tại Pokemon Center, hệ thống phục hồi toàn bộ HP cho tất cả Pokemon trong đội
- Xóa các trạng thái bất lợi (fainted, poisoned...)

### 3.3 Hệ thống chiến đấu PvE

**FR-07: Chiến đấu Pokemon hoang dã (UC2)**
- Hệ thống spawn Pokemon hoang dã dựa trên cấp độ khu vực
- Chiến đấu theo lượt, thứ tự dựa trên chỉ số Speed
- Menu hành động: Fight (tấn công), Bag (mở túi đồ), Pokemon (đổi Pokemon), Run (bỏ chạy)
- Tính toán sát thương bao gồm: type advantage, STAB (Same-Type Attack Bonus)
- Health bar hiển thị với màu sắc: xanh (>50%), vàng (20-50%), đỏ (<20%)
- EXP bar hiển thị tiến trình lên cấp

**FR-08: Bắt Pokemon (UC3)**
- Người chơi sử dụng Poke Ball để bắt Pokemon hoang dã (không áp dụng trainer battle)
- Tỷ lệ bắt tính theo: HP ratio của Pokemon, loại ball
- Mô phỏng ball shake (1-3 lần), thành công nếu tất cả pass
- Pokemon bắt được thêm vào đội hình (tối đa 6 con)

**FR-09: Sử dụng vật phẩm (UC4)**
- Túi đồ chia thành 2 pocket: Capture (Poke Ball, Great Ball, Ultra Ball) và Healing (Potion, Super Potion)
- Vật phẩm hồi máu: phục hồi HP cho Pokemon đang active
- Sử dụng vật phẩm trong battle tiêu tốn 1 lượt

**FR-10: Thách đấu Gym (UC14)**
- 8 Gym Leader rải khắp các địa điểm trong story
- Mỗi Gym battle là trainer battle (IsTrainerBattle = true, không bắt Pokemon được)
- Thắng Gym: nhận badge, kiểm tra đủ 8 badge để mở Pokemon League

**FR-11: Thách đấu Pokemon League (UC15)**
- Yêu cầu: đã thu thập đủ 8 Gym badges
- Đánh liên tiếp 4 Elite Four + 1 Champion (gauntlet - không hồi máu giữa các trận)
- Thắng Champion: IsChampion = true, lưu vào MySQL, mở khóa PvP
- Hiển thị Hall of Fame với đội hình chiến thắng

**FR-12: Thách đấu Trainer (UC16)**
- NPC Trainer trong story có thể thách đấu người chơi
- Sử dụng cơ chế battle giống UC2 nhưng không có tùy chọn bắt Pokemon

### 3.4 Tiến trình & Phát triển Pokemon

**FR-13: Lên cấp (UC12)**
- EXP được cấp sau mỗi trận thắng, tính theo level Pokemon bị đánh bại
- Khi EXP đạt ngưỡng: tăng level, tính lại toàn bộ stats (HP, Attack, Defense, Speed)
- Có thể học chiêu thức mới (tối đa 4 moves, chọn thay thế nếu đầy)
- Kiểm tra điều kiện tiến hóa

**FR-14: Tiến hóa Pokemon**
- Khi đạt level tiến hóa, Pokemon tự động biến đổi qua WorldGenService
- Hiển thị animation tiến hóa đặc biệt (flash screen, sound effects)
- Người chơi có thể hủy tiến hóa

**FR-15: Quản lý đội hình (UC5)**
- Hiển thị danh sách Pokemon trong đội (tên, level, HP, type, moves)
- Chức năng: xem chi tiết stats, đổi vị trí, thả Pokemon
- Không thể thả Pokemon duy nhất còn lại

### 3.5 PvP Trực tuyến

**FR-16: Matchmaking (UC8)**
- Yêu cầu: Player phải là Champion (IsChampion = true)
- Kết nối PvP Server qua SignalR (BattleHub)
- Hệ thống xếp hàng đợi và tìm đối thủ phù hợp
- Server tạo BattleRoom khi tìm được cặp đấu

**FR-17: Chiến đấu PvP (UC9)**
- Hai người chơi chọn hành động đồng thời (cùng turn)
- Server xử lý: thu thập action, xác định thứ tự theo Speed, thực thi
- Server broadcast kết quả trận đấu cho cả hai client
- Hiển thị: combat log, health bar, move menu với type và power
- Hỗ trợ: timeout 60s cho mỗi lượt, auto-forfeit khi hết giờ
- Disconnect: đối thủ thắng mặc định

**FR-18: Ghi nhận kết quả PvP**
- Cập nhật PvPWins/PvPLosses sau mỗi trận
- Hiển thị record (W-L) sau trận đấu

### 3.6 Lưu trữ & Dữ liệu

**FR-19: Lưu/Tải game (UC10)**
- Lưu: serialize GameSession data vào MySQL qua IUserRepository
- Tải: đọc data từ MySQL, deserialize vào GameSession
- Auto-save tại: chuyển chapter, sau PvP, sau đạt Champion

**FR-20: Lịch sử trận đấu (UC11)**
- Hiển thị tổng Wins, Losses, Win Rate
- Danh sách các trận đấu gần đây

---

## 4. Yêu cầu phi chức năng (Non-Functional Requirements)

### 4.1 Kiến trúc

| Yêu cầu | Mô tả |
|----------|--------|
| NFR-01 | Sử dụng Clean Architecture 4 tầng (Presentation, Application, Domain, Infrastructure) |
| NFR-02 | Tách biệt UI (Screen classes) và Business Logic (Service classes) |
| NFR-03 | Sử dụng Interface cho tất cả services (IAuthenService, IBattleService, ICatchService, ...) |
| NFR-04 | GameSession singleton quản lý trạng thái toàn cục của phiên chơi |

### 4.2 Bảo mật

| Yêu cầu | Mô tả |
|----------|--------|
| NFR-05 | Password mã hóa bằng BCrypt (không lưu plain text) |
| NFR-06 | Validate input người dùng tại tất cả các điểm nhập (UserInputValidator) |
| NFR-07 | Connection string quản lý qua gamesettings.json (không hardcode) |

### 4.3 Hiệu năng & UX

| Yêu cầu | Mô tả |
|----------|--------|
| NFR-08 | Console UI responsive với color-coded health bars và typewriter text effect |
| NFR-09 | Sound effects cho evolution scene (Console.Beep - Windows only) |
| NFR-10 | PvP timeout 60 giây mỗi lượt để tránh chờ vô hạn |
| NFR-11 | SignalR cho real-time communication giữa client và PvP Server |

### 4.4 Dữ liệu

| Yêu cầu | Mô tả |
|----------|--------|
| NFR-12 | MySQL database cho lưu trữ user accounts và game progress |
| NFR-13 | JSON files cho static data (pokemon.json, story.json, item.json) |
| NFR-14 | Docker Compose cho MySQL deployment |
| NFR-15 | Database seeding qua command line argument (--seed) |

---

## 5. Công nghệ sử dụng (Technology Stack)

| Thành phần | Công nghệ |
|------------|-----------|
| Ngôn ngữ | C# (.NET 9.0) |
| Giao diện | Console Application (System.Console) |
| Cơ sở dữ liệu | MySQL 8.0 (via Docker) |
| Networking | SignalR (ASP.NET Core) |
| Server PvP | ASP.NET Core Web Application |
| Container | Docker Compose |
| Mã hóa | BCrypt.Net |
| Config | Microsoft.Extensions.Configuration |

---

## 6. Luồng người dùng chính (Main User Flows)

### Flow 1: Người chơi mới (New Player)
```
Khởi động App → Đăng ký (UC18) → Đăng nhập (UC1) → Chọn Starter (UC17) 
→ Bắt đầu Story (UC6) → Khám phá, chiến đấu, bắt Pokemon, lên cấp
→ Thu thập 8 Gym Badges (UC14) → Thách đấu Pokemon League (UC15)
→ Trở thành Champion → Mở khóa PvP (UC8 → UC9)
```

### Flow 2: Chiến đấu PvE (Wild Battle)
```
Khám phá vùng hoang (UC6) → Gặp Pokemon hoang dã → Chiến đấu (UC2)
→ Chọn hành động: Attack / Catch (UC3) / Items (UC4) / Switch / Run
→ Thắng → Nhận EXP → Lên cấp (UC12) → Có thể tiến hóa
```

### Flow 3: PvP Online
```
Main Menu → PvP Arena → Matchmaking (UC8) → Tìm đối thủ
→ Chiến đấu PvP (UC9) → Chọn move đồng thời → Server xử lý
→ Kết quả → Cập nhật W/L record
```

---

## 7. Ràng buộc hệ thống (System Constraints)

1. PvP chỉ khả dụng sau khi người chơi trở thành Champion (hoàn thành UC15)
2. Đội hình tối đa 6 Pokemon
3. Mỗi Pokemon tối đa 4 chiêu thức (moves)
4. Không thể bắt Pokemon trong trainer battle (Gym, League, NPC Trainer)
5. Pokemon League là gauntlet - đánh liên tiếp 5 trận không hồi máu
6. Bỏ chạy (Run) chỉ khả dụng trong wild battle, không áp dụng trainer battle
7. Console.Beep chỉ hoạt động trên Windows
8. Yêu cầu MySQL server đang chạy để lưu/tải dữ liệu người dùng
