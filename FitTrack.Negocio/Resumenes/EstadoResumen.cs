namespace FitTrack.Negocio.Resumenes;

// ÚNICO lugar donde se declaran los estados de un ResumenDiario (RF-NEG-03).
// Si algún día cambia un estado, se cambia solo aquí.
public enum EstadoResumen
{
    EnProgreso = 0,     // el día empezó y se están registrando comidas
    DentroDeMeta = 1,   // los totales no superan ninguna meta
    Excedido = 2,       // algún total superó su meta
    Cerrado = 3         // el día terminó (estado terminal)
}