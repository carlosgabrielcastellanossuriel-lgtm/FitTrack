\# Máquina de estados: ResumenDiario



Cada día, un usuario tiene un `ResumenDiario` que acumula lo consumido y tiene un estado.



\## Dónde está en el código



\- Estados: `FitTrack.Negocio/Resumenes/EstadoResumen.cs`

\- Transiciones: `FitTrack.Negocio/Resumenes/TransicionesResumen.cs`

\- Entidad: `FitTrack.Negocio/Resumenes/ResumenDiario.cs`. El único método que cambia el estado es `CambiarEstado`.



\## Estados



| Estado | Significado |

|---|---|

| EnProgreso | Estado inicial. El día empezó y se registran comidas. |

| DentroDeMeta | Ningún total (calorías, proteínas, carbohidratos, grasas) supera su meta. |

| Excedido | Al menos un total superó su meta. |

| Cerrado | El día terminó. \*\*Estado terminal.\*\* |



\## Transiciones permitidas



| Desde | Hacia | Quién la ejecuta | Condición |

|---|---|---|---|

| EnProgreso | DentroDeMeta | Sistema | Se registra una comida y ningún total supera su meta. |

| EnProgreso | Excedido | Sistema | Se registra una comida y algún total supera su meta. |

| DentroDeMeta | Excedido | Sistema | Un nuevo registro hace que algún total supere su meta. |

| DentroDeMeta | Cerrado | Sistema | Termina el día. |

| Excedido | Cerrado | Sistema | Termina el día. |



\## Transiciones prohibidas (explícitas)



| Desde | Hacia | Motivo |

|---|---|---|

| Cerrado | EnProgreso | Un día cerrado no se reabre. |

| Excedido | DentroDeMeta | Un día excedido no vuelve atrás. |



Toda transición que no esté en la tabla de permitidas también se rechaza.



\## Estado terminal



`Cerrado`: no tiene transiciones de salida.

