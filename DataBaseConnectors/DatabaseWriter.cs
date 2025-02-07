using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Oraton.Classes;

namespace Oraton.DataBase
{
    internal class DatabaseWriter
    {
        public void InsertUser(User user)
        {
            var data = new Dictionary<string, object>
        {
            { "username", user.Username },
            { "password", user.Password },
            { "email", user.Email }
        };
            InsertData("users", data);
        }

        public void InsertNote(Note note)
        {
            var data = new Dictionary<string, object>
        {
            { "title", note.Title },
            { "user_id", note.UserId },
            { "created_at", note.CreatedAt },
            { "updated_at", note.UpdatedAt }
        };
            InsertData("notes", data);
        }

        public void InsertNoteRow(NoteRow noteRow)
        {
            var data = new Dictionary<string, object>
        {
            { "note_id", noteRow.NoteId },
            { "content", noteRow.Content },
            { "order", noteRow.Order }
        };
            InsertData("note_rows", data);
        }

        private void InsertData(string tableName, Dictionary<string, object> data)
        {
            if (!IsValidTable(tableName))
            {
                throw new ArgumentException("Invalid table name.");
            }

            using (var connection = DatabaseConnection.GetConnection())
            {
                var columns = string.Join(", ", data.Keys);
                var parameters = string.Join(", ", GetParameterNames(data.Keys));

                string query = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    foreach (var kvp in data)
                    {
                        command.Parameters.AddWithValue(kvp.Key, kvp.Value);
                    }
                    command.ExecuteNonQuery();
                }
            }
        }
        public void SaveNoteRows(List<NoteRow> updatedRows, List<NoteRow> newRows, List<NoteRow> deletedRows)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var row in updatedRows)
                        {
                            UpdateNoteRow(row, connection);
                        }

                        foreach (var row in newRows)
                        {
                            InsertNoteRow(row, connection);
                        }

                        foreach (var row in deletedRows)
                        {
                            DeleteNoteRow(row, connection);
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private void InsertNoteRow(NoteRow noteRow, NpgsqlConnection connection)
        {
            string query = "INSERT INTO note_rows (note_id, content, \"order\") VALUES (@note_id, @content, @order) RETURNING id";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@note_id", noteRow.NoteId);
                command.Parameters.AddWithValue("@content", noteRow.Content);
                command.Parameters.AddWithValue("@order", noteRow.Order);

                // Получаем сгенерированный id из базы
                noteRow.Id = (int)command.ExecuteScalar();
            }
        }


        private void UpdateNoteRow(NoteRow noteRow, NpgsqlConnection connection)
        {
            string query = "UPDATE note_rows SET content = @content WHERE id = @id";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@content", noteRow.Content);
                command.Parameters.AddWithValue("@id", noteRow.Id);
                command.ExecuteNonQuery();
            }
        }

        private void DeleteNoteRow(NoteRow noteRow, NpgsqlConnection connection)
        {
            string query = "DELETE FROM note_rows WHERE id = @id";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", noteRow.Id);
                command.ExecuteNonQuery();
            }
        }
        public void UpdateNote(Note note)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                string query = "UPDATE notes SET title = @title, updated_at = @updated_at WHERE id = @id";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@title", note.Title);
                    command.Parameters.AddWithValue("@updated_at", DateTime.Now);
                    command.Parameters.AddWithValue("@id", note.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteNote(int noteId)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                // Удаление всех строк заметки из note_rows
                string deleteRowsQuery = "DELETE FROM note_rows WHERE note_id = @noteId";
                using (var deleteRowsCommand = new NpgsqlCommand(deleteRowsQuery, connection))
                {
                    deleteRowsCommand.Parameters.AddWithValue("@noteId", noteId);
                    deleteRowsCommand.ExecuteNonQuery();
                }

                // Удаление самой заметки
                string deleteNoteQuery = "DELETE FROM notes WHERE id = @noteId";
                using (var deleteNoteCommand = new NpgsqlCommand(deleteNoteQuery, connection))
                {
                    deleteNoteCommand.Parameters.AddWithValue("@noteId", noteId);
                    deleteNoteCommand.ExecuteNonQuery();
                }
            }
        }
        public void SaveTextStyles(List<TextStyle> textStyles)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = @"
                INSERT INTO note_text_styles (note_id, row_id, start_index, length, font_family, font_size)
                VALUES (@noteId, @rowId, @startIndex, @length, @fontFamily, @fontSize)
                ON CONFLICT (note_id, row_id, start_index, length) 
                DO UPDATE SET 
                    font_family = EXCLUDED.font_family, 
                    font_size = EXCLUDED.font_size;";

                    foreach (var style in textStyles)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@noteId", style.NoteId);
                        cmd.Parameters.AddWithValue("@rowId", style.RowId);
                        cmd.Parameters.AddWithValue("@startIndex", style.StartIndex);
                        cmd.Parameters.AddWithValue("@length", style.Length);
                        cmd.Parameters.AddWithValue("@fontFamily", (object?)style.FontFamily ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@fontSize", (object?)style.FontSize ?? DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private bool IsValidTable(string tableName)
        {
            return tableName == "users" || tableName == "notes" || tableName == "note_rows";
        }

        private IEnumerable<string> GetParameterNames(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                yield return "@" + key;
            }
        }
    }
}
