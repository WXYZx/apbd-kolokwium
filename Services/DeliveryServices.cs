using System.Data;
using Kolokwium1.Models;
using Microsoft.Data.SqlClient;

namespace Kolokwium1.Services;

public class DeliveryServices : IDeliveryServices
{
    private readonly string _connectionString =
        "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=kolokwium;Integrated Security=True;";

    async Task<DeliveryDTO> IDeliveryServices.getDelivery(string visitId)
    {
        DeliveryDTO? result = null;

        string command = @"SELECT *, Driver.firstName as driver_First_Name, Driver.lastName as driver_Last_name FROM Driver
                  JOIN Customers ON Driver.id_customer = Customers.id_customer
                  JOIN Drivers ON Drivers.id_driver = Drivers.id_driver
                  JOIN Products ON Drivers.id_product = Products.id_product
                  JOIN Product_Delivery ON Delivery.id_delivery = Products.id_delivery
                  WHERE Driver.visitId = @visitId";


        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();
            cmd.Parameters.AddWithValue("@id", int.Parse(visitId));

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    DateTime date = DateTime.Parse(reader["date"].ToString());
                    Customer customer = new Customer()
                    {
                        FirstName = reader["first_name"].ToString(),
                        LastName = reader["last_name"].ToString(),
                        DateOfBirth = DateTime.Parse(reader["date_of_birth"].ToString()),
                    };

                    Driver driver = new Driver()
                    {
                        FirstName = reader["first_name"].ToString(),
                        LastName = reader["last_name"].ToString(),
                        LicenceNumber = reader["licence_number"].ToString(),
                    };

                    Product product = new Product()
                    {
                        Name = reader["name"].ToString(),
                        Price = Convert.ToDouble((double)reader["price"]),
                        Amount = (int)reader["amount"],
                    };

                    if (result == null)
                    {
                        result = new DeliveryDTO()
                        {
                            Date = date,
                            Customer = customer,
                            Driver = driver,
                            Products = new List<Product>()
                        };

                        result.Products.Add(Product);
                    }
                    else
                    {
                        result.Products.Add(Product);
                    }

                }
            }
        }
        return result;
    }



    async Task IDeliveryServices.addNewDelivery(postDeliveryDTO newDeliveryDto)
    {


        String command = "";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync(); 
            cmd.CommandText = @"SELECT COUNT(*) FROM Delivery WHERE Delivery.delivery_id = @id";
            cmd.Parameters.AddWithValue("@id", newDeliveryDto.DeliveryId);
            
            var a = (int)await cmd.ExecuteScalarAsync();
            
            if (a > 0) 
            { 
                throw new ConflictException("delivery o podanym ID już istnieje");
            }

            cmd.Parameters.Clear();

            cmd.CommandText = @"SELECT COUNT(*) FROM Delivery WHERE Delivery.delivery_id = @id";
            cmd.Parameters.AddWithValue("@id", newDeliveryDto.CustomerId);
            
            a = (int)await cmd.ExecuteScalarAsync();
            if (a < 1)
            {
                throw new NotFoundException("delivery o podanym ID nie istnieje");
            }

            cmd.Parameters.Clear();
            cmd.CommandText = @"SELECT COUNT(*) FROM Driver WHERE driver_id (SELECT driver_id FROM Driver WHERE driver.visitId = @visitId)";);
            cmd.Parameters.AddWithValue("@licence_number", newDeliveryDto.LicenceNumber);
            
            if (!((int)await cmd.ExecuteScalarAsync() > 0))
            {
                throw new NotFoundException("driver o podanym ID nie istnieje");
            }

            cmd.Parameters.Clear();


            
            await cmd.ExecuteNonQueryAsync();
            foreach (var s in newDeliveryDto.Products)
            {
                cmd.Parameters.Clear();
                cmd.CommandText = @"SELECT COUNT(*) FROM Product WHERE name = @name";
                cmd.Parameters.Add("@name", s.Name);
            
                if (!((int)await cmd.ExecuteScalarAsync() > 0))
                {
                    throw new NotFoundException("product o takiej nazwie nie istnieje");
                }
                cmd.Parameters.Clear();
                cmd.CommandText = @"INSERT INTO ProductDelivery (delivery_id, product_id, amount) VALUES (@deliveryId, (SELECT product_id FROM Product WHERE name = @name), @amount)";
                cmd.Parameters.AddWithValue("@delivery_id", newDeliveryDto.DeliveryId);
                cmd.Parameters.AddWithValue("@name", s.Name);
                cmd.Parameters.AddWithValue("@amount", s.Amount);
                
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}