# Tlax2026MVP - Unity Driving Simulator

**Version:** 1.0.0
**Unity Version:** 2022.3 LTS
**Ultima actualizacion:** 2026-02-02

---

## Descripcion

Simulador de examen de manejo para evaluacion de conductores en Mexico (Tlaxcala 2026). Sistema de evaluacion basado en telemetria en tiempo real con deteccion automatica de infracciones.

---

## Requisitos

### Software
- Unity 2022.3 LTS o superior
- TextMesh Pro (incluido)

### Assets de Terceros (ya incluidos)
| Asset | Uso |
|-------|-----|
| Realistic Car Controller Pro (RCCP) | Fisica de vehiculos, controles, camara |
| Gley Urban Traffic System | Trafico AI, semaforos |
| Gley Pedestrian System | Peatones AI, cruces peatonales |

---

## Estructura del Proyecto

```
Tlax2026MVP/
└── Assets/
    ├── Custom/                         # Scripts personalizados
    │   ├── TelemetryLogger.cs          # Logger de eventos a JSON
    │   ├── ViolationDetector.cs        # Deteccion de infracciones
    │   ├── NotificationManager.cs      # Popups de notificacion
    │   └── SimpleSpeedometer.cs        # HUD de velocidad
    │
    ├── Realistic Car Controller Pro/   # RCCP (asset comprado)
    ├── Gley/                           # Traffic & Pedestrian (asset comprado)
    ├── Scenes/                         # Escenas del proyecto
    └── TextMesh Pro/                   # UI de texto
```

---

## Scripts Implementados

### 1. TelemetryLogger.cs

Logger de telemetria que registra eventos durante la sesion y exporta a JSON.

**Funcionalidades:**
- Singleton pattern (`TelemetryLogger.Instance`)
- Registro de eventos con timestamp, tipo, descripcion, puntos y velocidad
- Export a JSON en `Application.persistentDataPath`

**Uso:**
```csharp
// Registrar un evento
TelemetryLogger.Instance.LogEvent(
    "VELOCIDAD",                          // Tipo de evento
    "Exceso de velocidad (limite: 45)",   // Descripcion
    -5,                                   // Puntos (negativo = penalizacion)
    67.5f                                 // Velocidad actual
);

// Exportar al final de la sesion
TelemetryLogger.Instance.ExportToJSON(finalScore);
```

**Output JSON:**
```json
{
    "sessionStart": "2026-02-02 10:30:00",
    "sessionEnd": "2026-02-02 10:45:00",
    "finalScore": 85,
    "events": [
        {
            "timestamp": "45.23s",
            "eventType": "VELOCIDAD",
            "description": "Exceso de velocidad",
            "points": -5,
            "speed": 67.5
        }
    ]
}
```

---

### 2. ViolationDetector.cs

Detector de infracciones de transito en tiempo real.

**Funcionalidades:**
- Deteccion de exceso de velocidad
- Deteccion de colisiones con vehiculos
- Deteccion de atropellos (tag "Pedestrian")
- Sistema de puntuacion configurable
- Integracion con TelemetryLogger y NotificationManager

**Configuracion (Inspector):**
| Propiedad | Default | Descripcion |
|-----------|---------|-------------|
| `speedLimit` | 45 | Limite de velocidad en km/h |
| `speedingPenalty` | 5 | Puntos por exceso de velocidad |
| `collisionPenalty` | 10 | Puntos por colision |
| `pedestrianPenalty` | 25 | Puntos por atropello |
| `totalScore` | 100 | Puntaje inicial |

**Setup:**
1. Agregar componente al vehiculo del jugador
2. El vehiculo debe tener un `Rigidbody` adjunto
3. Peatones deben tener el tag "Pedestrian"

**Uso:**
```csharp
// Obtener puntaje actual
int score = violationDetector.totalScore;

// Exportar telemetria manualmente
violationDetector.ExportTelemetry();
```

---

### 3. NotificationManager.cs

Sistema de notificaciones en pantalla.

**Funcionalidades:**
- Singleton pattern (`NotificationManager.Instance`)
- Muestra mensajes con color personalizado
- Auto-oculta despues de `displayDuration` segundos

**Setup:**
1. Crear GameObject vacio en la escena
2. Agregar componente NotificationManager
3. Asignar un TextMeshProUGUI al campo `notificationText`
4. El texto se oculta automaticamente al iniciar

