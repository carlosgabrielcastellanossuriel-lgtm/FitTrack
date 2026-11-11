using System.Globalization;
using System.Net;
using System.Text.Json;
using FitTrack.Core.Usuarios;

namespace FitTrack.Negocio.Alimentos;

// Consulta la API de USDA FoodData Central y guarda el alimento encontrado como uno personalizado.
public class ServicioUsda
{
    private readonly HttpClient _http;
    private readonly ServicioAlimento _alimentos;

    // La clave se lee de una variable de entorno para no escribirla nunca en el código.
    public string? Clave { get; set; } = Environment.GetEnvironmentVariable("FITTRACK_USDA_CLAVE");

    public ServicioUsda(HttpClient http, ServicioAlimento alimentos)
    {
        _http = http;
        _alimentos = alimentos;

        // Toda consulta a USDA debe fallar rápido si el servicio no responde.
        _http.Timeout = TimeSpan.FromSeconds(10);
    }

    // Busca el alimento por nombre en USDA y lo crea a nombre del usuario.
    public async Task<Resultado> ImportarAsync(Usuario usuario, string nombre)
    {
        // 1. Sin nombre no hay nada que buscar.
        if (string.IsNullOrWhiteSpace(nombre))
            return Resultado.Error("Escribe el nombre del alimento a buscar.");

        // 2. Sin clave configurada tampoco se puede consultar la API.
        if (string.IsNullOrWhiteSpace(Clave))
            return Resultado.Error("La integración con USDA no está configurada: falta la clave FITTRACK_USDA_CLAVE.");

        // La clave va en el encabezado, NO en la URL, para que no termine escrita en los logs.
        string url = "https://api.nal.usda.gov/fdc/v1/foods/search?query="
            + Uri.EscapeDataString(nombre)
            + "&dataType=SR%20Legacy&pageSize=1";

        try
        {
            // 3. Se hace la petición con la clave en X-Api-Key.
            using HttpRequestMessage solicitud = new HttpRequestMessage(HttpMethod.Get, url);
            solicitud.Headers.Add("X-Api-Key", Clave);

            using HttpResponseMessage respuesta = await _http.SendAsync(solicitud);

            // 5. Se distingue la clave inválida y el límite de consultas de los demás errores.
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized || respuesta.StatusCode == HttpStatusCode.Forbidden)
                return Resultado.Error("La clave de USDA no es válida.");

            if (respuesta.StatusCode == HttpStatusCode.TooManyRequests)
                return Resultado.Error("Se alcanzó el límite de consultas de USDA. Intenta más tarde.");

            if (!respuesta.IsSuccessStatusCode)
                return Resultado.Error("No se pudo consultar USDA. Intenta de nuevo más tarde.");

            // 6. Se lee el JSON tal cual, sin clases extra para la respuesta.
            string json = await respuesta.Content.ReadAsStringAsync();
            using JsonDocument documento = JsonDocument.Parse(json);

            JsonElement foods = documento.RootElement.GetProperty("foods");
            if (foods.GetArrayLength() == 0)
                return Resultado.Error("No se encontró ese alimento en USDA.");

            JsonElement alimento = foods[0];
            string descripcion = alimento.GetProperty("description").GetString() ?? nombre;

            // 7. Se buscan los valores por 100 g dentro de "foodNutrients".
            double? calorias = null;
            double proteina = 0;
            double carbohidratos = 0;
            double grasas = 0;

            foreach (JsonElement nutriente in alimento.GetProperty("foodNutrients").EnumerateArray())
            {
                if (!nutriente.TryGetProperty("nutrientNumber", out JsonElement numero))
                    continue;

                // nutrientNumber puede venir como texto o como número.
                string codigo = numero.ToString();
                if (!nutriente.TryGetProperty("value", out JsonElement valor))
                    continue;

                double? cantidad = LeerNumero(valor);
                if (cantidad == null)
                    continue;

                if (codigo == "208")
                    calorias = cantidad;
                else if (codigo == "203")
                    proteina = cantidad.Value;
                else if (codigo == "205")
                    carbohidratos = cantidad.Value;
                else if (codigo == "204")
                    grasas = cantidad.Value;
            }

            // Sin calorías no sirve para el plan, así que se rechaza.
            if (calorias == null)
                return Resultado.Error("USDA no trae calorías para ese alimento.");

            // 8. Se guarda como alimento personalizado del usuario.
            return await _alimentos.CrearAsync(descripcion, calorias.Value, proteina, carbohidratos, grasas, usuario.Id);
        }
        catch (HttpRequestException)
        {
            return Resultado.Error("No se pudo consultar USDA. Intenta de nuevo más tarde.");
        }
        catch (TaskCanceledException)
        {
            // Aquí también cae el timeout de 10 segundos.
            return Resultado.Error("No se pudo consultar USDA. Intenta de nuevo más tarde.");
        }
        catch (JsonException)
        {
            return Resultado.Error("No se pudo consultar USDA. Intenta de nuevo más tarde.");
        }
    }

    // Acepta el valor como número JSON o como texto ("12.5") y devuelve null si no se puede leer.
    private static double? LeerNumero(JsonElement valor)
    {
        if (valor.ValueKind == JsonValueKind.Number)
            return valor.GetDouble();

        if (valor.ValueKind == JsonValueKind.String)
        {
            // Con cultura invariante para que el punto decimal no dependa del idioma del servidor.
            if (double.TryParse(valor.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double numero))
                return numero;
        }

        return null;
    }
}
