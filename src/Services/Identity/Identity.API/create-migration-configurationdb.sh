#!/bin/bash

# Usage: create-migration-configurationdb.sh <MigrationName> <Command>

# Check if the required arguments are provided
if [ $# -ne 2 ]; then
  echo "Usage: create-migration-configurationdb.sh <MigrationName> <Command>"
  exit 1
fi

# Define the migration name and DbContext name
MigrationName="$1"
Command="$2"

# Define the directory where migrations will be stored
MigrationsDirectory="Data/Configuration/Migrations"
DbContextName="ApplicationConfigurationDbContext"

# Create the migration using dotnet ef migrations add command
/usr/local/share/dotnet/x64/dotnet ef migrations add "${MigrationName}" -o "${MigrationsDirectory}" --context "${DbContextName}"


# Check if the migration command was successful
if [ $? -eq 0 ]; then
  echo "Migration '${MigrationName}' for DbContext '${DbContextName}' created successfully."
else
  echo "Error creating migration '${MigrationName}' for DbContext '${DbContextName}'."
fi

# Check if the command is "update" for applying migrations to the database
if [ "${Command}" = "update" ]; then
  
  echo "Applying migrations to the database..."
  # Apply migrations to the database using dotnet ef database update command
  /usr/local/share/dotnet/x64/dotnet ef database update --context "${DbContextName}"

  # Check if the database update command was successful
  if [ $? -eq 0 ]; then
    echo "Migrations for DbContext '${DbContextName}' applied to the database successfully."
  else
    echo "Error applying migrations for DbContext '${DbContextName}' to the database."
  fi
fi
