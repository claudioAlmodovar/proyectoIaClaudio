# Backend

API minimal construida con [ASP.NET Core](https://learn.microsoft.com/aspnet/core/?view=aspnetcore-8.0) en
C#.

## Ejecución

```bash
cd backend
dotnet restore
dotnet run
```

La API expone endpoints básicos para gestionar tareas en memoria y cuenta con documentación generada
por Swagger en `/swagger` cuando se ejecuta en modo desarrollo.

## Apertura en Visual Studio 2022

1. Abrir Visual Studio 2022.
2. Seleccionar **Archivo > Abrir > Proyecto o solución...**.
3. Navegar a la carpeta `backend` y elegir el archivo `BackendApi.sln`.
4. Visual Studio cargará automáticamente el proyecto web `BackendApi` listo para compilarse y ejecutarse.
