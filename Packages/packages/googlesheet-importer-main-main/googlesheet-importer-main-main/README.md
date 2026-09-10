# Big Bear Googlesheet Importer

Package dùng để lấy data từ googlesheet theo phương thức Oauth 2.0<br>
Package có thể quản lý dữ liệu từ nhiều sheet khác nhau.

## Hướng dẫn sử dụng

### cài đặt package
1. Trong Unity mở <b>Package Manager</b>
2. Chọn <b>Add Package from git URL...</b>
3. Điền url = https://gitlab.com/big-bear-team/packages/package-googlesheet-importer.git

### Điều kiện
#### Có service account có thể truy cập google sheet
- account Id : **data-collector@valued-aleph-337304.iam.gserviceaccount.com**
- credential json file<br>
      Cách tạo tham khảo ở link: https://code-maze.com/google-sheets-api-with-net-core/
#### Các package cần thiết   
1. <b>Sirenix Odin package</b>: sử dụng odin để dựng các window
2. <b>Big Bear Core</b> [(link)](https://gitlab.com/big-bear-team/packages/package-core.git)

### Các tính năng
#### I. Thêm google sheet cần import data
1. Mở google sheet muốn import data,  share quyền truy cập cho account **data-collector@valued-aleph-337304.iam.gserviceaccount.com** với quyền **viewer** <br>
   ![share access](Documentation~/images/share access.png)
2. Mở **Googlesheet Importer Window**, vào Menu Item: **BigBear -> Googlesheet Importer**
 
![share access](Documentation~/images/open google sheet importer.png)

3. Trong tab **Data Config** chọn **Add New Sheet**
4. Điền thông tin của sheet

![share access](Documentation~/images/add sheet.png)

- <font color="cyan">**Sheet Name**</font> : tên của sheet
- <font color="cyan">**Spreadsheet ID**</font> <font color = "red">**(*)**</font>: id của sheet lấy trên đường dẫn link của google sheet
- <font color="cyan">**Script Folder**</font> <font color = "red">**(*)**</font>: đường dẫn đến folder chứa script sau khi được generate
- <font color="cyan">**Asset Folder**</font> <font color = "red">**(*)**</font>: đường dẫn đến folder chứa Scriptable Object sau khi được generate
- <font color="cyan">**Sprite Folder**</font> : đường dẫn đến folder chứa Sprite cho trường hợp có các field là sprite
- <font color="cyan">**Skeleton Folder**</font> : đường dẫn đến folder chứa Skeleton Data cho trường hợp có các field là anim Spine
- <font color="cyan">**Prefab Folder**</font> : đường dẫn đến folder chứa Prefab cho trường hợp có các field là prefab
- <font color="cyan">**Default Sprite**</font> : là sprite default nếu không thể load được sprite như trong data <br>
các field có <font color = "red">**(*)**</font> là bắt buộc điền thông tin

#### II. Xóa sheet data
1. Trong tab **Data Config**, phần **Delete Sheet**, chọn sheet cần xóa trong dropdown, rồi bấm **Delete Sheet**<br>
![share access](Documentation~/images/delete sheet.png)
#### II. Import Data từng sheet
1. Chọn sheet muốn import data bên menu trái
2. Điền thông tin vào **Sheet Info**
3. bấm **Load Sheet** để lấy data của sheet về
4. Chọn Tab muốn generate data trong phần **Select Tab**, Data của tab sẽ hiển thị ngay bên dưới
5. bấm **Generate Script** để generate script của tab đã chọn
6. bấm **Generate Assets** để generate ScriptableObject chứa data của tab đã chọn<br>
![share access](Documentation~/images/sheet data.png)
