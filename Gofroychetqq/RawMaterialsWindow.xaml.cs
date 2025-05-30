using System.Linq;
using System.Windows;

namespace Gofroychetqq
{
    public partial class RawMaterialsWindow : Window
    {
        private etonEntities db = new etonEntities();
        public RawMaterialsWindow()
        {
            InitializeComponent();
            LoadMaterials();
        }

        private void LoadMaterials()
        {
            db = new etonEntities(); // Пересоздаём контекст для получения свежих данных
            var materials = db.Material.ToList().Select(m => new
            {
                m.MaterialID,
                m.Name,
                m.MaterialType,
                m.MinimumStock,
                Stock = (db.Prihod.Where(p => p.MaterialID == m.MaterialID).Sum(p => (double?)p.Quantity) ?? 0)
                        - (db.Rashod.Where(r => r.MaterialID == m.MaterialID).Sum(r => (double?)r.Quantity) ?? 0)
            }).ToList();
            RawMaterialsDataGrid.ItemsSource = materials;
        }

        private void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditMaterialWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadMaterials();
            }
        }

        private void EditMaterial_Click(object sender, RoutedEventArgs e)
        {
            var material = ((FrameworkElement)sender).Tag;
            if (material != null)
            {
                // �������� MaterialID ����� ���������, ��� ��� ��� ��������� ���
                var materialIDProperty = material.GetType().GetProperty("MaterialID");
                if (materialIDProperty != null)
                {
                    int materialID = (int)materialIDProperty.GetValue(material);
                    var dbMaterial = db.Material.FirstOrDefault(x => x.MaterialID == materialID);
                    if (dbMaterial != null)
                    {
                        var editWindow = new EditMaterialWindow(dbMaterial);
                        if (editWindow.ShowDialog() == true)
                        {
                            LoadMaterials();
                        }
                    }
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
} 