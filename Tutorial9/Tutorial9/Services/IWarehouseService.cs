using Tutorial9.Model;

namespace Tutorial9.Services;

public interface IWarehouseService
{
    Task<int> AddProductToWarehouse(AddProductWarehouse product);
    Task<int> AddProductToWarehouseProcedure(AddProductWarehouse product);
}
