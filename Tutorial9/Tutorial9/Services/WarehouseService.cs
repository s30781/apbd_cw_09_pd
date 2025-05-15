using System.Data;
using Microsoft.Data.SqlClient;
using Tutorial9.Model;


namespace Tutorial9.Services;

public class WarehouseService(IConfiguration configuration) : IWarehouseService
{
    public async Task<int> AddProductToWarehouse(AddProductWarehouse product)
    {
        await using SqlConnection connection = new SqlConnection(configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand("", connection);

        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            //sprawdzenie czy orfukt istenieje w bazie danych
            command.CommandText = "SELECT COUNT(*) FROM Product WHERE IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            int count = (int)await command.ExecuteScalarAsync();

            if (count <= 0)
                throw new Exception($"Produkt o ID {product.IdProduct} nie istnieje.");

            //spradzenie cy zmagazyn insteje
            command.CommandText = "SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdWarehouse", product.IdWarehouse);
            count = (int)await command.ExecuteScalarAsync();

            if (count <= 0)
                throw new Exception($"Magazyn o ID {product.IdWarehouse} nie istnieje.");

            //szukanie amowienia
            command.CommandText = @"
                SELECT TOP 1 IdOrder
                FROM [Order]
                WHERE IdProduct = @IdProduct
                AND Amount = @Amount
                AND CreatedAt < @CreatedAt
                ORDER BY CreatedAt DESC";

            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            command.Parameters.AddWithValue("@Amount", product.Amount);
            command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);
            object? idOrderObj = await command.ExecuteScalarAsync();

            if (idOrderObj is null)
                throw new Exception("Nie znaleziono odpowiedniego zamówienia.");

            int idOrder = Convert.ToInt32(idOrderObj);

            //sprawdzanie czy zamowienie zrealizownay
            command.CommandText = "SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = @IdOrder";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            count = (int)await command.ExecuteScalarAsync();

            if (count > 0)
                throw new Exception("Zamówienie zostało już zrealizowane.");

            //aktualizowanie zamowienia - FulfilledAt
            command.CommandText = @"
                UPDATE [Order]
                SET FulfilledAt = @FulfilledAt
                WHERE IdOrder = @IdOrder";

            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@FulfilledAt", DateTime.Now);
            await command.ExecuteNonQueryAsync();

            //pobranie ceny jednostkowe
            command.CommandText = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            decimal price = (decimal)await command.ExecuteScalarAsync();

            //wstawienie produktu
            command.CommandText = @"
                INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdWarehouse", product.IdWarehouse);
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@Amount", product.Amount);
            command.Parameters.AddWithValue("@Price", price * product.Amount);
            command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);

            int newId = (int)await command.ExecuteScalarAsync();

            await transaction.CommitAsync();
            return newId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> AddProductToWarehouseProcedure(AddProductWarehouse product)
    {
        const string procedureName = "AddProductToWarehouse";

        await using SqlConnection conn = new SqlConnection(configuration.GetConnectionString("Default"));
        await using SqlCommand cmd = new SqlCommand(procedureName, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@IdProduct", product.IdProduct);
        cmd.Parameters.AddWithValue("@IdWarehouse", product.IdWarehouse);
        cmd.Parameters.AddWithValue("@Amount", product.Amount);
        cmd.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);

        await conn.OpenAsync();
        int result = (int)await cmd.ExecuteScalarAsync();
        return result;
    }
}
