## FitTrack

**FitTrack** es un gestor de calorías y macronutrientes diseñado para brindar un control claro y preciso sobre la alimentación diaria. El sistema evalúa y calcula metas calóricas y de macronutrientes (proteínas, carbohidratos y grasas) personalizadas a partir de los datos del perfil del usuario (peso, altura, edad y objetivo nutricional: déficit, superávit o mantenimiento).

A través de un catálogo interactivo de alimentos —que integra opciones predeterminadas e ítems personalizados—, FitTrack permite registrar las porciones consumidas durante el día. El sistema consolida los datos en tiempo real, alerta mediante notificaciones cuando se alcanza o supera el tope diario, y ofrece un panel de control para consultar el consumo actual y el historial.

FitTrack es justamente lo que necesitas!!

---

## Entidades Principales

* **Usuario:** Representa el perfil, almacenando datos antropométricos y las metas calculadas de calorías y macronutrientes.

* **Alimento:** Catálogo de insumos nutricionales (básicos y personalizados) con sus valores de calorías, proteínas, carbohidratos y grasas.

* **RegistroComida:** Asocia un alimento y su porción consumida al día en curso, calculando los valores nutricionales aportados.

* **ResumenDiario:** Acumula el consumo diario del usuario y gestiona el estado del día (*En Progreso, Dentro de Meta, Excedido, Cerrado*).

---

## Tecnologías Utilizadas

* **Lenguaje:** C#

* **Framework:** .NET | ASP.NET Core

* **Acceso a Datos (ORM):** Entity Framework Core

* **Base de Datos:** Microsoft SQL Server

## Prerrequisitos

* .NET SDK

* Microsoft SQL Server

### Pasos de Instalación
```bash

git clone [https://github.com/carlosgabrielcastellanossuriel-lgtm/FitTrack.git](https://github.com/carlosgabrielcastellanossuriel-lgtm/FitTrack.git)

cd FitTrack

```

2. **Restaurar las dependencias del proyecto:**

```bash

dotnet restore

```

3. **Configurar la base de datos:**

Abre el archivo appsettings.json y actualiza la cadena de conexión con los datos de tu servidor SQL:

```json

"ConnectionStrings": {

"DefaultConnection": "Server=TU_SERVIDOR;Database=FitTrackDB;Trusted_Connection=True;TrustServerCertificate=True;"

}

```

4. **Aplicar las migraciones para crear la base de datos:**

```bash

dotnet ef database update

```

5. **Ejecutar la API:**

```bash

dotnet run

```
## Diagrama de flujo
```mermaid
flowchart TD
    subgraph Usuario_Core ["1. Onboarding (Usuario)"]
        A[Registro de Usuario] --> B[Cálculo de Metas: Calorías, Proteínas, Carbohidratos y Grasas]
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
        G --> H[Calcular Calorías y Macros Totales de la Porción]
    end

    subgraph Resumen_Diario ["4. Resumen Diario & Máquina de Estados (ResumenDiario)"]
        H --> I{¿Existe ResumenDiario de hoy?}
        I -->|No| J[Crear ResumenDiario]
        I -->|Sí| K[Acumular a ResumenDiario Existente]
        J --> K
        
        K --> L[Estado: En Progreso]
        L --> M{Evaluación: ¿Excede Meta de Calorías, Proteínas, Carbohidratos o Grasas?}
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

**Diagrama Componentes**
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
    GN -->|"Solicita dashboard y resumen histórico mediante IReportes"| RA

    %% Persistencia (Base de Datos fuera del contenedor)
    CA -->|"Guarda usuarios y hashes de clave"| DB
    GP -->|"Guarda solicitudes de permisos"| DB
    MD -->|"Guarda metadatos de archivos"| DB
    N -->|"Guarda historial de alertas"| DB
    RA -->|"Lee métricas agrupadas"| DB
    AUD -->|"Guarda trazas de auditoría inmutables"| DB
    GN -->|"Guarda alimentos, registros y resúmenes diarios"| DB
```

**Diagrama Entidad-Relación**
```mermaid
erDiagram
    Usuario {
        int Id PK
        varchar Nombre
        int Edad
        float Peso
        float Altura
        varchar MetaNutricional
        int CaloriasMeta
        float ProteinaMeta
        float CarbohidratosMeta
        float GrasasMeta
    }

    Alimento {
        int Id PK
        varchar Nombre
        float CantidadCalorias "Base 100g"
        float CantidadProteina "Base 100g"
        float CantidadCarbohidratos "Base 100g"
        float CantidadGrasas "Base 100g"
        boolean EsBasico
        int UsuarioCreadorId FK "Nulo si es basico"
    }

    ResumenDiario {
        int Id PK
        int UsuarioId FK
        date Fecha
        float CaloriasAcumuladas
        float ProteinasAcumuladas
        float CarbohidratosAcumulados
        float GrasasAcumuladas
        varchar EstadoActual
    }

    RegistroComida {
        int Id PK
        int ResumenDiarioId FK
        int AlimentoId FK
        float CantidadConsumida "Gramos"
        float CaloriasTotales
        float CantidadProteina
        float CantidadCarbohidratos
        float CantidadGrasas
    }

    %% Relaciones
    Usuario |o--o{ Alimento : "crea (alimentos personalizados)"
    Usuario ||--o{ ResumenDiario : "tiene"
    ResumenDiario ||--o{ RegistroComida : "contiene"
    Alimento ||--o{ RegistroComida : "se registra en"
```
