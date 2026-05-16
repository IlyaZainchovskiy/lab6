using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace lab6
{
    #region 1. Моделі даних (Entity Framework Core)

    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    }

    public class Recipe
    {
        public int RecipeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CookingTimeMinutes { get; set; }

        public int CategoryId { get; set; }

        public virtual Category Category { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
    #endregion

    #region 2. Контекст бази даних (AppDbContext)

    public class AppDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Recipe> Recipes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Oop_PW6;Trusted_Connection=True;");
        }
    }
    #endregion

    #region 3. Інтерфейс користувача та Логіка (MainForm)

    public class MainForm : Form
    {
        private TabControl tabControl;
        private TabPage tabCrud;
        private TabPage tabListView;
        private TabPage tabDataGridView;

        private ListBox lstRecipes;
        private Label lblTitle, lblCategory, lblTime, lblDescription;
        private TextBox txtTitle;
        private ComboBox cmbCategory;
        private NumericUpDown numTime;
        private TextBox txtDescription;
        private Button btnAdd, btnEdit, btnDelete;

        private ListView listViewRecipes;

        private DataGridView dgvRecipes;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Кулінарна книга - Практична робота 6";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            tabControl = new TabControl { Dock = DockStyle.Fill };
            tabCrud = new TabPage("Управління рецептами (CRUD)");
            tabListView = new TabPage("Перегляд (ListView)");
            tabDataGridView = new TabPage("Перегляд (DataGridView)");

            tabControl.TabPages.Add(tabCrud);
            tabControl.TabPages.Add(tabListView);
            tabControl.TabPages.Add(tabDataGridView);

            // ================= Вкладка 1 =================
            lstRecipes = new ListBox { Left = 10, Top = 10, Width = 250, Height = 450 };
            lstRecipes.SelectedIndexChanged += LstRecipes_SelectedIndexChanged;

            lblTitle = new Label { Text = "Назва страви:", Left = 280, Top = 15, Width = 100 };
            txtTitle = new TextBox { Left = 390, Top = 10, Width = 350 };

            lblCategory = new Label { Text = "Категорія:", Left = 280, Top = 55, Width = 100 };
            cmbCategory = new ComboBox { Left = 390, Top = 50, Width = 350, DropDownStyle = ComboBoxStyle.DropDownList };

            lblTime = new Label { Text = "Час (хв):", Left = 280, Top = 95, Width = 100 };
            numTime = new NumericUpDown { Left = 390, Top = 90, Width = 100, Maximum = 1000 };

            lblDescription = new Label { Text = "Опис/Інгредієнти:", Left = 280, Top = 135, Width = 120 };
            txtDescription = new TextBox { Left = 280, Top = 160, Width = 460, Height = 250, Multiline = true, ScrollBars = ScrollBars.Vertical };

            btnAdd = new Button { Text = "Додати", Left = 280, Top = 430, Width = 100, BackColor = Color.LightGreen };
            btnEdit = new Button { Text = "Редагувати", Left = 390, Top = 430, Width = 100, BackColor = Color.LightYellow };
            btnDelete = new Button { Text = "Видалити", Left = 500, Top = 430, Width = 100, BackColor = Color.LightCoral };

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;

            tabCrud.Controls.AddRange(new Control[] { lstRecipes, lblTitle, txtTitle, lblCategory, cmbCategory, lblTime, numTime, lblDescription, txtDescription, btnAdd, btnEdit, btnDelete });

            // ================= Вкладка 2=================
            listViewRecipes = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            listViewRecipes.Columns.Add("ID", 50);
            listViewRecipes.Columns.Add("Назва", 200);
            listViewRecipes.Columns.Add("Категорія", 150);
            listViewRecipes.Columns.Add("Час (хв)", 100);
            tabListView.Controls.Add(listViewRecipes);

            // ================= Вкладка 3 =================
            dgvRecipes = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            tabDataGridView.Controls.Add(dgvRecipes);

            this.Controls.Add(tabControl);
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                using (var db = new AppDbContext())
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                   
                    if (!db.Categories.Any())
                    {
                        var cat1 = new Category { Name = "Супи" };
                        var cat2 = new Category { Name = "Основні страви" };
                        var cat3 = new Category { Name = "Десерти" };

                        db.Categories.AddRange(cat1, cat2, cat3);
                        db.SaveChanges(); 

                        var rec1 = new Recipe { Title = "Борщ український", Description = "Буряк, капуста, м'ясо, картопля...", CookingTimeMinutes = 120, CategoryId = cat1.CategoryId };
                        var rec2 = new Recipe { Title = "Стейк Рібай", Description = "Яловичина, сіль, перець, розмарин...", CookingTimeMinutes = 30, CategoryId = cat2.CategoryId };
                        var rec3 = new Recipe { Title = "Тирамісу", Description = "Маскарпоне, савоярді, кава...", CookingTimeMinutes = 45, CategoryId = cat3.CategoryId };

                        db.Recipes.AddRange(rec1, rec2, rec3);
                        db.SaveChanges(); 
                    }
                }

                RefreshData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка ініціалізації БД: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void RefreshData()
        {
            using (var db = new AppDbContext())
            {
                var recipes = db.Recipes.Include(r => r.Category).ToList();
                var categories = db.Categories.ToList();

                int? selectedCategoryId = cmbCategory.SelectedValue as int?;
                cmbCategory.DataSource = categories;
                cmbCategory.DisplayMember = "Name";
                cmbCategory.ValueMember = "CategoryId";
                if (selectedCategoryId.HasValue) cmbCategory.SelectedValue = selectedCategoryId.Value;

                // ---------------- Tab 1 ----------------
                int selectedRecipeIdx = lstRecipes.SelectedIndex;
                lstRecipes.DataSource = recipes;
                if (selectedRecipeIdx >= 0 && selectedRecipeIdx < lstRecipes.Items.Count)
                    lstRecipes.SelectedIndex = selectedRecipeIdx;

                // ---------------- Tab 2 ----------------
                listViewRecipes.Items.Clear();
                foreach (var recipe in recipes)
                {
                    var item = new ListViewItem(recipe.RecipeId.ToString());
                    item.SubItems.Add(recipe.Title);
                    item.SubItems.Add(recipe.Category?.Name ?? "Невідомо");
                    item.SubItems.Add(recipe.CookingTimeMinutes.ToString());
                    listViewRecipes.Items.Add(item);
                }

                // ---------------- Tab 3 ----------------
                var dgvData = recipes.Select(r => new
                {
                    ID = r.RecipeId,
                    Назва_Страви = r.Title,
                    Категорія = r.Category?.Name,
                    Час_Приготування = r.CookingTimeMinutes,
                    Опис = r.Description
                }).ToList();

                dgvRecipes.DataSource = dgvData;
            }
        }

        private void LstRecipes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstRecipes.SelectedItem is Recipe selectedRecipe)
            {
                txtTitle.Text = selectedRecipe.Title;
                txtDescription.Text = selectedRecipe.Description;
                numTime.Value = selectedRecipe.CookingTimeMinutes;
                cmbCategory.SelectedValue = selectedRecipe.CategoryId;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text)) return;

            using (var db = new AppDbContext())
            {
                var newRecipe = new Recipe
                {
                    Title = txtTitle.Text,
                    Description = txtDescription.Text,
                    CookingTimeMinutes = (int)numTime.Value,
                    CategoryId = (int)cmbCategory.SelectedValue
                };

                db.Recipes.Add(newRecipe);
                db.SaveChanges();
            }
            RefreshData(); 
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (lstRecipes.SelectedItem is Recipe selectedRecipe)
            {
                using (var db = new AppDbContext())
                {
                    var recipeToUpdate = db.Recipes.Find(selectedRecipe.RecipeId);
                    if (recipeToUpdate != null)
                    {
                        recipeToUpdate.Title = txtTitle.Text;
                        recipeToUpdate.Description = txtDescription.Text;
                        recipeToUpdate.CookingTimeMinutes = (int)numTime.Value;
                        recipeToUpdate.CategoryId = (int)cmbCategory.SelectedValue;

                        db.SaveChanges();
                    }
                }
                RefreshData(); 
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (lstRecipes.SelectedItem is Recipe selectedRecipe)
            {
                using (var db = new AppDbContext())
                {
                    var recipeToDelete = db.Recipes.Find(selectedRecipe.RecipeId);
                    if (recipeToDelete != null)
                    {
                        db.Recipes.Remove(recipeToDelete);
                        db.SaveChanges();
                    }
                }

                txtTitle.Text = "";
                txtDescription.Text = "";
                numTime.Value = 0;

                RefreshData(); 
            }
        }
    }
    #endregion

    #region Entry Point (Program.cs)
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
    #endregion
}