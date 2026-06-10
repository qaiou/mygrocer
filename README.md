# MYGROCER — Nura's Part Setup Guide
## CSE 6234 Software Design | Group 1 TT3L

---

## FILES YOU NEED TO COPY INTO THE PROJECT

Place each file into the correct folder in your Visual Studio project:

```
MYGROCER/
├── MYGROCER.csproj              ← replace existing
├── Program.cs                   ← replace existing
├── Controllers/
│   ├── HomeController.cs        ← replace existing
│   └── ProductsController.cs   ← replace existing
├── Models/
│   └── ProductsModel.cs        ← replace existing
├── Data/
│   ├── AppDbContext.cs          ← NEW file (create Data folder)
│   └── DbConnectionSingleton.cs ← NEW file (Singleton Pattern)
└── Views/
    ├── _ViewImports.cshtml      ← replace
    ├── _ViewStart.cshtml        ← replace
    ├── Shared/
    │   └── _Layout.cshtml       ← replace
    ├── Home/
    │   └── Index.cshtml         ← replace
    └── Products/
        ├── Index.cshtml         ← NEW (product listing)
        ├── AdminIndex.cshtml    ← NEW (admin panel)
        ├── Create.cshtml        ← NEW (add product)
        ├── Edit.cshtml          ← NEW (edit product)
        ├── Delete.cshtml        ← NEW (delete confirm)
        └── Details.cshtml       ← NEW (product detail)
```

---

## SETUP STEPS IN VISUAL STUDIO

### Step 1 — Download Visual Studio Community 2022 (FREE)
https://visualstudio.microsoft.com/vs/community/
When installing, tick: ✅ ASP.NET and web development

### Step 2 — Open the project
- Open Visual Studio
- File → Open → Project/Solution
- Navigate to your MYGROCER folder and open MYGROCER.csproj

### Step 3 — Copy all the files above into the project

### Step 4 — Run the app
- Press F5 or click the green ▶ Run button
- Browser opens automatically at https://localhost:7284
- The SQLite database (mygrocer.db) is created automatically with sample products
- No SQL Server installation needed!

---

## WHAT EACH LAYER DOES (for your presentation)

| Layer | Files | What it does |
|---|---|---|
| UI Layer (Frontend) | Views/*.cshtml | HTML pages the user sees |
| Business Logic Layer | Controllers/*.cs | Handles requests, applies rules |
| Database Layer | Data/AppDbContext.cs + Models/ | Stores/retrieves data from SQLite |

---

## SINGLETON PATTERN EXPLANATION (Design Pattern 1)

File: `Data/DbConnectionSingleton.cs`

The Singleton ensures only ONE database connection configuration instance exists.
You can see it working on the Admin Panel — it shows:
"Singleton DB Connection | Access Count: X | Connection: Data Source=mygrocer.db"

Every time a page loads, the access count goes up — proving the SAME instance is reused.

---

## URLS WHEN RUNNING

| Page | URL |
|---|---|
| Homepage | https://localhost:7284/ |
| All Products | https://localhost:7284/Products |
| Admin Panel | https://localhost:7284/Products/AdminIndex |
| Add Product | https://localhost:7284/Products/Create |

---

## YOUR PART COVERS

✅ Product listing (Homepage — featured products)
✅ Product listing (Products page — with search + category filter)  
✅ Admin View — list all products with search
✅ Admin Add new product (with validation)
✅ Admin Edit product (with validation)
✅ Admin Delete product (with confirmation)
✅ Design Pattern 1 — Singleton (DbConnectionSingleton.cs)
✅ All 3 Layers — UI (Views) + Business Logic (Controllers) + Database (EF Core + SQLite)
