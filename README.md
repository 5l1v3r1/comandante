# Comandante

#### ASP MVC .Net Core debugging tool.

Entity Framework
 * Display EntityFramework context details
 * Displays entity's rows and let you filter them
 * Let you create and update records
 * Let you run custom SQL on EF context

MVC
 * Display hosting information
 * Display routing information
 * Display services registered in IoC
 * Let you invoke method, properties, fields, and displays the results

Monitoring
 * Logs requests details

# Instalation
 ```cs
 Install-Package Comandante
```

# Usage
 ```cs
public void ConfigureServices(IServiceCollection services)
{
   // Add Comandante service
    services.AddComandante();
}
        
public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider, IHostingEnvironment env)
{
    // Use Comandante middleware
    app.UseComandante();
}
```
Run the app and navigate to http://localhost/comandante

<img src="comandante_index.png" width="50%">
&nbsp;

&nbsp;


# Entity Framework

### Display EntityFramework context details

<img src="comandante_entity_framework.png" width="50%">

### Displays entity's rows and let you filter them

<img src="comandante_entity_rows.png" width="50%">

### Let you create and update records

<img src="comandante_entity_create.png" width="50%">

### Let you run custom SQL on EF context

<img src="comandante_run_sql.png" width="50%">



# MVC

### Display hosting information

<img src="comandante_hosting.png" width="50%">

### Display routing information

<img src="comandante_routing.png" width="50%">

### Display services registered in IoC

<img src="comandante_services.png" width="50%">

### Let you invoke method, properties, fields, and displays the results

<img src="comandante_service_method.png" width="50%">

# Monitoring

### Logs requests details

<img src="comandante_requests.png" width="50%">


[![Analytics](https://ga-beacon.appspot.com/UA-149252719-1/welcome-page)](https://github.com/igrigorik/ga-beacon)
