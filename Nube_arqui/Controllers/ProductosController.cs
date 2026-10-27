using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NpgsqlTypes;
using Nube_arqui.Models;

namespace Nube_arqui.Controllers;

[ApiController]
[Route("api/productos")]
public class ProductosController(NpgsqlDataSource db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Producto>>> Listar(CancellationToken ct)
    {
        await using var cmd = db.CreateCommand("SELECT id, nombre, descripcion, precio, stock FROM productos ORDER BY id");
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var items = new List<Producto>();
        while (await reader.ReadAsync(ct)) items.Add(Leer(reader));
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Producto>> Obtener(int id, CancellationToken ct)
    {
        await using var cmd = db.CreateCommand("SELECT id, nombre, descripcion, precio, stock FROM productos WHERE id = @id");
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return NotFound();
        return Ok(Leer(reader));
    }

    [HttpPost]
    public async Task<ActionResult<Producto>> Crear(ProductoRequest request, CancellationToken ct)
    {
        await using var cmd = db.CreateCommand("INSERT INTO productos (nombre, descripcion, precio, stock) VALUES (@nombre, @descripcion, @precio, @stock) RETURNING id, nombre, descripcion, precio, stock");
        Parametros(cmd, request);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        var producto = Leer(reader);
        return CreatedAtAction(nameof(Obtener), new { id = producto.Id }, producto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Producto>> Actualizar(int id, ProductoRequest request, CancellationToken ct)
    {
        await using var cmd = db.CreateCommand("UPDATE productos SET nombre = @nombre, descripcion = @descripcion, precio = @precio, stock = @stock WHERE id = @id RETURNING id, nombre, descripcion, precio, stock");
        cmd.Parameters.AddWithValue("id", id);
        Parametros(cmd, request);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return NotFound();
        return Ok(Leer(reader));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await using var cmd = db.CreateCommand("DELETE FROM productos WHERE id = @id");
        cmd.Parameters.AddWithValue("id", id);
        return await cmd.ExecuteNonQueryAsync(ct) == 0 ? NotFound() : NoContent();
    }

    private static Producto Leer(NpgsqlDataReader r) => new(r.GetInt32(0), r.GetString(1), r.IsDBNull(2) ? null : r.GetString(2), r.GetDecimal(3), r.GetInt32(4));

    private static void Parametros(NpgsqlCommand cmd, ProductoRequest r)
    {
        cmd.Parameters.AddWithValue("nombre", r.Nombre);
        cmd.Parameters.AddWithValue("descripcion", NpgsqlDbType.Varchar, (object?)r.Descripcion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("precio", r.Precio!.Value);
        cmd.Parameters.AddWithValue("stock", r.Stock!.Value);
    }
}