**Uso:**
```csharp
NotificationManager.Instance.ShowNotification(
    "-5 EXCESO DE VELOCIDAD!",
    Color.yellow
);

NotificationManager.Instance.ShowNotification(
    "-25 ATROPELLO!",
    Color.red
);
```

---

### 4. SimpleSpeedometer.cs

Velocimetro simple para HUD.

**Funcionalidades:**
- Busca automaticamente GameObject "Player"
- Lee velocidad del Rigidbody
- Convierte m/s a km/h
- Actualiza TextMeshProUGUI cada frame

**Setup:**
1. Crear UI Text (TextMeshPro) en Canvas
2. Agregar componente SimpleSpeedometer
3. Asignar el TextMeshProUGUI al campo `speedText`
4. El vehiculo del jugador debe llamarse "Player"

---

## Tipos de Infracciones

### Criticas (Reprobacion inmediata)
| Infraccion | Descripcion |
|------------|-------------|
| Semaforo en rojo | Cruzar semaforo en rojo |
| Atropello | Colision con peaton |
| Exceso grave (+20 km/h) | Velocidad 20+ km/h sobre limite |
| Sentido contrario | Conducir en sentido contrario |
| Sin cinturon | Conducir sin cinturon de seguridad |
| Sin casco | Solo motocicleta |

### Mayores (-10 a -15 puntos)
| Infraccion | Descripcion |
|------------|-------------|
| Exceso de velocidad | 1-20 km/h sobre limite |
| Sin direccional | No usar direccional al dar vuelta |
| Alto total | No hacer alto total |
| Ceder paso | No ceder paso a peatones |

### Menores (-2 a -5 puntos)
| Infraccion | Descripcion |
|------------|-------------|
| Frenado brusco | Desaceleracion excesiva |
| Aceleracion agresiva | Aceleracion excesiva |
| Desviacion de carril | Salir del carril |
| Luces apagadas | De noche sin luces |

---

## Sistema de Puntuacion

| Rango | Resultado | Descripcion |
|-------|-----------|-------------|
| 90-100 | APTO | Licencia inmediata |
| 80-89 | APTO CONDICIONADO | Reforzar areas de oportunidad |
| 70-79 | APTO CONDICIONADO | Reentrenamiento obligatorio |
| <70 o critica | NO APTO | Licencia negada |

### Categorias de Evaluacion (100 puntos total)

| Categoria | Puntos Max | Descripcion |
|-----------|------------|-------------|
| Cumplimiento Normativo | 40 | Semaforos, limites, senales |
| Seguridad Personal | 20 | Cinturon, casco, equipo |
| Conduccion Preventiva | 20 | Frenado, aceleracion, distancia |
| Control del Vehiculo | 10 | Volante, cambios, estabilidad |
| Distractores | 10 | Atencion, concentracion |

---

## Zonas de Velocidad

| Zona | Limite | Restricciones |
|------|--------|---------------|
| Escolar | 20 km/h | Prioridad peatones x2 |
| Residencial | 30 km/h | - |
| Hospitalaria | 30 km/h | Sin claxon |
| Urbana | 40 km/h | - |
| Comercial | 40 km/h | - |
| Industrial | 40 km/h | - |
| Carretera | 80 km/h | - |
| Autopista | 110 km/h | - |
| Cruce ferroviario | 0 | Alto total obligatorio |

---

## Integracion con RCCP

El sistema usa las siguientes propiedades de RCCP:

```csharp
RCCP_CarController vehicle;

// Telemetria basica
float speed = vehicle.speed;                    // km/h
float rpm = vehicle.engineRPM;
int gear = vehicle.currentGear;

// Inputs
float throttle = vehicle.throttleInput;         // 0-1
float brake = vehicle.brakeInput;               // 0-1
float steer = vehicle.steerInput;               // -1 a 1

// Luces y senales
bool headlights = vehicle.lowBeamHeadLightsOn;
bool highBeams = vehicle.highBeamHeadLightsOn;
bool leftSignal = vehicle.indicatorsLeftInput;
bool rightSignal = vehicle.indicatorsRightInput;
```

---

## Integracion con Gley Traffic

