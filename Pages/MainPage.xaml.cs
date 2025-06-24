using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Oraton.Classes;
using Oraton.DataBase;
using Oraton.ViewModel;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Oraton.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private MainViewModel _viewModel;

        public MainPage()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            _viewModel.LoadNotes();

            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
        private void NoteRichTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                _viewModel.ProcessEnterKeyCommand.Execute(e);
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.NoteContentDocument))
            {
                if (_viewModel.NoteContentDocument != null)
                {
                    NoteRichTextBox.Document = _viewModel.NoteContentDocument;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveCommand.Execute(null);
        }

        private void CreateNoteButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CreateNoteCommand.Execute(null);
        }
        private void FontSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontSelector.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedFont = selectedItem.Content.ToString();
                ChangeSelectedTextFont(selectedFont);
            }
        }

        private void FontSizeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontSizeSelector.SelectedItem is ComboBoxItem selectedItem)
            {
                double selectedSize = double.Parse(selectedItem.Content.ToString());
                ChangeSelectedTextSize(selectedSize);
            }
        }

        private void ChangeSelectedTextFont(string fontName)
        {
            TextSelection selectedText = NoteRichTextBox.Selection;
            if (!selectedText.IsEmpty)
            {
                selectedText.ApplyPropertyValue(TextElement.FontFamilyProperty, new FontFamily(fontName));
            }
        }

        private void ChangeSelectedTextSize(double fontSize)
        {
            TextSelection selectedText = NoteRichTextBox.Selection;
            if (!selectedText.IsEmpty)
            {
                selectedText.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
            }
        }
        private void NoteRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                TextRange textRange = new TextRange(NoteRichTextBox.Document.ContentStart, NoteRichTextBox.Document.ContentEnd);
                viewModel.NoteContent = textRange.Text.Trim();
            }
        }
        private void AddReminderButton_Click(object sender, RoutedEventArgs e)
        {
            TextRange selectedTextRange = new TextRange(NoteRichTextBox.Selection.Start, NoteRichTextBox.Selection.End);
            string selectedText = selectedTextRange.Text.Trim();

            if (string.IsNullOrEmpty(selectedText))
            {
                MessageBox.Show("Будь ласка, виділіть текст з датою і часом.");
                return;
            }

            // Спроба знайти всю строку, в якій знаходиться виділення
            string fullText = new TextRange(NoteRichTextBox.Document.ContentStart, NoteRichTextBox.Document.ContentEnd).Text;
            string[] lines = fullText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                if (line.Contains(selectedText))
                {
                    // Спроба витягнути дату та повідомлення
                    // Формат очікується: "dd.MM.yyyy HH:mm текст"
                    string[] parts = line.Split(new[] { ' ' }, 3); // split на 3 частини: дата, час, текст

                    if (parts.Length >= 3 &&
                        DateTime.TryParseExact(parts[0] + " " + parts[1], "dd.MM.yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime reminderTime))
                    {
                        string message = parts[2];
                        new Reminder(reminderTime, message);
                        MessageBox.Show($"Нагадування встановлено: \"{message}\" на {reminderTime}");
                        return;
                    }
                }
            }

            MessageBox.Show("Не вдалося знайти рядок з правильною датою та текстом.\nОчікуваний формат: дд.мм.рррр гг:хх Ваш текст");
        }

    }
}
