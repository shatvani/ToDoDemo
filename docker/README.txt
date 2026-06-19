Három dolgot érdemes tudni róla:
* 3 stage: restore+build → publish → runtime. A cache miatt a dotnet restore csak akkor fut újra, ha a .csproj változik.
* 8080-as port: .NET 8+ óta ez az alapértelmezett konténerben (nem 80).
* USER app: nem root-ként fut — ez a Microsoft ajánlott practice, az aspnet:10.0 image tartalmazza ezt a usert.