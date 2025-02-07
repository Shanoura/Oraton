using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Oraton.Classes;

namespace Oraton.DataBase
{
    internal class DatabaseReader
    {
        public User GetUserByCredentials(string username, string password)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                string query = "SELECT id, username, password, email FROM users WHERE username = @username AND password = @password";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Password = reader.GetString(2),
                                Email = reader.GetString(3)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public bool IsUsernameTaken(string username)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                string query = "SELECT COUNT(*) FROM users WHERE username = @username";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    return (long)command.ExecuteScalar() > 0;
                }
            }
        }

        public bool IsEmailTaken(string email)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                string query = "SELECT COUNT(*) FROM users WHERE email = @email";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    return (long)command.ExecuteScalar() > 0;
                }
            }
        }

        public List<Note> GetNotes()
        {
            var notes = new List<Note>();

            // Проверяем, что пользователь авторизован
            if (CurrentUser.IsAuthenticated())
            {
                int currentUserId = CurrentUser.User.Id; // Получаем ID текущего пользователя

                using (var connection = DatabaseConnection.GetConnection())
                {
                    // Добавляем условие для выборки заметок только для текущего пользователя
                    string query = "SELECT id, title, user_id, created_at, updated_at FROM notes WHERE user_id = @userId";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        // Добавляем параметр для запроса
                        command.Parameters.AddWithValue("@userId", currentUserId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                notes.Add(new Note
                                {
                                    Id = reader.GetInt32(0),
                                    Title = reader.GetString(1),
                                    UserId = reader.GetInt32(2),
                                    CreatedAt = reader.GetDateTime(3),
                                    UpdatedAt = reader.GetDateTime(4)
                                });
                            }
                        }
                    }
                }
            }
            return notes;
        }

        public List<NoteRow> GetNoteRows()
        {
            var noteRows = new List<NoteRow>();
            using (var connection = DatabaseConnection.GetConnection())
            {
                string query = "SELECT id, note_id, content, \"order\" FROM note_rows"; // исправил кавычки для PostgreSQL
                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new NoteRow
                        {
                            Id = reader.GetInt32(0),
                            NoteId = reader.GetInt32(1),
                            Content = reader.GetString(2),
                            Order = reader.GetInt32(3)
                        };
                        noteRows.Add(row);
                        System.Diagnostics.Debug.WriteLine($"Loaded NoteRow: {row.Content} (NoteId: {row.NoteId})");
                    }
                }
            }
            return noteRows;
        }
        public List<TextStyle> GetTextStylesForNote(int noteId)
        {
            var textStyles = new List<TextStyle>();

            using (var connection = DatabaseConnection.GetConnection())
            {
                using (var cmd = new NpgsqlCommand("SELECT id, note_id, row_id, start_index, length, font_family, font_size FROM note_text_styles WHERE note_id = @noteId", connection))
                {
                    cmd.Parameters.AddWithValue("@noteId", noteId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            textStyles.Add(new TextStyle
                            {
                                Id = reader.GetInt32(0),
                                NoteId = reader.GetInt32(1),
                                RowId = reader.GetInt32(2),
                                StartIndex = reader.GetInt32(3),
                                Length = reader.GetInt32(4),
                                FontFamily = reader.IsDBNull(5) ? null : reader.GetString(5),
                                FontSize = reader.IsDBNull(6) ? null : (float?)reader.GetDouble(6)
                            });
                        }
                    }
                }
            }

            return textStyles;
        }

    }

}
