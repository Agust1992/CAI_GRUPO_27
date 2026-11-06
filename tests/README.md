# Pruebas HTTP

Requieren Windows, PowerShell 5.1 o superior y el SDK de .NET 10 o superior
activo en la terminal. No necesitan Python, tokens ni bases de trabajo.

Desde la raiz del repositorio:

```powershell
.\tests\ProbarProductos.ps1
```

El script compila los proyectos necesarios, inicia servidores en puertos libres
y usa bases SQLite temporales. Prueba los estados de orden, fallas de la
dependencia, errores 400/404/405/409/415/500, correlacion, logs y Swagger. Detiene
sus procesos al terminar, incluso si una comprobacion falla. Imprime la carpeta
de evidencia para revisar las salidas y logs.

`Dependencias` es un servidor simulado de pruebas. NO es Orders.API, no esta en
ECommerce.sln y no reemplaza la implementacion de Christian. Sus rutas de control
solo sirven para simular estados y errores. No se publica junto con las APIs.

Estas son pruebas HTTP de integracion. No se presentan como tests unitarios.
Antes de entregar falta repetir el flujo con Orders.API real y los demas
servicios del grupo. Las pruebas no certifican la libreria de persistencia de
la catedra, que sigue pendiente de confirmar.
