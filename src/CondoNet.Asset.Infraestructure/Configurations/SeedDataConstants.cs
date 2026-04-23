namespace CondoNet.Asset.Infraestructure.Configurations
{
    public static class SeedDataConstants
    {
        // Este ID debe ser el mismo que uses en el claim 'org_id' de tu JWT de pruebas
        public static readonly Guid TestOrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid TestCondoId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid TowerAId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        public static readonly Guid TowerBId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        public static readonly DateTime TestOrganizationCreatedAt = new DateTime(2026, 4, 22, 11, 41, 25, 511, DateTimeKind.Utc).AddTicks(4964);
    }
}
