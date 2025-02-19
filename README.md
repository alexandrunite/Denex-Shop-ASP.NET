# 🛍️ Denex - Online Store Platform  

## 📌 Project Overview  
Denex is a web-based e-commerce application built with **ASP.NET Core 8.0, Entity Framework, and MySQL**. The platform allows users to browse products, manage their shopping carts, place orders, and interact with an advanced role-based authentication system. The application is developed as part of the **Web Application Development (DAW) course**.

---

## 🚀 Key Features  

### 🔹 Authentication & Authorization  
- Secure user authentication with **ASP.NET Identity Framework**  
- Role-based access control (**Admin, Collaborator, Registered User, Guest**)  
- Admin panel for **managing users, approving product requests, and overseeing store operations**  

### 🔹 Product Management  
- **CRUD (Create, Read, Update, Delete)** functionality for products  
- Categories system to classify products efficiently  
- Product details page with **price, rating, and user reviews**  

### 🔹 Shopping Cart & Orders  
- Add products to cart & manage quantities  
- Cart linked to **individual user accounts**  
- Order checkout simulation  

### 🔹 Collaborator System  
- Registered users can apply for **Collaborator** status  
- Collaborators can **submit product requests** for admin approval  
- Request-based **product addition & modification workflow**  

### 🔹 Search & Filtering  
- Full-text **search bar for products**  
- Sorting by **price, rating, or category**  
- Pagination for better navigation  

### 🔹 Admin Dashboard  
- **Pending approval requests** section  
- Ability to **approve, reject, or delete products**  
- **User management panel** for assigning roles  

---

## 🛠 Tech Stack  
- **Backend:** ASP.NET Core 8.0, C#, Entity Framework Core  
- **Frontend:** Razor Pages, Bootstrap, HTML, CSS  
- **Database:** MySQL (hosted via Docker)  
- **Authentication:** ASP.NET Identity  
- **Logging & Monitoring:** ILogger, Console Logs  

---

## 🔧 Installation & Setup  

### 🐳 1. Run MySQL Database in Docker  
```sh
docker run --name shop-db -e MYSQL_ROOT_PASSWORD=root -e MYSQL_DATABASE=ShopDB -e MYSQL_USER=ShopUser -e MYSQL_PASSWORD=DeniselulAlex1! -p 3307:3306 -d mysql
```

### 🏗 2. Clone the Repository
```sh
git clone https://github.com/your-username/Denex.git
cd Denex
```

### ⚙ 3. Configure Connection String
In appsettings.json, ensure the database configuration matches:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=127.0.0.1;Port=3307;Database=ShopDB;User=ShopUser;Password=DeniselulAlex1!;"
}
```

### 🏃‍♂️ 4. Run the Application
```sh
dotnet restore
dotnet ef database update
dotnet run
```
Access the app at: http://localhost:5000/
