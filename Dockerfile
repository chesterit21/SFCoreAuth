# =================================================================
# Dockerfile Standar untuk Proyek .NET 9
# =================================================================

# Tahap 1: Build - Menggunakan image SDK .NET 9
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy file project (.csproj)
COPY *.csproj .

# Tambahkan baris ini untuk membersihkan cache di dalam kontainer build
RUN dotnet nuget locals all --clear

# Restore paket NuGet
RUN dotnet restore

# Copy semua sisa file proyek
COPY . .

# Jalankan publish. Ganti "NamaProjectAnda.csproj" dengan nama file .csproj Anda
RUN dotnet publish "SFCoreAuth.WebApp.csproj" -c Release -o /app/publish

# ---

# Tahap 2: Final - Menggunakan image runtime .NET 9
# Gunakan 'aspnet:9.0' untuk aplikasi web (MVC, API)
# Gunakan 'runtime:9.0' untuk aplikasi console
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Ganti "NamaProjectAnda.dll" dengan nama file .dll output Anda
ENTRYPOINT ["dotnet", "SFCoreAuth.WebApp.dll"]

# Untuk aplikasi web, tambahkan baris ini
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080