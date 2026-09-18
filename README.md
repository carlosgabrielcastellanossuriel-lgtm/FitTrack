# FitTrack

                  *********** Diagrama de flujo ***********

```mermaid
flowchart TD
    subgraph Usuario_Core ["1. Onboarding (Usuario)"]
        A[Registro de Usuario] --> B[Cálculo de Metas: Calorías y Macros]
    end

    subgraph Catálogo_Alimentos ["2. Gestión de Alimentos (Alimento)"]
        C{¿Alimento existe en Catálogo?}
        C -->|Sí| D[Seleccionar Alimento Básico]
        C -->|No| E[Crear Alimento Personalizado]
        E -->|Ingresar Calorías y Macros| F[Guardar en Catálogo]
        F --> D
    end

    subgraph Consumo ["3. Registro de Comida (RegistroComida)"]
        D --> G[Crear RegistroComida]
        G --> H[Calcular Calorías Totales de la Porción]
    end

    subgraph Resumen_Diario ["4. Resumen Diario & Máquina de Estados (ResumenDiario)"]
        H --> I{¿Existe ResumenDiario de hoy?}
        I -->|No| J[Crear ResumenDiario]
        I -->|Sí| K[Acumular a ResumenDiario Existente]
        J --> K
        
        K --> L[Estado: En Progreso]
        L --> M{Evaluación: ¿Excede Meta?}
        M -->|No| N[Estado: Dentro De Meta]
        M -->|Sí| O[Estado: Excedido]
        O -->|Dispara| P[Notificación de Alerta]
        
        N --> Q[Fin de Día: Estado Cerrado]
        O --> Q
    end

    subgraph Vistas ["5. Opciones del Usuario"]
        R[Ver Dashboard Día Actual]
        S[Consultar Resumen Histórico por ID/Fecha]
        N -.-> R
        O -.-> R
        Q -.-> S
    end
```


                  *********** Diagrama Componentes ***********

```mermaid
flowchart TB
    subgraph WebAPI ["Contenedor: Web API (.NET)"]
        
        subgraph Core ["Capa Core (Fundación Transversal)"]
            CA["1. Control de acceso"]
            GP["2. Gestión de permisos"]
            MD["3. Manejador de documentos"]
            N["4. Notificaciones"]
            RA["5. Reportes con agregación"]
            AUD["6. Auditoría"]
        end

        subgraph ModuloNegocio ["Capa de Negocio"]
            GN["7. Gestor Nutricional\n(FitTrack)"]
        end

    end

    DB[("Base de Datos Persistente\n(SQL / EF Core)")]

    %% Relaciones del Módulo hacia el Core (Dependencia en un solo sentido)
    GN -->|"Consulta usuario activo mediante IControlAcceso"| CA
    GN -->|"Dispara alertas calóricas mediante INotificaciones"| N
    GN -->|"Registra cierre de día y cambios mediante IAuditoria"| AUD

    %% Persistencia (Base de Datos fuera del contenedor)
    CA -->|"Guarda usuarios y hashes de clave"| DB
    GP -->|"Guarda solicitudes de permisos"| DB
    MD -->|"Guarda metadatos de archivos"| DB
    N -->|"Guarda historial de alertas"| DB
    RA -->|"Lee métricas agrupadas"| DB
    AUD -->|"Guarda trazas de auditoría inmutables"| DB
    GN -->|"Guarda alimentos, registros y resúmenes diarios"| DB

    end
```
