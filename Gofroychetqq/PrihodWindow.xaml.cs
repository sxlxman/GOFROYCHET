using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace Gofroychetqq
{
    /// <summary>
    /// Логика взаимодействия для PrihodWindow.xaml
    /// </summary>
    public partial class PrihodWindow : Window
    {
        private etonEntities _context;
        private List<Material> _materials;
        private User _currentUser;
        private Supply _currentSupply;

        public PrihodWindow(User currentUser)
        {
            InitializeComponent();
            _context = new etonEntities();
            _currentUser = currentUser;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Загрузка поставщиков
                SupplierComboBox.ItemsSource = _context.Supplier.ToList();

                // Загрузка материалов
                _materials = _context.Material.ToList();
                var materialColumn = (DataGridComboBoxColumn)MaterialsDataGrid.Columns[0];
                materialColumn.ItemsSource = _materials;

                // Установка текущей даты
                SupplyDatePicker.SelectedDate = DateTime.Now;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                // 1. Создаем поставку
                _currentSupply = new Supply
                {
                    SupplierID = ((Supplier)SupplierComboBox.SelectedItem).SupplierID,
                    DocumentNumber = DocumentNumberTextBox.Text,
                    SupplyDate = SupplyDatePicker.SelectedDate.Value,
                    Note = NoteTextBox.Text,
                    SupplyStatusID = 1 // Статус "Новая поставка"
                };
                _context.Supply.Add(_currentSupply);
                _context.SaveChanges();

                // 2. Создаем накладную
                var nakladnaya = new Nakladnaya
                {
                    Number = GetNextNakladnayaNumber(),
                    CreateDate = DateTime.Now,
                    TypeNakladnaya = 1 // Тип "Приходная накладная"
                };
                _context.Nakladnaya.Add(nakladnaya);
                _context.SaveChanges();

                // 3. Создаем записи прихода
                foreach (var item in MaterialsDataGrid.Items)
                {
                    var materialItem = item as dynamic;
                    if (materialItem?.Material != null && materialItem.Quantity > 0)
                    {
                        var prihod = new Prihod
                        {
                            SupplyID = _currentSupply.SupplyID,
                            MaterialID = materialItem.Material.MaterialID,
                            Quantity = materialItem.Quantity,
                            PrihodDate = DateTime.Now,
                            UserID = _currentUser.UserID,
                            NakladnayaID = nakladnaya.NakladnayaID
                        };
                        _context.Prihod.Add(prihod);
                    }
                }

                _context.SaveChanges();
                MessageBox.Show("Приход сырья успешно оформлен", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);

                // Активируем кнопку печати
                PrintButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (SupplierComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите поставщика", "Предупреждение",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(DocumentNumberTextBox.Text))
            {
                MessageBox.Show("Введите номер документа", "Предупреждение",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!SupplyDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату поставки", "Предупреждение",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Проверка на наличие материалов
            if (MaterialsDataGrid.Items.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один материал", "Предупреждение",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Проверка на корректность количества
            foreach (var item in MaterialsDataGrid.Items)
            {
                var materialItem = item as dynamic;
                if (materialItem?.Material == null || materialItem.Quantity <= 0)
                {
                    MessageBox.Show("Укажите корректное количество для всех материалов", "Предупреждение",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        private int GetNextNakladnayaNumber()
        {
            var lastNakladnaya = _context.Nakladnaya
                .OrderByDescending(n => n.Number)
                .FirstOrDefault();

            return lastNakladnaya?.Number + 1 ?? 1;
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSupply != null)
            {
                var printWindow = new NakladnayaPrintWindow(_currentSupply);
                printWindow.ShowDialog();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            var materials = MaterialsDataGrid.ItemsSource as ObservableCollection<MaterialQuantity>;
            if (materials == null)
            {
                materials = new ObservableCollection<MaterialQuantity>();
                MaterialsDataGrid.ItemsSource = materials;
            }
            materials.Add(new MaterialQuantity { Quantity = 0 });
        }

        private void DeleteMaterial_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var material = button.DataContext as MaterialQuantity;
                if (material != null)
                {
                    var materials = MaterialsDataGrid.ItemsSource as ObservableCollection<MaterialQuantity>;
                    materials?.Remove(material);
                }
            }
        }
    }
}
