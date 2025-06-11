Provide a "DefaultConnection" using your connectionString in appsettings.json "ConnectionStrings"
Provide a "Jwt" :{
"Issuer":"http://localhost:5300",
"Audience":"http://localhost:5300",
"Key": -//-,
"ValidInMinutes": 10
}

Install required EF Core Packages
Migrate the Database using code first approach
Run the application

Provide the file vilidation_rules to the validation folder on the same level as src and .sln