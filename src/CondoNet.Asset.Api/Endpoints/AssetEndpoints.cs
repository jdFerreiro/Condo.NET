namespace CondoNet.Asset.Api.Endpoints
{
    using Microsoft.AspNetCore.Routing;

    public static class AssetEndpoints
    {
        public static void MapAssetEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapCondominiumEndpoints();
            app.MapTowerEndpoints();
            app.MapUnitEndpoints();
            app.MapCommonAssetEndpoints();
            app.MapCriticalEquipmentEndpoints();
        }
    }
}
