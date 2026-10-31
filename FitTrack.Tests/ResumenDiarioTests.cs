using FitTrack.Negocio.Perfiles;
using FitTrack.Negocio.Resumenes;
using Xunit;

namespace FitTrack.Tests;

public class ResumenDiarioTests
{
    [Fact]
    public void Transicion_EnProgreso_A_DentroDeMeta_SePermite()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.EnProgreso;

        // Actuar
        bool cambio = resumen.CambiarEstado(EstadoResumen.DentroDeMeta);

        // Verificar
        Assert.True(cambio);
        Assert.Equal(EstadoResumen.DentroDeMeta, resumen.EstadoActual);
    }

    [Fact]
    public void Transicion_EnProgreso_A_Excedido_SePermite()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.EnProgreso;

        // Actuar
        bool cambio = resumen.CambiarEstado(EstadoResumen.Excedido);

        // Verificar
        Assert.True(cambio);
        Assert.Equal(EstadoResumen.Excedido, resumen.EstadoActual);
    }

    [Fact]
    public void Transicion_DentroDeMeta_A_Excedido_SePermite()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.DentroDeMeta;

        // Actuar
        bool cambio = resumen.CambiarEstado(EstadoResumen.Excedido);

        // Verificar
        Assert.True(cambio);
        Assert.Equal(EstadoResumen.Excedido, resumen.EstadoActual);
    }

    [Fact]
    public void Transicion_DentroDeMeta_A_Cerrado_SePermite()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.DentroDeMeta;

        // Actuar
        bool cambio = resumen.CambiarEstado(EstadoResumen.Cerrado);

        // Verificar
        Assert.True(cambio);
        Assert.Equal(EstadoResumen.Cerrado, resumen.EstadoActual);
    }

    [Fact]
    public void Transicion_Excedido_A_Cerrado_SePermite()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.Excedido;

        // Actuar
        bool cambio = resumen.CambiarEstado(EstadoResumen.Cerrado);

        // Verificar
        Assert.True(cambio);
        Assert.Equal(EstadoResumen.Cerrado, resumen.EstadoActual);
    }

    [Fact]
    public void Transicion_Cerrado_A_EnProgreso_SeRechazaYElEstadoNoCambia()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.Cerrado;

        // Actuar
        bool cambio = resumen.CambiarEstado(EstadoResumen.EnProgreso);

        // Verificar
        Assert.False(cambio);
        Assert.Equal(EstadoResumen.Cerrado, resumen.EstadoActual);
    }

    [Fact]
    public void Transicion_Excedido_A_DentroDeMeta_SeRechazaYElEstadoNoCambia()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.Excedido;

        // Actuar
        bool cambio = resumen.CambiarEstado(EstadoResumen.DentroDeMeta);

        // Verificar
        Assert.False(cambio);
        Assert.Equal(EstadoResumen.Excedido, resumen.EstadoActual);
    }

    [Fact]
    public void EstadoTerminal_Cerrado_NoTieneSalida()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.Cerrado;

        // Actuar
        // Se intenta cada estado posible como destino.
        foreach (EstadoResumen destino in Enum.GetValues<EstadoResumen>())
        {
            // Verificar: ninguna salida se permite y el estado no cambia.
            Assert.False(resumen.CambiarEstado(destino));
            Assert.Equal(EstadoResumen.Cerrado, resumen.EstadoActual);
        }
    }

    [Fact]
    public void RegistrarComida_ConTotalSobreLaMeta_QuedaExcedido()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.EnProgreso;

        PerfilUsuario perfil = new PerfilUsuario();
        perfil.CaloriasMeta = 2000;
        perfil.ProteinaMeta = 150;
        perfil.CarbohidratosMeta = 200;
        perfil.GrasasMeta = 70;

        // Actuar
        bool registrada = resumen.RegistrarComida(2500, 100, 150, 50, perfil);

        // Verificar
        Assert.True(registrada);
        Assert.Equal(EstadoResumen.Excedido, resumen.EstadoActual);
    }

    [Fact]
    public void RegistrarComida_DentroDeLaMeta_QuedaDentroDeMeta()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.EnProgreso;

        PerfilUsuario perfil = new PerfilUsuario();
        perfil.CaloriasMeta = 2000;
        perfil.ProteinaMeta = 150;
        perfil.CarbohidratosMeta = 200;
        perfil.GrasasMeta = 70;

        // Actuar
        bool registrada = resumen.RegistrarComida(500, 30, 50, 20, perfil);

        // Verificar
        Assert.True(registrada);
        Assert.Equal(EstadoResumen.DentroDeMeta, resumen.EstadoActual);
    }
}
