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

namespace Gofroychetqq
{
    /// <summary>
    /// Логика взаимодействия для PrihodWindow.xaml
    /// </summary>
    public partial class PrihodWindow : Window
    {
        private etonEntities _context;
        private List<Material> _materials;

        public PrihodWindow()
        {
            InitializeComponent();
            _context = new etonEntities();
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
                // Проверка заполнения обязательных полей
                if (SupplierComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите поставщика", "Предупреждение",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(DocumentNumberTextBox.Text))
                {
                    MessageBox.Show("Введите номер документа", "Предупреждение",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!SupplyDatePicker.SelectedDate.HasValue)
                {
                    MessageBox.Show("Выберите дату поставки", "Предупреждение",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Создание записи о поставке
                var supply = new Supply
                {
                    SupplierID = ((Supplier)SupplierComboBox.SelectedItem).SupplierID,
                    DocumentNumber = DocumentNumberTextBox.Text,
                    SupplyDate = SupplyDatePicker.SelectedDate.Value,
                    Note = NoteTextBox.Text,
                    SupplyStatusID = 1 // Статус "Новая поставка"
                };

                _context.Supply.Add(supply);
                _context.SaveChanges();

                // Создание записей о приходе материалов
                foreach (var item in MaterialsDataGrid.Items)
                {
                    var materialItem = item as dynamic;
                    if (materialItem?.Material != null && materialItem.Quantity > 0)
                    {
                        var prihod = new Prihod
                        {
                            SupplyID = supply.SupplyID,
                            MaterialID = materialItem.Material.MaterialID,
                            Quantity = materialItem.Quantity,
                            PrihodDate = DateTime.Now
                        };
                        _context.Prihod.Add(prihod);
                    }
                }

                _context.SaveChanges();
                MessageBox.Show("Приход сырья успешно оформлен", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
