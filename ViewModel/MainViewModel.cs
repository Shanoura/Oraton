using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Oraton.Classes;
using Oraton.Commands;
using Oraton.DataBase;
using Microsoft.VisualBasic;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media;

namespace Oraton.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseReader _databaseReader = new DatabaseReader();
        private readonly DatabaseWriter _databaseWriter = new DatabaseWriter();

        private Note _selectedNote;
        private string _noteContent;
        public ObservableCollection<TextStyle> TextStyles { get; set; } = new();
        public ObservableCollection<Note> Notes { get; set; } = new ObservableCollection<Note>();

        public ICommand SaveCommand { get; }
        public ICommand CreateNoteCommand { get; }
        public ICommand RenameNoteCommand { get; }
        public ICommand DeleteNoteCommand { get; }
        public ICommand ProcessEnterKeyCommand { get; }

        public Note SelectedNote
        {
            get => _selectedNote;
            set
            {
                _selectedNote = value;
                OnPropertyChanged();
                LoadNoteContent();
            }
        }

        public string NoteContent
        {
            get => _noteContent;
            set
            {
                _noteContent = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            SaveCommand = new RelayCommand(_ => SaveNoteContent(), _ => SelectedNote != null);
            CreateNoteCommand = new RelayCommand(_ => CreateNewNote(), _ => true);
            RenameNoteCommand = new RelayCommand(_ => RenameNote(), _ => SelectedNote != null);
            DeleteNoteCommand = new RelayCommand(_ => DeleteNote(), _ => SelectedNote != null);
            ProcessEnterKeyCommand = new RelayCommand<KeyEventArgs>(ProcessEnterKey);
        }

        public void LoadNotes()
        {
            Notes.Clear();
            foreach (var note in _databaseReader.GetNotes())
            {
                Notes.Add(note);
            }
        }

        public void SaveCurrentNote()
        {
            if (SelectedNote == null) return;

            _databaseWriter.UpdateNote(SelectedNote);
            OnPropertyChanged();
        }
        private FlowDocument _noteContentDocument = new FlowDocument();
        public FlowDocument NoteContentDocument
        {
            get => _noteContentDocument;
            set
            {
                _noteContentDocument = value;
                OnPropertyChanged(nameof(NoteContentDocument));
            }
        }
        private void SaveTextStyles()
        {
            if (SelectedNote == null) return;

            TextStyles.Clear();

            var noteRows = _databaseReader.GetNoteRows()
                .Where(row => row.NoteId == SelectedNote.Id)
                .OrderBy(row => row.Order)
                .ToList();

            int rowIndex = 0;

            foreach (var block in NoteContentDocument.Blocks)
            {
                if (block is Paragraph paragraph && rowIndex < noteRows.Count)
                {
                    int startIndex = 0;
                    int rowId = noteRows[rowIndex].Id; // Получаем реальный RowId

                    foreach (var inline in paragraph.Inlines)
                    {
                        if (inline is Run run)
                        {
                            var text = run.Text;
                            if (!string.IsNullOrEmpty(text))
                            {
                                TextStyles.Add(new TextStyle
                                {
                                    NoteId = SelectedNote.Id,
                                    RowId = rowId, // Используем реальный ID строки
                                    StartIndex = startIndex,
                                    Length = text.Length,
                                    FontFamily = run.FontFamily?.Source,
                                    FontSize = (float?)run.FontSize
                                });

                                startIndex += text.Length;
                            }
                        }
                    }

                    rowIndex++; // Переход к следующей строке
                }
            }

            _databaseWriter.SaveTextStyles(TextStyles.ToList());
        }


        private void LoadNoteContent()
        {
            if (SelectedNote != null)
            {
                var rows = _databaseReader.GetNoteRows()
                    .Where(row => row.NoteId == SelectedNote.Id)
                    .OrderBy(row => row.Order)
                    .ToList();

                var textStyles = _databaseReader.GetTextStylesForNote(SelectedNote.Id);

                var flowDocument = new FlowDocument();

                foreach (var row in rows)
                {
                    var paragraph = new Paragraph();
                    var textStylesForRow = textStyles.Where(s => s.RowId == row.Id).OrderBy(s => s.StartIndex).ToList();

                    int currentIndex = 0;
                    foreach (var style in textStylesForRow)
                    {
                        if (style.StartIndex > currentIndex)
                        {
                            paragraph.Inlines.Add(new Run(row.Content.Substring(currentIndex, style.StartIndex - currentIndex)));
                        }

                        var styledRun = new Run(row.Content.Substring(style.StartIndex, style.Length));

                        if (!string.IsNullOrEmpty(style.FontFamily))
                        {
                            styledRun.FontFamily = new FontFamily(style.FontFamily);
                        }

                        if (style.FontSize.HasValue)
                        {
                            styledRun.FontSize = style.FontSize.Value;
                        }

                        paragraph.Inlines.Add(styledRun);
                        currentIndex = style.StartIndex + style.Length;
                    }

                    if (currentIndex < row.Content.Length)
                    {
                        paragraph.Inlines.Add(new Run(row.Content.Substring(currentIndex)));
                    }

                    flowDocument.Blocks.Add(paragraph);
                }

                NoteContentDocument = flowDocument;
            }
            else
            {
                NoteContentDocument = new FlowDocument();
            }
        }

        private void CreateNewNote()
        {
            int currentUserId = CurrentUser.User.Id;
            var newNote = new Note
            {
                Title = "Нова замітка",
                UserId = currentUserId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _databaseWriter.InsertNote(newNote);
            Notes.Add(newNote);
            SelectedNote = newNote;
            LoadNotes();
        }

        private void SaveNoteContent()
        {
            if (SelectedNote == null) return;

            var newContent = NoteContent;

            var rows = _databaseReader.GetNoteRows()
                .Where(row => row.NoteId == SelectedNote.Id)
                .OrderBy(row => row.Order)
                .ToList();

            var newLines = newContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            var updatedRows = new List<NoteRow>();
            var newRows = new List<NoteRow>();

            for (int i = 0; i < newLines.Length; i++)
            {
                if (i < rows.Count)
                {
                    if (rows[i].Content != newLines[i])
                    {
                        rows[i].Content = newLines[i];
                        updatedRows.Add(rows[i]);
                    }
                }
                else
                {
                    var newRow = new NoteRow
                    {
                        NoteId = SelectedNote.Id,
                        Content = newLines[i],
                        Order = i
                    };
                    newRows.Add(newRow);
                }
            }

            var deletedRows = rows.Skip(newLines.Length).ToList();
            _databaseWriter.SaveNoteRows(updatedRows, newRows, deletedRows);

            // Сохранение стилей текста
            SaveTextStyles();

            SaveCurrentNote();
            MessageBox.Show("Зміни збережено!", "Збереження", MessageBoxButton.OK, MessageBoxImage.Information);
        }



        private void RenameNote()
        {
            if (SelectedNote == null) return;

            var newTitle = Microsoft.VisualBasic.Interaction.InputBox(
                "Введіть нову назву нотатки:", "Перейменування", SelectedNote.Title);

            if (!string.IsNullOrWhiteSpace(newTitle) && newTitle != SelectedNote.Title)
            {
                SelectedNote.Title = newTitle;
                _databaseWriter.UpdateNote(SelectedNote);

                // Обновляем отображение
                OnPropertyChanged(nameof(SelectedNote));
                LoadNotes();
            }
        }

        private void DeleteNote()
        {
            if (SelectedNote == null) return;

            var result = MessageBox.Show(
                "Ви впевнені, що хочете видалити цю нотатку?",
                "Видалення нотатки", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _databaseWriter.DeleteNote(SelectedNote.Id);
                Notes.Remove(SelectedNote);
                SelectedNote = Notes.FirstOrDefault();

                // Принудительно обновляем команды
                CommandManager.InvalidateRequerySuggested();
            }
        }
        private void ProcessEnterKey(object parameter)
        {
            if (parameter is KeyEventArgs e && e.Key == Key.Enter)
            {
                int caretIndex = NoteTextBoxCaretIndex;
                int lineStart = NoteContent.LastIndexOf('\n', caretIndex - 1) + 1;

                if (lineStart < NoteContent.Length && NoteContent.Substring(lineStart).Trim().StartsWith("-"))
                {
                    e.Handled = true;
                    NoteContent = NoteContent.Insert(caretIndex, "\n- ");
                    NoteTextBoxCaretIndex = caretIndex + 3;
                }
            }
        }

        // Свойство для управления положением каретки
        private int _noteTextBoxCaretIndex;
        public int NoteTextBoxCaretIndex
        {
            get => _noteTextBoxCaretIndex;
            set
            {
                _noteTextBoxCaretIndex = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