```csharp
// Obtener estado de semaforo
int state = TrafficManager.Instance.GetTrafficLightState(position);
// 0 = Rojo, 1 = Amarillo, 2 = Verde

// Verificar vehiculo de emergencia cercano
bool emergency = TrafficManager.Instance.IsEmergencyVehicleNearby(pos, radius);

// Ajustar densidad de trafico
TrafficManager.Instance.SetDensity(50); // 0-100
```

---

## Como Agregar Nuevas Infracciones

1. Agregar tipo al enum en ViolationDetector (o crear nuevo):
```csharp
public enum ViolationType {
    Speeding,
    RedLight,
    PedestrianCollision,
    NewViolationType  // Agregar aqui
}
```

2. Crear metodo de deteccion:
```csharp
void CheckNewViolation() {
    if (/* condicion de infraccion */) {
        totalScore -= newViolationPenalty;
        NotificationManager.Instance?.ShowNotification(
            $"-{newViolationPenalty} NUEVA INFRACCION!",
            Color.yellow);
        TelemetryLogger.Instance?.LogEvent(
            "NUEVA",
            "Descripcion de la infraccion",
            -newViolationPenalty,
            speed);
    }
}
```

3. Llamar en Update():
```csharp
void Update() {
    // ... existing checks
    CheckNewViolation();
}
```

---

## Tags Necesarios

Configurar los siguientes tags en Unity:

| Tag | Uso |
|-----|-----|
| `Player` | Vehiculo del jugador |
| `Pedestrian` | Peatones (NPC y Gley) |
| `Vehicle` | Vehiculos de trafico |
| `TrafficLight` | Semaforos |

---

## Layers Recomendados

| Layer | Numero | Uso |
|-------|--------|-----|
| Player | 8 | Vehiculo del jugador |
| Traffic | 9 | Vehiculos AI |
| Pedestrian | 10 | Peatones AI |
| ZoneTrigger | 11 | Zonas de velocidad |

---

## Archivos de Telemetria

Los archivos JSON se guardan en:

| Plataforma | Ruta |
|------------|------|
| Windows | `%USERPROFILE%\AppData\LocalLow\<Company>\Tlax2026MVP\` |
| macOS | `~/Library/Application Support/<Company>/Tlax2026MVP/` |
| Linux | `~/.config/unity3d/<Company>/Tlax2026MVP/` |

---

## Pendientes (TODO)

### Fase 1 - MVP Core
- [x] TelemetryLogger basico
- [x] ViolationDetector (velocidad, colisiones)
- [x] NotificationManager
- [x] SimpleSpeedometer
- [ ] Integracion completa con RCCP
- [ ] ZoneManager con triggers

### Fase 2 - Trafico
- [ ] Integracion con Gley Traffic
- [ ] Deteccion de semaforo en rojo
- [ ] Deteccion de senal de alto
- [ ] Vehiculos de emergencia

### Fase 3 - Evaluacion Completa
- [ ] Sistema de categorias de puntuacion
- [ ] Pre-checklist (cinturon, espejos)
- [ ] Pantalla de resultados
- [ ] Export PDF de resultados

### Fase 4 - Escenarios
- [ ] ScenarioConfig ScriptableObjects
- [ ] Sistema de checkpoints
- [ ] Clima y hora del dia
- [ ] Multiples rutas de examen

### Fase 5 - Tipos de Licencia
- [ ] Tipo C - Automovil (actual)
- [ ] Tipo D - Motocicleta
- [ ] Tipo A - Autobus
- [ ] Tipo B/F - Camion

---

## Notas de Desarrollo

### Convencion de Codigo
- Namespace: `Simulador.Core`, `Simulador.Zones`, `Simulador.UI`
- Singletons para managers: `Instance` property
- Eventos con `System.Action`: `OnViolationDetected`, `OnScoreChanged`
- Descripciones bilingues: `description`, `descriptionES`

### Performance
- ViolationDetector usa cooldown de 5 segundos para evitar spam
- TelemetryLogger solo escribe a disco en `ExportToJSON()`
- NotificationManager usa coroutines para auto-ocultar

### Debug
```csharp
// Ver eventos en consola
Debug.Log($"[VIOLATION] {type}: {description}");

// Verificar path de telemetria
Debug.Log($"Telemetry path: {Application.persistentDataPath}");
```

---

## Contacto

Proyecto: Simulador de Manejo Tlaxcala 2026
Desarrollador: Miguel
