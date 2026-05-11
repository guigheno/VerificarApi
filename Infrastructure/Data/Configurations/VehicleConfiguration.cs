using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VerificarApi.Domain.Entities;

namespace VerificarApi.Infrastructure.Data.Configurations
{
    public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.ToTable("Vehicles");

            builder.HasKey(v => v.Id);

            builder.Property(v => v.Marca).IsRequired().HasMaxLength(100);
            builder.Property(v => v.Modelo).IsRequired().HasMaxLength(100);
            builder.Property(v => v.Versao).HasMaxLength(100);
            builder.Property(v => v.Motorizacao).HasMaxLength(50);
            builder.Property(v => v.Aspiracao).HasMaxLength(50);
            builder.Property(v => v.Combustivel).HasMaxLength(50);
            builder.Property(v => v.Cambio).HasMaxLength(50);
            builder.Property(v => v.Categoria).HasMaxLength(50);
            builder.Property(v => v.PaisOrigem).HasMaxLength(50);
            builder.Property(v => v.Importado).IsRequired();
            builder.Property(v => v.AnoFabricacao).IsRequired();
            builder.Property(v => v.AnoModelo).IsRequired();
            builder.Property(v => v.FipeValor).HasColumnType("decimal(18,2)");
            builder.Property(v => v.FipeData).IsRequired();
            builder.Property(v => v.Descricao).HasMaxLength(1000);
            builder.Property(v => v.PontosPositivos).HasMaxLength(1000);
            builder.Property(v => v.PontosNegativos).HasMaxLength(1000);
            builder.Property(v => v.NotaConsumo).HasColumnType("decimal(3,2)");
            builder.Property(v => v.NotaConforto).HasColumnType("decimal(3,2)");
            builder.Property(v => v.NotaDesempenho).HasColumnType("decimal(3,2)");
            builder.Property(v => v.NotaConfiabilidade).HasColumnType("decimal(3,2)");
            builder.Property(v => v.NotaCustoManutencao).HasColumnType("decimal(3,2)");
            

        }
    }
}