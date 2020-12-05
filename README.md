# ClubDePadelRegistroyReservaApiRest

Este proyecto es una Web Api que proporciona la lógica de negocio para el proyecto web llamado ClubDePadelRegistroyReserva

## Ejecutar el servidor

Es necesario descargar el comprimido o clonar el proyecto, compilarlo y ejecutarlo en Visual Studio 2019 con un servidor IIS. Es necesario tener instalado en el equipo el .NET FRAMEWORK y el paquete NuGet oficial de EntityFrameWork. Una vez ejecutado el proyecto en un servidor IIS, accede a la ruta [https://localhost:puerto1] en el navegador web.

## Conectar la API REST con la aplicación WEB

Para utilizar la aplicación web completa, además de ejecutar la API REST en el servidor será necesario descargar el proyecto web ClubDePadelRegistroyReserva y modificar los archivos environment.ts de ese proyecto, cambiando el valor del campo URL por la del servidor donde se está ejecutando nuestra API REST. Luego tendrás que ejecutar esa WEB en otro puerto (con el comando ng serve). La WEB estará ahora en un nuevo servidor [http://localhost:puerto2]. Finalmente, debes ir al Web.config del proyecto API REST y actualizar el campo 'value' de la línea de código 31: [add name="Access-Control-Allow-Origin" value="http://localhost:4200"] con la ruta del servidor de la WEB (es posible que el puerto del servidor WEB sea 4200, en ese caso no será necesario actualizar esto).

## Documentación del API REST

Una vez ejecutado el proyecto en un servidor IIS, accede a la ruta [https://localhost:puerto1] en el navegador web. Podrás ver la documentación autogenerada por el Framework en la pestaña 'Documentación' donde aparecen los endpoints y los métodos que puedes realizar (GET, POST, DELETE, ...) para cada entidad (User y Reservation). Esta documentación puede estar desactualizada o tener detalles incompletos.

## Base de datos

Esta WEB API incluye una base de datos Sql Server de tipo LocalDb alojada en un archivo MDF dentro de la carpeta AppData. La base de datos solo tiene dos tablas [User] y [Reservation]. La tabla de usuarios contiene dos usuarios previamente creados de ejemplo: user1 y user2. Sus contraseñas son iguales a su nombre de usuario. Estos usuarios pueden ser utilizados para hacer login en la app.

## Probar la API REST

Para probar únicamente la API REST sin usar la WEB, es recomendable utilizar un cliente como por ejemplo Postman que facilita la construcción de las llamadas HTTP. Antes de probar cualquier Endpoint, es necesario utilizar los usuarios de ejemplo o bien registrar un usuario nuevo con el método POST del endpoint [*/api/users] enviando el BODY en formato JSON. 

Una vez registrado el usuario nuevo o con alguno previamente creado, hay que autenticarse usando el método GET [*/api/users/login/username={username}&password={password}]. Si has conseguido recibir un código HTTP OK en esta llamada, recoge el token de la cabecera 'Authorization' y utilízalo como cabecera para las siguientes llamadas que requieren de autorización (por defecto, el token caduca a los 10 minutos, pero se renueva por cada llamada HTTP).

### Desarrollado por: [https://github.com/alem25]